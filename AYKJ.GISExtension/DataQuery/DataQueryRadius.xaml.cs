#region << 版 本 注 释 >>
/*
 * ========================================================================
 * Copyright(c)  陈锋, All Rights Reserved.
 * ========================================================================
 * CLR版本：       4.0.30319.261
 * 类 名 称：       DataQueryRadius
 * 机器名称：       GIS-FLYH
 * 命名空间：       AYKJ.GISExtension.Control.Query
 * 文 件 名：       DataQuerySpatial
 * 创建时间：       2012/7/20 11:21:12
 * 作    者：       陈锋
 * 功能说明：       半径搜索
 * 修改时间：
 * 修 改 人：
 * ========================================================================
*/
#endregion

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
using ESRI.ArcGIS.Client;
using AYKJ.GISDevelop.Platform;
using ESRI.ArcGIS.Client.Symbols;
using System.Xml.Linq;
using ESRI.ArcGIS.Client.Geometry;
using AYKJ.GISDevelop.Platform.ToolKit;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AYKJ.GISExtension
{
    public partial class DataQueryRadius : UserControl
    {
        public static ESRI.ArcGIS.Client.Geometry.Polygon CirlePolygon;
        //定义一个空间查询服务的地址
        string strGeometryurl;
        //定义一个Map用来接收平台的Map
        public static Map mainmap;
        //绘制图形GraphicLayer
        public static GraphicsLayer Draw_GraLayer;
        //一次查询定位
        public static GraphicsLayer Data_GraLayer;
        //空间查询方法
        clsRangeQuery clsrangequery;
        //查询后属性数据
        static List<clstipwxy> lstdata;
        //绑定的空间数据
        Dictionary<clstipwxy, Graphic> Dict_ResultGraphic;
        //所有的专题数据
        Dictionary<string, GraphicsLayer> Dict_Data;
        //动画
        WaitAnimationWindow waitanimationwindow;
        DrawCircle drc;

        //是否暴露给外部接口，由外部接口调用
        bool Expose = false;
        Graphic ingra;
        bool isLoad = false;
        #region ESRI样式
        public SimpleFillSymbol DrawFillSymbol = new SimpleFillSymbol()
        {
            Fill = new SolidColorBrush(Color.FromArgb(30, 255, 0, 0)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)),
            BorderThickness = 2
        };
        #endregion

        public DataQueryRadius()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(DataQuerySpatial_Loaded);
        }

        void DataQuerySpatial_Loaded(object sender, RoutedEventArgs e)
        {
            mainmap = (Application.Current as IApp).MainMap;
            isLoad = true;
            if (!this.Expose)
            {
                Dict_ResultGraphic = new Dictionary<clstipwxy, Graphic>();
                lstdata = new List<clstipwxy>();
            }
            Dict_Data = (Application.Current as IApp).Dict_ThematicLayer;
            

            clsrangequery = new clsRangeQuery();
            clsrangequery.RangeQueryEvent -= clsrangequery_RangeQueryEvent;
            clsrangequery.RangeQueryEvent += new RangeQueryDelegate(clsrangequery_RangeQueryEvent);
            clsrangequery.RangeQueryFaildEvent -= clsrangequery_RangeQueryFaildEvent;
            clsrangequery.RangeQueryFaildEvent += new RangeQueryDelegate(clsrangequery_RangeQueryFaildEvent);

            Draw_GraLayer = new GraphicsLayer();
            Data_GraLayer = new GraphicsLayer();
            if (this.Expose)
                Draw_GraLayer.Graphics.Add(this.ingra);

            mainmap.Layers.Insert((mainmap.Layers.Count- Dict_Data.Count), Draw_GraLayer);
            mainmap.Layers.Add(Data_GraLayer);

            XElement xele = PFApp.Extent;
            strGeometryurl = (from item in PFApp.Extent.Elements("GeometryService")
                              select item.Attribute("Url").Value).ToArray()[0];
            if (!this.Expose)
            {
                CreateLayer();
            }
           // this.Expose = false;
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
            Button btn = sender as Button;
            switch (btn.Name)
            {
                case "btn_point":
                    drc = new DrawCircle(Draw_GraLayer);
                    drc.Map = mainmap;
                    drc.DrawCircleEvent -= drc_DrawCircleEvent;
                    drc.DrawCircleEvent += new DrawCircleDelegate(drc_DrawCircleEvent);
                    drc.IsActivated = true;
                    break;
                case "btn_clear":
                    if (drc != null)
                        drc.IsActivated = false;
                    data_result.ItemsSource = null;
                    break;
            }
        }

        void drc_DrawCircleEvent(object sender, EventArgs e)
        {
            Graphic draw_graphic = new Graphic();
            drc.CirclereturnPolygon = CirlePolygon;
            draw_graphic.Geometry = drc.Getreturngeometry as ESRI.ArcGIS.Client.Geometry.Polygon;
            draw_graphic.Symbol = DrawFillSymbol;
            draw_graphic.Geometry.SpatialReference = mainmap.SpatialReference;
            Draw_GraLayer.Graphics.Add(draw_graphic);
            drc.IsActivated = false;

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
            if (lst == null || lst.Count == 0)
            {
                Message.Show("所选范围内没有专题数据");
                data_result.ItemsSource = null;
                return;
            }
            waitanimationwindow = new WaitAnimationWindow("数据查询中，请稍候...");
            waitanimationwindow.Show();
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
            Message.ShowErrorInfo("查询出错",e.Message);
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

        public void clsRangeQueryExpose(List<Graphic> lstreturngra,Graphic ingra,string[] queryType)
        {
            this.Expose = true;
            this.ingra = ingra;
            if(isLoad)
            {
                Draw_GraLayer.ClearGraphics();
                Draw_GraLayer.Graphics.Add(this.ingra);
            }
            Dict_Data = (Application.Current as IApp).Dict_ThematicLayer;
            CreateLayer(queryType);
            Dict_ResultGraphic = new Dictionary<clstipwxy, Graphic>();

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
            if (lstreturngra.Count != 0)
            {
                Graphic gra = lstreturngra[lstreturngra.Count / 2];
                Envelope eve = new Envelope()
                {
                    XMax = gra.Geometry.Extent.XMax + 0.000001,
                    YMax = gra.Geometry.Extent.YMax + 0.000001,
                    XMin = gra.Geometry.Extent.XMin - 0.000001,
                    YMin = gra.Geometry.Extent.YMin - 0.000001
                };

                (Application.Current as IApp).MainMap.ZoomTo(eve);
            }

            data_result.ItemsSource = null;
            data_result.ItemsSource = lstdata;
            //waitanimationwindow.Close();
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
                sp_layer.Children.Clear();
                data_result.ItemsSource = null;
                drc.IsActivated = false;
                drc = null;
                Expose = false;
            }
            catch (Exception)
            { }
        }
        #endregion

        /// <summary>
        /// 单选定位高亮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// 创建查询图层
        /// </summary>
        void CreateLayer(string[] queryType)
        {
            sp_layer.Children.Clear();
            if (queryType != null)
            {
                for (int i = 0; i < Dict_Data.Count(); i++)
                {
                    CheckBox ckb = new CheckBox();
                    ckb.Content = Dict_Data.Keys.ToList()[i];
                    List<Graphic> lsttmp = Dict_Data.Values.ToList()[i].Graphics.ToList();
                    ckb.Tag = lsttmp;
                    if (queryType.Contains(ckb.Content as string))
                        ckb.IsChecked = true;
                    else
                        ckb.IsChecked = false;
                    ckb.Margin = new Thickness(0, 5, 5, 0);
                    ckb.Style = this.Resources["CheckBoxStyle"] as Style;
                    ckb.BorderBrush = null;
                    ckb.BorderThickness = new Thickness(0);
                    ckb.Foreground = new SolidColorBrush(Colors.White);
                    sp_layer.Children.Add(ckb);
                }
                chkLayer.IsChecked = false;
            }
            else
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
            }

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
            ToolTipService.SetToolTip(e.Row, tbx);//ToolTipTransparentStyle
        }
    }

    public class clstipwxy
    {
        public string wxyid { set; get; }
        public string wxyname { set; get; }
        public string wxytip { set; get; }
        public string wxytype { set; get; }
        public string wxydwdm { set; get; }
        public string wxygrid { set;get; }
        public Graphic wxygra { set; get; }
    }
}
