/// <summary>  
/// 作者：陈锋 
/// 时间：2012/9/3 10:01:53  
/// 公司:南京安元科技有限公司  
/// 版权：2012-2020  
/// CLR版本：4.0.30319.261  
/// clsAutoSearch说明：自动搜索专题
/// 唯一标识：1409ff1f-cc8f-468d-a0a4-7314e71cf640  
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using AYKJ.GISDevelop.Platform;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Tasks;

namespace AYKJ.GISInterface
{
    /// <summary>
    /// 委托
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void AutoAllSearchDelegate(object sender, EventArgs e);

    public class clsAutoAllSearch
    {
        //网络分析服务
        RouteTask routeTask;
        //空间查询服务
        clsRangeQuery clsrangequery;
        //查询的设施点
        List<Graphic> lstQueryGraphic;
        //空间关系服务地址
        string strgeourl;
        //设施点分析地址
        string strclosestfacilityurl;
        //进行服务区域分析的起点
        List<Graphic> lstFacilities;
        //服务区域分析次数
        int alscount;
        //每次搜索的范围
        double dbevery;
        //搜索的总范围
        double dball;
        //临时绘制的图层
        GraphicsLayer tmpgl;
        //查询的类型
        FacilityTravelDirection ftd;
        //所要查询的数据
        Dictionary<string, GraphicsLayer> Dict_Data;
        //要查询的数据类型
        List<string> lst_DataType;
        //服务区中心点
        Graphic CeterPointGraphic;
        //查询出来返回的数据
        public Dictionary<string, List<Graphic>> Dict_RData;
        //查询出来返回的路径属于哪个范围
        public Dictionary<string, Dictionary<Graphic, Int32>> Dict_RKm;
        //查询出来返回的最近数据的距离
        public Dictionary<Graphic, double> Dict_ShortLen;
        //定义事件
        public event AutoAllSearchDelegate AutoSearchEvent;
        public event AutoAllSearchDelegate AutoSearchFaildEvent;

        void ProcessAction(object sender, EventArgs e)
        {
            if (AutoSearchEvent == null)
                AutoSearchEvent += new AutoAllSearchDelegate(AutoSearchErrorEvent);
            AutoSearchEvent(sender, e);
        }

        /// <summary>
        /// 如果没有自己指定关联方法，将会调用该方法抛出错误
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void AutoSearchErrorEvent(object sender, EventArgs e)
        {
            AutoSearchFaildEvent(sender, e);
        }

        /// <summary>
        /// 自动搜索离起点最近的某些设施
        /// </summary>
        /// <param name="StartGraphic">起点</param>
        /// <param name="strnurl">网络分析的地址</param>
        /// <param name="strgurl">空间服务的地址</param>
        /// <param name="strcfurl">设施点分析的地址</param>
        /// <param name="Dict_tmp">设施集合</param>
        /// <param name="lsttype">设施类别</param>
        /// <param name="db1">每次搜索的范围</param>
        /// <param name="db2">搜索的总范围</param>
        /// <param name="gl">临时展现图层</param>
        /// <param name="strtype">搜索类别("From"为从该点出发，"To"为到达该点)</param>
        public void AutoSearch(Graphic StartGraphic, string strnurl, string strgurl, string strcfurl, Dictionary<string, GraphicsLayer> Dict_tmp,
            List<string> lsttype, double db1, double db2, GraphicsLayer gl, clsAutoSearchType type)
        {
            tmpgl = gl;
            CeterPointGraphic = StartGraphic;
            dbevery = db1;
            dball = db2;
            lstQueryGraphic = new List<Graphic>();
            lst_DataType = lsttype;
            for (int i = 0; i < lst_DataType.Count; i++)
            {
                lstQueryGraphic.AddRange(Dict_tmp[lst_DataType[i]]);
            }
            if (lstQueryGraphic.Contains(StartGraphic))
                lstQueryGraphic.Remove(StartGraphic);
            Dict_RData = new Dictionary<string, List<Graphic>>();
            Dict_Data = new Dictionary<string, GraphicsLayer>();
            Dict_RKm = new Dictionary<string, Dictionary<Graphic, Int32>>();
            Dict_Data = Dict_tmp;
            if (type == clsAutoSearchType.To)
            {
                ftd = FacilityTravelDirection.TravelDirectionToFacility;
            }
            else
            {
                ftd = FacilityTravelDirection.TravelDirectionFromFacility;
            }
            strgeourl = strgurl;
            strclosestfacilityurl = strcfurl;
            routeTask = new RouteTask();
            routeTask.Url = strnurl;
            routeTask.Failed -= routeTask_Failed;
            routeTask.SolveServiceAreaCompleted -= routeTask_SolveServiceAreaCompleted;
            routeTask.SolveServiceAreaCompleted += new EventHandler<RouteEventArgs>(routeTask_SolveServiceAreaCompleted);
            routeTask.Failed += new EventHandler<TaskFailedEventArgs>(routeTask_Failed);
            clsrangequery = new clsRangeQuery();
            clsrangequery.RangeQueryEvent += new RangeQueryDelegate(clsrangequery_RangeQueryEvent);
            clsrangequery.RangeQueryFaildEvent += new RangeQueryDelegate(clsrangequery_RangeQueryFaildEvent);

            lstFacilities = new List<Graphic>();
            lstFacilities.Add(StartGraphic);
            alscount = 0;
            RouteServiceAreaParameters routeParams = new RouteServiceAreaParameters()
            {
                DefaultBreaks = dbevery.ToString(),
                Facilities = lstFacilities,
                TrimPolygonDistance = 1000,
                TrimPolygonDistanceUnits = esriUnits.esriMeters,
                OutSpatialReference = lstFacilities[0].Geometry.SpatialReference,
                TravelDirection = ftd
            };
            routeTask.SolveServiceAreaAsync(routeParams);
        }

        /// <summary>
        /// 服务半径分析结果
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void routeTask_SolveServiceAreaCompleted(object sender, RouteEventArgs e)
        {
            ESRI.ArcGIS.Client.Symbols.SimpleFillSymbol sfs = new ESRI.ArcGIS.Client.Symbols.SimpleFillSymbol();
            sfs.BorderBrush = new System.Windows.Media.SolidColorBrush(new System.Windows.Media.Color() { A = 255, R = 255, G = 100, B = 50 });
            Graphic gra = e.ServiceAreaPolygons[0];
            gra.Symbol = sfs;
            GraphicsLayer gl = new GraphicsLayer();
            tmpgl.Graphics.Add(gra);
            //(Application.Current as IApp).MainMap.Layers.Add(gl);
            (Application.Current as IApp).MainMap.ZoomTo(gra.Geometry);
            clsrangequery.RangeQuery(strgeourl, e.ServiceAreaPolygons[0].Geometry, lstQueryGraphic);
        }

        /// <summary>
        /// 空间查询结果
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clsrangequery_RangeQueryEvent(object sender, EventArgs e)
        {
            alscount = alscount + 1;
            //当返回数量为0的时候，继续扩大搜索半径
            if ((sender as clsRangeQuery).lstReturnGraphic.Count == 0)
            {
                if (dball < (dbevery * (alscount + 1)))
                {
                    if (Dict_RData.Count == 0)
                    {
                        ProcessAction(this, EventArgs.Empty);
                    }
                    else
                    {
                        RouteInit();
                        //ProcessAction(this, EventArgs.Empty);
                        //UseClosestFacility();
                    }
                    return;
                }

                RouteServiceAreaParameters routeParams = new RouteServiceAreaParameters()
                {
                    DefaultBreaks = (dbevery * (alscount + 1)).ToString(),
                    Facilities = lstFacilities,
                    TrimPolygonDistance = 1000,
                    TrimPolygonDistanceUnits = esriUnits.esriMeters,
                    OutSpatialReference = lstFacilities[0].Geometry.SpatialReference,
                    TravelDirection = ftd
                };
                routeTask.SolveServiceAreaAsync(routeParams);
            }
            //当返回数量不为0的时候，根据传入的值进行删选，看是否每个类别都有返回值
            else
            {
                List<Graphic> lstreturngra = (sender as clsRangeQuery).lstReturnGraphic;
                for (int i = 0; i < lstreturngra.Count; i++)
                {
                    string[] arytmp = lstreturngra[i].Attributes["StaTag"].ToString().Split('|');
                    if (!Dict_RData.Keys.Contains((Application.Current as IApp).DictThematicEnCn[arytmp[0]]))
                    {
                        List<Graphic> lsttmp = new List<Graphic>();
                        lsttmp.Add(lstreturngra[i]);
                        Dict_RData.Add((Application.Current as IApp).DictThematicEnCn[arytmp[0]], lsttmp);
                        Dictionary<Graphic, Int32> dict_km = new Dictionary<Graphic, Int32>();

                        dict_km.Add(lstreturngra[i], alscount - 1);
                        Dict_RKm.Add((Application.Current as IApp).DictThematicEnCn[arytmp[0]], dict_km);
                    }
                    else
                    {
                        if (!Dict_RData[(Application.Current as IApp).DictThematicEnCn[arytmp[0]]].Contains(lstreturngra[i]))
                        {
                            List<Graphic> lsttmp = Dict_RData[(Application.Current as IApp).DictThematicEnCn[arytmp[0]]];
                            lsttmp.Add(lstreturngra[i]);
                            Dict_RData.Remove((Application.Current as IApp).DictThematicEnCn[arytmp[0]]);
                            Dict_RData.Add((Application.Current as IApp).DictThematicEnCn[arytmp[0]], lsttmp);

                            Dictionary<Graphic, Int32> dict_km = Dict_RKm[(Application.Current as IApp).DictThematicEnCn[arytmp[0]]];
                            dict_km.Add(lstreturngra[i], alscount - 1);
                            Dict_RKm.Remove((Application.Current as IApp).DictThematicEnCn[arytmp[0]]);
                            Dict_RKm.Add((Application.Current as IApp).DictThematicEnCn[arytmp[0]], dict_km);
                        }
                    }
                }
                //查询到所有类型，结束搜索操作，进行最短路径查询
                if (Dict_RData.Count == lst_DataType.Count)
                {
                    RouteInit();
                    //ProcessAction(this, EventArgs.Empty);
                    //UseClosestFacility();
                }
                //继续查找未搜索到的类型
                else
                {

                    if (dball < (dbevery * (alscount + 1)))
                    {
                        if (Dict_RData.Count == 0)
                        {
                            ProcessAction(this, EventArgs.Empty);
                        }
                        else
                        {
                            RouteInit();
                            //ProcessAction(this, EventArgs.Empty);
                            //UseClosestFacility();
                        }
                        return;
                    }

                    lstQueryGraphic = new List<Graphic>();
                    for (int i = 0; i < lst_DataType.Count; i++)
                    {
                        //if (!Dict_RData.Keys.Contains(lst_DataType[i]))
                        //{
                        lstQueryGraphic.AddRange(Dict_Data[lst_DataType[i]]);
                        //}
                    }
                    if (lstQueryGraphic.Contains(CeterPointGraphic))
                        lstQueryGraphic.Remove(CeterPointGraphic);
                    RouteServiceAreaParameters routeParams = new RouteServiceAreaParameters()
                    {
                        DefaultBreaks = (dbevery * (alscount + 1)).ToString(),
                        Facilities = lstFacilities,
                        TrimPolygonDistance = 1000,
                        TrimPolygonDistanceUnits = esriUnits.esriMeters,
                        OutSpatialReference = lstFacilities[0].Geometry.SpatialReference,
                        TravelDirection = ftd
                    };
                    routeTask.SolveServiceAreaAsync(routeParams);
                }
            }

        }

        /// <summary>
        /// 网络分析失败
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        void routeTask_Failed(object sender, TaskFailedEventArgs e)
        {
            AutoSearchFaildEvent(sender, e);
        }

        /// <summary>
        /// 空间查询失败
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clsrangequery_RangeQueryFaildEvent(object sender, EventArgs e)
        {
            AutoSearchFaildEvent(sender, e);
        }



        /// <summary>
        /// 最短路径
        /// </summary>
        /// <param name="lstStart">起点</param>
        /// <param name="lstEnd">终点</param>
        /// <param name="strnurl">网络分析服务地址</param>
        void RouteInit()
        {
            string str = routeTask.Url;
            str = str.Substring(0, str.Length - 14) + "Route";
            routeTask = new RouteTask();
            routeTask.Url = str;
            routeTask.SolveCompleted -= routeTask_SolveCompleted;
            routeTask.Failed -= routeTask_Failed;
            routeTask.SolveCompleted += new EventHandler<RouteEventArgs>(routeTask_SolveCompleted);
            routeTask.Failed += new EventHandler<TaskFailedEventArgs>(routeTask_Failed);

            lst_Start = new List<Graphic>();
            lstTypeRoute = new List<int>();
            for (int i = 0; i < Dict_RData.Count; i++)
            {
                lstTypeRoute.Add(Dict_RData.Values.ToList()[i].Count);
                lst_Start.AddRange(Dict_RData.Values.ToList()[i]);
            }

            RouteSolve();
        }
        List<Graphic> lst_Start = new List<Graphic>();
        List<int> lstTypeRoute = new List<int>();
        List<Graphic> lst_Route = new List<Graphic>();

        /// <summary>
        /// 执行路径分析
        /// </summary>
        void RouteSolve()
        {
            alscount = 0;
            lst_Route = new List<Graphic>();
            GraphicsLayer stopsGraphicsLayer = new GraphicsLayer();
            Graphic startgra = new Graphic() { Geometry = lst_Start[alscount].Geometry };
            Graphic endgra = new Graphic() { Geometry = CeterPointGraphic.Geometry };
            stopsGraphicsLayer.Graphics.Add(startgra);
            stopsGraphicsLayer.Graphics.Add(endgra);

            RouteParameters routeParameters = new RouteParameters()
            {
                Stops = stopsGraphicsLayer,
                ReturnRoutes = true,
                OutSpatialReference = CeterPointGraphic.Geometry.SpatialReference

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
            lst_Route.Add(routeResult.Route);
            alscount = alscount + 1;
            if (alscount < lst_Start.Count)
            {
                GraphicsLayer stopsGraphicsLayer = new GraphicsLayer();
                Graphic startgra = new Graphic() { Geometry = lst_Start[alscount].Geometry };
                Graphic endgra = new Graphic() { Geometry = CeterPointGraphic.Geometry };
                stopsGraphicsLayer.Graphics.Add(startgra);
                stopsGraphicsLayer.Graphics.Add(endgra);

                RouteParameters routeParameters = new RouteParameters()
                {
                    Stops = stopsGraphicsLayer,
                    ReturnRoutes = true,
                    OutSpatialReference = CeterPointGraphic.Geometry.SpatialReference
                };
                routeTask.SolveAsync(routeParameters);
            }
            else
            {
                GeometryService geometryservice = new GeometryService();
                geometryservice.Url = strgeourl;
                geometryservice.Failed -= geometryservice_Failed;
                geometryservice.Failed += new EventHandler<TaskFailedEventArgs>(geometryservice_Failed);
                geometryservice.LengthsCompleted -= geometryservice_LengthsCompleted;
                geometryservice.LengthsCompleted += new EventHandler<LengthsEventArgs>(geometryservice_LengthsCompleted);
                geometryservice.LengthsAsync(lst_Route, LinearUnit.Meter, true, null);
            }
        }

        /// <summary>
        /// 长度计算返回结果
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void geometryservice_LengthsCompleted(object sender, LengthsEventArgs e)
        {
            Dict_ShortLen = new Dictionary<Graphic, double>();
            int k = 0;
            for (int i = 0; i < lstTypeRoute.Count; i++)
            {
                for (int m = 0; m < lstTypeRoute[i]; m++)
                {
                    Dict_ShortLen.Add(Dict_RData.Values.ToList()[i][m], e.Results[k]);
                    k = k + 1;
                }
            }
            ProcessAction(this, EventArgs.Empty);
        }

        /// <summary>
        /// 长度计算出错
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void geometryservice_Failed(object sender, TaskFailedEventArgs e)
        {
            AutoSearchFaildEvent(sender, e);
        }
    }
}
