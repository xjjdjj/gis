using System;
using System.Collections.Generic;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Tasks;
using AYKJ.GISDevelop.Platform;

namespace AYKJ.GISInterface
{
    /// <summary>
    /// 委托
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void CircleRouteAnalysisDelegate(object sender, EventArgs e);

    public class clsCircleRouteAnalysis
    {
        //定义事件
        public event CircleRouteAnalysisDelegate RouteAnalysisEvent;
        public event CircleRouteAnalysisDelegate RouteAnalysisFaildEvent;
        //救援点集合
        List<Graphic> lst_Rescue;
        //受体点集合
        List<Graphic> lst_Recipient;
        //边缘点集合
        List<Graphic> lst_Edge;
        //安全点集合
        List<Graphic> lst_Safety;
        //最快逃离区域
        Graphic gra_Leak;
        //事故发生点
        MapPoint Mp_Accident;
        //承载的地图服务
        Map map;
        //空间服务地址
        string strgeourl;
        //网络分析服务地址
        string strneturl;
        //网络分析最短路径服务地址
        string strclosestfacilityurl;
        //路径查询地址
        string strqueryurl;
        string strquerybaiduurl;
        //救援路线类
        clsRescueRoute clsrescueroute;
        //最短逃生路径类
        clsEscapeRoute clsescaperoute;
        //最短事故区域内逃生路径类
        clsCircleSafePoint clscirclesafepoint;
        //计算次数
        int count;
        //最快救援路线
        public List<Graphic> lst_ReturnRescue;
        //最快逃生路线
        public List<Graphic> lst_ReturnEscape;
        //逃离区域里面的路径
        List<Graphic> lst_EscapeIn;
        //逃离区域的边缘点到安全点的路径
        List<Graphic> lst_EscapeOut;
        List<List<Graphic>> lst_new;
        //事故影响范围
        double dbleaklen;

        void ProcessAction(object sender, EventArgs e)
        {
            if (RouteAnalysisEvent == null)
                RouteAnalysisEvent += new CircleRouteAnalysisDelegate(RouteAnalysisErrorEvent);
            RouteAnalysisEvent(sender, e);
        }

        /// <summary>
        /// 如果没有自己指定关联方法，将会调用该方法抛出错误
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void RouteAnalysisErrorEvent(object sender, EventArgs e)
        {
            RouteAnalysisFaildEvent(sender, e);
        }

        /// <summary>
        /// 分析最短救援和逃生路径
        /// </summary>
        /// <param name="lstrescue">救援点集合</param>
        /// <param name="lstrecipient">受体点集合</param>
        /// <param name="lstSafety">安全点集合</param>
        /// <param name="graleak">最快逃离区域</param>
        /// <param name="mpaccident">事故发生点</param>
        /// <param name="sp">空间参考</param>
        /// <param name="strgurl">空间服务地址</param>
        /// <param name="strnurl">网络服务地址</param>
        /// <param name="strcfurl">网络分析最短路径服务地址</param>
        /// <param name="strqurl">路线查询地址</param>
        /// <param name="dblen">事故影响区域范围</param>
        public void Route(List<Graphic> lstrescue, List<Graphic> lstrecipient, List<Graphic> lstsafety, Graphic graleak, MapPoint mpaccident, Map mp, string strgurl, string strnurl, string strcfurl, string strqurl, double dblen)
        {
            lst_Rescue = lstrescue;
            lst_Recipient = lstrecipient;
            lst_Safety = lstsafety;
            Mp_Accident = mpaccident;
            map = mp;
            dbleaklen = dblen;
            strgeourl = strgurl;
            strneturl = strnurl;
            strqueryurl = strqurl;
            strclosestfacilityurl = strcfurl;

            gra_Leak = graleak;
            gra_Leak.Geometry.SpatialReference = map.SpatialReference;

            lst_ReturnRescue = new List<Graphic>();
            lst_ReturnEscape = new List<Graphic>();
            lst_Edge = new List<Graphic>();
            lst_EscapeIn = new List<Graphic>();
            lst_EscapeOut = new List<Graphic>();
            lst_new = new List<List<Graphic>>();

            clsescaperoute = new clsEscapeRoute();
            clsescaperoute.EscapeRouteFaildEvent += new EscapeRouteDelegate(clsescaperoute_EscapeRouteFaildEvent);
            clsescaperoute.EscapeRouteEvent += new EscapeRouteDelegate(clsescaperoute_EscapeRouteEvent);
            clsrescueroute = new clsRescueRoute();
            clsrescueroute.RescueRouteFaildEvent += new RescueRouteDelegate(clsrescueroute_RescueRouteFaildEvent);
            clsrescueroute.RescueRouteEvent += new RescueRouteDelegate(clsrescueroute_RescueRouteEvent);
            List<Graphic> lstEnd = new List<Graphic>();
            lstEnd.Add(new Graphic() { Geometry = mpaccident });
            if (PFApp.MapServerType == enumMapServerType.Baidu)
            {
                strquerybaiduurl = strgurl;
                clsrescueroute.RouteInit(map, lst_Rescue, lstEnd, null, strgurl, false);
                
            }
            else
                clsrescueroute.RouteInit(map, lst_Rescue, lstEnd, null, strneturl, strgeourl, false);
        }

        /// <summary>
        /// 救援路线分析成功
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clsrescueroute_RescueRouteEvent(object sender, EventArgs e)
        {
            List<Graphic> lst = (sender as clsRescueRoute).lst_Result;
            for (int i = 0; i < lst.Count; i++)
            {
                PointCollection pc = (lst[i].Geometry as Polyline).Paths[0];
                pc.Add(Mp_Accident);
                Polyline polyline = new Polyline();
                polyline.Paths.Add(pc);
                polyline.SpatialReference = map.SpatialReference;
                Graphic gra = new Graphic() { Geometry = polyline };
                lst_ReturnRescue.Add(gra);
            }
            if (PFApp.MapServerType == enumMapServerType.Baidu)
            {
                //逃生时不考虑障碍，直接从事故点出发
                List<Graphic> lstEnd = new List<Graphic>();
                lstEnd.Add(new Graphic() { Geometry = Mp_Accident });
                clsescaperoute.RouteInit(map, lstEnd, lst_Safety, null, strquerybaiduurl, false);
            }
            else
            {
                count = 0;
                if (lst_Recipient[count].Geometry as MapPoint != Mp_Accident)
                {
                    clscirclesafepoint = new clsCircleSafePoint();
                    clscirclesafepoint.SafePointEvent += new CircleSafePointDelegate(clssafepoint_SafePointEvent);
                    clscirclesafepoint.SafePointFaildEvent += new CircleSafePointDelegate(clssafepoint_SafePointFaildEvent);
                    clscirclesafepoint.SafeRoute(map, lst_Recipient[count].Geometry as MapPoint, Mp_Accident, strgeourl, strclosestfacilityurl, strqueryurl, dbleaklen);
                }
                else
                {
                    List<Graphic> lstStart = new List<Graphic>();
                    lstStart.Add(lst_Recipient[count]);
                    clsescaperoute.RouteInit(map, lstStart, lst_Safety, null, strneturl, strgeourl, true);
                }
            }
        }

        /// <summary>
        /// 范围区域内最短路径计算成功
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clssafepoint_SafePointEvent(object sender, EventArgs e)
        {
            List<Graphic> lst = (sender as clsCircleSafePoint).Return_lst;
            lst_EscapeIn.Add(lst[0]);
            List<Graphic> lstStart = new List<Graphic>();
            lstStart.Add(new Graphic() { Geometry = (lst[0].Geometry as Polyline).Paths[0][(lst[0].Geometry as Polyline).Paths[0].Count - 1] });
            List<Graphic> lstBar = new List<Graphic>();
            lstBar.Add(gra_Leak);
            clsescaperoute.RouteInit(map, lstStart, lst_Safety, lstBar, strneturl, strgeourl, true);
        }

        /// <summary>
        /// 逃生路径计算成功
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clsescaperoute_EscapeRouteEvent(object sender, EventArgs e)
        {
            List<Graphic> lst = (sender as clsEscapeRoute).lst_Result;
            if (lst.Count != 0)
            {
                for (int i = 0; i < lst.Count; i++)
                {
                    Graphic gra = lst[i];
                    lst_EscapeOut.Add(gra);
                }
                lst_new.Add((sender as clsEscapeRoute).lst_Point);
            }
            else
            {
                PointCollection pc = new PointCollection();
                pc.Add(lst_Edge[count].Geometry as MapPoint);
                pc.Add(lst_Safety[count].Geometry as MapPoint);
                Polyline polyline = new Polyline();
                polyline.Paths.Add(pc);
                Graphic gra = new Graphic() { Geometry = polyline };
                lst_EscapeOut.Add(gra);
            }
            count = count + 1;
            if (count < lst_Recipient.Count)
            {
                clscirclesafepoint.SafeRoute(map, lst_Recipient[count].Geometry as MapPoint, Mp_Accident, strgeourl, strclosestfacilityurl, strqueryurl, dbleaklen);
            }
            else
            {
                Union();
            }
        }

        /// <summary>
        /// 逃离区域内的线和外的线合并
        /// </summary>
        void Union()
        {
            for (int i = 0; i < lst_EscapeIn.Count; i++)
            {
                Graphic gra = UnionLine(lst_EscapeIn[i], lst_EscapeOut[i]);
                lst_ReturnEscape.Add(gra);
            }
            if (lst_EscapeIn.Count == 0)
            {
                lst_ReturnEscape = lst_EscapeOut;
            }
            ProcessAction(this, EventArgs.Empty);
        }

        Graphic UnionLine(Graphic gra1, Graphic gra2)
        {
            if (gra2.Geometry == null && gra1.Geometry != null)
            {
                return gra1;
            }
            else if (gra2.Geometry != null && gra1.Geometry == null)
            {
                return gra2;
            }
            else if (gra2.Geometry == null && gra1.Geometry == null)
            {
                return null;
            }
            PointCollection pc1 = (gra1.Geometry as Polyline).Paths[0];
            PointCollection pc2 = (gra2.Geometry as Polyline).Paths[0];
            bool isfirst = true;
            for (int i = 0; i < pc1.Count; i++)
            {
                if (pc2.Contains(pc1[i]))
                {
                    if (isfirst != true)
                    {
                        pc2.Remove(pc1[i]);
                        pc1.Remove(pc1[i]);
                        i = i - 1;
                    }
                    isfirst = false;
                }
            }
            Polyline polyline = new Polyline();
            polyline.Paths.Add(pc1);
            polyline.Paths.Add(pc2);
            polyline.SpatialReference = map.SpatialReference;
            Graphic gra = new Graphic() { Geometry = polyline };
            return gra;
        }

        /// <summary>
        /// 范围区域内最短路径计算失败
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clssafepoint_SafePointFaildEvent(object sender, EventArgs e)
        {
            RouteAnalysisFaildEvent(sender, e);
        }

        /// <summary>
        /// 最短逃生路径计算失败
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clsescaperoute_EscapeRouteFaildEvent(object sender, EventArgs e)
        {
            RouteAnalysisFaildEvent(sender, e);
        }

        /// <summary>
        /// 救援路径分析出错
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clsrescueroute_RescueRouteFaildEvent(object sender, EventArgs e)
        {
            RouteAnalysisFaildEvent(sender, e);
        }

        /// <summary>
        /// 分析出错
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void geometryservice_Failed(object sender, TaskFailedEventArgs e)
        {
            RouteAnalysisFaildEvent(sender, e);
        }

    }
}
