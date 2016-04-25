using System;
using System.Collections.Generic;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Tasks;
using System.Linq;

namespace AYKJ.GISInterface
{
    /// <summary>
    /// 委托
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void ContinueSafePointDelegate(object sender, EventArgs e);

    public class clsContinueSafePoint
    {
        //定义事件
        public event ContinueSafePointDelegate SafePointEvent;
        public event ContinueSafePointDelegate SafePointFaildEvent;
        //GeometryService
        GeometryService geometryservice;
        //Query查询
        QueryTask querytask;
        //网络分析服务
        RouteTask routetask;
        //受体点
        MapPoint Mp_Recipient;
        //事故边缘线
        Polyline Polyline_Leak;

        //事故区域
        Polygon Polygon_Leak;
        //禁止逃离区域
        Graphic NoRunPolygon;
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
        //double dbshortlen;
        ////事故点和受体的连线集合
        //List<Polyline> lstleakline;
        //事故点
        MapPoint Mp_Leak;
        int rcount = 0;
        List<Graphic> lstroad;
        List<Graphic> lstroadend = new List<Graphic>();
        Graphic LeakArea;


        void ProcessAction(object sender, EventArgs e)
        {
            if (SafePointEvent == null)
                SafePointEvent += new ContinueSafePointDelegate(SafePointErrorEvent);
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
        /// <param name="leakarea">避开区域</param>
        /// <param name="strgurl">空间服务地址</param>
        /// <param name="strnurl">网络服务地址</param>
        /// <param name="strqurl">路线查询地址</param>
        public void SafeRoute(Map tmpmap, MapPoint mprecipient, MapPoint mpleak, Graphic leakarea, string strgurl, string strnurl, string strqurl)
        {
            LeakArea = leakarea;
            map = tmpmap;
            strgeourl = strgurl;
            strneturl = strnurl;
            Mp_Leak = mpleak;
            Mp_Recipient = mprecipient;

            geometryservice = new GeometryService(strgeourl);
            geometryservice.Failed += new EventHandler<TaskFailedEventArgs>(geometryservice_Failed);
            geometryservice.ProjectCompleted += new EventHandler<GraphicsEventArgs>(geometryservice_ProjectCompleted);
            geometryservice.TrimExtendCompleted += new EventHandler<GraphicsEventArgs>(geometryservice_TrimExtendCompleted);
            geometryservice.BufferCompleted += new EventHandler<GraphicsEventArgs>(geometryservice_BufferCompleted);
            geometryservice.CutCompleted += new EventHandler<CutEventArgs>(geometryservice_CutCompleted);
            geometryservice.RelationCompleted += new EventHandler<RelationEventArgs>(geometryservice_RelationCompleted);

            querytask = new QueryTask(strqurl);
            querytask.Failed += new EventHandler<TaskFailedEventArgs>(querytask_Failed);
            querytask.ExecuteCompleted += new EventHandler<QueryEventArgs>(querytask_ExecuteCompleted);

            routetask = new RouteTask(strnurl);
            routetask.Failed += new EventHandler<TaskFailedEventArgs>(routetask_Failed);
            routetask.SolveClosestFacilityCompleted += new EventHandler<RouteEventArgs>(routetask_SolveClosestFacilityCompleted);

            List<Graphic> lst = new List<Graphic>();
            lst.Add(leakarea);
            geometryservice.ProjectAsync(lst, new SpatialReference(21480), "BuffFirst");
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
                MapPoint mp1 = e.Results[0].Geometry as MapPoint;
                MapPoint mp2 = e.Results[1].Geometry as MapPoint;
                MapPoint mp3 = e.Results[2].Geometry as MapPoint;
                MapPoint mpout = ReturnPoint(mp1, mp2, mp3);
                List<Graphic> lst = new List<Graphic>();
                mpout.SpatialReference = mp1.SpatialReference;
                Graphic gra = new Graphic() { Geometry = mpout };
                lst.Add(gra);
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

                //GraphicsLayer gl = new GraphicsLayer();
                //ESRI.ArcGIS.Client.Symbols.SimpleLineSymbol sls = new ESRI.ArcGIS.Client.Symbols.SimpleLineSymbol();
                //sls.Color = new System.Windows.Media.SolidColorBrush(new System.Windows.Media.Color() { A = 255, R = 255, B = 0, G = 0 });
                //Graphic gra = new Graphic() { Geometry = polyline };
                //Graphic gra1 = new Graphic() { Geometry = Polyline_Leak };
                //gra.Symbol = sls;
                //gra1.Symbol = sls;
                //gl.Graphics.Add(gra1);
                //gl.Graphics.Add(gra);
                //map.Layers.Add(gl);

                List<Polyline> lst = new List<Polyline>();
                lst.Add(polyline);
                Polyline_Leak = new Polyline();
                Polyline_Leak.Paths.Add(Polygon_Leak.Rings[0]);
                Polyline_Leak.SpatialReference = Polygon_Leak.SpatialReference;
                geometryservice.TrimExtendAsync(lst, Polyline_Leak, CurveExtension.DefaultCurveExtension);
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
                bufferParams.Distances.Add(30);
                bufferParams.Features.Add(gra);
                geometryservice.BufferAsync(bufferParams);
            }
            else if (e.UserState.ToString() == "BuffSecond")
            {
                List<Graphic> lst = new List<Graphic>();
                Mp_Leak.SpatialReference = Mp_Recipient.SpatialReference;
                Graphic gra1 = new Graphic() { Geometry = Mp_Leak };
                MapPoint mp1 = (LeakArea.Geometry as Polygon).Rings[0][100];
                mp1.SpatialReference = Mp_Recipient.SpatialReference;
                Graphic gra2 = new Graphic() { Geometry = mp1 };
                Graphic gra3 = new Graphic() { Geometry = Mp_Recipient };
                lst.Add(gra1);
                lst.Add(gra2);
                lst.Add(gra3);
                Polygon_Leak = new Polygon();
                Polygon_Leak = e.Results[0].Geometry as Polygon;
                Polygon_Leak.SpatialReference = Mp_Recipient.SpatialReference;
                geometryservice.ProjectAsync(lst, new SpatialReference(21480), "First");
            }
        }

        void geometryservice_BufferCompleted(object sender, GraphicsEventArgs e)
        {
            //PointCollection pc = (e.Results[0].Geometry as Polygon).Rings[0];
            //Polyline polyline = new Polyline();
            //polyline.Paths.Add(pc);
            //polyline.SpatialReference = new SpatialReference(21480);
            List<Graphic> lstgra = new List<Graphic>();
            lstgra.Add(e.Results[0]);
            //lstgra.Add(new Graphic() { Geometry = polyline });
            geometryservice.ProjectAsync(lstgra, map.SpatialReference, "BuffSecond");
        }


        /// <summary>
        /// 延长线成功
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void geometryservice_TrimExtendCompleted(object sender, GraphicsEventArgs e)
        {
            NoRunGraphic = e.Results[0];
            Polyline_Leak = new Polyline();

            Graphic gra = new Graphic() { Geometry = Polygon_Leak };
            List<Graphic> lst = new List<Graphic>();
            lst.Add(gra);
            if ((NoRunGraphic.Geometry as Polyline).Paths.Count == 0)
                return;
            MapPoint mp1 = (NoRunGraphic.Geometry as Polyline).Paths[0][0];
            MapPoint mp2 = (NoRunGraphic.Geometry as Polyline).Paths[0][(NoRunGraphic.Geometry as Polyline).Paths[0].Count - 1];
            double db1 = GetDistance(mp1.X, mp1.Y, Mp_Recipient.X, Mp_Recipient.Y);
            double db2 = GetDistance(mp2.X, mp2.Y, Mp_Recipient.X, Mp_Recipient.Y);
            if (db1 < db2)
            {
                Return_MapPoint = mp1;
            }
            else
            {
                Return_MapPoint = mp2;
            }
            Return_MapPoint.SpatialReference = Mp_Recipient.SpatialReference;

            //GraphicsLayer gl = new GraphicsLayer();
            //ESRI.ArcGIS.Client.Symbols.SimpleLineSymbol sls = new ESRI.ArcGIS.Client.Symbols.SimpleLineSymbol();
            //sls.Color = new System.Windows.Media.SolidColorBrush(new System.Windows.Media.Color() { A = 255, R = 255, B = 0, G = 0 });
            //NoRunGraphic.Symbol = sls;
            //gl.Graphics.Add(NoRunGraphic);
            //map.Layers.Add(gl);

            geometryservice.CutAsync(lst, NoRunGraphic.Geometry as Polyline, "");
        }


        void IsContian(Polygon pl_a, Polygon pl_b)
        {
            geometryservice.RelationCompleted -= geometryservice_RelationCompleted;
            geometryservice.RelationCompleted += new EventHandler<RelationEventArgs>(geometryservice_RelationCompleted);

            //ESRI.ArcGIS.Client.Symbols.SimpleFillSymbol sfs = new ESRI.ArcGIS.Client.Symbols.SimpleFillSymbol();
            //sfs.Fill = new System.Windows.Media.SolidColorBrush(new System.Windows.Media.Color() { A = 100, R = 255, B = 0, G = 0 });
            //Graphic gra = new Graphic { Geometry = pl_a };
            //gra.Symbol = sfs;
            //GraphicsLayer gl = new GraphicsLayer();
            //gl.Graphics.Add(gra);
            //map.Layers.Add(gl);

            List<Graphic> lst1 = new List<Graphic>();
            lst1.Add(new Graphic() { Geometry = pl_a });
            //lst1.Add(new Graphic() { Geometry = pl_b });
            List<Graphic> lstGra = new List<Graphic>();
            lstGra.Add(new Graphic() { Geometry = Mp_Leak });

            List<Graphic> lst2 = new List<Graphic>();
            lst2.Add(new Graphic() { Geometry = pl_a });
            lst2.Add(new Graphic() { Geometry = pl_b });
            if (geometryservice.IsBusy == false)
            {
                geometryservice.RelationAsync(lst1, lstGra, GeometryRelation.esriGeometryRelationIntersection, null, lst2);
            }
        }

        /// <summary>
        /// 获取安全逃生弧线
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void geometryservice_CutCompleted(object sender, CutEventArgs e)
        {
            if (e.UserState.ToString() != "Road")
            {
                List<Graphic> lsttmp = (sender as GeometryService).CutLastResult.ToList();
                IsContian(lsttmp[0].Geometry as Polygon, lsttmp[lsttmp.Count - 1].Geometry as Polygon);
            }
            else
            {
                for (int i = 0; i < (e.Results[0].Geometry as Polyline).Paths[0].Count; i++)
                {
                    if ((e.Results[1].Geometry as Polyline).Paths[0].Contains((e.Results[0].Geometry as Polyline).Paths[0][i]))
                    {
                        MapPoint tmpmp = (e.Results[0].Geometry as Polyline).Paths[0][i];
                        tmpmp.SpatialReference = map.SpatialReference;
                        lstroadend.Add(new Graphic() { Geometry = tmpmp });
                    }
                }

                rcount = rcount + 1;
                if (rcount < lstroad.Count)
                {
                    List<Graphic> lst = new List<Graphic>();
                    lst.Add(lstroad[rcount]);
                    geometryservice.CutAsync(lst, Return_GraCz.Geometry as Polyline, "Road");
                }
                else
                {
                    //GraphicsLayer gl = new GraphicsLayer();
                    //foreach (Graphic gra in lstroadend)
                    //{
                    //    ESRI.ArcGIS.Client.Symbols.SimpleMarkerSymbol sms = new ESRI.ArcGIS.Client.Symbols.SimpleMarkerSymbol();
                    //    sms.Color = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
                    //    sms.Size = 5;
                    //    gra.Symbol = sms;
                    //    gl.Graphics.Add(gra);
                    //}
                    //map.Layers.Add(gl);

                    //clsEscapeRoute clsescaperoute = new clsEscapeRoute();
                    //clsescaperoute.EscapeRouteFaildEvent += new EscapeRouteDelegate(clsescaperoute_EscapeRouteFaildEvent);
                    //clsescaperoute.EscapeRouteEvent += new EscapeRouteDelegate(clsescaperoute_EscapeRouteEvent);
                    //clsescaperoute.RouteInit(map, lstStart, lstroadend, lstbar, strneturl, strgeourl, true);


                    //Graphic gratmp = new Graphic();
                    //Polygon polygon = new Polygon();
                    //for (int i = 0; i < lstarea.Count; i++)
                    //{
                    //    polygon.Rings.Add((lstarea[i].Geometry as Polygon).Rings[0]);
                    //}
                    //gratmp.Geometry = polygon;
                    //gratmp.Geometry.SpatialReference = map.SpatialReference;
                    //List<Graphic> lsttmparea = new List<Graphic>();
                    //lsttmparea.Add(gratmp);
                    //geometryservice.RelationAsync(lsttmparea, lstroadend, GeometryRelation.esriGeometryRelationIntersection, null, lstroadend);

                    List<Graphic> lstStart = new List<Graphic>();
                    Graphic regra = new Graphic() { Geometry = Mp_Recipient };
                    lstStart.Add(regra);
                    List<Graphic> lstbar = new List<Graphic>();
                    if (NoRunPolygon!=null)
                    lstbar.Add(NoRunPolygon);

                    RouteClosestFacilityParameters routeParams = new RouteClosestFacilityParameters()
                    {
                        Incidents = lstStart,
                        Barriers = null,
                        PolylineBarriers = null,
                        PolygonBarriers = lstbar,
                        Facilities = lstroadend,
                        ReturnDirections = true,
                        DirectionsLanguage = new System.Globalization.CultureInfo("en-US"),
                        DirectionsLengthUnits = esriUnits.esriMeters,
                        DirectionsTimeAttribute = "Meters",
                        ReturnRoutes = true,
                        ReturnFacilities = true,
                        ReturnIncidents = true,
                        ReturnBarriers = true,
                        ReturnPolylineBarriers = true,
                        ReturnPolygonBarriers = true,
                        FacilityReturnType = FacilityReturnType.ServerFacilityReturnAll,
                        OutputLines = "esriNAOutputLineTrueShape",//"esriNAOutputLineNone","esriNAOutputLineStraight"
                        DefaultCutoff = double.NaN,
                        DefaultTargetFacilityCount = 1,
                        TravelDirection = FacilityTravelDirection.TravelDirectionToFacility,
                        OutSpatialReference = map.SpatialReference,
                        RestrictUTurns = "esriNFSBAllowBacktrack",//"esriNFSBAtDeadEndsOnly","esriNFSBNoBacktrack"
                        UseHierarchy = true,
                        OutputGeometryPrecision = 0,
                        OutputGeometryPrecisionUnits = esriUnits.esriMeters
                    };
                    routetask.SolveClosestFacilityAsync(routeParams);
                }
            }
            return;
        }

        /// <summary>
        /// 查询道路成功
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void querytask_ExecuteCompleted(object sender, QueryEventArgs e)
        {
            lstroad = e.FeatureSet.Features.ToList();
            if (lstroad.Count == 0)
            {
                PointCollection pc = new PointCollection();
                pc.Add(Mp_Recipient);
                pc.Add(Return_MapPoint);
                Polyline pl = new Polyline();
                pl.Paths.Add(pc);
                pl.SpatialReference = map.SpatialReference;
                Return_lst = new List<Graphic>();
                Return_lst.Add(new Graphic() { Geometry = pl });
                ProcessAction(this, EventArgs.Empty);
                return;
            }
            List<Graphic> lst = new List<Graphic>();
            rcount = 0;
            lst.Add(lstroad[rcount]);
            geometryservice.CutAsync(lst, Return_GraCz.Geometry as Polyline, "Road");
        }

        void geometryservice_RelationCompleted(object sender, RelationEventArgs e)
        {
            if (e.UserState != null)
            {
                if (e.Results.Count == 0)
                {
                    NoRunPolygon = (e.UserState as List<Graphic>)[1];
                    PointCollection pc = ((e.UserState as List<Graphic>)[0].Geometry as Polygon).Rings[0];
                    PointCollection pc_all = new PointCollection();
                    PointCollection pcline = (NoRunGraphic.Geometry as Polyline).Paths[0];
                    for (int i = 0; i < pc.Count; i++)
                    {
                        if (!pcline.Contains(pc[i]))
                        {
                            pc_all.Add(pc[i]);
                        }
                    }
                    Polyline pl = new Polyline();
                    pl.Paths.Add(pc_all);
                    pl.SpatialReference = (e.UserState as List<Graphic>)[0].Geometry.SpatialReference;
                    Return_GraCz = new Graphic() { Geometry = pl };
                    //Return_GraCz = (e.UserState as List<Graphic>)[0];
                }
                else
                {
                    try
                    {
                        NoRunPolygon = (e.UserState as List<Graphic>)[2];
                    }
                    catch(Exception ex)
                    {
                        NoRunPolygon = null;
                    }
                    PointCollection pc = ((e.UserState as List<Graphic>)[1].Geometry as Polygon).Rings[0];
                    PointCollection pc_all = new PointCollection();
                    PointCollection pcline = (NoRunGraphic.Geometry as Polyline).Paths[0];
                    for (int i = 0; i < pc.Count; i++)
                    {
                        if (!pcline.Contains(pc[i]))
                        {
                            pc_all.Add(pc[i]);
                        }
                    }
                    Polyline pl = new Polyline();
                    pl.Paths.Add(pc_all);
                    pl.SpatialReference = (e.UserState as List<Graphic>)[1].Geometry.SpatialReference;
                    Return_GraCz = new Graphic() { Geometry = pl };
                    //Return_GraCz = (e.UserState as List<Graphic>)[1];
                }

                //GraphicsLayer gl = new GraphicsLayer();
                //ESRI.ArcGIS.Client.Symbols.SimpleLineSymbol sls = new ESRI.ArcGIS.Client.Symbols.SimpleLineSymbol();
                //sls.Color = new System.Windows.Media.SolidColorBrush(new System.Windows.Media.Color() { A = 255, R = 255, B = 255, G = 0 });
                //Return_GraCz.Symbol = sls;
                //gl.Graphics.Add(Return_GraCz);
                //map.Layers.Add(gl);

                Query query = new Query();
                query.ReturnGeometry = true;
                query.Geometry = Return_GraCz.Geometry;
                querytask.ExecuteAsync(query);
            }
            else
            {
                List<Graphic> lstGraphic = new List<Graphic>();
                if (e.Results.Count != 0)
                {
                    for (int i = 0; i < e.Results.Count; i++)
                    {
                        lstGraphic.Add(lstroadend[e.Results[i].Graphic2Index]);
                    }
                    for (int i = 0; i < lstGraphic.Count; i++)
                    {
                        lstroadend.Remove(lstGraphic[i]);
                    }
                }
            }
        }

        /// <summary>
        /// 最近道路点计算
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void routetask_SolveClosestFacilityCompleted(object sender, RouteEventArgs e)
        {
            Graphic g = e.RouteResults[0].Route;
            (g.Geometry as Polyline).Paths[0].Insert(0, Mp_Recipient);

            Return_lst = new List<Graphic>();
            Return_lst.Add(g);

            ProcessAction(this, EventArgs.Empty);
        }

        /// <summary>
        /// 逃生路径计算成功
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clsescaperoute_EscapeRouteEvent(object sender, EventArgs e)
        {
            Return_lst = (sender as clsEscapeRoute).lst_Result;
            (Return_lst[0].Geometry as Polyline).Paths[0].Insert(0, Mp_Recipient);
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
        /// 最近道路点计算失败
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void routetask_Failed(object sender, TaskFailedEventArgs e)
        {
            PointCollection pc = new PointCollection();
            pc.Add(Mp_Recipient);
            pc.Add(Return_MapPoint);
            Polyline pl = new Polyline();
            pl.Paths.Add(pc);
            pl.SpatialReference = map.SpatialReference;
            Return_lst = new List<Graphic>();
            Return_lst.Add(new Graphic() { Geometry = pl });
            ProcessAction(this, EventArgs.Empty);
            //SafePointFaildEvent(sender, e);
        }

        /// <summary>
        /// 逃生路线计算失败
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clsescaperoute_EscapeRouteFaildEvent(object sender, EventArgs e)
        {
            PointCollection pc = new PointCollection();
            pc.Add(Mp_Recipient);
            pc.Add(Return_MapPoint);
            Polyline pl = new Polyline();
            pl.Paths.Add(pc);
            pl.SpatialReference = map.SpatialReference;
            Return_lst = new List<Graphic>();
            Return_lst.Add(new Graphic() { Geometry = pl });
            ProcessAction(this, EventArgs.Empty);
            //SafePointFaildEvent(sender, e);
        }

        /// <summary>
        /// 道路查询失败
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void querytask_Failed(object sender, TaskFailedEventArgs e)
        {
            PointCollection pc = new PointCollection();
            pc.Add(Mp_Recipient);
            pc.Add(Return_MapPoint);
            Polyline pl = new Polyline();
            pl.Paths.Add(pc);
            pl.SpatialReference = map.SpatialReference;
            Return_lst = new List<Graphic>();
            Return_lst.Add(new Graphic() { Geometry = pl });
            ProcessAction(this, EventArgs.Empty);
            //SafePointFaildEvent(sender, e);
        }

        /// <summary>
        /// 计算发生错误
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void geometryservice_Failed(object sender, TaskFailedEventArgs e)
        {
            PointCollection pc = new PointCollection();
            pc.Add(Mp_Recipient);
            pc.Add(Return_MapPoint);
            Polyline pl = new Polyline();
            pl.Paths.Add(pc);
            pl.SpatialReference = map.SpatialReference;
            Return_lst = new List<Graphic>();
            Return_lst.Add(new Graphic() { Geometry = pl });
            ProcessAction(this, EventArgs.Empty);
            //SafePointFaildEvent(sender, e);
        }

        /// <summary>
        /// 求某点在两点形成的直线上的的投影点
        /// </summary>
        /// <param name="p1">形成直线的某一点</param>
        /// <param name="p2">形成直线的另一点</param>
        /// <param name="pout">直线外以ian</param>
        /// <returns></returns>
        MapPoint ReturnPoint(MapPoint p1, MapPoint p2, MapPoint pout)
        {
            MapPoint pProject = new MapPoint();
            double k = (double)((p2.Y - p1.Y) / (p2.X - p1.X));
            //垂线斜率不存在情况
            if (k == 0)
            {
                pProject.X = pout.X;
                pProject.Y = p1.Y;
            }
            else
            {
                pProject.X = (float)((k * p1.X + pout.X / k + pout.Y - p1.Y) / (1 / k + k));
                pProject.Y = (float)(-1 / k * (pProject.X - pout.X) + pout.Y);
            }
            return pProject;
        }

        private const double EARTH_RADIUS = 6378.137;//地球半径
        private static double rad(double d)
        {
            return d * Math.PI / 180.0;
        }

        private static double GetDistance(double lat1, double lng1, double lat2, double lng2)
        {
            double radLat1 = rad(lat1);
            double radLat2 = rad(lat2);
            double a = radLat1 - radLat2;
            double b = rad(lng1) - rad(lng2);
            double s = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(a / 2), 2) +
             Math.Cos(radLat1) * Math.Cos(radLat2) * Math.Pow(Math.Sin(b / 2), 2)));
            s = s * EARTH_RADIUS;
            s = Math.Round(s * 10000) / 10000;
            return s;
        }

    }
}
