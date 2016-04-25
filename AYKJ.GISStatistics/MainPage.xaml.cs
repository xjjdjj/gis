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
using AYKJ.GISDevelop.Platform;
using System.Xml.Linq;
using AYKJ.GISDevelop.Platform.Part;
using AYKJ.GISDevelop.Platform.ToolKit;
using ESRI.ArcGIS.Client.Tasks;
using System.Text;
using Visifire.Charts;

namespace AYKJ.GISStatistics
{
    public partial class MainPage : UserControl, IWidgets
    {
        #region 变量定义
        //定义一个Map用来接收平台的Map
        Map mainmap;
        //所有的专题数据
        Dictionary<string, GraphicsLayer> Dict_Data;
        //动画
        WaitAnimationWindow waitanimationwindow;
        //geo服务
        string strGeometryurl;
        //区县级地址
        string strxjqh;
        //乡镇级地址
        string strxzqh;
        //行政字段
        string strregionname;
        //区县列表
        Dictionary<string, Graphic> Dict_City;
        //乡镇列表
        Dictionary<string, Graphic> Dict_Town;
        //区县乡镇对照关系
        Dictionary<string, List<string>> Dict_CityTown;
        //空间查询服务
        clsRangeQuery clsrangequery;
        List<clstipwxy> lstdata;
        //高亮图层
        GraphicsLayer Data_GraLayer;
        //绑定的空间数据
        Dictionary<clstipwxy, Graphic> Dict_ResultGraphic;
        #endregion

        public MainPage()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            waitanimationwindow = new WaitAnimationWindow("行政区划获取中....");
            waitanimationwindow.Show();
            //设置面板的起始位置
            this.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            this.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            this.Margin = new Thickness() { Top = 10, Right = 13 };
            Storyboard_Close.Completed += new EventHandler(Storyboard_Close_Completed);

            mainmap = (Application.Current as IApp).MainMap;
            Dict_Data = (Application.Current as IApp).Dict_ThematicLayer;
            rbtn_data.IsChecked = true;
            //创建图层
            CreateLayer();
            XElement xele = PFApp.Extent;
            strGeometryurl = (from item in PFApp.Extent.Elements("GeometryService")
                              select item.Attribute("Url").Value).ToArray()[0];

            var xzqhUrl = (from item in xele.Element("ExtensionServices").Elements("ExtensionService")
                           where item.Attribute("Name").Value == "行政区域"
                           select new
                           {
                               Name = item.Attribute("Name").Value,
                               Url = item.Attribute("Url").Value,
                           }).ToList();
            strxzqh = xzqhUrl[0].Url;
            var xjqhUrl = (from item in xele.Element("ExtensionServices").Elements("ExtensionService")
                           where item.Attribute("Name").Value == "行政区域县界"
                           select new
                           {
                               Name = item.Attribute("Name").Value,
                               Url = item.Attribute("Url").Value,
                           }).ToList();
            strxjqh = xjqhUrl[0].Url;
            var RegionName = (from item in xele.Element("DataServices").Elements("Parameter")
                              where item.Attribute("Name").Value == "行政字段"
                              select new
                              {
                                  Name = item.Attribute("Name").Value,
                                  Value = item.Attribute("Value").Value,
                              }).ToList();
            strregionname = RegionName[0].Value;

            Data_GraLayer = new GraphicsLayer();
            mainmap.Layers.Add(Data_GraLayer);

            clsrangequery = new clsRangeQuery();
            clsrangequery.RangeQueryEvent += new RangeQueryDelegate(clsrangequery_RangeQueryEvent);
            clsrangequery.RangeQueryFaildEvent += new RangeQueryDelegate(clsrangequery_RangeQueryFaildEvent);

            //创建行政区划
            CreateRegionData();
        }

        /// <summary>
        /// 获取行政区域数据
        /// </summary>
        void CreateRegionData()
        {
            QueryTask querytask = new QueryTask(strxjqh);
            querytask.ExecuteCompleted -= querytask_ExecuteCompleted;
            querytask.ExecuteCompleted += new EventHandler<QueryEventArgs>(querytask_ExecuteCompleted);
            querytask.Failed -= querytask_Failed;
            querytask.Failed += new EventHandler<TaskFailedEventArgs>(querytask_Failed);
            Query query = new Query();
            query.Where = "1=1";
            query.ReturnGeometry = true;
            querytask.ExecuteAsync(query, "City");
        }

        void querytask_Failed(object sender, TaskFailedEventArgs e)
        {
            Message.Show("行政区划没有获取成功");
            waitanimationwindow.Close();
        }

        void querytask_ExecuteCompleted(object sender, QueryEventArgs e)
        {
            if (e.UserState.ToString() == "City")
            {
                FeatureSet featureset = e.FeatureSet;
                Dict_City = new Dictionary<string, Graphic>();
                foreach (Graphic gra in featureset)
                {
                    Dict_City.Add(gra.Attributes[strregionname].ToString(), gra);
                }

                QueryTask querytask = new QueryTask(strxzqh);
                querytask.ExecuteCompleted -= querytask_ExecuteCompleted;
                querytask.ExecuteCompleted += new EventHandler<QueryEventArgs>(querytask_ExecuteCompleted);
                querytask.Failed -= querytask_Failed;
                querytask.Failed += new EventHandler<TaskFailedEventArgs>(querytask_Failed);
                Query query = new Query();
                query.Where = "1=1";
                query.OutFields.Add("*");
                query.ReturnGeometry = true;
                querytask.ExecuteAsync(query, "Town");
            }
            else if (e.UserState.ToString() == "Town")
            {
                Dict_CityTown = new Dictionary<string, List<string>>();
                Dict_Town = new Dictionary<string, Graphic>();
                FeatureSet featureset = e.FeatureSet;
                for (int i = 0; i < featureset.Features.Count; i++)
                {
                    Dict_Town.Add(featureset.Features[i].Attributes["VNAME"].ToString(), featureset.Features[i]);
                    if (!Dict_CityTown.Keys.Contains(featureset.Features[i].Attributes["CNAME"].ToString()))
                    {
                        List<string> lsttmp = new List<string>();
                        lsttmp.Add(featureset.Features[i].Attributes["VNAME"].ToString());
                        Dict_CityTown.Add(featureset.Features[i].Attributes["CNAME"].ToString(), lsttmp);
                    }
                    else
                    {
                        List<string> lsttmp = Dict_CityTown[featureset.Features[i].Attributes["CNAME"].ToString()];
                        lsttmp.Add(featureset.Features[i].Attributes["VNAME"].ToString());
                        Dict_CityTown.Remove(featureset.Features[i].Attributes["CNAME"].ToString());
                        Dict_CityTown.Add(featureset.Features[i].Attributes["CNAME"].ToString(), lsttmp);
                    }
                }
                for (int i = 0; i < Dict_City.Count; i++)
                {
                    if (!Dict_CityTown.Keys.Contains(Dict_City.Keys.ToList()[i]))
                    {
                        Dict_CityTown.Add(Dict_City.Keys.ToList()[i], null);
                    }
                }
                Dict_City.Add("全部", null);
                cbx_county.ItemsSource = Dict_City.Keys.ToList();
                cbx_county.SelectedIndex = Dict_City.Count() - 1;
            }
            waitanimationwindow.Close();
        }

        #region 窗口操作
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
        #endregion

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
            rbtn_data.IsChecked = true;
            txt_count.Text = "";
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
            rbtn_data.IsChecked = true;
            txt_count.Text = "";
        }
        #endregion

        #region 接口实现
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
                Storyboard_Show.Begin();
            }
        }

        public event IWidgetEventHander OpenEnd;

        public event PartEventHander ReInitEnd;

        public event PartEventHander ShowEnd;

        public event PartEventHander CloseEnd;

        public bool IsOpen
        {
            get { throw new NotImplementedException(); }
        }

        public PartDescriptor Descri
        {
            get { return new PartDescriptor() { Name = "AYKJ.GISStatistics" }; }
        }

        public void ReInit()
        {
            throw new NotImplementedException();
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
            PFApp.Root.Children.Remove(this);
            data_result.ItemsSource = null;
            txt_count.Text = "";
            rbtn_data.IsChecked = true;
            mainmap.Layers.Remove(Data_GraLayer);           
        }

        public event PartEventHander LinkGisPlatformEnd;

        public string LinkReturnGisPlatform(string mark, string s)
        {
            return ""; //throw new NotImplementedException();
        }

        public string LinkReturnGisPlatform(string mark, object obj1,object obj2)
        {
            return ""; //throw new NotImplementedException();
        }

        public string LinkFromGiPlatform(string oAction, string oStr, object oArr, object oCls, object[] oArrStr, object[] oArrArr, object[] oArrCls)
        {
            return "";// throw new NotImplementedException();
        }
        #endregion

        /// <summary>
        /// 数据统计
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Finish_Click(object sender, RoutedEventArgs e)
        {
            Data_GraLayer.Graphics.Clear();
            txt_count.Text = "";
            waitanimationwindow = new WaitAnimationWindow("数据统计中...");
            waitanimationwindow.Show();
            rbtn_data.IsChecked = true;
            List<Graphic> lstall = new List<Graphic>();
            for (int i = 0; i < sp_layer.Children.Count; i++)
            {
                CheckBox c = sp_layer.Children[i] as CheckBox;
                if (c.IsChecked == true)
                {
                    lstall.AddRange(Dict_Data[c.Content.ToString()]);
                }
            }

            Dict_ResultGraphic = new Dictionary<clstipwxy, Graphic>();
            //List<Graphic> lstall = (Application.Current as IApp).lstThematic;
            if (cbx_county.SelectedItem.ToString() == "全部")
            {               
                lstdata = new List<clstipwxy>();
                for (int i = 0; i < lstall.Count; i++)
                {
                    string[] arytmp = lstall[i].Attributes["StaTag"].ToString().Split('|');
                    lstdata.Add(new clstipwxy()
                    {
                        wxyid = arytmp[1],
                        wxyname = EllipsisName(arytmp[3]),
                        wxytip = arytmp[3],
                        wxytype = (Application.Current as IApp).DictThematicEnCn[arytmp[0]],
                        wxydwdm = arytmp[2]
                    });
                    Dict_ResultGraphic.Add(lstdata[lstdata.Count - 1], lstall[i]);
                }
                data_result.ItemsSource = null;
                data_result.ItemsSource = lstdata;
                txt_count.Text = "数据总量:" + lstdata .Count()+ "个";
                waitanimationwindow.Close();
            }
            else
            {
                if (cbx_town.SelectedItem.ToString() == "全部")
                {
                    clsrangequery.RangeQuery(strGeometryurl, Dict_City[cbx_county.SelectedItem.ToString()].Geometry, lstall);
                }
                else
                {
                    clsrangequery.RangeQuery(strGeometryurl, Dict_Town[cbx_town.SelectedItem.ToString()].Geometry, lstall);
                }
            }
        }

        /// <summary>
        /// 数据整理成功
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clsrangequery_RangeQueryEvent(object sender, EventArgs e)
        {
            List<Graphic> lstreturngra = (sender as clsRangeQuery).lstReturnGraphic;
            if (lstreturngra.Count == 0)
                Message.Show("该区域没有合适的数据");
            lstdata = new List<clstipwxy>();
            for (int i = 0; i < lstreturngra.Count; i++)
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
            txt_count.Text = "数据总量:" + lstdata.Count() + "个";
            waitanimationwindow.Close();
        }

        /// <summary>
        /// 数据整理失败
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clsrangequery_RangeQueryFaildEvent(object sender, EventArgs e)
        {
            Message.Show("数据没有获取成功");
            waitanimationwindow.Close();
        }
        
        /// <summary>
        /// 定位
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
        /// Tip展示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// 文字名缩减
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        string EllipsisName(string str)
        {
            string tmpstr = string.Empty;
            int length = Encoding.Unicode.GetByteCount(str);
            if (length < 24)
            {
                tmpstr = str;
            }
            else
            {
                tmpstr = str.Substring(0, 10) + "...";
            }
            return tmpstr;
        }

        /// <summary>
        /// 数据图表切换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbtn_Checked(object sender, RoutedEventArgs e)
        {
            if (grid_data != null)
                grid_data.Visibility = System.Windows.Visibility.Collapsed;
            if (grid_chart != null)
                grid_chart.Visibility = System.Windows.Visibility.Collapsed;
            switch ((sender as RadioButton).Name)
            {
                case "rbtn_data":
                    grid_data.Visibility = System.Windows.Visibility.Visible;
                    break;
                case "rbtn_chart":
                    grid_chart.Visibility = System.Windows.Visibility.Visible;
                    if (data_result.ItemsSource != null)
                    {
                        Dictionary<string, double> Dict_Count = new Dictionary<string, double>();
                        for (int i = 0; i < Dict_ResultGraphic.Count; i++)
                        {
                            string strtypr = Dict_ResultGraphic.Keys.ToArray()[i].wxytype;
                            if (!Dict_Count.Keys.Contains(strtypr))
                            {
                                Dict_Count.Add(strtypr, 1);
                            }
                            else
                            {
                                double db = Dict_Count[strtypr];
                                db = db + 1;
                                Dict_Count.Remove(strtypr);
                                Dict_Count.Add(strtypr, db);
                            }
                            CreateChart("柱图", Dict_Count);
                        }
                    }
                    break;
                case "rbtn_chartpie":
                    grid_chart.Visibility = System.Windows.Visibility.Visible;
                    if (data_result.ItemsSource != null)
                    {
                        Dictionary<string, double> Dict_Count = new Dictionary<string, double>();
                        for (int i = 0; i < Dict_ResultGraphic.Count; i++)
                        {
                            string strtypr = Dict_ResultGraphic.Keys.ToArray()[i].wxytype;
                            if (!Dict_Count.Keys.Contains(strtypr))
                            {
                                Dict_Count.Add(strtypr, 1);
                            }
                            else
                            {
                                double db = Dict_Count[strtypr];
                                db = db + 1;
                                Dict_Count.Remove(strtypr);
                                Dict_Count.Add(strtypr, db);
                            }
                            CreateChart("饼图", Dict_Count);
                        }
                    }
                    break;
            }
        }

        void CreateChart(string temp_chart, Dictionary<string, double> temp_Dictionary)
        {
            grid_chart.Children.Clear();
            RenderAs render = new RenderAs();
            switch (temp_chart)
            {
                case "饼图":
                    render = RenderAs.Pie;
                    break;
                case "柱图":
                    render = RenderAs.Column;
                    break;
            }
            DrawChart(render, temp_Dictionary);
        }

        void DrawChart(RenderAs temp_renderas, Dictionary<string, double> temp_Dictionary)
        {
            Chart mychart = new Chart();
            mychart.BorderThickness = new Thickness(0);
            mychart.Background = new SolidColorBrush(Color.FromArgb(0, 250, 250, 250));
            mychart.ToolTipEnabled = true;
            Axis xaxis = new Axis();
            xaxis.Enabled = false;
            mychart.AxesX.Add(xaxis);
            Axis yaxis = new Axis();
            yaxis.Enabled = false;
            mychart.AxesY.Add(yaxis);
            ChartGrid xgrid = new ChartGrid();
            xgrid.Enabled = false;
            xaxis.Grids.Add(xgrid);
            ChartGrid ygrid = new ChartGrid();
            ygrid.Enabled = false;
            yaxis.Grids.Add(ygrid);
            mychart.ScrollingEnabled = false;
            mychart.View3D = true;
            mychart.AnimatedUpdate = true;
            DataSeries ds = new DataSeries();
            ds.RenderAs = temp_renderas;

            Legend legend = new Legend();
            legend.Background = new SolidColorBrush(Color.FromArgb(0, 0, 250, 250));
            legend.FontColor = new SolidColorBrush(Colors.White);
            legend.FontFamily = new System.Windows.Media.FontFamily("NSimSun");
            mychart.Legends.Add(legend);
            
            for (Int32 i = 0; i < temp_Dictionary.Count; i++)
            {
                DataPoint dp = new DataPoint();
                dp.AxisXLabel = temp_Dictionary.Keys.ToList()[i];
                dp.ShowInLegend = false;                
                dp.LegendText = temp_Dictionary.Keys.ToList()[i] + ":" + temp_Dictionary.Values.ToList()[i];
                dp.LabelText = "#Percentage%";
                dp.YValue = temp_Dictionary.Values.ToList()[i];
                ds.DataPoints.Add(dp);
            }
            ds.MarkerColor = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
            ds.LabelFontColor = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
            mychart.Series.Add(ds);
            grid_chart.Children.Add(mychart);
        }

        /// <summary>
        /// 根据区县，切换乡镇
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbx_county_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            data_result.ItemsSource = null;
            txt_count.Text = "";
            rbtn_data.IsChecked = true;
            Data_GraLayer.Graphics.Clear();
            if (cbx_county.SelectedItem == null)
                return;
            if (cbx_county.SelectedItem.ToString() != "全部")
            {
                if (Dict_CityTown[cbx_county.SelectedItem.ToString()] != null)
                {
                    List<string> lst = Dict_CityTown[cbx_county.SelectedItem.ToString()];
                    lst.Insert(0, "全部");
                    cbx_town.ItemsSource = lst;
                    cbx_town.SelectedIndex = 0;
                }
                else
                {
                    List<string> lst = new List<string>();
                    lst.Add("全部");
                    cbx_town.ItemsSource = lst;
                    cbx_town.SelectedIndex = 0;
                }
                mainmap.ZoomTo(Dict_City[cbx_county.SelectedItem.ToString()].Geometry);
            }
            else
            {
                List<string> lst = new List<string>();
                lst.Add("全部");
                cbx_town.ItemsSource = lst;
                cbx_town.SelectedIndex = 0;

                if (PFApp.Root.Children.Contains(this))
                {
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
            }
        }

        /// <summary>
        /// 根据乡镇，切换地图
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbx_town_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            data_result.ItemsSource = null;
            txt_count.Text = "";
            rbtn_data.IsChecked = true;
            Data_GraLayer.Graphics.Clear();
            if (cbx_town.SelectedItem == null)
                return;
            if (cbx_town.SelectedItem.ToString() == "全部")
            {
                if (cbx_town.SelectedItem.ToString() != "全部")
                {
                    mainmap.ZoomTo(Dict_City[cbx_county.SelectedItem.ToString()].Geometry);
                }
                else
                {
                    if (PFApp.Root.Children.Contains(this))
                    {
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
                }
            }
            else
            {
                mainmap.ZoomTo(Dict_Town[cbx_town.SelectedItem.ToString()].Geometry);
            }
        }
    }

    public class clstipwxy
    {
        public string wxyid { set; get; }
        public string wxyname { set; get; }
        public string wxytip { set; get; }
        public string wxytype { set; get; }
        public string wxydwdm { set; get; }
    }
}
