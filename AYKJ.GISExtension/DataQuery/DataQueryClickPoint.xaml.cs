using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using AYKJ.GISDevelop.Platform;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using System.ServiceModel;
using System.ComponentModel;
using System.Windows.Threading;
using AYKJ.GISDevelop.Platform.ToolKit;

namespace AYKJ.GISExtension
{
    public partial class DataQueryClickPoint : UserControl
    {
        //定义一个Map用来接收平台的Map
        public static Map mainmap;

        #region//20120816zc:定义变量
        //2012080zc:
        public static List<Image> listThisClickImg;
        //点的信息
        private List<clsPoint> listPoint;
        //从配置文件读取对应名称
        private List<string> listName;
        
        //20120809zc：
        //加载数据服务
        private static AykjDataServiceInner.AykjDataClient AykjClientInner;
        //配置文件
        XElement xele;
        //高亮层
        public GraphicsLayer selectHigh_GraLayer;

        //20120816zc:从平台返回的点击对象的相关信息组合字符串
        private static string sReturn;
        //选中删除的数据信息
        string strDelete;
        Dictionary<string, GraphicsLayer> DictIn_layer;
        Dictionary<string, string> DictIn_EnCn;
        //
        string m_wxyid;
        string m_wxytype;
        string m_dwdm;
        string m_idkey;
        string m_remark;
        string m_x;
        string m_y;
        //打点操作
        private Draw MyThisDrawZoom;
        //MapPoint pt;
        clsPoint cccc;

        static DispatcherTimer timer_c = null; //定时器

        //动画
        public static WaitAnimationWindow waitanimationwindow;

        #endregion

        public DataQueryClickPoint()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(DataQueryClickPoint_Loaded);
        }

        void DataQueryClickPoint_Loaded(object sender, RoutedEventArgs e)
        {
            mainmap = (Application.Current as IApp).MainMap;

            //lblClickTag.Content = "";

            //存入专题数据库中
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

            //(Application.Current as IApp).strImageType = "defaultClick";

            //20130926：最新选项卡功能，只能删除
            //20131008:这个全局变量的设置在“数据管理”代码中统一管理，不再放在各个选项卡中设置。
            //(Application.Current as IApp).strImageType = "deleteData";

            //20131008:初始化高亮图层
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
            
            //openTimer();
        }

        #region//20120912：即时刷新数据

        public void openTimer()
        {
            if (timer_c == null)
            {
                timer_c = new DispatcherTimer();
                timer_c.Interval = TimeSpan.FromSeconds(2);
                timer_c.Tick += new EventHandler(timer_c_Chart_Tick);
            }
            if (!timer_c.IsEnabled)
                timer_c.Start();

            return;
        }

        //定时效果代码
        public void timer_c_Chart_Tick(object sender, EventArgs e)
        {
            //20121009
            timer_c.Stop();

            if ((Application.Current as IApp).syslistthisimg != null && (Application.Current as IApp).syslistthisimg.Count > 0)
            {
                foreach (Image i in (Application.Current as IApp).syslistthisimg)
                {
                    i.MouseLeftButtonUp -= new MouseButtonEventHandler(i_MouseLeftButtonUp);
                    i.MouseLeftButtonUp += new MouseButtonEventHandler(i_MouseLeftButtonUp);
                }
            }

            timer_c.Start();

            return;

        } 

        public class clsPoint
        {
            [Display(Name = "属性", GroupName = "clsPoint")]
            public string name { set; get; }
            [Display(Name = "值", GroupName = "clsPoint")]
            public string value { set; get; }
        }

        private void btn_clickPoint_Click(object sender, RoutedEventArgs e)
        {
            PFApp.ClickType = "searchData";
            //(Application.Current as IApp).strImageType = "searchData";
        }


        //在地图上点选删除对应的专题数据
        private void btn_clicDelete_Click(object sender, RoutedEventArgs e)
        {
            PFApp.ClickType = "deleteData";
            //(Application.Current as IApp).strImageType = "deleteData";
        }

        void i_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            string type = (Application.Current as IApp).strImageType;
            
            string strImg = (sender as Image).Tag.ToString();

            m_wxytype = strImg.Split('|')[0];
            m_wxyid = strImg.Split('|')[1];
            m_dwdm = strImg.Split('|')[2];
            m_remark = strImg.Split('|')[3];

            //return;

            switch (type)
            {
                case "searchData"://显示详细

                    //获取点选查询的配置
                    listName = new List<string>();
                    xele = PFApp.Extent;
                    var ln = (from item in xele.Element("PointNames").Elements("name")
                              select new
                              {
                                  n = item.Attribute("n").Value,
                              }).ToList();
                    foreach (var item in ln)
                    {
                        listName.Add(item.n);
                    }

                    listPoint = new List<clsPoint>();
                    clsPoint cp;
                    for (int i = 0; i < strImg.Split('|').Length; i++)
                    {
                        cp = new clsPoint();
                        cp.value = strImg.Split('|')[i];
                        cp.name = listName[i];

                        listPoint.Add(cp);
                    }
                    
                    data_result_c.ItemsSource = null;
                    data_result_c.ItemsSource = listPoint;

                    //查询数据库，获取坐标信息，高亮显示
                    if (AykjClientInner != null)
                    {
                        AykjClientInner.getDataLocationCompleted -= AykjClientInner_getDataLocationCompleted;
                        AykjClientInner.getDataLocationCompleted += new EventHandler<AykjDataServiceInner.getDataLocationCompletedEventArgs>(AykjClientInner_getDataLocationCompleted);

                        AykjClientInner.getDataLocationAsync(m_wxyid, m_wxytype, m_dwdm);
                    }
                    break;
                case "deleteData":

                    //20120927
                    MessageWindow msw = new MessageWindow(MsgType.Info, m_remark,"删除？");
                    msw.Closed -= msw_Closed;
                    msw.Closed += new EventHandler(msw_Closed);

                    msw.Show();
                    break;
            }
        }


        void msw_Closed(object sender, EventArgs e)
        {
            MessageWindow cb_msw = sender as MessageWindow;
            if (cb_msw.DialogResult == true)
            {
                //调用wcf删除数据
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

                    AykjClientInner.deleteSqlDataByIdCompleted -= AykjClientInner_deleteSqlDataByIdCompleted;
                    AykjClientInner.deleteSqlDataByIdCompleted += new EventHandler<AykjDataServiceInner.deleteSqlDataByIdCompletedEventArgs>(AykjClientInner_deleteSqlDataByIdCompleted);

                    AykjClientInner.deleteSqlDataByIdAsync(m_wxyid, m_wxytype, m_dwdm);

                }
            }
        }

        #endregion

        void AykjClientInner_getDataLocationCompleted(object sender, AykjDataServiceInner.getDataLocationCompletedEventArgs e)
        {
            if (e.Result.Contains("成功"))
            {
                //AYKJ.GISDevelop.Platform.ToolKit.Message.Show("查询数据坐标位置成功");
                MapPoint mp = new MapPoint(
                    Double.Parse(e.Result.ToString().Split(',')[1]),
                    Double.Parse(e.Result.ToString().Split(',')[2])
                );

                if (selectHigh_GraLayer == null)
                {
                    selectHigh_GraLayer = new GraphicsLayer();
                }
                else
                {
                    selectHigh_GraLayer.Graphics.Clear();
                }

                if (mainmap == null)
                {
                    mainmap = (Application.Current as IApp).MainMap;
                }

                if (!mainmap.Layers.Contains(selectHigh_GraLayer))
                {
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

                mainmap.ZoomTo(eve);

            }
            else
            {
                AYKJ.GISDevelop.Platform.ToolKit.Message.ShowErrorInfo("坐标查询失败",e.Result);
            }
        }

        //删除数据成功事件
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

                    for (int i = 0; i < gl.Graphics.Count; i++)
                    {
                        Graphic g = gl.Graphics[i];
                        string stag = g.Attributes["StaTag"].ToString();
                        if (stag.Split('|')[0] == m_wxytype && stag.Split('|')[1] == m_wxyid && stag.Split('|')[2] == m_dwdm && stag.Split('|')[3] == m_remark)
                        {
                            (Application.Current as IApp).lstThematic.Remove(g);

                            gl.Graphics.Remove(g);

                            //20131008:删除系统内存储的数据
                            List<clsThematic> ddd = (Application.Current as IApp).dict_thematic[s];
                            foreach (clsThematic cls in ddd)
                            {
                                if (cls.str_wxytype == m_wxytype
                                        && cls.str_wxyid == m_wxyid
                                        && cls.str_dwdm == m_dwdm
                                        && cls.str_remark == m_remark)
                                {
                                    (Application.Current as IApp).dict_thematic[s].Remove(cls);

                                    break;
                                }
                            }

                            break;
                        }
                    }

                    if (selectHigh_GraLayer != null)
                    {
                        selectHigh_GraLayer.Graphics.Clear();
                    }

                   
                }
                catch (Exception ex)
                {
                    AYKJ.GISDevelop.Platform.ToolKit.Message.Show("数据删除成功，地图没有自动刷新，请手动刷新页面。");
                    return;
                }

                AYKJ.GISDevelop.Platform.ToolKit.Message.Show("删除成功");

            }
            else
            {
                AYKJ.GISDevelop.Platform.ToolKit.Message.ShowErrorInfo("删除数据失败",e.Result);
            }
        }


        /// <summary>
        /// datagrid切换选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void data_result_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        #region 重置信息
        public void Reset()
        {
            try
            {
                //关闭该页面后的点击响应事件恢复为默认（弹出气泡）。
                //(Application.Current as IApp).strImageType = "defaultClick";

                //20130926：最新该选项卡功能
                (Application.Current as IApp).strImageType = "deleteData";

                data_result_c.ItemsSource = null;

                //清除高亮图层
                if (selectHigh_GraLayer != null)
                {
                    selectHigh_GraLayer.Graphics.Clear();
                    mainmap.Layers.Remove(selectHigh_GraLayer);
                }

                //20121009
                if (timer_c!=null)
                {
                    timer_c.Stop();

                    if ((Application.Current as IApp).syslistthisimg != null && (Application.Current as IApp).syslistthisimg.Count > 0)
                    {
                        foreach (Image i in (Application.Current as IApp).syslistthisimg)
                        {
                            i.MouseLeftButtonUp -= new MouseButtonEventHandler(i_MouseLeftButtonUp);
                        }
                    }                    
                }

            }
            catch (Exception)
            { }
        }
        #endregion


    }
}
