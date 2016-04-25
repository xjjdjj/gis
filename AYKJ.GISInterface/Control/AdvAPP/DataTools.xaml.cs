using AYKJ.GISDevelop.Platform;
using AYKJ.GISDevelop.Platform.ToolKit;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace AYKJ.GISInterface.Control.AdvAPP
{
    public partial class DataTools : UserControl
    {
        #region 全局变量

        #region 控制器
        bool IsActive = false;
        #endregion

        private AykjDataServiceInner.AykjDataClient AykjClientInner;
        public GraphicsLayer selectHigh_GraLayer;
        /// <summary>
        /// 当前执行任务
        /// </summary>
        String Task = "";
        String PointInfo;
        Map mainmap;
        public bool bVisible = true;
        public bool bDisable = true;
        int Enable = 0;
        int Disable = 1;
        int Active = 2;
        Dictionary<string, ToggleButton> dicBtn;

        Dictionary<string, string[]> dicImgPath;

        Dictionary<string, string> dicOperator = new Dictionary<string, string>()
        {
            {"打点","AddPoint"},
            {"删除","DeletePoint"}
        };

        #region 打点状态
        ContextMenu cm;
        MapPoint mp;
        #endregion
        #endregion

        public DataTools()
        {
            InitializeComponent();
        }
        #region 窗口事件
        void loadMenu()
        {
            //获取辅助操作内容
            XElement xele = PFApp.Extent;
            try
            {
                string[] ary = xele.Element("DataTools").Attribute("Visible").Value.Split('|');
                string enimg = xele.Element("DataTools").Attribute("EnImg").Value.ToLower();
                this.dicImgPath = new Dictionary<string, string[]>();
                foreach (string str in ary)
                {
                    string key = enimg == "true" ? dicOperator.ContainsKey(str) ? dicOperator[str] : str : str;
                    string path = "/Image/DataTools/" + key;
                    //三种状态
                    this.dicImgPath.Add(str, new string[] { path + this.Enable.ToString() + ".png", 
                        path + this.Disable.ToString() + ".png", 
                        path + this.Active.ToString() + ".png" });
                }
                this.dicBtn = new Dictionary<string, ToggleButton>();
            }
            catch (Exception ex)
            {
                //无法获取操作内容,不显示元素，等待关闭
                this.Visibility = System.Windows.Visibility.Collapsed;
                this.bDisable = false;
                return;
            }
            foreach (KeyValuePair<string, string[]> kv in this.dicImgPath)
            {
                StackPanel sp = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness() { Top = 10,Left=10 } };

                ToggleButton tbtn = CreateTooButton(kv);
                this.dicBtn.Add(kv.Key, tbtn);
                sp.Children.Add(tbtn);
                //横向添加按钮
                ToolBar.Children.Add(sp);
            }
        }

        /// <summary>
        /// 创建工具栏按钮
        /// </summary>
        /// <param name="strname"></param>
        /// <param name="IsTwo"></param>
        /// <returns></returns>
        ToggleButton CreateTooButton(KeyValuePair<string,string[]> kv)
        {
            ToggleButton togbtn = new ToggleButton();
            
            togbtn.Cursor = Cursors.Hand;
            togbtn.Width = 25;
            togbtn.Height = 25;
            togbtn.Content = kv.Key;
 
            togbtn.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            togbtn.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            string path = kv.Value[this.Disable];
            if (kv.Key == this.Task)
            {
                path = kv.Value[this.Enable];
                togbtn.Style = this.Resources["ToolButton"] as Style;
                togbtn.Click += new RoutedEventHandler(ToggleButton_Click);
            }
            else
            {
                togbtn.Style = this.Resources["ToolButtonDis"] as Style;
            }

            Uri uri = new Uri(path, UriKind.Relative);
            togbtn.Background = new ImageBrush()
            {
                Stretch = Stretch.Fill,
                ImageSource = new BitmapImage(uri)
            };

            return togbtn;
        }

        /// <summary>
        /// 按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            //切换按钮状态
            SwitchBtnState(this.Active);

            //只有当前任务有绑定事件
            switch (this.Task)
            {
                case "打点":
                    AddPoint();
                    break;
                case "删除":
                    break;
            }
        }

        public void initWindow()
        {
            this.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            this.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            
            this.Margin = new Thickness() { Top = 10 };
        }

        public void rest()
        {

        }

        public void Hide()
        {
            this.IsActive = false;
            Storyboard_fadeout.Begin();
            Storyboard_fadeout.Completed += Storyboard_fadeout_Completed;
        }

        void Storyboard_fadeout_Completed(object sender, EventArgs e)
        {
            //关闭动画结束后关闭
            if (PFApp.Root.Children.Contains(this))
            {
                PFApp.Root.Children.Remove(this);
            }
            
        }

        public void Show(String task, String pointinfo)
        {
            this.IsActive = true; 
            //切换任务
            this.Task = task;
            this.PointInfo = pointinfo;
            if (PFApp.Root.Children.Count > 0)
            {
                for (int i = 0; i < PFApp.Root.Children.Count; i++)
                {
                    if (PFApp.Root.Children[i].GetType().Name == "DataTools")
                    {
                        PFApp.Root.Children.RemoveAt(i);
                    }
                }
            }
            PFApp.Root.Children.Add(this);
            //没有初始化按钮
            if (this.dicBtn == null)
            {
                loadMenu();
            }
            if (!this.bDisable)
                return;
            mainmap = (Application.Current as IApp).MainMap;
            initWindow();
            Storyboard_fadein.Begin();
        }

        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btn_close_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }
#endregion

        #region 业务事件
        /// <summary>
        /// 改变图标状态
        /// </summary>
        /// <param name="state"></param>
        void SwitchBtnState(int state)
        {
            this.dicBtn[this.Task].Background = new ImageBrush()
            {
                ImageSource = new BitmapImage(new Uri(this.dicImgPath[this.Task][state], UriKind.Relative)),
                Stretch = Stretch.Fill
            };
        }

        #region 打点业务

        void AddPoint()
        {
            //绑定地图事件
            mainmap.MouseClick -= mainmap_MouseClick;
            mainmap.MouseClick += mainmap_MouseClick;

            //修改右键菜单,右键菜单为空时才能触发右键事件
            cm = ContextMenuService.GetContextMenu(this.mainmap);
            ContextMenuService.SetContextMenu(this.mainmap, null);


            mainmap.MouseRightButtonDown -= mainmap_MouseRightButtonDown;
            mainmap.MouseRightButtonDown += mainmap_MouseRightButtonDown;
        }

        #region 打点绑定事件
        void mainmap_MouseClick(object sender, Map.MouseEventArgs e)
        {
            if (this.IsActive)
            {
                //坐标转换，从屏幕坐标到地图坐标
                this.mp = e.MapPoint;
                if (this.mp != null)
                {
                    AddPoint2DB(this.mp, this.PointInfo);
                }
            }
        }

        void mainmap_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.IsActive)
            {
                removeAddState();
                SwitchBtnState(this.Enable);
            }
        }
        #endregion

        /// <summary>
        /// 恢复打点任务为未触发状态
        /// </summary>
        void removeAddState()
        {
            //删除点坐标信息，保留点属性信息
            this.mp = null;
            //清空绑定的事件
            mainmap.MouseClick -= mainmap_MouseClick;
            mainmap.MouseRightButtonDown -= mainmap_MouseRightButtonDown;

            //恢复右键菜单
            if (this.cm != null)
            {
                ContextMenuService.SetContextMenu(this.mainmap, this.cm);
            }
        }


        /// <summary>
        /// 添加点到数据库
        /// </summary>
        /// <param name="mp"></param>
        /// <param name="PointInfo"></param>
        void AddPoint2DB(MapPoint mp, string PointInfo)
        {
            if (mp == null || string.IsNullOrWhiteSpace(PointInfo))
                return;
            if (AykjClientInner == null)
            {
                #region//20120809zc:读取数据服务信息
                XElement xele = PFApp.Extent;
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
                    string[] tmps = PointInfo.Split('|');
                    AykjClientInner.addNewSqlDataAsync(tmps[0], tmps[1], tmps[2], "", tmps[3], mp.X.ToString("f4"), mp.Y.ToString("f4"), tmps[4], tmps[5]);//20150305:江宁智慧安监新增一个street字段，默认为空。
                }
                catch (Exception e1)
                {
                    AYKJ.GISDevelop.Platform.ToolKit.Message.Show("添加专题数据失败");
                }
            }
            mainmap.MouseClick -= mainmap_MouseClick;
            this.Hide();
        }

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
            if (addPointFinished != null)
                addPointFinished.InvokeSelf(rtnstr);
            removeAddState();
            SwitchBtnState(this.Disable);
        }

        //打点的显示函数
        public void Add_Image()
        {
            try
            {
                string[] tmps = this.PointInfo.Split('|');

                string tmp_wxyid = tmps[0];
                string tmp_wxytype = tmps[1];
                string tmp_dwdm = tmps[2];
                string tmp_remark = tmps[3];
                string tmp_x = this.mp.X.ToString("f4");
                string tmp_y = this.mp.Y.ToString("f4");
                string tmp_street = tmps[4];
                string tmp_enttype = tmps[5];

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
                        str_cntype = tmp_cntype,
                        str_x = tmp_x,
                        str_y = tmp_y,
                        str_dwdm = tmp_dwdm,
                        str_remark = tmp_remark,
                        str_street = tmp_street,
                        str_enttype = tmp_enttype
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

        #region 专题数据图片加载触发事件
        string m_wxytype, m_wxyid, m_dwdm, m_remark;
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

        private void Image_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// 鼠标单击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void theme_btn_click(object sender, RoutedEventArgs e)
        {
            string type = PFApp.ClickType;
            strImg = (sender as Button).Tag.ToString();
            //return;
            switch (type)
            {
                case "searchData"://显示详细
                    break;
                case "deleteData":
                    //删除改为右键
                    break;
                default:
                    defaultClick();
                    break;
            }
        }

        void defaultClick()
        {
            if (AykjClientInner == null)
            {
                #region//20120809zc:读取数据服务信息
                XElement xele = PFApp.Extent;
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
                    AykjClientInner.getDataLocationCompleted -= AykjClient_getDataLocationCompleted;
                    AykjClientInner.getDataLocationCompleted += new EventHandler<AykjDataServiceInner.getDataLocationCompletedEventArgs>(AykjClient_getDataLocationCompleted);
                    string[] tmps = PointInfo.Split('|');
                    if (m_wxytype.Equals("zdwxy"))
                    {
                        //20150514zc：重大危险源也是一种企业。
                        AykjClientInner.getDataLocationAsync(tmps[0], "enterprise", tmps[2]);
                    }
                    else
                    {
                        AykjClientInner.getDataLocationAsync(tmps[0], tmps[1], tmps[2]);
                    }
                }
                catch (Exception e1)
                {
                    AYKJ.GISDevelop.Platform.ToolKit.Message.Show("查询数据失败");
                }
            }
        }

        string imgunload = "/Image/unload.jpg";
        string swfdefault = "/swf/1.swf";
        string strImg;
        GraphicsLayer ShowBusiness_Layer;
        void AykjClient_getDataLocationCompleted(object sender, AykjDataServiceInner.getDataLocationCompletedEventArgs e)
        {
            XElement xele = PFApp.Extent;
            //企业详细信息页面

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
                        if (ShowBusiness_Layer == null)
                        {
                            ShowBusiness_Layer = new GraphicsLayer();
                            ShowBusiness_Layer.ID = "ShowBusiness_Layer";
                        }
                        else
                        {
                            ShowBusiness_Layer.Graphics.Clear();
                        }
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
                        if (mainmap.Layers["ShowBusiness_Layer"] == null)
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
                        AYKJ.GISDevelop.Platform.ToolKit.Message.ShowErrorInfo("查看详细信息出错", ex.Message);
                    }
                    #endregion
                }
                else if (strImg.Contains("zdwxy"))//“重大危险源”也是一种企业。
                {
                    #region//20130926:查询企业位置，在wcf配置文件中含有独立查询语句的。
                    //"成功:查询的数据位置," + x + "," + y+","+name+","+add+","+lp+","+phone;
                    try
                    {
                        if (ShowBusiness_Layer == null)
                        {
                            ShowBusiness_Layer = new GraphicsLayer();
                            ShowBusiness_Layer.ID = "ShowBusiness_Layer";
                        }
                        else
                        {
                            ShowBusiness_Layer.Graphics.Clear();
                        }
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
                        tmpgra.Attributes.Add("IsPmtShow", Visibility.Visible);
                        tmpgra.Symbol = BusinessSymbol;
                        Graphic tmpgra2 = new Graphic();
                        tmpgra2.Geometry = mp;
                        tmpgra2.Symbol = HighMarkerStyle;
                        ShowBusiness_Layer.Graphics.Add(tmpgra2);
                        ShowBusiness_Layer.Graphics.Add(tmpgra);
                        if (mainmap.Layers["ShowBusiness_Layer"] == null)
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
                        AYKJ.GISDevelop.Platform.ToolKit.Message.ShowErrorInfo("查看详细信息出错", ex.Message);
                    }
                    #endregion
                }
                else
                {
                    try
                    {
                        string result = "{'module':'sp','wxytype':'" + ary_str[0] + "','wxyid':'" + ary_str[1] + "','dwdm':'" + ary_str[2] + "'}";
                        rtnToBussinesspage("openDetailInfoPage", result);
                    }
                    catch
                    {
                        //兼容老版本，业务系统没有自己打开详细页面的方法
                        #region 其他类型
                        if (strImg.Contains("accident"))
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
                        #endregion
                    }

                }
            }

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

        /// <summary>
        /// 鼠标进入按钮的时候修改右键内容
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //新设鼠标右键事件
            cm = ContextMenuService.GetContextMenu(this.mainmap);
            ContextMenu tmp = new ContextMenu();

            MenuItem MI_ZoomOut = new MenuItem();
            MI_ZoomOut.FontFamily = new System.Windows.Media.FontFamily("NSimSun");
            MI_ZoomOut.FontSize = 12;
            MI_ZoomOut.Header = "删除数据";
            ToolTipService.SetToolTip(MI_ZoomOut, "删除数据");
            MI_ZoomOut.Cursor = Cursors.Hand;
            Image image_ZoomOut = new Image() { Width = 16, Height = 16 };
            image_ZoomOut.Source = new BitmapImage(new Uri("../Image/MapTools/删除数据.png", UriKind.Relative));
            MI_ZoomOut.Icon = image_ZoomOut;
            MI_ZoomOut.Click += new RoutedEventHandler(MI_ZoomOut_Click);
            MI_ZoomOut.Tag = (sender as Button).Tag;
            tmp.Items.Add(MI_ZoomOut);

            ContextMenuService.SetContextMenu(this.mainmap, tmp);
        }

        private void MI_ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            strImg = (sender as MenuItem).Tag.ToString();
            string[] tmp = strImg.Split('|');
            m_wxytype = tmp[0];
            m_wxyid = tmp[1];
            m_dwdm = tmp[2];
            m_remark = tmp[3];
           
            MessageWindow msw = new MessageWindow(MsgType.Info, m_remark, "删除？");

            msw.Closed -= msw_Closed;
            msw.Closed += new EventHandler(msw_Closed);

            msw.Show();
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void msw_Closed(object sender, EventArgs e)
        {
            MessageWindow cb_msw = sender as MessageWindow;

            if (cb_msw.DialogResult == true)
            {
                XElement xele = PFApp.Extent;
                var dataServices = (from item in xele.Element("DataServices").Elements("DataService")
                                    where item.Attribute("Name").Value == "专题数据"
                                    select new
                                    {
                                        Type = item.Attribute("Type").Value,
                                        Url = item.Attribute("Url").Value,
                                        Scale = item.Attribute("Scale").Value,
                                        RealUrl = item.Attribute("RealUrl").Value,
                                    }).ToList();
                if (dataServices.Count == 0)
                {
                    AYKJ.GISDevelop.Platform.ToolKit.Message.ShowErrorInfo("缺少数据服务", "请检查配置信息，数据服务是否为空");
                    return;
                }

                if (dataServices[0].Type == "OneTime")
                {
                    BasicHttpBinding binding = new BasicHttpBinding();
                    binding.MaxReceivedMessageSize = 8192000;

                    AykjClientInner = new AykjDataServiceInner.AykjDataClient(binding, new EndpointAddress(dataServices[0].Url));
                    AykjClientInner.deleteSqlDataByIdCompleted -= AykjClient_deleteSqlDataByIdCompleted;
                    AykjClientInner.deleteSqlDataByIdCompleted += AykjClient_deleteSqlDataByIdCompleted;
                    AykjClientInner.deleteSqlDataByIdAsync(m_wxyid, m_wxytype, m_dwdm);
                }
            }
        }

        /// <summary>
        /// 删除点位信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void AykjClient_deleteSqlDataByIdCompleted(object sender, AykjDataServiceInner.deleteSqlDataByIdCompletedEventArgs e)
        {
            if (e.Result.Contains("不存在"))
            {
                AYKJ.GISDevelop.Platform.ToolKit.Message.Show("指定数据不存在");
                return;
            }

            if (e.Result.Contains("成功"))
            {
                //刷新地图显示
                try
                {
                    //删除数据所在的图层名
                    string s = (Application.Current as IApp).DictThematicEnCn[m_wxytype];
                    GraphicsLayer gl = (Application.Current as IApp).Dict_ThematicLayer[s];

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
                AYKJ.GISDevelop.Platform.ToolKit.Message.ShowErrorInfo("删除数据失败", e.Result);
            }
        }

        /// <summary>
        /// 鼠标离开按钮的时候恢复右键内容
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //还原鼠标右键事件
            ContextMenuService.SetContextMenu(this.mainmap, cm);
        }

        #endregion

        #region 企业详细信息页面内事件
        void btn_gra_close_Click(object sender, RoutedEventArgs e)
        {
            mainmap.Layers.Remove(ShowBusiness_Layer);
        }

        void btn_JY_Click(object sender, RoutedEventArgs e)
        {
            XElement xele = PFApp.Extent;
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
            //获取图片
            rtnToBussinesspage("getPmtUrl", strImg);
        }

        void btn_SP_Click(object sender, RoutedEventArgs e)
        {
            string[] ary_str = strImg.Split('|');
            string result = "{'module':'sp','wxytype':'" + ary_str[0] + "','wxyid':'" + ary_str[1] + "','dwdm':'" + ary_str[2] + "'}";
            rtnToBussinesspage("rtnShiPinFromSL", result);
        }

        void btn_XX_Click(object sender, RoutedEventArgs e)
        {
            XElement xele = PFApp.Extent;
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
        #endregion



        #endregion


        #endregion

        /// <summary>
        /// 与JS交互
        /// </summary>
        /// <param name="func"></param>
        /// <param name="mess"></param>
        void rtnToBussinesspage(string func, string mess)
        {
            ScriptObject queryfinished = HtmlPage.Window.GetProperty(func) as ScriptObject;
            if (queryfinished != null)
                queryfinished.InvokeSelf(mess);
        }

       
    }
}
