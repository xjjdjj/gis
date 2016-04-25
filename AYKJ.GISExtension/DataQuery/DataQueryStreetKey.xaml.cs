using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Json;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Linq;
using AYKJ.GISDevelop.Platform;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using System.Text;

namespace AYKJ.GISExtension
{
    public partial class DataQueryStreetKey : UserControl
    {
        //图层名称列表
        private static List<string> listLayerName;

        //图层代码列表
        private static List<string> listLayerCode;

        //可供选择的字段列表
        private static List<string> listField;

        //所有的字段列表
        private static List<string> listAllField;

        //专题数据的名称和代码对应
        private static Dictionary<string, string> Dict_EnCn;
        //专题数据（<图层，图层内所有标注>）
        private static Dictionary<string, List<Dictionary<string, string>>> dict_thematic;
        //所有的专题数据
        Dictionary<string, GraphicsLayer> Dict_Data;

        //用户选择的图层列表
        private static List<string> listSelectLayer;
        //用户选择的字段列表
        private static List<string> listSelectStreet;

        //
        public static GraphicsLayer selectHigh_GraLayer;

        //定义一个Map用来接收平台的Map
        public static Map mainmap;

        //20120918:
        //查询后属性数据
        List<clstipwxy> lstdata;
        //绑定的空间数据
        Dictionary<clstipwxy, MapPoint> Dict_ResultPoint;

        #region ESRI样式
        public SimpleMarkerSymbol DrawMarkerSymbol = new SimpleMarkerSymbol()
        {
            Color = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)),
            Size = 5,
            Style = SimpleMarkerSymbol.SimpleMarkerStyle.Circle
        };
        #endregion

        XElement xele;
        //20150327：可供选择的企业列表
        private static List<string> listEnterprise;
        private static List<string> listSelectEnterprise;


        public DataQueryStreetKey()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(DataQueryKey_Loaded);
        }

        void DataQueryKey_Loaded(object sender, RoutedEventArgs e)
        {
            mainmap = (Application.Current as IApp).MainMap;

            xele = PFApp.Extent;

            //20131008:初始化高亮图层
            if (mainmap.Layers["selectHigh_Layer_circle"] != null)
            {
                selectHigh_GraLayer = mainmap.Layers["selectHigh_Layer_circle"] as GraphicsLayer;
                selectHigh_GraLayer.Graphics.Clear();
            }
            else
            {
                selectHigh_GraLayer = new GraphicsLayer();
                selectHigh_GraLayer.ID = "selectHigh_Layer_circle";
                mainmap.Layers.Add(selectHigh_GraLayer);
            }

            listSelectStreet = new List<string>();

            CreateLayer();            
        }

        /// <summary>
        /// 创建查询图层
        /// </summary>
        void CreateLayer()
        {            
            Dict_Data = (Application.Current as IApp).Dict_ThematicLayer;
            
            CheckBox mChk = new CheckBox();

            //读取配置文件,获取街道信息
            var lf = (from item in xele.Element("JnStreet").Elements("street")
                      select new
                      {
                          position = item.Attribute("position").Value,
                          name = item.Attribute("name").Value,
                      }).ToList();
            if (lf != null)
            {
                listField = new List<string>();
                List<string> listFieldName = new List<string>();
                foreach (var item in lf)
                {
                    listField.Add(item.position);
                    listFieldName.Add(item.name);
                }

                //初始化列表
                sp_field.Children.Clear();
                for (int j = 0; j < listField.Count; j++)
                {
                    mChk = new CheckBox();
                    if ((Application.Current as IApp).iListSelectStreet == null)
                    {
                        if ((Application.Current as IApp).sysRole == listFieldName[j] || (Application.Current as IApp).sysRole == "ajj")
                        {
                            mChk.IsChecked = true;
                        }
                        else
                        {
                            mChk.IsChecked = false;
                        }
                    }
                    else
                    {
                        //根据保存的街道显示状态初始化街道列表
                        listSelectStreet = (Application.Current as IApp).iListSelectStreet;
                        mChk.IsChecked = false;

                        foreach (string s in listSelectStreet)
                        {
                            if (s == listFieldName[j])
                            {
                                mChk.IsChecked = true;
                                break;
                            }
                        }
                    }
                    mChk.Margin = new Thickness(0, 5, 5, 0);
                    mChk.Style = this.Resources["CheckBoxStyle"] as Style;
                    mChk.Foreground = new SolidColorBrush(Colors.White);
                    mChk.BorderBrush = new SolidColorBrush(Colors.White);
                    mChk.BorderBrush = null;
                    mChk.BorderThickness = new Thickness(0);

                    mChk.Content = listFieldName[j];
                    mChk.Tag = listField[j];
                    mChk.Checked += new RoutedEventHandler(mChk_Checked_Street);
                    mChk.Unchecked += new RoutedEventHandler(mChk_Unchecked_Street);
                    sp_field.Children.Add(mChk);
                }
                if ((Application.Current as IApp).sysRole == "ajj" && (Application.Current as IApp).iListSelectStreet == null)
                {
                    chkField.IsChecked = true;
                }
                if ((Application.Current as IApp).iListSelectStreet != null
                    && (Application.Current as IApp).iListSelectStreet.Count == listFieldName.Count)
                {
                    chkField.IsChecked = true;
                }
                getSelectStreet();
            }
            else
            {
                AYKJ.GISDevelop.Platform.ToolKit.Message.Show("查询街道配置读取错误");
            }

            //读取配置文件,获取企业类型信息
            var entl = (from item in xele.Element("JnEnterpriseType").Elements("enttype")
                      select new
                      {
                          name = item.Attribute("name").Value,
                      }).ToList();
            if (entl != null)
            {
                listEnterprise = new List<string>();
                foreach (var item in entl)
                {
                    listEnterprise.Add(item.name);
                }

                //初始化字列表
                sp_layer.Children.Clear();

                for (int j = 0; j < listEnterprise.Count; j++)
                {
                    mChk = new CheckBox();

                    if ((Application.Current as IApp).iListSelectEnterprise == null)
                    {
                        mChk.IsChecked = true;                        
                    }
                    else
                    {
                        //根据保存的企业类型显示状态初始化企业类型列表
                        listSelectEnterprise = (Application.Current as IApp).iListSelectEnterprise;
                        mChk.IsChecked = false;

                        foreach (string s in listSelectEnterprise)
                        {
                            if (s == listEnterprise[j])
                            {
                                mChk.IsChecked = true;
                                break;
                            }
                        }
                    }
                    mChk.Margin = new Thickness(0, 5, 5, 0);
                    mChk.Style = this.Resources["CheckBoxStyle"] as Style;
                    mChk.Foreground = new SolidColorBrush(Colors.White);
                    mChk.BorderBrush = new SolidColorBrush(Colors.White);
                    mChk.BorderBrush = null;
                    mChk.BorderThickness = new Thickness(0);

                    mChk.Content = listEnterprise[j];
                    mChk.Tag = listEnterprise[j];
                    mChk.Checked += new RoutedEventHandler(mChk_Checked_Enttype);
                    mChk.Unchecked += new RoutedEventHandler(mChk_Unchecked_Enttype);
                    sp_layer.Children.Add(mChk);
                }
                if ((Application.Current as IApp).iListSelectEnterprise == null)
                {
                    chkLayer.IsChecked = true;
                }
                if ((Application.Current as IApp).iListSelectEnterprise != null
                    && (Application.Current as IApp).iListSelectEnterprise.Count == listEnterprise.Count)
                {
                    chkLayer.IsChecked = true;
                }
                getSelectEnterpriseType();
            }
            else
            {
                AYKJ.GISDevelop.Platform.ToolKit.Message.Show("查询企业类型配置读取错误");
            }

        }
         
        //获取当前街道的勾选情况
        private void getSelectStreet()
        {
            listSelectStreet = new List<string>();
            
            foreach (var item in sp_field.Children)
            {
                CheckBox chk = item as CheckBox;
                if (chk.IsChecked.Value)
                {
                    listSelectStreet.Add(chk.Content.ToString());
                }
            }
        }

        //获取当前企业类型的勾选情况
        private void getSelectEnterpriseType()
        {
            listSelectEnterprise = new List<string>();

            foreach (var item in sp_layer.Children)
            {
                CheckBox chk = item as CheckBox;
                if (chk.IsChecked.Value)
                {
                    listSelectEnterprise.Add(chk.Content.ToString());
                }
            }
        }


        #region 企业类型选择
        //改变查询条件，清空查询结果        
        void mChk_Unchecked_Enttype(object sender, RoutedEventArgs e)
        {
            data_result.ItemsSource = null;
            getSelectEnterpriseType();

            string stname = (sender as CheckBox).Content.ToString();
            foreach (Layer ly in mainmap.Layers)
            {
                if (ly is GraphicsLayer)
                {
                    GraphicsLayer gcly = ly as GraphicsLayer;
                    foreach (Graphic gc in gcly.Graphics)
                    {
                        if (gc.Attributes["StaTag"] != null && gc.Attributes["StaTag"].ToString().Split('|').Length >= 6)
                        {
                            if (gc.Attributes["StaTag"].ToString().Split('|')[5] == stname)
                            {
                                gc.Attributes["mVisible"] = Visibility.Collapsed;
                            }
                        }
                    }
                }
            }
        }

        //改变查询条件，清空查询结果        
        void mChk_Checked_Enttype(object sender, RoutedEventArgs e)
        {
            data_result.ItemsSource = null;
            getSelectEnterpriseType();

            string stname = (sender as CheckBox).Content.ToString();
            foreach (Layer ly in mainmap.Layers)
            {
                if (ly is GraphicsLayer)
                {
                    GraphicsLayer gcly = ly as GraphicsLayer;
                    foreach (Graphic gc in gcly.Graphics)
                    {
                        if (gc.Attributes["StaTag"] != null && gc.Attributes["StaTag"].ToString().Split('|').Length >= 6)
                        {
                            if (gc.Attributes["StaTag"].ToString().Split('|')[5] == stname
                                && IfStreetShow(gc.Attributes["StaTag"].ToString()))//如果要显示企业，该企业所属的街道必须也是勾选“显示”。
                            {
                                gc.Attributes["mVisible"] = Visibility.Visible;
                            }
                        }
                    }
                }
            }
        }
        
        private bool IfStreetShow(string str_StaTag)
        {
            //当前企业所属的街道
            string street = str_StaTag.Split('|')[4];
            //获取当前勾选显示的街道

            getSelectStreet();

            if (listSelectStreet != null)
            {
                foreach (string s in listSelectStreet)
                {
                    if (s == street) return true;
                }
            }

            return false;
        }
        #endregion


        #region 街道选择
        //改变查询条件，清空查询结果        
        void mChk_Unchecked_Street(object sender, RoutedEventArgs e)
        {
            data_result.ItemsSource = null;
            getSelectStreet();

            string stname = (sender as CheckBox).Content.ToString();
            foreach (Layer ly in mainmap.Layers)
            {
                if (ly is GraphicsLayer)
                {
                    GraphicsLayer gcly = ly as GraphicsLayer;
                    foreach (Graphic gc in gcly.Graphics)
                    {
                        if (gc.Attributes["StaTag"] != null && gc.Attributes["StaTag"].ToString().Split('|').Length >= 5)
                        {
                            if (gc.Attributes["StaTag"].ToString().Split('|')[4] == stname)
                            {
                                gc.Attributes["mVisible"] = Visibility.Collapsed;
                            }
                        }
                    }
                }
            }
        }

        //改变查询条件，清空查询结果        
        void mChk_Checked_Street(object sender, RoutedEventArgs e)
        {
            data_result.ItemsSource = null;
            getSelectStreet();

            string stname = (sender as CheckBox).Content.ToString();
            foreach (Layer ly in mainmap.Layers)
            {
                if (ly is GraphicsLayer)
                {
                    GraphicsLayer gcly = ly as GraphicsLayer;
                    foreach (Graphic gc in gcly.Graphics)
                    {
                        if (gc.Attributes["StaTag"] != null && gc.Attributes["StaTag"].ToString().Split('|').Length >= 5)
                        {
                            if (gc.Attributes["StaTag"].ToString().Split('|')[4] == stname
                                && IfEnterpriseTypeShow(gc.Attributes["StaTag"].ToString()))//如果要显示街道内的所有企业，这些企业类型必须也是勾选“显示”。
                            {
                                gc.Attributes["mVisible"] = Visibility.Visible;
                            }
                        }
                    }
                }
            }
        }

        private bool IfEnterpriseTypeShow(string str_StaTag)
        {
            //当前企业类别
            string enttype = str_StaTag.Split('|')[5];

            //获取当前勾选显示的企业类别
            getSelectEnterpriseType();

            if (listSelectEnterprise != null)
            {
                foreach (string s in listSelectEnterprise)
                {
                    if (s == enttype) return true;
                }
            }

            return false;
        }
        #endregion

        /// <summary>
        /// datagrid切换选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void data_result_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectHigh_GraLayer.Graphics.Clear();
            if (data_result.SelectedItem != null)
            {
                classSearch cs = data_result.SelectedItem as classSearch;

                Graphic gra = new Graphic();
                gra.Geometry = new MapPoint(cs.x, cs.y);
                gra.Symbol = HighMarkerStyle;
                selectHigh_GraLayer.Graphics.Add(gra);

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

        //点击查询按钮的事件
        private void btn_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            switch (btn.Name)
            {
                case "btn_search":
                    //选中图层
                    //20150327：江宁项目，默认只查“企业”
                    listSelectLayer = new List<string>();
                    //foreach (var item in sp_layer.Children)
                    //{
                    //    CheckBox chk = item as CheckBox;
                    //    if (chk.IsChecked.Value)
                    //    {
                    //        listSelectLayer.Add(chk.Content.ToString());
                    //    }
                    //}
                    listSelectLayer.Add("企业");

                    //选中街道
                    getSelectStreet();

                    //选中的企业类型
                    getSelectEnterpriseType();


                    //输入的关键字
                    string strKey = txtKey.Text.ToString().Trim();

                    if (listSelectLayer.Count == 0)
                    {
                        AYKJ.GISDevelop.Platform.ToolKit.Message.Show("请至少选择一个图层");
                    }
                    else if (listSelectStreet.Count == 0)
                    {
                        AYKJ.GISDevelop.Platform.ToolKit.Message.Show("请至少选择一个街道");
                    }
                    else if (strKey == "")
                    {
                        AYKJ.GISDevelop.Platform.ToolKit.Message.Show("请输入关键字");
                    }
                    else
                    {
                        //开始查询
                        //List<classSearch> listSearchResult = new List<classSearch>();

                        lstdata = new List<clstipwxy>();
                        Dict_ResultPoint = new Dictionary<clstipwxy, MapPoint>();

                        for (int i = 0; i < listSelectLayer.Count; i++)
                        {
                            if ((Application.Current as IApp).dict_thematic.Keys.ToArray().Contains(listSelectLayer[i]))
                            {
                                //某图层的所有标注点
                                //List<Dictionary<string, string>> listDict = dict_thematic[listSelectLayer[i]];
                                List<Dictionary<string, string>> listDict = new List<Dictionary<string, string>>();

                                List<clsThematic> ddd = (Application.Current as IApp).dict_thematic[listSelectLayer[i]];
                                foreach (clsThematic cls in ddd)
                                {
                                    Dictionary<string, string> ds = new Dictionary<string, string>();
                                    ds.Add("WXYID", cls.str_wxyid);
                                    ds.Add("WXYTYPE", cls.str_wxytype);
                                    ds.Add("X", cls.str_x);
                                    ds.Add("Y", cls.str_y);
                                    ds.Add("DWDM", cls.str_dwdm);
                                    ds.Add("REMARK", cls.str_remark);
                                    ds.Add("NOTE", cls.str_note);
                                    ds.Add("ORDER", cls.str_order);
                                    ds.Add("STREET", cls.str_street);
                                    ds.Add("ENTTYPE", cls.str_enttype);

                                    listDict.Add(ds);
                                }


                                for (int j = 0; j < listDict.Count; j++)
                                {
                                    //该图层上某个标注点
                                    Dictionary<string, string> tmpDict = listDict[j];

                                    for (int n = 0; n < listSelectStreet.Count; n++)
                                    {
                                        if (tmpDict["STREET"].Equals(listSelectStreet[n]))
                                        {
                                            for (int m = 0; m < listSelectEnterprise.Count; m++)
                                            {
                                                if (tmpDict["ENTTYPE"].Equals(listSelectEnterprise[m]))
                                                {
                                                    //符合以上筛选条件后的点，再进行关键字的查询。
                                                    foreach (string key in tmpDict.Keys.ToList())
                                                    {
                                                        if (key != "X" && key != "Y")
                                                        {
                                                            if (tmpDict[key].Contains(strKey))
                                                            {
                                                                lstdata.Add(new clstipwxy()
                                                                {
                                                                    wxyid = tmpDict["WXYID"],
                                                                    wxyname = EllipsisName(tmpDict["REMARK"]),
                                                                    wxytip = tmpDict["REMARK"],
                                                                    wxytype = (Application.Current as IApp).DictThematicEnCn[tmpDict["WXYTYPE"]],
                                                                    wxydwdm = tmpDict["DWDM"]
                                                                });

                                                                Dict_ResultPoint.Add(lstdata[lstdata.Count - 1], new MapPoint(Double.Parse(tmpDict["X"]), Double.Parse(tmpDict["Y"])));
                                                                break;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }

                                }
                            }

                        }

                        data_result.ItemsSource = null;
                        if (lstdata == null || lstdata.Count < 1)
                        {
                            //查询无结果
                            AYKJ.GISDevelop.Platform.ToolKit.Message.Show("未找到符合查询条件的数据");
                        }
                        else
                        {
                            data_result.ItemsSource = lstdata;
                        }
                        

                    }

                    break;

                case "btn_reset"://按钮的重置事件
                    chkField.IsChecked = true;
                    for (int i = 0; i < sp_layer.Children.Count; i++)
                    {
                        CheckBox c = sp_layer.Children[i] as CheckBox;
                        c.IsChecked = true;
                    }

                    chkLayer.IsChecked = true;
                    for (int i = 0; i < sp_field.Children.Count; i++)
                    {
                        CheckBox c = sp_field.Children[i] as CheckBox;
                        c.IsChecked = true;
                    }

                    txtKey.Text = "";

                    data_result.ItemsSource = null;
                    break;
            }


        }

        #region 重置信息
        public void Reset()
        {
            (Application.Current as IApp).iListSelectStreet = listSelectStreet;

            (Application.Current as IApp).iListSelectEnterprise = listSelectEnterprise;
        }
        #endregion

        //自定义查询后显示的类
        public class classSearch
        {
            [Display(Name = "名称", GroupName = "classSearch")]
            public string name { get; set; }
            [Display(Name = "类型", GroupName = "classSearch")]
            public string type { get; set; }
            [Display(Name = "x", GroupName = "classSearch")]
            public double x { get; set; }
            [Display(Name = "y", GroupName = "classSearch")]
            public double y { get; set; }

        }

        //全选
        private void chk_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            switch (chk.Name)
            {
                case "chkLayer":
                    for (int i = 0; i < sp_layer.Children.Count; i++)
                    {
                        CheckBox c = sp_layer.Children[i] as CheckBox;
                        c.IsChecked = true;
                    }
                    getSelectEnterpriseType();
                    break;
                case "chkField":
                    for (int i = 0; i < sp_field.Children.Count; i++)
                    {
                        CheckBox c = sp_field.Children[i] as CheckBox;
                        c.IsChecked = true;
                    }
                    getSelectStreet();
                    break;
            }
            //改变查询条件，清空查询结果
            data_result.ItemsSource = null;
        }

        //全不选
        private void chk_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            switch (chk.Name)
            {
                case "chkLayer":

                    for (int i = 0; i < sp_layer.Children.Count; i++)
                    {
                        CheckBox c = sp_layer.Children[i] as CheckBox;
                        c.IsChecked = false;
                    }
                    getSelectEnterpriseType();
                    break;
                case "chkField":
                    for (int i = 0; i < sp_field.Children.Count; i++)
                    {
                        CheckBox c = sp_field.Children[i] as CheckBox;
                        c.IsChecked = false;
                    }
                    getSelectStreet();
                    break;
            }
            //改变查询条件，清空查询结果
            data_result.ItemsSource = null;
        }

        private void btn_postion_Click(object sender, RoutedEventArgs e)
        {
            selectHigh_GraLayer.Graphics.Clear();
            if (data_result.SelectedItem != null)
            {
                Graphic gra = new Graphic();
                gra.Geometry = Dict_ResultPoint[data_result.SelectedItem as clstipwxy];
                gra.Symbol = HighMarkerStyle;
                selectHigh_GraLayer.Graphics.Add(gra);

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
                tmpstr = str.Substring(0, 5) + "...";
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
            ToolTipService.SetToolTip(e.Row, tbx);
        }
    }
}
