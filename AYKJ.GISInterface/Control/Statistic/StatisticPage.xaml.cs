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
using ESRI.ArcGIS.Client.Tasks;
using System.Xml.Linq;
using AYKJ.GISDevelop.Platform;
using Visifire.Charts;
using ESRI.ArcGIS.Client.Symbols;
using System.Windows.Markup;
using AYKJ.GISDevelop.Platform.ToolKit;
using ESRI.ArcGIS.Client.Geometry;

namespace AYKJ.GISInterface
{
    public partial class StatisticPage : UserControl
    {
        //行政区地址
        string strXzqurl;
        //行政区名字段
        string strregionname;
        //空间服务
        string strGeometryurl;
        //空间查询服务
        clsRangeQuery clsrangequery;
        //地图
        Map StatisticMap;
        //所有的专题数据
        Dictionary<string, GraphicsLayer> Dict_Data;
        //专题数据的中英文对照
        Dictionary<string, string> DictEnCn;
        //所有行政区域数据
        List<Graphic> lst_Region;
        //行政区名字典
        Dictionary<string, Graphic> Dict_Region;
        //选择后要查询的专题数据
        List<Graphic> lst_data;
        //查询区域的次数
        int intregion;
        //统计数据
        Dictionary<string, Dictionary<string, List<Graphic>>> Dict_Statistic;
        //加载图表的数据层
        GraphicsLayer ChartLayer;
        //图表样式
        string strcharttype;
        //动画
        WaitAnimationWindow waitanimationwindow;
        //图例
        Dictionary<string, Brush> Dict_Color;

        public StatisticPage()
        {
            InitializeComponent();
        }

        public void InitStatistic(string strtype, string strlayer)
        {
            waitanimationwindow = new WaitAnimationWindow("行政区划数据获取中，请稍候...");
            waitanimationwindow.Show();
            XElement xele = PFApp.Extent;
            strGeometryurl = (from item in PFApp.Extent.Elements("GeometryService")
                              select item.Attribute("Url").Value).ToArray()[0];

            var XzqUrl = (from item in xele.Element("ExtensionServices").Elements("ExtensionService")
                          where item.Attribute("Name").Value == "行政区域"
                          select new
                          {
                              Name = item.Attribute("Name").Value,
                              Url = item.Attribute("Url").Value,
                          }).ToList();
            strXzqurl = XzqUrl[0].Url;

            var RegionName = (from item in xele.Element("DataServices").Elements("Parameter")
                              where item.Attribute("Name").Value == "行政字段"
                              select new
                              {
                                  Name = item.Attribute("Name").Value,
                                  Value = item.Attribute("Value").Value,
                              }).ToList();
            strregionname = RegionName[0].Value;

            StatisticMap = (Application.Current as IApp).MainMap;
            Dict_Data = (Application.Current as IApp).Dict_ThematicLayer;
            DictEnCn = (Application.Current as IApp).DictThematicEnCn;

            ChartLayer = new GraphicsLayer();
            StatisticMap.Layers.Add(ChartLayer);

            clsrangequery = new clsRangeQuery();
            clsrangequery.RangeQueryEvent += new RangeQueryDelegate(clsrangequery_RangeQueryEvent);

            GetRegionData();
            sp.Visibility = System.Windows.Visibility.Collapsed;

            strcharttype = strtype;

            ChartLayer.Graphics.Clear();
            intregion = 0;
            Dict_Statistic = new Dictionary<string, Dictionary<string, List<Graphic>>>();
            lst_data = new List<Graphic>();
            LayerData(strlayer);
            Storyboard_Close.Completed += new EventHandler(Storyboard_Close_Completed);
        }

        #region 获取数据
        void LayerData(string strlayer)
        {
            string[] arylayer = strlayer.Split('|');
            for (int i = 0; i < Dict_Data.Count; i++)
            {
                if (arylayer.Contains(Dict_Data.Keys.ToList()[i]))
                {
                    lst_data.AddRange(Dict_Data.Values.ToList()[i].Graphics);
                }
            }
        }

        /// <summary>
        /// 获取行政区域数据
        /// </summary>
        void GetRegionData()
        {
            Dict_Region = new Dictionary<string, Graphic>();
            QueryTask querytask = new QueryTask(strXzqurl);
            querytask.ExecuteCompleted -= querytask_ExecuteCompleted;
            querytask.ExecuteCompleted += new EventHandler<QueryEventArgs>(querytask_ExecuteCompleted);
            querytask.Failed -= querytask_Failed;
            querytask.Failed += new EventHandler<TaskFailedEventArgs>(querytask_Failed);
            Query query = new Query();
            query.Where = "1=1";
            query.ReturnGeometry = true;
            querytask.ExecuteAsync(query);
        }

        void querytask_Failed(object sender, TaskFailedEventArgs e)
        {
            Message.Show("行政区划获取失败");
        }

        void querytask_ExecuteCompleted(object sender, QueryEventArgs e)
        {
            lst_Region = e.FeatureSet.Features.ToList();
            for (int i = 0; i < lst_Region.Count(); i++)
            {
                Dict_Region.Add(lst_Region[i].Attributes[strregionname].ToString(), lst_Region[i]);
            }

            waitanimationwindow.Change("专题图生成中，请稍候...");

            if (lst_Region != null)
            {
                clsrangequery.RangeQuery(strGeometryurl, lst_Region[intregion].Geometry, lst_data);
            }
            else
            {
                Message.Show("行政区划没有获取");
            }
            if (PFApp.Root.Children.Count > 0)
            {
                for (int i = 0; i < PFApp.Root.Children.Count; i++)
                {
                    if (PFApp.Root.Children[i].GetType().Name == "StatisticPage")
                    {
                        PFApp.Root.Children.RemoveAt(i);
                    }
                }
            }
            PFApp.Root.Children.Add(this);
            
        }
        #endregion

        /// <summary>
        /// 获取该有的数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clsrangequery_RangeQueryEvent(object sender, EventArgs e)
        {
            List<Graphic> lsttmp = clsrangequery.lstReturnGraphic;
            Dictionary<string, List<Graphic>> Dict_tmp = new Dictionary<string, List<Graphic>>();
            for (int i = 0; i < lsttmp.Count; i++)
            {
                string strtype = DictEnCn[lsttmp[i].Attributes["StaTag"].ToString().Split('|')[0]];
                if (!Dict_tmp.Keys.ToList().Contains(strtype))
                {
                    List<Graphic> lst = new List<Graphic>();
                    lst.Add(lsttmp[i]);
                    Dict_tmp.Add(strtype, lst);
                }
                else
                {
                    List<Graphic> lst = Dict_tmp[strtype];
                    Dict_tmp.Remove(strtype);
                    lst.Add(lsttmp[i]);
                    Dict_tmp.Add(strtype, lst);
                }
            }
            Dict_Statistic.Add(lst_Region[intregion].Attributes[strregionname].ToString(), Dict_tmp);
            intregion = intregion + 1;
            if (intregion < lst_Region.Count())
            {
                clsrangequery.RangeQuery(strGeometryurl, lst_Region[intregion].Geometry, lst_data);
            }
            else
            {
                CreateChart();
                waitanimationwindow.Close();

                XElement xele = PFApp.Extent;
                Envelope eve = new Envelope()
                {
                    XMax = double.Parse(xele.Element("MapExtent").Attribute("XMax").Value),
                    XMin = double.Parse(xele.Element("MapExtent").Attribute("XMin").Value),
                    YMax = double.Parse(xele.Element("MapExtent").Attribute("YMax").Value),
                    YMin = double.Parse(xele.Element("MapExtent").Attribute("YMin").Value),
                    SpatialReference = StatisticMap.SpatialReference
                };
                StatisticMap.ZoomTo(eve);
                Storyboard_Show.Begin();
            }
        }

        /// <summary>
        /// 创建图表
        /// </summary>
        void CreateChart()
        {
            GetLegendColor();
            for (int i = 0; i < Dict_Statistic.Count; i++)
            {
                if (Dict_Statistic.Values.ToArray()[i].Count == 0)
                    continue;
                string strStatistic = string.Empty;
                if (strcharttype == "柱状图")
                {
                    strStatistic = CreateColumnTemplate(Dict_Statistic.Values.ToList()[i]);
                }
                else if (strcharttype == "饼状图")
                {
                    strStatistic = CreatePieTemplate(Dict_Statistic.Values.ToList()[i]);
                }
                var StatisticTemplate = (ControlTemplate)XamlReader.Load(strStatistic);
                var StatisticMs = new MarkerSymbol
                {
                    ControlTemplate = StatisticTemplate
                };
                Graphic tmpgra = new Graphic();
                tmpgra.Geometry = Dict_Region[Dict_Statistic.Keys.ToList()[i]].Geometry.Extent.GetCenter();
                if (strcharttype == "柱状图")
                {
                    StatisticMs.OffsetX = 75;
                    StatisticMs.OffsetY = 60;
                }
                else if (strcharttype == "饼状图")
                {
                    StatisticMs.OffsetX = 180;
                    StatisticMs.OffsetY = 150;
                }
                tmpgra.Symbol = StatisticMs;
                ChartLayer.Graphics.Add(tmpgra);
            }
        }

        /// <summary>
        /// 生成柱状图样式模版
        /// </summary>
        /// <param name="Dict_tmp"></param>
        /// <param name="ratmp"></param>
        /// <returns></returns>
        string CreateColumnTemplate(Dictionary<string, List<Graphic>> Dict_tmp)
        {
            string strIncome = "<ControlTemplate xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">"
                                              + "<Canvas >"
                                              + "<vc:Chart xmlns:vc=\"clr-namespace:Visifire.Charts;assembly=SLVisifire.Charts\" ColorSet=\"Visifire1\" "
                                              + "Width=\"150\" Height=\"120\" ToolTipEnabled=\"true\" BorderThickness=\"0\" View3D=\"True\" DataPointWidth =\"13\" Watermark=\"False\" >"
                                              + ""
                                              + "<vc:Chart.AxesY >"
                                              + "<vc:Axis Enabled=\"true\" >"
                                              + "<vc:Axis.AxisLabels>"
                                              + "<vc:AxisLabels Enabled=\"true\"/>"
                                              + "</vc:Axis.AxisLabels>"
                                              + "<vc:Axis.Grids>"
                                              + "<vc:ChartGrid Enabled=\"false\"/>"
                                              + "</vc:Axis.Grids>"
                                              + "</vc:Axis>"
                                              + "</vc:Chart.AxesY>"
                                              + "<vc:Chart.AxesX>"
                                              + "<vc:Axis Enabled=\"true\" >"
                                              + "<vc:Axis.AxisLabels>"
                                              + "<vc:AxisLabels Enabled=\"true\"/>"
                                              + "</vc:Axis.AxisLabels>"
                                              + "<vc:Axis.Grids>"
                                              + "<vc:ChartGrid Enabled=\"false\"/>"
                                              + "</vc:Axis.Grids>"
                                              + "</vc:Axis>"
                                              + "</vc:Chart.AxesX>";
            strIncome = strIncome + "<vc:Chart.Series><vc:DataSeries RenderAs=\"Column\" >";
            strIncome = strIncome + "<vc:DataSeries.DataPoints>";
            for (int i = 0; i < Dict_tmp.Count(); i++)
            {
                strIncome = strIncome + "<vc:DataPoint YValue=\"";
                strIncome = strIncome + Dict_tmp.Values.ToList()[i].Count() + "\"";
                strIncome = strIncome + " AxisXLabel=\"";
                strIncome = strIncome + Dict_tmp.Keys.ToArray()[i] + "\" FontFamily=\"NSimSun\" FontSize=\"12\"";
                //strIncome = strIncome + " Color=\"" + Dict_Color[Dict_tmp.Keys.ToList()[i]] + "\"/>";
                strIncome = strIncome + " Color=\"" + (Dict_Color[Dict_tmp.Keys.ToList()[i]] as SolidColorBrush).Color.ToString() + "\"/>";
            }
            strIncome = strIncome + "</vc:DataSeries.DataPoints>"
                               + "</vc:DataSeries></vc:Chart.Series>"
                               + "</vc:Chart>"
                               + "</Canvas>"
                               + "</ControlTemplate>";
            return strIncome;
        }

        /// <summary>
        /// 生成饼状图样式模版
        /// </summary>
        /// <param name="Dict_tmp"></param>
        /// <param name="ratmp"></param>
        /// <returns></returns>
        string CreatePieTemplate(Dictionary<string, List<Graphic>> Dict_tmp)
        {
            string strIncome = "<ControlTemplate xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">"
                                              + "<Canvas >"
                                              + "<vc:Chart xmlns:vc=\"clr-namespace:Visifire.Charts;assembly=SLVisifire.Charts\"  FontFamily=\"NSimSun\" FontSize=\"16\" "
                                              + "Width=\"300\" Height=\"300\" ToolTipEnabled=\"true\" BorderThickness=\"0\" View3D=\"True\" DataPointWidth =\"13\" Watermark=\"False\" >"
                                              + ""
                                              + "<vc:Chart.AxesY >"
                                              + "<vc:Axis Enabled=\"true\" >"
                                              + "<vc:Axis.AxisLabels>"
                                              + "<vc:AxisLabels Enabled=\"true\"/>"
                                              + "</vc:Axis.AxisLabels>"
                                              + "<vc:Axis.Grids>"
                                              + "<vc:ChartGrid Enabled=\"false\"/>"
                                              + "</vc:Axis.Grids>"
                                              + "</vc:Axis>"
                                              + "</vc:Chart.AxesY>"
                                              + "<vc:Chart.AxesX>"
                                              + "<vc:Axis Enabled=\"true\" >"
                                              + "<vc:Axis.AxisLabels>"
                                              + "<vc:AxisLabels Enabled=\"true\" />"
                                              + "</vc:Axis.AxisLabels>"
                                              + "<vc:Axis.Grids>"
                                              + "<vc:ChartGrid Enabled=\"false\"/>"
                                              + "</vc:Axis.Grids>"
                                              + "</vc:Axis>"
                                              + "</vc:Chart.AxesX>";
            strIncome = strIncome + "<vc:Chart.Series><vc:DataSeries RenderAs=\"Pie\" >";
            strIncome = strIncome + "<vc:DataSeries.DataPoints>";
            for (int i = 0; i < Dict_tmp.Count(); i++)
            {
                strIncome = strIncome + "<vc:DataPoint YValue=\"";
                strIncome = strIncome + Dict_tmp.Values.ToList()[i].Count() + "\"";
                strIncome = strIncome + " AxisXLabel=\"";
                strIncome = strIncome + Dict_tmp.Keys.ToArray()[i] + "\" FontFamily=\"NSimSun\" FontSize=\"12\"";
                //strIncome = strIncome + " Color=\"" + Dict_Color[Dict_tmp.Keys.ToList()[i]] + "\"/>";
                strIncome = strIncome + " Color=\"" + (Dict_Color[Dict_tmp.Keys.ToList()[i]] as SolidColorBrush).Color.ToString() + "\"/>";
            }
            strIncome = strIncome + "</vc:DataSeries.DataPoints>"
                               + "</vc:DataSeries></vc:Chart.Series>"
                               + "</vc:Chart>"
                               + "</Canvas>"
                               + "</ControlTemplate>";
            return strIncome;
        }

        #region 两侧面板的展示和关闭
        /// <summary>
        /// 量测面板展开
        /// </summary>
        public void Show()
        {
            //PFApp.Root.Children.Add(this);
        }
        /// <summary>
        /// 面板关闭方法
        /// </summary>
        public void Close()
        {
            Storyboard_Close.Begin();
        }

        void Storyboard_Close_Completed(object sender, EventArgs e)
        {
            intregion = 0;
            Dict_Statistic = new Dictionary<string, Dictionary<string, List<Graphic>>>();
            lst_data = new List<Graphic>();
            StatisticMap.Layers.Remove(ChartLayer);
            PFApp.Root.Children.Remove(this);
        }

        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        #endregion

        /// <summary>
        /// 获取专题图颜色
        /// </summary>
        /// <returns></returns>
        Dictionary<string, Brush> GetLegendColor()
        {
            Dict_Color = new Dictionary<string, Brush>();
            Dictionary<string, GraphicsLayer> Dict = (Application.Current as IApp).Dict_ThematicLayer;
            int n = Dict.Count / 15;
            int m = Dict.Count % 15;
            ColorSets emcs = new ColorSets();
            string resourceName = "Visifire.Charts.ColorSets.xaml";  // Visifire 默认颜色集合的文件
            using (System.IO.Stream s = typeof(Chart).Assembly.GetManifestResourceStream(resourceName))
            {
                if (s != null)
                {
                    System.IO.StreamReader reader = new System.IO.StreamReader(s);
                    String xaml = reader.ReadToEnd();
                    emcs = System.Windows.Markup.XamlReader.Load(xaml) as ColorSets;
                    reader.Close();
                    s.Close();
                }
            }
            for (int i = 0; i < n; i++)
            {
                ColorSet cs = emcs[i];
                for (int k = 0; k < 15; k++)
                {
                    Dict_Color.Add(Dict.Keys.ToList()[k + i * 15], cs.Brushes[k]);
                }
            }

            ColorSet tmp = emcs[n];
            for (int i = 0; i < m; i++)
            {
                Dict_Color.Add(Dict.Keys.ToList()[i + n * 15], tmp.Brushes[i]);
            }
            CreateLegend();
            return Dict_Color;
        }

        /// <summary>
        /// 生成专题图图例
        /// </summary>
        void CreateLegend()
        {
            if (sp.Children.Count > 1)
                sp.Children.RemoveAt(1);
            StackPanel sp_all = new StackPanel() { Orientation = Orientation.Horizontal };
            int m = Dict_Color.Count / 5;
            int n = Dict_Color.Count % 5;
            for (int i = 0; i < m; i++)
            {
                StackPanel sp_tmp = new StackPanel()
                {
                    Orientation = Orientation.Vertical,
                    Margin = new Thickness(5, 0, 0, 0)
                };
                for (int k = 0; k < 5; k++)
                {
                    StackPanel sp_hor = new StackPanel()
                    {
                        Orientation = Orientation.Horizontal,
                        Margin = new Thickness(5, 5, 0, 0)
                    };
                    Border border = new Border()
                    {
                        Height = 20,
                        Width = 40,
                        Background = Dict_Color.Values.ToList()[k + i * 5]
                    };
                    TextBlock txt = new TextBlock()
                    {
                        VerticalAlignment = System.Windows.VerticalAlignment.Center,
                        Text = Dict_Color.Keys.ToList()[k + i * 5],
                        Margin = new Thickness(10, 0, 0, 0),
                        FontFamily = new FontFamily("NSimSun"),
                        FontSize = 12
                    };
                    sp_hor.Children.Add(border);
                    sp_hor.Children.Add(txt);
                    sp_tmp.Children.Add(sp_hor);
                }
                sp_all.Children.Add(sp_tmp);
            }

            StackPanel sp_y = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(5, 0, 0, 0)
            };
            for (int i = 0; i < n; i++)
            {
                StackPanel sp_hor = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(5, 5, 0, 0)
                };
                Border border = new Border()
                {
                    Height = 20,
                    Width = 40,
                    Background = Dict_Color.Values.ToList()[i + m * 5]
                };
                TextBlock txt = new TextBlock()
                {
                    VerticalAlignment = System.Windows.VerticalAlignment.Center,
                    Text = Dict_Color.Keys.ToList()[i + m * 5],
                    Margin = new Thickness(10, 0, 0, 0),
                    FontFamily = new FontFamily("NSimSun"),
                    FontSize = 12
                };
                sp_hor.Children.Add(border);
                sp_hor.Children.Add(txt);
                sp_y.Children.Add(sp_hor);
            }
            sp_all.Children.Add(sp_y);
            sp.Children.Add(sp_all);
            sp.Visibility = System.Windows.Visibility.Visible;
        }
    }
}
