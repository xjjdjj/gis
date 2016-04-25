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
    public delegate void ContinuousLeakDelegate(object sender, EventArgs e);

    public class clsContinuousLeak
    {
        //定义泄漏事件
        public event ContinuousLeakDelegate ContinuousLeakEvent;
        public event ContinuousLeakDelegate ContinuousLeakFaildEvent;
        #region 连续泄漏模型参数
        //风向
        double WindDirection;
        //泄漏分析的Map
        Map LeakMap;
        //Geometry服务地址
        string strgeourl;
        //燃烧区参数
        List<double[]> lstCombustionObj;
        //中毒区参数
        List<double[]> lstPoisoningObj;
        //中毒起点坐标
        List<Graphic> lstContinued;
        //经纬度转平面坐标服务
        GeometryService geometrytaskcontinue;
        //中毒和燃烧
        string strContinuedType;
        //燃烧长度
        double dbCombustionLen;
        //中毒长度
        double dbPoisoningLen;
        //返回生成的纺锤体
        public List<Graphic> lst_Return;
        List<double> lstAreas;
        //返回生成的标注
        public List<Graphic> lst_Txt;
        Dictionary<Graphic, Graphic> Dict_Txt;
        #endregion

        void ProcessAction(object sender, EventArgs e)
        {
            if (ContinuousLeakEvent == null)
                ContinuousLeakEvent += new ContinuousLeakDelegate(ContinuousLeakErrorEvent);
            ContinuousLeakEvent(sender, e);
        }

        /// <summary>
        /// 如果没有自己指定关联方法，将会调用该方法抛出错误
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ContinuousLeakErrorEvent(object sender, EventArgs e)
        {
            ContinuousLeakFaildEvent(sender, e);
        }

        #region 连续泄漏
        /// <summary>
        /// 连续泄漏模型调用方法
        /// </summary>
        /// <param name="map">承载服务的地图</param>
        /// <param name="strurl">Geo的Url</param>
        /// <param name="dbx">起始点经度</param>
        /// <param name="dby">起始点纬度</param>
        /// <param name="dbCombustionSize">燃烧区步长</param>
        /// <param name="aryCombustionValue">燃烧半径</param>
        /// <param name="dbCombustionStart">燃烧区偏移量</param>
        /// <param name="dbPoisoningSize">中毒区步长</param>
        /// <param name="aryPoisoningValue">中毒半径</param>
        /// <param name="dbPoisoningStart">中毒区偏移量</param>
        /// <param name="dbangle">风向</param>
        /// <param name="strType">为0则展示燃烧和中毒，1为燃烧，2为中毒</param>
        public void LeakContinued(Map map, string strurl, double dbx, double dby, double dbCombustionSize, double[] aryCombustionValue, double dbCombustionStart,
             double dbPoisoningSize, double[] aryPoisoningValue, double dbPoisoningStart, double dbangle, string strType)
        {
            LeakMap = map;
            strgeourl = strurl;
            lst_Return = new List<Graphic>();
            lst_Txt = new List<Graphic>();

            WindDirection = dbangle;

            dbCombustionLen = dbCombustionSize * (aryCombustionValue.Length - 1);
            dbPoisoningLen = dbPoisoningSize * (aryPoisoningValue.Length - 1);

            strContinuedType = strType;
            MapPoint mpstart = new MapPoint() { X = dbx, Y = dby, SpatialReference = LeakMap.SpatialReference };



            lstContinued = new List<Graphic>();
            Graphic gratmp = new Graphic() { Geometry = mpstart };
            lstContinued.Add(gratmp);

            #region 燃烧区参数
            lstCombustionObj = new List<double[]>();
            double[] aryCombustionSize = new double[1];
            aryCombustionSize[0] = dbCombustionSize;
            lstCombustionObj.Add(aryCombustionSize);
            lstCombustionObj.Add(aryCombustionValue);
            double[] aryCombustionRadian = new double[2];
            aryCombustionRadian[0] = dbangle;
            aryCombustionRadian[1] = dbCombustionStart;
            lstCombustionObj.Add(aryCombustionRadian);
            #endregion

            #region 中毒区参数
            lstPoisoningObj = new List<double[]>();
            double[] aryPoisoningSize = new double[1];
            aryPoisoningSize[0] = dbPoisoningSize;
            lstPoisoningObj.Add(aryPoisoningSize);
            lstPoisoningObj.Add(aryPoisoningValue);
            double[] aryPoisoningRadian = new double[2];
            aryPoisoningRadian[0] = dbangle;
            aryPoisoningRadian[1] = dbCombustionStart;
            lstPoisoningObj.Add(aryPoisoningRadian);
            #endregion

            //if (map.SpatialReference != null)
            //{
            //    NoJwd();
            //}
            //else
            //{

            if (PFApp.MapServerType == enumMapServerType.Baidu)
                LeakContinuedByToolKit();
            else if (PFApp.MapServerType == enumMapServerType.Esri && isWGS84())
                LeakContinuedByService();
            else if (PFApp.MapServerType == enumMapServerType.Esri && !isWGS84())//如果不是经纬度坐标则采用百度方式计算
                LeakContinuedByToolKit();
            else if (strgeourl != "")
                LeakContinuedByService();
            //}
        }

        //20150119：粗略判断当前的坐标参考是经纬度还是平面坐标
        bool isWGS84()
        {            
            MapPoint po = (lstContinued[0] as Graphic).Geometry as MapPoint;
            if (po.X > 180 || po.X < -180)
                return false;
            else
                return true;//经纬度坐标
        }

        void LeakContinuedByToolKit()
        {
            //获得地方坐标系下的中心点
            Graphic gra = lstContinued[0];
            //计算各个扩散点坐标
            List<Graphic> lsttmp = new List<Graphic>();

            if (strContinuedType == "0")
            {
                lsttmp = CreateEllipse(
                    (gra.Geometry as MapPoint),
                    lstCombustionObj[1],
                    lstCombustionObj[0][0],
                    lstCombustionObj[2][1],
                    270 - lstCombustionObj[2][0]);
            }
            else if (strContinuedType == "1")
            {
                lsttmp = CreateEllipse(
                    (gra.Geometry as MapPoint), 
                    lstCombustionObj[1], 
                    lstCombustionObj[0][0], 
                    lstCombustionObj[2][1], 
                    270 - lstCombustionObj[2][0]);
            }
            else if (strContinuedType == "2")
            {
                lsttmp = CreateEllipse(
                    (gra.Geometry as MapPoint), 
                    lstPoisoningObj[1], 
                    lstPoisoningObj[0][0], 
                    lstPoisoningObj[2][1], 
                    270 - lstPoisoningObj[2][0]);
            }
            CreateModel(lsttmp);
        }

        void LeakContinuedByService()
        {
            //经纬度转换成地方坐标系
            geometrytaskcontinue = new GeometryService(strgeourl);
            geometrytaskcontinue.Failed += new EventHandler<TaskFailedEventArgs>(geometrytaskcontinue_Failed);
            geometrytaskcontinue.ProjectCompleted += new EventHandler<GraphicsEventArgs>(geometrytaskcontinue_ProjectCompleted);
            if (strContinuedType == "0")
            {
                geometrytaskcontinue.ProjectAsync(lstContinued, new SpatialReference(21481), lstCombustionObj);
            }
            else if (strContinuedType == "1")
            {
                geometrytaskcontinue.ProjectAsync(lstContinued, new SpatialReference(21481), lstCombustionObj);
            }
            else if (strContinuedType == "2")
            {
                geometrytaskcontinue.ProjectAsync(lstContinued, new SpatialReference(21481), lstPoisoningObj);
            }
        }

        #region 经纬度转换
        /// <summary>
        /// 将中心点坐标转换成经纬度
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void geometrytaskcontinue_ProjectCompleted(object sender, GraphicsEventArgs e)
        {
            //获得地方坐标系下的中心点
            Graphic gra = (sender as GeometryService).ProjectLastResult[0];

            //计算各个扩散点坐标
            List<Graphic> lsttmp = CreateEllipse((gra.Geometry as MapPoint),
                (e.UserState as List<double[]>)[1],
                (e.UserState as List<double[]>)[0][0],
                (e.UserState as List<double[]>)[2][1],
                270 - (e.UserState as List<double[]>)[2][0]);
            //地方坐标转换成经纬度
            GeometryService geometrytaskContiuedToJwd2 = new GeometryService(strgeourl);
            geometrytaskContiuedToJwd2.ProjectCompleted += new EventHandler<GraphicsEventArgs>(geometrytaskContiuedToJwd2_ProjectCompleted);
            geometrytaskContiuedToJwd2.ProjectAsync(lsttmp, new SpatialReference(4326));
        }

        /// <summary>
        /// 转换成经纬度后在地图上绘制模型
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void geometrytaskContiuedToJwd2_ProjectCompleted(object sender, GraphicsEventArgs e)
        {
            List<Graphic> lsttmp = (sender as GeometryService).ProjectLastResult.ToList();
            CreateModel(lsttmp);
        }
        #endregion

        //将现有范围扩大一定比例
        private Graphic ExtendMapExtent(Graphic gc)
        {
            Graphic rc = new Graphic();
            return rc;
        }

        void CreateModel(List<Graphic> lsttmp)
        {
            ESRI.ArcGIS.Client.Geometry.PointCollection pc = new ESRI.ArcGIS.Client.Geometry.PointCollection();
            for (int i = 0; i < lsttmp.Count(); i++)
            {
                pc.Add(lsttmp[i].Geometry as MapPoint);
            }

            Graphic gra = new Graphic();
            Polygon pg = new Polygon();
            pg.Rings.Add(pc);
            pg.SpatialReference = LeakMap.SpatialReference;
            gra.Geometry = pg;

            Graphic lengra = new Graphic();
            lengra.Geometry = pc[100];
            lstAreas = new List<double>();

            if (strContinuedType == "0" || strContinuedType == "1")
            {
                lst_Return.Add(gra);
                lengra.Attributes.Add("StaLenType", "燃烧距离");
                lengra.Attributes.Add("StaAreaType", "燃烧面积");
                lengra.Attributes.Add("StaLenNum", dbCombustionLen.ToString());
                lst_Txt.Add(lengra);
                LeakMap.ZoomTo(gra.Geometry);
                if (strContinuedType == "0")
                {
                    strContinuedType = "2";
                    if (PFApp.MapServerType != enumMapServerType.Baidu && isWGS84())
                    {
                        //重新绘制中毒区模型
                        geometrytaskcontinue.ProjectAsync(lstContinued, new SpatialReference(21481), lstPoisoningObj);
                    }
                    else
                    {
                        ////获得地方坐标系下的中心点
                        Graphic gra2 = lstContinued[0];
                        //计算各个扩散点坐标
                        List<Graphic> lsttmp2 = new List<Graphic>();
                        lsttmp2 = CreateEllipse(
                        (gra2.Geometry as MapPoint),
                        lstPoisoningObj[1],
                        lstPoisoningObj[0][0],
                        lstPoisoningObj[2][1],
                        270 - lstPoisoningObj[2][0]);
                        CreateModel(lsttmp2);
                    }
                }
                else
                {
                    LeakArea();
                }
            }
            else if (strContinuedType == "2")
            {
                lengra.Attributes.Add("StaLenType", "中毒距离");
                lengra.Attributes.Add("StaAreaType", "中毒面积");
                lengra.Attributes.Add("StaLenNum", dbPoisoningLen.ToString());
                lst_Txt.Add(lengra);


                if (lst_Return.Count == 0)
                {
                    lst_Return.Add(gra);
                    LeakArea();
                }
                else
                {
                    if (PFApp.MapServerType != enumMapServerType.Baidu)
                    {
                        ShearPolygon(lst_Return[0], gra);
                    }
                    else
                    {
                        //燃烧区的区域比较小
                        if (lst_Return[0].Geometry.Extent.Height < gra.Geometry.Extent.Height)
                        {
                            LeakMap.ZoomTo(gra.Geometry);
                            //中毒区被裁剪
                            ShearPolygon(gra, lst_Return[0]);
                        }
                        else
                        {
                            LeakMap.ZoomTo(lst_Return[0].Geometry);
                            ShearPolygon(lst_Return[0], gra);
                        }
                        if (PFApp.MapServerType == enumMapServerType.Baidu)
                        {
                            lst_Return.Add(gra);
                            LeakArea();
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 应用于泄漏模型中连续泄露模型
        /// </summary>
        /// <param name="LPoint">中心点位置</param>
        /// <param name="yValue">输入的是bValue</param>
        /// <param name="xStep">输入的bInterval</param>
        /// <param name="xStart">燃烧区：X开始偏移位置的位移</param>
        /// <param name="direction">风向</param>
        /// <returns></returns>
        List<Graphic> CreateEllipse(MapPoint LPoint, double[] yValue, double xStep, double xStart, double direction)
        {
            try
            {
                int intPointCount = yValue.Length;
                if (intPointCount <= 1) return null;
                double[,] EllipseXY = new double[2 * (intPointCount + 1), 2];
                List<Graphic> lsttmp = new List<Graphic>();
                double xt = 0, yt = 0;
                int intlist = 2 * intPointCount - 1;
                double dbstep = 0;
                for (int i = 0; i < intPointCount; i++)
                {
                    dbstep = dbstep + xStep;
                    EllipseXY[i, 0] = xStart + dbstep;//起点偏移量xStart加上每一次的X轴方向的偏移量
                    EllipseXY[i, 1] = yValue[i];//y轴方向上每一次的偏移量

                    EllipseXY[intlist - i, 0] = EllipseXY[i, 0];
                    EllipseXY[intlist - i, 1] = -yValue[i];

                }
                EllipseXY[2 * (intPointCount + 1) - 1, 0] = EllipseXY[0, 0];
                EllipseXY[2 * (intPointCount + 1) - 1, 1] = EllipseXY[0, 1];
                EllipseXY[2 * (intPointCount + 1) - 2, 0] = xStart;
                EllipseXY[2 * (intPointCount + 1) - 2, 1] = 0;

                direction = Math.PI * direction / 180;
                for (int i = 0; i < 2 * (intPointCount + 1); i++)
                {
                    //不偏移角度的话，下面两句即可
                    //xt = EllipseXY[i, 0];
                    //yt = EllipseXY[i, 1];
                    //偏移角度后的算法如下
                    xt = EllipseXY[i, 0] * Math.Cos(direction) - EllipseXY[i, 1] * Math.Sin(direction);
                    yt = EllipseXY[i, 0] * Math.Sin(direction) + EllipseXY[i, 1] * Math.Cos(direction);
                    xt += LPoint.X;//平移
                    yt += LPoint.Y;
                    MapPoint mptmp = new MapPoint(xt, yt, LPoint.SpatialReference);
                    Graphic gratmp = new Graphic() { Geometry = mptmp };
                    lsttmp.Add(gratmp);
                }
                return lsttmp;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        ///  裁切图像
        /// </summary>
        /// <param name="ShearGra">裁切的图像</param>
        /// <param name="CutGra">被裁切的图像</param>
        void ShearPolygon(Graphic ShearGra, Graphic CutGra)
        {
            if (PFApp.MapServerType == enumMapServerType.Baidu)
                ShearPolygonByToolKit(ShearGra, CutGra);
            else if (PFApp.MapServerType == enumMapServerType.Esri)
                ShearPolygonByService(ShearGra, CutGra);
            else if(strgeourl!="")
                ShearPolygonByService(ShearGra, CutGra);
        }

        void ShearPolygonByToolKit(Graphic ShearGra, Graphic CutGra)
        {
            int count = (CutGra.Geometry as Polygon).Rings[0].Count;
            MapPoint cutqtr = (CutGra.Geometry as Polygon).Rings[0][10];
            MapPoint cutmid = (CutGra.Geometry as Polygon).Rings[0][ count / 2];

            int qtrloc = 0;
            int midloc = 0;
            bool insect = false;
            foreach (MapPoint p in (ShearGra.Geometry as Polygon).Rings[0])
            {
                if (p.X > cutqtr.X)
                    qtrloc = 1;
                else
                    qtrloc = 0;
                if (p.X > cutmid.X)
                    midloc = 1;
                else
                    midloc = 0;
                if (qtrloc != midloc)
                {
                    insect = true;
                    break;
                }
            }
            //两个面不相交,又短又瘦
            if (!insect)
            {
                (ShearGra.Geometry as Polygon).Rings.Add((CutGra.Geometry as Polygon).Rings[0]);
            }
            else
            {
                List<MapPoint> inmps = new List<MapPoint>();
                int shearbreak = 0;
                for (int i = 0; i < (ShearGra.Geometry as Polygon).Rings[0].Count; i++)
                {
                    MapPoint mp =(ShearGra.Geometry as Polygon).Rings[0][i];
                    //点在面内
                    if (ToolKitForNonArcGISLayer.CheckRegion(CutGra.Geometry as Polygon, mp))
                    {
                        inmps.Add(mp);
                    }
                    else
                    {
                        shearbreak = i;
                        break;
                    }
                }
                bool sta = false;
                for (int i = 1; i < count; i++)
                {
                    MapPoint mp = (CutGra.Geometry as Polygon).Rings[0][i];
                    if (ToolKitForNonArcGISLayer.CheckRegion(ShearGra.Geometry as Polygon, mp))
                    {
                        sta = true;
                        inmps.Add(mp);
                    }
                    else
                    {
                        if (sta)
                            break;
                    }
                }
                sta = false;
                for (int i = shearbreak; i < (ShearGra.Geometry as Polygon).Rings[0].Count; i++)
                {
                    MapPoint mp = (ShearGra.Geometry as Polygon).Rings[0][i];
                    //点在面内
                    if (ToolKitForNonArcGISLayer.CheckRegion(CutGra.Geometry as Polygon, mp))
                    {
                        sta = true;
                        inmps.Add(mp);
                    }
                    else
                    {
                        if(sta)
                            break;
                    }
                }
                (ShearGra.Geometry as Polygon).Rings.Add(new ESRI.ArcGIS.Client.Geometry.PointCollection(inmps));
                //MessageBox.Show("");
            }

        }

        void ShearPolygonByService(Graphic ShearGra, Graphic CutGra)
        {
            GeometryService geometrytaskcut = new GeometryService(strgeourl);
            geometrytaskcut.CutCompleted += new EventHandler<CutEventArgs>(geometrytaskcut_CutCompleted);
            List<Graphic> lsttmp = new List<Graphic>();
            lsttmp.Add(CutGra);
            Polyline pl = new Polyline();
            pl.Paths.Add((ShearGra.Geometry as Polygon).Rings[0]);
            geometrytaskcut.CutAsync(lsttmp, pl);
        }

        /// <summary>
        /// 裁切后图像处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void geometrytaskcut_CutCompleted(object sender, CutEventArgs e)
        {
            List<Graphic> lsttmp = (sender as GeometryService).CutLastResult.ToList();
            Graphic gra = lsttmp[0];
            for (int i = 1; i < lsttmp.Count; i++)
            {
                if (gra.Geometry.Extent.Height < lsttmp[i].Geometry.Extent.Height)
                    gra = lsttmp[i];
            }
            lst_Return.Add(gra);
            LeakArea();
        }

        /// <summary>
        /// 连续泄漏模拟出错
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void geometrytaskcontinue_Failed(object sender, TaskFailedEventArgs e)
        {
            ContinuousLeakFaildEvent(sender, e);
        }
        #endregion

        void LeakArea()
        {
            if (geometrytaskcontinue != null)
            {
                geometrytaskcontinue.ProjectCompleted -= geometrytask_ProjectCompleted;
                geometrytaskcontinue.ProjectCompleted -= geometrytaskcontinue_ProjectCompleted;
                geometrytaskcontinue.ProjectCompleted += new EventHandler<GraphicsEventArgs>(geometrytask_ProjectCompleted);
                geometrytaskcontinue.ProjectAsync(lst_Return, new SpatialReference(21480), lst_Return);
            }
            else
            {
                BaiduMeature bm = new BaiduMeature();
                foreach (Graphic g in lst_Return)
                {
                    double[] db = bm.GetAreaAndLength((g.Geometry as Polygon).Rings[0].ToArray());
                    lstAreas.Add(db[0]);
                }
                for (int i = 0; i < lst_Return.Count(); i++)
                {
                    Graphic g = lst_Txt[i];
                    g.Attributes.Add("StaAreaNum", Math.Round(lstAreas[i], 2));
                }
                ProcessAction(this, EventArgs.Empty);
            }
        }

        void geometrytask_ProjectCompleted(object sender, GraphicsEventArgs e)
        {
            geometrytaskcontinue.AreasAndLengthsCompleted -= geometrytask_AreasAndLengthsCompleted;
            geometrytaskcontinue.AreasAndLengthsCompleted += new EventHandler<AreasAndLengthsEventArgs>(geometrytask_AreasAndLengthsCompleted);
            geometrytaskcontinue.AreasAndLengthsAsync(e.Results);
        }

        void geometrytask_AreasAndLengthsCompleted(object sender, AreasAndLengthsEventArgs e)
        {
            for (int i = 0; i < lst_Return.Count(); i++)
            {
                Graphic gra = lst_Txt[i];
                gra.Attributes.Add("StaAreaNum", e.Results.Areas[i].ToString("0.00"));
            }
            ProcessAction(this, EventArgs.Empty);
        }
    }
}
