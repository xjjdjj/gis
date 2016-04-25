using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using AYKJ.GISDevelop.Platform;
using AYKJ.GISDevelop.Platform.ToolKit;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.FeatureService.Symbols;
using System.Windows.Media;
using System.Text;
using System.Windows.Input;
using System.IO;

namespace AYKJ.GISInterface
{
    public partial class AutoAllSearch : UserControl
    {
        //draw
        Draw draw;
        //所有的专题数据
        Dictionary<string, GraphicsLayer> Dict_Data;
        //绑定的空间数据
        Dictionary<clskmwxy, Graphic> Dict_ResultGraphic;
        //承载的地图
        Map mainmap;
        //打点的地图图层
        GraphicsLayer Data_GraLayer;
        //高亮的地图图层
        GraphicsLayer Draw_GraLayer;
        //空间服务地址
        string strGeometryurl;
        //设施点分析地址
        string strclosestfacilityurl;
        //服务分析地址
        string strareaurl;
        //动画
        WaitAnimationWindow waw;

        public AutoAllSearch()
        {
            InitializeComponent();
        }

        void InitAutoSearch()
        {
            //设置面板的起始位置
            this.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            this.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            this.Margin = new Thickness() { Top = 10, Right = 13 };

            mainmap = (Application.Current as IApp).MainMap;
            Data_GraLayer = new GraphicsLayer();
            Draw_GraLayer = new GraphicsLayer();
            mainmap.Layers.Add(Data_GraLayer);
            mainmap.Layers.Add(Draw_GraLayer);

            Dict_Data = (Application.Current as IApp).Dict_ThematicLayer;
            XElement xele = PFApp.Extent;
            strGeometryurl = (from item in PFApp.Extent.Elements("GeometryService")
                              select item.Attribute("Url").Value).ToArray()[0];
            var areaurl = (from item in xele.Element("ExtensionServices").Elements("ExtensionService")
                           where item.Attribute("Name").Value == "网络服务区分析"
                           select new
                           {
                               Name = item.Attribute("Name").Value,
                               Url = item.Attribute("Url").Value,
                           }).ToList();
            strareaurl = areaurl[0].Url;
            var cfurl = (from item in xele.Element("ExtensionServices").Elements("ExtensionService")
                         where item.Attribute("Name").Value == "网络设施点分析"
                         select new
                         {
                             Name = item.Attribute("Name").Value,
                             Url = item.Attribute("Url").Value,
                         }).ToList();
            strclosestfacilityurl = cfurl[0].Url;
            CreateLayer();
            Storyboard_Close.Completed += new EventHandler(Storyboard_Close_Completed);
        }

        /// <summary>
        /// 打点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_point_Click(object sender, RoutedEventArgs e)
        {
            CallAutoSearch();
        }

        /// <summary>
        /// 绘制搜索方法
        /// </summary>
        void CallAutoSearch()
        {
            Draw_GraLayer.Graphics.Clear();
            draw = new Draw(mainmap);
            draw.DrawComplete -= draw_DrawComplete;
            draw.DrawComplete += new EventHandler<DrawEventArgs>(draw_DrawComplete);
            draw.DrawMode = DrawMode.Point;
            draw.IsEnabled = true;
        }

        /// <summary>
        /// 打点之后立刻执行搜索
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void draw_DrawComplete(object sender, DrawEventArgs e)
        {
            draw.IsEnabled = false;
            Draw_GraLayer.Graphics.Clear();
            Graphic gra = new Graphic() { Geometry = e.Geometry };
            SimpleMarkerSymbol sms = new SimpleMarkerSymbol();
            sms.Color = new System.Windows.Media.SolidColorBrush() { Color = System.Windows.Media.Colors.Red };
            gra.Symbol = sms;
            Draw_GraLayer.Graphics.Add(gra);
            UseAutoSearch(gra);
        }

        /// <summary>
        /// 根据名称进行搜索
        /// </summary>
        /// <param name="strname"></param>
        public void CallAutoSearch(string str)
        {
            InitAutoSearch();

            if (PFApp.Root.Children.Count > 0)
            {
                for (int i = 0; i < PFApp.Root.Children.Count; i++)
                {
                    if (PFApp.Root.Children[i].GetType().Name == "AutoSearch")
                    {
                        PFApp.Root.Children.RemoveAt(i);
                    }
                }
            }
            PFApp.Root.Children.Add(this);
            Storyboard_Show.Begin();

            var buffer = System.Text.Encoding.UTF8.GetBytes(str);
            var ms = new MemoryStream(buffer);
            var jsonObject = System.Json.JsonObject.Load(ms) as System.Json.JsonObject;
            string tmp_wxyid = jsonObject["wxyid"].ToString().Replace("\"", "");
            string tmp_wxytype = jsonObject["wxytype"].ToString().Replace("\"", "");
            string tmp_dwdm = jsonObject["dwdm"].ToString().Replace("\"", "");
            string statag = tmp_wxytype + "|" + tmp_wxyid + "|" + tmp_dwdm;
            var Explosion = (from item in (Application.Current as IApp).lstThematic
                             where item.Attributes["StaTag"].ToString().Contains(statag)
                             select new
                             {
                                 Name = item.Attributes["remark"],
                                 Graphic = item,
                             }).ToList();

            txt_every.Text = jsonObject["every"].ToString().Replace("\"", "");
            txt_all.Text = jsonObject["all"].ToString().Replace("\"", "");

            //var Explosion = (from item in (Application.Current as IApp).lstThematic
            //                 where item.Attributes["StaRemark"].ToString() == strname
            //                 select new
            //                 {
            //                     Name = item.Attributes["remark"],
            //                     Graphic = item,
            //                 }).ToList();
            if (Explosion.Count == 0)
            {
                Message.Show("没有找到该设施点，请重新输入");
                return;
            }
            UseAutoSearch(Explosion[0].Graphic);
        }

        public void LoadAutoSearch()
        {
            if (PFApp.Root.Children.Count > 0)
            {
                for (int i = 0; i < PFApp.Root.Children.Count; i++)
                {
                    if (PFApp.Root.Children[i].GetType().Name == "AutoSearch")
                    {
                        PFApp.Root.Children.RemoveAt(i);
                    }
                }
            }
            PFApp.Root.Children.Add(this);
            Storyboard_Show.Begin();
            InitAutoSearch();
        }

        /// <summary>
        /// 根据XY坐标进行搜索
        /// </summary>
        /// <param name="strx"></param>
        /// <param name="stry"></param>
        void CallAutoSearch(string strx, string stry)
        {
            Graphic gra = new Graphic() { Geometry = new MapPoint() { X = double.Parse(strx), Y = double.Parse(stry), SpatialReference = mainmap.SpatialReference } };
            UseAutoSearch(gra);
        }

        /// <summary>
        /// 进行搜索
        /// </summary>
        /// <param name="gra"></param>
        void UseAutoSearch(Graphic gra)
        {
            data_result.ItemsSource = null;
            List<string> lst = new List<string>();
            for (int i = 0; i < sp_layer.Children.Count; i++)
            {
                CheckBox c = sp_layer.Children[i] as CheckBox;
                if (c.IsChecked == true)
                {
                    lst.Add(c.Content.ToString());
                }
            }
            if (lst.Count == 0)
            {
                Message.Show("请先选择图层");
                return;
            }

            clsAutoAllSearch clsautoallsearch = new clsAutoAllSearch();
            clsautoallsearch.AutoSearchFaildEvent += new AutoAllSearchDelegate(clsautosearch_AutoSearchFaildEvent);
            clsautoallsearch.AutoSearchEvent += new AutoAllSearchDelegate(clsautosearch_AutoSearchEvent);
            clsautoallsearch.AutoSearch(gra, strareaurl, strGeometryurl, strclosestfacilityurl,
                (Application.Current as IApp).Dict_ThematicLayer, lst, double.Parse(txt_every.Text), double.Parse(txt_all.Text), Draw_GraLayer, clsAutoSearchType.To);
            waw = new WaitAnimationWindow("设施点搜索中，请稍候");
            waw.Show();
        }

        /// <summary>
        /// 搜索成功
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clsautosearch_AutoSearchEvent(object sender, EventArgs e)
        {
            waw.Close();
            Dictionary<string, List<Graphic>> Dict_Result = (sender as clsAutoAllSearch).Dict_RData;
            //Dictionary<string, Graphic> Dict_Result = (sender as clsAutoAllSearch).Dict_ShortData;
            if (Dict_Result == null)
                return;
            List<clswxy> lst = new List<clswxy>();
            Dict_ResultGraphic = new Dictionary<clskmwxy, Graphic>(); ;
            for (int i = 0; i < Dict_Result.Count; i++)
            {
                for (int m = 0; m < Dict_Result.Values.ToList()[i].Count; m++)
                {
                    Graphic gra = Dict_Result.Values.ToList()[i][m];
                    string[] arytmp = gra.Attributes["StaTag"].ToString().Split('|');
                    Dict_ResultGraphic.Add(new clskmwxy()
                    {
                        wxyid = arytmp[1],
                        wxykm = (sender as clsAutoAllSearch).Dict_ShortLen[gra].ToString("0") + "米",
                        //wxykm = ReturnKm((sender as clsAutoAllSearch).Dict_RKm[(Application.Current as IApp).DictThematicEnCn[arytmp[0]]][gra]),
                        wxyname = EllipsisName(arytmp[3]),
                        wxytip = arytmp[3],
                        wxytype = (Application.Current as IApp).DictThematicEnCn[arytmp[0]],
                        wxydwdm = arytmp[2]
                    }, gra);
                }
            }
            data_result.ItemsSource = Dict_ResultGraphic.Keys.ToList();
        }

        string ReturnKm(Int32 m)
        {
            string tmp = string.Empty;
            tmp = ((m + 1) * double.Parse(txt_every.Text)).ToString() + "m";
            return tmp;
        }

        /// <summary>
        /// 搜索失败
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clsautosearch_AutoSearchFaildEvent(object sender, EventArgs e)
        {
            waw.Close();
            Message.Show("搜索失败，请检查参数");
        }

        #region 图层控制
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
            MouseWheelSupportAddOn.Activate(scrolls1, true);
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
        #endregion

        /// <summary>
        /// 清除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_clear_Click(object sender, RoutedEventArgs e)
        {
            txt_every.Text = "5000";
            txt_all.Text = "20000";
            Data_GraLayer.Graphics.Clear();
            Draw_GraLayer.Graphics.Clear();
            data_result.ItemsSource = null;
        }

        /// <summary>
        /// 数据定位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_postion_Click(object sender, RoutedEventArgs e)
        {
            Data_GraLayer.Graphics.Clear();
            if (data_result.SelectedItem != null)
            {
                Graphic gra = new Graphic();
                gra.Geometry = Dict_ResultGraphic[data_result.SelectedItem as clskmwxy].Geometry;
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
        /// 数据重置
        /// </summary>
        public void Reset()
        {
            try
            {
                Dict_ResultGraphic = new Dictionary<clskmwxy, Graphic>();
                mainmap.Layers.Remove(Data_GraLayer);
                mainmap.Layers.Remove(Draw_GraLayer);
                data_result.ItemsSource = null;
                txt_every.Text = "5000";
                txt_all.Text = "20000";
            }
            catch (Exception)
            { }
        }

        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            Storyboard_Close.Begin();
        }

        void Storyboard_Close_Completed(object sender, EventArgs e)
        {
            Reset();
            if (PFApp.Root.Children.Contains(this))
            {
                PFApp.Root.Children.Remove(this);
            }
        }

        #region 缩小放大
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
        #endregion

        /// <summary>
        /// 文字名缩减
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        string EllipsisName(string str)
        {
            string tmpstr = string.Empty;
            int length = Encoding.Unicode.GetByteCount(str);
            if (length < 15)
            {
                tmpstr = str;
            }
            else
            {
                tmpstr = str.Substring(0, 7) + "...";
            }
            return tmpstr;
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
            tbx.Text = (e.Row.DataContext as clskmwxy).wxytip;
            ToolTipService.SetToolTip(e.Row, tbx);
        }

        /// <summary>
        /// 只允许输入数字
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBok_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            TextBox txt = sender as TextBox;
            //屏蔽非法按键
            if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
            {
                e.Handled = false;
            }
            else if ((e.Key >= Key.D0 && e.Key <= Key.D9))
            {

                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }
    }
}
