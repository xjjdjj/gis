using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Linq;
using AYKJ.GISDevelop.Control;
using AYKJ.GISDevelop.Platform;
using AYKJ.GISDevelop.Platform.Part;
using AYKJ.GISDevelop.Platform.ToolKit;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Toolkit;
using System.Windows.Messaging;
using Divelements.SilverlightTools;

namespace AYKJ.GISDevelop
{
    public partial class MainPage : UserControl, IMainPage
    {
        BarArea bararea;
        //截图文件路径
        string pathCaptureImg;
        GraphicsLayer ShowBusiness_Layer;
        #region zc打点定位查询接口变量定义
        //加载数据服务
        private AykjDataServiceInner.AykjDataClient AykjClientInner;
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
        //Image控件列表
        private static List<Image> listThisImg;
        //定时器
        static DispatcherTimer timer_m = null;
        //Map
        Map mainmap;
        string strImg;
        List<InfoWindow> listInfoWin;

        string imgunload = "/Image/unload.jpg";
        string swfdefault = "/swf/1.swf";
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

        public MainPage()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
            ShowBusiness_Layer = new GraphicsLayer();
            ShowBusiness_Layer.ID = "ShowBusiness_Layer";
            if (mainmap == null)
                mainmap = (Application.Current as IApp).MainMap;

            #region//20121217:数据处理
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

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            App.Root = CRoot;
            App.SpRoot = DRoot;
            //CRoot.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            //CRoot.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            //CRoot.Margin = new Thickness() ;//{}(0, 0, 0, menuBar.ActualHeight + 1);

            //调用内部工具条页面
            HtmlPage.RegisterScriptableObject("Page", this);
            bararea = new BarArea();
            //20120730zc:从配置文件读取截图存储路径
            try
            {
                XElement xele = PFApp.Extent;
                var path = (from item in xele.Element("CapturPath").Elements("path")
                            select new
                            {
                                Value = item.Attribute("Dir").Value,
                            }).ToList();
                pathCaptureImg = path[0].Value;
            }
            catch (Exception ex)
            { }

            //20120912
            (Application.Current as IApp).strImageType = "defaultClick";
        }

        #region 页面事件响应
        void btn_gra_close_Click(object sender, RoutedEventArgs e)
        {
            App.mainMap.Layers.Remove(ShowBusiness_Layer);
        }

        void btn_MNL_Click(object sender, RoutedEventArgs e)
        {
            string[] ary_str = strImg.Split('|');
            string result = "{'module':'mnl','wxytype':'" + ary_str[0] + "','wxyid':'" + ary_str[1] + "','dwdm':'" + ary_str[2] + "'}";
            rtnToBussinesspage("rtnMoNiLiangFromSL", result);
        }

        void btn_JY_Click(object sender, RoutedEventArgs e)
        {
            var urlList = (from item in xele.Element("ShowPages").Elements("ShowPage")
                           where item.Attribute("Name").Value == "yuan"
                           select new
                           {
                               Url = item.Attribute("Url").Value,
                               Name = item.Attribute("Name").Value,
                           }).ToList();
            if (urlList.Count != 0)
            {
                string[] ary_str = strImg.Split('|');
                if (ary_str[2].Trim() == "")
                    return;
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
                string str_url = urlList[0].Url;

                //方式1(不适用于引用外部（跨域）xap包)
                //HtmlPage.PopupWindow(new Uri(ary_url[0]), "_blank", option);
                //方式2(不适用于引用外部（跨域）xap包)
                HtmlPage.Window.Navigate(new Uri(str_url), "blank");
            }
        }

        void btn_PMT_Click(object sender, RoutedEventArgs e)
        {
            var urlList = (from item in xele.Element("ShowPages").Elements("ShowPage")
                           where item.Attribute("Name").Value == "PMT"
                           select new
                           {
                               Url = item.Attribute("Url").Value,
                               Name = item.Attribute("Name").Value,
                           }).ToList();
            if (urlList.Count != 0)
            {
                string[] ary_str = strImg.Split('|');
                if (ary_str[2].Trim() == "")
                    return;
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
                string str_url = urlList[0].Url + "?id=" + ary_str[1];

                //方式1(不适用于引用外部（跨域）xap包)
                //HtmlPage.PopupWindow(new Uri(ary_url[0]), "_blank", option);
                //方式2(不适用于引用外部（跨域）xap包)
                HtmlPage.Window.Navigate(new Uri(str_url), "blank");
            }
        }

        SwfWindow sw;

        double x, y;

        //当前版本不使用此方法打开视频链接

        void btn_SP_Click(object sender, RoutedEventArgs e)
        {
            string[] ary_str = strImg.Split('|');
            string result = "{'module':'sp','wxytype':'" + ary_str[0] + "','wxyid':'" + ary_str[1] + "','dwdm':'" + ary_str[2] + "'}";
            rtnToBussinesspage("rtnShiPinFromSL", result);
        }

        void btn_XX_Click(object sender, RoutedEventArgs e)
        {
            var urlList = (from item in xele.Element("ShowPages").Elements("ShowPage")
                           where item.Attribute("Name").Value == "enterprise"
                           select new
                           {
                               Url = item.Attribute("Url").Value,
                               Name = item.Attribute("Name").Value,
                           }).ToList();
            if (urlList.Count != 0)
            {
                string[] ary_str = strImg.Split('|');
                if (ary_str[2].Trim() == "")
                    return;
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
                string str_url = urlList[0].Url + "?id=" + ary_str[1];

                //方式1(不适用于引用外部（跨域）xap包)
                //HtmlPage.PopupWindow(new Uri(ary_url[0]), "_blank", option);
                //方式2(不适用于引用外部（跨域）xap包)
                HtmlPage.Window.Navigate(new Uri(str_url), "blank");
            }
        }

        #endregion

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


        #region//定位功能实现
        void AykjClientInner_getDataLocationCompleted(object sender, AykjDataServiceInner.getDataLocationCompletedEventArgs e)
        {
            if (e.Result.Contains("成功"))
            {
                string[] results = e.Result.ToString().Split(',');
                MapPoint mp = new MapPoint(
                    Double.Parse(results[1]),
                    Double.Parse(results[2])
                );
                mp.SpatialReference = mainmap.SpatialReference;
                string[] ary_str = strImg.Split('|');
                if (strImg.Contains("enterprise"))
                {                   
                    #region//20130926:查询企业位置，在wcf配置文件中含有独立查询语句的。
                    //"成功:查询的数据位置," + x + "," + y+","+name+","+add+","+lp+","+phone;
                    try
                    {
                        ShowBusiness_Layer.Graphics.Clear();
                        bool IsExit = false;
                        foreach (Layer layer in mainmap.Layers)
                        {
                            if (layer.ID == "ShowBusiness_Layer")
                                IsExit = true;
                        }
                        //string[] ary_identify = e.Result.ToString().Split(',')[2].Split('|');
                        Graphic tmpgra = new Graphic();
                        tmpgra.Geometry = mp;
                        tmpgra.Attributes.Add("WxySource", "/Image/DataImages/" + ary_str[0] + ".png");
                        tmpgra.Attributes.Add("StaName", results[3]);
                        tmpgra.Attributes.Add("StaAddress", results[4]);
                        tmpgra.Attributes.Add("StaPerson", results[5]);
                        tmpgra.Attributes.Add("StaPhone", results[6]);
                        var urlList = (from item in xele.Element("ShowPages").Elements("ShowPage")
                                       where item.Attribute("Name").Value == "GIS_IMAGE"
                                       select new
                                       {
                                           Url = item.Attribute("Url").Value,
                                           Name = item.Attribute("Name").Value,
                                           Default = item.Attribute("DefaultUrl").Value,
                                       }).ToList();
                        if (urlList.Count != 0)
                        {
                            string strimgurl = "";
                            string mediaurl = "";
                            //获取静态图片地址
                            var list = (from item in xele.Element("ShowVideo").Elements("Url")
                                        where item.Attribute("id").Value == ary_str[1]
                                        select new
                                        {
                                            SimulateUrl = item.Attribute("SimulateUrl").Value,
                                            VideoUrl = item.Attribute("VideoUrl").Value,
                                        }).ToList();
                            if (list.Count > 0)
                            {
                                strimgurl = list[0].SimulateUrl;
                                mediaurl = list[0].VideoUrl;
                            }

                            if (strimgurl == null || strimgurl == "")
                                strimgurl = imgunload;
                            if (mediaurl == null || mediaurl == "")
                                mediaurl = swfdefault;
                            tmpgra.Attributes.Add("PmtSource", strimgurl);

                            tmpgra.Attributes.Add("MediaSource", mediaurl);
                        }
                        tmpgra.Symbol = BusinessSymbol;
                        Graphic tmpgra2 = new Graphic();
                        tmpgra2.Geometry = mp;
                        tmpgra2.Symbol = HighMarkerStyle;
                        ShowBusiness_Layer.Graphics.Add(tmpgra2);
                        ShowBusiness_Layer.Graphics.Add(tmpgra);
                        if (IsExit == false)
                        {
                            mainmap.Layers.Add(ShowBusiness_Layer);
                        }
                        else
                        {
                            mainmap.Layers.Remove(ShowBusiness_Layer);
                            mainmap.Layers.Insert(mainmap.Layers.Count, ShowBusiness_Layer);
                        }
                    }
                    catch (Exception ex)
                    {
                        Message.Show(ex.Message);
                    }
                    #endregion
                }
                #region 其他类型
                else if (strImg.Contains("accident"))
                {
                    var urlList = (from item in xele.Element("ShowPages").Elements("ShowPage")
                                   where item.Attribute("Name").Value == "accident"
                                   select new
                                   {
                                       Url = item.Attribute("Url").Value,
                                       Name = item.Attribute("Name").Value,
                                   }).ToList();
                    if (urlList.Count != 0)
                    {
                        OpenPage3(ary_str[1], urlList[0].Url);
                    }
                }
                else if (strImg.Contains("boiler"))
                {
                    var urlList = (from item in xele.Element("ShowPages").Elements("ShowPage")
                                   where item.Attribute("Name").Value == "boiler"
                                   select new
                                   {
                                       Url = item.Attribute("Url").Value,
                                       Name = item.Attribute("Name").Value,
                                   }).ToList();
                    if (urlList.Count != 0)
                    {
                        OpenPage1(ary_str[1], "Boiler", urlList[0].Url);
                    }
                }
                else if (strImg.Contains("cars"))
                {
                    var urlList = (from item in xele.Element("ShowPages").Elements("ShowPage")
                                   where item.Attribute("Name").Value == "cars"
                                   select new
                                   {
                                       Url = item.Attribute("Url").Value,
                                       Name = item.Attribute("Name").Value,
                                   }).ToList();
                    if (urlList.Count != 0)
                    {
                        OpenPage2(ary_str[1], "CarsResource", urlList[0].Url);
                    }
                }
                else if (strImg.Contains("colliery"))
                {
                    var urlList = (from item in xele.Element("ShowPages").Elements("ShowPage")
                                   where item.Attribute("Name").Value == "colliery"
                                   select new
                                   {
                                       Url = item.Attribute("Url").Value,
                                       Name = item.Attribute("Name").Value,
                                   }).ToList();
                    if (urlList.Count != 0)
                    {
                        OpenPage1(ary_str[1], "CollieryHazards", urlList[0].Url);
                    }
                }
                else if (strImg.Contains("communication"))
                {
                    //通讯保障 wu
                    var urlList = (from item in xele.Element("ShowPages").Elements("ShowPage")
                                   where item.Attribute("Name").Value == "communication"
                                   select new
                                   {
                                       Url = item.Attribute("Url").Value,
                                       Name = item.Attribute("Name").Value,
                                   }).ToList();
                    if (urlList.Count != 0)
                    {
                        //OpenPage(ary_str[1], ary_str[0], ary_str[2], urlList[0].Url);
                    }
                }
                else if (strImg.Contains("dangerousplace"))
                {
                    var urlList = (from item in xele.Element("ShowPages").Elements("ShowPage")
                                   where item.Attribute("Name").Value == "dangerousplace"
                                   select new
                                   {
                                       Url = item.Attribute("Url").Value,
                                       Name = item.Attribute("Name").Value,
                                   }).ToList();
                    if (urlList.Count != 0)
                    {
                        OpenPage1(ary_str[1], "dangerplace", urlList[0].Url);
                    }
                }
                else if (strImg.Contains("expert"))
                {
                    var urlList = (from item in xele.Element("ShowPages").Elements("ShowPage")
                                   where item.Attribute("Name").Value == "expert"
                                   select new
                                   {
                                       Url = item.Attribute("Url").Value,
                                       Name = item.Attribute("Name").Value,
                                   }).ToList();
                    if (urlList.Count != 0)
                    {
                        OpenPage2(ary_str[1], "rescueExpert", urlList[0].Url);
                    }
                }
                else if (strImg.Contains("gov_jy_wzck"))
                {
                    //物资仓库
                    var urlList = (from item in xele.Element("ShowPages").Elements("ShowPage")
                                   where item.Attribute("Name").Value == "gov_jy_wzck"
                                   select new
                                   {
                                       Url = item.Attribute("Url").Value,
                                       Name = item.Attribute("Name").Value,
                                   }).ToList();
                    if (urlList.Count != 0)
                    {
                        //OpenPage(ary_str[1], ary_str[0], ary_str[2], urlList[0].Url);
                    }
                }


                else if (strImg.Contains("mdeical"))
                {
                    //医疗保障
                    var urlList = (from item in xele.Element("ShowPages").Elements("ShowPage")
                                   where item.Attribute("Name").Value == "mdeical"
                                   select new
                                   {
                                       Url = item.Attribute("Url").Value,
                                       Name = item.Attribute("Name").Value,
                                   }).ToList();
                    if (urlList.Count != 0)
                    {
                        //OpenPage(ary_str[1], ary_str[0], "", urlList[0].Url);
                    }
                }
                else if (strImg.Contains("mines"))
                {
                    var urlList = (from item in xele.Element("ShowPages").Elements("ShowPage")
                                   where item.Attribute("Name").Value == "mines"
                                   select new
                                   {
                                       Url = item.Attribute("Url").Value,
                                       Name = item.Attribute("Name").Value,
                                   }).ToList();
                    if (urlList.Count != 0)
                    {
                        OpenPage1(ary_str[1], "MinesHazards", urlList[0].Url);
                    }
                }
                else if (strImg.Contains("penstock"))
                {
                    var urlList = (from item in xele.Element("ShowPages").Elements("ShowPage")
                                   where item.Attribute("Name").Value == "penstock"
                                   select new
                                   {
                                       Url = item.Attribute("Url").Value,
                                       Name = item.Attribute("Name").Value,
                                   }).ToList();
                    if (urlList.Count != 0)
                    {
                        OpenPage1(ary_str[1], "TbHazhardsPenstock", urlList[0].Url);
                    }
                }
                else if (strImg.Contains("pressurevessel"))
                {
                    var urlList = (from item in xele.Element("ShowPages").Elements("ShowPage")
                                   where item.Attribute("Name").Value == "pressurevessel"
                                   select new
                                   {
                                       Url = item.Attribute("Url").Value,
                                       Name = item.Attribute("Name").Value,
                                   }).ToList();
                    if (urlList.Count != 0)
                    {
                        OpenPage1(ary_str[1], "PressureVesselHazards", urlList[0].Url);
                    }
                }
                else if (strImg.Contains("reservoirarea"))
                {
                    var urlList = (from item in xele.Element("ShowPages").Elements("ShowPage")
                                   where item.Attribute("Name").Value == "reservoirarea"
                                   select new
                                   {
                                       Url = item.Attribute("Url").Value,
                                       Name = item.Attribute("Name").Value,
                                   }).ToList();
                    if (urlList.Count != 0)
                    {
                        OpenPage1(ary_str[1], "ReservoirAreaHazards", urlList[0].Url);
                    }
                }
                else if (strImg.Contains("responseteam"))
                {
                    var urlList = (from item in xele.Element("ShowPages").Elements("ShowPage")
                                   where item.Attribute("Name").Value == "responseteam"
                                   select new
                                   {
                                       Url = item.Attribute("Url").Value,
                                       Name = item.Attribute("Name").Value,
                                   }).ToList();
                    if (urlList.Count != 0)
                    {
                        OpenPage2(ary_str[1], "rescueTeam", urlList[0].Url);
                    }
                }
                else if (strImg.Contains("shelter"))
                {
                    var urlList = (from item in xele.Element("ShowPages").Elements("ShowPage")
                                   where item.Attribute("Name").Value == "shelter"
                                   select new
                                   {
                                       Url = item.Attribute("Url").Value,
                                       Name = item.Attribute("Name").Value,
                                   }).ToList();
                    if (urlList.Count != 0)
                    {
                        OpenPage2(ary_str[1], "TbRescueShelter", urlList[0].Url);
                    }
                }
                else if (strImg.Contains("supplies"))
                {
                    //救援物资
                    var urlList = (from item in xele.Element("ShowPages").Elements("ShowPage")
                                   where item.Attribute("Name").Value == "supplies"
                                   select new
                                   {
                                       Url = item.Attribute("Url").Value,
                                       Name = item.Attribute("Name").Value,
                                   }).ToList();
                    if (urlList.Count != 0)
                    {
                        //OpenPage(ary_str[1], ary_str[0], ary_str[2], urlList[0].Url);
                    }
                }

                else if (strImg.Contains("tailingspond"))
                {
                    var urlList = (from item in xele.Element("ShowPages").Elements("ShowPage")
                                   where item.Attribute("Name").Value == "tailingspond"
                                   select new
                                   {
                                       Url = item.Attribute("Url").Value,
                                       Name = item.Attribute("Name").Value,
                                   }).ToList();
                    if (urlList.Count != 0)
                    {
                        OpenPage1(ary_str[1], "HazhardsTailingspond", urlList[0].Url);
                    }
                }
                else if (strImg.Contains("tank"))
                {
                    var urlList = (from item in xele.Element("ShowPages").Elements("ShowPage")
                                   where item.Attribute("Name").Value == "tank"
                                   select new
                                   {
                                       Url = item.Attribute("Url").Value,
                                       Name = item.Attribute("Name").Value,
                                   }).ToList();
                    if (urlList.Count != 0)
                    {
                        OpenPage1(ary_str[1], "TankAraeHazards", urlList[0].Url);
                    }
                }
                else if (strImg.Contains("technical"))
                {
                    //技术支持
                    var urlList = (from item in xele.Element("ShowPages").Elements("ShowPage")
                                   where item.Attribute("Name").Value == "technical"
                                   select new
                                   {
                                       Url = item.Attribute("Url").Value,
                                       Name = item.Attribute("Name").Value,
                                   }).ToList();
                    if (urlList.Count != 0)
                    {
                        //OpenPage(ary_str[1], ary_str[0], ary_str[2], urlList[0].Url);
                    }
                }
                else if (strImg.Contains("transport"))
                {
                    //运输保障
                    var urlList = (from item in xele.Element("ShowPages").Elements("ShowPage")
                                   where item.Attribute("Name").Value == "transport"
                                   select new
                                   {
                                       Url = item.Attribute("Url").Value,
                                       Name = item.Attribute("Name").Value,
                                   }).ToList();
                    if (urlList.Count != 0)
                    {
                        //OpenPage(ary_str[1], ary_str[0], "", urlList[0].Url);
                    }
                }
                else if (strImg.Contains("workplace"))
                {
                    var urlList = (from item in xele.Element("ShowPages").Elements("ShowPage")
                                   where item.Attribute("Name").Value == "workplace"
                                   select new
                                   {
                                       Url = item.Attribute("Url").Value,
                                       Name = item.Attribute("Name").Value,
                                   }).ToList();
                    if (urlList.Count != 0)
                    {
                        OpenPage1(ary_str[1], "TbHazhardsWorkplace", urlList[0].Url);
                    }
                }
                else if (strImg.Contains("yjgzwl"))
                {
                    //应急机构
                    var urlList = (from item in xele.Element("ShowPages").Elements("ShowPage")
                                   where item.Attribute("Name").Value == "yjgzwl"
                                   select new
                                   {
                                       Url = item.Attribute("Url").Value,
                                       Name = item.Attribute("Name").Value,
                                   }).ToList();
                    if (urlList.Count != 0)
                    {
                        //OpenPage(ary_str[1], ary_str[0], ary_str[2], urlList[0].Url);
                    }
                }
            }
#endregion
            else
            {
                mLink = false;
                Message.Show("坐标查询失败，请检查服务是否正确");
                return;
            }
        }

        void OpenPage(string str_url)
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
            HtmlPage.Window.Navigate(new Uri(str_url), "blank");
        }

        void OpenPage3(string id, string strurl)
        {
            if (id.Trim() == "" || strurl.Trim() == "")
                return;
            string str_url = strurl + "?id=" + id;
            OpenPage(str_url);
        }

        void OpenPage2(string resId, string nameSpace, string strurl)
        {
            if (resId.Trim() == "" || nameSpace.Trim() == "" || strurl.Trim() == "")
                return;
            string str_url = strurl + "?resId=" + resId + "&nameSpace=" + nameSpace;
            OpenPage(str_url);
        }

        void OpenPage1(string ids, string types, string strurl)
        {
            if (ids.Trim() == "" || types.Trim() == "" || strurl.Trim() == "")
                return;
            string str_url = strurl + "?ids=" + ids + "&types=" + types;
            OpenPage(str_url);
        }

        void OpenPage(string wxyid, string wxytype, string dwdm, string strurl)
        {
            if (wxyid.Trim() == "" || wxytype.Trim() == "" || strurl.Trim() == "")
                return;
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
            string str_url;
            if (dwdm.Trim() != "")
            {
                str_url = strurl + "?wxyid=" + wxyid + "&wxytype=" + wxytype + "&dwdm=" + dwdm;
            }
            else
            {
                str_url = strurl + "?wxyid=" + wxyid + "&wxytype=" + wxytype;
            }
            //方式1(不适用于引用外部（跨域）xap包)
            //HtmlPage.PopupWindow(new Uri(ary_url[0]), "_blank", option);
            //方式2(不适用于引用外部（跨域）xap包)
            HtmlPage.Window.Navigate(new Uri(str_url), "blank");
        }
        #endregion

        #region //弹出气泡相关触发函数
        private void tpl_btnDetail_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            string s = btn.Tag.ToString();
            //弹出系统内页面
            ChildDevelop childpage = new ChildDevelop();
            childpage.Title = "查看详细信息";
            childpage.Show();
            childpage.initData(s);
            btn.Tag = "s+" + btn.Tag;
            tpl_btnClose_Click(btn, null);
        }

        private void tpl_btnRealVideo_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            string s = btn.Tag.ToString();
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

            //读取配置文件
            //var urlList = (from item in xele.Element("ShowVideo").Elements("Url")
            //               where item.Attribute("name").Value == "危险源分布图"
            //               select new
            //               {
            //                   value = item.Attribute("value").Value,
            //                   name = item.Attribute("name").Value,
            //               }).ToList();
            //string str_url = urlList[0].value + "?wxyid="+s.Split('|')[1];

            string strid = s.Split('|')[1];
            var urlList = (from item in xele.Element("ShowVideo").Elements("Url")
                           where item.Attribute("name").Value == strid
                           select new
                           {
                               value = item.Attribute("value").Value,
                               name = item.Attribute("name").Value,
                           }).ToList();
            if (urlList.Count != 0)
            {
                string str_url = urlList[0].value;
                //方式1(不适用于引用外部（跨域）xap包)
                //HtmlPage.PopupWindow(new Uri(ary_url[0]), "_blank", option);
                //方式2(不适用于引用外部（跨域）xap包)
                HtmlPage.Window.Navigate(new Uri(str_url), "blank");
                btn.Tag = "s+" + btn.Tag;
                tpl_btnClose_Click(btn, null);
            }
            else
            {
                Message.Show("该企业尚未上传厂区平面图");
            }
        }

        private void tpl_btnRealData_Click(object sender, RoutedEventArgs e)
        {
            //20121217演示要求，暂时注释掉。
            Button btn = sender as Button;
            string tag = btn.Tag.ToString();
            //Message.Show(tag);
            var Explosion = (from item in (Application.Current as IApp).lstThematic
                             where item.Attributes["StaTag"].ToString().Contains(tag)
                             select new
                             {
                                 Name = item.Attributes["remark"],
                                 Graphic = item,
                             }).ToList();
            if (Explosion.Count != 0)
            {
                Graphic gra = Explosion[0].Graphic;
                Envelope eve = new Envelope()
                {
                    XMax = gra.Geometry.Extent.XMax + 0.000001,
                    YMax = gra.Geometry.Extent.YMax + 0.000001,
                    XMin = gra.Geometry.Extent.XMin - 0.000001,
                    YMin = gra.Geometry.Extent.YMin - 0.000001
                };
                mainmap.ZoomTo(eve);
            }
            btn.Tag = "s+" + btn.Tag;
            tpl_btnClose_Click(btn, null);
        }

        //关闭弹出气泡
        private void tpl_btnClose_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            //if (btn == null)
            //    return;
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


        #region//20120912：响应业务系统页面的调用并返回值

        public class clsDevelop
        {
            public string dwdm { get; set; }
            public string wxyid { get; set; }
            public string wxytype { get; set; }
            public string remark { get; set; }
            public string x { get; set; }
            public string y { get; set; }

        }
        //统一接口
        [ScriptableMember]
        public string linkFromAspxPage(string oAction, string oStr, object oArr, object oCls, object oArrStr, object oArrArr, object oArrCls)
        {
            //clsDevelop iAry = ((ScriptObject)(oCls)).ConvertTo<clsDevelop>();

            object[] new_oArrStr = ((ScriptObject)(oArrStr)).ConvertTo<object[]>();
            object[] new_oArrArr = ((ScriptObject)(oArrArr)).ConvertTo<object[]>();
            object[] new_oArrCls = ((ScriptObject)(oArrCls)).ConvertTo<object[]>();

            //找到含有接口的页面
            foreach (var item in App.Menus)
            {
                if (item.Type != "App")
                {
                    IPart myPart = PFApp.UIS[item.MenuName] as IPart;
                    string rs = myPart.LinkFromGiPlatform(oAction, oStr, oArr, oCls, new_oArrStr, new_oArrArr, new_oArrCls);
                }
            }
            return "ok";
        }
        #endregion        

        void myPart_LinkGisPlatformEnd(IPart sender)
        {
            //throw new NotImplementedException();
        }
        //20120723zc:响应Gis平台主页面AspxPage的调用并返回值
        [ScriptableMember]
        public string LinkGetDataFromThisAspxPage(string s1, string s2)
        {
            //MessageBox.Show("GetDataFromGisPlatformAspxPage:" + "调用" + s1 + "。传入值：" + s2);
            string rs = bararea.LinkGetDataFromThisMainPage(s1, s2);
            return rs;
        }

        //20120726zc:
        public void LinkControlPage()
        {
            //MessageBox.Show("cc");
            //20120730zc:以"Capture_+当前时间(YYYYMMRRHHMMSS)"为截图的名称
            string time = DateTime.Now.Year.ToString() + "" + DateTime.Now.Month.ToString("d2") + "" + DateTime.Now.Day.ToString("d2") + "-"
                + DateTime.Now.Hour.ToString("d2") + "" + DateTime.Now.Minute.ToString("d2") + "" + DateTime.Now.Second.ToString("d2");
            HtmlPage.Window.Invoke("fnCapture", pathCaptureImg + "Capture_" + time + ".bmp");
        }

        //将信息返回给aspx页面中的特定方法
        void rtnToBussinesspage(string func, string mess)
        {
            ScriptObject queryfinished = HtmlPage.Window.GetProperty(func) as ScriptObject;
            queryfinished.InvokeSelf(mess);
        }

        #region//20120727zc:权限问题，此处不用。
        [ScriptableMember]
        public void LinkSaveImg()
        {
            bararea.SaveImgFile();
        }
        #endregion

        /// <summary>
        /// 增加Map
        /// </summary>
        public void ShowMap()
        {
            //map.ShowNorMap();
            AddOverMap();//鹰眼
            //AddNav();//导航
            MapNavigation.Map = App.mainMap;
        }

        /// <summary>
        /// 增加鹰眼
        /// </summary>
        private void AddOverMap()
        {
            viewMap.Visibility = System.Windows.Visibility.Visible;
        }

        #region 接口函数
        public void AddToRight(IPart part)
        {
            throw new NotImplementedException();
        }

        public void RemoveFromRight(IPart part)
        {
            throw new NotImplementedException();
        }

        public void UpdateMenuButtonTo(string name)
        {
            throw new NotImplementedException();
        }

        public void CloseMenu(string name)
        {
            throw new NotImplementedException();
        }


        public double RightDistance(int VisulIndex)
        {
            return menuBar.RightDistance(VisulIndex);
        }

        private void btn_alert_login_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ScriptObject so = HtmlPage.Window.GetProperty("clo") as ScriptObject;
            so.InvokeSelf();
        }
        #endregion
    }
}
