using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using AYKJ.GISDevelop.Platform.ToolKit;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client;
using AYKJ.GISDevelop.Platform;
using ESRI.ArcGIS.Client.Geometry;
using System.Text;
using System.Windows.Browser;

namespace AYKJ.GISExtension
{
    public partial class DataQuerySpatialToolKit : UserControl
    {
        //定义一个Map用来接收平台的Map
        public static Map mainmap;
        //绘制图形GraphicLayer
        GraphicsLayer Draw_GraLayer;
        //绘制的Draw
        Draw draw;
        //一次查询定位
        GraphicsLayer Data_GraLayer;
        //空间查询方法
        clsRangeQuery clsrangequery;
        //查询后属性数据
        List<clstipwxy> lstdata;
        //绑定的空间数据
        Dictionary<clstipwxy, Graphic> Dict_ResultGraphic;
        //所有的专题数据
        Dictionary<string, GraphicsLayer> Dict_Data;
        //动画
        WaitAnimationWindow waitanimationwindow;
        #region ESRI样式        
        public SimpleFillSymbol DrawFillSymbol = new SimpleFillSymbol()
        {
            Fill = new SolidColorBrush(Color.FromArgb(30, 255, 0, 0)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)),
            BorderThickness = 2
        };
        #endregion

        public DataQuerySpatialToolKit()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(DataQuerySpatial_Loaded);
        }

        void DataQuerySpatial_Loaded(object sender, RoutedEventArgs e)
        {
            mainmap = (Application.Current as IApp).MainMap;
            Dict_ResultGraphic = new Dictionary<clstipwxy, Graphic>();

            lstdata = new List<clstipwxy>();
            Dict_Data = (Application.Current as IApp).Dict_ThematicLayer;

            clsrangequery = new clsRangeQuery();
            clsrangequery.RangeQueryEvent -= clsrangequery_RangeQueryEvent;
            clsrangequery.RangeQueryEvent += new RangeQueryDelegate(clsrangequery_RangeQueryEvent);
            clsrangequery.RangeQueryFaildEvent -= clsrangequery_RangeQueryFaildEvent;
            clsrangequery.RangeQueryFaildEvent += new RangeQueryDelegate(clsrangequery_RangeQueryFaildEvent);

            Draw_GraLayer = new GraphicsLayer();
            Data_GraLayer = new GraphicsLayer();
            mainmap.Layers.Add(Draw_GraLayer);
            mainmap.Layers.Add(Data_GraLayer);

            draw = new Draw(mainmap);
            draw.FillSymbol = DrawFillSymbol;
            draw.IsEnabled = false;
            draw.DrawBegin -= draw_DrawBegin;
            draw.DrawBegin += new EventHandler(draw_DrawBegin);
            draw.DrawComplete -= draw_DrawComplete;
            draw.DrawComplete += new EventHandler<DrawEventArgs>(draw_DrawComplete);

            CreateLayer();
        }

        /// <summary>
        /// 绘制图形进行查询
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_query(object sender, RoutedEventArgs e)
        {
            Draw_GraLayer.Graphics.Clear();
            Data_GraLayer.Graphics.Clear();
            draw.IsEnabled = true;
            Button btn = sender as Button;
            switch (btn.Name)
            {
                case "btn_rect":
                    draw.DrawMode = DrawMode.Rectangle;
                    break;
                case "btn_polygon":
                    draw.DrawMode = DrawMode.Polygon;
                    break;
                case "btn_clear":
                    draw.IsEnabled = false;
                    data_result.ItemsSource = null;
                    break;
            }
        }

        /// <summary>
        /// 绘制图形之前
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void draw_DrawBegin(object sender, EventArgs e)
        {
            Draw_GraLayer.Graphics.Clear();
            Data_GraLayer.Graphics.Clear();
        }

        /// <summary>
        /// 绘制图形结束
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void draw_DrawComplete(object sender, DrawEventArgs e)
        {
            draw.IsEnabled = false;
            List<Graphic> lst = new List<Graphic>();
            Dict_Data = (Application.Current as IApp).Dict_ThematicLayer;
            for (int i = 0; i < sp_layer.Children.Count; i++)
            {
                CheckBox c = sp_layer.Children[i] as CheckBox;
                if (c.IsChecked == true)
                {
                    lst.AddRange(Dict_Data[c.Content.ToString()]);
                }
            }

            Graphic draw_graphic = new Graphic();
            if (e.Geometry is ESRI.ArcGIS.Client.Geometry.Polygon)
            {
                //20150615zc:江宁平面图功能专用，返回平面图的范围，便于定位放大。
                ESRI.ArcGIS.Client.Geometry.Polygon p = e.Geometry as ESRI.ArcGIS.Client.Geometry.Polygon;
                String str_Point = string.Empty;
                foreach (ESRI.ArcGIS.Client.Geometry.MapPoint point in p.Rings[0])
                {
                    str_Point += point.X.ToString() + "," + point.Y.ToString() + "|";
                }
                rtnToBussinesspage("getSpatialGeometry", str_Point.Substring(0, str_Point.Length-1));                


                ESRI.ArcGIS.Client.Geometry.Polygon polygon1 = e.Geometry as ESRI.ArcGIS.Client.Geometry.Polygon;
                if ((e.Geometry as ESRI.ArcGIS.Client.Geometry.Polygon).Rings[0].Count < 4)
                {
                    Message.Show("多边形顶点数不能小于3", "提示");
                    Draw_GraLayer.Graphics.Clear();
                    return;
                }
                draw_graphic = new Graphic() { Geometry = e.Geometry, Symbol = DrawFillSymbol };
            }

            if (e.Geometry is ESRI.ArcGIS.Client.Geometry.Envelope)
            {
                if (e.Geometry.Extent.XMax == e.Geometry.Extent.XMin
                    && e.Geometry.Extent.YMax == e.Geometry.Extent.YMin)
                {
                    Message.Show("请绘制正确的矩形", "提示");
                    Draw_GraLayer.Graphics.Clear();
                    return;
                }

                ESRI.ArcGIS.Client.Geometry.Envelope env = e.Geometry as ESRI.ArcGIS.Client.Geometry.Envelope;

                ESRI.ArcGIS.Client.Geometry.Polygon polygon1 = new ESRI.ArcGIS.Client.Geometry.Polygon();
                ESRI.ArcGIS.Client.Geometry.PointCollection pcoll = new ESRI.ArcGIS.Client.Geometry.PointCollection();

                MapPoint p1 = new MapPoint();
                p1.X = Convert.ToDouble(env.XMin);
                p1.Y = Convert.ToDouble(env.YMin);
                MapPoint p2 = new MapPoint();
                p2.X = Convert.ToDouble(env.XMax);
                p2.Y = Convert.ToDouble(env.YMin);
                MapPoint p3 = new MapPoint();
                p3.X = Convert.ToDouble(env.XMax);
                p3.Y = Convert.ToDouble(env.YMax);
                MapPoint p4 = new MapPoint();
                p4.X = Convert.ToDouble(env.XMin);
                p4.Y = Convert.ToDouble(env.YMax);
                pcoll.Add(p1);
                pcoll.Add(p2);
                pcoll.Add(p3);
                pcoll.Add(p4);
                pcoll.Add(p1);
                polygon1.Rings.Add(pcoll);
                polygon1.SpatialReference = mainmap.SpatialReference;
                draw_graphic = new Graphic() { Geometry = polygon1, Symbol = DrawFillSymbol };
            }
            Draw_GraLayer.Graphics.Add(draw_graphic);

            if (lst == null || lst.Count == 0)
            {
                Message.Show("所选范围内没有专题数据");
                Draw_GraLayer.Graphics.Clear();
                data_result.ItemsSource = null;
                return;
            }
            waitanimationwindow = new WaitAnimationWindow("数据查询中，请稍候...");
            waitanimationwindow.Show();
            clsrangequery.RangeQuery("",draw_graphic.Geometry, lst);

        }

        //将信息返回给aspx页面中的特定方法
        bool rtnToBussinesspage(string func, string mess)
        {
            ScriptObject queryfinished = HtmlPage.Window.GetProperty(func) as ScriptObject;
            if (queryfinished != null)
            {
                queryfinished.InvokeSelf(mess);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 调用方法返回结果
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clsrangequery_RangeQueryEvent(object sender, EventArgs e)
        {
            List<Graphic> lstreturngra = (sender as clsRangeQuery).lstReturnGraphic;            
            lstdata = new List<clstipwxy>();
            for (int i = 0; i < lstreturngra.Count(); i++)
            {
                string[] arytmp = lstreturngra[i].Attributes["StaTag"].ToString().Split('|');
                lstdata.Add(new clstipwxy()
                {
                    wxyid = arytmp[1],
                    wxyname = EllipsisName(arytmp[3]),
                    wxytip = arytmp[3],
                    wxytype = (Application.Current as IApp).DictThematicEnCn[arytmp[0]],
                    wxydwdm = arytmp[2]
                });
                Dict_ResultGraphic.Add(lstdata[lstdata.Count - 1], lstreturngra[i]);
            }
            data_result.ItemsSource = null;
            data_result.ItemsSource = lstdata;
            waitanimationwindow.Close();
        }

        /// <summary>
        /// 文字名缩减
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        string EllipsisName(string str)
        {
            string tmpstr = string.Empty;
            int length = Encoding.Unicode.GetByteCount(str);
            if (length < 29)
            {
                tmpstr = str;
            }
            else
            {
                tmpstr = str.Substring(0, 12) + "...";
            }
            return tmpstr;
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
            Message.ShowErrorInfo("查询出错",e.Message);
        }

        #region 重置信息
        public void Reset()
        {
            try
            {
                Dict_ResultGraphic = new Dictionary<clstipwxy, Graphic>();
                lstdata = new List<clstipwxy>();
                clsrangequery = new clsRangeQuery();
                clsrangequery.RangeQueryEvent -= clsrangequery_RangeQueryEvent;
                mainmap.Layers.Remove(Draw_GraLayer);
                mainmap.Layers.Remove(Data_GraLayer);
                draw.IsEnabled = false;
                draw.DrawBegin -= draw_DrawBegin;
                draw.DrawComplete -= draw_DrawComplete;
                data_result.ItemsSource = null;
                sp_layer.Children.Clear();
            }
            catch (Exception)
            { }
        }
        #endregion

        private void btn_postion_Click(object sender, RoutedEventArgs e)
        {
            Data_GraLayer.Graphics.Clear();
            if (data_result.SelectedItem != null)
            {
                Graphic gra = new Graphic();
                gra.Geometry = Dict_ResultGraphic[data_result.SelectedItem as clstipwxy].Geometry;
                gra.Symbol = HighMarkerStyle;
                Data_GraLayer.Graphics.Add(gra);

                Envelope eve = new Envelope()
                {
                    XMax = gra.Geometry.Extent.XMax + 0.000001,
                    YMax = gra.Geometry.Extent.YMax + 0.000001,
                    XMin = gra.Geometry.Extent.XMin - 0.000001,
                    YMin = gra.Geometry.Extent.YMin - 0.000001
                };
                mainmap.ZoomTo(eve);
            }
        }

        /// <summary>
        /// 创建查询图层
        /// </summary>
        void CreateLayer()
        {
            for (int i = 0; i < Dict_Data.Count(); i++)
            {
                CheckBox ckb = new CheckBox();
                ckb.Content = Dict_Data.Keys.ToList()[i];
                List<Graphic> lsttmp = Dict_Data.Values.ToList()[i].Graphics.ToList();
                ckb.Tag = lsttmp;
                ckb.IsChecked = true;
                ckb.Margin = new Thickness(0, 5, 5, 0);
                ckb.Style = this.Resources["CheckBoxStyle"] as Style;
                ckb.BorderBrush = null;
                ckb.BorderThickness = new Thickness(0);
                ckb.Foreground = new SolidColorBrush(Colors.White);
                sp_layer.Children.Add(ckb);
            }
            chkLayer.IsChecked = true;
            //MouseWheelSupportAddOn.Activate(scrolls1, true);
        }

        /// <summary>
        /// 全选图层
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chk_Checked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < sp_layer.Children.Count; i++)
            {
                CheckBox c = sp_layer.Children[i] as CheckBox;
                c.IsChecked = true;
            }
            data_result.ItemsSource = null;
        }

        /// <summary>
        /// 全部选图层
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chk_Unchecked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < sp_layer.Children.Count; i++)
            {
                CheckBox c = sp_layer.Children[i] as CheckBox;
                c.IsChecked = false;
            }
            data_result.ItemsSource = null;
        }

        private void data_result_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            TextBox tbx = new TextBox()
            {
                FontFamily = new FontFamily("NSimSun"),
                FontSize = 12,
                Background = new SolidColorBrush(Colors.Black),
                Foreground = new SolidColorBrush(Colors.White),
                BorderBrush = null,
                Margin = new Thickness(-9, -3, -9, -3)
            };
            tbx.Text = (e.Row.DataContext as clstipwxy).wxytip;
            ToolTipService.SetToolTip(e.Row, tbx);
            //AykjToolTipService.SetTipText(e.Row, (e.Row.DataContext as clstipwxy).wxytip);
        }
    }
}
