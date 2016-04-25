#region << 版 本 注 释 >>
/*
 * ========================================================================
 * Copyright(c)  陈锋, All Rights Reserved.
 * ========================================================================
 * CLR版本：       4.0.30319.261
 * 类 名 称：       ThematicLayerControl
 * 机器名称：       GIS-FLYH
 * 命名空间：       AYKJ.GISDevelop.Control
 * 文 件 名：       ThematicLayerControl
 * 创建时间：       2012/7/19 9:15:37
 * 作    者：       陈锋
 * 功能说明：       
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
using AYKJ.GISDevelop.Platform;
using ESRI.ArcGIS.Client;
using System.Windows.Controls.Primitives;
using System.Xml.Linq;
using ESRI.ArcGIS.Client.Geometry;

namespace AYKJ.GISDevelop.Control
{
    public partial class ThematicLayerControl : UserControl
    {
        Map map;
        double dblenged = 0;
        Dictionary<string, double> Dict_GridHeight;
        List<CheckBox> lstckb = new List<CheckBox>();
        public ToggleButton currrentogbtn;

        #region 区域快速定位
        //市域级名称列表
        Dictionary<string, string> Dict_sy;
        Dictionary<string, Graphic> Dict_sygra;
        //区县级名称列表
        Dictionary<string, string> Dict_qx;
        Dictionary<string, Graphic> Dict_qxgra;
        //市域与区县挂钩
        Dictionary<string, List<string>> Dict_syqx;
        //存储第一排名称
        List<string> lstfirstname;
        #endregion

        public ThematicLayerControl()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(ThematicLayerControl_Loaded);
        }

        void ThematicLayerControl_Loaded(object sender, RoutedEventArgs e)
        {
            toc_grid.Children.Clear();
            map = (Application.Current as IApp).MainMap;
            //设置面板的起始位置
            this.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            this.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            this.Margin = new Thickness(0, 0, 30, 5);
            Dict_GridHeight = new Dictionary<string, double>();
            //if (MapControl.ThematicSp != null)
            //{
            //    toc_grid.Children.Add(MapControl.ThematicSp);
            //}
            if (MapNavControl.ThematicSp != null)
            {
                toc_grid.Children.Add(MapNavControl.ThematicSp);
            }
            LoadMapLayers();
            Storyboard_Close.Completed += new EventHandler(Storyboard_Close_Completed);

            MouseWheelSupportAddOn.Activate(scrollviewer, true);

            if ((Application.Current as IApp).Dict_Xzqz_sy == null ||
                (Application.Current as IApp).Dict_Xzqz_sygra == null ||
                (Application.Current as IApp).Dict_Xzqz_qx == null ||
                (Application.Current as IApp).Dict_Xzqz_qxgra == null)
            {
                sp_qh.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                sp_qh.Visibility = System.Windows.Visibility.Visible;
                CreateXZQH();
            }
        }

        /// <summary>
        /// 加载地图服务图层控制
        /// </summary>
        void LoadMapLayers()
        {
            ks_sp.Children.Clear();
            for (int i = 0; i < map.Layers.Count(); i++)
            {
                Layer layer = map.Layers[i];
                if (layer.GetType().ToString().Contains("ArcGISTiledMapServiceLayer"))
                {
                    //20130918:此处判断用作限制最底层服务不可隐藏。
                    if (i == 0)
                        CreateTiledControl(layer, false);
                    else
                        CreateTiledControl(layer, true);
                }
                else if (layer.GetType().ToString().Contains("ArcGISDynamicMapServiceLayer"))
                {
                    //20130918:此处判断用作限制最底层服务不可隐藏。
                    if (i == 0)
                        CreateDynamicControl(layer, false);
                    else
                        CreateDynamicControl(layer, true);
                }
                else if (layer.GetType().ToString().Contains("ArcGISTiledLayerForBaidu"))
                {
                    //20130918:此处判断用作限制最底层服务不可隐藏。
                    if (i == 0)
                        CreateBaiDuLayerControl(layer, false);
                    else
                        CreateBaiDuLayerControl(layer, true);
                }
                else if (layer.GetType().ToString().Contains("BaiduMapLayer"))
                {
                    //20130918:此处判断用作限制最底层服务不可隐藏。
                    if (i == 0)
                        CreateBaiduMapLayerControl(layer, false);
                    else
                        CreateBaiduMapLayerControl(layer, true);
                }
            }

            for (int i = 0; i < lstckb.Count(); i++)
            {
                lstckb[i].IsChecked = false;
            }
        }

        /// <summary>
        /// 创建缓存服务控件
        /// </summary>
        /// <param name="tmplayer"></param>
        void CreateBaiDuLayerControl(Layer tmplayer, Boolean serverIsHidden)
        {
            ArcGISTiledLayerForBaidu agtms = tmplayer as ArcGISTiledLayerForBaidu;
            StackPanel spagtms = new StackPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Orientation = Orientation.Vertical
            };
            StackPanel spagtmsfather = CreateBaiduServerControl(tmplayer, serverIsHidden);
            spagtms.Children.Add(spagtmsfather);
            ks_sp.Children.Insert(0, spagtms);
        }

        void CreateBaiduMapLayerControl(Layer tmplayer, Boolean serverIsHidden)
        {
            BaiduMapLayer agtms = tmplayer as BaiduMapLayer;
            StackPanel spagtms = new StackPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Orientation = Orientation.Vertical
            };
            StackPanel spagtmsfather = CreateBaiduServerControl(tmplayer, serverIsHidden);
            spagtms.Children.Add(spagtmsfather);
            ks_sp.Children.Insert(0, spagtms);
        }

        /// <summary>
        /// 创建服务控件
        /// </summary>
        /// <param name="tmplayer"></param>
        /// <param name="spagtmschild"></param>
        /// <returns></returns>
        StackPanel CreateBaiduServerControl(Layer tmplayer, Boolean serverIsHidden)
        {
            #region 生成服务控件
            StackPanel spagtmsfather = new StackPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Orientation = Orientation.Horizontal
            };
            CheckBox ckbagtms = new CheckBox();
            ckbagtms.FontFamily = new System.Windows.Media.FontFamily("NSimSun");
            ckbagtms.FontSize = 12;
            ckbagtms.Content = tmplayer.ID;
            ckbagtms.IsChecked = tmplayer.Visible;
            ckbagtms.IsEnabled = serverIsHidden;
            ckbagtms.Tag = tmplayer;
            ckbagtms.Checked += new RoutedEventHandler(ckbagtms_Checked);
            ckbagtms.Unchecked += new RoutedEventHandler(ckbagtms_Unchecked);
            ckbagtms.Style = this.Resources["CheckBoxStyle"] as Style;
            ckbagtms.BorderBrush = null;
            ckbagtms.BorderThickness = new Thickness(0);
            ckbagtms.Foreground = new SolidColorBrush() { Color = Colors.White };
            spagtmsfather.Children.Add(ckbagtms);
            #endregion
            return spagtmsfather;
        }

        /// <summary>
        /// 创建缓存服务控件
        /// </summary>
        /// <param name="tmplayer"></param>
        void CreateTiledControl(Layer tmplayer, Boolean serverIsHidden)
        {
            ArcGISTiledMapServiceLayer agtms = tmplayer as ArcGISTiledMapServiceLayer;
            StackPanel spagtms = new StackPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Orientation = Orientation.Vertical
            };

            StackPanel spagtmschild = CreateTiledLayerControl(agtms);
            StackPanel spagtmsfather = CreateServerControl(tmplayer, spagtmschild, serverIsHidden);

            spagtms.Children.Add(spagtmsfather);
            spagtms.Children.Add(spagtmschild);

            ks_sp.Children.Insert(0, spagtms);
        }

        /// <summary>
        /// 创建动态服务控件
        /// </summary>
        /// <param name="tmplayer"></param>
        void CreateDynamicControl(Layer tmplayer, Boolean serverIsHidden)
        {
            ArcGISDynamicMapServiceLayer agdms = tmplayer as ArcGISDynamicMapServiceLayer;
            StackPanel spagtms = new StackPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Orientation = Orientation.Vertical
            };

            StackPanel spagtmschild = CreateDynamicLayerControl(agdms);
            //TransformGroup tmpTransformGroup = new TransformGroup();
            //ScaleTransform scaleTransform = new ScaleTransform();
            //scaleTransform.ScaleX = 1;
            //scaleTransform.ScaleY = 1;
            //tmpTransformGroup.Children.Add(scaleTransform);
            //spagtmschild.RenderTransform = tmpTransformGroup;
            StackPanel spagtmsfather = CreateServerControl(tmplayer, spagtmschild, serverIsHidden);
            spagtms.Children.Add(spagtmsfather);
            spagtms.Children.Add(spagtmschild);
            ks_sp.Children.Insert(0, spagtms);
        }

        /// <summary>
        /// 创建服务控件
        /// </summary>
        /// <param name="tmplayer"></param>
        /// <param name="spagtmschild"></param>
        /// <returns></returns>
        StackPanel CreateServerControl(Layer tmplayer, StackPanel spagtmschild, Boolean serverIsHidden)
        {
            #region 生成服务控件
            StackPanel spagtmsfather = new StackPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Orientation = Orientation.Horizontal
            };

            CheckBox ckbfather = new CheckBox();
            ckbfather.Height = 12;
            ckbfather.Width = 12;
            ckbfather.Style = this.Resources["TreeCheckBoxStyle"] as Style;
            ckbfather.Margin = new Thickness(0, 0, 5, 0);
            ckbfather.Tag = spagtmschild;
            ckbfather.Checked += new RoutedEventHandler(ckbfather_Checked);
            ckbfather.Unchecked += new RoutedEventHandler(ckbfather_Unchecked);

            ckbfather.IsChecked = false;
            spagtmschild.Height = 0;
            //lstckb.Add(ckbfather);

            CheckBox ckbagtms = new CheckBox();
            ckbagtms.FontFamily = new System.Windows.Media.FontFamily("NSimSun");
            ckbagtms.FontSize = 12;
            ckbagtms.Content = tmplayer.ID;
            ckbagtms.IsChecked = tmplayer.Visible;
            ckbagtms.IsEnabled = serverIsHidden;
            ckbagtms.Tag = tmplayer;
            ckbagtms.Checked += new RoutedEventHandler(ckbagtms_Checked);
            ckbagtms.Unchecked += new RoutedEventHandler(ckbagtms_Unchecked);
            ckbagtms.Style = this.Resources["CheckBoxStyle"] as Style;
            ckbagtms.BorderBrush = null;
            ckbagtms.BorderThickness = new Thickness(0);
            ckbagtms.Foreground = new SolidColorBrush() { Color = Colors.White };

            spagtmsfather.Children.Add(ckbfather);
            spagtmsfather.Children.Add(ckbagtms);
            #endregion
            return spagtmsfather;
        }

        /// <summary>
        /// 创建缓存图层控件
        /// </summary>
        /// <param name="agtms"></param>
        /// <returns></returns>
        StackPanel CreateTiledLayerControl(ArcGISTiledMapServiceLayer agtms)
        {
            #region 生成图层控制控件
            StackPanel spagtmschild = new StackPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Orientation = Orientation.Vertical,
                Margin = new Thickness(35, 0, 0, 5)
            };
            spagtmschild.Name = "spagtmschild_" + agtms.ID;
            Dict_GridHeight.Add(spagtmschild.Name, 21 * agtms.Layers.Count());
            for (int m = 0; m < agtms.Layers.Count(); m++)
            {
                Grid grid = new Grid()
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(0, 5, 0, 0)
                };
                StackPanel sp = new StackPanel()
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    Orientation = Orientation.Horizontal
                };
                CheckBox ckblayer = new CheckBox();
                ckblayer.FontFamily = new System.Windows.Media.FontFamily("NSimSun");
                ckblayer.FontSize = 12;
                ckblayer.Content = agtms.Layers[m].Name;
                ckblayer.IsEnabled = false;
                ckblayer.Style = this.Resources["CheckBoxStyle"] as Style;
                ckblayer.BorderBrush = null;
                ckblayer.BorderThickness = new Thickness(0);
                ckblayer.Foreground = new SolidColorBrush() { Color = Colors.White };

                if (agtms.Layers[m].DefaultVisibility == true)
                {
                    ckblayer.IsChecked = true;
                }
                else
                {
                    ckblayer.IsChecked = false;
                }
                sp.Children.Add(ckblayer);
                grid.Children.Add(sp);
                spagtmschild.Children.Add(grid);
            }
            #endregion
            return spagtmschild;
        }

        /// <summary>
        /// 创建动态图层控件
        /// </summary>
        /// <param name="agdms"></param>
        /// <returns></returns>
        StackPanel CreateDynamicLayerControl(ArcGISDynamicMapServiceLayer agdms)
        {
            #region 生成图层控制控件
            StackPanel spagtmschild = new StackPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Orientation = Orientation.Vertical,
                Margin = new Thickness(35, 0, 0, 5)
            };
            spagtmschild.Name = "spagtmschild_" + agdms.ID;
            Dict_GridHeight.Add(spagtmschild.Name, 21 * agdms.Layers.Count());
            for (int m = 0; m < agdms.Layers.Count(); m++)
            {
                Grid grid = new Grid()
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(0, 5, 0, 0)
                };
                StackPanel sp = new StackPanel()
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    Orientation = Orientation.Horizontal
                };
                CheckBox ckblayer = new CheckBox();
                ckblayer.FontFamily = new System.Windows.Media.FontFamily("NSimSun");
                ckblayer.FontSize = 12;
                ckblayer.Content = agdms.Layers[m].Name;
                ckblayer.Checked += new RoutedEventHandler(ckblayer_Checked);
                ckblayer.Unchecked += new RoutedEventHandler(ckblayer_Unchecked);
                ckblayer.Style = this.Resources["CheckBoxStyle"] as Style;
                ckblayer.BorderBrush = null;
                ckblayer.BorderThickness = new Thickness(0);
                ckblayer.Foreground = new SolidColorBrush() { Color = Colors.White };
                List<object> lst = new List<object>();
                lst.Add(agdms);
                lst.Add(m);
                ckblayer.Tag = lst;
                ckblayer.IsEnabled = true;
                if (agdms.Layers[m].DefaultVisibility == true)
                {
                    ckblayer.IsChecked = true;
                }
                else
                {
                    ckblayer.IsChecked = false;
                }
                sp.Children.Add(ckblayer);
                grid.Children.Add(sp);
                spagtmschild.Children.Add(grid);
            }
            #endregion
            //spagtmschild.Height = spagtmschild.ActualHeight;
            return spagtmschild;
        }

        #region 控制服务图层控件的显示隐藏
        void ckbfather_Unchecked(object sender, RoutedEventArgs e)
        {
            StackPanel sptmp = (sender as CheckBox).Tag as StackPanel;
            CloseLayerStoryboard(sptmp, sptmp.Name);
        }

        void ckbfather_Checked(object sender, RoutedEventArgs e)
        {
            StackPanel sptmp = (sender as CheckBox).Tag as StackPanel;
            OpenLayerStoryboard(sptmp, sptmp.Name);
        }
        #endregion

        #region 控制服务的显示隐藏
        void ckbagtms_Unchecked(object sender, RoutedEventArgs e)
        {
            ((sender as CheckBox).Tag as Layer).Visible = false;
        }

        void ckbagtms_Checked(object sender, RoutedEventArgs e)
        {
            ((sender as CheckBox).Tag as Layer).Visible = true;
        }
        #endregion

        #region 控制图层的显示隐藏
        void ckblayer_Unchecked(object sender, RoutedEventArgs e)
        {
            List<object> lst = (sender as CheckBox).Tag as List<object>;
            ArcGISDynamicMapServiceLayer agdms = lst[0] as ArcGISDynamicMapServiceLayer;
            List<int> lstint = new List<int>();
            if (agdms.VisibleLayers != null)
            {
                for (int i = 0; i < agdms.VisibleLayers.Length; i++)
                {
                    lstint.Add(agdms.VisibleLayers[i]);
                }
            }
            if (lstint.Contains(Convert.ToInt32(lst[1])))
            {
                lstint.Remove(Convert.ToInt32(lst[1]));
            }
            agdms.VisibleLayers = lstint.ToArray();
        }

        void ckblayer_Checked(object sender, RoutedEventArgs e)
        {
            List<object> lst = (sender as CheckBox).Tag as List<object>;
            ArcGISDynamicMapServiceLayer agdms = lst[0] as ArcGISDynamicMapServiceLayer;
            List<int> lstint = new List<int>();
            if (agdms.VisibleLayers != null)
            {
                for (int i = 0; i < agdms.VisibleLayers.Length; i++)
                {
                    lstint.Add(agdms.VisibleLayers[i]);
                }
            }
            if (!lstint.Contains(Convert.ToInt32(lst[1])))
            {
                lstint.Add(Convert.ToInt32(lst[1]));
            }
            agdms.VisibleLayers = lstint.ToArray();
        }
        #endregion

        #region 两侧面板的展示和关闭
        /// <summary>
        /// 量测面板展开
        /// </summary>
        public void Show()
        {
            PFApp.Root.Children.Add(this);
            Storyboard_Show.Begin();
        }
        /// <summary>
        /// 面板关闭方法
        /// </summary>
        public void Close()
        {
            currrentogbtn.IsChecked = false;
            Storyboard_Close.Begin();
        }

        void Storyboard_Close_Completed(object sender, EventArgs e)
        {
            PFApp.Root.Children.Remove(this);
        }

        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        #endregion

        #region 展开子图层动画
        void OpenLayerStoryboard(StackPanel spagtmschild, string spid)
        {
            try
            {
                dblenged = Dict_GridHeight[spid];
                Storyboard sboard = new Storyboard();
                DoubleAnimation danima = new DoubleAnimation();
                danima.SetValue(Storyboard.TargetNameProperty, spid);
                danima.SetValue(Storyboard.TargetPropertyProperty, new PropertyPath("FrameworkElement.Height"));
                //danima.SetValue(Storyboard.TargetPropertyProperty, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleY)"));
                danima.From = 0;
                danima.To = dblenged;
                danima.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 500));
                sboard.Children.Add(danima);
                if (this.LayoutRoot.Resources.Contains("Storyboard_LayerClose"))
                {
                    this.LayoutRoot.Resources.Remove("Storyboard_LayerClose");
                }
                this.LayoutRoot.Resources.Add("Storyboard_LayerClose", sboard);
                sboard.Begin();
            }
            catch (Exception ex)
            { }
        }
        #endregion

        #region 收缩子图层动画
        void CloseLayerStoryboard(StackPanel spagtmschild, string spid)
        {
            try
            {
                dblenged = Dict_GridHeight[spid];
                Storyboard sboard = new Storyboard();
                DoubleAnimation danima = new DoubleAnimation();
                danima.SetValue(Storyboard.TargetNameProperty, spid);
                danima.SetValue(Storyboard.TargetPropertyProperty, new PropertyPath("FrameworkElement.Height"));//(UIElement.RenderTransform).(CompositeTransform.ScaleY)
                //danima.SetValue(Storyboard.TargetPropertyProperty, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleY)"));
                danima.From = dblenged;
                danima.To = 0;
                danima.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 500));
                sboard.Children.Add(danima);
                if (this.LayoutRoot.Resources.Contains("Storyboard_LayerClose"))
                {
                    this.LayoutRoot.Resources.Remove("Storyboard_LayerClose");
                }
                this.LayoutRoot.Resources.Add("Storyboard_LayerClose", sboard);
                sboard.Begin();
            }
            catch (Exception ex)
            { }
        }
        #endregion

        #region 加载快速定位区域窗口
        void CreateXZQH()
        {
            Dict_sy = (Application.Current as IApp).Dict_Xzqz_sy;
            Dict_sygra = (Application.Current as IApp).Dict_Xzqz_sygra;
            Dict_qx = (Application.Current as IApp).Dict_Xzqz_qx;
            Dict_qxgra = (Application.Current as IApp).Dict_Xzqz_qxgra;
            Dict_syqx = new Dictionary<string, List<string>>();
            lstfirstname = new List<string>();
            SYQX();
            CreateSyQx(Dict_sy.Values.ToList());
            border_layer.Height = 355 - 25 - ((Dict_sy.Count / 4) + 1) * 19;
        }

        /// <summary>
        /// 根据省名获取市名
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_fj_Click(object sender, RoutedEventArgs e)
        {
            CreateSyQx(Dict_sy.Values.ToList());
            lstfirstname = new List<string>();
            sp_sh.Children.Clear();
            XElement xele = PFApp.Extent;
            Envelope eve = new Envelope()
            {
                XMax = double.Parse(xele.Element("MapExtent").Attribute("XMax").Value),
                XMin = double.Parse(xele.Element("MapExtent").Attribute("XMin").Value),
                YMax = double.Parse(xele.Element("MapExtent").Attribute("YMax").Value),
                YMin = double.Parse(xele.Element("MapExtent").Attribute("YMin").Value),
                SpatialReference = map.SpatialReference
            };
            map.ZoomTo(eve);
            border_layer.Height = 355 - 25 - ((Dict_sy.Count / 4) + 1) * 19;
        }

        /// <summary>
        /// 区域定位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnSYCX_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            string strname = btn.Content.ToString();
            if (Dict_sy.Values.ToList().Contains(strname))
            {
                map.ZoomTo(Dict_sygra[strname].Geometry);
                if (!lstfirstname.Contains(strname))
                {
                    lstfirstname.Add(strname);
                    TextBlock txt = new TextBlock();
                    txt.Text = " > ";
                    txt.Foreground = new SolidColorBrush(Colors.White);
                    sp_sh.Children.Add(txt);
                    Button btntmp = new Button();
                    btntmp.Style = this.Resources["ButtonStyle_xzqh"] as Style;
                    btntmp.Click += new RoutedEventHandler(btnSYCX_Click);
                    btntmp.Content = btn.Content;
                    btntmp.Cursor = Cursors.Hand;
                    sp_sh.Children.Add(btntmp);
                }
                else
                {
                    TextBlock txttmp = sp_sh.Children[0] as TextBlock;
                    Button btntmp = sp_sh.Children[1] as Button;
                    sp_sh.Children.Clear();
                    sp_sh.Children.Add(txttmp);
                    sp_sh.Children.Add(btntmp);
                    if (lstfirstname.Count == 2)
                        lstfirstname.RemoveAt(1);
                }
                List<string> lst = Dict_syqx[btn.Content.ToString()];
                CreateSyQx(lst);
                border_layer.Height = 355 - 25 - ((lst.Count / 4) + 1) * 19;
            }
            else if (Dict_qx.Values.ToList().Contains(strname))
            {
                map.ZoomTo(Dict_qxgra[strname].Geometry);
                sp_xzqh.Children.Clear();
                border_layer.Height = 355 - 25;
                if (!lstfirstname.Contains(strname))
                {
                    lstfirstname.Add(strname);
                    TextBlock txt = new TextBlock();
                    txt.Text = " > ";
                    txt.Foreground = new SolidColorBrush(Colors.White);
                    sp_sh.Children.Add(txt);
                    Button btntmp = new Button();
                    btntmp.Style = this.Resources["ButtonStyle_xzqh"] as Style;
                    btntmp.Click += new RoutedEventHandler(btnSYCX_Click);
                    btntmp.Content = btn.Content;
                    btntmp.Cursor = Cursors.Hand;
                    sp_sh.Children.Add(btntmp);
                }
            }
        }

        /// <summary>
        /// 创建上级行政单位的所属下级行政单位
        /// </summary>
        /// <param name="lst"></param>
        /// <param name="strtype"></param>
        void CreateSyQx(List<string> lst)
        {
            sp_xzqh.Children.Clear();
            int intspcount = lst.Count() / 4;
            for (int i = 0; i < intspcount; i++)
            {
                StackPanel sphor = new StackPanel();
                sphor.Margin = new Thickness() { Left = 14, Right = 10, Top = 5 };
                sphor.Orientation = Orientation.Horizontal;
                for (int m = 0; m < 4; m++)
                {
                    Button btn = new Button();
                    btn.Style = this.Resources["ButtonStyle_xzqh"] as Style;
                    btn.Content = lst[i * 4 + m];
                    btn.Cursor = Cursors.Hand;

                    btn.Click += new RoutedEventHandler(btnSYCX_Click);

                    TextBlock txt = new TextBlock();
                    txt.Text = " > ";
                    txt.Foreground = new SolidColorBrush(Colors.White);
                    sphor.Children.Add(btn);
                    sphor.Children.Add(txt);
                }
                sphor.Children.RemoveAt(sphor.Children.Count - 1);
                sp_xzqh.Children.Add(sphor);
            }
            StackPanel sp_tmp = new StackPanel();
            sp_tmp.Margin = new Thickness() { Left = 14, Right = 10, Top = 5 };
            sp_tmp.Orientation = Orientation.Horizontal;
            if (lst.Count - sp_xzqh.Children.Count * 4 == 0)
                return;
            for (int i = 0; i < lst.Count - sp_xzqh.Children.Count * 4; i++)
            {
                Button btn = new Button();
                btn.Style = this.Resources["ButtonStyle_xzqh"] as Style;
                btn.Content = lst[sp_xzqh.Children.Count * 4 + i];
                btn.Cursor = Cursors.Hand;

                btn.Click += new RoutedEventHandler(btnSYCX_Click);

                TextBlock txt = new TextBlock();
                txt.Foreground = new SolidColorBrush(Colors.White);
                txt.Text = " > ";
                sp_tmp.Children.Add(btn);
                sp_tmp.Children.Add(txt);
            }
            sp_tmp.Children.RemoveAt(sp_tmp.Children.Count - 1);
            sp_xzqh.Children.Add(sp_tmp);
        }

        /// <summary>
        /// 市域级与区县级挂钩
        /// </summary>
        void SYQX()
        {
            List<string> lsttmp = new List<string>();
            lsttmp.Add("全部");
            Dict_syqx.Add("全部", lsttmp);
            for (int i = 0; i < Dict_qx.Count; i++)
            {
                string str = Dict_qx.Keys.ToList()[i].Substring(0, Dict_qx.Keys.ToList()[i].Length - 2);
                if (Dict_syqx.Keys.Contains(Dict_sy[str + "00"]))
                {
                    List<string> lst = Dict_syqx[Dict_sy[str + "00"]];
                    lst.Add(Dict_qx.Values.ToList()[i]);
                    Dict_syqx.Remove(Dict_sy[str + "00"]);
                    Dict_syqx.Add(Dict_sy[str + "00"], lst);
                }
                else
                {
                    List<string> lst = new List<string>();
                    lst.Add(Dict_qx.Values.ToList()[i]);
                    Dict_syqx.Add(Dict_sy[str + "00"], lst);
                }
            }
        }
        #endregion

    }
}
