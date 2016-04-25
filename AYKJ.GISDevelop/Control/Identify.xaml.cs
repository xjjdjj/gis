/// <summary>  
/// 作者：陈锋 
/// 时间：2012/5/24 11:52:38  
/// 公司:南京安元科技有限公司  
/// 版权：2012-2020  
/// CLR版本：4.0.30319.261  
/// Identify说明：基础地理信息查询
/// 唯一标识：3c4afb10-5fce-4aa5-a743-3846ae955c52  
/// </summary>

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using AYKJ.GISDevelop.Platform;
using AYKJ.GISDevelop.Platform.ToolKit;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Tasks;
using System.Windows.Controls.Primitives;

namespace AYKJ.GISDevelop.Control
{
    public partial class Identify : UserControl
    {
        //地图
        Map SystemMap;
        //定义一个地图Draw
        Draw Draw_Identify;
        //面样式
        FillSymbol fillsymbol;
        //线样式
        LineSymbol linesymbol;
        //点样式
        SimpleMarkerSymbol markersymbol;
        //一次高亮图层
        GraphicsLayer FirstGraLayer;
        //控制图层是否显示
        bool isshowlayer = false;
        //当前选择的图层
        List<string> lstQueryUr = new List<string>();
        //图层xml
        string strtreexml = string.Empty;
        //结果xml
        string strresultxml = string.Empty;
        //执行查询方法次数
        int intquery = 0;
        //已执行查询方法次数
        int intqueryed = 0;
        //查询的结果数
        int intcount = 0;
        //查询结果<别名，value>
        Dictionary<string, string> DataValue = new Dictionary<string, string>();
        public ToggleButton currrentogbtn;

        public Identify()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(Identify_Loaded);
        }

        /// <summary>
        /// 实例化窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Identify_Loaded(object sender, RoutedEventArgs e)
        {
            //设置面板的起始位置
            this.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            this.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            this.Margin = new Thickness(0, 0, 30, 5);

            SystemMap = App.mainMap;
            lstQueryUr = new List<string>();
            GetLayers(SystemMap);
            FirstGraLayer = new GraphicsLayer();
            SystemMap.Layers.Add(FirstGraLayer);
            Draw_Identify = new Draw(SystemMap);
            linesymbol = new LineSymbol() { Width = 4, Color = new SolidColorBrush(new Color() { A = 255, R = 0, G = 255, B = 0 }) };
            markersymbol = new SimpleMarkerSymbol() { Color = new SolidColorBrush(new Color() { A = 255, R = 0, G = 255, B = 0 }), Size = 10 };
            fillsymbol = new FillSymbol() { BorderThickness = 2, BorderBrush = new SolidColorBrush(new Color() { A = 255, R = 0, G = 255, B = 0 }), Fill = new SolidColorBrush(new Color() { A = 100, R = 0, G = 255, B = 0 }) };

            Draw_Identify.DrawComplete -= Draw_Identify_DrawComplete;
            Draw_Identify.DrawComplete += new EventHandler<DrawEventArgs>(Draw_Identify_DrawComplete);

            Draw_Identify.DrawMode = DrawMode.Point;
            Draw_Identify.IsEnabled = true;
            Storyboard_Close.Completed += new EventHandler(Storyboard_Close_Completed);
        }

        #region 图层选择
        /// <summary>
        /// 根据Map获取图层和对应的ID
        /// </summary>
        /// <param name="tmpmap"></param>
        void GetLayers(Map tmpmap)
        {
            string strxml = "<root>";
            strxml = strxml + "<node name=\"" + "全部图层" + "\"/>";
            for (int i = 0; i < tmpmap.Layers.Count(); i++)
            {
                //if (tmpmap.Layers[i].ID == "数字管网服务")
                //{
                if (tmpmap.Layers[i].GetType().Name == "ArcGISDynamicMapServiceLayer")
                {
                    ArcGISDynamicMapServiceLayer tmpDynamicMap = tmpmap.Layers[i] as ArcGISDynamicMapServiceLayer;
                    lstQueryUr.Add(tmpDynamicMap.Url);
                    strxml = strxml + "<node name=\"" + tmpmap.Layers[i].ID + "\" value=\"" + tmpDynamicMap.Url.ToString() + "\">";
                    for (int m = 0; m < tmpDynamicMap.Layers.Count(); m++)
                    {
                        if (tmpDynamicMap.Layers[m].SubLayerIds != null)
                        {
                            string tmp = tmpDynamicMap.Url.ToString() + "/" + (m + 1).ToString() + "-" + tmpDynamicMap.Url.ToString() + "/" + (m + tmpDynamicMap.Layers[m].SubLayerIds.Count()).ToString();
                            strxml = strxml + "<node name=\"" + tmpDynamicMap.Layers[m].Name + "\" value=\"" + tmp + "\">";
                            int k;
                            for (k = m + 1; k < tmpDynamicMap.Layers[m].SubLayerIds.Count() + m + 1; k++)
                            {
                                strxml = strxml + "<node name=\"" + tmpDynamicMap.Layers[k].Name + "\" value=\"" + tmpDynamicMap.Url.ToString() + "/" + k + "\"/>";
                            }
                            strxml = strxml + "</node>";
                            m = k - 1;
                        }
                        else
                        {
                            strxml = strxml + "<node name=\"" + tmpDynamicMap.Layers[m].Name + "\" value=\"" + tmpDynamicMap.Url.ToString() + "/" + m + "\"/>";
                        }
                    }
                    strxml = strxml + "</node>";
                }
                else if (tmpmap.Layers[i].GetType().Name == "ArcGISTiledMapServiceLayer")
                {
                    ArcGISTiledMapServiceLayer tmpDynamicMap = tmpmap.Layers[i] as ArcGISTiledMapServiceLayer;
                    lstQueryUr.Add(tmpDynamicMap.Url);
                    strxml = strxml + "<node name=\"" + tmpmap.Layers[i].ID + "\" value=\"" + tmpDynamicMap.Url.ToString() + "\">";
                    for (int m = 0; m < tmpDynamicMap.Layers.Count(); m++)
                    {
                        if (tmpDynamicMap.Layers[m].SubLayerIds != null)
                        {
                            string tmp = tmpDynamicMap.Url.ToString() + "/" + (m + 1).ToString() + "-" + tmpDynamicMap.Url.ToString() + "/" + (m + tmpDynamicMap.Layers[m].SubLayerIds.Count()).ToString();
                            strxml = strxml + "<node name=\"" + tmpDynamicMap.Layers[m].Name + "\" value=\"" + tmp + "\">";
                            int k;
                            for (k = m + 1; k < tmpDynamicMap.Layers[m].SubLayerIds.Count() + m + 1; k++)
                            {
                                strxml = strxml + "<node name=\"" + tmpDynamicMap.Layers[k].Name + "\" value=\"" + tmpDynamicMap.Url.ToString() + "/" + k + "\"/>";
                            }
                            strxml = strxml + "</node>";
                            m = k - 1;
                        }
                        else
                        {
                            strxml = strxml + "<node name=\"" + tmpDynamicMap.Layers[m].Name + "\" value=\"" + tmpDynamicMap.Url.ToString() + "/" + m + "\"/>";
                        }
                    }
                    strxml = strxml + "</node>";
                }
                //}
            }
            strxml = strxml + "</root>";
            strtreexml = strxml;
            XElement root = XElement.Parse(strxml);
            // 构造带层级关系的数据源（递归方式）
            var result = LoadData(root);
            tree.DataContext = result;
        }

        private List<TreeViewModel> LoadData(XElement root)
        {
            if (root == null)
                return null;
            var items = from n in root.Elements("node")
                        select new TreeViewModel
                        {
                            Title = n.Attribute("name").Value,
                            Lay = (n.Attribute("value") != null ? n.Attribute("value").Value : ""),
                            Children = LoadData(n),
                            Type =(n.Attribute("type") != null ? n.Attribute("type").Value : "")
                        };
            return items.ToList<TreeViewModel>();
        }

        private void btn_showlayer_Click(object sender, RoutedEventArgs e)
        {
            if (isshowlayer == false)
            {
                GridLayer.Visibility = Visibility.Visible;
                isshowlayer = true;
            }
            else
            {
                GridLayer.Visibility = Visibility.Collapsed;
                isshowlayer = false;
            }
        }

        private void layer_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (tree.SelectedItem == null)
                return;
            lstQueryUr = new List<string>();
            string StrTitle = ((TreeViewModel)(e.NewValue)).Title;
            txt_layer.Text = StrTitle;
            GridLayer.Visibility = Visibility.Collapsed;
            isshowlayer = false;
            if (StrTitle == "全部图层")
            {
                for (int i = 0; i < SystemMap.Layers.Count(); i++)
                {
                    if (SystemMap.Layers[i].GetType().Name == "ArcGISDynamicMapServiceLayer")
                    {
                        ArcGISDynamicMapServiceLayer tmpDynamicMap = SystemMap.Layers[i] as ArcGISDynamicMapServiceLayer;
                        lstQueryUr.Add(tmpDynamicMap.Url);
                    }
                    else if (SystemMap.Layers[i].GetType().Name == "ArcGISTiledMapServiceLayer")
                    {
                        ArcGISTiledMapServiceLayer tmpTiledMap = SystemMap.Layers[i] as ArcGISTiledMapServiceLayer;
                        lstQueryUr.Add(tmpTiledMap.Url);
                    }
                }
            }
            else
            {
                string[] tmpary = ((TreeViewModel)(e.NewValue)).Lay.ToString().Split('-');
                for (int i = 0; i < tmpary.Length; i++)
                {
                    lstQueryUr.Add(tmpary[i]);
                }
            }
            XElement root = XElement.Parse(strtreexml);
            // 构造带层级关系的数据源（递归方式）
            var result = LoadData(root);
            tree.DataContext = result;
        }

        /// <summary>
        /// 判断一个字符串是否为正整数
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public int IsNumeric(string str)
        {
            int i;
            if (str != null && Regex.IsMatch(str, @"^\d+$"))
                i = int.Parse(str);
            else
                i = -1;
            return i;
        }
        #endregion

        /// <summary>
        /// 绘制结束的查询
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Draw_Identify_DrawComplete(object sender, DrawEventArgs e)
        {
            Draw_Identify.IsEnabled = false;
            layertree.DataContext = null;
            dataresult.ItemsSource = null;
            txt_count.Text = "";
            intquery = 0;
            intqueryed = 0;
            intcount = 0;
            txt_coor.Text = "X:=" + e.Geometry.Extent.XMax.ToString("0.000") + "  " + "Y:=" + e.Geometry.Extent.YMax.ToString("0.000");
            if (txt_layer.Text == "全部图层")
            {
                for (int i = 0; i < lstQueryUr.Count; i++)
                {
                    IdentifyParameters identifyParams = new IdentifyParameters()
                    {
                        Geometry = e.Geometry,
                        MapExtent = SystemMap.Extent,
                        ReturnGeometry = true
                    };
                    identifyParams.LayerOption = LayerOption.all;
                    IdentifyTask identifyTask = new IdentifyTask(lstQueryUr[i]);
                    identifyTask.Failed += new EventHandler<TaskFailedEventArgs>(identifyTask_Failed);
                    identifyTask.ExecuteCompleted += new EventHandler<IdentifyEventArgs>(identifyTask_ExecuteCompleted);
                    identifyTask.ExecuteAsync(identifyParams);
                    intquery = intquery + 1;
                }
                StoryboardStart();
            }
            else
            {
                intquery = 1;
                if (lstQueryUr.Count == 1)
                {
                    string tmpstr = lstQueryUr[0].Substring(lstQueryUr[0].LastIndexOf("/") + 1, lstQueryUr[0].Length - lstQueryUr[0].LastIndexOf("/") - 1);
                    int inttmp = IsNumeric(tmpstr);
                    if (inttmp == -1)
                    {
                        IdentifyParameters identifyParams = new IdentifyParameters()
                        {
                            Geometry = e.Geometry,
                            MapExtent = SystemMap.Extent,
                            ReturnGeometry = true
                        };
                        identifyParams.LayerOption = LayerOption.all;
                        IdentifyTask identifyTask = new IdentifyTask(lstQueryUr[0]);
                        identifyTask.Failed += new EventHandler<TaskFailedEventArgs>(identifyTask_Failed);
                        identifyTask.ExecuteCompleted += new EventHandler<IdentifyEventArgs>(identifyTask_ExecuteCompleted);
                        identifyTask.ExecuteAsync(identifyParams);
                    }
                    else
                    {
                        string strtmpurl = lstQueryUr[0].Substring(0, lstQueryUr[0].LastIndexOf("/"));
                        IdentifyParameters identifyParams = new IdentifyParameters()
                        {
                            Geometry = e.Geometry,
                            MapExtent = SystemMap.Extent,
                            ReturnGeometry = true
                        };
                        identifyParams.LayerOption =  LayerOption.all;
                        identifyParams.LayerIds.Add(inttmp);
                        IdentifyTask identifyTask = new IdentifyTask(strtmpurl);
                        identifyTask.Failed += new EventHandler<TaskFailedEventArgs>(identifyTask_Failed);
                        identifyTask.ExecuteCompleted += new EventHandler<IdentifyEventArgs>(identifyTask_ExecuteCompleted);
                        identifyTask.ExecuteAsync(identifyParams);
                    }
                    StoryboardStart();
                }
                else
                {
                    string tmpstr1 = lstQueryUr[0].Substring(lstQueryUr[0].LastIndexOf("/") + 1, lstQueryUr[0].Length - lstQueryUr[0].LastIndexOf("/") - 1);
                    string tmpstr2 = lstQueryUr[1].Substring(lstQueryUr[1].LastIndexOf("/") + 1, lstQueryUr[1].Length - lstQueryUr[1].LastIndexOf("/") - 1);
                    string strtmpurl = lstQueryUr[0].Substring(0, lstQueryUr[0].LastIndexOf("/"));
                    IdentifyParameters identifyParams = new IdentifyParameters()
                    {
                        Geometry = e.Geometry,
                        MapExtent = SystemMap.Extent,
                        ReturnGeometry = true
                    };
                    identifyParams.LayerOption = LayerOption.all;
                    for (int i = int.Parse(tmpstr1); i < int.Parse(tmpstr2) + 1; i++)
                    {
                        identifyParams.LayerIds.Add(i);
                    }
                    IdentifyTask identifyTask = new IdentifyTask(strtmpurl);
                    identifyTask.Failed += new EventHandler<TaskFailedEventArgs>(identifyTask_Failed);
                    identifyTask.ExecuteCompleted += new EventHandler<IdentifyEventArgs>(identifyTask_ExecuteCompleted);
                    identifyTask.ExecuteAsync(identifyParams);
                    StoryboardStart();
                }
            }
            strresultxml = "<root>";
        }

        /// <summary>
        /// 查询返回结果
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void identifyTask_ExecuteCompleted(object sender, IdentifyEventArgs e)
        {
            try
            {
                intqueryed = intqueryed + 1;
                intcount = intcount + e.IdentifyResults.Count();
                Dictionary<string, List<Graphic>> dict_result = new Dictionary<string, List<Graphic>>();
                Dictionary<string, string> dict_lyid = new Dictionary<string, string>();
                for (int i = 0; i < e.IdentifyResults.Count(); i++)
                {
                    if (dict_result.Keys.ToList().Contains(e.IdentifyResults[i].LayerName))
                    {
                        List<Graphic> lsttmp = dict_result[e.IdentifyResults[i].LayerName];
                        lsttmp.Add(e.IdentifyResults[i].Feature);
                        dict_result.Remove(e.IdentifyResults[i].LayerName);
                        dict_result.Add(e.IdentifyResults[i].LayerName, lsttmp);
                    }
                    else
                    {
                        List<Graphic> lsttmp = new List<Graphic>();
                        lsttmp.Add(e.IdentifyResults[i].Feature);
                        dict_result.Add(e.IdentifyResults[i].LayerName, lsttmp);
                        dict_lyid.Add(e.IdentifyResults[i].LayerName, e.IdentifyResults[i].LayerId.ToString());
                    }
                }
                for (int i = 0; i < dict_result.Count(); i++)
                {
                    strresultxml = strresultxml + "<node name=\"" + dict_result.Keys.ToList()[i] + ":" + dict_result.Values.ToList()[i].Count() + "\" value=\"" + ((ESRI.ArcGIS.Client.Tasks.TaskBase)(sender)).Url.ToString() + "/" + dict_lyid[dict_result.Keys.ToList()[i]].ToString() + "\">";
                    List<Graphic> lsttmp = dict_result.Values.ToList()[i];
                    for (int m = 0; m < lsttmp.Count(); m++)
                    {
                        if (lsttmp[m].Attributes["OBJECTID"] == null)
                        {
                            if (lsttmp[m].Attributes["序号"] == null)
                            {
                                if (lsttmp[m].Attributes["要素编号"] == null)
                                {
                                    try
                                    {
                                        strresultxml = strresultxml + "<node name=\"" + lsttmp[m].Attributes["FID"].ToString() + "\" value=\"" + ((ESRI.ArcGIS.Client.Tasks.TaskBase)(sender)).Url.ToString() + "/" + dict_lyid[dict_result.Keys.ToList()[i]].ToString() + "\" type=\"FID\"/>";
                                    }
                                    catch (Exception ex)
                                    {
                                        
                                    }
                                }
                                else
                                {
                                    strresultxml = strresultxml + "<node name=\"" + lsttmp[m].Attributes["要素编号"].ToString() + "\" value=\"" + ((ESRI.ArcGIS.Client.Tasks.TaskBase)(sender)).Url.ToString() + "/" + dict_lyid[dict_result.Keys.ToList()[i]].ToString() + "\" type=\"FID\"/>";
                                }
                            }
                            else
                            {
                                strresultxml = strresultxml + "<node name=\"" + lsttmp[m].Attributes["序号"].ToString() + "\" value=\"" + ((ESRI.ArcGIS.Client.Tasks.TaskBase)(sender)).Url.ToString() + "/" + dict_lyid[dict_result.Keys.ToList()[i]].ToString() + "\" type=\"序号\"/>";
                            }
                        }
                        else
                        {
                            strresultxml = strresultxml + "<node name=\"" + lsttmp[m].Attributes["OBJECTID"].ToString() + "\" value=\"" + ((ESRI.ArcGIS.Client.Tasks.TaskBase)(sender)).Url.ToString() + "/" + dict_lyid[dict_result.Keys.ToList()[i]].ToString() + "\" type=\"OBJECTID\"/>";
                        }

                    }
                    strresultxml = strresultxml + "</node>";
                }
                if (intqueryed == intquery)
                {
                    strresultxml = strresultxml + "</root>";
                    XElement root = XElement.Parse(strresultxml);
                    // 构造带层级关系的数据源（递归方式）
                    var result = LoadData(root);
                    layertree.DataContext = result;
                    txt_count.Text = "识别了" + intcount.ToString() + "个要素";
                    StoryboardStop();
                }                
            }
            catch (Exception ex)
            {
                StoryboardStop();
            }
            
        }

        /// <summary>
        /// 查询失败
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void identifyTask_Failed(object sender, TaskFailedEventArgs e)
        {
            Message.Show("无法识别该要素");
            StoryboardStop();
        }

        /// <summary>
        /// 关闭窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// 选择定位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            FirstGraLayer.Graphics.Clear();
            if (e.NewValue == null)
                return;
            if (((TreeViewModel)(e.NewValue)).Children.Count() == 0)
            {
                string strobjid = ((TreeViewModel)(e.NewValue)).Title;
                //if (strobjid.Contains("DEM"))
                //    return;
                string strurl = ((TreeViewModel)(e.NewValue)).Lay;
                string strtype = ((TreeViewModel)(e.NewValue)).Type;
                QueryTask queryTask = new QueryTask(strurl);
                queryTask.ExecuteCompleted += new EventHandler<QueryEventArgs>(queryTask_ExecuteCompleted);
                queryTask.Failed += new EventHandler<TaskFailedEventArgs>(identifyTask_Failed);
                ESRI.ArcGIS.Client.Tasks.Query query = new ESRI.ArcGIS.Client.Tasks.Query();
                query.OutFields.Add("*");
                //query.Where = "FID = " + strobjid;
                query.Where = strtype+" = " + strobjid;
                query.ReturnGeometry = true;
                queryTask.ExecuteAsync(query);
                StoryboardStart();
                DataValue = new Dictionary<string, string>();
            }
        }

        void queryTask_ExecuteCompleted(object sender, QueryEventArgs e)
        {
            string Aliasesfiled = "";
            if (e.FeatureSet.Features.Count < 1)
                return;
            Graphic gra = e.FeatureSet.Features[0];

            #region 定位高亮
            if (gra.Geometry.ToString().Contains("Point"))
            {
                gra.Symbol = markersymbol;
                ESRI.ArcGIS.Client.Geometry.MapPoint OldMaPoint = gra.Geometry as ESRI.ArcGIS.Client.Geometry.MapPoint;
                ESRI.ArcGIS.Client.Geometry.Envelope displayExtent = new ESRI.ArcGIS.Client.Geometry.Envelope() { XMax = OldMaPoint.X + 0.00000000001, YMax = OldMaPoint.Y + 0.00000000001, XMin = OldMaPoint.X - 0.00000000001, YMin = OldMaPoint.Y - 0.00000000001 };

                SystemMap.ZoomTo(displayExtent);
            }
            if (gra.Geometry.ToString().Contains("Polyline"))
            {
                ESRI.ArcGIS.Client.Geometry.Polyline tempgeo = new ESRI.ArcGIS.Client.Geometry.Polyline();
                tempgeo.Paths = ((ESRI.ArcGIS.Client.Geometry.Polyline)(gra.Geometry)).Paths;
                gra.Geometry = tempgeo as ESRI.ArcGIS.Client.Geometry.Polyline;

                gra.Symbol = linesymbol;
                SystemMap.Extent = gra.Geometry.Extent;
                if (SystemMap == null)
                    return;
                if (SystemMap.Layers.Count == 0)
                    return;
                SystemMap.Cursor = Cursors.Arrow;

                #region 高亮线缩小至1/6
                double wfactor = (SystemMap.Extent.XMax - SystemMap.Extent.XMin);
                double hfactor = (SystemMap.Extent.YMax - SystemMap.Extent.YMin);
                ESRI.ArcGIS.Client.Geometry.Envelope FixedZoomOutEnv = new ESRI.ArcGIS.Client.Geometry.Envelope();
                FixedZoomOutEnv.XMax = SystemMap.Extent.XMax + wfactor;
                FixedZoomOutEnv.XMin = SystemMap.Extent.XMin - wfactor;
                FixedZoomOutEnv.YMax = SystemMap.Extent.YMax + hfactor;
                FixedZoomOutEnv.YMin = SystemMap.Extent.YMin - hfactor;
                SystemMap.ZoomTo(FixedZoomOutEnv);
                #endregion
            }
            if (gra.Geometry.ToString().Contains("Polygon"))
            {
                gra.Symbol = fillsymbol;
                SystemMap.Extent = gra.Geometry.Extent;
                if (SystemMap == null)
                    return;
                if (SystemMap.Layers.Count == 0)
                    return;
                SystemMap.Cursor = Cursors.Arrow;
                #region 高亮线缩小至1/6
                double wfactor = (SystemMap.Extent.XMax - SystemMap.Extent.XMin);
                double hfactor = (SystemMap.Extent.YMax - SystemMap.Extent.YMin);
                ESRI.ArcGIS.Client.Geometry.Envelope FixedZoomOutEnv = new ESRI.ArcGIS.Client.Geometry.Envelope();
                FixedZoomOutEnv.XMax = SystemMap.Extent.XMax + wfactor;
                FixedZoomOutEnv.XMin = SystemMap.Extent.XMin - wfactor;
                FixedZoomOutEnv.YMax = SystemMap.Extent.YMax + hfactor;
                FixedZoomOutEnv.YMin = SystemMap.Extent.YMin - hfactor;
                SystemMap.ZoomTo(FixedZoomOutEnv);
                #endregion
            }
            FirstGraLayer.Graphics.Add(gra);
            #endregion

            #region 绑定datagrid
            Dictionary<string, string> tempdict = new Dictionary<string, string>();
            List<DataObject> ListData = new List<DataObject>();
            for (int i = 0; i < gra.Attributes.Count(); i++)
            {
                Aliasesfiled += e.FeatureSet.FieldAliases.Values.ToList()[i].ToString() + ",";
                if (gra.Attributes[e.FeatureSet.FieldAliases.Keys.ToList()[i]] != null && e.FeatureSet.FieldAliases.Values.ToList()[i] != null)
                {
                    DataValue.Add(e.FeatureSet.FieldAliases.Values.ToList()[i], gra.Attributes[e.FeatureSet.FieldAliases.Keys.ToList()[i]].ToString());
                }
                if (e.FeatureSet.FieldAliases.Values.ToList()[i].Contains("长度") || e.FeatureSet.FieldAliases.Values.ToList()[i].Contains("面积"))
                    continue;
                if (gra.Attributes[e.FeatureSet.FieldAliases.Keys.ToList()[i]] != null)
                {
                    ListData.Add(new DataObject() { DataName = e.FeatureSet.FieldAliases.Values.ToList()[i], DataValue = gra.Attributes[e.FeatureSet.FieldAliases.Keys.ToList()[i]].ToString() });
                }
                else
                {
                    ListData.Add(new DataObject() { DataName = e.FeatureSet.FieldAliases.Values.ToList()[i], DataValue = "" });
                }
            }
            //GetFiledName(Aliasesfiled.Substring(0, Aliasesfiled.Length - 1));
            dataresult.ItemsSource = ListData;
            StoryboardStop();
            #endregion
        }

        #region
        /// <summary>
        /// 获取配置名
        /// </summary>
        /// <param name="Filed"></param>
        //public void GetFiledName(string Filed)
        //{
        //    #region webservice调用
        //    //读取配置文件
        //    XElement xele = PFApp.Extent;
        //    var WebServicesUrl = (from item in xele.Element("WebServices").Elements("WebService")
        //                          where item.Attribute("Name").Value == "ResultField"
        //                          select new
        //                          {
        //                              Url = item.Attribute("Url").Value
        //                          }).ToList();
        //    string ServiceUrl = WebServicesUrl[0].Url;

        //    var endpointAddr = new EndpointAddress(ServiceUrl);
        //    var binding = new BasicHttpBinding();
        //    var ctor = typeof(FiledsNameServiceReference.WebService1SoapClient).GetConstructor(new Type[] { typeof(System.ServiceModel.Channels.Binding), typeof(EndpointAddress) });
        //    FiledsNameServiceReference.WebService1SoapClient client = (FiledsNameServiceReference.WebService1SoapClient)ctor.Invoke(new object[] { binding, endpointAddr });
        //    client.GetPropertyNameCompleted += new EventHandler<FiledsNameServiceReference.GetPropertyNameCompletedEventArgs>(client_GetPropertyNameCompleted);
        //    client.GetPropertyNameAsync(Filed);
        //    #endregion
        //}

        //void client_GetPropertyNameCompleted(object sender, FiledsNameServiceReference.GetPropertyNameCompletedEventArgs e)
        //{
        //    List<DataObject> ListData = new List<DataObject>();
        //    List<FiledsNameServiceReference.FiledName> Result = e.Result.ToList();
        //    for (int i = 0; i < DataValue.Count(); i++)
        //    {
        //        string FiledName = "";
        //        for (int j = 0; j < Result.Count; j++)
        //        {
        //            if (DataValue.Keys.ToList()[i] == Result[j].FieldNameEN)
        //            {
        //                FiledName = Result[j].FieldNameCN;
        //            }
        //            //else
        //            //{
        //            //    FiledName = DataValue.Keys.ToList()[i];
        //            //}
        //        }
        //        if (FiledName != "")
        //            ListData.Add(new DataObject() { DataName = FiledName, DataValue = DataValue.Values.ToList()[i] });
        //    }
        //    dataresult.ItemsSource = ListData;
        //    StoryboardStop();
        //}

        #endregion

        #region 框架事件
        /// <summary>
        /// 面板关闭方法
        /// </summary>
        public void Close()
        {
            currrentogbtn.IsChecked = false;
            Storyboard_Close.Begin();
        }

        /// <summary>
        /// 量测面板展开
        /// </summary>
        public void Show()
        {
            //展开面板
            PFApp.Root.Children.Add(this);
            Storyboard_Show.Begin();
        }


        void Storyboard_Close_Completed(object sender, EventArgs e)
        {
            txt_coor.Text = "";
            txt_count.Text = "";
            txt_layer.Text = "全部图层";
            layertree.DataContext = null;
            dataresult.ItemsSource = null;
            SystemMap.Layers.Remove(FirstGraLayer);
            if (Draw_Identify != null)
            {
                Draw_Identify.DrawComplete -= Draw_Identify_DrawComplete;
                Draw_Identify.IsEnabled = false;
                StoryboardStop();
                Draw_Identify = null;
            }
            if (this.Parent != null)
            {
                (this.Parent as Grid).Children.Remove(this);
            }
        }

        #endregion

        #region 加载动画
        /// <summary>
        /// 动画执行开始
        /// </summary>
        public void StoryboardStart()
        {
            tip_show_canvas.Visibility = Visibility.Visible;
            bigRound.Begin();
            smallRound.Begin();
            Storyboard7.Begin();
            bigRound2.Begin();
            smallRound2.Begin();
        }
        /// <summary>
        /// 动画加载结束
        /// </summary>
        public void StoryboardStop()
        {
            bigRound.Stop();
            smallRound.Stop();
            Storyboard7.Stop();
            bigRound2.Stop();
            smallRound2.Stop();
            tip_show_canvas.Visibility = Visibility.Collapsed;
        }
        #endregion

    }

    public class TreeViewModel
    {
        public string Title { get; set; }
        public string Lay { get; set; }
        public Uri Address { get; set; }
        public List<TreeViewModel> Children { get; set; }
        public string Type { get; set; }
    }

    public class DataObject
    {
        [Display(Name = "属性", GroupName = "DataObject")]
        public string DataName { get; set; }
        [Display(Name = "值", GroupName = "DataObject")]
        public string DataValue { get; set; }
    }
}
