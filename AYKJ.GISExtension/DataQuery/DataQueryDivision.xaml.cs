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
using AYKJ.GISDevelop.Platform.ToolKit;
using System.Xml.Linq;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client.Geometry;
using System.Text;

namespace AYKJ.GISExtension
{
    public partial class DataQueryDivision : UserControl
    {
        //定义一个空间查询服务的地址
        string strGeometryurl;
        //定义行政区划中文字段
        string strregionname;
        //定义市域级服务地址
        string strsyurl;
        //定义区县级服务地址
        string strqxurl;
        //定义一个Map用来接收平台的Map
        public static Map mainmap;
        //一次查询定位
        public static GraphicsLayer Data_GraLayer;
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
        //市域级名称列表
        Dictionary<string, string> Dict_sy;
        Dictionary<string, Graphic> Dict_sygra;
        //区县级名称列表
        Dictionary<string, string> Dict_qx;
        Dictionary<string, Graphic> Dict_qxgra;
        //市域与区县挂钩
        Dictionary<string, List<string>> Dict_syqx;

        public DataQueryDivision()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(DataQueryDivision_Loaded);
        }

        void DataQueryDivision_Loaded(object sender, RoutedEventArgs e)
        {
            mainmap = (Application.Current as IApp).MainMap;
            XElement xele = PFApp.Extent;
            strGeometryurl = (from item in PFApp.Extent.Elements("GeometryService")
                              select item.Attribute("Url").Value).ToArray()[0];
            cbx_sy.SelectionChanged -= cbx_sy_SelectionChanged;
            cbx_sy.SelectionChanged += new SelectionChangedEventHandler(cbx_sy_SelectionChanged);
            cbx_qx.SelectionChanged -= cbx_qx_SelectionChanged;
            cbx_qx.SelectionChanged += new SelectionChangedEventHandler(cbx_qx_SelectionChanged);
            clsrangequery = new clsRangeQuery();
            clsrangequery.RangeQueryEvent -= clsrangequery_RangeQueryEvent;
            clsrangequery.RangeQueryEvent += new RangeQueryDelegate(clsrangequery_RangeQueryEvent);
            clsrangequery.RangeQueryFaildEvent -= clsrangequery_RangeQueryFaildEvent;
            clsrangequery.RangeQueryFaildEvent += new RangeQueryDelegate(clsrangequery_RangeQueryFaildEvent);
            Data_GraLayer = new GraphicsLayer();
            mainmap.Layers.Add(Data_GraLayer);
            Dict_ResultGraphic = new Dictionary<clstipwxy, Graphic>();
            Dict_sy = new Dictionary<string, string>();
            Dict_sygra = new Dictionary<string, Graphic>();
            Dict_qx = new Dictionary<string, string>();
            Dict_qxgra = new Dictionary<string, Graphic>();
            Dict_syqx = new Dictionary<string, List<string>>();
            Dict_Data = (Application.Current as IApp).Dict_ThematicLayer;
            lstdata = new List<clstipwxy>();
            CreateLayer();
            //行政区域信息赋值
            Dict_sy = (Application.Current as IApp).Dict_Xzqz_sy;
            Dict_sygra = (Application.Current as IApp).Dict_Xzqz_sygra;
            Dict_qx = (Application.Current as IApp).Dict_Xzqz_qx;
            Dict_qxgra = (Application.Current as IApp).Dict_Xzqz_qxgra;
            QYQX();
      
        }

        #region 行政区划
        /// <summary>
        /// 市域级与区县级挂钩
        /// </summary>
        void QYQX()
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
            cbx_sy.ItemsSource = Dict_syqx.Keys.ToList();
            cbx_sy.SelectedIndex = 0;
        }

        /// <summary>
        /// 市域级选择定位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void cbx_sy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbx_sy.SelectedValue == null)
                return;
            List<string> lst = Dict_syqx[cbx_sy.SelectedValue.ToString()];
            if (cbx_sy.SelectedValue.ToString() != "全部")
            {
                mainmap.ZoomTo(Dict_sygra[cbx_sy.SelectedValue.ToString()].Geometry);
            }
            else
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
                QueryData();
            }
            lst.Insert(0, "全部");
            cbx_qx.ItemsSource = lst;
            cbx_qx.SelectedIndex = 0;
        }

        /// <summary>
        /// 区县级选择定位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void cbx_qx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbx_sy.SelectedValue.ToString() != "全部" && cbx_qx.SelectedValue != null)
            {
                if (cbx_qx.SelectedValue.ToString() != "全部")
                {
                    mainmap.ZoomTo(Dict_qxgra[cbx_qx.SelectedValue.ToString()].Geometry);
                }
                else
                {
                    mainmap.ZoomTo(Dict_sygra[cbx_sy.SelectedValue.ToString()].Geometry);
                }
                QueryData();
            }
        }

        #endregion

        /// <summary>
        /// 清除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_clear_Click(object sender, RoutedEventArgs e)
        {
            Data_GraLayer.Graphics.Clear();
            data_result.ItemsSource = null;
            cbx_sy.SelectedIndex = 0;
            cbx_qx.SelectedIndex = 0;
        }

        /// <summary>
        /// 根据市域查询数据
        /// </summary>
        void QueryData()
        {
            Data_GraLayer.Graphics.Clear();
            waitanimationwindow = new WaitAnimationWindow("数据查询中....");
            waitanimationwindow.Show();
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

            if (cbx_sy.SelectedValue == null || lst.Count == 0)
            {
                waitanimationwindow.Close();
                return;
            }

            if (cbx_sy.SelectedValue.ToString() == "全部")
            {
                GetResultData(lst);
                waitanimationwindow.Close();
            }
            else
            {
                if (cbx_qx.SelectedValue != null)
                {
                    if (cbx_qx.SelectedValue.ToString() == "全部")
                    {
                        clsrangequery.RangeQuery(strGeometryurl, Dict_sygra[cbx_sy.SelectedValue.ToString()].Geometry, lst);
                    }
                    else
                    {
                        clsrangequery.RangeQuery(strGeometryurl, Dict_qxgra[cbx_qx.SelectedValue.ToString()].Geometry, lst);
                    }
                }
                else
                {
                    waitanimationwindow.Close();
                }
            }
        }

        /// <summary>
        /// 查询失败
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clsrangequery_RangeQueryFaildEvent(object sender, EventArgs e)
        {
            waitanimationwindow.Close();
            Message.Show("查询出错");
        }

        /// <summary>
        /// 查询成功
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clsrangequery_RangeQueryEvent(object sender, EventArgs e)
        {
            List<Graphic> lstreturngra = (sender as clsRangeQuery).lstReturnGraphic;
            GetResultData(lstreturngra);
            waitanimationwindow.Close();
        }

        /// <summary>
        /// 生成表格
        /// </summary>
        /// <param name="lstreturngra"></param>
        void GetResultData(List<Graphic> lstreturngra)
        {
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

        #region 图层
        /// <summary>
        /// 创建查询图层
        /// </summary>
        void CreateLayer()
        {
            for (int i = 0; i < Dict_Data.Count(); i++)
            {
                CheckBox ckb = new CheckBox();
                ckb.Content = Dict_Data.Keys.ToList()[i];
                ckb.Checked += new RoutedEventHandler(ckb_Checked);
                ckb.Unchecked += new RoutedEventHandler(ckb_Unchecked);
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

        void ckb_Unchecked(object sender, RoutedEventArgs e)
        {
            if (chkLayer.IsChecked != false)
            {
                if (cbx_sy.SelectedValue != null && cbx_qx.SelectedValue != null)
                {
                    QueryData();
                }
            }
        }

        void ckb_Checked(object sender, RoutedEventArgs e)
        {
            if (cbx_sy.SelectedValue != null && cbx_qx.SelectedValue != null)
            {
                QueryData();
            }
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
                c.Unchecked -= ckb_Unchecked;
                c.Checked -= ckb_Checked;
                c.IsChecked = true;
            }
            data_result.ItemsSource = null;
            if (cbx_sy.SelectedValue != null && cbx_qx.SelectedValue != null)
            {
                QueryData();
            }
            for (int i = 0; i < sp_layer.Children.Count; i++)
            {
                CheckBox c = sp_layer.Children[i] as CheckBox;
                c.Unchecked += new RoutedEventHandler(ckb_Unchecked);
                c.Checked += new RoutedEventHandler(ckb_Checked);
            }
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

        #region 重置信息
        public void Reset()
        {
            try
            {
                Dict_ResultGraphic = new Dictionary<clstipwxy, Graphic>();
                lstdata = new List<clstipwxy>();
                //clsrangequery = new clsRangeQuery();
                //clsrangequery.RangeQueryEvent -= clsrangequery_RangeQueryEvent;
                //mainmap.Layers.Remove(Draw_GraLayer);
                mainmap.Layers.Remove(Data_GraLayer);
                sp_layer.Children.Clear();
                data_result.ItemsSource = null;
            }
            catch (Exception)
            { }
        }
        #endregion

    }
}
