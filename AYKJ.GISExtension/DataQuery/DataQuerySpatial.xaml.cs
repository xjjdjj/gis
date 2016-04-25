#region << 版 本 注 释 >>
/*
 * ========================================================================
 * Copyright(c)  陈锋, All Rights Reserved.
 * ========================================================================
 * CLR版本：       4.0.30319.261
 * 类 名 称：       DataQueryRadius
 * 机器名称：       GIS-FLYH
 * 命名空间：       AYKJ.GISExtension.Control.Query
 * 文 件 名：       DataQueryRadius
 * 创建时间：       2012/7/20 11:21:12
 * 作    者：       陈锋
 * 功能说明：       空间查询
 * 修改时间：
 * 修 改 人：
 * ========================================================================
*/
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Linq;
using AYKJ.GISDevelop.Platform;
using AYKJ.GISDevelop.Platform.ToolKit;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using System.Windows.Input;
using System.Text;

namespace AYKJ.GISExtension
{
    public partial class DataQuerySpatial : UserControl
    {
        //定义一个空间查询服务的地址
        string strGeometryurl;
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
        public SimpleLineSymbol DrawLineSymbol = new SimpleLineSymbol()
        {
            Color = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)),
            Width = 2,
            Style = SimpleLineSymbol.LineStyle.Solid
        };
        public SimpleFillSymbol DrawFillSymbol = new SimpleFillSymbol()
        {
            Fill = new SolidColorBrush(Color.FromArgb(30, 255, 0, 0)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)),
            BorderThickness = 2
        };
        public SimpleMarkerSymbol DrawMarkerSymbol = new SimpleMarkerSymbol()
        {
            Color = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)),
            Size = 5,
            Style = SimpleMarkerSymbol.SimpleMarkerStyle.Circle
        };
        #endregion

        public DataQuerySpatial()
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
            draw.LineSymbol = DrawLineSymbol;
            draw.FillSymbol = DrawFillSymbol;
            draw.IsEnabled = false;
            draw.DrawBegin -= draw_DrawBegin;
            draw.DrawBegin += new EventHandler(draw_DrawBegin);
            draw.DrawComplete -= draw_DrawComplete;
            draw.DrawComplete += new EventHandler<DrawEventArgs>(draw_DrawComplete);

            XElement xele = PFApp.Extent;
            strGeometryurl = (from item in PFApp.Extent.Elements("GeometryService")
                              select item.Attribute("Url").Value).ToArray()[0];
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
                case "btn_point":
                    draw.DrawMode = DrawMode.Point;
                    break;
                case "btn_line":
                    draw.DrawMode = DrawMode.Polyline;
                    break;
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
        /// 只允许输入数字
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txt_dis_KeyDown(object sender, KeyEventArgs e)
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
            if (e.Geometry is ESRI.ArcGIS.Client.Geometry.MapPoint)
            {
                draw_graphic = new Graphic() { Geometry = e.Geometry, Symbol = DrawMarkerSymbol };

            }
            if (e.Geometry is ESRI.ArcGIS.Client.Geometry.Polyline)
            {
                draw_graphic = new Graphic() { Geometry = e.Geometry, Symbol = DrawLineSymbol };

            }
            if (e.Geometry is ESRI.ArcGIS.Client.Geometry.Polygon)
            {
                ESRI.ArcGIS.Client.Geometry.Polygon polygon1 = e.Geometry as ESRI.ArcGIS.Client.Geometry.Polygon;
                if ((e.Geometry as ESRI.ArcGIS.Client.Geometry.Polygon).Rings[0].Count < 4)
                {
                    Message.Show("多边形顶点数不能小于3");
                    Draw_GraLayer.Graphics.Clear();
                    return;
                }
                draw_graphic = new Graphic() { Geometry = e.Geometry, Symbol = DrawFillSymbol };
            }

            if (e.Geometry is ESRI.ArcGIS.Client.Geometry.Envelope)
            {
                ESRI.ArcGIS.Client.Geometry.Envelope env = e.Geometry as ESRI.ArcGIS.Client.Geometry.Envelope;

                if (env.Height == 0.0 || env.Width == 0.0)
                {
                    if (ckb_buff.IsChecked==true)
                    {
                        Message.Show("矩形边长为0，无法进行缓冲查询。请按住鼠标左键不放并拖动鼠标重画矩形。");
                        Draw_GraLayer.Graphics.Clear();
                        return;
                    }
                }

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

            if (ckb_buff.IsChecked == true)
            {
                if (txt_dis.Text.Trim() == "")
                {
                    Message.Show("请输入缓冲数值");
                    Draw_GraLayer.Graphics.Clear();
                    return;
                }
                if (Convert.ToInt32(txt_dis.Text.Trim()) == 0)
                {
                    Message.Show("请输入大于0的缓冲数值");
                    Draw_GraLayer.Graphics.Clear();
                    return;
                }
                clsrangequery.RangeBuffQuery(strGeometryurl, draw_graphic.Geometry, int.Parse(txt_dis.Text.Trim()), lst);
            }
            else
            {
                clsrangequery.RangeQuery(strGeometryurl, draw_graphic.Geometry, lst);
            }
            waitanimationwindow = new WaitAnimationWindow("数据查询中，请稍候...");
            waitanimationwindow.Show();
        }

        /// <summary>
        /// 调用方法返回结果
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clsrangequery_RangeQueryEvent(object sender, EventArgs e)
        {
            List<Graphic> lstreturngra = (sender as clsRangeQuery).lstReturnGraphic;
            if (ckb_buff.IsChecked == true)
            {
                SimpleFillSymbol sfs = new SimpleFillSymbol() { Fill = new SolidColorBrush() { Color = Colors.Blue, Opacity = 0.2 } };
                sfs.BorderThickness = 0;
                (sender as clsRangeQuery).BuffGraphic.Symbol = sfs;
                Draw_GraLayer.Graphics.Add((sender as clsRangeQuery).BuffGraphic);
            }
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
            if (lstdata.Count < 1)
                Message.Show("没有符合的数据");
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

        #region 控制缓冲区可用
        private void ckb_buff_Checked(object sender, RoutedEventArgs e)
        {
            txt_dis.IsEnabled = true;
        }

        private void ckb_buff_Unchecked(object sender, RoutedEventArgs e)
        {
            txt_dis.IsEnabled = false;
        }
        #endregion

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
                txt_dis.Text = "";
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
