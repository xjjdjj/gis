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
using AYKJ.GISDevelop.Platform;
using AYKJ.GISDevelop.Platform.ToolKit.ToolKitExtentLayer;

namespace AYKJ.GISInterface
{
    /// <summary>
    /// 委托
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void InstantaneousLeakDelegate(object sender, EventArgs e);

    public class clsInstantaneousLeak
    {
        //定义泄漏事件
        public event InstantaneousLeakDelegate InstantaneousLeakEvent;
        public event InstantaneousLeakDelegate InstantaneousLeakFaildEvent;
        //泄漏分析的Map
        Map LeakMap;
        //Geometry服务地址
        string strgeourl;
        //坐标点和半径的值
        Dictionary<Graphic, double> Dict_Shaft;
        //绘制移动圆的次数
        int int_Shaft;
        //空间信息服务
        GeometryService geometrytask;
        //返回生成的数值
        public List<Graphic> lst_Return;
        //返回合并后的区域
        public Graphic graUnio;

        void ProcessAction(object sender, EventArgs e)
        {
            if (InstantaneousLeakEvent == null)
                InstantaneousLeakEvent += new InstantaneousLeakDelegate(InstantaneousLeakErrorEvent);
            InstantaneousLeakEvent(sender, e);
        }

        /// <summary>
        /// 如果没有自己指定关联方法，将会调用该方法抛出错误
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void InstantaneousLeakErrorEvent(object sender, EventArgs e)
        {
            InstantaneousLeakFaildEvent(sender, e);
        }

        #region 瞬时泄漏
        /// <summary>
        /// 瞬时泄漏模型展示
        /// </summary>
        /// <param name="map">承载的地图</param>
        /// <param name="strurl">分析的Url</param>
        /// <param name="dbx">起始扩散点经度</param>
        /// <param name="dby">起始扩散点纬度</param>
        /// <param name="arysize">每个扩散点步长</param>
        /// <param name="aryradius">每个扩散点的半径</param>
        /// <param name="dbangle">扩散方向</param>
        public void LeakShaft(Map map, string strurl,double dbx, double dby, double[] arysize, double[] aryradius, double dbangle)
        {
            LeakMap = map;
            strgeourl = strurl;
            lst_Return = new List<Graphic>();

            if (PFApp.MapServerType == enumMapServerType.Baidu)
                CalculationShaftByToolKit(dbx, dby, arysize, aryradius, dbangle);
            else if (PFApp.MapServerType == enumMapServerType.Esri)
                CalculationShaftByService(strgeourl, dbx, dby, arysize, aryradius, dbangle);
            else if(strurl!="")
                CalculationShaftByService(strgeourl, dbx, dby, arysize, aryradius, dbangle);
        }

        public void CalculationShaftByToolKit(double dbx, double dby, double[] arysize, double[] aryradius, double radian)
        {
            List<Graphic> lstmp = ToolKitForNonArcGISLayer.CalculationShaft(dbx, dby, arysize, aryradius, radian,LeakMap);
            spreadGeometry(lstmp,aryradius);
        }
        /// <summary>
        /// 扩散
        /// </summary>
        /// <param name="lstmp"></param>
        /// <param name="aryradius"></param>
        void spreadGeometry(List<Graphic> lstmp, double[] aryradius)
        {
            Dict_Shaft = new Dictionary<Graphic, double>();
            for (int i = 0; i < aryradius.Count(); i++)
            {
                Dict_Shaft.Add(lstmp[i], aryradius[i]);
            }
            int_Shaft = 0;

            ToolKitForNonArcGISLayer mathtool = new ToolKitForNonArcGISLayer();

            foreach (KeyValuePair<Graphic, double> kv in Dict_Shaft)
            {
                Polygon polygon = new Polygon();
                List<MapPoint> mps = mathtool.GetCirclePoints(kv.Key.Geometry as MapPoint, kv.Value);
                polygon.Rings.Add(new ESRI.ArcGIS.Client.Geometry.PointCollection(mps));
                graUnio = new Graphic();
                graUnio.Geometry = polygon;
                lst_Return.Add(graUnio);
            }

            //20130925:将所有圆圈缩放至当前视野
            double xmax = lst_Return[0].Geometry.Extent.XMax;
            double ymax = lst_Return[0].Geometry.Extent.YMax;
            double xmin = lst_Return[0].Geometry.Extent.XMin;
            double ymin = lst_Return[0].Geometry.Extent.YMax;
            foreach (Graphic gc in lst_Return)
            {
                if (gc.Geometry.Extent.XMax > xmax) xmax = gc.Geometry.Extent.XMax;
                if (gc.Geometry.Extent.YMax > ymax) ymax = gc.Geometry.Extent.YMax;
                if (gc.Geometry.Extent.XMin < xmin) xmin = gc.Geometry.Extent.XMin;
                if (gc.Geometry.Extent.YMin < ymin) ymin = gc.Geometry.Extent.YMin;
            }
            //Graphic n_gc = new Graphic();
            ESRI.ArcGIS.Client.Geometry.Envelope n_env = new Envelope(xmax, ymax, xmin, ymin);
            //n_gc.Geometry = n_env;


            LeakMap.ZoomTo(n_env);

            ProcessAction(this, EventArgs.Empty);
        }

        public void CalculationShaftByService(string strgeourl, double dbx, double dby, double[] arysize, double[] aryradius, double dbangle)
        {
            geometrytask = new GeometryService(strgeourl);
            geometrytask.BufferCompleted -= geometrytaskshaft_BufferCompleted;
            geometrytask.Failed -= geometrytask_Failed;
            geometrytask.ProjectCompleted -= geometrytaskshaft_ProjectCompleted;
            geometrytask.UnionCompleted -= geometrytask_UnionCompleted;
            geometrytask.BufferCompleted += new EventHandler<GraphicsEventArgs>(geometrytaskshaft_BufferCompleted);
            geometrytask.Failed += new EventHandler<TaskFailedEventArgs>(geometrytask_Failed);
            geometrytask.ProjectCompleted += new EventHandler<GraphicsEventArgs>(geometrytaskshaft_ProjectCompleted);
            geometrytask.UnionCompleted += new EventHandler<GeometryEventArgs>(geometrytask_UnionCompleted);

            CalculationShaft(dbx, dby, arysize, aryradius, dbangle);
        }
        /// <summary>
        /// 根据角度来计算扩散点经纬度
        /// </summary>
        /// <param name="dbx">起始扩散点经度</param>
        /// <param name="dby">起始扩散点纬度</param>
        /// <param name="arysize">每个扩散点步长</param>
        /// <param name="aryradius">每个扩散点的半径</param>
        /// <param name="radian">角度</param>
        public void CalculationShaft(double dbx, double dby, double[] arysize, double[] aryradius, double radian)
        {
            MapPoint mpstart = new MapPoint() { X = dbx, Y = dby, SpatialReference = LeakMap.SpatialReference };

            //经纬度转换成地方坐标系
            List<Graphic> lsttmp = new List<Graphic>();
            Graphic gratmp = new Graphic() { Geometry = mpstart };
            lsttmp.Add(gratmp);
            List<double[]> lstobj = new List<double[]>();
            lstobj.Add(arysize);
            lstobj.Add(aryradius);
            double[] aryradian = new double[1];
            aryradian[0] = radian;
            lstobj.Add(aryradian);

            geometrytask.ProjectAsync(lsttmp, new SpatialReference(21481), lstobj);
        }

        /// <summary>
        /// 根据转换后的地方坐标和步长、角度来计算地方坐标系下面的扩散点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void geometrytaskshaft_ProjectCompleted(object sender, GraphicsEventArgs e)
        {
            Graphic gra = (sender as GeometryService).ProjectLastResult[0];
            CalculationMapPoint((gra.Geometry as MapPoint), (e.UserState as List<double[]>)[0], (e.UserState as List<double[]>)[1], (e.UserState as List<double[]>)[2]);
        }

        /// <summary>
        /// 根据转换后的地方坐标和步长、角度来计算地方坐标系下面的扩散点
        /// </summary>
        /// <param name="mp">地方坐标系下起始扩散点坐标</param>
        /// <param name="arysize">步长</param>
        /// <param name="aryradius">扩散半径</param>
        /// <param name="aryradian">角度</param>
        void CalculationMapPoint(MapPoint mp, double[] arysize, double[] aryradius, double[] aryradian)
        {
            List<Graphic> lstmp = new List<Graphic>();
            lstmp.Add(new Graphic() { Geometry = mp });

            for (int i = 0; i < arysize.Count(); i++)
            {
                MapPoint slopemp = new MapPoint();
                double dbx = Math.Sin(aryradian[0] * Math.PI / 180) * arysize[i];
                double dby = Math.Cos(aryradian[0] * Math.PI / 180) * arysize[i];
                double x = (lstmp[i].Geometry as MapPoint).X;
                double y = (lstmp[i].Geometry as MapPoint).Y;

                x = (lstmp[i].Geometry as MapPoint).X - dbx;
                y = (lstmp[i].Geometry as MapPoint).Y - dby;

                slopemp.X = x;
                slopemp.Y = y;
                slopemp.SpatialReference = lstmp[i].Geometry.SpatialReference;
                lstmp.Add(new Graphic() { Geometry = slopemp });
            }

            //地方坐标转换成经纬度
            GeometryService geometrytaskToJwd = new GeometryService(strgeourl);
            geometrytaskToJwd.ProjectCompleted += new EventHandler<GraphicsEventArgs>(geometrytaskToJwd_ProjectCompleted);
            geometrytaskToJwd.ProjectAsync(lstmp, new SpatialReference(4326), aryradius);
        }

        /// <summary>
        /// 返回计算得出的经纬度下的扩散点来生成扩散区域
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void geometrytaskToJwd_ProjectCompleted(object sender, GraphicsEventArgs e)
        {
            Dict_Shaft = new Dictionary<Graphic, double>();
            for (int i = 0; i < ((e.UserState) as double[]).Count(); i++)
            {
                Dict_Shaft.Add((sender as GeometryService).ProjectLastResult[i], ((e.UserState) as double[])[i]);
            }
            int_Shaft = 0;

            BufferParameters bufferparameters = new BufferParameters();
            bufferparameters.Unit = LinearUnit.Meter;
            bufferparameters.BufferSpatialReference = LeakMap.SpatialReference;
            bufferparameters.OutSpatialReference = LeakMap.SpatialReference;
            bufferparameters.Distances.Add(Dict_Shaft.Values.ToArray()[0]);
            bufferparameters.Features.Add(Dict_Shaft.Keys.ToArray()[0]);
            geometrytask.BufferAsync(bufferparameters);
        }

        /// <summary>
        /// 扩散区域生成成功
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void geometrytaskshaft_BufferCompleted(object sender, GraphicsEventArgs e)
        {
            Graphic gra = new Graphic();
            gra.Geometry = e.Results[0].Geometry;
            lst_Return.Add(gra);
            int_Shaft = int_Shaft + 1;
            if (int_Shaft > Dict_Shaft.Count - 1)
            {
                geometrytask.UnionAsync(lst_Return);                
                return;
            }
            BufferParameters bufferparameters = new BufferParameters();
            bufferparameters.Unit = LinearUnit.Meter;
            bufferparameters.BufferSpatialReference = LeakMap.SpatialReference;
            bufferparameters.OutSpatialReference = LeakMap.SpatialReference;
            bufferparameters.Distances.Add(Dict_Shaft.Values.ToArray()[int_Shaft]);
            bufferparameters.Features.Add(Dict_Shaft.Keys.ToArray()[int_Shaft]);
            geometrytask.BufferAsync(bufferparameters);
        }

        /// <summary>
        /// 组合
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void geometrytask_UnionCompleted(object sender, GeometryEventArgs e)
        {
            graUnio = new Graphic();
            graUnio.Geometry = e.Result;
            LeakMap.ZoomTo(graUnio.Geometry);
            ProcessAction(this, EventArgs.Empty);
        }

        /// <summary>
        /// 模拟失败
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void geometrytask_Failed(object sender, TaskFailedEventArgs e)
        {
            InstantaneousLeakFaildEvent(sender, e);
        }
        #endregion
    }
}
