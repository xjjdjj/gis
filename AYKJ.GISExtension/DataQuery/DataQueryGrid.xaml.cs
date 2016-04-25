using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ESRI.ArcGIS.Client.Tasks;
using AYKJ.GISDevelop.Platform;
using System.Xml.Linq;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using System.Windows.Media;
using ESRI.ArcGIS.Client.Symbols;
using AYKJ.GISDevelop.Platform.ToolKit;

namespace AYKJ.GISExtension.DataQuery
{
    public partial class DataQueryGrid : UserControl
    {
        QueryTask querytask;
        //配置文件
        XElement xele;
        string queryurl;
        string drawGrid;
        Map mainmap;
        //当前Geometry
        Dictionary<string, ESRI.ArcGIS.Client.Geometry.Geometry> dicGridGeo;
        //当前展示的次级网格
        Dictionary<string, Graphic> dicSelectSubGridGra;
        
        Dictionary<string, Graphic> dicGridGra;
        Dictionary<string, Graphic> dicSubGridGra;
       
        Dictionary<string, string> dicGrid;
        Dictionary<string, string> dicSubGrid;
        Dictionary<string, string> dicSubCodeFid;
        Dictionary<string, ESRI.ArcGIS.Client.Geometry.Polygon> dicGridSub;
        Dictionary<string, Graphic> dicGridDraw;
        Dictionary<string, CheckBox> dicCheckBox;
        Dictionary<string, CheckBox> dicSubCheckBox;
        Dictionary<string, GraphicsLayer> Dict_Data;
        //查询后属性数据
        List<clstipwxy> lstdata;

        public static GraphicsLayer selectHigh_GraLayer;
        WaitAnimationWindow waitanimationwindow;
        bool isSubGrid = false;
        public static GraphicsLayer Draw_GraLayer;
        public DataQueryGrid()
        {
            InitializeComponent();
            mainmap = (Application.Current as IApp).MainMap;
            dicGridGra = (Application.Current as IApp).Dict_grid_gra;
            dicSubGridGra = (Application.Current as IApp).Dict_subgrid_gra;
            dicGrid = (Application.Current as IApp).Dict_grid;
            dicSubGrid = (Application.Current as IApp).Dict_subgrid;
            dicSubCodeFid = (Application.Current as IApp).Dict_SubCode_FID;

            dicGridDraw = new Dictionary<string, Graphic>();
            Dict_Data = (Application.Current as IApp).Dict_ThematicLayer;
            this.Loaded -= DataQueryGrid_Loaded;
            this.Loaded += new RoutedEventHandler(DataQueryGrid_Loaded);
        }

        void getGridGeo()
        {
            sp_layer.RowDefinitions.Clear();
            sp_layer.ColumnDefinitions.Clear();
            dicCheckBox = new Dictionary<string, CheckBox>();
            int count = dicGrid.Count;
            int rownum = count / 4;
            if (count % 4 != 0)
                rownum++;
            sp_layer.RowDefinitions.Clear();
            sp_layer.ColumnDefinitions.Clear();
            //行
            for (int ii = 0; ii < rownum; ii++)
                sp_layer.RowDefinitions.Add(new RowDefinition());
            //列
            for (int jj = 0; jj < 4; jj++)
                sp_layer.ColumnDefinitions.Add(new ColumnDefinition());

            int i = 0;
            foreach (KeyValuePair<string, Graphic> kv in dicGridGra)
            {
                string key = kv.Key;
                kv.Value.Symbol = new SimpleFillSymbol() { BorderThickness = 2, BorderBrush = new SolidColorBrush() { Color = new Color() { A = 255, B = 255, G = 0, R = 0 } } };
                CreateLayer(dicGrid[key], key, i, rownum);
                i++;
            }
        }

        void DataQueryGrid_Loaded(object sender, RoutedEventArgs e)
        {
            selectHigh_GraLayer = new GraphicsLayer();
            Draw_GraLayer = new GraphicsLayer();
            mainmap.Layers.Add(selectHigh_GraLayer);
            mainmap.Layers.Add(Draw_GraLayer);
            MouseWheelSupportAddOn.Activate(scrolls2, true);
            getGridGeo();
        }

        void CreateLayer(string str, string code, int i, int rownum)
        {
            CheckBox ckb = new CheckBox();
            ckb.Content = str;
            ckb.Checked += new RoutedEventHandler(ckbP_Checked);
            ckb.Unchecked += new RoutedEventHandler(ckbP_Unchecked);
            ckb.Tag = code;
            ckb.IsChecked = false;
            ckb.Margin = new Thickness(0, 5, 5, 0);
            ckb.Style = this.Resources["CheckBoxStyle"] as Style;
            ckb.BorderBrush = null;
            ckb.BorderThickness = new Thickness(0);
            ckb.Foreground = new SolidColorBrush(Colors.White);

            sp_layer.Children.Add(ckb);
            ckb.SetValue(Grid.ColumnProperty, i / rownum);
            ckb.SetValue(Grid.RowProperty, i % rownum);
            dicCheckBox.Add(str, ckb);
            //((System.Windows.Controls.Grid)(this.FindName("sp_layer"+i/rownum))).Children.Add(ckb);
        }

        void ckbP_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox ckb = sender as CheckBox;
            string key = ckb.Tag as string;
            Graphic gra = dicGridGra[key];
            drawGrid = ckb.Content as string;
            Draw_GraLayer.Graphics.Clear();
            Draw_GraLayer.Graphics.Add(gra);


            Envelope eve = new Envelope()
            {
                XMax = gra.Geometry.Extent.XMax + 0.000001,
                YMax = gra.Geometry.Extent.YMax + 0.000001,
                XMin = gra.Geometry.Extent.XMin - 0.000001,
                YMin = gra.Geometry.Extent.YMin - 0.000001
            };
            mainmap.ZoomTo(eve);
            foreach (KeyValuePair<string, CheckBox> kv in dicCheckBox)
            {
                if (kv.Value.IsChecked == true && kv.Key != ckb.Content as string)
                {
                    kv.Value.IsChecked = false;
                }
            }

            dicSelectSubGridGra = new Dictionary<string, Graphic>();
            foreach (KeyValuePair<string, string> kv in dicSubCodeFid)
            {
                //次级网格的外键等于父级网格的code
                if (kv.Value == key)
                {
                    dicSelectSubGridGra.Add(kv.Key, dicSubGridGra[kv.Key]);
                }
            }
        

            isSubGrid = true;
            GridResults.Visibility = Visibility.Visible;
            //每选择一次区县，网格内点数据重新赋值
            lstdata = new List<clstipwxy>();
            data_result.ItemsSource = null;
            sp_layer2.Children.Clear();
            selectHigh_GraLayer.Graphics.Clear();
            dicGridSub = new Dictionary<string, ESRI.ArcGIS.Client.Geometry.Polygon>();

            //组装子网格
            dicSubCheckBox = new Dictionary<string, CheckBox>();
            int count = dicSelectSubGridGra.Count;
            int rownum = count / 4;
            if (count % 4 != 0)
                rownum++;
            sp_layer2.RowDefinitions.Clear();
            sp_layer2.ColumnDefinitions.Clear();
            //行
            for (int ii = 0; ii < rownum; ii++)
                sp_layer2.RowDefinitions.Add(new RowDefinition());
            //列
            for (int jj = 0; jj < 4; jj++)
                sp_layer2.ColumnDefinitions.Add(new ColumnDefinition());

            int i = 0;
            foreach (KeyValuePair<string, Graphic> kv in dicSelectSubGridGra)
            {
                kv.Value.Symbol =new SimpleFillSymbol() { BorderThickness = 2, BorderBrush = new SolidColorBrush() { Color = new Color() { A = 255, B = 255, G = 0, R = 0 } } };
                CreateSubLayer(dicSubGrid[kv.Key], kv.Key, i, rownum);
                i++;
            }

  
        }

        void ckbP_Unchecked(object sender, RoutedEventArgs e)
        {
            if (drawGrid == (sender as CheckBox).Content as string)
            {
                Draw_GraLayer.Graphics.Clear();
                XElement xele = PFApp.Extent;
                Envelope eve = new Envelope()
                {
                    XMax = double.Parse(xele.Element("MapExtent").Attribute("XMax").Value),
                    XMin = double.Parse(xele.Element("MapExtent").Attribute("XMin").Value),
                    YMax = double.Parse(xele.Element("MapExtent").Attribute("YMax").Value),
                    YMin = double.Parse(xele.Element("MapExtent").Attribute("YMin").Value),
                    SpatialReference = mainmap.SpatialReference
                };
                mainmap.ZoomTo(eve);
            }

            sp_layer2.Children.Clear();
            selectHigh_GraLayer.Graphics.Clear();
            data_result.ItemsSource = null;
            GridResults.Visibility = Visibility.Collapsed;
        }


        void CreateSubLayer(string str, Graphic gra, int i,int rownum)
        {
            CheckBox ckb = new CheckBox();
            ckb.Content = str;
            ckb.Checked += new RoutedEventHandler(ckb_Checked);
            ckb.Unchecked += new RoutedEventHandler(ckb_Unchecked);
            ckb.Tag = gra;
            ckb.IsChecked = false;
            ckb.Margin = new Thickness(0, 5, 5, 0);
            ckb.Style = this.Resources["CheckBoxStyle"] as Style;
            ckb.BorderBrush = null;
            ckb.BorderThickness = new Thickness(0);
            ckb.Foreground = new SolidColorBrush(Colors.White);
            sp_layer2.Children.Add(ckb);
            ckb.SetValue(Grid.ColumnProperty, i/rownum);
            ckb.SetValue(Grid.RowProperty, i % rownum);
        }

        void CreateSubLayer(string str, string code, int i, int rownum)
        {
            CheckBox ckb = new CheckBox();
            ckb.Content = str;
            ckb.Checked += new RoutedEventHandler(ckb_Checked);
            ckb.Unchecked += new RoutedEventHandler(ckb_Unchecked);
            ckb.Tag = code;
            ckb.IsChecked = false;
            ckb.Margin = new Thickness(0, 5, 5, 0);
            ckb.Style = this.Resources["CheckBoxStyle"] as Style;
            ckb.BorderBrush = null;
            ckb.BorderThickness = new Thickness(0);
            ckb.Foreground = new SolidColorBrush(Colors.White);
            sp_layer2.Children.Add(ckb);
            ckb.SetValue(Grid.ColumnProperty, i / rownum);
            ckb.SetValue(Grid.RowProperty, i % rownum);
        }

        void ckb_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox ckb = sender as CheckBox;
            string key = ckb.Tag as string;
            Graphic gra = dicSelectSubGridGra[key];
            if(!dicGridSub.ContainsKey(ckb.Content as string))
                dicGridSub.Add(ckb.Content as string, gra.Geometry as ESRI.ArcGIS.Client.Geometry.Polygon);
            Draw_GraLayer.Graphics.Clear();
            if (!dicSubCheckBox.ContainsKey(ckb.Content as string))
            {
                //如果当前网格未画，添加
                dicSubCheckBox.Add(ckb.Content as string, ckb);
                //判断网格内危险源
                foreach (KeyValuePair<string, GraphicsLayer> kv in Dict_Data)
                {
                    GraphicsLayer gl = kv.Value;
                    foreach (Graphic g in gl.Graphics)
                    {
                        if (CheckRegion(gra.Geometry as ESRI.ArcGIS.Client.Geometry.Polygon, g.Geometry as MapPoint)) 
                        {
                            string str = (g.Attributes).Values.ToArray()[2].ToString();
                            string[] strs = str.Trim().Split('|');
            
                            clstipwxy wxy = new clstipwxy()
                            {
                                wxytype = strs[0],
                                wxyname = strs[3],
                                wxydwdm = strs[2],
                                wxyid = strs[1],
                                wxygrid = ckb.Content as string,
                                wxytip = "危险源位于" + ckb.Content as string,
                                wxygra = g
                            };
                            lstdata.Add(wxy);
                        }
                    }
                }
                //dicGridResults.Add(ckb.Content as string, dicGridWXYResults);
            }
            //添加当前网格
            foreach (KeyValuePair<string, CheckBox> kv in dicSubCheckBox)
            {
                string strkey = kv.Value.Tag as string;
                Draw_GraLayer.Graphics.Add(dicSelectSubGridGra[strkey]);
            }
            //更新griddata
            data_result.ItemsSource = null;

            if (lstdata == null || lstdata.Count < 1)
            {
                //查询无结果
                AYKJ.GISDevelop.Platform.ToolKit.Message.Show("当前网格内没有危险源");
            }
            else
            {
                data_result.ItemsSource = lstdata;
            }

            if (dicGridSub.Count > 0)
            {
                ESRI.ArcGIS.Client.Geometry.Polygon pg = new ESRI.ArcGIS.Client.Geometry.Polygon();
                foreach (KeyValuePair<string, ESRI.ArcGIS.Client.Geometry.Polygon> kv in dicGridSub)
                {
                    for (int i = 0; i < kv.Value.Rings.Count; i++)
                        pg.Rings.Add(kv.Value.Rings[i]);
                }

                Envelope eve = new Envelope()
                {
                    XMax = pg.Extent.XMax + 0.000001,
                    YMax = pg.Extent.YMax + 0.000001,
                    XMin = pg.Extent.XMin - 0.000001,
                    YMin = pg.Extent.YMin - 0.000001
                };
                mainmap.ZoomTo(eve);
            }
            else
            {
                Graphic g = dicCheckBox[drawGrid].Tag as Graphic;
                Envelope eve = new Envelope()
                {
                    XMax = g.Geometry.Extent.XMax + 0.000001,
                    YMax = g.Geometry.Extent.YMax + 0.000001,
                    XMin = g.Geometry.Extent.XMin - 0.000001,
                    YMin = g.Geometry.Extent.YMin - 0.000001
                };
                mainmap.ZoomTo(eve);
            }
        }

        void ckb_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox ckb = sender as CheckBox;
            //缩放
            if (dicGridSub.ContainsKey(ckb.Content as string))
                dicGridSub.Remove(ckb.Content as string);
            if (dicGridSub.Count > 0)
            {
                ESRI.ArcGIS.Client.Geometry.Polygon pg = new ESRI.ArcGIS.Client.Geometry.Polygon();
                foreach (KeyValuePair<string, ESRI.ArcGIS.Client.Geometry.Polygon> kv in dicGridSub)
                {
                    for (int i = 0; i < kv.Value.Rings.Count; i++)
                        pg.Rings.Add(kv.Value.Rings[i]);
                }

                Envelope eve = new Envelope()
                {
                    XMax = pg.Extent.XMax + 0.000001,
                    YMax = pg.Extent.YMax + 0.000001,
                    XMin = pg.Extent.XMin - 0.000001,
                    YMin = pg.Extent.YMin - 0.000001
                };
                mainmap.ZoomTo(eve);
            }
            else
            {
                string strkey = dicCheckBox[drawGrid].Tag as string;
                Graphic gra = dicGridGra[strkey];

                Envelope eve = new Envelope()
                {
                    XMax = gra.Geometry.Extent.XMax + 0.000001,
                    YMax = gra.Geometry.Extent.YMax + 0.000001,
                    XMin = gra.Geometry.Extent.XMin - 0.000001,
                    YMin = gra.Geometry.Extent.YMin - 0.000001
                };
                mainmap.ZoomTo(eve);
            }

            if (dicSubCheckBox.ContainsKey(ckb.Content as string))
                dicSubCheckBox.Remove(ckb.Content as string);
            //移除并重新加载网格
            Draw_GraLayer.Graphics.Clear();
            foreach (KeyValuePair<string, CheckBox> kv in dicSubCheckBox)
            {
                string strkey = kv.Value.Tag as string;
                Draw_GraLayer.Graphics.Add(dicSelectSubGridGra[strkey]);
            }

            //移除并更新危险源
            List<clstipwxy> delwxy = new List<clstipwxy>();
            foreach (clstipwxy wxy in lstdata)
            {
                if (wxy.wxygrid == ckb.Content as string)
                    delwxy.Add(wxy);
            }
            foreach (clstipwxy wxy in delwxy)
            {
                if (lstdata.Contains(wxy))
                    lstdata.Remove(wxy);
            }
            data_result.ItemsSource = null;
            if (lstdata != null && lstdata.Count > 0)
            {
                data_result.ItemsSource = lstdata;
            }
        }

        bool isDrawed = true;
        ///// <summary>
        ///// 全选图层
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void chk_Checked(object sender, RoutedEventArgs e)
        //{
        //    isDrawed = false;
        //    for (int i = 0; i < sp_layer.Children.Count; i++)
        //    {
        //        CheckBox c = sp_layer.Children[i] as CheckBox;
        //        c.IsChecked = true;
        //    }
        //    foreach (KeyValuePair<string, Graphic> kv in dicGridDraw)
        //    {
        //        Draw_GraLayer.Graphics.Add(kv.Value);
        //    }
        //    //data_result.ItemsSource = null;
        //}

        ///// <summary>
        ///// 全部选图层
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void chk_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    isDrawed = false;
        //    dicGridDraw.Clear();
        //    Draw_GraLayer.Graphics.Clear();
        //    for (int i = 0; i < sp_layer.Children.Count; i++)
        //    {
        //        CheckBox c = sp_layer.Children[i] as CheckBox;
        //        c.IsChecked = false;
        //    }
        //    isDrawed = true;
        //    //data_result.ItemsSource = null;
        //}
        #region Grid事件
        private void btn_postion_Click(object sender, RoutedEventArgs e)
        {
            selectHigh_GraLayer.Graphics.Clear();
            if (data_result.SelectedItem != null)
            {
                Graphic gra = (data_result.SelectedItem as clstipwxy).wxygra;
                Graphic g = new Graphic();
                g.Geometry = gra.Geometry as MapPoint;
                g.Symbol = HighMarkerStyle;
                selectHigh_GraLayer.Graphics.Add(g);

                Envelope eve = new Envelope()
                {
                    XMax = g.Geometry.Extent.XMax + 0.000001,
                    YMax = g.Geometry.Extent.YMax + 0.000001,
                    XMin = g.Geometry.Extent.XMin - 0.000001,
                    YMin = g.Geometry.Extent.YMin - 0.000001
                };
                mainmap.ZoomTo(eve);
            }
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
        }
        #endregion
        #region 重置信息
        public void Reset()
        {
            try
            {
                dicGridDraw = new Dictionary<string, Graphic>();
                sp_layer2.Children.Clear();
                sp_layer.Children.Clear();
                data_result.ItemsSource = null;
                GridResults.Visibility = Visibility.Collapsed;
                selectHigh_GraLayer.Graphics.Clear();
                Draw_GraLayer.Graphics.Clear();
            }
            catch (Exception)
            { }
        }
        #endregion

        #region 辅助算法
        public bool CheckRegion(ESRI.ArcGIS.Client.Geometry.Polygon region, MapPoint point)
        {
            ESRI.ArcGIS.Client.Geometry.PointCollection points = region.Rings[0];
            //Point2DCollection points = region.Parts[0];

            double pValue = double.NaN;
            int i = 0;
            int j = 0;

            double yValue = double.NaN;
            int m = 0;
            int n = 0;

            double iPointX = double.NaN;
            double iPointY = double.NaN;
            double jPointX = double.NaN;
            double jPointY = double.NaN;

            int k = 0;
            int p = 0;

            yValue = points[0].Y - points[(points.Count - 1)].Y;
            if (yValue < 0)
            {
                p = 1;
            }
            else if (yValue > 0)
            {
                p = 0;
            }
            else
            {
                m = points.Count - 2;
                n = m + 1;
                while (points[m].Y == points[n].Y)
                {
                    m--;
                    n--;

                    if (m == 0)
                    {
                        m = points.Count - 2;
                        n = m + 1;

                        while (points[m].X == points[n].X)
                        {
                            m--;
                            n--;

                            if (m == 0)
                            {
                                ////polygon只是点
                                if (points[0].X == point.X && points[0].Y == point.Y)
                                {
                                    return true;
                                }
                                else
                                    return false;
                            }
                        }

                        return true;
                    }
                }
                yValue = points[n].Y - points[m].Y;

                if (yValue < 0)
                {
                    p = 1;
                }
                else if (yValue > 0)
                {
                    p = 0;
                }
            }


            //使多边形封闭
            int count = points.Count;
            i = 0;
            j = count - 1;
            while (i < count)
            {
                iPointX = points[j].X;
                iPointY = points[j].Y;
                jPointX = points[i].X;
                jPointY = points[i].Y;
                if (point.Y > iPointY)
                {
                    if (point.Y < jPointY)
                    {
                        pValue = (point.Y - iPointY) * (jPointX - iPointX) / (jPointY - iPointY) + iPointX;
                        if (point.X < pValue)
                        {
                            k++;
                        }
                        else if (point.X == pValue)
                        {
                            return true;
                        }
                    }
                    else if (point.X == jPointY)
                    {
                        p = 0;
                    }
                }
                else if (point.Y < iPointY)
                {
                    if (point.Y > jPointY)
                    {
                        pValue = (point.Y - iPointY) * (jPointX - iPointX) / (jPointY - iPointY) + iPointX;
                        if (point.X < pValue)
                        {
                            k++;
                        }
                        else if (point.X == pValue)
                        {
                            return true;
                        }
                    }
                    else if (point.Y == jPointY)
                    {
                        p = 1;
                    }
                }
                else
                {
                    if (point.X == iPointX)
                    {
                        return true;
                    }
                    if (point.Y < jPointY)
                    {
                        if (p != 1)
                        {
                            if (point.X < iPointX)
                            {
                                k++;
                            }
                        }
                    }
                    else if (point.Y > jPointY)
                    {
                        if (p > 0)
                        {
                            if (point.X < iPointX)
                            {
                                k++;
                            }
                        }
                    }
                    else
                    {
                        if (point.X > iPointX && point.X <= jPointX)
                        {
                            return true;
                        }
                        if (point.X < iPointX && point.X >= jPointX)
                        {
                            return true;
                        }
                    }
                }
                j = i;
                i++;
            }

            if (k % 2 != 0)
            {
                return true;
            }
            return false;
        }
        #endregion
    }
}
