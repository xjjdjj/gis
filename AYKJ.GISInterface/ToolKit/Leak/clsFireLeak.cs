using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Tasks;
using System.Collections.Generic;
using AYKJ.GISDevelop.Platform.ToolKit.ToolKitExtentLayer;
using AYKJ.GISDevelop.Platform;

namespace AYKJ.GISInterface
{
    /// <summary>
    /// 委托
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void FireLeakDelegate(object sender, EventArgs e);

    public class clsFireLeak
    {
        #region 同心圆的参数
        //泄漏分析的Map
        Map LeakMap;
        //Gemoetry服务地址
        string strgeourl;
        //类别和半径的值
        Dictionary<string, double> Dict_Level;
        //同心圆的圆心
        MapPoint Mp_Circle;
        public double Mp_R;
        //绘制同心圆的次数
        int int_Circle;        
        //GeometryService
        GeometryService geometrytask;
        //同心圆
        Dictionary<string, Graphic> Dict_Graphic;
        //标识位置
        double[] indexs = new double[] { 0, 0.25, 0.5, 0.75 };
        //裁切图像的次数
        int int_cut;
        //是否为第一次绘制
        bool IsFirstDraw;
        #endregion
        //定义查询事件
        public event FireLeakDelegate FireLeakEvent;
        public event ExplosionLeakDelegate FireLeakFaildEvent;
        #region 返回参数
        //返回圆心列表
        public List<Graphic> ReturnCircleCenter;
        //返回圆列表
        public List<Graphic> ReturnCircle;
        //返回圆环列表
        public List<Graphic> ReturnCircleRing;
        //返回标注列表
        public List<Graphic> ReturnCircleTxt;
        //返回圆环面积
        public Dictionary<Graphic, string> ReturnCircleArea;
        #endregion

        void ProcessAction(object sender, EventArgs e)
        {
            if (FireLeakEvent == null)
                FireLeakEvent += new FireLeakDelegate(FireLeaErrorEvent);
            FireLeakEvent(sender, e);
        }

        /// <summary>
        /// 如果没有自己指定关联方法，将会调用该方法抛出错误
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void FireLeaErrorEvent(object sender, EventArgs e)
        {
            FireLeakFaildEvent(sender, e);
        }

        /// <summary>
        /// 同心圆扩散模型
        /// </summary>
        /// <param name="map">承载数据的地图服务</param>
        /// <param name="strurl">地图的Url</param>
        /// <param name="x">圆心X坐标</param>
        /// <param name="y">圆心Y坐标</param>
        /// <param name="lsttmp">半径</param>
        /// <param name="Dict_RColor">类别及颜色</param>
        public void LeakCircle(Map map, string strurl, double x, double y, Dictionary<string, double> Dict_RLevel)
        {
            LeakMap = map;
            strgeourl = strurl;
            Dict_Graphic = new Dictionary<string, Graphic>();
            Dict_Level = Dict_RLevel;

            ReturnCircleCenter = new List<Graphic>();
            ReturnCircle = new List<Graphic>();
            ReturnCircleRing = new List<Graphic>();
            ReturnCircleTxt = new List<Graphic>();

            Mp_Circle = new MapPoint(x, y, LeakMap.SpatialReference);
            Graphic gra = new Graphic();
            gra.Geometry = Mp_Circle;
            gra.Geometry.SpatialReference = LeakMap.SpatialReference;
            ReturnCircleCenter.Add(gra);

            int_Circle = 0;
            IsFirstDraw = true;

            if (PFApp.MapServerType == enumMapServerType.Baidu)
                drawCircleByToolKit(Mp_Circle, Dict_RLevel);
            else if (PFApp.MapServerType == enumMapServerType.Esri)
                drawCircleByService(Dict_RLevel, gra);
            else if (strurl != "")
                drawCircleByService(Dict_RLevel, gra);
        }

        /// <summary>
        /// 数学画圆
        /// </summary>
        /// <param name="center"></param>
        /// <param name="r"></param>

        void drawCircleByToolKit(MapPoint center, Dictionary<string, double> Dict_RLevel)
        {
            foreach (double d in Dict_RLevel.Values.ToList())
            {
                Mp_R = d;
                drawCircle(Mp_Circle, d);
            }

        }

        void drawCircle(MapPoint center, double r)
        {
            ToolKitForNonArcGISLayer mathtool = new ToolKitForNonArcGISLayer();
            List<MapPoint> circlerings = mathtool.GetCirclePoints(center, r);

            Polygon polygon = new Polygon();
            polygon.Rings.Add(new ESRI.ArcGIS.Client.Geometry.PointCollection(circlerings));

            Graphic gra = new Graphic();
            gra.Geometry = polygon;
            ReturnCircle.Add(gra);

            if (IsFirstDraw == true)
            {
                ReturnCircleRing.Add(new Graphic() { Geometry = polygon });
            }
            IsFirstDraw = false;
            Graphic txtgra = new Graphic();
            txtgra.Attributes.Add("StaType", Dict_Level.Keys.ToList()[int_Circle]);
            txtgra.Attributes.Add("StaNum", Dict_Level.Values.ToList()[int_Circle]);

            int len = (gra.Geometry as Polygon).Rings[0].Count - 1;
            double index = indexs[int_Circle % indexs.Length] * len;
            txtgra.Geometry = (gra.Geometry as Polygon).Rings[0][(int)index];

            ReturnCircleTxt.Add(txtgra);
            Dict_Graphic.Add(Dict_Level.Keys.ToList()[int_Circle], new Graphic() { Geometry = polygon });

            int_Circle = int_Circle + 1;
            if (int_Circle > Dict_Level.Count - 1)
            {
                CreateAnnular();
                return;
            }
        }

        void drawCircleByService(Dictionary<string, double> Dict_RLevel, Graphic gra)
        {
            geometrytask = new GeometryService(strgeourl);
            geometrytask.BufferCompleted -= geometrytask_BufferCompleted;
            geometrytask.Failed -= geometrytask_Failed;
            geometrytask.BufferCompleted += new EventHandler<GraphicsEventArgs>(geometrytask_BufferCompleted);
            geometrytask.Failed += new EventHandler<TaskFailedEventArgs>(geometrytask_Failed);

            if (Dict_RLevel.Values.ToList()[0] != 0)
            {
                BufferParameters bufferparameters = new BufferParameters();
                bufferparameters.Unit = LinearUnit.Meter;
                bufferparameters.BufferSpatialReference = LeakMap.SpatialReference;
                bufferparameters.OutSpatialReference = LeakMap.SpatialReference;
                bufferparameters.Distances.Add(Dict_RLevel.Values.ToList()[0]);
                bufferparameters.Features.Add(gra);
                geometrytask.BufferAsync(bufferparameters);
            }
            else
            {
                ReturnCircle.Add(null);
                ReturnCircleRing.Add(null);
                ReturnCircleTxt.Add(null);
                if (Dict_RLevel.Values.ToList()[1] != 0)
                {
                    int_Circle = 1;
                    BufferParameters bufferparameters = new BufferParameters();
                    bufferparameters.Unit = LinearUnit.Meter;
                    bufferparameters.BufferSpatialReference = LeakMap.SpatialReference;
                    bufferparameters.OutSpatialReference = LeakMap.SpatialReference;
                    bufferparameters.Distances.Add(Dict_RLevel.Values.ToList()[1]);
                    bufferparameters.Features.Add(gra);
                    geometrytask.BufferAsync(bufferparameters);
                }
                else
                {
                    ReturnCircle.Add(null);
                    ReturnCircleRing.Add(null);
                    ReturnCircleTxt.Add(null);
                    if (Dict_RLevel.Values.ToList()[2] != 0)
                    {
                        int_Circle = 2;
                        BufferParameters bufferparameters = new BufferParameters();
                        bufferparameters.Unit = LinearUnit.Meter;
                        bufferparameters.BufferSpatialReference = LeakMap.SpatialReference;
                        bufferparameters.OutSpatialReference = LeakMap.SpatialReference;
                        bufferparameters.Distances.Add(Dict_RLevel.Values.ToList()[2]);
                        bufferparameters.Features.Add(gra);
                        geometrytask.BufferAsync(bufferparameters);
                    }
                    else
                    {
                        ReturnCircle.Add(null);
                        ReturnCircleRing.Add(null);
                        ReturnCircleTxt.Add(null);

                        int_Circle = 3;
                        BufferParameters bufferparameters = new BufferParameters();
                        bufferparameters.Unit = LinearUnit.Meter;
                        bufferparameters.BufferSpatialReference = LeakMap.SpatialReference;
                        bufferparameters.OutSpatialReference = LeakMap.SpatialReference;
                        bufferparameters.Distances.Add(Dict_RLevel.Values.ToList()[3]);
                        bufferparameters.Features.Add(gra);
                        geometrytask.BufferAsync(bufferparameters);
                    }
                }
            }
        }

        /// <summary>
        /// 绘制成功
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void geometrytask_BufferCompleted(object sender, GraphicsEventArgs e)
        {
            Graphic gra = new Graphic();
            gra.Geometry = e.Results[0].Geometry;
            ReturnCircle.Add(gra);

            if (IsFirstDraw == true)
            {
                ReturnCircleRing.Add(new Graphic() { Geometry = e.Results[0].Geometry });
            }
            IsFirstDraw = false;
            Graphic txtgra = new Graphic();
            txtgra.Attributes.Add("StaType", Dict_Level.Keys.ToList()[int_Circle]);
            txtgra.Attributes.Add("StaNum", Dict_Level.Values.ToList()[int_Circle]);

            int len = (gra.Geometry as Polygon).Rings[0].Count - 1;
            double index = indexs[int_Circle % indexs.Length] * len;
            txtgra.Geometry = (gra.Geometry as Polygon).Rings[0][(int)index];

            ReturnCircleTxt.Add(txtgra);
            Dict_Graphic.Add(Dict_Level.Keys.ToList()[int_Circle], new Graphic() { Geometry = e.Results[0].Geometry });

            int_Circle = int_Circle + 1;
            if (int_Circle > Dict_Level.Count - 1)
            {
                CreateAnnular();
                return;
            }
            BufferParameters bufferparameters = new BufferParameters();
            bufferparameters.Unit = LinearUnit.Meter;
            bufferparameters.BufferSpatialReference = LeakMap.SpatialReference;
            bufferparameters.OutSpatialReference = LeakMap.SpatialReference;
            Graphic tmpgra = new Graphic();
            tmpgra.Geometry = Mp_Circle;
            tmpgra.Geometry.SpatialReference = LeakMap.SpatialReference;
            bufferparameters.Distances.Add(Dict_Level.Values.ToList()[int_Circle]);
            bufferparameters.Features.Add(tmpgra);
            geometrytask.BufferAsync(bufferparameters);
        }

        /// <summary>
        /// 模型出错
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void geometrytask_Failed(object sender, TaskFailedEventArgs e)
        {
            FireLeakFaildEvent(sender, e);
        }

        /// <summary>
        /// 生成同心环
        /// </summary>
        void CreateAnnular()
        {
            if (Dict_Graphic.Count < 2)
            {
                //ReturnCircleRing.Add(Dict_Graphic.Values.ToList()[0]);
                RingArea();
                return;
            }
            int_cut = 1;

            if (geometrytask != null)
            {
                geometrytask.CutCompleted -= geometrytaskcut_CutCompleted;
                geometrytask.CutCompleted += new EventHandler<CutEventArgs>(geometrytaskcut_CutCompleted);
                List<Graphic> lsttmp = new List<Graphic>();
                lsttmp.Add(Dict_Graphic.Values.ToList()[int_cut]);
                Polyline pl = new Polyline();
                pl.Paths.Add((Dict_Graphic.Values.ToList()[int_cut - 1].Geometry as Polygon).Rings[0]);
                geometrytask.CutAsync(lsttmp, pl);
            }
            else
            {
                int_cut = 1;
                //客户端实现
                while (int_cut < Dict_Graphic.Count)
                {
                    Graphic gra = Dict_Graphic.Values.ToList()[int_cut];
                    Polyline p1 = new Polyline();
                    p1.Paths.Add((Dict_Graphic.Values.ToList()[int_cut - 1].Geometry as Polygon).Rings[0]);
                    ReturnCircleRing.Add(ToolKitForNonArcGISLayer.cutGeometry(gra, p1));
                    int_cut += 1;
                }

                RingArea();
                LeakMap.ZoomTo(ReturnCircleRing[ReturnCircleRing.Count - 1].Geometry);
            }
        }

        /// <summary>
        /// 裁切后图像处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void geometrytaskcut_CutCompleted(object sender, CutEventArgs e)
        {
            List<Graphic> lst = (sender as GeometryService).CutLastResult.ToList();
            Graphic gra = new Graphic();
            if (lst[0].Geometry.Extent.Height > lst[1].Geometry.Extent.Height)
            {
                gra = lst[0];
            }
            else
            {
                gra = lst[1];
            }
           
            ReturnCircleRing.Add(gra);
            int_cut = int_cut + 1;
            if (int_cut < Dict_Graphic.Count)
            {
                List<Graphic> lsttmp = new List<Graphic>();
                lsttmp.Add(Dict_Graphic.Values.ToList()[int_cut]);
                Polyline pl = new Polyline();
                pl.Paths.Add((Dict_Graphic.Values.ToList()[int_cut - 1].Geometry as Polygon).Rings[0]);
                geometrytask.CutAsync(lsttmp, pl);
            }
            else
            {
                //ProcessAction(this, EventArgs.Empty);
                //geometrytask.LabelPointsAsync(ReturnCircleRing);
                RingArea();
                LeakMap.ZoomTo(gra.Geometry);
            }
        }

        void geometrytask_LabelPointsCompleted(object sender, GraphicsEventArgs e)
        {
            ReturnCircleTxt = e.Results.ToList();
            RingArea();
            //ProcessAction(this, EventArgs.Empty);
        }

        /// <summary>
        /// 计算面积
        /// </summary>
        void RingArea()
        {
            if (geometrytask != null)
            {
                List<Graphic> lst = new List<Graphic>();
                for (int i = 0; i < ReturnCircleRing.Count; i++)
                {
                    if (ReturnCircleRing[i] != null)
                    {
                        lst.Add(ReturnCircleRing[i]);
                    }
                }
                geometrytask.ProjectCompleted -= geometrytask_ProjectCompleted;
                geometrytask.ProjectCompleted += new EventHandler<GraphicsEventArgs>(geometrytask_ProjectCompleted);
                geometrytask.ProjectAsync(lst, new SpatialReference(21480), lst);
            }
            else
            {
                ReturnCircleArea = ToolKitForNonArcGISLayer.RingArea(ReturnCircleTxt, ReturnCircleRing, Dict_Level);
                ProcessAction(this, EventArgs.Empty);
            }
        }

        void geometrytask_ProjectCompleted(object sender, GraphicsEventArgs e)
        {  
            ReturnCircleArea = new Dictionary<Graphic, string>();
            geometrytask.AreasAndLengthsCompleted -= geometrytask_AreasAndLengthsCompleted;
            geometrytask.AreasAndLengthsCompleted += new EventHandler<AreasAndLengthsEventArgs>(geometrytask_AreasAndLengthsCompleted);
            geometrytask.AreasAndLengthsAsync(e.Results, e.UserState);
        }

        void geometrytask_AreasAndLengthsCompleted(object sender, AreasAndLengthsEventArgs e)
        {
            List<Graphic> lst = e.UserState as List<Graphic>;
            List<Graphic> lsttxt = new List<Graphic>();
            for (int i = 0; i < ReturnCircleTxt.Count; i++)
            {
                if (ReturnCircleTxt[i] != null)
                {
                    lsttxt.Add(ReturnCircleTxt[i]);
                }
            }
            for (int i = 0; i < lst.Count; i++)
            {
                lsttxt[i].Attributes.Add("StaArea", e.Results.Areas[i].ToString("0.00") + "平方米");
                ReturnCircleArea.Add(lst[i], e.Results.Areas[i].ToString("0.00") + "平方米");
            }
            ProcessAction(this, EventArgs.Empty);
        }
    }
}