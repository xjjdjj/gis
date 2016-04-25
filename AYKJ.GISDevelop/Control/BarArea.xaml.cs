/// <summary>  
/// 作者：陈锋 
/// 时间：2012/5/24 13:30:31  
/// 公司:南京安元科技有限公司  
/// 版权：2012-2020  
/// CLR版本：4.0.30319.261  
/// BarArea说明：基础功能条
/// 唯一标识：97182203-d1a1-44cc-8f7f-68263b574287  
/// </summary>

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Printing;
using AYKJ.GISDevelop.Platform;
using AYKJ.GISDevelop.Platform.Part;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using System.Linq;
using System.IO;
using System.Xml.Linq;
using AYKJ.GISDevelop.Platform.ToolKit;

namespace AYKJ.GISDevelop.Control
{
    public partial class BarArea : UserControl, IPlatformControl
    {
        #region 参数定义
        //测量辅助
        Measure measure;
        //信息查询
        Identify identify;
        //书签
        BookMark bookmark;
        //鼠标坐标
        MouseCoordinate Mcord;
        //坐标定位
        SetCoordinate setcoordinate;
        //放大镜
        MagnifyGlass MagnifyGlass;
        ////图层列表
        //LayerControl layercontrol;
        //专题图层控制
        ThematicLayerControl layercontrol;
        //图例
        Legend legend;
        //主地图
        public Map SystemMainMap;
        //工具栏button
        private ToggleButton CurrBarBtn;
        //前进后退标识
        string Mode_PreNext = "";
        //放大缩小Draw
        private Draw MyDrawZoom;
        //储存的地图范围
        List<Envelope> MapExtentHistory = new List<Envelope>();
        int CurrentExtentIndex = 0;
        bool _newExtent = true;
        //菜单button
        private ToggleButton CurrButton;
        //菜单的高度
        double MenuHeight = 0;
        bool IsCanShowAll = true;
        //拉框放大缩小的样式
        SimpleFillSymbol ZoomFillSymbol = new SimpleFillSymbol()
        {
            Fill = new SolidColorBrush(new Color() { A = 33, R = 255, G = 0, B = 0 }),
            BorderThickness = 2,
            BorderBrush = new SolidColorBrush(new Color() { A = 255, R = 255, G = 0, B = 0 })
        };
        List<ToggleButton> lstchild = new List<ToggleButton>();
        bool _isEnToolImg = false;
        Dictionary<string, string> dic_ToolImg = new Dictionary<string, string>() { { "拉框缩小", "TogBtn_ZoomOut" }, { "拉框放大", "TogBtn_ZoomIn" }, { "漫游", "TogBtn_Pan" },
                    { "前一视图", "TogBtn_Pre" }, { "后一视图", "TogBtn_Next" }, { "坐标定位", "TogBtn_SetCoor" }, { "放大镜", "TogBtn_Plus" },{"书签","TogBtn_Mark"},{"图例","TogBtn_Legend"},
                    {"图层列表","TogBtn_Layer"},{"地图查询","TogBtn_Identify"},{"全图","TogBtn_FullMap"},{"坐标显示","TogBtn_Coordinates"},{"清除","TogBtn_Clear"} ,
                    {"打印","TogBtn_Print"},{"截图","TogBtn_PrcScn"},{"距离量测","TogBtn_LineMeasure"},{"面积量测","TogBtn_AreaMeasure"},{"全屏显示","TogBtn_FullScreen"},{"输出","TogBtn_ImageExporter"},{"影像切换","TogBtn_Image"}};
        #endregion

        #region ToolBar命名
        ToggleButton TogBtn_ZoomOut;
        ToggleButton TogBtn_ZoomIn;
        ToggleButton TogBtn_SetCoor;
        ToggleButton TogBtn_Pre;
        ToggleButton TogBtn_Plus;
        ToggleButton TogBtn_Pan;
        ToggleButton TogBtn_Next;
        ToggleButton TogBtn_Mark;
        ToggleButton TogBtn_Legend;
        ToggleButton TogBtn_Layer;
        ToggleButton TogBtn_Identify;
        ToggleButton TogBtn_FullMap;
        ToggleButton TogBtn_Coordinates;
        ToggleButton TogBtn_Clear;
        ToggleButton TogBtn_Image;
        #endregion

        //20130926:百度测试，发现地图视野变化时，在移动后会再次发生微小的（小数点后几位）移动，导致再次触发extendchanged事件。
        int m_baidu_ex;

        public BarArea()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(BarArea_Loaded);
        }

        void BarArea_Loaded(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Visible;
            BarAreaInit();
            CreateRightMenu();
        }

        /// <summary>
        /// 实例化地图
        /// </summary>
        void BarAreaInit()
        {
            ((App.Current as PFApp).RootVisual as MainPage).ShowMap();
            SystemMainMap = App.mainMap;

            SystemMainMap.ExtentChanged -= SystemMainMap_ExtentChanged;
            SystemMainMap.ExtentChanged += new EventHandler<ExtentEventArgs>(SystemMainMap_ExtentChanged);
            MyDrawZoom = new Draw(SystemMainMap);
            MyDrawZoom.FillSymbol = ZoomFillSymbol;
            MyDrawZoom.DrawMode = DrawMode.Rectangle;
            //加载工具栏
            ToolBarVisable();
            //SpBar.MinHeight = 70 + ToolBar.Children.Count * 40;
            //加载菜单
            LoadMenu();

            //实例化量测
            measure = new Measure();
            //实例化坐标显示
            Mcord = new MouseCoordinate();
            //实例化坐标定位
            setcoordinate = new SetCoordinate();
            //实例化放大镜
            MagnifyGlass = new MagnifyGlass();
            //实例化图层控制
            //layercontrol = new LayerControl();
            layercontrol = new ThematicLayerControl();
            //实例化信息查询
            identify = new Identify();
            //实例化书签
            bookmark = new BookMark();
            //实例化图例
            legend = new Legend();

            MyDrawZoom.DrawComplete -= MyDrawZoom_DrawComplete;
            MyDrawZoom.DrawComplete += new EventHandler<DrawEventArgs>(MyDrawZoom_DrawComplete);
        }

        /// <summary>
        /// 加载菜单
        /// </summary>
        private void LoadMenu()
        {
            int menucount = 0;

            //遍历配置中的菜单配置信息
            foreach (var item in App.Menus)
            {
                if (item.Type != "Interface")
                {
                    //菜单的每一项为Button
                    ToggleButton btn = new ToggleButton()
                    {
                        Content = item.Title,
                        Name = item.MenuName,
                        Cursor = Cursors.Hand,
                        Width = 65,
                        Height = 25,
                        Margin = new Thickness(0, 3, 0, 2),
                        //使用应用的默认样式，可以进行覆盖，
                        //在系统加载的时候，后读的样式资源将会覆盖系统默认的样式
                        Style = this.Resources["MenuButton"] as Style,
                    };
                    if (item.Visible != "True")
                        btn.Visibility = Visibility.Collapsed;
                    else
                        menucount++;
                    btn.Click += new RoutedEventHandler(btn_Click);
                    //btn.SetValue(Grid.ColumnProperty, i++);
                    MenuBar.Children.Add(btn);
                    IPart part = PFApp.UIS[item.MenuName] as IPart;
                    part.ShowEnd += new PartEventHander(part_ShowEnd);
                    part.CloseEnd += new PartEventHander(part_CloseEnd);
                }
            }

            //根据浏览器的大小获取菜单栏的高度
            Application.Current.Host.Content.Resized += new EventHandler(Content_Resized);
            MenuHeight = Application.Current.Host.Content.ActualHeight - 36 - 123 - ToolBar.Children.Count * 34;
            //菜单需要的高度
            double NeedHeight = menucount * 35;
            //如果菜单不能显示完全，显示滚动条
            if (NeedHeight < MenuHeight)
            {
                //隐藏按钮
                IsCanShowAll = true;
                btn_Top.Visibility = Visibility.Collapsed;
                btn_Bottom.Visibility = Visibility.Collapsed;
            }
            else
            {
                IsCanShowAll = false;
                btn_Top.Visibility = Visibility.Visible;
                btn_Bottom.Visibility = Visibility.Visible;
                MenuHeight = MenuHeight - 28;
            }
            if (MenuHeight < 0)
            {
                ScrollLst.Height = 0;
            }
            else
            {
                ScrollLst.Height = NeedHeight;
            }
        }

        void Content_Resized(object sender, EventArgs e)
        {
            int menucount = 0;
            for (int i = 0; i < App.Menus.Count; i++)
            {
                if (App.Menus[i].Visible == "True")
                {
                    menucount = menucount + 1;
                }
            }

            //根据浏览器的大小获取菜单栏的高度
            MenuHeight = Application.Current.Host.Content.ActualHeight - 36 - 123 - ToolBar.Children.Count * 34;
            //菜单需要的高度
            double NeedHeight = menucount * 35;
            //如果菜单不能显示完全，显示滚动条
            if (NeedHeight < MenuHeight)
            {
                //隐藏按钮
                IsCanShowAll = true;
                btn_Top.Visibility = Visibility.Collapsed;
                btn_Bottom.Visibility = Visibility.Collapsed;
                ScrollLst.Height = NeedHeight;
            }
            else
            {
                IsCanShowAll = false;
                btn_Top.Visibility = Visibility.Visible;
                btn_Bottom.Visibility = Visibility.Visible;
                MenuHeight = MenuHeight - 28;
                ScrollLst.Height = 35;
            }
            if (MenuHeight < 0)
            {
                ScrollLst.Height = 0;
            }
            //else
            //{
            //    ScrollLst.Height = NeedHeight;
            //}
        }

        void part_CloseEnd(IPart sender)
        {
            foreach (var itembtn in MenuBar.Children)
            {
                ToggleButton tbtn = itembtn as ToggleButton;
                if ((PFApp.UIS[tbtn.Name] as FrameworkElement).Parent != null)
                {
                    tbtn.IsChecked = true;
                }
                else
                {
                    tbtn.IsChecked = false;
                }
            }
        }

        void part_ShowEnd(IPart sender)
        {
            foreach (var itembtn in MenuBar.Children)
            {
                ToggleButton tbtn = itembtn as ToggleButton;
                if ((PFApp.UIS[tbtn.Name] as FrameworkElement).Parent != null)
                {
                    tbtn.IsChecked = true;
                }
                else
                {
                    tbtn.IsChecked = false;
                }
            }
        }

        void btn_Click(object sender, RoutedEventArgs e)
        {
            //判断点击关联的面板是否存在
            if (!PFApp.UIS.ContainsKey((sender as ToggleButton).Name))
                return;
            //获取关联的面板
            IPart part = PFApp.UIS[(sender as ToggleButton).Name] as IPart;
            if (CurrButton == sender)
            {
                if ((part as UserControl).Parent != null)
                {
                    part.Close();
                }
                else
                {
                    for (int i = 0; i < PFApp.Menus.Count; i++)
                    {
                        IPart tmppart = PFApp.UIS[PFApp.Menus[i].MenuName] as IPart;
                        if ((tmppart as UserControl).Parent != null)
                        {
                            foreach (var itembtn in MenuBar.Children)
                            {
                                ToggleButton tbtn = itembtn as ToggleButton;
                                tbtn.IsChecked = false;
                            }
                            tmppart.Close();
                        }
                    }
                    part.Show();
                    (sender as ToggleButton).IsChecked = true;
                }
            }
            else
            {
                CurrButton = sender as ToggleButton;
                if (this.VerticalAlignment != VerticalAlignment.Top)
                {
                    //避免在动画的执行过程中重复点击
                    this.IsEnabled = false;
                    //FromCenterToBottom();
                }
                else
                {

                    if (PFApp.UIS.ContainsKey(this.CurrButton.Name))
                    {
                        try
                        {
                            for (int i = 0; i < PFApp.Menus.Count; i++)
                            {
                                IPart tmppart = PFApp.UIS[PFApp.Menus[i].MenuName] as IPart;
                                if ((tmppart as UserControl).Parent != null)
                                {
                                    foreach (var itembtn in MenuBar.Children)
                                    {
                                        ToggleButton tbtn = itembtn as ToggleButton;
                                        tbtn.IsChecked = false;
                                    }
                                    tmppart.Close();
                                }
                            }
                            part.Show();
                            (sender as ToggleButton).IsChecked = true;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("组件未实现平台指定的打开方法或者该方法中有异常！\n" + ex.ToString());
                        }
                    }

                }
            }
        }

        public Panel Root
        {
            get { return SpBar; }
        }

        /// <summary>
        /// 加载工具栏
        /// </summary>
        void ToolBarVisable()
        {
            XElement xele = PFApp.Extent;
            string[] ary = xele.Element("Tool").Attribute("Visible").Value.Split('|');

            //兼容以前版本的配置文件，没有EnImg选项则跳过
            try
            {
                if (xele.Element("Tool").Attribute("EnImg").Value.ToString() == "True")
                {
                    this._isEnToolImg = true;
                    if (PFApp.MapServerType == enumMapServerType.Baidu)
                    {
                        ary.ToList().Remove("影像切换");
                    }
                }
            }
            catch (Exception ex)
            {
                if (PFApp.MapServerType == enumMapServerType.Baidu)
                {
                    ary.ToList().Remove("TogBtn_Image");
                }
            }
            for (int i = 0; i < ary.Length - 1; i++)
            {
                StackPanel sp = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness() { Top = 10 } };
                sp.Children.Add(CreateTooButton(ary[i], false));
                i = i + 1;
                sp.Children.Add(CreateTooButton(ary[i], true));
                ToolBar.Children.Add(sp);
            }
        }

        /// <summary>
        /// 创建工具栏按钮
        /// </summary>
        /// <param name="strname"></param>
        /// <param name="IsTwo"></param>
        /// <returns></returns>
        ToggleButton CreateTooButton(string strname, bool IsTwo)
        {
            ToggleButton togbtn = new ToggleButton();
            togbtn.Click += new RoutedEventHandler(ToggleButton_Click);
            togbtn.Cursor = Cursors.Hand;
            togbtn.Width = 24;
            togbtn.Height = 24;
            togbtn.Content = strname;
            togbtn.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            togbtn.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            togbtn.Style = this.Resources["ToolButton"] as Style;
            string path = strname;
            if (this._isEnToolImg)
                path = dic_ToolImg[strname];
            Uri uri = new Uri("/Image/MapTools/" + path + ".png", UriKind.Relative);
            togbtn.Background = new ImageBrush()
            {
                Stretch = Stretch.Fill,
                ImageSource = new BitmapImage(uri)
            };
            if (IsTwo == true)
            {
                togbtn.Margin = new Thickness() { Left = 10 };
            }
            switch (strname)
            {

                case "拉框缩小":
                    TogBtn_ZoomOut = togbtn;
                    break;
                case "拉框放大":
                    TogBtn_ZoomIn = togbtn;
                    break;
                case "漫游":
                    TogBtn_Pan = togbtn;
                    break;
                case "前一视图":
                    TogBtn_Pre = togbtn;
                    break;
                case "后一视图":
                    TogBtn_Next = togbtn;
                    break;
                case "坐标定位":
                    TogBtn_SetCoor = togbtn;
                    lstchild.Add(TogBtn_SetCoor);
                    break;
                case "放大镜":
                    TogBtn_Plus = togbtn;
                    lstchild.Add(TogBtn_Plus);
                    break;
                case "书签":
                    TogBtn_Mark = togbtn;
                    lstchild.Add(TogBtn_Mark);
                    break;
                case "图例":
                    TogBtn_Legend = togbtn;
                    lstchild.Add(TogBtn_Legend);
                    break;
                case "图层列表":
                    TogBtn_Layer = togbtn;
                    lstchild.Add(TogBtn_Layer);
                    break;
                case "地图查询":
                    TogBtn_Identify = togbtn;
                    lstchild.Add(TogBtn_Identify);
                    break;
                case "全图":
                    TogBtn_FullMap = togbtn;
                    break;
                case "坐标显示":
                    TogBtn_Coordinates = togbtn;
                    lstchild.Add(TogBtn_Coordinates);
                    break;
                case "清除":
                    TogBtn_Clear = togbtn;
                    break;
                case "影像切换":
                    TogBtn_Image = togbtn;
                    break;
            }
            return togbtn;
        }

        /// <summary>
        /// 工具条菜单按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            MyDrawZoom.IsEnabled = false;
            Mode_PreNext = "";

            try
            {
                //获取按钮的对象名称，用于区分按钮在做进一步操作
                if (CurrBarBtn != null)
                {
                    //VisualStateManager.GoToState(CurrBarBtn, "Unchecked", true);
                    CurrBarBtn.IsChecked = false;
                }

                bool IsExit = false;
                //跟新当前button
                if (CurrBarBtn == sender as ToggleButton)
                    IsExit = true;
                CurrBarBtn = sender as ToggleButton;
                CurrBarBtn.IsChecked = true;
                #region 只允许打开一个窗口
                if (IsExit != true)
                {
                    if (Mcord.Parent != null && CurrBarBtn != sender as ToggleButton)
                    {
                        Mcord.Close();
                        TogBtn_Coordinates.IsChecked = false;
                    }
                    if (setcoordinate.Parent != null && lstchild.Contains(CurrBarBtn))
                    {
                        setcoordinate.Close();
                        TogBtn_SetCoor.IsChecked = false;
                    }
                    if (MagnifyGlass.Parent != null && lstchild.Contains(CurrBarBtn))
                    {
                        MagnifyGlass.Close();
                        TogBtn_Plus.IsChecked = false;
                    }
                    if (layercontrol.Parent != null && lstchild.Contains(CurrBarBtn))
                    {
                        layercontrol.Close();
                        TogBtn_Layer.IsChecked = false;
                    }
                    if (identify.Parent != null && lstchild.Contains(CurrBarBtn))
                    {
                        identify.Close();
                        TogBtn_Identify.IsChecked = false;
                    }
                    if (bookmark.Parent != null && lstchild.Contains(CurrBarBtn))
                    {
                        bookmark.Close();
                        TogBtn_Mark.IsChecked = false;
                    }
                    if (legend.Parent != null && lstchild.Contains(CurrBarBtn))
                    {
                        legend.Close();
                        TogBtn_Legend.IsChecked = false;
                    }
                }
                #endregion
                string btnName = CurrBarBtn.Content.ToString();
                //如果执行了量测操作，要清除
                if (measure != null)
                {
                    measure.CloseMeasure();
                }
                //分按钮进行操作
                switch (btnName)
                {
                    //拉框放大
                    case "拉框放大":
                        {
                            MyDrawZoom.IsEnabled = true;
                            Mode_PreNext = "TogBtn_ZoomIn";
                            break;
                        }
                    //拉框缩小
                    case "拉框缩小":
                        {
                            MyDrawZoom.IsEnabled = true;
                            Mode_PreNext = "TogBtn_ZoomOut";
                            break;
                        }
                    //漫游
                    case "漫游":
                        {
                            SystemMainMap.Cursor = Cursors.Arrow;
                            break;
                        }
                    //前一视图
                    case "前一视图":
                        {
                            if (CurrentExtentIndex != 0)
                            {
                                CurrentExtentIndex--;
                                _newExtent = false;
                                SystemMainMap.IsHitTestVisible = false;
                                SystemMainMap.ZoomTo(MapExtentHistory[CurrentExtentIndex]);
                                TogBtn_Next.IsEnabled = true;
                                TogBtn_Next.Opacity = 1;
                                if (CurrentExtentIndex == 0)
                                {
                                    TogBtn_Pre.IsEnabled = false;
                                    TogBtn_Pre.IsChecked = false;
                                    TogBtn_Pre.Opacity = 0.5;
                                }
                            }
                            break;
                        }
                    //后一视图
                    case "后一视图":
                        {
                            if (CurrentExtentIndex < MapExtentHistory.Count - 1)
                            {
                                CurrentExtentIndex++;
                                _newExtent = false;
                                SystemMainMap.IsHitTestVisible = false;
                                SystemMainMap.ZoomTo(MapExtentHistory[CurrentExtentIndex]);
                                //ZoomPre.IsEnabled = true;
                                TogBtn_Pre.IsEnabled = true;
                                TogBtn_Pre.Opacity = 1;
                                if (CurrentExtentIndex == MapExtentHistory.Count - 1)
                                {
                                    TogBtn_Next.IsEnabled = false;
                                    TogBtn_Next.IsChecked = false;
                                    TogBtn_Next.Opacity = 0.5;
                                }
                            }
                            break;
                        }
                    //全图
                    case "全图":
                        {
                            XElement xele = PFApp.Extent;
                            Envelope eve = new Envelope()
                            {
                                XMax = double.Parse(xele.Element("MapExtent").Attribute("XMax").Value),
                                XMin = double.Parse(xele.Element("MapExtent").Attribute("XMin").Value),
                                YMax = double.Parse(xele.Element("MapExtent").Attribute("YMax").Value),
                                YMin = double.Parse(xele.Element("MapExtent").Attribute("YMin").Value),
                                SpatialReference = SystemMainMap.SpatialReference
                            };
                            SystemMainMap.ZoomTo(eve);
                            //SystemMainMap.ZoomTo(SystemMainMap.Layers.GetFullExtent());
                            TogBtn_Pan.IsChecked = true;
                            TogBtn_FullMap.IsChecked = false;
                            CurrBarBtn = TogBtn_Pan;
                            break;
                        }
                    //距离量测
                    case "距离量测":
                        {
                            measure = new Measure();
                            measure.StartMeasure("Polyline");
                            break;
                        }
                    //面积量测
                    case "面积量测":
                        {
                            measure = new Measure();
                            measure.StartMeasure("Polygon");
                            break;
                        }
                    //坐标显示
                    case "坐标显示":
                        {
                            if (Mcord.Parent != null)
                            {
                                Mcord.Close();
                                TogBtn_Coordinates.IsChecked = false;
                            }
                            else
                            {
                                Mcord.Show();
                                TogBtn_Coordinates.IsChecked = true;
                            }
                            break;
                        }
                    //地图查询
                    case "地图查询":
                        {
                            if (identify.Parent != null)
                            {
                                identify.Close();
                                TogBtn_Identify.IsChecked = false;
                            }
                            else
                            {
                                identify.Show();
                                identify.currrentogbtn = TogBtn_Identify;
                                TogBtn_Identify.IsChecked = true;
                            }
                            break;
                        }
                    //放大镜
                    case "放大镜":
                        {
                            if (MagnifyGlass.Parent != null)
                            {
                                MagnifyGlass.Close();
                                TogBtn_Plus.IsChecked = false;
                            }
                            else
                            {
                                MagnifyGlass.Show();
                                TogBtn_Plus.IsChecked = true;
                            }
                            break;
                        }
                    //输出
                    case "输出":
                        {
                            try
                            {
                                clsMapExportToImage imageExporter = new clsMapExportToImage();
                                bool bResult = imageExporter.ExportPNG(SystemMainMap as UIElement);
                                string strResult = bResult ? "成功！" : "没有成功！";
                                Message.Show("图片输出" + strResult);
                            }
                            catch (Exception pEr)
                            {
                                string msg = pEr.Message;
                            }
                            break;
                        }
                    //打印
                    case "打印":
                        {
                            PrintDocument print = new PrintDocument();
                            print.PrintPage += (pd, pe) =>
                            {
                                Image img = new Image();
                                img.Projection = new System.Windows.Media.PlaneProjection();
                                (img.Projection as System.Windows.Media.PlaneProjection).RotationZ = 90;
                                WriteableBitmap bitmap = new WriteableBitmap(SystemMainMap, null);
                                img.Source = bitmap;
                                pe.PageVisual = img;

                            };
                            print.Print(null);
                            break;
                        }
                    //截图
                    case "截图":
                        {
                            ((App.Current as PFApp).RootVisual as MainPage).LinkControlPage();
                            break;
                        }
                    //书签
                    case "书签":
                        {
                            if (bookmark.Parent != null)
                            {
                                bookmark.Close();
                                TogBtn_Mark.IsChecked = false;
                            }
                            else
                            {
                                bookmark.Show();
                                bookmark.currrentogbtn = TogBtn_Mark;
                                TogBtn_Mark.IsChecked = true;
                            }
                            break;
                        }
                    //图层列表
                    case "图层列表":
                        {
                            if (layercontrol.Parent != null)
                            {
                                layercontrol.Close();
                                TogBtn_Layer.IsChecked = false;
                            }
                            else
                            {
                                layercontrol.Show();
                                layercontrol.currrentogbtn = TogBtn_Layer;
                                TogBtn_Layer.IsChecked = true;
                            }
                            break;
                        }
                    //清除
                    case "清除":
                        {
                            foreach (var item in SystemMainMap.Layers)
                            {
                                if ((item is GraphicsLayer) && MapNavControl.Dict_layer == null)
                                {
                                    (item as GraphicsLayer).ClearGraphics();
                                }
                                else
                                {
                                    if ((item is GraphicsLayer) && !MapNavControl.Dict_layer.Keys.ToList().Contains(item.ID))
                                    {
                                        (item as GraphicsLayer).ClearGraphics();
                                    }
                                }
                            }
                            SystemMainMap.Cursor = Cursors.Arrow;
                            break;
                        }
                    //图例
                    case "图例":
                        {
                            if (legend.Parent != null)
                            {
                                legend.Close();
                                TogBtn_Legend.IsChecked = false;
                            }
                            else
                            {
                                legend.Show();
                                legend.currrentogbtn = TogBtn_Legend;
                                TogBtn_Legend.IsChecked = true;
                            }
                        }
                        break;
                    //全屏显示
                    case "全屏显示":
                        {
                            Application.Current.Host.Content.IsFullScreen = !Application.Current.Host.Content.IsFullScreen;
                            break;
                        }
                    //坐标定位
                    case "坐标定位":
                        {
                            if (setcoordinate.Parent != null)
                            {
                                setcoordinate.Close();
                                TogBtn_SetCoor.IsChecked = false;
                            }
                            else
                            {
                                setcoordinate.Show();
                                TogBtn_SetCoor.IsChecked = true;
                            }
                            break;
                        }
                    case "影像切换":
                        {
                            if (SystemMainMap.Layers[1].Visible == false)
                            {
                                SystemMainMap.Layers[1].Visible = true;
                                SystemMainMap.Layers[2].Visible = true;
                            }
                            else
                            {
                                SystemMainMap.Layers[1].Visible = false;
                                SystemMainMap.Layers[2].Visible = false;
                            }
                            break;
                        }
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("错误信息" + ex.ToString());
            }
        }

        /// <summary>
        /// 地图范围变化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SystemMainMap_ExtentChanged(object sender, ExtentEventArgs e)
        {
            #region
            //if (e.OldExtent == null)
            //{
            //    MapExtentHistory.Add(e.NewExtent.Clone());
            //    CurrentExtentIndex++;
            //    return;
            //}

            //if (_newExtent)
            //{
            //    Envelope old_env = MapExtentHistory[MapExtentHistory.Count - 1];
            //    double o_xmax = double.Parse(old_env.XMax.ToString("0.000000"));
            //    double o_ymax = double.Parse(old_env.YMax.ToString("0.000000"));
            //    double o_xmin = double.Parse(old_env.XMin.ToString("0.000000"));
            //    double o_ymin = double.Parse(old_env.YMin.ToString("0.000000"));
            //    Envelope new_env = e.NewExtent;
            //    double n_xmax = double.Parse(new_env.XMax.ToString("0.000000"));
            //    double n_ymax = double.Parse(new_env.YMax.ToString("0.000000"));
            //    double n_xmin = double.Parse(new_env.XMin.ToString("0.000000"));
            //    double n_ymin = double.Parse(new_env.YMin.ToString("0.000000"));
            //    if (o_xmax != n_xmax && o_xmin != n_xmin && o_ymax != n_ymax && o_ymin != n_ymin)
            //    {
            //        MapExtentHistory.Add(new_env);
            //        CurrentExtentIndex++;
            //    }
            //}
            //else
            //{
            //    _newExtent = true;
            //}
            #endregion

            if (e.OldExtent == null)
            {
                //IsInitiaExt = false;
                MapExtentHistory.Add(e.NewExtent.Clone());
                m_baidu_ex = 0;
                return;
            }

            //20130926:如果属于第二次的地图视野变化后的自行“微颤”现象，则不处理此次变动（详见该变量定义处的描述）。
            m_baidu_ex++;
            if (m_baidu_ex > 1)
            {
                m_baidu_ex = 0;
                return;
            }

            if (_newExtent)
            {
                CurrentExtentIndex++;
                if (MapExtentHistory.Count - CurrentExtentIndex > 0)
                {
                    MapExtentHistory.RemoveRange(CurrentExtentIndex, (MapExtentHistory.Count - CurrentExtentIndex));
                }
                MapExtentHistory.Add(e.NewExtent.Clone());
                if (TogBtn_Pre.IsEnabled == false)
                {
                    TogBtn_Pre.IsEnabled = true;
                    TogBtn_Pre.Opacity = 1;
                }
            }
            else
            {
                SystemMainMap.IsHitTestVisible = true;
                _newExtent = true;
            }

            if (e.OldExtent.Height != e.NewExtent.Height)
            {
                if (SystemMainMap.Scale >= MapNavControl.DbScaleShow)
                {
                    for (int i = 0; i < MapNavControl.Dict_ckb.Count; i++)
                    {
                        CheckBox ckb = MapNavControl.Dict_ckb.Keys.ToList()[i];
                        //20150615zc:重大危险源不受控制
                        if (ckb.Content.Equals("重大危险源") || ckb.Content.Equals("企业")) continue;
                        ckb.IsChecked = false;
                    }
                }
                else
                {
                    for (int i = 0; i < MapNavControl.Dict_ckb.Count; i++)
                    {
                        CheckBox ckb = MapNavControl.Dict_ckb.Keys.ToList()[i];
                        //20150615zc:重大危险源不受控制
                        if (ckb.Content.Equals("重大危险源") || ckb.Content.Equals("企业")) continue;
                        ckb.IsChecked = MapNavControl.Dict_ckb.Values.ToList()[i];
                    }
                }
            }
            //if (SystemMainMap.Scale >= MapControl.DbScaleShow)
            //{
            //    for (int i = 0; i < MapControl.Dict_ckb.Count; i++)
            //    {
            //        CheckBox ckb = MapControl.Dict_ckb.Keys.ToList()[i];
            //        ckb.IsChecked = false;
            //    }
            //}
            //else
            //{
            //    for (int i = 0; i < MapControl.Dict_ckb.Count; i++)
            //    {
            //        CheckBox ckb = MapControl.Dict_ckb.Keys.ToList()[i];
            //        ckb.IsChecked = MapControl.Dict_ckb.Values.ToList()[i];
            //    }
            //}
        }

        /// <summary>
        /// 绘制完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MyDrawZoom_DrawComplete(object sender, DrawEventArgs e)
        {
            if (Mode_PreNext == "TogBtn_ZoomIn")
            {
                SystemMainMap.ZoomTo(e.Geometry as ESRI.ArcGIS.Client.Geometry.Envelope);
            }
            else if (Mode_PreNext == "TogBtn_ZoomOut")
            {
                Envelope currentExtent = SystemMainMap.Extent;

                Envelope zoomBoxExtent = e.Geometry as Envelope;
                MapPoint zoomBoxCenter = zoomBoxExtent.GetCenter();

                double whRatioCurrent = currentExtent.Width / currentExtent.Height;
                double whRatioZoomBox = zoomBoxExtent.Width / zoomBoxExtent.Height;

                Envelope newEnv = null;

                if (whRatioZoomBox > whRatioCurrent)
                {
                    double mapWidthPixels = SystemMainMap.Width;
                    double multiplier = currentExtent.Width / zoomBoxExtent.Width;
                    double newWidthMapUnits = currentExtent.Width * multiplier;
                    newEnv = new Envelope(new MapPoint(zoomBoxCenter.X - (newWidthMapUnits / 2), zoomBoxCenter.Y),
                                                   new MapPoint(zoomBoxCenter.X + (newWidthMapUnits / 2), zoomBoxCenter.Y));
                }
                else
                {
                    double mapHeightPixels = SystemMainMap.Height;
                    double multiplier = currentExtent.Height / zoomBoxExtent.Height;
                    double newHeightMapUnits = currentExtent.Height * multiplier;
                    newEnv = new Envelope(new MapPoint(zoomBoxCenter.X, zoomBoxCenter.Y - (newHeightMapUnits / 2)),
                                                   new MapPoint(zoomBoxCenter.X, zoomBoxCenter.Y + (newHeightMapUnits / 2)));
                }

                if (newEnv != null)
                    SystemMainMap.ZoomTo(newEnv);
            }

        }

        /// <summary>
        /// 菜单上移/下移
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void page_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as Button).Name)
            {
                case "btn_Top":
                    ScrollLst.PageUp();
                    break;
                case "btn_Bottom":
                    ScrollLst.PageDown();
                    break;
            }
        }

        #region 计算相对位置
        /// <summary>
        /// 左距
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public double LeftDistance(string content)
        {
            return 0;
        }


        /// <summary>
        /// 左距
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public double LeftDistance(int content)
        {
            return 0;
        }

        /// <summary>
        /// 测右边的距离
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public double RightDistance(int index)
        {
            return 0;
        }
        #endregion

        #region 右键菜单
        private void CreateRightMenu()
        {            
            ContextMenu cm = new ContextMenu();
            MenuItem MI_ZoomOut = new MenuItem();
            MI_ZoomOut.FontFamily = new System.Windows.Media.FontFamily("NSimSun");
            MI_ZoomOut.FontSize = 12;
            MI_ZoomOut.Header = "拉框缩小";
            ToolTipService.SetToolTip(MI_ZoomOut, "拉框缩小");
            MI_ZoomOut.Cursor = Cursors.Hand;
            Image image_ZoomOut = new Image() { Width = 16, Height = 16 };
            image_ZoomOut.Source = new BitmapImage(new Uri("../Image/MapTools/TogBtn_ZoomOut.png", UriKind.Relative));
            MI_ZoomOut.Icon = image_ZoomOut;
            MI_ZoomOut.Click += new RoutedEventHandler(MI_ZoomOut_Click);
            cm.Items.Add(MI_ZoomOut);

            MenuItem MI_ZoomIn = new MenuItem();
            MI_ZoomIn.FontFamily = new System.Windows.Media.FontFamily("NSimSun");
            MI_ZoomIn.FontSize = 12;
            MI_ZoomIn.Header = "拉框放大";
            ToolTipService.SetToolTip(MI_ZoomIn, "拉框放大");
            MI_ZoomIn.Cursor = Cursors.Hand;
            Image image_ZoomIn = new Image() { Width = 16, Height = 16 };
            image_ZoomIn.Source = new BitmapImage(new Uri("../Image/MapTools/TogBtn_ZoomIn.png", UriKind.Relative));
            MI_ZoomIn.Icon = image_ZoomIn;
            MI_ZoomIn.Click += new RoutedEventHandler(MI_ZoomIn_Click);
            cm.Items.Add(MI_ZoomIn);

            MenuItem MI_Pan = new MenuItem();
            MI_Pan.FontFamily = new System.Windows.Media.FontFamily("NSimSun");
            MI_Pan.FontSize = 12;
            MI_Pan.Header = "漫    游";
            ToolTipService.SetToolTip(MI_Pan, "漫游");
            MI_Pan.Cursor = Cursors.Hand;
            Image image_Pan = new Image() { Width = 16, Height = 16 };
            image_Pan.Source = new BitmapImage(new Uri("../Image/MapTools/TogBtn_Pan.png", UriKind.Relative));
            MI_Pan.Icon = image_Pan;
            MI_Pan.Click += new RoutedEventHandler(MI_Pan_Click);
            cm.Items.Add(MI_Pan);

            MenuItem MI_Full = new MenuItem();
            MI_Full.FontFamily = new System.Windows.Media.FontFamily("NSimSun");
            MI_Full.FontSize = 12;
            MI_Full.Header = "全    图";
            ToolTipService.SetToolTip(MI_Full, "全图");
            Image image_Full = new Image() { Width = 16, Height = 16 };
            image_Full.Source = new BitmapImage(new Uri("../Image/MapTools/TogBtn_FullMap.png", UriKind.Relative));
            MI_Full.Icon = image_Full;
            MI_Full.Cursor = Cursors.Hand;
            MI_Full.Click += new RoutedEventHandler(MI_Full_Click);
            cm.Items.Add(MI_Full);

            MenuItem MI_Clear = new MenuItem();
            MI_Clear.FontFamily = new System.Windows.Media.FontFamily("NSimSun");
            MI_Clear.FontSize = 12;
            MI_Clear.Header = "清    除";
            ToolTipService.SetToolTip(MI_Clear, "清除");
            Image image_Clear = new Image() { Width = 16, Height = 16 };
            image_Clear.Source = new BitmapImage(new Uri("../Image/MapTools/TogBtn_Clear.png", UriKind.Relative));
            MI_Clear.Icon = image_Clear;
            MI_Clear.Cursor = Cursors.Hand;
            MI_Clear.Click += new RoutedEventHandler(MI_Clear_Click);
            cm.Items.Add(MI_Clear);
            //ContextMenuService.SetContextMenu((this.Parent as Grid).Parent as Grid, cm);
            ContextMenuService.SetContextMenu(App.mainMap, cm);
        }

        /// <summary>
        /// 清除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MI_Clear_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton_Click(TogBtn_Clear, null);
            MyDrawZoom.IsEnabled = false;
            foreach (var item in SystemMainMap.Layers)
            {
                if (MapNavControl.Dict_layer != null)
                {
                    if ((item is GraphicsLayer) && !MapNavControl.Dict_layer.Keys.ToList().Contains(item.ID))
                    {
                        (item as GraphicsLayer).ClearGraphics();
                    }
                }
                else
                {
                    if (item is GraphicsLayer)
                    {
                        (item as GraphicsLayer).ClearGraphics();
                    }
                }
            }
            SystemMainMap.Cursor = Cursors.Arrow;
            TogBtn_Clear.IsChecked = false;
            TogBtn_Pan.IsChecked = true;
        }

        /// <summary>
        /// 全图
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MI_Full_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton_Click(TogBtn_FullMap, null);
            MyDrawZoom.IsEnabled = false;
            XElement xele = PFApp.Extent;
            Envelope eve = new Envelope()
            {
                XMax = double.Parse(xele.Element("MapExtent").Attribute("XMax").Value),
                XMin = double.Parse(xele.Element("MapExtent").Attribute("XMin").Value),
                YMax = double.Parse(xele.Element("MapExtent").Attribute("YMax").Value),
                YMin = double.Parse(xele.Element("MapExtent").Attribute("YMin").Value),
                SpatialReference = SystemMainMap.SpatialReference
            };
            SystemMainMap.ZoomTo(eve);
            TogBtn_Pan.IsChecked = true;
            TogBtn_FullMap.IsChecked = false;
        }

        /// <summary>
        /// 平移
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MI_Pan_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton_Click(TogBtn_Pan, null);
            MyDrawZoom.IsEnabled = false;
            SystemMainMap.Cursor = Cursors.Arrow;
        }

        /// <summary>
        /// 放大
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MI_ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton_Click(TogBtn_ZoomIn, null);
            MyDrawZoom.IsEnabled = true;
            Mode_PreNext = "TogBtn_ZoomIn";
        }

        /// <summary>
        /// 缩小
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MI_ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton_Click(TogBtn_ZoomOut, null);
            MyDrawZoom.IsEnabled = true;
            Mode_PreNext = "TogBtn_ZoomOut";
        }
        #endregion

        #region JS接口
        /// <summary>
        /// 响应Gis平台主页面MainPage的调用并返回值(20120724)
        /// </summary>
        /// <param name="s1">menu name</param>
        /// <param name="s2">要传入的值</param>
        /// <returns></returns>
        public string LinkGetDataFromThisMainPage(string s1, string s2)
        {
            //指向对应页面
            string rs = LinkToPage(s1, s2);
            return rs;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="n">menu name</param>
        /// <param name="s">要传入的值</param>
        /// <returns></returns>
        private string LinkToPage(string n, string s)
        {
            string rs = string.Empty;

            foreach (var item in App.Menus)
            {

                if (item.MenuName == n)
                {
                    //MessageBox.Show("指向 MenuName: " + item.MenuName);
                    IPart myPart = PFApp.UIS[n] as IPart;
                    rs = myPart.LinkReturnGisPlatform("test", s);
                    //MessageBox.Show("return from " + item.MenuName + " : " + rs);
                }
            }
            return rs;
        }
        #endregion

        /// <summary>
        /// 对话框必须由用户事件打开(20120727zc)
        /// </summary>
        public void SaveImgFile()
        {
            string strSave = "要保存的内容";
            SaveFileDialog sfd = new SaveFileDialog()
            {
                DefaultExt = "txt",
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                FilterIndex = 2
            };
            if (sfd.ShowDialog() == true)
            {
                //FileName.Text = "文件名称：" + sfd.File.Name;
                using (Stream stream = sfd.OpenFile())
                {
                    Byte[] fileContent = System.Text.UTF8Encoding.UTF8.GetBytes(strSave);
                    stream.Write(fileContent, 0, fileContent.Length);
                    stream.Close();
                }
            }

        }
    }
}