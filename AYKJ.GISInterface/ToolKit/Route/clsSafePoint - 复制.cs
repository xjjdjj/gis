using System;
using System.Collections.Generic;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Tasks;
using System.Linq;

namespace AYKJ.GISExtension
{
    /// <summary>
    /// 委托
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void SafePointDelegate(object sender, EventArgs e);

    public class clsSafePoint
    {
        //定义事件
        public event SafePointDelegate SafePointEvent;
        public event SafePointDelegate SafePointFaildEvent;
        //GeometryService
        GeometryService geometryservice;
        //受体点
        MapPoint Mp_Recipient;
        //事故边缘线
        Polyline Polyline_Leak;
        //事故禁止逃离线
        Graphic NoRunGraphic;
        //Geometry的Url
        string strgeourl;
        //NetWork的Url
        string strneturl;
        //地图
        Map map;
        //最近点
        MapPoint Return_MapPoint;
        public Graphic Return_GraCz;
        public List<Graphic> Return_lst;
        double dbshortlen;
        //事故点到事故边缘线路径
        double dbleaklen;
        //事故点和受体的连线集合
        List<Polyline> lstleakline;

        void ProcessAction(object sender, EventArgs e)
        {
            if (SafePointEvent == null)
                SafePointEvent += new SafePointDelegate(SafePointErrorEvent);
            SafePointEvent(sender, e);
        }

        /// <summary>
        /// 如果没有自己指定关联方法，将会调用该方法抛出错误
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SafePointErrorEvent(object sender, EventArgs e)
        {
            SafePointFaildEvent(sender, e);
        }
         
        /// <summary>
        /// 计算最近边缘点
        /// </summary>
        /// <param name="tmpmap">承载的地图服务</param>
        /// <param name="mprecipient">受体点</param>
        /// <param name="mpleak">事故点</param>
        /// <param name="strgurl">空间服务路径</param>
        /// <param name="strnurl">网络服务路径</param>
        /// <param name="dblen">事故点影响距离</param>
        public void SafeRoute(Map tmpmap, MapPoint mprecipient, MapPoint mpleak, string strgurl, string strnurl,double dblen)
        {
            map = tmpmap;
            strgeourl = strgurl;
            strneturl = strnurl;
            Mp_Recipient = mprecipient;
            dbleaklen = dblen;

            Polyline polyline = new Polyline();
            PointCollection pc = new PointCollection();
            pc.Add(mprecipient);
            pc.Add(mpleak);
            polyline.Paths.Add(pc);
            polyline.SpatialReference = mprecipient.SpatialReference;

            lstleakline = new List<Polyline>();
            lstleakline.Add(polyline);


            geometryservice = new GeometryService(strgeourl);
            geometryservice.Failed += new EventHandler<TaskFailedEventArgs>(geometryservice_Failed);
            geometryservice.TrimExtendCompleted += new EventHandler<GraphicsEventArgs>(geometryservice_TrimExtendCompleted);
            geometryservice.LengthsCompleted += new EventHandler<LengthsEventArgs>(geometryservice_LengthsCompleted);
            geometryservice.CutCompleted += new EventHandler<CutEventArgs>(geometryservice_CutCompleted);
            geometryservice.ProjectCompleted += new EventHandler<GraphicsEventArgs>(geometryservice_ProjectCompleted);
            geometryservice.BufferCompleted += new EventHandler<GraphicsEventArgs>(geometryservice_BufferCompleted);
            List<Graphic> lstpolyline = new List<Graphic>();
            lstpolyline.Add(new Graphic() { Geometry = mpleak });
            geometryservice.ProjectAsync(lstpolyline, new SpatialReference(21480), "BuffFirst");
        }

        /// <summary>
        /// 延长线成功
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void geometryservice_TrimExtendCompleted(object sender, GraphicsEventArgs e)
        {
            if (e.UserState.ToString() == "First")
            {
                Graphic gra = e.Results[0];
                MapPoint mp1 = (gra.Geometry as Polyline).Paths[0][0];
                mp1.SpatialReference = gra.Geometry.SpatialReference;
                MapPoint mp2 = (gra.Geometry as Polyline).Paths[0][(gra.Geometry as Polyline).Paths[0].Count - 1];
                mp2.SpatialReference = gra.Geometry.SpatialReference;
                PointCollection pc1 = new PointCollection();
                pc1.Add(Mp_Recipient);
                pc1.Add(mp1);
                PointCollection pc2 = new PointCollection();
                pc2.Add(Mp_Recipient);
                pc2.Add(mp2);
                Polyline pline1 = new Polyline();
                pline1.Paths.Add(pc1);
                pline1.SpatialReference = gra.Geometry.SpatialReference;
                Polyline pline2 = new Polyline();
                pline2.Paths.Add(pc2);
                pline2.SpatialReference = gra.Geometry.SpatialReference;
                List<Graphic> lst = new List<Graphic>();
                lst.Add(new Graphic() { Geometry = pline1 });
                lst.Add(new Graphic() { Geometry = pline2 });

                //GraphicsLayer gl = new GraphicsLayer();
                //ESRI.ArcGIS.Client.Symbols.SimpleLineSymbol sls = new ESRI.ArcGIS.Client.Symbols.SimpleLineSymbol();
                //sls.Color = new System.Windows.Media.SolidColorBrush(new System.Windows.Media.Color() { A = 255, R = 255, B = 0, G = 0 });
                //lst[0].Symbol = sls;
                //lst[1].Symbol = sls;
                //gl.Graphics.Add(lst[0]);
                //gl.Graphics.Add(lst[1]);
                //map.Layers.Add(gl);

                geometryservice.LengthsAsync(lst, LinearUnit.Meter, true, gra);
            }
            else if (e.UserState.ToString() == "Second")
            {
                NoRunGraphic = e.Results[0];
                Graphic gra = new Graphic() { Geometry = Polyline_Leak };
                List<Graphic> lst = new List<Graphic>();
                lst.Add(gra);

                GraphicsLayer gl = new GraphicsLayer();
                ESRI.ArcGIS.Client.Symbols.SimpleLineSymbol sls = new ESRI.ArcGIS.Client.Symbols.SimpleLineSymbol();
                sls.Color = new System.Windows.Media.SolidColorBrush(new System.Windows.Media.Color() { A = 255, R = 255, B = 0, G = 0 });
                NoRunGraphic.Symbol = sls;
                //gl.Graphics.Add(NoRunGraphic);
                map.Layers.Add(gl);

                geometryservice.CutAsync(lst, NoRunGraphic.Geometry as Polyline);
            }
        }

        /// <summary>
        /// 获取安全逃生弧线
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void geometryservice_CutCompleted(object sender, CutEventArgs e)
        {
            List<Graphic> lsttmp = (sender as GeometryService).CutLastResult.ToList();
            Graphic gra = lsttmp[0];
            for (int i = 1; i < lsttmp.Count; i++)
            {
                if (gra.Geometry.Extent.Height > lsttmp[i].Geometry.Extent.Height)
                    gra = lsttmp[i];
            }
            Return_GraCz = gra;

            clsEscapeRoute clsescaperoute = new clsEscapeRoute();
            clsescaperoute.EscapeRouteFaildEvent += new EscapeRouteDelegate(clsescaperoute_EscapeRouteFaildEvent);
            clsescaperoute.EscapeRouteEvent += new EscapeRouteDelegate(clsescaperoute_EscapeRouteEvent);
            List<Graphic> lstStart = new List<Graphic>();
            Graphic regra = new Graphic() { Geometry = Mp_Recipient };
            lstStart.Add(regra);
            List<Graphic> lstEnd = new List<Graphic>();
            for (int i = 0; i < (gra.Geometry as Polyline).Paths.Count; i++)
            {
                PointCollection pc = (gra.Geometry as Polyline).Paths[i];
                for (int m = 0; m < pc.Count; m++)
                {
                    MapPoint mp = pc[m];
                    mp.SpatialReference = Mp_Recipient.SpatialReference;
                    Graphic tmp = new Graphic() {Geometry = mp };
                    lstEnd.Add(tmp);
                }
            }
            List<Graphic> lstbar = new List<Graphic>();
            lstbar.Add(NoRunGraphic);
            clsescaperoute.RouteInit(map, lstStart, lstEnd, lstbar, strneturl, strgeourl, true);
        }

        /// <summary>
        /// 逃生路径计算成功
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clsescaperoute_EscapeRouteEvent(object sender, EventArgs e)
        {
            Return_lst = (sender as clsEscapeRoute).lst_Result;
            (Return_lst[0].Geometry as Polyline).Paths[0].Add((sender as clsEscapeRoute).lst_Point[0].Geometry as MapPoint);


            //Graphic gra = new Graphic() { Geometry = (Return_lst[0].Geometry as Polyline).Paths[0][(Return_lst[0].Geometry as Polyline).Paths[0].Count -1] };
            //ESRI.ArcGIS.Client.Symbols.SimpleMarkerSymbol sms = new ESRI.ArcGIS.Client.Symbols.SimpleMarkerSymbol();
            //sms.Color = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
            //sms.Size = 5;
            //gra.Symbol = sms;
            //GraphicsLayer gl = new GraphicsLayer();
            //map.Layers.Add(gl);
            //gl.Graphics.Add(gra);

            //ESRI.ArcGIS.Client.Symbols.SimpleLineSymbol sls = new ESRI.ArcGIS.Client.Symbols.SimpleLineSymbol();
            //sls.Color = new System.Windows.Media.SolidColorBrush(new System.Windows.Media.Color() { A = 255, R = 255, B = 0, G = 0 });
            //Return_lst[0].Symbol = sls;
            //gl.Graphics.Add(Return_lst[0]);


            ProcessAction(this, EventArgs.Empty);
        }

        /// <summary>
        /// 逃生路线计算失败
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clsescaperoute_EscapeRouteFaildEvent(object sender, EventArgs e)
        {
            SafePointFaildEvent(sender, e);
        }

        /// <summary>
        /// 计算最短点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void geometryservice_LengthsCompleted(object sender, LengthsEventArgs e)
        {
            if (e.Results[0] > e.Results[1])
            {
                dbshortlen = e.Results[1];
                Return_MapPoint = ((e.UserState as Graphic).Geometry as Polyline).Paths[0][((e.UserState as Graphic).Geometry as Polyline).Paths[0].Count - 1];
            }
            else
            {
                dbshortlen = e.Results[0];
                Return_MapPoint = ((e.UserState as Graphic).Geometry as Polyline).Paths[0][0];
            }

            Graphic gra1 = new Graphic();
            gra1.Geometry = Return_MapPoint;
            Graphic gra2 = new Graphic();
            gra2.Geometry = Mp_Recipient;
            List<Graphic> lst = new List<Graphic>();
            lst.Add(gra1);
            lst.Add(gra2);
            geometryservice.ProjectAsync(lst, new ESRI.ArcGIS.Client.Geometry.SpatialReference(21480), "First");
        }

        /// <summary>
        /// 计算发生错误
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void geometryservice_Failed(object sender, TaskFailedEventArgs e)
        {
            SafePointFaildEvent(sender, e);
        }

        /// <summary>
        /// 坐标转换成功
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void geometryservice_ProjectCompleted(object sender, GraphicsEventArgs e)
        {
            if (e.UserState.ToString() == "First")
            {
                MapPoint mp = ReturnCzPoint(e.Results[0].Geometry as MapPoint, e.Results[1].Geometry as MapPoint,dbshortlen);
                mp.SpatialReference = e.Results[0].Geometry.SpatialReference;
                List<Graphic> lst = new List<Graphic>();
                lst.Add(new Graphic() { Geometry = mp });
                geometryservice.ProjectAsync(lst, Mp_Recipient.SpatialReference, "Second");
            }
            else if (e.UserState.ToString() == "Second")
            {
                Polyline polyline = new Polyline();
                PointCollection pc = new PointCollection();

                pc.Add(Mp_Recipient);
                pc.Add(e.Results[0].Geometry as MapPoint);
                polyline.Paths.Add(pc);
                polyline.SpatialReference = Mp_Recipient.SpatialReference;
                List<Polyline> lst = new List<Polyline>();
                lst.Add(polyline);
                Polyline_Leak.SpatialReference = Mp_Recipient.SpatialReference;

                geometryservice.TrimExtendAsync(lst, Polyline_Leak, CurveExtension.DefaultCurveExtension, "Second");
            }
            else if (e.UserState.ToString() == "BuffFirst")
            {
                Graphic gra = e.Results[0];
                BufferParameters bufferParams = new BufferParameters()
                {
                    BufferSpatialReference = new SpatialReference(21480),
                    OutSpatialReference = new SpatialReference(21480),
                    Unit = LinearUnit.Meter,
                };
                bufferParams.Distances.Add(dbleaklen);
                bufferParams.Features.Add(gra);
                geometryservice.BufferAsync(bufferParams);
            }
            else if (e.UserState.ToString() == "BuffSecond")
            {
                Polyline_Leak = e.Results[0].Geometry as Polyline;
                geometryservice.TrimExtendAsync(lstleakline, Polyline_Leak, CurveExtension.DefaultCurveExtension, "First");
            }
        }

        /// <summary>
        /// 生成安全点范围
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void geometryservice_BufferCompleted(object sender, GraphicsEventArgs e)
        {
            PointCollection pc = (e.Results[0].Geometry as Polygon).Rings[0];
            Polyline polyline = new Polyline();
            polyline.Paths.Add(pc);
            polyline.SpatialReference = new SpatialReference(21480);
            List<Graphic> lstgra = new List<Graphic>();
            lstgra.Add(new Graphic() { Geometry = polyline });
            geometryservice.ProjectAsync(lstgra, map.SpatialReference, "BuffSecond");
        }

        /// <summary>
        /// 根据两点形成直线，获取以P2为直线，距离为N的点
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        MapPoint ReturnCzPoint(MapPoint p1, MapPoint p2, double len)
        {
            MapPoint mp = new MapPoint();
            double slope1 = 0;
            if (p2.Y == p1.Y)
            {
                mp.X = p1.X;
                mp.Y = (p1.Y + len);
            }
            else if (p1.X == p2.X)
            {
                mp.X = (p1.X + len);
                mp.Y = p1.Y;
            }
            else
            {
                double slope = (p2.Y - p1.Y) / (p2.X - p1.X);
                slope1 = -1 / slope;//其垂直直线的斜率  
                double lm = 0;
                lm = p1.Y - p1.X * slope;//原始直线方程;y=slope*x+lm  
                //其垂直直线的lm
                double lm1 = p2.Y - slope1 * p2.X;
                double tmp = lm1 - p2.Y;
                double p4x = ReturnX((1 + Math.Pow(slope1, 2)), (-2 * p2.X + 2 * slope1 * tmp), Math.Pow(p2.X, 2) + Math.Pow(tmp, 2) - Math.Pow(len, 2));
                mp.X = p4x;
                double p4y = slope1 * p4x + lm1;
                mp.Y = p4y;
            }
            return mp;
        }

        /// <summary>
        /// 解析一元二次方程
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        double ReturnX(double a, double b, double c)
        {
            double d = b * b;
            double e = 4 * a * c;
            double f = d - e;
            double g = (int)(Math.Sqrt(f));
            double i = -b + g;
            double j = -b - g;
            double h = i / (2 * a);
            double k = j / (2 * a);
            //判断其根的状况
            if (f == 0)
            {
                //Console.WriteLine("此方程有一根为:" + h);
            }
            else if (f > 0)
            {
                //Console.WriteLine("此方程有二根为:" + h);
                //Console.WriteLine("               " + k);
            }
            else
            {
                //Console.WriteLine("此方程没有根");
            }
            return h;
        }

    }
}
