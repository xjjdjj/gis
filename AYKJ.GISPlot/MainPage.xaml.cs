using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using System.Windows.Printing;
using System.Linq;
using System.Windows.Shapes;
using PlotApiForArcGIS;
using AYKJ.GISDevelop.Platform;
using System.Xml.Linq;
using AYKJ.GISDevelop.Platform.Part;

namespace AYKJ.GISPlot
{
    public partial class MainPage : UserControl, IWidgets
    {
        #region 参数定义
        //标绘Draw
        public clsIPlotDraw plotdraw;
        //标绘图层
        private GraphicsLayer PlotGraphicsLayer;
        //是否编辑标绘
        private bool IsEdit;
        private string strplottype;
        //编辑标绘
        private EditGeometry editgeometry;
        //是否删除标绘
        private bool IsDelete;
        //接收颜色拾取器的Grid
        private Grid grid_color;
        //应急Draw
        private Draw agsDraw;
        //文字大小
        private Dictionary<string, double> Dict_TxtFontSize; 

        //文字字体
        private Dictionary<string, string> Dict_TxtFontFamily;
        //地图
        Map PlotMap;
        //Nav列表
        private Dictionary<string,RadioButton> Dict_RadioButtons;
        private Dictionary<string, StackPanel> Dict_StackPanels;
        private Dictionary<string, Grid> Dict_Grids;
        #endregion

        public MainPage()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            //设置面板的起始位置
            this.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            this.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            this.Margin = new Thickness() { Top = 10, Right = 13 };

            PlotMap = (Application.Current as IApp).MainMap;
            //实例化标绘图层
            PlotGraphicsLayer = new GraphicsLayer();
            //注册标绘图层事件
            PlotGraphicsLayer.MouseLeftButtonDown += new GraphicsLayer.MouseButtonEventHandler(PlotGraphicsLayer_MouseLeftButtonDown);
            //将标绘图层加到地图上
            PlotMap.Layers.Add(PlotGraphicsLayer);
            //将编辑状态设为不可用
            IsEdit = false;
            //将删除状态设为不可用
            IsDelete = false;

            //实例化编辑的空间对象
            editgeometry = new EditGeometry();
            editgeometry.Map = PlotMap;
            editgeometry.IsEnabled = true;
            editgeometry.EditVerticesEnabled = false;

            //实例化标绘Draw
            plotdraw = new clsIPlotDraw(PlotMap);
            plotdraw.setPlotDrawMode(clsIPlotDrawMode.None);
            //注册标绘Draw的完成事件
            plotdraw.DrawEnd -= _plotDraw_DrawEnd;
            plotdraw.DrawEnd += new clsIPlotDraw.DrawEndEventHandler(_plotDraw_DrawEnd);

            //实例化地图普通的Draw
            agsDraw = new Draw(PlotMap);
            SimpleFillSymbol simpleFillSymbol = new SimpleFillSymbol();
            simpleFillSymbol.Fill = new SolidColorBrush(Color.FromArgb(55, 0, 0, 0));
            simpleFillSymbol.BorderBrush = new SolidColorBrush(Colors.Black);
            agsDraw.FillSymbol = simpleFillSymbol;
            //注册地图普通的Draw的完成事件
            agsDraw.DrawComplete += new EventHandler<DrawEventArgs>(agsDraw_DrawComplete);
            //将地图Draw设为不可用
            agsDraw.IsEnabled = false;

            #region 增加填充样式的下拉列表框
            List<string> lsttype = new List<string>();
            lsttype.Add("无");
            lsttype.Add("横条纹");
            lsttype.Add("左斜条纹");
            lsttype.Add("右斜条纹");
            lsttype.Add("竖条纹");
            cbx_Monochrome.ItemsSource = lsttype;
            cbx_Monochrome.SelectedIndex = 0;
            #endregion

            #region 增加字体样式的下拉列表框
            Dict_TxtFontFamily = new Dictionary<string, string>();
            Dict_TxtFontFamily.Add("宋体", "NSimSun");
            Dict_TxtFontFamily.Add("楷体", "KaiTi");
            Dict_TxtFontFamily.Add("黑体", "SimHei");
            Dict_TxtFontFamily.Add("仿宋", "FangSong");
            cbx_TxtFontFamily.ItemsSource = Dict_TxtFontFamily.Keys.ToList();
            cbx_TxtFontFamily.SelectedIndex = 0;
            #endregion

            #region 增加字体大小的下拉列表框
            Dict_TxtFontSize = new Dictionary<string, double>();
            Dict_TxtFontSize.Add("12号", 12);
            Dict_TxtFontSize.Add("14号", 14);
            Dict_TxtFontSize.Add("16号", 16);
            Dict_TxtFontSize.Add("18号", 18);
            Dict_TxtFontSize.Add("20号", 20);
            Dict_TxtFontSize.Add("22号", 22);
            Dict_TxtFontSize.Add("24号", 24);
            cbx_TxtFontSize.ItemsSource = Dict_TxtFontSize.Keys.ToList();
            cbx_TxtFontSize.SelectedIndex = 0;
            #endregion



            CreatePlotThematicButton();

            //20140320：新疆项目，增加多种标绘图片
            //CreatePlotThematicButton1();

            //CreatePlotThematicButton2();

            //CreatePlotThematicButton3();

            //CreatePlotThematicButton4();

            Storyboard_Close.Completed += new EventHandler(Storyboard_Close_Completed);
        }

        /// <summary>
        /// 动态创建专题按钮
        /// </summary>
        void CreatePlotThematicButton()
        {
            
            XElement xele = PFApp.Extent;

            var plots = xele.Elements("PlotImage").ToArray();
            if (plots.Length > 1)
            {
                //清空导航栏后动态添加
                SpNav.Children.Clear();
                SpNav.Visibility = Visibility.Visible;
                SP_PlotThematic.Children.Clear();
                Dict_RadioButtons = new Dictionary<string, RadioButton>();
                Dict_StackPanels = new Dictionary<string, StackPanel>();
                Dict_Grids = new Dictionary<string, Grid>();
                
                for (int i = 0; i < plots.Length; i++)
                {
                    string[] arysource = plots[i].Attribute("Source").Value.TrimEnd(';').Split(';');
                    string[] aryname = plots[i].Attribute("Name").Value.TrimEnd(';').Split(';');
                    string type = plots[i].Attribute("Type").Value;
                    bool b = i == 0;
                    RadioButton rb = ThematicRadio(type, b);
                    SpNav.Children.Add(rb);
                    //Dict_RadioButtons.Add(type, rb);

                    //StackPanel sp = ThematicStackPanel();

                    if (b)
                    {
                        Dict_Grids.Add(type, grid_ImgDefault);
                        //SP_PlotThematic.Children.Add(sp);
                        CreatePlotThematicStackPanel(arysource, aryname, SpBtn, b);
                    }
                    else
                    {
                        StackPanel sp = new StackPanel();
                        CreatePlotThematicStackPanel(arysource, aryname, sp, b);
                        Grid gr = ThematicGrid(b);
                        gr.Children.Add(sp);
                        grd_ImgesBtns.Children.Add(gr);
                        
                        Dict_Grids.Add(type, gr);
                    }
                    
                    //Dict_StackPanels.Add(type,sp);

                    
                }
            }
            else
            {
                SP_PlotThematic.Children.Clear();
                string[] arysource = xele.Element("PlotImage").Attribute("Source").Value.TrimEnd(';').Split(';');
                string[] aryname = xele.Element("PlotImage").Attribute("Name").Value.TrimEnd(';').Split(';');
                CreatePlotThematicStackPanel(arysource, aryname, SP_PlotThematic,true);
            }           
        }

        void CreatePlotThematicStackPanel(string[] arysource,string[] aryname,StackPanel sp_plot,bool isDefault)
        {
            int row1 = arysource.Length / 7;
            for (int k = 0; k <= row1; k++)
            {
                StackPanel sp;
                if (isDefault && k == 0)
                {
                    sp = SP_PlotThematic;
                    for (int i = 0; (i + 7 * k) < arysource.Length && i < 7; i++)
                        sp.Children.Add(ThematicButton(arysource[i + 7 * k], aryname[i + 7 * k]));
                }
                else
                {
                    sp = ThematicStackPanel();
                    for (int i = 0; (i + 7 * k) < arysource.Length && i < 7; i++)
                        sp.Children.Add(ThematicButton(arysource[i + 7 * k], aryname[i + 7 * k]));
                    sp_plot.Children.Add(sp);
                }                                                
            }
        }

        Grid ThematicGrid(bool isVisible)
        {
            Grid gr = new Grid()
            {
                Margin=new Thickness(0),
                Visibility=isVisible?Visibility.Visible:Visibility.Collapsed                
            };
            return gr;
        }

        StackPanel ThematicStackPanel()
        {
            StackPanel sp = new StackPanel()
            {                
                Orientation = System.Windows.Controls.Orientation.Horizontal,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                VerticalAlignment = System.Windows.VerticalAlignment.Top,
                Height = 27,
                Margin = new Thickness() { Bottom = 0, Left = 10, Right = 0, Top = 10 }
            };
            return sp;
        }

        RadioButton ThematicRadio(string type,bool isChecked)
        {
            //一个专题标签配一个专题sp
            RadioButton rb = new RadioButton()
            {
                Content=type,
                Style=this.Resources["RadioButtonStyle1"] as Style,
                Width=type.Length<3?33:66,
                HorizontalAlignment=System.Windows.HorizontalAlignment.Left,
                Margin=new Thickness(2,0,0,0),
                IsChecked=isChecked,
                //Foreground= new Brush "#FF7EC307",
                Foreground = new SolidColorBrush(Color.FromArgb(255, 126, 195, 7)),
                Cursor=Cursors.Hand
            };
        
            rb.Checked += new RoutedEventHandler(rbtn_Checked1);
            return rb;
        }



        Button ThematicButton(string strsource, string strname)
        {
            Button btn = new Button()
            {
                Width = 24,
                Height = 24,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0),
                FontFamily = new FontFamily("NSimSun"),
                FontSize = 12,
                Cursor = Cursors.Hand,
                Content = strsource,
                Style = this.Resources["ButtonStyle_Plot"] as Style,
                Tag = "专题"
            };
            btn.Click += new RoutedEventHandler(btn_Plot_Click);
            //ToolTipService.SetToolTip(btn, strsource.Split('/')[strsource.Split('/').Length - 1].Replace(".png", ""));
            ToolTipService.SetToolTip(btn, strname);
            return btn;
        }

        /// <summary>
        /// 标绘事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Plot_Click(object sender, RoutedEventArgs e)
        {
            plotdraw.setPlotDrawMode(clsIPlotDrawMode.None);
            agsDraw.IsEnabled = false;
            IsDelete = false;
            IsEdit = false;
            switch ((sender as Button).Tag.ToString())
            {
                #region 箭头
                case "简单箭头":
                    plotdraw.setPlotDrawMode(clsIPlotDrawMode.IArrowSimple);
                    break;
                case "尾箭头":
                    plotdraw.setPlotDrawMode(clsIPlotDrawMode.IArrowTailed);
                    break;
                case "直箭头":
                    plotdraw.setPlotDrawMode(clsIPlotDrawMode.IArrowStraight);
                    break;
                case "自定义箭头":
                    plotdraw.setPlotDrawMode(clsIPlotDrawMode.IArrowCustom);
                    break;
                case "自定义尾箭头":
                    plotdraw.setPlotDrawMode(clsIPlotDrawMode.IArrowCustomTailed);
                    break;
                case "双箭头":
                    plotdraw.setPlotDrawMode(clsIPlotDrawMode.IArrowDouble);
                    break;
                #endregion
                #region 标志
                case "集合区":
                    plotdraw.setPlotDrawMode(clsIPlotDrawMode.IAssemblyArea);
                    break;
                case "曲线旗帜":
                    plotdraw.setPlotDrawMode(clsIPlotDrawMode.IFlagCurve);
                    break;
                case "矩形旗帜":
                    plotdraw.setPlotDrawMode(clsIPlotDrawMode.IFlagRect);
                    break;
                case "三角旗帜":
                    plotdraw.setPlotDrawMode(clsIPlotDrawMode.IFlagTriangle);
                    break;
                case "圆形区域":
                    plotdraw.setPlotDrawMode(clsIPlotDrawMode.IAssemblyCircle);
                    break;
                case "三角形":
                    strplottype = "三角形";
                    agsDraw.DrawMode = DrawMode.Triangle;
                    agsDraw.IsEnabled = true;
                    break;
                case "椭圆":
                    strplottype = "椭圆";
                    agsDraw.DrawMode = DrawMode.Ellipse;
                    agsDraw.IsEnabled = true;
                    break;
                #endregion
                #region 功能
                case "开始编辑":
                    IsEdit = true;
                    break;
                case "删除标绘":
                    IsDelete = true;
                    break;
                #endregion
                #region 应急
                case "火灾":
                    strplottype = "Icon_Fire";
                    agsDraw.DrawMode = DrawMode.Point;
                    agsDraw.IsEnabled = true;
                    break;
                case "泄漏":
                    strplottype = "Icon_Leakage";
                    agsDraw.DrawMode = DrawMode.Point;
                    agsDraw.IsEnabled = true;
                    break;
                case "专题":
                    //strplottype = ToolTipService.GetToolTip((sender as Button)).ToString();
                    strplottype = (sender as Button).Content.ToString();
                    agsDraw.DrawMode = DrawMode.Point;
                    agsDraw.IsEnabled = true;
                    break;
                #endregion
                #region 文字
                case "指向文字":
                    strplottype = "指向文字";
                    agsDraw.DrawMode = DrawMode.Point;
                    agsDraw.IsEnabled = true;
                    break;
                case "默认文字":
                    strplottype = "默认文字";
                    agsDraw.DrawMode = DrawMode.Point;
                    agsDraw.IsEnabled = true;
                    break;
                #endregion
            }
        }

        /// <summary>
        /// 矢量标绘图标绘制完成
        /// </summary>
        /// <param name="polygon"></param>
        void _plotDraw_DrawEnd(ESRI.ArcGIS.Client.Geometry.Polygon polygon)
        {
            plotdraw.setPlotDrawMode(clsIPlotDrawMode.None);
            Graphic plotgra = new Graphic();
            plotgra.Geometry = polygon;
            if (rbtn_Monochrome.IsChecked == true)
            {
                switch (cbx_Monochrome.SelectedItem.ToString())
                {
                    case "无":
                        plotgra.Symbol = MonochromeFillSymbol();
                        break;
                    case "横条纹":
                        plotgra.Symbol = HorFillSymbol();
                        break;
                    case "左斜条纹":
                        plotgra.Symbol = LeftFillSymbol();
                        break;
                    case "右斜条纹":
                        plotgra.Symbol = RightFillSymbol();
                        break;
                    case "竖条纹":
                        plotgra.Symbol = VerFillSymbol();
                        break;
                }
            }
            else
            {
                if (ckbx_IsAnimation.IsChecked == false)
                {
                    plotgra.Symbol = GradientFillSymbol();
                }
                else
                {

                    plotgra.Attributes.Add("StaFill", GradientLinearGradientBrush());
                    plotgra.Attributes.Add("StaStrokeColor", new SolidColorBrush()
                    {
                        Opacity = double.Parse(txt_ContourOpacity.Text),
                        Color = (btn_ContourColor.Background as SolidColorBrush).Color
                    });
                    plotgra.Attributes.Add("StaStrokeThickness", double.Parse(txt_ContourWidth.Text));
                    plotgra.Symbol = AnimationFillSymbol;
                }

            }
            PlotGraphicsLayer.Graphics.Add(plotgra);
        }

        /// <summary>
        /// 应用图标标绘绘制完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void agsDraw_DrawComplete(object sender, DrawEventArgs e)
        {
            agsDraw.IsEnabled = false;
            Graphic tmpgra = new Graphic();
            tmpgra.Geometry = e.Geometry;
            if (strplottype == "Icon_Leakage")
            {
                tmpgra.Symbol = LeakSymbol;
            }
            else if (strplottype == "Icon_Fire")
            {
                tmpgra.Symbol = FireSymbol;
            }
            else if (strplottype == "指向文字")
            {
                AddPointingText(e.Geometry);
            }
            else if (strplottype == "默认文字")
            {
                AddDefaultText(e.Geometry);
            }
            else if (strplottype == "三角形")
            {
                _plotDraw_DrawEnd(e.Geometry as ESRI.ArcGIS.Client.Geometry.Polygon);
            }
            else if (strplottype == "椭圆")
            {
                _plotDraw_DrawEnd(e.Geometry as ESRI.ArcGIS.Client.Geometry.Polygon);
            }
            else
            {
                PictureMarkerSymbol picturemarkersymbol = new PictureMarkerSymbol();
                //XElement xele = PFApp.Extent;
                //string[] arysource = xele.Element("PlotImage").Attribute("Source").Value.TrimEnd(';').Split(';');
                //string[] aryname = xele.Element("PlotImage").Attribute("Name").Value.TrimEnd(';').Split(';');
                //for(int)
                //picturemarkersymbol.Source = new BitmapImage(new Uri("/Image/Plot/Thematic/" + strplottype + ".png", UriKind.Relative));
                picturemarkersymbol.Source = new BitmapImage(new Uri("/" + strplottype, UriKind.Relative));
                picturemarkersymbol.OffsetX = 16;
                picturemarkersymbol.OffsetY = 16;
                tmpgra.Symbol = picturemarkersymbol;
            }
            PlotGraphicsLayer.Graphics.Add(tmpgra);
        }

        /// <summary>
        /// 点击标绘图标进行编辑或者删除操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PlotGraphicsLayer_MouseLeftButtonDown(object sender, GraphicMouseButtonEventArgs e)
        {
            if (IsEdit == true)
            {
                e.Handled = true;
                if (e.Graphic.Geometry is MapPoint)
                {
                    e.Graphic.Selected = true;
                    Graphic selectedPointGraphic = e.Graphic;
                }
                else
                {
                    editgeometry.StartEdit(e.Graphic);
                }
            }
            if (IsDelete == true)
            {
                PlotGraphicsLayer.Graphics.Remove(e.Graphic);
            }
        }

        #region 标绘样式控制
        private void rbtn_Monochrome_Checked(object sender, RoutedEventArgs e)
        {
            if (rbtn_Gradient != null && rbtn_Monochrome != null)
            {
                rbtn_Gradient.IsChecked = false;
                grid_Monochrome.Visibility = System.Windows.Visibility.Visible;
                grid_Gradient.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void rbtn_Gradient_Checked(object sender, RoutedEventArgs e)
        {
            if (rbtn_Gradient != null && rbtn_Monochrome != null)
            {
                rbtn_Monochrome.IsChecked = false;
                grid_Monochrome.Visibility = System.Windows.Visibility.Collapsed;
                grid_Gradient.Visibility = System.Windows.Visibility.Visible;
            }
        }
        #endregion

        #region 透明度控制
        private void sld_ContourOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sld_ContourOpacity != null)
            {
                txt_ContourOpacity.Text = sld_ContourOpacity.Value.ToString("0.0");
            }
        }

        private void sld_ContourWidth_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sld_ContourWidth != null)
            {
                txt_ContourWidth.Text = sld_ContourWidth.Value.ToString("0");
            }
        }

        private void sld_GradientOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sld_GradientOpacity != null)
            {
                txt_GradientOpacity.Text = sld_GradientOpacity.Value.ToString("0.0");
            }
        }

        private void sld_MonochromeOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sld_MonochromeOpacity != null)
            {
                txt_MonochromeOpacity.Text = sld_MonochromeOpacity.Value.ToString("0.0");
            }
        }

        private void sld_TxtBlackColorOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sld_TxtBlackColorOpacity != null)
            {
                txt_TxtBlackColorOpacity.Text = sld_TxtBlackColorOpacity.Value.ToString("0.0");
            }
        }
        #endregion

        #region 颜色拾取
        private void btn_SelectColor_Click(object sender, MouseButtonEventArgs e)
        {
            grid_color = sender as Grid;
            ColorPickerWin colorpickerwin = new ColorPickerWin();
            colorpickerwin.selectedColor += new SelectedColor(colorpickerwin_selectedColor);
            colorpickerwin.Show();
        }

        private void btn_SelectColor_Red_Click(object sender, MouseButtonEventArgs e)
        {
            //grid_color = sender as Grid;
            //ColorPickerWin colorpickerwin = new ColorPickerWin();
            //colorpickerwin.selectedColor += new SelectedColor(colorpickerwin_selectedColor);
            //colorpickerwin.Show();
            
            btn_MonochromeColor.Background = new SolidColorBrush(((sender as Grid).Background as SolidColorBrush).Color);
        }

        private void btn_SelectColor_Green_Click(object sender, MouseButtonEventArgs e)
        {
            //grid_color = sender as Grid;
            //ColorPickerWin colorpickerwin = new ColorPickerWin();
            //colorpickerwin.selectedColor += new SelectedColor(colorpickerwin_selectedColor);
            //colorpickerwin.Show();
            btn_MonochromeColor.Background = new SolidColorBrush(((sender as Grid).Background as SolidColorBrush).Color);
        }

        void colorpickerwin_selectedColor(Color strColor)
        {
            grid_color.Background = new SolidColorBrush(strColor);
        }
        #endregion

        #region 填充样式
        /// <summary>
        /// 纯色填充样式
        /// </summary>
        /// <returns></returns>
        SimpleFillSymbol MonochromeFillSymbol()
        {
            SimpleFillSymbol simpleFillSymbol = new SimpleFillSymbol();
            simpleFillSymbol.Fill = new SolidColorBrush()
            {
                Opacity = double.Parse(txt_MonochromeOpacity.Text),
                Color = (btn_MonochromeColor.Background as SolidColorBrush).Color
            };
            simpleFillSymbol.BorderThickness = double.Parse(txt_ContourWidth.Text);
            simpleFillSymbol.BorderBrush = new SolidColorBrush()
            {
                Opacity = double.Parse(txt_ContourOpacity.Text),
                Color = (btn_ContourColor.Background as SolidColorBrush).Color
            };
            return simpleFillSymbol;
        }

        /// <summary>
        /// 渐变样式
        /// </summary>
        /// <returns></returns>
        FillSymbol GradientFillSymbol()
        {
            FillSymbol fillsymbol = new FillSymbol();
            fillsymbol.Fill = GradientLinearGradientBrush();
            fillsymbol.BorderThickness = double.Parse(txt_ContourWidth.Text);
            fillsymbol.BorderBrush = new SolidColorBrush()
            {
                Opacity = double.Parse(txt_ContourOpacity.Text),
                Color = (btn_ContourColor.Background as SolidColorBrush).Color
            };
            return fillsymbol;
        }

        LinearGradientBrush GradientLinearGradientBrush()
        {
            LinearGradientBrush lineargradientbrush = new LinearGradientBrush();
            if (rbtn_LeftRight.IsChecked == true)
            {
                lineargradientbrush.StartPoint = new Point(1, 0.5);
                lineargradientbrush.EndPoint = new Point(0, 0.5);
            }
            else if (rbtn_RightLeft.IsChecked == true)
            {
                lineargradientbrush.StartPoint = new Point(0, 0.5);
                lineargradientbrush.EndPoint = new Point(1, 0.5);
            }
            else if (rbtn_TopBottom.IsChecked == true)
            {
                lineargradientbrush.StartPoint = new Point(0.5, 1);
                lineargradientbrush.EndPoint = new Point(0.5, 0);
            }
            else if (rbtn_BottomTop.IsChecked == true)
            {
                lineargradientbrush.StartPoint = new Point(0.5, 0);
                lineargradientbrush.EndPoint = new Point(0.5, 1);
            }

            GradientStop gradientstop1 = new GradientStop();
            gradientstop1.Color = new Color()
            {
                A = byte.Parse((double.Parse(txt_GradientOpacity.Text) * 255).ToString("0")),
                R = (btn_GradientColorFirst.Background as SolidColorBrush).Color.R,
                G = (btn_GradientColorFirst.Background as SolidColorBrush).Color.G,
                B = (btn_GradientColorFirst.Background as SolidColorBrush).Color.B,
            };
            gradientstop1.Offset = 0;

            GradientStop gradientstop2 = new GradientStop();
            gradientstop2.Color = new Color()
            {
                A = byte.Parse((double.Parse(txt_GradientOpacity.Text) * 255).ToString("0")),
                R = (btn_GradientColorSecond.Background as SolidColorBrush).Color.R,
                G = (btn_GradientColorSecond.Background as SolidColorBrush).Color.G,
                B = (btn_GradientColorSecond.Background as SolidColorBrush).Color.B,
            };
            gradientstop2.Offset = 1;

            lineargradientbrush.GradientStops.Add(gradientstop1);
            lineargradientbrush.GradientStops.Add(gradientstop2);

            return lineargradientbrush;
        }

        /// <summary>
        /// 横条纹样式
        /// </summary>
        /// <returns></returns>
        FillSymbol HorFillSymbol()
        {
            FillSymbol fillsymbol = new FillSymbol();
            fillsymbol.Fill = HorLinearGradientBrush();
            fillsymbol.BorderThickness = double.Parse(txt_ContourWidth.Text);
            fillsymbol.BorderBrush = new SolidColorBrush()
            {
                Opacity = double.Parse(txt_ContourOpacity.Text),
                Color = (btn_ContourColor.Background as SolidColorBrush).Color
            };
            return fillsymbol;
        }

        LinearGradientBrush HorLinearGradientBrush()
        {
            LinearGradientBrush lineargradientbrush = new LinearGradientBrush();
            lineargradientbrush.MappingMode = BrushMappingMode.Absolute;
            lineargradientbrush.SpreadMethod = GradientSpreadMethod.Repeat;
            lineargradientbrush.StartPoint = new Point(0, 6);
            lineargradientbrush.EndPoint = new Point(0, 0);

            GradientStop gradientstop1 = new GradientStop();
            gradientstop1.Color = new Color()
            {
                A = byte.Parse((double.Parse(txt_MonochromeOpacity.Text) * 255).ToString("0")),
                R = (btn_MonochromeColor.Background as SolidColorBrush).Color.R,
                G = (btn_MonochromeColor.Background as SolidColorBrush).Color.G,
                B = (btn_MonochromeColor.Background as SolidColorBrush).Color.B,
            };
            gradientstop1.Offset = 0.2;

            GradientStop gradientstop2 = new GradientStop();
            gradientstop2.Color = new Color() { A = 0, R = 255, G = 255, B = 255 };
            gradientstop2.Offset = 0.2;

            lineargradientbrush.GradientStops.Add(gradientstop1);
            lineargradientbrush.GradientStops.Add(gradientstop2);

            return lineargradientbrush;
        }

        /// <summary>
        /// 竖条纹样式
        /// </summary>
        /// <returns></returns>
        FillSymbol VerFillSymbol()
        {
            FillSymbol fillsymbol = new FillSymbol();
            fillsymbol.Fill = VerLinearGradientBrush();
            fillsymbol.BorderThickness = double.Parse(txt_ContourWidth.Text);
            fillsymbol.BorderBrush = new SolidColorBrush()
            {
                Opacity = double.Parse(txt_ContourOpacity.Text),
                Color = (btn_ContourColor.Background as SolidColorBrush).Color
            };
            return fillsymbol;
        }

        LinearGradientBrush VerLinearGradientBrush()
        {
            LinearGradientBrush lineargradientbrush = new LinearGradientBrush();
            lineargradientbrush.MappingMode = BrushMappingMode.Absolute;
            lineargradientbrush.SpreadMethod = GradientSpreadMethod.Repeat;
            lineargradientbrush.StartPoint = new Point(4, 0);
            lineargradientbrush.EndPoint = new Point(0, 0);

            GradientStop gradientstop1 = new GradientStop();
            gradientstop1.Color = new Color()
            {
                A = byte.Parse((double.Parse(txt_MonochromeOpacity.Text) * 255).ToString("0")),
                R = (btn_MonochromeColor.Background as SolidColorBrush).Color.R,
                G = (btn_MonochromeColor.Background as SolidColorBrush).Color.G,
                B = (btn_MonochromeColor.Background as SolidColorBrush).Color.B,
            };
            gradientstop1.Offset = 0.2;

            GradientStop gradientstop2 = new GradientStop();
            gradientstop2.Color = new Color() { A = 0, R = 255, G = 255, B = 255 };
            gradientstop2.Offset = 0.2;

            lineargradientbrush.GradientStops.Add(gradientstop1);
            lineargradientbrush.GradientStops.Add(gradientstop2);
            return lineargradientbrush;
        }

        /// <summary>
        /// 左斜条纹样式
        /// </summary>
        /// <returns></returns>
        FillSymbol LeftFillSymbol()
        {
            FillSymbol fillsymbol = new FillSymbol();
            fillsymbol.Fill = LeftLinearGradientBrush();
            fillsymbol.BorderThickness = double.Parse(txt_ContourWidth.Text);
            fillsymbol.BorderBrush = new SolidColorBrush()
            {
                Opacity = double.Parse(txt_ContourOpacity.Text),
                Color = (btn_ContourColor.Background as SolidColorBrush).Color
            };
            return fillsymbol;
        }

        LinearGradientBrush LeftLinearGradientBrush()
        {
            LinearGradientBrush lineargradientbrush = new LinearGradientBrush();
            lineargradientbrush.MappingMode = BrushMappingMode.Absolute;
            lineargradientbrush.SpreadMethod = GradientSpreadMethod.Repeat;
            lineargradientbrush.StartPoint = new Point(4, 0);
            lineargradientbrush.EndPoint = new Point(0, 4);

            GradientStop gradientstop1 = new GradientStop();
            gradientstop1.Color = new Color()
            {
                A = byte.Parse((double.Parse(txt_MonochromeOpacity.Text) * 255).ToString("0")),
                R = (btn_MonochromeColor.Background as SolidColorBrush).Color.R,
                G = (btn_MonochromeColor.Background as SolidColorBrush).Color.G,
                B = (btn_MonochromeColor.Background as SolidColorBrush).Color.B,
            };
            gradientstop1.Offset = 0.2;

            GradientStop gradientstop2 = new GradientStop();
            gradientstop2.Color = new Color() { A = 0, R = 255, G = 255, B = 255 };
            gradientstop2.Offset = 0.2;

            lineargradientbrush.GradientStops.Add(gradientstop1);
            lineargradientbrush.GradientStops.Add(gradientstop2);

            return lineargradientbrush;
        }

        /// <summary>
        /// 右斜条纹样式
        /// </summary>
        /// <returns></returns>
        FillSymbol RightFillSymbol()
        {
            FillSymbol fillsymbol = new FillSymbol();
            fillsymbol.Fill = RightLinearGradientBrush();
            fillsymbol.BorderThickness = double.Parse(txt_ContourWidth.Text);
            fillsymbol.BorderBrush = new SolidColorBrush()
            {
                Opacity = double.Parse(txt_ContourOpacity.Text),
                Color = (btn_ContourColor.Background as SolidColorBrush).Color
            };
            return fillsymbol;
        }

        LinearGradientBrush RightLinearGradientBrush()
        {
            LinearGradientBrush lineargradientbrush = new LinearGradientBrush();
            lineargradientbrush.MappingMode = BrushMappingMode.Absolute;
            lineargradientbrush.SpreadMethod = GradientSpreadMethod.Repeat;
            lineargradientbrush.StartPoint = new Point(4, 4);
            lineargradientbrush.EndPoint = new Point(0, 0);

            GradientStop gradientstop1 = new GradientStop();
            gradientstop1.Color = new Color()
            {
                A = byte.Parse((double.Parse(txt_MonochromeOpacity.Text) * 255).ToString("0")),
                R = (btn_MonochromeColor.Background as SolidColorBrush).Color.R,
                G = (btn_MonochromeColor.Background as SolidColorBrush).Color.G,
                B = (btn_MonochromeColor.Background as SolidColorBrush).Color.B,
            };
            gradientstop1.Offset = 0.2;

            GradientStop gradientstop2 = new GradientStop();
            gradientstop2.Color = new Color() { A = 0, R = 255, G = 255, B = 255 };
            gradientstop2.Offset = 0.2;

            lineargradientbrush.GradientStops.Add(gradientstop1);
            lineargradientbrush.GradientStops.Add(gradientstop2);

            return lineargradientbrush;
        }
        #endregion

        #region 添加文字
        /// <summary>
        /// 添加指向性文字
        /// </summary>
        /// <param name="tmpgeo"></param>
        void AddPointingText(ESRI.ArcGIS.Client.Geometry.Geometry tmpgeo)
        {
            Graphic tmpgra = new Graphic();
            tmpgra.Geometry = tmpgeo;

            double dbwidth = double.Parse(txt_ContourWidth.Text);
            tmpgra.Attributes.Add("StaBorderColor", ToHexColor((btn_ContourColor.Background as SolidColorBrush).Color, byte.Parse((double.Parse(txt_ContourOpacity.Text) * 255).ToString("0"))));
            tmpgra.Attributes.Add("StaBorderThickness1", dbwidth);
            tmpgra.Attributes.Add("StaBorderThickness2", new Thickness(dbwidth, dbwidth, 0, dbwidth));
            tmpgra.Attributes.Add("StaBorderThickness3", new Thickness(0, dbwidth, 0, 0));
            tmpgra.Attributes.Add("StaBorderThickness4", new Thickness(0, dbwidth, dbwidth, dbwidth));
            tmpgra.Attributes.Add("StaBackColor", ToHexColor((btn_TxtBlackColor.Background as SolidColorBrush).Color, byte.Parse((double.Parse(txt_TxtBlackColorOpacity.Text) * 255).ToString("0"))));
            tmpgra.Attributes.Add("StaTxtColor", ToHexColor((btn_TxtForeground.Background as SolidColorBrush).Color, 255));
            tmpgra.Attributes.Add("StaTxt", txt_Tagging.Text.Trim());
            tmpgra.Attributes.Add("StaFontFamily", new FontFamily(Dict_TxtFontFamily[cbx_TxtFontFamily.SelectedItem.ToString()]));
            tmpgra.Attributes.Add("StaFontSize", Dict_TxtFontSize[cbx_TxtFontSize.SelectedItem.ToString()]);

            PointingTextSymbol.OffsetY = 55;
            tmpgra.Symbol = PointingTextSymbol;

            PlotGraphicsLayer.Graphics.Add(tmpgra);
        }

        /// <summary>
        /// 添加默认文字
        /// </summary>
        /// <param name="tmpgeo"></param>
        void AddDefaultText(ESRI.ArcGIS.Client.Geometry.Geometry tmpgeo)
        {
            Graphic tmpgra = new Graphic();
            tmpgra.Geometry = tmpgeo;
            TextSymbol textsymbol = new TextSymbol();
            textsymbol.FontSize = Dict_TxtFontSize[cbx_TxtFontSize.SelectedItem.ToString()];
            textsymbol.FontFamily = new FontFamily(Dict_TxtFontFamily[cbx_TxtFontFamily.SelectedItem.ToString()]);
            textsymbol.Foreground = new SolidColorBrush((btn_TxtForeground.Background as SolidColorBrush).Color);
            textsymbol.Text = txt_Tagging.Text.Trim();
            tmpgra.Symbol = textsymbol;
            PlotGraphicsLayer.Graphics.Add(tmpgra);
        }

        /// <summary>
        /// RGB转换成16位值
        /// </summary>
        /// <param name="color"></param>
        /// <param name="byteOpacity"></param>
        /// <returns></returns>
        private string ToHexColor(Color color, byte byteOpacity)
        {
            string A = Convert.ToString(byteOpacity, 16);
            if (A == "0")
                A = "00";
            if (A.Length == 1)
                A = "0" + A;
            string R = Convert.ToString(color.R, 16);
            if (R == "0")
                R = "00";
            if (R.Length == 1)
                R = "0" + R;
            string G = Convert.ToString(color.G, 16);
            if (G == "0")
                G = "00";
            if (G.Length == 1)
                G = "0" + G;
            string B = Convert.ToString(color.B, 16);
            if (B == "0")
                B = "00";
            if (B.Length == 1)
                B = "0" + B;
            string HexColor = "#" + A + R + G + B;
            return HexColor;
        }
        #endregion

        private void rbtn_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rbtn = sender as RadioButton;
            if (grid_border == null || grid_full == null || grid_txt == null)
                return;
            grid_border.Visibility = System.Windows.Visibility.Collapsed;
            grid_full.Visibility = System.Windows.Visibility.Collapsed;
            grid_txt.Visibility = System.Windows.Visibility.Collapsed;

            switch (rbtn.Name)
            {
                case "rbtn_full":
                    grid_full.Visibility = System.Windows.Visibility.Visible;
                    break;
                case "rbtn_border":
                    grid_border.Visibility = System.Windows.Visibility.Visible;
                    break;
                case "rbtn_txt":
                    grid_txt.Visibility = System.Windows.Visibility.Visible;
                    break;
            }
        }

        private void rbtn_Checked1(object sender, RoutedEventArgs e)
        {
            RadioButton rbtn = sender as RadioButton;
            if (grid_border == null || grid_full == null || grid_txt == null)
                return;

            string type = rbtn.Content.ToString();

            foreach (KeyValuePair<string, Grid> kv in Dict_Grids) 
            {
                if (kv.Key == type)
                    kv.Value.Visibility = Visibility.Visible;
                else
                    kv.Value.Visibility = Visibility.Collapsed;
            }

            ////20140320:设置标绘图片的切换
            //grid_ImgDefault.Visibility = System.Windows.Visibility.Collapsed;
            //grid_ImgCheliang.Visibility = System.Windows.Visibility.Collapsed;
            //grid_ImgSheshui.Visibility = System.Windows.Visibility.Collapsed;
            //grid_ImgXiaofang.Visibility = System.Windows.Visibility.Collapsed;
            //grid_ImgMiehuo.Visibility = System.Windows.Visibility.Collapsed;

            //switch (rbtn.Name)
            //{
            //    //20140320:设置标绘图片的切换
            //    case "rbtn_ImgDefault":
            //        grid_ImgDefault.Visibility = System.Windows.Visibility.Visible;
            //        break;
            //    case "rbtn_ImgCheliang":
            //        grid_ImgCheliang.Visibility = System.Windows.Visibility.Visible;
            //        break;
            //    case "rbtn_ImgSheshui":
            //        grid_ImgSheshui.Visibility = System.Windows.Visibility.Visible;
            //        break;
            //    case "rbtn_ImgXiaofang":
            //        grid_ImgXiaofang.Visibility = System.Windows.Visibility.Visible;
            //        break;
            //    case "rbtn_ImgMiehuo":
            //        grid_ImgMiehuo.Visibility = System.Windows.Visibility.Visible;
            //        break;
            //}
        }

        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btn_Max_Click(object sender, RoutedEventArgs e)
        {
            Storyboard_Max.Begin();
            btn_MinClose.Visibility = System.Windows.Visibility.Collapsed;
            btn_Max.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void btn_Min_Click(object sender, RoutedEventArgs e)
        {
            Storyboard_Min.Begin();
            btn_MinClose.Visibility = System.Windows.Visibility.Visible;
            btn_Max.Visibility = System.Windows.Visibility.Visible;
        }

        #region 面板展开关闭接口
        public void Open()
        {
            Show();
        }

        public void Show()
        {
            this.HorizontalAlignment = HorizontalAlignment.Right;
            this.VerticalAlignment = VerticalAlignment.Top;
            if (!PFApp.Root.Children.Contains(this))
            {
                PFApp.Root.Children.Add(this);
                Storyboard_show.Begin();
            }
        }

        public void Close()
        {
            if (this.Parent != null)
            {
                Storyboard_Close.Begin();
            }
        }

        void Storyboard_Close_Completed(object sender, EventArgs e)
        {
            if (agsDraw != null)
                agsDraw.IsEnabled = false;
            if (plotdraw != null)
                plotdraw.setPlotDrawMode(clsIPlotDrawMode.None);
            agsDraw = null;
            if (PlotMap != null && PlotGraphicsLayer != null)
                PlotMap.Layers.Remove(PlotGraphicsLayer);
            PFApp.Root.Children.Remove(this);
        }

        public event IWidgetEventHander OpenEnd;

        public event PartEventHander CloseEnd;

        public PartDescriptor Descri
        {
            get { return new PartDescriptor() { Name = "AYKJ.GISPlot" }; }
        }

        public bool IsOpen
        {
            get { throw new NotImplementedException(); }
        }

        public string LinkFromGiPlatform(string oAction, string oStr, object oArr, object oCls, object[] oArrStr, object[] oArrArr, object[] oArrCls)
        {
            throw new NotImplementedException();
        }

        public event PartEventHander LinkGisPlatformEnd;

        public string LinkReturnGisPlatform(string mark, string s)
        {
            return "";
        }

        public string LinkReturnGisPlatform(string mark, object obj1, object obj2)
        {
            return "";
        }

        public void ReInit()
        {
            throw new NotImplementedException();
        }

        public event PartEventHander ReInitEnd;

        public event PartEventHander ShowEnd;
        #endregion
    }
}
