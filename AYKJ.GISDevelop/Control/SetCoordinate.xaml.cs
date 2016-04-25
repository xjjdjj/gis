/// <summary>  
/// 作者：陈锋 
/// 时间：2012/5/24 12:13:01  
/// 公司:南京安元科技有限公司  
/// 版权：2012-2020  
/// CLR版本：4.0.30319.261  
/// SetCoordinate说明：坐标定位
/// 唯一标识：9b9fc8d3-1aad-43fd-8317-a4194a4f8c86  
/// </summary>

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AYKJ.GISDevelop.Platform;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;

namespace AYKJ.GISDevelop.Control
{
    public partial class SetCoordinate : UserControl
    {
        Map map;//地图
        GraphicsLayer gralay;
        SimpleMarkerSymbol markerSymbol = new SimpleMarkerSymbol()
        {
            Color = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0)),
            Size = 15,
            Style = SimpleMarkerSymbol.SimpleMarkerStyle.Circle
        };

        public SetCoordinate()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(SetCoordinate_Loaded);
        }

        void SetCoordinate_Loaded(object sender, RoutedEventArgs e)
        {
            //设置面板的起始位置
            this.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            this.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            this.Margin = new Thickness(0, 0, 0, 21);

            map = App.mainMap;
            gralay = new GraphicsLayer();
            map.Layers.Add(gralay);
        }


        #region 两侧面板的展示和关闭
        /// <summary>
        /// 面板展开
        /// </summary>
        public void Show()
        {
            //展开面板
            PFApp.Root.Children.Add(this);
        }
        /// <summary>
        /// 面板关闭方法
        /// </summary>
        public void Close()
        {
            map.Layers.Remove(gralay);
            txt_x.Text = "";
            txt_y.Text = "";
            PFApp.Root.Children.Remove(this);
        }

        void Measure_ClearAll()
        {
            Close();
        }
        #endregion

        private void btn_click(object sender, RoutedEventArgs e)
        {
            MapPoint mp = new MapPoint();
            if (txt_x.Text.Trim() == "" || txt_x.Text.Trim() == "")
                return;
            mp.X = double.Parse(txt_x.Text.Trim());
            mp.Y = double.Parse(txt_y.Text.Trim());
            mp.SpatialReference = map.SpatialReference;

            Graphic gra = new Graphic()
            {
                Symbol = markerSymbol,
                Geometry = mp
            };

            switch ((sender as Button).Name)
            {
                case "btn_pan":
                    map.PanTo(mp);
                    break;
                case "btn_zoom":
                    Envelope evl = new Envelope()
                    {
                        XMax = mp.X + 1,
                        YMax = mp.Y + 1,
                        XMin = mp.X - 1,
                        YMin = mp.Y - 1,
                        SpatialReference = mp.SpatialReference
                    };
                    map.ZoomTo(evl);
                    break;
                case "btn_point":
                    gralay.Graphics.Add(gra);
                    break;
                case "btn_xy":
                    TextSymbol txtSymbol = new TextSymbol()
                    {
                        OffsetX = 40,
                        OffsetY = -15,
                        Foreground = new SolidColorBrush() { Color = new Color() { A = 255, R = 0, G = 0, B = 0 } },
                        FontFamily = new FontFamily("NSimSun"),
                        Text = mp.X.ToString()+", "+mp.Y.ToString(), 
                        FontSize = 12
                    };
                    Graphic txtgra = new Graphic()
                    {
                        Symbol = txtSymbol,
                        Geometry = mp
                    };
                    gralay.Graphics.Add(txtgra);
                    break;
                case "btn_zs":
                    Graphic zsgra = new Graphic()
                    {
                        Symbol = MarkerSymbolAlert,
                        Geometry = mp
                    };
                    zsgra.Attributes.Add("coorxy", mp.X.ToString() + ", " + mp.Y.ToString());
                    gralay.Graphics.Add(zsgra);
                    break;
                case "btn_clear":
                    gralay.Graphics.Clear();
                    break;
            }
        }
    }
}
