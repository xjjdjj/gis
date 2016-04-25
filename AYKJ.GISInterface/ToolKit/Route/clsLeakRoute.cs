using System;
using System.Collections.Generic;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Tasks;

namespace AYKJ.GISExtension
{
    /// <summary>
    /// 委托
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void LeakRouteDelegate(object sender, EventArgs e);

    public class clsLeakRoute
    {
        //定义事件
        public event LeakRouteDelegate LeakRouteEvent;
        public event LeakRouteDelegate LeakRouteFaildEvent;
        //GeometryService
        GeometryService geometryservice;
        //受体点
        MapPoint Mp_Recipient;
        //返回的最近点
        public MapPoint Return_MapPoint;

        void ProcessAction(object sender, EventArgs e)
        {
            if (LeakRouteEvent == null)
                LeakRouteEvent += new LeakRouteDelegate(LeakRouteErrorEvent);
            LeakRouteEvent(sender, e);
        }

        /// <summary>
        /// 如果没有自己指定关联方法，将会调用该方法抛出错误
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void LeakRouteErrorEvent(object sender, EventArgs e)
        {
            LeakRouteFaildEvent(sender, e);
        }

        /// <summary>
        /// 计算最近边缘点
        /// </summary>
        /// <param name="mprecipient">受体点</param>
        /// <param name="polygonleak">事故边缘线</param>
        /// <param name="mpleak">事故点</param>
        /// <param name="strurl">空间信息服务路径</param>
        public void LeakRoute(MapPoint mprecipient, Polyline polygonleak, MapPoint mpleak, string strurl)
        {
            Mp_Recipient = mprecipient;
            //countlen = 0;
            //Lst_End = new List<Graphic>();

            Polyline polyline = new Polyline();
            PointCollection pc = new PointCollection();
            pc.Add(mprecipient);
            pc.Add(mpleak);
            polyline.Paths.Add(pc);
            polyline.SpatialReference = mprecipient.SpatialReference;

            List<Polyline> lst = new List<Polyline>();
            lst.Add(polyline);

            Polyline tmppl = new Polyline();
            tmppl.Paths.Add(polygonleak.Paths[0]);
            tmppl.SpatialReference = mprecipient.SpatialReference;

            geometryservice = new GeometryService(strurl);
            geometryservice.Failed += new EventHandler<TaskFailedEventArgs>(geometryservice_Failed);
            geometryservice.TrimExtendCompleted += new EventHandler<GraphicsEventArgs>(geometryservice_TrimExtendCompleted);
            geometryservice.LengthsCompleted += new EventHandler<LengthsEventArgs>(geometryservice_LengthsCompleted);
            geometryservice.TrimExtendAsync(lst, tmppl, CurveExtension.DefaultCurveExtension);
        }

        /// <summary>
        /// 延长线成功
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void geometryservice_TrimExtendCompleted(object sender, GraphicsEventArgs e)
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
            geometryservice.LengthsAsync(lst, LinearUnit.Meter, true, gra);
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
                Return_MapPoint = ((e.UserState as Graphic).Geometry as Polyline).Paths[0][((e.UserState as Graphic).Geometry as Polyline).Paths[0].Count -1];
            }
            else
            {
                Return_MapPoint = ((e.UserState as Graphic).Geometry as Polyline).Paths[0][0];
            }
            ProcessAction(this, EventArgs.Empty);
        }

        /// <summary>
        /// 计算发生错误
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void geometryservice_Failed(object sender, TaskFailedEventArgs e)
        {
            LeakRouteFaildEvent(sender, e);
        }

    }
}
