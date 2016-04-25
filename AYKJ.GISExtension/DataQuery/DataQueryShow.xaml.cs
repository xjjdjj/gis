﻿using System;
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
    public partial class DataQueryShow : UserControl
    {
        //图层名称列表
        private static List<string> listLayerName;
        //图层代码列表
        private static List<string> listLayerCode;

        //可供选择的街道列表
        private static List<string> listStreet;

        //所有的字段列表
        private static List<string> listAllField;

        //专题数据的名称和代码对应
        private static Dictionary<string, string> Dict_EnCn;
        //专题数据（<图层，图层内所有标注>）
        private static Dictionary<string, List<Dictionary<string, string>>> dict_thematic;
        //所有的专题数据
        Dictionary<string, GraphicsLayer> Dict_Data;

        //用户选择的图层列表
        private List<string> listSelectLayer;
        //用户选择的字段列表
        private List<string> listSelectField;

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


        public DataQueryShow()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(DataQueryKey_Loaded);
        }

        void DataQueryKey_Loaded(object sender, RoutedEventArgs e)
        {
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

            CreateLayer();

            
        }

        /// <summary>
        /// 创建查询图层
        /// </summary>
        void CreateLayer()
        {
            Dict_Data = (Application.Current as IApp).Dict_ThematicLayer;

            for (int i = 0; i < Dict_Data.Count(); i++)
            {

            }

            CheckBox mChk = new CheckBox();

            //初始化图层列表
            sp_layer.Children.Clear();
            for (int i = 0; i < Dict_Data.Count; i++)
            {
                mChk = new CheckBox();
                //样式
                mChk.IsChecked = true;
                mChk.Margin = new Thickness(0, 5, 5, 0);
                mChk.Style = this.Resources["CheckBoxStyle"] as Style;
                mChk.BorderBrush = null;
                mChk.BorderThickness = new Thickness(0);
                mChk.Foreground = new SolidColorBrush(Colors.White);

                mChk.Content = Dict_Data.Keys.ToList()[i];
                mChk.Tag = i;
                mChk.Checked += new RoutedEventHandler(mChk_LayerChecked);
                mChk.Unchecked += new RoutedEventHandler(mChk_LayerUnchecked);
                sp_layer.Children.Add(mChk);
            }
            chkAllLayer.IsChecked = true;

            XElement xele = PFApp.Extent;
            //读取配置文件,获取街道列表信息
            var lf = (from item in xele.Element("JnStreet").Elements("street")
                      select new
                      {
                          position = item.Attribute("position").Value,
                          name = item.Attribute("name").Value,
                      }).ToList();
            if (lf != null)
            {
                listStreet = new List<string>();
                List<string> listStreetPositon = new List<string>();
                foreach (var item in lf)
                {
                    listStreetPositon.Add(item.position);
                    listStreet.Add(item.name);
                }

                //初始化字段列表
                sp_street.Children.Clear();
                for (int j = 0; j < listStreet.Count; j++)
                {
                    mChk = new CheckBox();
                    //样式
                    mChk.IsChecked = true;
                    mChk.Margin = new Thickness(0, 5, 5, 0);
                    mChk.Style = this.Resources["CheckBoxStyle"] as Style;
                    mChk.Foreground = new SolidColorBrush(Colors.White);
                    mChk.BorderBrush = new SolidColorBrush(Colors.White);
                    mChk.BorderBrush = null;
                    mChk.BorderThickness = new Thickness(0);

                    mChk.Content = listStreet[j];
                    mChk.Tag = listStreetPositon[j];
                    mChk.Checked += new RoutedEventHandler(mChk_StreetChecked);
                    mChk.Unchecked += new RoutedEventHandler(mChk_StreetUnchecked);
                    sp_street.Children.Add(mChk);
                }
                chkAllStreet.IsChecked = true;
            }
            else
            {
                AYKJ.GISDevelop.Platform.ToolKit.Message.Show("查询字段配置读取错误");
            }
        }

        #region 图层筛选控制
        //所有图层撤销选择
        private void chk_AllLayerUnchecked(object sender, RoutedEventArgs e)
        {
            //设置checkbox状态
            foreach (CheckBox ck in sp_layer.Children)
            {
                ck.IsChecked = false;
            }
            
        }

        //所有图层选择
        private void chk_AllLayerChecked(object sender, RoutedEventArgs e)
        {
            //设置checkbox状态
            foreach (CheckBox ck in sp_layer.Children)
            {
                ck.IsChecked = true;
            }

            //foreach (Layer ly in mainmap.Layers)
            //{
            //    if (ly is GraphicsLayer)
            //    {
            //        ly.Visible = true;
            //    }
            //}
        }

        //图层撤销选中
        void mChk_LayerUnchecked(object sender, RoutedEventArgs e)
        {
            string lyname = (sender as CheckBox).Content.ToString();
            foreach(Layer ly in mainmap.Layers)
            {
                if (ly is GraphicsLayer && lyname==ly.ID)
                {
                    ly.Visible = false;
                }
            }            
        }

        //图层选中      
        void mChk_LayerChecked(object sender, RoutedEventArgs e)
        {
            string lyname = (sender as CheckBox).Content.ToString();
            foreach (Layer ly in mainmap.Layers)
            {
                if (ly is GraphicsLayer && lyname == ly.ID)
                {
                    ly.Visible = true;
                }
            }
        }
        #endregion

        #region 街道筛选控制
        //所有街道撤销选择
        private void chk_AllStreetUnchecked(object sender, RoutedEventArgs e)
        {
            //设置checkbox状态
            foreach (CheckBox ck in sp_street.Children)
            {
                ck.IsChecked = false;
            }

            //foreach (Layer ly in mainmap.Layers)
            //{
            //    if (ly is GraphicsLayer)
            //    {
            //        GraphicsLayer gcly = ly as GraphicsLayer;
            //        foreach (Graphic gc in gcly.Graphics)
            //        {
            //            foreach (String st in listStreet)
            //            {
            //                if (gc.Attributes["StaTag"].ToString().Split('|')[4] == st)
            //                {
            //                    gc.Attributes["mVisible"] = Visibility.Collapsed;
            //                    break;
            //                }
            //            }
            //        }
            //    }
            //}
        }

        //所有街道选择
        private void chk_AllStreetChecked(object sender, RoutedEventArgs e)
        {
            //设置checkbox状态
            foreach (CheckBox ck in sp_street.Children)
            {
                ck.IsChecked = true;
            }
        }

        //街道撤销选中     
        void mChk_StreetUnchecked(object sender, RoutedEventArgs e)
        {
            string stname = (sender as CheckBox).Content.ToString();
            foreach (Layer ly in mainmap.Layers)
            {
                if (ly is GraphicsLayer)
                {
                    GraphicsLayer gcly = ly as GraphicsLayer;
                    foreach (Graphic gc in gcly.Graphics)
                    {
                        if (gc.Attributes["StaTag"].ToString().Split('|')[4] == stname)
                        {
                            gc.Attributes["mVisible"] = Visibility.Collapsed;
                        }
                    }
                }
            }
        }
        //街道选中           
        void mChk_StreetChecked(object sender, RoutedEventArgs e)
        {
            string stname = (sender as CheckBox).Content.ToString();
            foreach (Layer ly in mainmap.Layers)
            {
                if (ly is GraphicsLayer)
                {
                    GraphicsLayer gcly = ly as GraphicsLayer;
                    foreach (Graphic gc in gcly.Graphics)
                    {
                        if (gc.Attributes["StaTag"].ToString().Split('|')[4] == stname)
                        {
                            gc.Attributes["mVisible"] = Visibility.Visible;
                        }
                    }
                }
            }
        }
        #endregion

        public void getAllDataFromMainPage(Dictionary<string, string> dic_encn, Dictionary<string, List<Dictionary<string, string>>> dic_thematic)
        {
            //中英文信息赋值
            Dict_EnCn = dic_encn;
            //专题信息赋值
            dict_thematic = dic_thematic;
            //图层信息,名称和代码
            listLayerName = new List<string>();
            listLayerCode = new List<string>();
            foreach (KeyValuePair<string, string> kv in Dict_EnCn)
            {
                listLayerName.Add(kv.Value);
                listLayerCode.Add(kv.Key);
            }
            //读取配置文件,获取所有的字段列表信息
            listAllField = new List<string>();
            XElement xele = PFApp.Extent;
            var laf = (from item in xele.Element("KeyAllFields").Elements("fieldname")
                       select new
                       {
                           value = item.Attribute("value").Value,
                       }).ToList();
            foreach (var item in laf)
            {
                //listAllField.Add(item.value);
            }
        }

        //获取所有标注信息
        public void getAllDataFromMainPage(string strAll)
        {
            //AYKJ.GISDevelop.Platform.ToolKit.Message.Show(strAll);
            JsonValue jsonObject = JsonObject.Parse(strAll);

            XElement xele = PFApp.Extent;

            //读取配置文件,获取图层名称
            listLayerName = new List<string>();
            var ln = (from item in xele.Element("DataServices").Elements("Parameter")
                      where item.Attribute("Name").Value == "点位名称"
                      select new
                      {
                          value = item.Attribute("Value").Value,
                      }).ToList();
            string s = ln[0].value;

            if (s.Trim() != "")
            {

                foreach (var item in s.Split('|'))
                {
                    listLayerName.Add(item);
                }
            }
            //读取配置文件,获取图层代码
            listLayerCode = new List<string>();
            var lc = (from item in xele.Element("DataServices").Elements("Parameter")
                      where item.Attribute("Name").Value == "点位代码"
                      select new
                      {
                          value = item.Attribute("Value").Value,
                      }).ToList();
            string c = lc[0].value;

            if (c.Trim() != "")
            {

                foreach (var item in c.Split('|'))
                {
                    listLayerCode.Add(item);
                }
            }
            //生成图层名称和代码的键值对
            Dict_EnCn = new Dictionary<string, string>();
            for (int i = 0; i < listLayerName.Count; i++)
            {
                Dict_EnCn.Add(listLayerCode[i], listLayerName[i]);

            }

            //读取配置文件,获取所有的字段列表信息
            listAllField = new List<string>();
            var laf = (from item in xele.Element("KeyAllFields").Elements("fieldname")
                       select new
                       {
                           value = item.Attribute("value").Value,
                       }).ToList();
            foreach (var item in laf)
            {
                listAllField.Add(item.value);
            }

            dict_thematic = new Dictionary<string, List<Dictionary<string, string>>>();

            #region 解析Json
            for (int i = 0; i < jsonObject["zttc"].Count; i++)
            {
                Dictionary<string, string> td = new Dictionary<string, string>();

                for (int j = 0; j < listAllField.Count; j++)
                {
                    td[listAllField[j]] = jsonObject["zttc"][i][listAllField[j]].ToString().Replace("\"", "");

                }

                //获取类型的中文名（即图层的中文名称）
                //20120802zc:规定第二个必须为类型
                //如果中英文对照表中不存在此种WXYTYPE则不执行
                if (Dict_EnCn.ContainsKey(jsonObject["zttc"][i][listAllField[1]].ToString().Replace("\"", "")))
                {
                    string tmp_cntype = Dict_EnCn[jsonObject["zttc"][i][listAllField[1]].ToString().Replace("\"", "")];

                    if (!dict_thematic.Keys.ToArray().Contains(tmp_cntype))
                    {
                        #region 获取专题数据
                        List<Dictionary<string, string>> lsttmp = new List<Dictionary<string, string>>();

                        lsttmp.Add(td);

                        dict_thematic.Add(tmp_cntype, lsttmp);
                        #endregion

                    }
                    else
                    {
                        #region 获取专题数据
                        List<Dictionary<string, string>> lsttmp = dict_thematic[tmp_cntype];

                        lsttmp.Add(td);

                        dict_thematic.Remove(tmp_cntype);
                        dict_thematic.Add(tmp_cntype, lsttmp);
                        #endregion
                    }
                }
            }
            #endregion
        }



        #region 重置信息
        public void Reset()
        {
            try
            {
                chkAllStreet.IsChecked = true;
                for (int i = 0; i < sp_layer.Children.Count; i++)
                {
                    CheckBox c = sp_layer.Children[i] as CheckBox;
                    c.IsChecked = true;
                }

                chkAllLayer.IsChecked = true;
                for (int i = 0; i < sp_street.Children.Count; i++)
                {
                    CheckBox c = sp_street.Children[i] as CheckBox;
                    c.IsChecked = true;
                }


                if (selectHigh_GraLayer != null)
                {
                    selectHigh_GraLayer.Graphics.Clear();
                    mainmap.Layers.Remove(selectHigh_GraLayer);
                }

            }
            catch (Exception)
            { }
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

                    break;
                case "chkField":
                    for (int i = 0; i < sp_street.Children.Count; i++)
                    {
                        CheckBox c = sp_street.Children[i] as CheckBox;
                        c.IsChecked = true;
                    }
                    break;
            }

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
                    break;
                case "chkField":
                    for (int i = 0; i < sp_street.Children.Count; i++)
                    {
                        CheckBox c = sp_street.Children[i] as CheckBox;
                        c.IsChecked = false;
                    }
                    break;
            }

        }

        private void btn_postion_Click(object sender, RoutedEventArgs e)
        {
            selectHigh_GraLayer.Graphics.Clear();
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
