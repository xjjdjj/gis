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
using AYKJ.GISDevelop.Platform.Part;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client;
using System.Text;
using System.Xml.Linq;

namespace AYKJ.GISKeysearch
{
    public partial class MainPage : UserControl, IWidgets
    {
        //用户选择的图层列表
        private List<string> listSelectLayer;
        //用户选择的字段列表
        private List<string> listSelectField;

        //20120918:
        //查询后属性数据
        List<clstipwxy> lstdata;
        //绑定的空间数据
        Dictionary<clstipwxy, MapPoint> Dict_ResultPoint;

        //
        public static GraphicsLayer selectHigh_GraLayer;

        //定义一个Map用来接收平台的Map
        public static Map mainmap;

        //所有的专题数据
        Dictionary<string, GraphicsLayer> Dict_Data;

        //可供选择的字段列表
        private static List<string> listField;

        public MainPage()
        {
            InitializeComponent();

            mainmap = (Application.Current as IApp).MainMap;

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

            Storyboard_Close.Completed += new EventHandler(Storyboard_Close_Completed);

            //设置面板的起始位置
            this.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            this.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            this.Margin = new Thickness() { Top = 10, Right = 13 };

            CreateLayer();
        }


        /// <summary>
        /// 创建查询图层
        /// </summary>
        void CreateLayer()
        {

            XElement xele = PFApp.Extent;
            //读取配置文件,获取字段列表信息            
            var lf = (from item in xele.Element("KeySearchFields").Elements("fieldname")
                      select new
                      {
                          value = item.Attribute("value").Value,
                          name = item.Attribute("name").Value,
                      }).ToList();

            if (lf != null)
            {
                listField = new List<string>();
                foreach (var item in lf)
                {
                    listField.Add(item.value);//英文
                }
            }
            else
            {
                AYKJ.GISDevelop.Platform.ToolKit.Message.Show("查询字段配置读取错误");
            }
        }

        //点击查询按钮的事件
        private void btn_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            switch (btn.Name)
            {
                case "btn_search":
                    Dict_Data = (Application.Current as IApp).Dict_ThematicLayer;

                    //选中图层
                    listSelectLayer = new List<string>();
                    for (int i = 0; i < Dict_Data.Count; i++)
                    {
                        listSelectLayer.Add(Dict_Data.Keys.ToList()[i]);
                    }

                    //选中字段
                    listSelectField = new List<string>();
                    for (int j = 0; j < listField.Count; j++)
                    {
                        listSelectField.Add(listField[j]);
                    }


                    //输入的关键字
                    string strKey = txtKey.Text.ToString().Trim();

                    if (listSelectLayer.Count == 0)
                    {
                        AYKJ.GISDevelop.Platform.ToolKit.Message.Show("请至少选择一个图层");
                    }
                    else if (listSelectField.Count == 0)
                    {
                        AYKJ.GISDevelop.Platform.ToolKit.Message.Show("请至少选择一个字段");
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

                                    listDict.Add(ds);
                                }


                                for (int j = 0; j < listDict.Count; j++)
                                {
                                    //该图层上某个标注点
                                    Dictionary<string, string> tmpDict = listDict[j];


                                    for (int n = 0; n < listSelectField.Count; n++)
                                    {
                                        if (tmpDict.Keys.Contains(listSelectField[n]))
                                        {
                                            //如果该标注点的某个字段名在用户所选的字段列表中
                                            if (tmpDict[listSelectField[n]].Contains(strKey))
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

                                                //classSearch cs = new classSearch();
                                                //cs.name = tmpDict["REMARK"];
                                                //cs.type = tmpDict["WXYTYPE"];
                                                //cs.x = Double.Parse(tmpDict["X"]);
                                                //cs.y = Double.Parse(tmpDict["Y"]);

                                                //listSearchResult.Add(cs);

                                                break;
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

                    txtKey.Text = "";

                    data_result.ItemsSource = null;
                    break;
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
            if (length < 30)
            {
                tmpstr = str;
            }
            else
            {
                tmpstr = str.Substring(0, 14) + "...";
            }
            return tmpstr;
        }

        public class clstipwxy
        {
            public string wxyid { set; get; }
            public string wxyname { set; get; }
            public string wxytip { set; get; }
            public string wxytype { set; get; }
            public string wxydwdm { set; get; }
            public string wxygrid { set; get; }
            public Graphic wxygra { set; get; }
        }

        #region 面板关闭打开控制
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


        #region 面板展开关闭接口
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
        }

        public event IWidgetEventHander OpenEnd;

        public event PartEventHander CloseEnd;

        public PartDescriptor Descri
        {
            get { return new PartDescriptor() { Name = "AYKJ.GISKeysearch" }; }
        }

        public bool IsOpen
        {
            get { throw new NotImplementedException(); }
        }

        public string LinkFromGiPlatform(string oAction, string oStr, object oArr, object oCls, object[] oArrStr, object[] oArrArr, object[] oArrCls)
        {
            throw new NotImplementedException();
        }

        public event PartEventHander LinkGisPlatformEnd;

        public string LinkReturnGisPlatform(string mark, string s)
        {
            return "";
        }

        public string LinkReturnGisPlatform(string mark, object obj1, object obj2)
        {
            return "";
        }

        public void ReInit()
        {
            throw new NotImplementedException();
        }

        public event PartEventHander ReInitEnd;

        public event PartEventHander ShowEnd;
        #endregion
    }
}
