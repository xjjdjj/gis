using System;
using System.Collections.Generic;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Tasks;
using System.IO;
using AYKJ.GISDevelop.Platform;
using System.ServiceModel;
using System.Linq;
namespace AYKJ.GISInterface
{
    /// <summary>
    /// 委托
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void RescueRouteDelegate(object sender, EventArgs e);

    public class clsRescueRoute
    {
        //定义事件
        public event RescueRouteDelegate RescueRouteEvent;
        public event RescueRouteDelegate RescueRouteFaildEvent;
        //路径分析的Map
        Map RouteMap;
        //网络分析
        RouteTask routeTask;
        //救援类型
        bool IsTrue;
        //路径分析的次数
        int routecount;
        //救援点列表
        List<Graphic> lst_Start;
        //事故发生点列表
        List<Graphic> lst_End;
        //返回路径列表
        public List<Graphic> lst_Result;
        //定义Geometry服务
        GeometryService geometryservice;
        //最短路径
        Graphic graShortRoute;
        //最短路径长度
        double dbShortLength = 0;
        //扩散路径
        List<Graphic> lstleak;
        AykjDataServiceInner.AykjDataClient client;
        void ProcessAction(object sender, EventArgs e)
        {
            if (RescueRouteEvent == null)
                RescueRouteEvent += new RescueRouteDelegate(RescueRouteErrorEvent);
            RescueRouteEvent(sender, e);
        }

        /// <summary>
        /// 如果没有自己指定关联方法，将会调用该方法抛出错误
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void RescueRouteErrorEvent(object sender, EventArgs e)
        {
            RescueRouteFaildEvent(sender, e);
        }


        /// <summary>
        /// 最短路径
        /// </summary>
        /// <param name="map">地图</param>
        /// <param name="lststart">起点</param>
        /// <param name="lstend">终点</param>
        /// <param name="lstbar">障碍点</param>
        /// <param name="strurl">百度Direction API URL</param>
        /// <param name="IsOnly">是否返回最短</param>
        public void RouteInit(Map map, List<Graphic> lststart, List<Graphic> lstend, List<Graphic> lstbar, string strurl, bool IsOnly)
        {
            RouteMap = map;
            graShortRoute = new Graphic();
            IsTrue = IsOnly;
            lst_Start = lststart;
            lst_End = lstend;
            lst_Result = new List<Graphic>();
            lstleak = lstbar;

            ZoomToVisualRange(lst_Start, lst_End);
            RouteSolve(strurl);
        }

        int indexStart = 0;
        int indexEnd = 0;

        void RouteSolve(string url)
        {
            GetRoute(lst_Start[indexStart], lst_End[indexEnd], url);
        }


        void GetRoute(Graphic grastart,Graphic graend, string url)
        {
            Coordinate start = new Coordinate((grastart.Geometry as MapPoint).X, (grastart.Geometry as MapPoint).Y);
            Coordinate end = new Coordinate((graend.Geometry as MapPoint).X, (graend.Geometry as MapPoint).Y);
            Coordinate startll = BaiduProjector.ConvertMC2LL(start);
            Coordinate endll = BaiduProjector.ConvertMC2LL(end);

            string strurl = string.Format(url, startll.Y, startll.X, endll.Y, endll.X);
            if (client == null)
            {
                var xele = PFApp.Extent;
                var dataServices = (from item in xele.Element("DataServices").Elements("DataService")
                                    where item.Attribute("Name").Value == "专题数据"
                                    select new
                                    {
                                        Type = item.Attribute("Type").Value,
                                        Url = item.Attribute("Url").Value,
                                    }).ToList();
                BasicHttpBinding binding = new BasicHttpBinding();
                binding.MaxReceivedMessageSize = int.MaxValue;
                client = new AykjDataServiceInner.AykjDataClient(binding, new EndpointAddress(dataServices[0].Url));
                client.GetCrossDomainAPICompleted += new EventHandler<AykjDataServiceInner.GetCrossDomainAPICompletedEventArgs>(client_GetCrossDomainAPICompleted);
            }

            client.GetCrossDomainAPIAsync(strurl, url);

        }

        void client_GetCrossDomainAPICompleted(object sender, AykjDataServiceInner.GetCrossDomainAPICompletedEventArgs e)
        {
            routecount = routecount + 1;
            Graphic lastBaiduRoute = new Graphic();
            lastBaiduRoute.Geometry = new Polyline();
            if (e.Result.Contains("success:"))
            {
                string str = e.Result.Remove(0, 8);
                var buffer = System.Text.Encoding.UTF8.GetBytes(str);
                var ms = new MemoryStream(buffer);
                var jsonObject = System.Json.JsonObject.Load(ms) as System.Json.JsonObject;
                if (jsonObject["status"].ToString() == "0")
                {
                    if (jsonObject["result"]["routes"] != null)
                    {
                        for (int j = 0; j < jsonObject["result"]["routes"].Count; j++)
                        {
                            List<MapPoint> tmpps = new List<MapPoint>();
                            for (int z = 0; z < jsonObject["result"]["routes"][j]["steps"].Count; z++)
                            {
                                double olng = jsonObject["result"]["routes"][j]["steps"][z]["stepOriginLocation"]["lng"];
                                double olat = jsonObject["result"]["routes"][j]["steps"][z]["stepOriginLocation"]["lat"];
                                double dlng = jsonObject["result"]["routes"][j]["steps"][z]["stepDestinationLocation"]["lng"];
                                double dlat = jsonObject["result"]["routes"][j]["steps"][z]["stepDestinationLocation"]["lat"];
                                Coordinate ocoor = BaiduProjector.ConvertLL2MC(new Coordinate(olng, olat));
                                tmpps.Add(new MapPoint(ocoor.X,ocoor.Y));
                                try
                                {
                                    string tmpstr = jsonObject["result"]["routes"][j]["steps"][z]["path"];
                                    string[] tmpstrs = tmpstr.ToString().Split(',');
                                    double plng = Double.Parse(tmpstrs[0]);
                                    double plat = Double.Parse(tmpstrs[1]);
                                    Coordinate pcoor = BaiduProjector.ConvertLL2MC(new Coordinate(plng, plat));
                                    tmpps.Add(new MapPoint(pcoor.X, pcoor.Y));
                                }
                                catch (Exception ex)
                                { }
                                Coordinate dcoor = BaiduProjector.ConvertLL2MC(new Coordinate(dlng, dlat));
                                tmpps.Add(new MapPoint(dcoor.X,dcoor.Y));
                                (lastBaiduRoute.Geometry as Polyline).Paths.Add(new PointCollection(tmpps));
                            }
                        }
                    }
                }
            }
            //20150115:保存路径的起始点信息
            lastBaiduRoute.Attributes.Add("info-start", lst_Start[indexStart].Attributes["info"]);
            lastBaiduRoute.Attributes.Add("info-end", lst_End[indexEnd].Attributes["info"]);

            lst_Result.Add(lastBaiduRoute);
            if (indexEnd == lst_End.Count - 1)
            {
                indexEnd = 0;
                if (indexStart == lst_Start.Count - 1)
                    indexStart = 0;
                else
                    indexStart++;
            }
            else
            {
                indexEnd++;
            }
            if (indexStart == 0 && indexEnd == 0)
            {
                routeSolveCompleted();
            }
            else
            {
                RouteSolve(e.UserState.ToString());
            }
        }

        void routeSolveCompleted()
        {
            if (IsTrue == false)
            {

                //lst_Point = lst_End;
                ZoomToVisualRange(lst_Start, lst_End);
                ProcessAction(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 最短路径
        /// </summary>
        /// <param name="map">承载分析地图服务</param>
        /// <param name="lstStart">起点</param>
        /// <param name="lstEnd">终点</param>
        /// <param name="strnurl">网络分析服务地址</param>
        /// <param name="strgurl">长度计算服务地址</param>
        /// <param name="IsOnly">是否返回最短</param>
        public void RouteInit(Map map, List<Graphic> lststart, List<Graphic> lstend, List<Graphic> lstbar, string strnurl, string strgurl, bool IsOnly)
        {
            RouteMap = map;
            graShortRoute = new Graphic();
            IsTrue = IsOnly;
            lst_Start = lststart;
            lst_End = lstend;
            lst_Result = new List<Graphic>();
            lstleak = lstbar;

            ZoomToVisualRange(lst_Start, lst_End);

            geometryservice = new GeometryService();
            geometryservice.Url = strgurl;
            geometryservice.Failed -= geometryservice_Failed;
            geometryservice.Failed += new EventHandler<TaskFailedEventArgs>(geometryservice_Failed);
            geometryservice.LengthsCompleted -= geometryservice_LengthsCompleted;
            geometryservice.LengthsCompleted += new EventHandler<LengthsEventArgs>(geometryservice_LengthsCompleted);

            routeTask = new RouteTask();
            routeTask.Url = strnurl;
            routeTask.SolveCompleted -= routeTask_SolveCompleted;
            routeTask.Failed -= routeTask_Failed;
            routeTask.SolveCompleted += new EventHandler<RouteEventArgs>(routeTask_SolveCompleted);
            routeTask.Failed += new EventHandler<TaskFailedEventArgs>(routeTask_Failed);
            RouteSolve();
        }

        /// <summary>
        /// 执行路径分析
        /// </summary>
        void RouteSolve()
        {
            routecount = 0;
            GraphicsLayer stopsGraphicsLayer = new GraphicsLayer();
            Graphic startgra = new Graphic() { Geometry = lst_Start[routecount].Geometry };
            Graphic endgra = new Graphic() { Geometry = lst_End[0].Geometry };
            stopsGraphicsLayer.Graphics.Add(startgra);
            stopsGraphicsLayer.Graphics.Add(endgra);

            RouteParameters routeParameters = new RouteParameters()
            {
                Stops = stopsGraphicsLayer,
                PolygonBarriers = lstleak,
                ReturnRoutes = true,
                OutSpatialReference = RouteMap.SpatialReference

            };
            routeTask.SolveAsync(routeParameters);
        }

        /// <summary>
        /// 路径分析返回结果
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void routeTask_SolveCompleted(object sender, RouteEventArgs e)
        {
            RouteResult routeResult = e.RouteResults[0];
            Graphic lastRoute = routeResult.Route;
            if (IsTrue == false)
            {
                lst_Result.Add(lastRoute);
                routecount = routecount + 1;
                if (routecount < lst_Start.Count)
                {
                    GraphicsLayer stopsGraphicsLayer = new GraphicsLayer();
                    Graphic startgra = new Graphic() { Geometry = lst_Start[routecount].Geometry };
                    Graphic endgra = new Graphic() { Geometry = lst_End[0].Geometry };
                    stopsGraphicsLayer.Graphics.Add(startgra);
                    stopsGraphicsLayer.Graphics.Add(endgra);

                    RouteParameters routeParameters = new RouteParameters()
                    {
                        Stops = stopsGraphicsLayer,
                        ReturnRoutes = true,
                        OutSpatialReference = RouteMap.SpatialReference
                    };
                    routeTask.SolveAsync(routeParameters);
                }
                else
                {
                    ProcessAction(this, EventArgs.Empty);
                }
            }
            else
            {
                List<Graphic> lsttmp = new List<Graphic>();
                lastRoute.Geometry.SpatialReference = RouteMap.SpatialReference;
                lsttmp.Add(lastRoute);
                geometryservice.LengthsAsync(lsttmp, LinearUnit.Meter, true, lastRoute);
            }
        }

        /// <summary>
        /// 长度计算返回结果
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void geometryservice_LengthsCompleted(object sender, LengthsEventArgs e)
        {
            if (routecount > 0)
            {
                if (e.Results[0] < dbShortLength)
                {
                    graShortRoute = e.UserState as Graphic;
                    dbShortLength = e.Results[0];
                }
            }
            else
            {
                graShortRoute = e.UserState as Graphic;
                dbShortLength = e.Results[0];
            }
            routecount = routecount + 1;
            if (routecount < lst_End.Count)
            {
                GraphicsLayer stopsGraphicsLayer = new GraphicsLayer();
                Graphic startgra = new Graphic() { Geometry = lst_Start[routecount].Geometry };
                Graphic endgra = new Graphic() { Geometry = lst_End[0].Geometry };
                stopsGraphicsLayer.Graphics.Add(startgra);
                stopsGraphicsLayer.Graphics.Add(endgra);

                RouteParameters routeParameters = new RouteParameters()
                {
                    Stops = stopsGraphicsLayer,
                    ReturnRoutes = true,
                    OutSpatialReference = RouteMap.SpatialReference

                };
                routeTask.SolveAsync(routeParameters);
            }
            else
            {
                lst_Result.Add(graShortRoute);
                ProcessAction(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 路径分析出错
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void routeTask_Failed(object sender, TaskFailedEventArgs e)
        {
            RescueRouteFaildEvent(sender, e);
        }

        /// <summary>
        /// 长度计算出错
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void geometryservice_Failed(object sender, TaskFailedEventArgs e)
        {
            RescueRouteFaildEvent(sender, e);
        }

        void ZoomToVisualRange(List<Graphic> lstStart, List<Graphic> lstEnd)
        {
            double dbxmax = lstStart[0].Geometry.Extent.XMax;
            double dbxmin = lstStart[0].Geometry.Extent.XMin;
            double dbymax = lstStart[0].Geometry.Extent.YMax;
            double dbymin = lstStart[0].Geometry.Extent.YMin;

            for (int i = 0; i < lstEnd.Count; i++)
            {
                if (dbxmax < lstEnd[i].Geometry.Extent.XMax)
                    dbxmax = lstEnd[i].Geometry.Extent.XMax;
                if (dbxmin > lstEnd[i].Geometry.Extent.XMin)
                    dbxmin = lstEnd[i].Geometry.Extent.XMin;
                if (dbymax < lstEnd[i].Geometry.Extent.YMax)
                    dbymax = lstEnd[i].Geometry.Extent.YMax;
                if (dbymin > lstEnd[i].Geometry.Extent.YMin)
                    dbymin = lstEnd[i].Geometry.Extent.YMin;
            }
            for (int i = 1; i < lstStart.Count; i++)
            {
                if (dbxmax < lstStart[i].Geometry.Extent.XMax)
                    dbxmax = lstStart[i].Geometry.Extent.XMax;
                if (dbxmin > lstStart[i].Geometry.Extent.XMin)
                    dbxmin = lstStart[i].Geometry.Extent.XMin;
                if (dbymax < lstStart[i].Geometry.Extent.YMax)
                    dbymax = lstStart[i].Geometry.Extent.YMax;
                if (dbymin > lstStart[i].Geometry.Extent.YMin)
                    dbymin = lstStart[i].Geometry.Extent.YMin;
            }
            Envelope eve = new Envelope()
            {
                XMax = dbxmax + (dbxmax - dbxmin) / 3,
                XMin = dbxmin - (dbxmax - dbxmin) / 3,
                YMax = dbymax + (dbymax - dbymin) / 3,
                YMin = dbymin - (dbymax - dbymin) / 3,
                SpatialReference = RouteMap.SpatialReference

            };
            RouteMap.ZoomTo(eve);
        }
    }
}
