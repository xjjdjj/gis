#region << 版 本 注 释 >>
/*
 * ========================================================================
 * Copyright(c)  陈锋, All Rights Reserved.
 * ========================================================================
 * CLR版本：       4.0.30319.261
 * 类 名 称：       MainPage
 * 机器名称：       GIS-FLYH
 * 命名空间：       AYKJ.GISInterface
 * 文 件 名：       MainPage
 * 创建时间：       2012/7/20 10:24:16
 * 作    者：       陈锋
 * 功能说明：       安元科技GIS扩展集合
 * 修改时间：
 * 修 改 人：
 * ========================================================================
*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Linq;
using AYKJ.GISDevelop.Platform;
using AYKJ.GISDevelop.Platform.Part;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Toolkit;
using System.Windows.Controls.Primitives;
using AYKJ.GISDevelop.Platform.ToolKit;
using System.ServiceModel;
using System.IO;
using AYKJ.GISExtension;
using ESRI.ArcGIS.Client.Symbols;
using System.Windows.Media;
using System.Threading;
using ESRI.ArcGIS.Client.Tasks;
using AYKJ.GISInterface.Control.AdvAPP;

namespace AYKJ.GISInterface
{
    public partial class MainPage : UserControl, IWidgets
    {
        WaitAnimationWindow waitanimationwindow;

        #region 子页面定义
        LeakModelPage leakmodelpage;
        AYKJ.GPS.GPSPage gpspage;
        StatisticPage statisticpage;
        AccidentSimulation accidentsimulation;
        EvacuationRoute evacuationroute;
        AutoSearch autosearch;
        AutoAllSearch autoallsearch;
        AddPointWithLoc addpointwithloc;
        DataTools datatools;
        bool IsStartGPS = false;
        bool IsTrackPlayBack = false;
        #endregion

        #region zc打点定位查询接口变量定义
        //加载数据服务
        private AykjDataServiceInner.AykjDataClient AykjClientInner;
        private AykjDataServiceInner.AykjDataClient AykjClientInnerDraw;
        //配置文件
        XElement xele;
        //20120823
        string m_wxyid;
        string m_wxytype;
        string m_dwdm;
        string m_idkey;
        string m_remark;
        string m_x;
        string m_y;
        string m_street = "";//江宁智慧安监，默认为空字符串。
        string m_enttype = "";//江宁智慧安监，默认为空字符串。
        //Image控件列表
        private static List<Image> listThisImg;
        //定时器
        static DispatcherTimer timer_m = null;
        //Map
        Map mainmap;
        string strImg;
        List<InfoWindow> listInfoWin;

        //接口页面(20120912)
        bool mLink;//true时，表示此时是由业务系统调用时的获取坐标
        //需要异步查询坐标，定义全局变量
        string iAction;
        List<object> iListObj;
        string iParms;


        //20121010
        Dictionary<string, GraphicsLayer> DictIn_layer;
        Dictionary<string, string> DictIn_EnCn;
        //打点操作
        private Draw MyThisDrawZoom;
        //高亮层
        public GraphicsLayer selectHigh_GraLayer;
        #endregion
        
        #region 查询功能
        //空间查询方法
        clsRangeQuery clsrangequery;
        GeometryService geoService;
        DrawCircle drc;
        //绘制图形GraphicLayer
        public static GraphicsLayer Draw_GraLayer;
        public static ESRI.ArcGIS.Client.Geometry.Polygon CirlePolygon;
        //所有的专题数据
        Dictionary<string, GraphicsLayer> Dict_Data;
        //定义一个空间查询服务的地址
        string strGeometryurl;
        string[] selectType;
        string queryparam = "";
        bool querywait = true;
        Graphic pass2ExposeGra;
        DataQueryPageExpose dataQueryPageEx;

        public string rtnQueryResultsStr = "";

        //20150617zc:
        private List<cls_PmtService> list_PmtService;
        private List<Polygon> list_polygon;
        #endregion
        #region ESRI样式
        public SimpleFillSymbol DrawFillSymbol = new SimpleFillSymbol()
        {
            Fill = new SolidColorBrush(Color.FromArgb(30, 255, 0, 0)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)),
            BorderThickness = 2
        };
        //20150617:企业平面图中的点定位后，高亮该企业的范围
        public SimpleFillSymbol PmtFillSymbol = new SimpleFillSymbol()
        {
            Fill = new SolidColorBrush(Color.FromArgb(60, 0, 0, 255)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(200, 0, 0, 255)),
            BorderThickness = 1
        };
        #endregion

        public MainPage()
        {
            InitializeComponent();

            #region 实例化各个子页面
            leakmodelpage = new LeakModelPage();
            gpspage = new GPS.GPSPage();
            statisticpage = new StatisticPage();
            accidentsimulation = new AccidentSimulation();
            #endregion

            if (mainmap == null)
                mainmap = (Application.Current as IApp).MainMap;

            #region//数据处理
            xele = PFApp.Extent;
            var dataServices = (from item in xele.Element("DataServices").Elements("DataService")
                                where item.Attribute("Name").Value == "专题数据"
                                select new
                                {
                                    Type = item.Attribute("Type").Value,
                                    Url = item.Attribute("Url").Value,
                                }).ToList();
            if (AykjClientInner == null)
                AykjClientInner = new AykjDataServiceInner.AykjDataClient(new BasicHttpBinding(), new EndpointAddress(dataServices[0].Url));
            if (listThisImg == null)
            {
                listThisImg = new List<Image>();
            }
            //OpenTimer();
            mLink = false;
            #endregion
        }

        #region 定时器触发专题点位图片加载事件
        /// <summary>
        /// 打开计时器
        /// </summary>
        public void OpenTimer()
        {
            if (timer_m == null)
            {
                timer_m = new DispatcherTimer();
                timer_m.Interval = TimeSpan.FromSeconds(2);
                timer_m.Tick += new EventHandler(timer_m_Chart_Tick);
            }
            if (!timer_m.IsEnabled)
                timer_m.Start();
        }

        /// <summary>
        /// 触发定时器事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void timer_m_Chart_Tick(object sender, EventArgs e)
        {
            timer_m.Stop();
            if ((Application.Current as IApp).syslistthisimg != null && (Application.Current as IApp).syslistthisimg.Count > 0)
            {
                foreach (Image i in (Application.Current as IApp).syslistthisimg)
                {
                    i.MouseLeftButtonUp -= iM_MouseLeftButtonUp;
                    i.MouseLeftButtonUp += new MouseButtonEventHandler(iM_MouseLeftButtonUp);
                }
            }
            timer_m.Start();
            return;
        }

        void iM_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            string type = (Application.Current as IApp).strImageType;
            strImg = (sender as Image).Tag.ToString();
            m_wxytype = strImg.Split('|')[0];
            m_wxyid = strImg.Split('|')[1];
            m_dwdm = strImg.Split('|')[2];
            m_remark = strImg.Split('|')[3];
            switch (type)
            {
                case "defaultClick":
                    AykjClientInner.getDataLocationCompleted -= AykjClientInner_getDataLocationCompleted;
                    AykjClientInner.getDataLocationCompleted += new EventHandler<AykjDataServiceInner.getDataLocationCompletedEventArgs>(AykjClientInner_getDataLocationCompleted);
                    AykjClientInner.getDataLocationAsync(m_wxyid, m_wxytype, m_dwdm);
                    break;
            }
        }

        #endregion

        #region 实现平台接口
        public string LinkReturnGisPlatform(string mark, string s)
        {
            return "";
        }

        public string LinkReturnGisPlatform(string mark, object obj1,object obj2)
        {
            return "";
        }

        /// <summary>
        /// 此处的类定义应和业务系统中保持一致
        /// </summary>
        public class clsWxy
        {
            public string dwdm { get; set; }
            public string wxyid { get; set; }
            public string wxytype { get; set; }
            public string remark { get; set; }
            public string x { get; set; }
            public string y { get; set; }
        }

        /// <summary>
        /// 20150305:江宁智慧安监专用
        /// </summary>
        public class clsWxy_Jn
        {
            public string dwdm { get; set; }
            public string wxyid { get; set; }
            public string wxytype { get; set; }
            public string remark { get; set; }
            public string x { get; set; }
            public string y { get; set; }
            public string street { get; set; }
            public string enttype { get; set; }
        }

        /// <summary>
        /// 调用接口的方法
        /// </summary>
        /// <param name="oAction">操作类型</param>
        /// <param name="oStr">单独string型参数</param>
        /// <param name="oArr">单独array型参数</param>
        /// <param name="oCls">单独class型参数</param>
        /// <param name="oArrStr">string型参数的array</param>
        /// <param name="oArrArr">array型参数的array</param>
        /// <param name="oArrCls">class型参数的array</param>
        /// <returns></returns>
        public string LinkFromGiPlatform(string oAction, string oStr, object oArr, object oCls, object[] oArrStr, object[] oArrArr, object[] oArrCls)
        {
            iAction = oAction;

            switch (iAction)
            {
                #region 
                case "打开重大危险源平面图":
                    string[] zdwxypmts=null;
                    if (oArr != null)
                    {
                        zdwxypmts = ((ScriptObject)(oArr)).ConvertTo<string[]>();
                    }
                    ShowPmtWindow(oStr, zdwxypmts);
                    break;
                #endregion

                #region 登录权限控制接口，传入角色信息：街道名/安监局
                case "江宁登录":
                    InitSystemRole(oStr);
                    break;
                #endregion

                #region 标绘功能 支持百度地图
                case "电子标绘":
                    plotting();
                    break;
                #endregion

                #region 数据查询功能 支持百度地图
                case "半径查询(画圆)":
                    try
                    {
                        string[] querytype=null;
                        if (oArr != null)
                        {
                            querytype = ((ScriptObject)(oArr)).ConvertTo<string[]>();
                        }

                        string radius = oStr;
                        clsWxy iAry = ((ScriptObject)(oCls)).ConvertTo<clsWxy>();
                        if (iAry != null)
                        {
                            this.DataQuery(querytype,radius, iAry.wxyid, iAry.dwdm, iAry.remark, iAry.wxytype,1);
                        }
                        else
                        {
                            Message.Show("转换参数为空，请检查传入参数是否正确");
                            return "error";
                        }
                    }
                    catch (Exception e)
                    {
                        Message.Show("转换参数失败，请检查传入参数是否正确");
                        return "error";
                    }
                    break;
                case "半径查询(不画圆)":
                    try
                    {
                        string[] querytype=null;
                        if (oArr != null)
                        {
                            querytype = ((ScriptObject)(oArr)).ConvertTo<string[]>();
                        }

                        string radius = oStr;
                        clsWxy iAry = ((ScriptObject)(oCls)).ConvertTo<clsWxy>();
                        if (iAry != null)
                        {
                            this.DataQuery(querytype,radius, iAry.wxyid, iAry.dwdm, iAry.remark, iAry.wxytype,0);
                        }
                        else
                        {
                            Message.Show("转换参数为空，请检查传入参数是否正确");
                            return "error";
                        }
                    }
                    catch (Exception e)
                    {
                        Message.Show("转换参数失败，请检查传入参数是否正确");
                        return "error";
                    }
                    break;
                case "半径查询":
                    try
                    {
                        clsWxy iAry0 = ((ScriptObject)(oCls)).ConvertTo<clsWxy>();
                        if (iAry0 != null)
                        {
                            //wxyid = 点选|
                            //dwdm = 细节
                            this.DataQuery(iAry0.wxyid, iAry0.dwdm, iAry0.wxytype);
                        }
                        else
                        {
                            Message.Show("转换参数为空，请检查传入参数是否正确");
                            return "error";
                        }
                    }
                    catch (Exception e)
                    {
                        Message.Show("转换参数失败，请检查传入参数是否正确");
                        return "error";
                    }
                    break;
                #endregion

                #region 数据管理 支持百度地图
                case "删除数据":
                    //根据实际的方法，整理参数数据
                    try
                    {
                        clsWxy iAry1 = ((ScriptObject)(oCls)).ConvertTo<clsWxy>();
                        if (iAry1 != null)
                        {
                            iParms = iAry1.wxyid + "|" + iAry1.wxytype + "|" + iAry1.dwdm + "|" + iAry1.remark;
                            linkDeleteData(iParms);
                        }
                        else
                        {
                            Message.Show("转换参数为空，请检查传入参数是否正确");
                            return "error";
                        }
                    }
                    catch (Exception e)
                    {
                        Message.Show("转换参数失败，请检查传入参数是否正确");
                        return "error";
                    }
                    break;
                case "GPS打点"://20150325:江宁智慧安监要求直接从业务系统传入gps经纬度坐标进行打点，无需地图操作。
                    //根据实际的方法，整理参数数据
                    try
                    {
                        clsWxy_Jn iAry1 = ((ScriptObject)(oCls)).ConvertTo<clsWxy_Jn>();
                        if (iAry1 != null)
                        {
                            iParms = iAry1.wxyid + "|" + iAry1.wxytype + "|" + iAry1.dwdm + "|" + iAry1.remark + "|" + iAry1.x + "|" + iAry1.y + "|" + iAry1.street + "|" + iAry1.enttype;

                            link_addData_WithLocation(iParms);
                        }
                        else
                        {
                            Message.Show("转换参数为空，请检查传入参数是否正确");
                            return "error";
                        }
                    }
                    catch (Exception e)
                    {
                        Message.Show("转换参数失败，请检查传入参数是否正确");
                        return "error";
                    }

                    break;
                    //case "打点"://20150327:整体注释掉，江宁系统不需要。
                    ////根据实际的方法，整理参数数据
                    //try
                    //{
                    //    clsWxy iAry1 = ((ScriptObject)(oCls)).ConvertTo<clsWxy>();
                    //    if (iAry1 != null)
                    //    {
                    //        iParms = iAry1.wxyid + "|" + iAry1.wxytype + "|" + iAry1.dwdm + "|" + iAry1.remark + "|" + iAry1.x + "|" + iAry1.y;

                    //        linkAddDataWithLocation(iParms);
                    //    }
                    //    else
                    //    {
                    //        Message.Show("转换参数为空，请检查传入参数是否正确");
                    //        return "error";
                    //    }
                    //}
                    //catch (Exception e)
                    //{
                    //    Message.Show("转换参数失败，请检查传入参数是否正确");
                    //    return "error";
                    //}

                    //break;
                case "无坐标打点":
                    //根据实际的方法，整理参数数据
                    try
                    {
                        clsWxy_Jn iAry1 = ((ScriptObject)(oCls)).ConvertTo<clsWxy_Jn>();
                        if (iAry1 != null)
                        {
                            iParms = iAry1.wxyid + "|" + iAry1.wxytype + "|" + iAry1.dwdm + "|" + iAry1.remark + "|" + iAry1.street + "|" + iAry1.enttype;

                            //dataqueryclickpoint111.linkAddDataWithoutLocation(iParms);

                            linkAddDataWithoutLocation(iParms);
                        }
                        else
                        {
                            Message.Show("转换参数为空，请检查传入参数是否正确");
                            return "error";
                        }
                    }
                    catch (Exception e)
                    {
                        Message.Show("转换参数失败，请检查传入参数是否正确");
                        return "error";
                    }

                    break;
                case "定位":
                    //根据实际的方法，整理参数数据
                    try
                    {
                        clsWxy iAry1 = ((ScriptObject)(oCls)).ConvertTo<clsWxy>();
                        if (iAry1 != null)
                        {
                            iParms = iAry1.wxyid + "|" + iAry1.wxytype + "|" + iAry1.dwdm + "|" + iAry1.remark;

                            mLink = true;
                            linkGetLocation(iParms);
                        }
                        else
                        {
                            Message.Show("转换参数为空，请检查传入参数是否正确");
                            return "error";
                        }
                    }
                    catch (Exception e)
                    {
                        Message.Show("转换参数失败，请检查传入参数是否正确");
                        return "error";
                    }

                    break;
                #endregion

                #region 简单模拟 支持百度地图
                case "爆炸模拟":
                    if (leakmodelpage == null)
                    {
                        leakmodelpage = new LeakModelPage();
                    }

                    //根据实际的方法，整理参数数据
                    try
                    {
                        double[] iArr = ((ScriptObject)(oArr)).ConvertTo<double[]>();
                        if (iArr != null)
                        {
                            double iDeath = iArr[0];
                            double iSeriouslyInjured = iArr[1];
                            double iMinorInjuries = iArr[2];
                            double iSafe = iArr[3];

                            string remark = oStr;

                            leakmodelpage.linkBtn_Explosion_Click(iDeath, iSeriouslyInjured, iMinorInjuries, iSafe, remark);
                        }
                        else
                        {
                            Message.Show("转换参数为空，请检查传入参数是否正确");
                            return "error";
                        }
                    }
                    catch (Exception e)
                    {
                        Message.Show("转换参数失败，请检查传入参数是否正确");
                        return "error";
                    }

                    break;
                case "火灾模拟":
                    if (leakmodelpage == null)
                    {
                        leakmodelpage = new LeakModelPage();
                    }

                    //根据实际的方法，整理参数数据
                    try
                    {
                        double[] iArr = ((ScriptObject)(oArr)).ConvertTo<double[]>();
                        if (iArr != null)
                        {

                            double iDeath = iArr[0];
                            double iSeriouslyInjured = iArr[1];
                            double iMinorInjuries = iArr[2];
                            double iSafe = iArr[3];

                            string remark = oStr;

                            leakmodelpage.linkBtn_Fire_Click(iDeath, iSeriouslyInjured, iMinorInjuries, iSafe, remark);

                        }
                        else
                        {
                            Message.Show("转换参数为空，请检查传入参数是否正确");
                            return "error";
                        }
                    }
                    catch (Exception e)
                    {
                        Message.Show("转换参数失败，请检查传入参数是否正确");
                        return "error";
                    }

                    break;
                case "瞬时泄漏模拟":
                    try
                    {
                        object[] iObjArysize = (object[])oArrArr[0];
                        object[] iObjAryradius = (object[])oArrArr[1];

                        double[] iArysize = new double[iObjArysize.Length];
                        for (int i = 0; i < iObjArysize.Length; i++)
                        {
                            iArysize[i] = Double.Parse(iObjArysize[i].ToString());
                        }

                        double[] iAryradius = new double[iObjAryradius.Length];
                        for (int j = 0; j < iObjAryradius.Length; j++)
                        {
                            iAryradius[j] = Double.Parse(iObjAryradius[j].ToString());
                        }

                        //通过传入参数确定泄漏点坐标
                        var buffer = System.Text.Encoding.UTF8.GetBytes(oStr);
                        var ms = new MemoryStream(buffer);
                        var jsonObject = System.Json.JsonObject.Load(ms) as System.Json.JsonObject;
                        string tmp_wxyid = jsonObject["wxyid"].ToString().Replace("\"", "");
                        string tmp_wxytype = jsonObject["wxytype"].ToString().Replace("\"", "");
                        string tmp_dwdm = jsonObject["dwdm"].ToString().Replace("\"", "");
                        string statag = tmp_wxytype + "|" + tmp_wxyid + "|" + tmp_dwdm;
                        var Ss_LeakSs = (from item in (Application.Current as IApp).lstThematic
                                         where item.Attributes["StaTag"].ToString().Contains(statag)
                                         select new
                                         {
                                             Name = item.Attributes["remark"],
                                             Graphic = item,
                                         }).ToList();
                        if (Ss_LeakSs.Count == 0)
                        {
                            Message.Show("没有查到该企业单位");
                        }
                        else
                        {
                            leakmodelpage.ClearGraphic();
                            leakmodelpage.CallInstantaneousLeak(iArysize, iAryradius, Ss_LeakSs[0].Graphic.Geometry.Extent.XMax, Ss_LeakSs[0].Graphic.Geometry.Extent.YMax);

                            //20120904:调用扩展功能，显示问题，暂不用
                            //if (accidentsimulation == null)
                            //{
                            //    accidentsimulation = new AccidentSimulation();
                            //}
                            //accidentsimulation.linkCmb_type_SelectInstantaneousLeak(iParms);
                        }
                    }
                    catch (Exception e)
                    {
                        Message.Show("转换参数失败，请检查传入参数是否正确");
                        return "error";
                    }

                    break;
                case "连续泄漏模拟":
                    try
                    {
                        object[] iObjAryCombustion = (object[])oArrArr[0];
                        object[] iObjAryPoisoning = (object[])oArrArr[1];

                        double[] iAryCombustion = new double[iObjAryCombustion.Length];
                        for (int i = 0; i < iObjAryCombustion.Length; i++)
                        {
                            iAryCombustion[i] = Double.Parse(iObjAryCombustion[i].ToString());
                        }

                        double[] iAryPoisoning = new double[iObjAryPoisoning.Length];
                        for (int j = 0; j < iObjAryPoisoning.Length; j++)
                        {
                            iAryPoisoning[j] = Double.Parse(iObjAryPoisoning[j].ToString());
                        }

                        //解析传入的string型参数
                        //燃烧区步长
                        double c_dbCombustionStep = Double.Parse(oArrStr[0].ToString());

                        //中毒区步长
                        double c_dbPoisoningStep = Double.Parse(oArrStr[1].ToString());

                        //燃烧偏移量
                        double c_dbCombustionStart = Double.Parse(oArrStr[2].ToString());

                        //燃烧区面积
                        double c_CombustionArea = Double.Parse(oArrStr[3].ToString());

                        //中毒偏移量
                        double c_dbPoisoningStart = Double.Parse(oArrStr[4].ToString());

                        //中毒区面积
                        double c_PoisoningArea = Double.Parse(oArrStr[5].ToString());

                        //为0则展示燃烧和中毒，1为燃烧，2为中毒
                        string c_strtype = oArrStr[6].ToString();



                        //通过传入参数确定泄漏点坐标
                        var buffer = System.Text.Encoding.UTF8.GetBytes(oStr);
                        var ms = new MemoryStream(buffer);
                        var jsonObject = System.Json.JsonObject.Load(ms) as System.Json.JsonObject;
                        string tmp_wxyid = jsonObject["wxyid"].ToString().Replace("\"", "");
                        string tmp_wxytype = jsonObject["wxytype"].ToString().Replace("\"", "");
                        string tmp_dwdm = jsonObject["dwdm"].ToString().Replace("\"", "");
                        string statag = tmp_wxytype + "|" + tmp_wxyid + "|" + tmp_dwdm;
                        var Lx_LeakLx = (from item in (Application.Current as IApp).lstThematic
                                         where item.Attributes["StaTag"].ToString().Contains(statag)
                                         select new
                                         {
                                             Name = item.Attributes["remark"],
                                             Graphic = item,
                                         }).ToList();
                        //20120919：(BUG_CLEAR)
                        if (Lx_LeakLx.Count == 0)
                        {
                            Message.Show("没有查到该企业单位");
                        }
                        else
                        {
                            leakmodelpage.ClearGraphic();
                            leakmodelpage.CallContinuousLeak(iAryCombustion, iAryPoisoning, Lx_LeakLx[0].Graphic.Geometry.Extent.XMax, Lx_LeakLx[0].Graphic.Geometry.Extent.YMax, c_dbCombustionStep, c_dbPoisoningStep, c_dbCombustionStart, c_CombustionArea, c_dbPoisoningStart, c_PoisoningArea, c_strtype);

                            //20120904:调用扩展功能，显示问题，暂不用
                            //if (accidentsimulation == null)
                            //{
                            //    accidentsimulation = new AccidentSimulation();
                            //}
                            //accidentsimulation.linkCmb_type_SelectContinuousLeak(iParms);
                        }

                    }
                    catch (Exception e)
                    {
                        Message.Show("调用失败，请检查传入参数是否正确");
                        return "error";
                    }


                    break;
                #endregion

                #region 路径分析 百度地图简单支持
                case "最短逃生路径":
                    try
                    {
                        List<Graphic> lstStart = new List<Graphic>();

                        var buffer = System.Text.Encoding.UTF8.GetBytes(oStr);
                        var ms = new MemoryStream(buffer);
                        var jsonObject = System.Json.JsonObject.Load(ms) as System.Json.JsonObject;
                        string tmp_wxyid = jsonObject["wxyid"].ToString().Replace("\"", "");
                        string tmp_wxytype = jsonObject["wxytype"].ToString().Replace("\"", "");
                        string tmp_dwdm = jsonObject["dwdm"].ToString().Replace("\"", "");
                        string escapeRoute_remark = tmp_wxytype + "|" + tmp_wxyid + "|" + tmp_dwdm;

                        var escapeRoute_start = (from item in (Application.Current as IApp).lstThematic
                                                 where item.Attributes["StaTag"].ToString().Contains(escapeRoute_remark)
                                                 select new
                                                 {
                                                     id = item.Attributes["StaTag"].ToString().Split('|')[1],
                                                     Name = item.Attributes["remark"],
                                                     Graphic = item,
                                                 }).ToList();
                        if (escapeRoute_start.Count == 0)
                        {
                            Message.Show("没有查到指定的起始点位:" + escapeRoute_remark);
                            return "error";
                        }
                        else
                        {
                            lstStart.Add(new Graphic()
                            {
                                Geometry = new MapPoint()
                                {
                                    X = escapeRoute_start[0].Graphic.Geometry.Extent.XMax,
                                    Y = escapeRoute_start[0].Graphic.Geometry.Extent.YMax,
                                    SpatialReference = mainmap.SpatialReference
                                }
                            });
                            //20150109:将该点的信息存入graphic
                            lstStart[lstStart.Count - 1].Attributes.Add("info", escapeRoute_remark);
                        }

                        List<Graphic> lstEnd = new List<Graphic>();
                        string[] iArr_end = ((ScriptObject)(oArr)).ConvertTo<string[]>();
                        if (iArr_end != null)
                        {
                            foreach (string s in iArr_end)
                            {
                                buffer = System.Text.Encoding.UTF8.GetBytes(s);
                                ms = new MemoryStream(buffer);
                                jsonObject = System.Json.JsonObject.Load(ms) as System.Json.JsonObject;
                                tmp_wxyid = jsonObject["wxyid"].ToString().Replace("\"", "");
                                tmp_wxytype = jsonObject["wxytype"].ToString().Replace("\"", "");
                                tmp_dwdm = jsonObject["dwdm"].ToString().Replace("\"", "");
                                string str_escapeRoute = tmp_wxytype + "|" + tmp_wxyid + "|" + tmp_dwdm;

                                var escapeRoute_end = (from item in (Application.Current as IApp).lstThematic
                                                       where item.Attributes["StaTag"].ToString().Contains(str_escapeRoute)
                                                       select new
                                                       {
                                                           id = item.Attributes["StaTag"].ToString().Split('|')[1],
                                                           Name = item.Attributes["remark"],
                                                           Graphic = item,
                                                       }).ToList();
                                if (escapeRoute_end.Count == 0)
                                {
                                    Message.Show("没有查到指定的终点点位:" + s);
                                    return "error";
                                }
                                else
                                {
                                    lstEnd.Add(new Graphic()
                                    {
                                        Geometry = new MapPoint()
                                        {
                                            X = escapeRoute_end[0].Graphic.Geometry.Extent.XMax,
                                            Y = escapeRoute_end[0].Graphic.Geometry.Extent.YMax,
                                            SpatialReference = mainmap.SpatialReference
                                        }
                                    });
                                    //20150109:将该点的信息存入graphic
                                    lstEnd[lstEnd.Count - 1].Attributes.Add("info", str_escapeRoute);
                                }
                            }
                        }

                        leakmodelpage.linkBtn_EscapeRoute_Click(lstStart, lstEnd);

                    }
                    catch (Exception e)
                    {
                        Message.Show("调用失败，请检查传入参数是否正确");
                        return "error";
                    }

                    break;
                case "最短救援路径":
                    try
                    {
                        List<Graphic> lstEnd = new List<Graphic>();
                        var buffer = System.Text.Encoding.UTF8.GetBytes(oStr);
                        var ms = new MemoryStream(buffer);
                        var jsonObject = System.Json.JsonObject.Load(ms) as System.Json.JsonObject;
                        string tmp_wxyid = jsonObject["wxyid"].ToString().Replace("\"", "");
                        string tmp_wxytype = jsonObject["wxytype"].ToString().Replace("\"", "");
                        string tmp_dwdm = jsonObject["dwdm"].ToString().Replace("\"", "");
                        string escapeRoute_remark = tmp_wxytype + "|" + tmp_wxyid + "|" + tmp_dwdm;

                        var escapeRoute_end = (from item in (Application.Current as IApp).lstThematic
                                               where item.Attributes["StaTag"].ToString().Contains(escapeRoute_remark)
                                               select new
                                               {
                                                   id = item.Attributes["StaTag"].ToString().Split('|')[1],
                                                   Name = item.Attributes["remark"],
                                                   Graphic = item,
                                               }).ToList();
                        if (escapeRoute_end.Count == 0)
                        {
                            Message.Show("没有查到指定的终点点位:" + escapeRoute_remark);
                            return "error";
                        }
                        else
                        {
                            lstEnd.Add(new Graphic()
                            {
                                Geometry = new MapPoint()
                                {
                                    X = escapeRoute_end[0].Graphic.Geometry.Extent.XMax,
                                    Y = escapeRoute_end[0].Graphic.Geometry.Extent.YMax,
                                    SpatialReference = mainmap.SpatialReference
                                }
                            });
                            //20150115:将该点的信息存入graphic
                            lstEnd[lstEnd.Count - 1].Attributes.Add("info", escapeRoute_remark);
                        }

                        List<Graphic> lstStart = new List<Graphic>();
                        string[] iArr_start = ((ScriptObject)(oArr)).ConvertTo<string[]>();
                        if (iArr_start != null)
                        {
                            foreach (string s in iArr_start)
                            {
                                buffer = System.Text.Encoding.UTF8.GetBytes(s);
                                ms = new MemoryStream(buffer);
                                jsonObject = System.Json.JsonObject.Load(ms) as System.Json.JsonObject;
                                tmp_wxyid = jsonObject["wxyid"].ToString().Replace("\"", "");
                                tmp_wxytype = jsonObject["wxytype"].ToString().Replace("\"", "");
                                tmp_dwdm = jsonObject["dwdm"].ToString().Replace("\"", "");
                                string str_Route = tmp_wxytype + "|" + tmp_wxyid + "|" + tmp_dwdm;

                                var Route_start = (from item in (Application.Current as IApp).lstThematic
                                                   where item.Attributes["StaTag"].ToString().Contains(str_Route)
                                                   select new
                                                   {
                                                       id = item.Attributes["StaTag"].ToString().Split('|')[1],
                                                       Name = item.Attributes["remark"],
                                                       Graphic = item,
                                                   }).ToList();
                                if (Route_start.Count == 0)
                                {
                                    Message.Show("没有查到指定的起点点位:" + s);
                                    return "error";
                                }
                                else
                                {
                                    lstStart.Add(new Graphic()
                                    {
                                        Geometry = new MapPoint()
                                        {
                                            X = Route_start[0].Graphic.Geometry.Extent.XMax,
                                            Y = Route_start[0].Graphic.Geometry.Extent.YMax,
                                            SpatialReference = mainmap.SpatialReference
                                        }
                                    });
                                    //20150115:将该点的信息存入graphic
                                    lstStart[lstStart.Count - 1].Attributes.Add("info", str_Route);
                                }
                            }
                        }


                        leakmodelpage.linkBtn_RescuePath_Click(lstStart, lstEnd);

                    }
                    catch (Exception e)
                    {
                        Message.Show("调用失败，请检查传入参数是否正确");
                        return "error";
                    }
                    break;
                #endregion

                #region 受体分析模拟 支持百度地图
                case "爆炸模拟(受体分析)":
                    accidentsimulation = new AccidentSimulation();
                    List<object> lstExplosion = new List<object>();
                    double[] ExplosionArr = ((ScriptObject)(oArr)).ConvertTo<double[]>();
                    lstExplosion.Add(ExplosionArr);
                    if (oStr == null)
                    {
                        accidentsimulation.CallDrawAccidentSimulation("爆炸", lstExplosion);
                    }
                    else
                    {
                        accidentsimulation.CallNameAccidentSimulation("爆炸", oStr, lstExplosion);
                    }
                    break;
                case "火灾模拟(受体分析)":
                    accidentsimulation = new AccidentSimulation();
                    List<object> lstFire = new List<object>();
                    double[] FireArr = ((ScriptObject)(oArr)).ConvertTo<double[]>();
                    lstFire.Add(FireArr);
                    if (oStr == null)
                    {
                        accidentsimulation.CallDrawAccidentSimulation("火灾", lstFire);
                    }
                    else
                    {
                        accidentsimulation.CallNameAccidentSimulation("火灾", oStr, lstFire);
                    }
                    break;
                case "瞬时泄漏模拟(受体分析)":
                    accidentsimulation = new AccidentSimulation();
                    List<object> lstInstantaneous = new List<object>();

                    double[] AryInstantaneous1 = new double[((object[])oArrArr[0]).Length];
                    for (int i = 0; i < AryInstantaneous1.Length; i++)
                    {
                        AryInstantaneous1[i] = Double.Parse(((object[])oArrArr[0])[i].ToString());
                    }
                    double[] AryInstantaneous2 = new double[((object[])oArrArr[1]).Length];
                    for (int i = 0; i < AryInstantaneous2.Length; i++)
                    {
                        AryInstantaneous2[i] = Double.Parse(((object[])oArrArr[1])[i].ToString());
                    }
                    lstInstantaneous.Add(AryInstantaneous1);
                    lstInstantaneous.Add(AryInstantaneous2);
                    if (oArr != null)
                    {
                        lstInstantaneous.Add(((ScriptObject)(oArr)).ConvertTo<double[]>());
                    }
                    if (oStr == null)
                    {
                        accidentsimulation.CallDrawAccidentSimulation("瞬时泄漏", lstInstantaneous);
                    }
                    else
                    {
                        accidentsimulation.CallNameAccidentSimulation("瞬时泄漏", oStr, lstInstantaneous);
                    }
                    break;
                case "连续泄漏模拟(受体分析)":
                    accidentsimulation = new AccidentSimulation();
                    List<object> lstContinuous = new List<object>();

                    object[] tmpContinuous1 = (object[])oArrArr[0];
                    object[] tmpContinuous2 = (object[])oArrArr[1];

                    double[] AryContinuous1 = new double[tmpContinuous1.Length];
                    for (int i = 0; i < AryContinuous1.Length; i++)
                    {
                        AryContinuous1[i] = Double.Parse(tmpContinuous1[i].ToString());
                    }

                    double[] AryContinuous2 = new double[tmpContinuous2.Length];
                    for (int j = 0; j < AryContinuous2.Length; j++)
                    {
                        AryContinuous2[j] = Double.Parse(tmpContinuous2[j].ToString());
                    }
                    lstContinuous.Add(AryContinuous1);
                    lstContinuous.Add(AryContinuous2);
                    lstContinuous.Add(oArrStr);
                    if (oStr == null)
                    {
                        accidentsimulation.CallDrawAccidentSimulation("连续泄漏", lstContinuous);
                    }
                    else
                    {
                        accidentsimulation.CallNameAccidentSimulation("连续泄漏", oStr, lstContinuous);
                    }
                    break;
                #endregion

                #region 自动搜索
                case "自动搜索":
                    autosearch = new AutoSearch();
                    if (oStr == null)
                    {
                        autosearch.LoadAutoSearch();
                    }
                    else
                    {
                        autosearch.CallAutoSearch(oStr);
                    }
                    break;
                case "自动搜索(全)":
                    autoallsearch = new AutoAllSearch();
                    if (oStr == null)
                    {
                        autoallsearch.LoadAutoSearch();
                    }
                    else
                    {
                        autoallsearch.CallAutoSearch(oStr);
                    }
                    break;
                #endregion

                #region 图形统计
                case "柱状图统计":
                    statisticpage = new StatisticPage();
                    statisticpage.InitStatistic("柱状图", oStr);
                    break;
                case "饼状图统计":
                    statisticpage = new StatisticPage();
                    statisticpage.InitStatistic("饼状图", oStr);
                    break;
                #endregion

                #region 疏散模拟
                case "爆炸模拟(疏散路径)":
                    evacuationroute = new EvacuationRoute();
                    List<object> lstRouteExplosion = new List<object>();
                    double[] RouteExplosionArr = ((ScriptObject)(oArr)).ConvertTo<double[]>();
                    lstRouteExplosion.Add(RouteExplosionArr);
                    List<string[]> lstExplosionAry = new List<string[]>();
                    string[] ExplosionAry1 = new string[((object[])oArrArr[0]).Length];
                    for (int i = 0; i < ExplosionAry1.Length; i++)
                    {
                        ExplosionAry1[i] = ((object[])oArrArr[0])[i].ToString();
                    }
                    string[] ExplosionAry2 = new string[((object[])oArrArr[1]).Length];
                    for (int i = 0; i < ExplosionAry2.Length; i++)
                    {
                        ExplosionAry2[i] = ((object[])oArrArr[1])[i].ToString();
                    }
                    lstExplosionAry.Add(ExplosionAry1);
                    lstExplosionAry.Add(ExplosionAry2);
                    //if (PFApp.MapServerType == enumMapServerType.Baidu)
                    //    Message.Show("暂不支持此功能");
                    if (oStr == null)
                    {
                        evacuationroute.CallDrawAccidentSimulation("爆炸", lstRouteExplosion, lstExplosionAry);
                    }
                    else
                    {
                        evacuationroute.CallNameAccidentSimulation("爆炸", oStr, lstRouteExplosion, lstExplosionAry);
                    }
                    break;
                case "火灾模拟(疏散路径)":
                    evacuationroute = new EvacuationRoute();
                    List<object> lstRouteFire = new List<object>();
                    double[] RouteFireArr = ((ScriptObject)(oArr)).ConvertTo<double[]>();
                    lstRouteFire.Add(RouteFireArr);
                    List<string[]> lstFireAry = new List<string[]>();
                    string[] FireAry1 = new string[((object[])oArrArr[0]).Length];
                    for (int i = 0; i < FireAry1.Length; i++)
                    {
                        FireAry1[i] = ((object[])oArrArr[0])[i].ToString();
                    }
                    string[] FireAry2 = new string[((object[])oArrArr[1]).Length];
                    for (int i = 0; i < FireAry2.Length; i++)
                    {
                        FireAry2[i] = ((object[])oArrArr[1])[i].ToString();
                    }
                    lstFireAry.Add(FireAry1);
                    lstFireAry.Add(FireAry2);
                    //if (PFApp.MapServerType == enumMapServerType.Baidu)
                    //    Message.Show("暂不支持此功能");
                    if (oStr == null)
                    {
                        evacuationroute.CallDrawAccidentSimulation("火灾", lstRouteFire, lstFireAry);
                    }
                    else
                    {
                        evacuationroute.CallNameAccidentSimulation("火灾", oStr, lstRouteFire, lstFireAry);
                    }
                    break;
                case "瞬时泄漏模拟(疏散路径)":
                    evacuationroute = new EvacuationRoute();
                    List<object> lstRouteInstantaneous = new List<object>();

                    double[] RouteAryInstantaneous1 = new double[((object[])oArrArr[0]).Length];
                    for (int i = 0; i < RouteAryInstantaneous1.Length; i++)
                    {
                        RouteAryInstantaneous1[i] = Double.Parse(((object[])oArrArr[0])[i].ToString());
                    }
                    double[] RouteAryInstantaneous2 = new double[((object[])oArrArr[1]).Length];
                    for (int i = 0; i < RouteAryInstantaneous2.Length; i++)
                    {
                        RouteAryInstantaneous2[i] = Double.Parse(((object[])oArrArr[1])[i].ToString());
                    }
                    lstRouteInstantaneous.Add(RouteAryInstantaneous1);
                    lstRouteInstantaneous.Add(RouteAryInstantaneous2);

                    List<string[]> lstInstantaneousAry = new List<string[]>();
                    string[] InstantaneousAry1 = new string[((object[])oArrArr[2]).Length];
                    for (int i = 0; i < InstantaneousAry1.Length; i++)
                    {
                        InstantaneousAry1[i] = ((object[])oArrArr[2])[i].ToString();
                    }
                    string[] InstantaneousAry2 = new string[((object[])oArrArr[3]).Length];
                    for (int i = 0; i < InstantaneousAry2.Length; i++)
                    {
                        InstantaneousAry2[i] = ((object[])oArrArr[3])[i].ToString();
                    }
                    lstInstantaneousAry.Add(InstantaneousAry1);
                    lstInstantaneousAry.Add(InstantaneousAry2);
                    //if (PFApp.MapServerType == enumMapServerType.Baidu)
                    //    Message.Show("暂不支持此功能");

                    if (oArr != null)
                    {
                        lstRouteInstantaneous.Add(((ScriptObject)(oArr)).ConvertTo<double[]>());
                    }
                    if (oStr == null)
                    {
                        evacuationroute.CallDrawAccidentSimulation("瞬时泄漏", lstRouteInstantaneous, lstInstantaneousAry);
                    }
                    else
                    {
                        evacuationroute.CallNameAccidentSimulation("瞬时泄漏", oStr, lstRouteInstantaneous, lstInstantaneousAry);
                    }
                    break;
                case "连续泄漏模拟(疏散路径)":
                    evacuationroute = new EvacuationRoute();
                    List<object> lstRouteContinuous = new List<object>();

                    object[] tmpRouteContinuous1 = (object[])oArrArr[0];
                    object[] tmpRouteContinuous2 = (object[])oArrArr[1];

                    double[] AryRouteContinuous1 = new double[tmpRouteContinuous1.Length];
                    for (int i = 0; i < AryRouteContinuous1.Length; i++)
                    {
                        AryRouteContinuous1[i] = Double.Parse(tmpRouteContinuous1[i].ToString());
                    }

                    double[] AryRouteContinuous2 = new double[tmpRouteContinuous2.Length];
                    for (int j = 0; j < AryRouteContinuous2.Length; j++)
                    {
                        AryRouteContinuous2[j] = Double.Parse(tmpRouteContinuous2[j].ToString());
                    }
                    lstRouteContinuous.Add(AryRouteContinuous1);
                    lstRouteContinuous.Add(AryRouteContinuous2);
                    lstRouteContinuous.Add(oArrStr);

                    List<string[]> lstContinuousAry = new List<string[]>();
                    string[] ContinuousAry1 = new string[((object[])oArrArr[2]).Length];
                    for (int i = 0; i < ContinuousAry1.Length; i++)
                    {
                        ContinuousAry1[i] = ((object[])oArrArr[2])[i].ToString();
                    }
                    string[] ContinuousAry2 = new string[((object[])oArrArr[3]).Length];
                    for (int i = 0; i < ContinuousAry2.Length; i++)
                    {
                        ContinuousAry2[i] = ((object[])oArrArr[3])[i].ToString();
                    }
                    lstContinuousAry.Add(ContinuousAry1);
                    lstContinuousAry.Add(ContinuousAry2);
                    //if (PFApp.MapServerType == enumMapServerType.Baidu)
                    //    Message.Show("暂不支持此功能");

                    if (oStr == null)
                    {
                        evacuationroute.CallDrawAccidentSimulation("连续泄漏", lstRouteContinuous, lstContinuousAry);
                    }
                    else
                    {
                        evacuationroute.CallNameAccidentSimulation("连续泄漏", oStr, lstRouteContinuous, lstContinuousAry);
                    }
                    break;
                #endregion

                #region GPS
                case "GPS定位":
                    gpspage = new GPS.GPSPage();
                    IsStartGPS = true;
                    string strids = oArrStr[0].ToString();
                    string strtypes = oArrStr[1].ToString();
                    gpspage.StartLocationGps(strids, strtypes);
                    break;
                case "停止定位":
                    if (IsStartGPS == true)
                    {
                        gpspage.StopLocationGps();
                        IsStartGPS = false;
                    }
                    break;
                case "轨迹回放":
                    gpspage = new GPS.GPSPage();
                    IsTrackPlayBack = true;
                    string strinfo = oArrStr[0].ToString();
                    string strtype = oArrStr[1].ToString();
                    gpspage.TrackPlayBack(strinfo, strtype);
                    break;
                case "停止回放":
                    if (IsTrackPlayBack == true)
                    {
                        gpspage.StopTrackPlayBack();
                        IsTrackPlayBack = false;
                    }
                    break;
                #endregion

                #region 数据输出
                case "专题数据输出":
                    waitanimationwindow = new WaitAnimationWindow("专题数据下载中....");
                    waitanimationwindow.Show();
                    DownLoadShpFile();
                    break;
                #endregion
            }
            return "ok";
        }

        //20150514zc：打开重大危险源的平面图
        private void ShowPmtWindow(string oStr, string[] zdwxypmts)
        {
            PMTWindow win_pmt = new PMTWindow(oStr,zdwxypmts);
            win_pmt.Show();
        }

        //20150327:传入江宁系统要使用的登录角色信息
        private void InitSystemRole(string oStr)
        {
            if (oStr != string.Empty)
            {
                (Application.Current as IApp).sysRole = oStr;
            }
        }
     
        #region 专题数据下载
        void DownLoadShpFile()
        {
            xele = PFApp.Extent;
            var dataServices = (from item in xele.Element("DataServices").Elements("DataService")
                                where item.Attribute("Name").Value == "数据输出"
                                select new
                                {
                                    Type = item.Attribute("Type").Value,
                                    Url = item.Attribute("Url").Value,
                                }).ToList();
            Dictionary<string, GraphicsLayer> Dict_Data = (Application.Current as IApp).Dict_ThematicLayer;
            List<string> lstname = new List<string>();
            List<object> lstjson = new List<object>();
            AykjCreateShp.ArrayOfAnyType aotname = new AykjCreateShp.ArrayOfAnyType();
            AykjCreateShp.ArrayOfAnyType aotjson = new AykjCreateShp.ArrayOfAnyType();
            for (int i = 0; i < Dict_Data.Count; i++)
            {
                aotname.Add(Dict_Data.Keys.ToArray()[i]);
                GraphicsLayer gralayer = Dict_Data.Values.ToArray()[i];
                AykjCreateShp.ArrayOfAnyType aottmp = new AykjCreateShp.ArrayOfAnyType();
                for (int m = 0; m < gralayer.Graphics.Count(); m++)
                {
                    Graphic tmpgra = gralayer.Graphics[m];
                    string strjson = "{\"wxyid\":\"" + tmpgra.Attributes["StaTag"].ToString().Split('|')[1] + "\",";
                    strjson = strjson + "\"wxytype\":\"" + tmpgra.Attributes["StaTag"].ToString().Split('|')[0] + "\",";
                    strjson = strjson + "\"dwdm\":\"" + tmpgra.Attributes["StaTag"].ToString().Split('|')[2] + "\",";
                    strjson = strjson + "\"remark\":\"" + tmpgra.Attributes["StaTag"].ToString().Split('|')[3] + "\",";
                    strjson = strjson + "\"x\":\"" + (tmpgra.Geometry as MapPoint).X.ToString() + "\",";
                    strjson = strjson + "\"y\":\"" + (tmpgra.Geometry as MapPoint).Y.ToString() + "\"}";
                    aottmp.Add(strjson);
                }
                aotjson.Add(aottmp);
            }
            AykjCreateShp.Service1SoapClient AykjClientShp = new AykjCreateShp.Service1SoapClient(new BasicHttpBinding(), new EndpointAddress(dataServices[0].Url));
            AykjClientShp.ToShpCompleted += new EventHandler<AykjCreateShp.ToShpCompletedEventArgs>(AykjClientShp_ToShpCompleted);
            AykjClientShp.ToShpAsync(aotname, aotjson);
        }

        void AykjClientShp_ToShpCompleted(object sender, AykjCreateShp.ToShpCompletedEventArgs e)
        {
            waitanimationwindow.Close();
            if (e.Result.ToString().Contains("Err:"))
            {
                Message.Show(e.Result.ToString().Split(':')[1]);
            }
            else
            {
                xele = PFApp.Extent;
                var aryurl = (from item in xele.Element("DataServices").Elements("Parameter")
                              where item.Attribute("Name").Value == "数据输出"
                              select new
                              {
                                  Value = item.Attribute("Value").Value,
                              }).ToList();
                string strurl = aryurl[0].Value + e.Result.Split('\\')[e.Result.Split('\\').Count() - 1];
                HtmlPage.Window.Invoke("DownLoadShp", strurl);
            }
        }
        #endregion

        #endregion

        #region
        string CurrButton = "";
        void plotting()
        {
            string type = "AYKJ.GISPlot";
            //判断点击关联的面板是否存在
            if (!PFApp.UIS.ContainsKey(type))
                return;
            
            string psender = "plot";
            //获取关联的面板
            IPart part = PFApp.UIS[type] as IPart;
            if (CurrButton == psender)
            {
                if ((part as UserControl).Parent != null)
                {
                    part.Close();
                }
                else
                {
                    part.Show();
                }
            }
            else
            {
                CurrButton = "plot";
                if (PFApp.UIS.ContainsKey(type))
                {
                    try
                    {
                        part.Show();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("组件未实现平台指定的打开方法或者该方法中有异常！\n" + ex.ToString());
                    }
                }
            }
        }
        #endregion

        #region 根据传值进行数据查询
        /// <summary>
        /// 不与地图交互
        /// </summary>
        /// <param name="querytype"></param>
        /// <param name="wxyid"></param>
        /// <param name="wxydwdm"></param>
        /// <param name="wxyremark"></param>
        /// <param name="wxytype"></param>
        public void DataQuery(string[] querytype,string radius, string wxyid, string wxydwdm, string wxyremark, string wxytype,int param)
        {
            if(this.mainmap == null)
            {
                mainmap = (Application.Current as IApp).MainMap;
            }
            if (Draw_GraLayer == null)
            {
                Draw_GraLayer = new GraphicsLayer();
                mainmap.Layers.Add(Draw_GraLayer);
            }
            XElement xele = PFApp.Extent;
            strGeometryurl = (from item in PFApp.Extent.Elements("GeometryService")
                              select item.Attribute("Url").Value).ToArray()[0];
            Draw_GraLayer.ClearGraphics();

            DataQueryWCF(querytype, radius, wxyid, wxytype, wxydwdm, param);

        }

        void DataQueryWCF(string[] querytype,string radiu,string m_wxyid,string m_wxytype,string m_dwdm,int param)
        {
            if (AykjClientInnerDraw == null)
            {
                xele = PFApp.Extent;
                var dataServices = (from item in xele.Element("DataServices").Elements("DataService")
                                    where item.Attribute("Name").Value == "专题数据"
                                    select new
                                    {
                                        Type = item.Attribute("Type").Value,
                                        Url = item.Attribute("Url").Value,
                                    }).ToList();

                AykjClientInnerDraw = new AykjDataServiceInner.AykjDataClient(new BasicHttpBinding(), new EndpointAddress(dataServices[0].Url));

            }
            if (AykjClientInnerDraw != null)
            {
                AykjClientInnerDraw.getDataLocationCompleted -= AykjClientInner_getDataCompleted;
                AykjClientInnerDraw.getDataLocationCompleted += new EventHandler<AykjDataServiceInner.getDataLocationCompletedEventArgs>(AykjClientInner_getDataCompleted);
                
                string state = radiu+"@";
                if (querytype.Length != 0)
                {
                    foreach (string str in querytype)
                    {
                        state += str + "|";
                    }
                    state += "@" + param;
                }
                else
                    state += param;
                AykjClientInnerDraw.getDataLocationAsync(m_wxyid, m_wxytype, m_dwdm, state);
            }
        }

        void AykjClientInner_getDataCompleted(object sender, AykjDataServiceInner.getDataLocationCompletedEventArgs e)
        {
            if (e.Result.Contains("成功"))
            {
                string radus = "";
                string param = "";
                string[] strparams = (e.UserState as string).Split('@');
                string[] querytype = null;
                if (strparams.Length == 2)
                {
                    radus = strparams[0];
                    param = strparams[1];
                }
                else
                {
                    radus = strparams[0];
                    param = strparams[2];
                    querytype = strparams[1].Split('|');
                }
 
                MapPoint mp = new MapPoint(
                    Double.Parse(e.Result.ToString().Split(',')[1]),
                    Double.Parse(e.Result.ToString().Split(',')[2])
                );

                double radiu = double.Parse(radus);

                      XElement element = PFApp.Extent;
                //读取geomtetry的地址参数
                string geoServiceUrl = (from item in element.Elements("GeometryService")
                                        select item.Attribute("Url").Value).ToArray()[0];

                if (PFApp.MapServerType == enumMapServerType.Baidu)
                {
                    radiusQueryByToolKit(radiu, mp, querytype, param);
                }
                else if (PFApp.MapServerType == enumMapServerType.Esri)
                {                    
                    radiusQueryByService(geoServiceUrl, mp, e.UserState);
                }
                else if (geoServiceUrl != "")
                {
                    radiusQueryByService(geoServiceUrl, mp, e.UserState);
                }
            }
            else
            {
                mLink = false;
                Message.Show("坐标查询失败");
                return;
            }
        }

        void radiusQueryByToolKit(double radiu, MapPoint mp, string[] querytype, string param)
        {
            Graphic gra = new Graphic() { Geometry = mp };
            List<Graphic> lstgra = new List<Graphic>();
            //lstgra.Add(gra);
            List<MapPoint> lstmp = new List<MapPoint>();
            #region 画圆
            double sita = 0;
            MapPoint centerpoint = mp;
            double fromangle = 0;
            double toangle = 360;
            for (double i = fromangle; i <= toangle; i += 0.2)
            {
                sita = i * Math.PI / 180;
                MapPoint tmp = new MapPoint(centerpoint.X + radiu * Math.Cos(sita), centerpoint.Y + radiu * Math.Sin(sita));
                tmp.SpatialReference = centerpoint.SpatialReference;
                lstmp.Add(tmp);
                Graphic grap = new Graphic() { Geometry = tmp };

                lstgra.Add(grap);

            }
            #endregion
            Polygon circle = new Polygon();
            circle.Rings.Add(new ESRI.ArcGIS.Client.Geometry.PointCollection(lstmp));

            List<Graphic> lst = new List<Graphic>();
            Dict_Data = (Application.Current as IApp).Dict_ThematicLayer;
            if (querytype != null && querytype.Length != 0)
            {
                List<string> tmpquery = new List<string>();
                for (int i = 0; i < querytype.Length; i++)
                {

                    if (querytype[i] != "" && querytype[i] != null)
                    {
                        if ((Application.Current as IApp).DictThematicEnCn.ContainsKey(querytype[i]))
                        {
                            string tmptype = (Application.Current as IApp).DictThematicEnCn[querytype[i]];
                            tmpquery.Add(tmptype);
                            lst.AddRange(Dict_Data[tmptype]);
                        }
                    }
                }
                selectType = tmpquery.ToArray();
            }
            else
            {
                foreach (KeyValuePair<string, GraphicsLayer> kv in Dict_Data)
                {
                    lst.AddRange(kv.Value);
                }
            }
            if (lst == null || lst.Count == 0)
            {
                Message.Show("所选范围内没有专题数据");
                //data_result.ItemsSource = null;
                return;
            }

            clsrangequery = new clsRangeQuery();
            clsrangequery.RangeQueryEvent -= clsrangequery_RangeQueryEvent;
            clsrangequery.RangeQueryEvent += new RangeQueryDelegate(clsrangequery_RangeQueryEvent);
            clsrangequery.RangeQueryFaildEvent -= clsrangequery_RangeQueryFaildEvent;
            clsrangequery.RangeQueryFaildEvent += new RangeQueryDelegate(clsrangequery_RangeQueryFaildEvent);
            Graphic draw_graphic = new Graphic();
            draw_graphic.Geometry = circle;
            draw_graphic.Geometry.SpatialReference = mainmap.SpatialReference;

            if (param == "1")
            {
                draw_graphic.Symbol = DrawFillSymbol;
                this.pass2ExposeGra = draw_graphic;
                //Draw_GraLayer.Graphics.Add(draw_graphic);
                this.queryparam = "1";
            }
            else if (param == "0")
            {
                this.queryparam = "0";
            }
            waitanimationwindow = new WaitAnimationWindow("数据查询中，请稍候...");
            waitanimationwindow.Show();
            clsrangequery.RangeQuery(draw_graphic.Geometry, lst);
        }

        //改走buffer，避免esri服务发布的地图是平面坐标时ProjectAsync失败
        void radiusQueryByService(string geoServiceUrl, MapPoint mp, object state)
        {
            geoService = new GeometryService(geoServiceUrl);
            geoService.BufferCompleted -= geometrytask_BufferCompleted;
            geoService.Failed -= geometrytask_Failed;
            geoService.BufferCompleted += new EventHandler<GraphicsEventArgs>(geometrytask_BufferCompleted);
            geoService.Failed += new EventHandler<TaskFailedEventArgs>(geometrytask_Failed);

            mp.SpatialReference = this.mainmap.SpatialReference;
            Graphic gra = new Graphic() { Geometry = mp };

            string[] strparams = state.ToString().Split('@');
            double radius = 0;
            double.TryParse(strparams[0], out radius);

            BufferParameters bufferparameters = new BufferParameters();
            bufferparameters.Unit = LinearUnit.Meter;
            bufferparameters.BufferSpatialReference = mainmap.SpatialReference;
            bufferparameters.OutSpatialReference = mainmap.SpatialReference;
            bufferparameters.Distances.Add(radius);
            bufferparameters.Features.Add(gra);
            geoService.BufferAsync(bufferparameters,state);
        }

        private void geometrytask_Failed(object sender, TaskFailedEventArgs e)
        {
            Message.Show("周边环境查询失败", e.Error.Message);
        }

        private void geometrytask_BufferCompleted(object sender, GraphicsEventArgs e)
        {
            try
            {
                string radus = "";
                string param = "";
                string statu = e.UserState as string;

                string[] querytype = null;
                string[] strparams = statu.Split('@');
                if (strparams.Length == 2)
                {
                    radus = strparams[0];
                    param = strparams[1];
                }
                else
                {
                    radus = strparams[0];
                    param = strparams[2];
                    querytype = strparams[1].Split('|');
                }

                //selectType = querytype;

                List<Graphic> gras = e.Results as List<Graphic>;
                if (gras.Count == 0)
                {
                    Message.ShowError("Buffer分析失败,没有返回区域范围");
                    return;
                }

                List<Graphic> lst = new List<Graphic>();
                Dict_Data = (Application.Current as IApp).Dict_ThematicLayer;

                if (querytype != null && querytype.Length != 0)
                {
                    List<string> tmpquery = new List<string>();
                    for (int i = 0; i < querytype.Length; i++)
                    {

                        if (querytype[i] != "" && querytype[i] != null)
                        {
                            string tmptype = (Application.Current as IApp).DictThematicEnCn[querytype[i]];

                            tmpquery.Add(tmptype);
                            lst.AddRange(Dict_Data[tmptype]);
                        }
                    }
                    selectType = tmpquery.ToArray();

                }
                else
                {
                    foreach (KeyValuePair<string, GraphicsLayer> kv in Dict_Data)
                    {
                        lst.AddRange(kv.Value);
                    }
                }

                if (lst == null || lst.Count == 0)
                {
                    Message.Show("所选范围内没有专题数据");
                    //data_result.ItemsSource = null;
                    return;
                }

                if (param == "1")
                {
                    gras[0].Symbol = DrawFillSymbol;
                    this.pass2ExposeGra = gras[0];
                    //Draw_GraLayer.Graphics.Add(draw_graphic);
                    this.queryparam = "1";
                }

                clsrangequery = new clsRangeQuery();
                clsrangequery.RangeQueryEvent -= clsrangequery_RangeQueryEvent;
                clsrangequery.RangeQueryEvent += new RangeQueryDelegate(clsrangequery_RangeQueryEvent);
                clsrangequery.RangeQueryFaildEvent -= clsrangequery_RangeQueryFaildEvent;
                clsrangequery.RangeQueryFaildEvent += new RangeQueryDelegate(clsrangequery_RangeQueryFaildEvent);

                try
                {
                    clsrangequery.RangeQuery(strGeometryurl, gras[0].Geometry, lst);
                }
                catch (Exception e2)
                {
                    MessageBox.Show("ex2:" + e2.Message);
                }
            }
            catch (Exception e1)
            {                
                MessageBox.Show("ex1:" + e1.Message);
            }            

            waitanimationwindow = new WaitAnimationWindow("数据查询中，请稍候...");
            waitanimationwindow.Show();
            
        }

        void radiusQueryByServiceOut(string geoServiceUrl, MapPoint mp, object state)
        {
            geoService = new GeometryService(geoServiceUrl);
            mp.SpatialReference = this.mainmap.SpatialReference;
            Graphic gra = new Graphic() { Geometry = mp };
            List<Graphic> lstgra = new List<Graphic>();
            lstgra.Add(gra);
            geoService.ProjectCompleted -= geoService_ProjectCompleted;
            geoService.ProjectCompleted += new EventHandler<GraphicsEventArgs>(geoService_ProjectCompleted);
            geoService.Failed -= geoService_Failed;
            geoService.Failed += geoService_Failed;
            SpatialReference sp = new SpatialReference(2385);
            try
            {
                geoService.ProjectAsync(lstgra, sp, state);
            }
            catch (Exception ex)
            {
                Message.Show("周边环境查询失败", ex.Message);
            }
        }

        void geoService_Failed(object sender, TaskFailedEventArgs e)
        {
            Message.ShowErrorInfo("周边环境查询失败",e.Error.Message+"请检查几何服务或几何服务地址是否正确");
        }

        void geoService_ProjectCompleted(object sender, GraphicsEventArgs e)
        {
            string radus = "";
            string param = "";
            string statu = e.UserState as string;
            string[] querytype = null;
            string[] strparams = statu.Split('@');
            if (strparams.Length == 2)
            {
                radus = strparams[0];
                param = strparams[1];
            }
            else
            {
                radus = strparams[0];
                param = strparams[2];
                querytype = strparams[1].Split('|');
            }

            List<Graphic> gras = e.Results as List<Graphic>;
            MapPoint mp = gras[0].Geometry as MapPoint;
            double radiu = double.Parse(radus);
            this.GetCirclePoints(mp, radiu,e.UserState as string);
            
        }

        /// <summary>
        /// 画圆
        /// </summary>
        /// <param name="centerpoint"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        void GetCirclePoints(MapPoint centerpoint, double r,string state)
        {
            string radus = "";
            string param = "";
            string statu = state;
            string[] querytype = null;
            string[] strparams = statu.Split('@');
            if (strparams.Length == 2)
            {
                radus = strparams[0];
                param = strparams[1];
            }
            else
            {
                radus = strparams[0];
                param = strparams[2];
                querytype = strparams[1].Split('|');
            }

            //centerpoint = BaiduMeature.ProjBaidu2LonLat(centerpoint);
  
            
            List<Graphic> lstgra = new List<Graphic>();
            #region 画圆
            double sita = 0;

            double fromangle = 0;
            double toangle = 360;
            for (double i = fromangle; i <= toangle; i += 0.2)
            {
                sita = i * Math.PI / 180;
                MapPoint tmp = new MapPoint(centerpoint.X + r * Math.Cos(sita), centerpoint.Y + r * Math.Sin(sita));
                tmp.SpatialReference = centerpoint.SpatialReference;
                Graphic gra = new Graphic() { Geometry = tmp };

                lstgra.Add(gra);

            }
            #endregion
            geoService.Failed -= geoService_Failed;
            geoService.Failed += geoService_Failed;
            geoService.ProjectCompleted -= geoService_ProjectCompleted;
            geoService.ProjectCompleted+=new EventHandler<GraphicsEventArgs>(geoService_ProjectCompletednew);
            try
            {
                geoService.ProjectAsync(lstgra, this.mainmap.SpatialReference, state);
            }
            catch (Exception ex)
            {
                Message.Show(ex.Message);
            }
            //return rtn;
        }
        void geoService_ProjectCompletednew(object sender, GraphicsEventArgs e)
        {
            string radus = "";
            string param = "";
            string statu = e.UserState as string;
            string[] querytype = null;
            string[] strparams = statu.Split('@');
            if (strparams.Length == 2)
            {
                radus = strparams[0];
                param = strparams[1];
            }
            else
            {
                radus = strparams[0];
                param = strparams[2];
                querytype = strparams[1].Split('|');
            }
            //selectType = querytype;

            List<Graphic> gras = e.Results as List<Graphic>;
            List<MapPoint> rtn = new List<MapPoint>();
            foreach (Graphic gra in gras)
            {
                MapPoint tmp = gra.Geometry as MapPoint;
                rtn.Add(tmp);
            }

            Polygon circle = new Polygon();
            circle.Rings.Add(new ESRI.ArcGIS.Client.Geometry.PointCollection(rtn));

            List<Graphic> lst = new List<Graphic>();
            Dict_Data = (Application.Current as IApp).Dict_ThematicLayer;
            if (querytype != null && querytype.Length != 0)
            {
                List<string> tmpquery = new List<string>();
                for (int i = 0; i < querytype.Length; i++)
                {

                    if (querytype[i] != "" && querytype[i] != null)
                    {
                        string tmptype = (Application.Current as IApp).DictThematicEnCn[querytype[i]];
                        tmpquery.Add(tmptype);
                        lst.AddRange(Dict_Data[tmptype]);
                    }
                }
                selectType = tmpquery.ToArray();
            }
            else
            {
                foreach (KeyValuePair<string, GraphicsLayer> kv in Dict_Data)
                {
                    lst.AddRange(kv.Value);
                }
            }
            if (lst == null || lst.Count == 0)
            {
                Message.Show("所选范围内没有专题数据");
                //data_result.ItemsSource = null;
                return;
            }

            clsrangequery = new clsRangeQuery();
            clsrangequery.RangeQueryEvent -= clsrangequery_RangeQueryEvent;
            clsrangequery.RangeQueryEvent += new RangeQueryDelegate(clsrangequery_RangeQueryEvent);
            clsrangequery.RangeQueryFaildEvent -= clsrangequery_RangeQueryFaildEvent;
            clsrangequery.RangeQueryFaildEvent += new RangeQueryDelegate(clsrangequery_RangeQueryFaildEvent);
            Graphic draw_graphic = new Graphic();
            draw_graphic.Geometry = circle;
            draw_graphic.Geometry.SpatialReference = mainmap.SpatialReference;

            if (param == "1")
            {
                draw_graphic.Symbol = DrawFillSymbol;
                this.pass2ExposeGra = draw_graphic;
                //Draw_GraLayer.Graphics.Add(draw_graphic);
                this.queryparam = "1";
            }

            clsrangequery.RangeQuery(strGeometryurl, draw_graphic.Geometry, lst);
            waitanimationwindow = new WaitAnimationWindow("数据查询中，请稍候...");
            waitanimationwindow.Show();
        }
        /// <summary>
        /// 需要与地图交互
        /// </summary>
        /// <param name="querytype"></param>
        /// <param name="subtype"></param>
        /// <param name="wxytype"></param>
        public void DataQuery(string querytype, string subtype, string wxytype)
        {
            if (this.mainmap == null)
            {
                mainmap = (Application.Current as IApp).MainMap;
            }
            if (Draw_GraLayer == null)
            {
                Draw_GraLayer = new GraphicsLayer();
                mainmap.Layers.Add(Draw_GraLayer);
            }
            XElement xele = PFApp.Extent;
            strGeometryurl = (from item in PFApp.Extent.Elements("GeometryService")
                              select item.Attribute("Url").Value).ToArray()[0];
            Draw_GraLayer.ClearGraphics();
            if (querytype == "radius")
            {
                DataQueryRadius(wxytype);
            }
        }

        void DataQueryRadius(string wxytype)
        {            
            selectType = null;
            if (wxytype.Contains("|"))
            {
                selectType = wxytype.Split('|');
            }
            else if (wxytype != "")
            {
                selectType = new string[] { wxytype };
            }


            clsrangequery = new clsRangeQuery();
            clsrangequery.RangeQueryEvent -= clsrangequery_RangeQueryEvent;
            clsrangequery.RangeQueryEvent += new RangeQueryDelegate(clsrangequery_RangeQueryEvent);
            clsrangequery.RangeQueryFaildEvent -= clsrangequery_RangeQueryFaildEvent;
            clsrangequery.RangeQueryFaildEvent += new RangeQueryDelegate(clsrangequery_RangeQueryFaildEvent);
            drc = new DrawCircle(Draw_GraLayer);
            drc.Map = mainmap;
            drc.DrawCircleEvent -= drc_DrawCircleEvent;
            drc.DrawCircleEvent += new DrawCircleDelegate(drc_DrawCircleEvent);
            drc.IsActivated = true;
        }
        void drc_DrawCircleEvent(object sender, EventArgs e)
        {
            //btn_query(btn_clear,null);
            Graphic draw_graphic = new Graphic();
            drc.CirclereturnPolygon = CirlePolygon;
            draw_graphic.Geometry = drc.Getreturngeometry as ESRI.ArcGIS.Client.Geometry.Polygon;
            draw_graphic.Symbol = DrawFillSymbol;
            draw_graphic.Geometry.SpatialReference = mainmap.SpatialReference;
            Draw_GraLayer.Graphics.Add(draw_graphic);
            drc.IsActivated = false;

            List<Graphic> lst = new List<Graphic>();
            Dict_Data = (Application.Current as IApp).Dict_ThematicLayer;

            if (selectType != null)
            {
                //string el_type = string.Empty;
                for (int i = 0; i < selectType.Length; i++)
                {
                    //foreach (var item in (Application.Current as IApp).DictThematicEnCn)
                    //{
                    //    if (item.Value == selectType[i])
                    //    {
                    //        el_type = item.Key;
                    //        break;
                    //    }
                    //}
                    //如果中英文对照表中存在此一类型，则进行查询，否则跳过，以免字典报错 shyh
                    if ((Application.Current as IApp).DictThematicEnCn.ContainsKey(selectType[i]))
                    {
                        lst.AddRange(Dict_Data[(Application.Current as IApp).DictThematicEnCn[selectType[i]]]);
                    }
                }
            }
            else
            {
                foreach (KeyValuePair<string, GraphicsLayer> kv in Dict_Data)
                {
                    lst.AddRange(kv.Value);
                }
            }

            if (lst == null || lst.Count == 0)
            {
                Message.Show("所选范围内没有专题数据");
                //data_result.ItemsSource = null;
                return;
            }
            waitanimationwindow = new WaitAnimationWindow("数据查询中，请稍候...");
            waitanimationwindow.Show();
            if(PFApp.MapServerType==enumMapServerType.Baidu)
                clsrangequery.RangeQuery(draw_graphic.Geometry, lst);
            else if(PFApp.MapServerType==enumMapServerType.Esri)                
                clsrangequery.RangeQuery(strGeometryurl, draw_graphic.Geometry, lst);
            else if(strGeometryurl!="")
                clsrangequery.RangeQuery(strGeometryurl, draw_graphic.Geometry, lst);
        }
        /// <summary>
        /// 出错
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clsrangequery_RangeQueryFaildEvent(object sender, RangeQueryEventArgs e)
        {
            waitanimationwindow.Close();
            Draw_GraLayer.Graphics.Clear();
            this.rtnQueryResultsStr = "没有查询到相关数据";
            this.querywait = false;
            Message.ShowErrorInfo("查询出错",e.Message);
        }

        private void centerAndZoom(ESRI.ArcGIS.Client.Map map, ESRI.ArcGIS.Client.Geometry.MapPoint mapPoint, double resolution)
        {
            double ratio = 1.0;
            if (map.Resolution != 0.0)
                ratio = resolution / map.Resolution;

            if (ratio == 1.0)
                map.PanTo(mapPoint);
            else
            {
                ESRI.ArcGIS.Client.Geometry.MapPoint mapCenter = map.Extent.GetCenter();
                double X = (mapPoint.X - ratio * mapCenter.X) / (1 - ratio);
                double Y = (mapPoint.Y - ratio * mapCenter.Y) / (1 - ratio);
                map.ZoomToResolution(resolution, new ESRI.ArcGIS.Client.Geometry.MapPoint(X, Y));
            }
        }

        /// <summary>
        /// 调用方法返回结果
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clsrangequery_RangeQueryEvent(object sender, EventArgs e)
        {            
            //20150617zc:新增企业平面图内部定位（缩放分辨率不同）
            if (iAction.Equals("定位"))
            {
                iAction = string.Empty;

                int rIndex = (sender as clsRangeQuery).ReturnIndex;
                MapPoint cPoint = (sender as clsRangeQuery).centerPoint;

                if (rIndex == -1)
                {
                    double defResolution = 2;
                    try
                    {
                        defResolution = double.Parse(xele.Element("PmtServices").Attribute("DefResolution").Value);
                    }
                    catch(Exception e1)
                    {
                        Message.ShowError("解析配置文件plat.xml中<PmtServices>中在底图直接定位的分辨率（DefResolution），请检查配置文件。","解析错误");
                    }
                    //mainmap.ZoomToResolution(defResolution, cPoint);

                    centerAndZoom(mainmap, cPoint, defResolution);
                }
                else
                {
                    if (list_PmtService[rIndex].Visible)
                    {
                        Graphic pmt_graphic = new Graphic() { Geometry = list_polygon[rIndex], Symbol = PmtFillSymbol };
                        selectHigh_GraLayer.Graphics.Add(pmt_graphic);
                    }
                    //mainmap.ZoomToResolution(Double.Parse(list_PmtService[rIndex].Resolution), cPoint);

                    centerAndZoom(mainmap, cPoint, Double.Parse(list_PmtService[rIndex].Resolution));
                }


                //调用结束，重置外部调用的标志位为不可用状态
                mLink = false;
                ScriptObject locPointFinished = HtmlPage.Window.GetProperty("locPointFinished") as ScriptObject;
                locPointFinished.InvokeSelf("success");
            }
            else
            {
                List<Graphic> lstreturngra = (sender as clsRangeQuery).lstReturnGraphic;
                if (this.queryparam == "1")
                {
                    this.queryparam = "";
                    if (dataQueryPageEx == null)
                    {
                        dataQueryPageEx = new GISExtension.DataQueryPageExpose("rbtn_radius", lstreturngra, pass2ExposeGra, selectType);
                        dataQueryPageEx.Show();
                    }
                    else if (dataQueryPageEx.isClosed)
                    {
                        dataQueryPageEx = null;
                        dataQueryPageEx = new GISExtension.DataQueryPageExpose("rbtn_radius", lstreturngra, pass2ExposeGra, selectType);
                        dataQueryPageEx.Show();
                    }
                    else
                        dataQueryPageEx.dataQueryRadius(lstreturngra, pass2ExposeGra, selectType);
                }
                else
                {
                    string R = "{\"return\":[";
                    for (int i = 0; i < lstreturngra.Count(); i++)
                    {
                        string[] arytmp = lstreturngra[i].Attributes["StaTag"].ToString().Split('|');
                        string wxytype = arytmp[0];
                        string wxyid = arytmp[1];
                        string dwdm = arytmp[2];
                        string remark = arytmp[3];

                        string r = "{" +
                                           "'WXYID':'" + wxyid + "'," +
                                           "'WXYTYPE':'" + wxytype + "'," +
                                           "'DWDM':'" + dwdm + "'," +
                                           "'REMARK':'" + remark +
                                           "'},";
                        R += r;
                    }
                    if (R.Contains("WXYID"))
                    {
                        R = R.Substring(0, R.Length - 1) + "]}";
                    }
                    else
                    {
                        R = "没有查询到相关数据";
                    }
                    this.rtnQueryResultsStr = R;
                    ScriptObject queryfinished = HtmlPage.Window.GetProperty("QueryFinished") as ScriptObject;
                    queryfinished.InvokeSelf(this.rtnQueryResultsStr);
                }
                this.querywait = false;
                waitanimationwindow.Close();
            }
        }
        #endregion

        #region 根据传值直接删除专题数据
        public void linkDeleteData(string oParms)
        {
            m_wxyid = oParms.Split('|')[0];
            m_wxytype = oParms.Split('|')[1];
            m_dwdm = oParms.Split('|')[2];
            m_remark = oParms.Split('|')[3];
            MessageWindow msw = new MessageWindow(MsgType.Info, m_remark, "删除？");
            msw.Closed -= msw_Closed;
            msw.Closed += new EventHandler(msw_Closed);
            msw.Show();
        }

        void msw_Closed(object sender, EventArgs e)
        {
            MessageWindow cb_msw = sender as MessageWindow;
            if (cb_msw.DialogResult == true)
            {
                //调用wcf删除数据
                if (AykjClientInner == null)
                {
                    #region 20120809zc:读取数据服务信息
                    xele = PFApp.Extent;
                    var dataServices = (from item in xele.Element("DataServices").Elements("DataService")
                                        where item.Attribute("Name").Value == "专题数据"
                                        select new
                                        {
                                            Type = item.Attribute("Type").Value,
                                            Url = item.Attribute("Url").Value,
                                        }).ToList();

                    AykjClientInner = new AykjDataServiceInner.AykjDataClient(new BasicHttpBinding(), new EndpointAddress(dataServices[0].Url));

                    #endregion
                }
                if (AykjClientInner != null)
                {
                    AykjClientInner.deleteSqlDataByIdCompleted -= AykjClientInner_deleteSqlDataByIdCompleted;
                    AykjClientInner.deleteSqlDataByIdCompleted += new EventHandler<AykjDataServiceInner.deleteSqlDataByIdCompletedEventArgs>(AykjClientInner_deleteSqlDataByIdCompleted);
                    AykjClientInner.deleteSqlDataByIdAsync(m_wxyid, m_wxytype, m_dwdm);
                }
            }
        }

        /// <summary>
        /// 删除数据成功事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void AykjClientInner_deleteSqlDataByIdCompleted(object sender, AykjDataServiceInner.deleteSqlDataByIdCompletedEventArgs e)
        {
            if (e.Result.Contains("不存在"))
            {
                Message.Show("指定数据不存在");
                return;
            }
            if (e.Result.Contains("成功"))
            {
                //刷新地图显示
                try
                {
                    DictIn_layer = (Application.Current as IApp).Dict_ThematicLayer;
                    DictIn_EnCn = (Application.Current as IApp).DictThematicEnCn;
                    //删除数据所在的图层名
                    string s = DictIn_EnCn[m_wxytype];
                    GraphicsLayer gl = DictIn_layer[s];
                    MapPoint t_c = new MapPoint();
                    for (int i = 0; i < gl.Graphics.Count; i++)
                    {
                        Graphic g = gl.Graphics[i];
                        string stag = g.Attributes["StaTag"].ToString();
                        if (stag.Split('|')[0] == m_wxytype && stag.Split('|')[1] == m_wxyid && stag.Split('|')[2] == m_dwdm && stag.Split('|')[3] == m_remark)
                        {
                            t_c = g.Geometry as MapPoint;
                            (Application.Current as IApp).lstThematic.Remove(g);
                            gl.Graphics.Remove(g);
                            break;
                        }
                    }
                    if (mainmap.Layers["selectHigh_Layer_circle"] != null)
                    {
                        selectHigh_GraLayer = mainmap.Layers["selectHigh_Layer_circle"] as GraphicsLayer;
                        selectHigh_GraLayer.Graphics.Clear();
                    }
                    //20130918:同时清除弹出窗口的高亮
                    try
                    {
                        if (mainmap.Layers["ShowBusiness_Layer"] != null)
                        {
                            GraphicsLayer t_business = mainmap.Layers["ShowBusiness_Layer"] as GraphicsLayer;

                            for (int j = 0; j < t_business.Graphics.Count; j++)
                            {
                                Graphic rc = t_business.Graphics[j];
                                MapPoint t_p = rc.Geometry as MapPoint;
                                if (t_p.Equals(t_c))
                                {
                                    t_business.Graphics.Remove(rc);
                                    j--;
                                }
                            }

                        }
                    }
                    catch (Exception e1)
                    {
                    }
                }
                catch (Exception ex)
                {
                    AYKJ.GISDevelop.Platform.ToolKit.Message.Show("数据删除成功，地图刷新异常，请手动刷新页面。");
                    return;
                }
                AYKJ.GISDevelop.Platform.ToolKit.Message.Show("删除成功");
            }
            else
            {
                AYKJ.GISDevelop.Platform.ToolKit.Message.Show("删除失败");
            }
        }
        #endregion

        #region 含坐标信息打点
        public void linkAddDataWithLocation(string oParm)
        {
            //20130922：提供输入坐标的窗口
            addpointwithloc = new AddPointWithLoc();

            addpointwithloc.LoadAddPointWithLoc(oParm);
            
            //link_addData_WithLocation(oParm);
        }

        //经纬度转Web墨卡托
        public static double[] lonLat2Mercator(double lon,double lat)
         {
          double[] xy = new double[2];
             double x = lon *20037508.342789/180;
             double y = Math.Log(Math.Tan((90 + lat) * Math.PI / 360)) / (Math.PI / 180);
             y = y *20037508.34789/180;
             xy[0] = x;
             xy[1] = y;
             return xy;
         }

        //Web墨卡托转经纬度
        public static double[] Mercator2lonLat(double mercatorX, double mercatorY)
        {
            double[] xy = new double[2];
            double x = mercatorX / 20037508.34 * 180;
            double y = mercatorY / 20037508.34 * 180;
            y = 180 / Math.PI * (2 * Math.Atan(Math.Exp(y * Math.PI / 180)) - Math.PI / 2);
            xy[0] = x;
            xy[1] = y;
            return xy;
        }

        /// <summary>
        /// 外部接口函数，含坐标信息打点
        /// </summary>
        /// <param name="ts">业务系统传入的规定格式的值（包含四个主要参数,以及坐标点的x,y）</param>
        public void link_addData_WithLocation(string ts)
        {
            //解析传入的值
            try
            {
                m_wxyid = ts.Split('|')[0];
                m_wxytype = ts.Split('|')[1];
                m_dwdm = ts.Split('|')[2];
                m_remark = ts.Split('|')[3];
                m_street = ts.Split('|')[6];
                m_enttype = ts.Split('|')[7];

                if (iAction == "GPS打点")
                {
                    //20150325:江宁智慧安监，传入的gps经纬度值，和地图坐标不一致，需要转换。
                    //20150326:经过多点测试，转换后，坐标值有一定偏移，大概偏移值是720和830。
                    m_x = (lonLat2Mercator(Double.Parse(ts.Split('|')[4]), Double.Parse(ts.Split('|')[5]))[0] - 720).ToString();
                    m_y = (lonLat2Mercator(Double.Parse(ts.Split('|')[4]), Double.Parse(ts.Split('|')[5]))[1] - 830).ToString();
                }
                else
                {
                    m_x = ts.Split('|')[4];
                    m_y = ts.Split('|')[5];
                }
            }
            catch (Exception e)
            {
                AYKJ.GISDevelop.Platform.ToolKit.Message.Show("外部页面传入打点值解析异常，请检查参数。");
                return;
            }


            if (AykjClientInner == null)
            {
                xele = PFApp.Extent;
                var dataServices = (from item in xele.Element("DataServices").Elements("DataService")
                                    where item.Attribute("Name").Value == "专题数据"
                                    select new
                                    {
                                        Type = item.Attribute("Type").Value,
                                        Url = item.Attribute("Url").Value,
                                    }).ToList();

                AykjClientInner = new AykjDataServiceInner.AykjDataClient(new BasicHttpBinding(), new EndpointAddress(dataServices[0].Url));
            }
            if (AykjClientInner != null)
            {
                try
                {
                    AykjClientInner.addNewSqlDataCompleted -= AykjClientInner_addNewSqlDataCompleted;
                    AykjClientInner.addNewSqlDataCompleted += new EventHandler<AykjDataServiceInner.addNewSqlDataCompletedEventArgs>(AykjClientInner_addNewSqlDataCompleted);

                    AykjClientInner.addNewSqlDataAsync(m_wxyid, m_wxytype, m_dwdm, "", m_remark, m_x, m_y, m_street, m_enttype);
                }
                catch (Exception e1)
                {
                    AYKJ.GISDevelop.Platform.ToolKit.Message.Show("含坐标专题数据打点异常");
                }
            }

        }
        #endregion

        #region 无坐标信息，手动打点
        /// <summary>
        /// 手动打点功能接口函数，无坐标信息
        /// </summary>
        /// <param name="oParm"></param>
        public void linkAddDataWithoutLocation(string oParm)
        {
            bool bSuccess = PFApp.Extent.Element("DataTools") != null ? link_addData_NoLocation_MainFrame(oParm) : link_addData_NoLocation1(oParm);
            if (!bSuccess)
                link_addData_NoLocation1(oParm);
        }

        public bool link_addData_NoLocation_MainFrame(string ts)
        {
            datatools = new DataTools();
            datatools.Show("打点",ts);
            return datatools.bDisable;
        }

        //无坐标打点，双击定位
        public bool link_addData_NoLocation1(string ts)
        {
            m_wxyid = ts.Split('|')[0];
            m_wxytype = ts.Split('|')[1];
            m_dwdm = ts.Split('|')[2];
            m_remark = ts.Split('|')[3];
            m_street = ts.Split('|')[4];
            m_enttype = ts.Split('|')[5];

            //在地图上点击获取坐标值
            if (mainmap == null)
            {
                mainmap = (Application.Current as IApp).MainMap;
            }
            //去除双击放大,
            mainmap.ZoomFactor = 3;
            mainmap.MouseClick -= mainmap_MouseClick;

            mainmap.MouseClick += new EventHandler<Map.MouseEventArgs>(mainmap_MouseClick);
            return true;
        }


        MapPoint startPoint;
        //和双击略有区别，用户在同一点上点两下
        void mainmap_MouseClick(object sender, Map.MouseEventArgs e)
        {
            if (startPoint == null)
                startPoint = e.MapPoint;
            else
            {
                if (e.MapPoint.X.ToString("f4") == startPoint.X.ToString("f4") && e.MapPoint.Y.ToString("f4") == startPoint.Y.ToString("f4"))
                {
                    //一次操作放大层级
                    //mainmap.ZoomFactor = 2;
                    mainmap.MouseClick -= mainmap_MouseClick;
                    addPoint2DB(e.MapPoint);
                }
                else
                    startPoint = e.MapPoint;
            }
        }

        void addPoint2DB(MapPoint pt)
        {
            m_x = pt.X.ToString("f4");
            m_y = pt.Y.ToString("f4");

            if (AykjClientInner == null)
            {
                #region//20120809zc:读取数据服务信息
                xele = PFApp.Extent;
                var dataServices = (from item in xele.Element("DataServices").Elements("DataService")
                                    where item.Attribute("Name").Value == "专题数据"
                                    select new
                                    {
                                        Type = item.Attribute("Type").Value,
                                        Url = item.Attribute("Url").Value,
                                    }).ToList();

                AykjClientInner = new AykjDataServiceInner.AykjDataClient(new BasicHttpBinding(), new EndpointAddress(dataServices[0].Url));

                #endregion

            }

            if (AykjClientInner != null)
            {
                try
                {
                    AykjClientInner.addNewSqlDataCompleted -= AykjClientInner_addNewSqlDataCompleted;
                    AykjClientInner.addNewSqlDataCompleted += new EventHandler<AykjDataServiceInner.addNewSqlDataCompletedEventArgs>(AykjClientInner_addNewSqlDataCompleted);

                    AykjClientInner.addNewSqlDataAsync(m_wxyid, m_wxytype, m_dwdm, "", m_remark, m_x, m_y, m_street, m_enttype);
                }
                catch (Exception e1)
                {
                    AYKJ.GISDevelop.Platform.ToolKit.Message.Show("无坐标专题数据打点异常");
                }
            }
        }

        /// <summary>
        /// 外部接口函数，无坐标信息打点
        /// </summary>
        /// <param name="ts">业务系统传入的规定格式的值（包含四个主要参数）</param>
        public void link_addData_NoLocation(string ts)
        {
            m_wxyid = ts.Split('|')[0];
            m_wxytype = ts.Split('|')[1];
            m_dwdm = ts.Split('|')[2];
            m_remark = ts.Split('|')[3];

            //在地图上点击获取坐标值
            if (mainmap == null)
            {
                mainmap = (Application.Current as IApp).MainMap;
            }

            if (MyThisDrawZoom == null)
            {
                MyThisDrawZoom = new Draw(mainmap);
            }

            MyThisDrawZoom.DrawMode = DrawMode.Point;
            MyThisDrawZoom.DrawComplete -= MyThisDrawZoom_DrawComplete;
            MyThisDrawZoom.DrawComplete += new EventHandler<DrawEventArgs>(MyThisDrawZoom_DrawComplete);
            MyThisDrawZoom.IsEnabled = true;

        }

        void MyThisDrawZoom_DrawComplete(object sender, DrawEventArgs e)
        {
            MyThisDrawZoom.IsEnabled = false;
            MyThisDrawZoom = null;

            MapPoint pt = new MapPoint()
            {
                X = e.Geometry.Extent.XMax,
                Y = e.Geometry.Extent.YMax
            };

            m_x = pt.X.ToString("f4");
            m_y = pt.Y.ToString("f4");

            if (AykjClientInner == null)
            {
                #region//20120809zc:读取数据服务信息
                xele = PFApp.Extent;
                var dataServices = (from item in xele.Element("DataServices").Elements("DataService")
                                    where item.Attribute("Name").Value == "专题数据"
                                    select new
                                    {
                                        Type = item.Attribute("Type").Value,
                                        Url = item.Attribute("Url").Value,
                                    }).ToList();
                AykjClientInner = new AykjDataServiceInner.AykjDataClient(new BasicHttpBinding(), new EndpointAddress(dataServices[0].Url));

                #endregion

            }

            if (AykjClientInner != null)
            {
                try
                {
                    AykjClientInner.addNewSqlDataCompleted -= AykjClientInner_addNewSqlDataCompleted;
                    AykjClientInner.addNewSqlDataCompleted += new EventHandler<AykjDataServiceInner.addNewSqlDataCompletedEventArgs>(AykjClientInner_addNewSqlDataCompleted);

                    AykjClientInner.addNewSqlDataAsync(m_wxyid, m_wxytype, m_dwdm, "", m_remark, m_x, m_y, m_street, m_enttype);
                }
                catch (Exception e1)
                {
                    AYKJ.GISDevelop.Platform.ToolKit.Message.Show("无坐标专题数据打点异常");
                }
            }
        }
        #endregion

        #region 打点后台操作
        //添加数据完成
        void AykjClientInner_addNewSqlDataCompleted(object sender, AykjDataServiceInner.addNewSqlDataCompletedEventArgs e)
        {
            string rtnstr = "";
            if (e.Result.Contains("成功"))
            {
                AYKJ.GISDevelop.Platform.ToolKit.Message.Show("打点成功");

                if (mainmap == null)
                    mainmap = (Application.Current as IApp).MainMap;

                //在地图上标注
                Add_Image();
                rtnstr = "success:打点成功";
            }
            else if (e.Result.Contains("重复"))
            {
                AYKJ.GISDevelop.Platform.ToolKit.Message.Show("数据重复，请检查（编号，代码，类型）是否已存在完全相同的记录");
                rtnstr = "false:数据重复，请检查（编号，代码，类型）是否已存在完全相同的记录";
            }
            else
            {
                AYKJ.GISDevelop.Platform.ToolKit.Message.Show("打点失败");
                rtnstr = "false:打点失败,无数据返回";
            }
            ScriptObject addPointFinished = HtmlPage.Window.GetProperty("addPointFinished") as ScriptObject;
            if(addPointFinished!=null)
                addPointFinished.InvokeSelf(rtnstr);
        }

        //打点的显示函数
        public void Add_Image()
        {
            try
            {
                string tmp_wxyid = m_wxyid;
                string tmp_wxytype = m_wxytype;
                string tmp_dwdm = m_dwdm;
                string tmp_remark = m_remark;
                string tmp_x = m_x;
                string tmp_y = m_y;
                string tmp_street = m_street;
                string tmp_enttype = m_enttype;

                //此处因为数据库中表的字段中不存在传感器类型对应的中文，将导致无法正常设置此处图层名，因此替换此处的tmp_cntype赋值方式。
                string tmp_cntype = (Application.Current as IApp).DictThematicEnCn[tmp_wxytype];

                
                if (!(Application.Current as IApp).dict_thematic.Keys.ToArray().Contains(tmp_cntype))
                {
                    #region 获取专题数据
                    List<clsThematic> lsttmp = new List<clsThematic>();
                    lsttmp.Add(new clsThematic()
                    {
                        str_wxyid = tmp_wxyid,
                        str_wxytype = tmp_wxytype,
                        str_cntype = tmp_cntype,
                        str_x = tmp_x,
                        str_y = tmp_y,
                        str_dwdm = tmp_dwdm,
                        str_remark = tmp_remark,
                        str_street = tmp_street,
                        str_enttype = tmp_enttype

                    });
                    (Application.Current as IApp).dict_thematic.Add(tmp_cntype, lsttmp);
                    #endregion

                    #region 将专题加到地图上去
                    Graphic tmpgra = new Graphic();
                    MapPoint mp = new MapPoint()
                    {
                        X = double.Parse(tmp_x),
                        Y = double.Parse(tmp_y),
                        SpatialReference = mainmap.SpatialReference
                    };
                    tmpgra.Geometry = mp;
                    tmpgra.Attributes.Add("StaRemark", tmp_remark);
                    tmpgra.Attributes.Add("StaSource", "/Image/DataImages/" + tmp_wxytype + ".png");
                    tmpgra.Attributes.Add("StaTag", tmp_wxytype + "|" + tmp_wxyid + "|" + tmp_dwdm + "|" + tmp_remark + "|" + tmp_street + "|" + tmp_enttype);
                    tmpgra.Attributes.Add("StaState", Visibility.Collapsed);

                    tmpgra.Symbol = ThematicSymbol;

                    //此处只需增加即可，因为初始化数据时，图层已经根据配置文件设置完整
                    (Application.Current as IApp).Dict_ThematicLayer[tmp_cntype].Graphics.Add(tmpgra);

                    (Application.Current as IApp).lstThematic.Add(tmpgra);
                    #endregion
                }
                else
                {
                    #region 获取专题数据
                    List<clsThematic> lsttmp = (Application.Current as IApp).dict_thematic[tmp_cntype];
                    lsttmp.Add(new clsThematic()
                    {
                        str_wxyid = tmp_wxyid,
                        str_wxytype = tmp_wxytype,
                        str_x = tmp_x,
                        str_y = tmp_y,
                        str_dwdm = tmp_dwdm,
                        str_remark = tmp_remark,
                        str_street = tmp_street,
                        str_cntype = tmp_cntype
                    });
                    (Application.Current as IApp).dict_thematic.Remove(tmp_cntype);
                    (Application.Current as IApp).dict_thematic.Add(tmp_cntype, lsttmp);
                    #endregion

                    #region 将专题加到地图上去
                    Graphic tmpgra = new Graphic();
                    MapPoint mp = new MapPoint()
                    {
                        X = double.Parse(tmp_x),
                        Y = double.Parse(tmp_y),
                        SpatialReference = mainmap.SpatialReference
                    };
                    tmpgra.Geometry = mp;
                    tmpgra.Attributes.Add("StaRemark", tmp_remark);
                    tmpgra.Attributes.Add("StaSource", "/Image/DataImages/" + tmp_wxytype + ".png");
                    tmpgra.Attributes.Add("StaTag", tmp_wxytype + "|" + tmp_wxyid + "|" + tmp_dwdm + "|" + tmp_remark + "|" + tmp_street + "|" + tmp_enttype);
                    tmpgra.Attributes.Add("StaState", Visibility.Collapsed);

                    tmpgra.Symbol = ThematicSymbol;

                    //此处只需增加即可，因为初始化数据时，图层已经根据配置文件设置完整
                    (Application.Current as IApp).Dict_ThematicLayer[tmp_cntype].Graphics.Add(tmpgra);

                    (Application.Current as IApp).lstThematic.Add(tmpgra);
                    #endregion
                }

                //高亮定位图层
                if (mainmap.Layers["selectHigh_Layer_circle"] != null)
                {
                    selectHigh_GraLayer = mainmap.Layers["selectHigh_Layer_circle"] as GraphicsLayer;
                    selectHigh_GraLayer.Graphics.Clear();
                }
                else
                {
                    selectHigh_GraLayer = new GraphicsLayer();
                    selectHigh_GraLayer.ID = "selectHigh_Layer_circle";
                    mainmap.Layers.Add(selectHigh_GraLayer);
                }


                Graphic gra = new Graphic();
                gra.Geometry = new MapPoint()
                {
                    X = double.Parse(tmp_x),
                    Y = double.Parse(tmp_y),
                    SpatialReference = mainmap.SpatialReference
                };
                gra.Symbol = HighMarkerStyle;
                selectHigh_GraLayer.Graphics.Add(gra);

                Envelope eve = new Envelope()
                {
                    XMax = gra.Geometry.Extent.XMax + 0.000001,
                    YMax = gra.Geometry.Extent.YMax + 0.000001,
                    XMin = gra.Geometry.Extent.XMin - 0.000001,
                    YMin = gra.Geometry.Extent.YMin - 0.000001
                };
                mainmap.ZoomTo(eve);

            }
            catch (Exception e)
            {
            }

        }
        #endregion

        #region 定位
        /// <summary>
        /// 定位功能
        /// </summary>
        /// <param name="oParm"></param>
        public void linkGetLocation(string oParm)
        {
            //改变数据顺序 wxyid + "|" + wxytype + "|" + dwdm + "|" + remark，适应此页面的定位函数的参数
            string o_id = oParm.Split('|')[0];
            string o_type = oParm.Split('|')[1];
            string o_dwdm = oParm.Split('|')[2];
            string o_remark = oParm.Split('|')[3];

            string i_parm = o_type + "|" + o_id + "|" + o_dwdm + "|" + o_remark;

            locationData(i_parm);
        }

        /// <summary>
        /// 定位的实现方法,注意s中各种数据的顺序
        /// </summary>
        /// <param name="s"></param>
        public void locationData(string s)
        {
            m_wxytype = s.Split('|')[0];
            m_wxyid = s.Split('|')[1];
            m_dwdm = s.Split('|')[2];
            m_remark = s.Split('|')[3];

            if (AykjClientInner == null)
            {
                xele = PFApp.Extent;
                var dataServices = (from item in xele.Element("DataServices").Elements("DataService")
                                    where item.Attribute("Name").Value == "专题数据"
                                    select new
                                    {
                                        Type = item.Attribute("Type").Value,
                                        Url = item.Attribute("Url").Value,
                                    }).ToList();

                AykjClientInner = new AykjDataServiceInner.AykjDataClient(new BasicHttpBinding(), new EndpointAddress(dataServices[0].Url));

            }
            if (AykjClientInner != null)
            {
                
                AykjClientInner.getDataLocationCompleted -= AykjClientInner_getDataLocationCompleted;
                 //AykjClientInner.getDataLocationCompleted -= AykjClientInner_getDataLocationCompleted;                                                                                                                    AykjClientInner_getDataCompleted
                AykjClientInner.getDataLocationCompleted += new EventHandler<AykjDataServiceInner.getDataLocationCompletedEventArgs>(AykjClientInner_getDataLocationCompleted);

                AykjClientInner.getDataLocationAsync(m_wxyid, m_wxytype, m_dwdm);
            }
        }
        #endregion

        #region 20150617zc:判断某点在定位时应使用哪一种缩放分辨率
        public class cls_PmtService
        {
            public string Name { get; set; }
            public string Resolution { get; set; }
            public bool Visible { get; set; }
            public string Bounds { get; set; }
        }
        double CheckResolution(MapPoint mp)
        {
            //默认的底图缩放分辨率
            double res = 1;
            //获取配置文件中所有企业平面图的范围
            try
            {
                var arr_PmtService = (from item in xele.Element("PmtServices").Elements("PmtService")
                                      select new
                                      {
                                          Name = item.Attribute("name").Value,
                                          Resolution = item.Attribute("resolution").Value,
                                          Visible = item.Attribute("visible").Value,
                                          Bounds = item.Attribute("bounds").Value,
                                      }).ToList();
                if (arr_PmtService == null) return res;
                if (arr_PmtService.Count == 0) return res;
                
                list_PmtService = new List<cls_PmtService>();
                foreach (var tmp_PmtService in arr_PmtService)
                {
                    cls_PmtService c_pmt = new cls_PmtService();
                    c_pmt.Name = tmp_PmtService.Name;
                    c_pmt.Resolution = tmp_PmtService.Resolution;
                    c_pmt.Visible = (tmp_PmtService.Visible.Equals("true")) ? true : false;
                    c_pmt.Bounds = tmp_PmtService.Bounds;
                    list_PmtService.Add(c_pmt);
                }
            }
            catch (Exception e1)
            {
                Message.ShowError("解析配置plat.xml文件中的节点<PmtServices>发生错误，无法初始化正确的企业平面图信息", "解析错误");
            }

            //判断该点是否在所有平面图范围内
            list_polygon = new List<Polygon>();
            for (int k = 0; k < list_PmtService.Count;k++ )
            {
                cls_PmtService PmtService  = list_PmtService[k];

                ESRI.ArcGIS.Client.Geometry.Polygon p = new Polygon();

                string s = PmtService.Bounds;
                List<MapPoint> list_point = new List<MapPoint>();
                foreach (string po in s.Split('|'))
                {
                    list_point.Add(new MapPoint(Double.Parse(po.Split(',')[0]), double.Parse(po.Split(',')[1]), mainmap.SpatialReference));
                }
                ESRI.ArcGIS.Client.Geometry.PointCollection pocol = new ESRI.ArcGIS.Client.Geometry.PointCollection(list_point);
                p.Rings.Add(pocol);

                if (clsrangequery == null)
                {
                    clsrangequery = new clsRangeQuery();
                    clsrangequery.RangeQueryEvent -= clsrangequery_RangeQueryEvent;
                    clsrangequery.RangeQueryEvent += new RangeQueryDelegate(clsrangequery_RangeQueryEvent);
                    clsrangequery.RangeQueryFaildEvent -= clsrangequery_RangeQueryFaildEvent;
                    clsrangequery.RangeQueryFaildEvent += new RangeQueryDelegate(clsrangequery_RangeQueryFaildEvent);
                }

                list_polygon.Add(p);
            }
            clsrangequery.MutilRangeQuery(list_polygon, mp);

            return res;
        }
        #endregion

        #region 定位功能实现
        void AykjClientInner_getDataLocationCompleted(object sender, AykjDataServiceInner.getDataLocationCompletedEventArgs e)
        {
            if (e.Result.Contains("成功"))
            {
                MapPoint mp = new MapPoint(
                    Double.Parse(e.Result.ToString().Split(',')[1]),
                    Double.Parse(e.Result.ToString().Split(',')[2])
                );
                if (mLink)
                {
                    switch (iAction)
                    {
                        case "定位"://20121011:新增“定位”分支代码
                            if (mainmap.Layers["selectHigh_Layer_circle"] != null)
                            {
                                selectHigh_GraLayer = mainmap.Layers["selectHigh_Layer_circle"] as GraphicsLayer;
                                selectHigh_GraLayer.Graphics.Clear();
                            }
                            else
                            {
                                selectHigh_GraLayer = new GraphicsLayer();
                                selectHigh_GraLayer.ID = "selectHigh_Layer_circle";
                                if (mainmap == null)
                                {
                                    mainmap = (Application.Current as IApp).MainMap;
                                }
                                mainmap.Layers.Add(selectHigh_GraLayer);
                            }

                            //高亮显示
                            Graphic gra = new Graphic();
                            gra.Geometry = new MapPoint(mp.X, mp.Y);
                            gra.Symbol = HighMarkerStyle;
                            selectHigh_GraLayer.Graphics.Add(gra);
                            Envelope eve = new Envelope()
                            {
                                XMax = gra.Geometry.Extent.XMax + 0.000001,
                                YMax = gra.Geometry.Extent.YMax + 0.000001,
                                XMin = gra.Geometry.Extent.XMin - 0.000001,
                                YMin = gra.Geometry.Extent.YMin - 0.000001
                            };
                            //mainmap.ZoomTo(eve);
                            CheckResolution(mp);
                            break;
                    }
                }
                //else
                //{
                //    if (listInfoWin == null) listInfoWin = new List<InfoWindow>();
                //    Dictionary<string, string> dict_Show = new Dictionary<string, string>();
                //    //根据实际情况配置                        
                //    dict_Show.Add("dataInfo", strImg);
                //    dict_Show.Add("dataClose", "s+" + strImg);
                //    InfoWindow s_window = new InfoWindow()
                //    {
                //        Name = "s+" + strImg,
                //        MaxHeight = 300,
                //        MaxWidth = 300,
                //        Style = this.Resources["InfoWindowStyle1"] as Style,
                //        Map = mainmap,
                //        Anchor = mp,
                //        IsOpen = true,
                //        ContentTemplate = LayoutRoot.Resources["InfoWindowSelectTemplate"] as System.Windows.DataTemplate,
                //        Content = dict_Show
                //    };
                //    bool bt = true;
                //    foreach (UIElement u in (Application.Current as IApp).mapLayoutRoot.Children)
                //    {
                //        string t = u.GetType().Name;
                //        if (t == "InfoWindow")
                //        {
                //            InfoWindow iw = u as InfoWindow;
                //            if (iw.Name == s_window.Name) bt = false;
                //        }
                //    }
                //    if (bt)
                //    {
                //        (Application.Current as IApp).mapLayoutRoot.Children.Add(s_window);
                //        listInfoWin.Add(s_window);
                //    }
                //}
            }
            else
            {
                mLink = false;
                Message.Show("坐标查询失败，请检查服务是否正确");
                ScriptObject locPointFinished = HtmlPage.Window.GetProperty("locPointFinished") as ScriptObject;
                locPointFinished.InvokeSelf("false");
                return;
            }
        }
        #endregion

        #region 专题数据图片加载触发事件
        private void Image_Loaded(object sender, RoutedEventArgs e)
        {
            Image i = sender as Image;
            bool b = true;
            if ((Application.Current as IApp).syslistthisimg != null && (Application.Current as IApp).syslistthisimg.Count > 0)
            {
                foreach (Image img in (Application.Current as IApp).syslistthisimg)
                {
                    if (i == img) b = false;
                }
            }
            if (b)
            {
                if ((Application.Current as IApp).syslistthisimg == null)
                {
                    (Application.Current as IApp).syslistthisimg = new List<Image>();
                }
                (Application.Current as IApp).syslistthisimg.Add(i);
            }
        }
        #endregion

        #region  弹出气泡相关触发函数
        private void tpl_btnDetail_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            string tag = btn.Tag.ToString();
            Message.Show(tag);
        }

        private void tpl_btnRealVideo_Click(object sender, RoutedEventArgs e)
        {
            //弹出新窗体
            HtmlPopupWindowOptions option = new HtmlPopupWindowOptions();
            option.Directories = false;//是否开启ie地址栏
            option.Height = 300;//浏览器窗口高度
            option.Width = 300;//浏览器窗口宽度
            option.Status = true;//状态栏是否可见
            option.Location = true;//是否弹出窗口
            option.Menubar = true;//菜单栏是否可见
            option.Resizeable = true;//是否可调整窗口高宽度
            option.Scrollbars = true;//滚动条是否可见
            option.Toolbar = true;//工具栏是否可见
            option.Left = option.Width / 2;//窗口的X坐标
            option.Top = option.Height / 2;//窗口的Y坐标

            //读取配置文件（20121217演示专用）
            var urlList = (from item in xele.Element("showVideo").Elements("url")
                           select new
                           {
                               value = item.Attribute("value").Value,
                               name = item.Attribute("name").Value,
                           }).ToList();
            string[] ary_url = urlList[0].value.Split('|');

            //方式1(不适用于引用外部（跨域）xap包)
            //HtmlPage.PopupWindow(new Uri(ary_url[0]), "_blank", option);
            //方式2(不适用于引用外部（跨域）xap包)
            //System.Windows.Browser.HtmlPage.Window.Navigate(new Uri("http://www.baidu.com"), "blank", "fullscreen=yes,channelmode=no");

            //20121217:(适用于引用外部（跨域）xap包)
            HtmlPage.Window.Invoke("fnCapture", "Capture.bmp");
        }

        private void tpl_btnRealData_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            string s = btn.Tag.ToString();

            //弹出系统内页面
            ChildRealTime childpage = new ChildRealTime();
            childpage.Title = "查看详细信息";
            childpage.Show();

            childpage.initData(s);
        }

        //关闭弹出气泡
        private void tpl_btnClose_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            string tag = btn.Tag.ToString();

            foreach (InfoWindow iw in listInfoWin)
            {
                if (iw.Name == tag)
                {
                    (Application.Current as IApp).mapLayoutRoot.Children.Remove(iw);
                    listInfoWin.Remove(iw);
                    break;
                }
            }
        }
        #endregion

        #region 界面打开关闭接口信息
        public void Open()
        {
            Show();
        }

        public event IWidgetEventHander OpenEnd;

        public event PartEventHander ReInitEnd;

        public event PartEventHander ShowEnd;

        public event PartEventHander CloseEnd;

        public event PartEventHander LinkGisPlatformEnd;

        public bool IsOpen
        {
            get { throw new NotImplementedException(); }
        }

        public PartDescriptor Descri
        {
            get { return new PartDescriptor() { Name = "AYKJ.GISInterface" }; }
        }

        public void ReInit()
        {
            throw new NotImplementedException();
        }

        public void Show()
        { }

        public void Close()
        { }

        #endregion

        /// <summary>
        /// 清除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ClearReset()
        {
            Map map = (Application.Current as IApp).MainMap;

            foreach (var item in map.Layers)
            {
                if ((item is GraphicsLayer) && !(Application.Current as IApp).Dict_ThematicLayer.Keys.ToList().Contains(item.ID))
                {
                    (item as GraphicsLayer).ClearGraphics();
                }
            }
            map.Cursor = Cursors.Arrow;
        }
    }

    public class clstipwxy
    {
        public string wxyid { set; get; }
        public string wxyname { set; get; }
        public string wxytip { set; get; }
        public string wxytype { set; get; }
        public string wxydwdm { set; get; }
    }
}
