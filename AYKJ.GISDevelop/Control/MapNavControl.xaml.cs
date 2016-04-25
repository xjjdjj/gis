using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Xml.Linq;
using AYKJ.GISDevelop.Platform;
using AYKJ.GISDevelop.Platform.Part;
using AYKJ.GISDevelop.Platform.ToolKit;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using System.Windows.Media;
using System.ServiceModel.Channels;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client.Symbols;
using System.Windows.Media.Imaging;
using System.Windows.Browser;
using System.Windows.Input;

namespace AYKJ.GISDevelop.Control
{
    public partial class MapNavControl : Page
    {
        private bool flag = true;
        #region 专题数据参数
        //加载一次性数据服务
        AykjDataService.AykjDataClient AykjClient;
        AykjRealData.Service1Client AykjRealClient;
        //配置文件
        XElement xele;
        //服务名和地址对应
        Dictionary<string, string> Dict_map;
        Dictionary<string, string> Dict_EnCn;
        Dictionary<string, string> Dict_EnType;
        //专题数据
        Dictionary<string, List<clsThematic>> dict_thematic;
        //Dict_EnCn中不存在对应关系的专题数据
        Dictionary<string, List<clsThematic>> dict_missthematic;
        //专题数据GraphicsLayer
        public static Dictionary<string, GraphicsLayer> Dict_layer;
        //加载图层的Grid
        public static StackPanel ThematicSp;
        //专题图层的显示和当时显示按钮
        public static Dictionary<CheckBox, bool> Dict_ckb;
        public static double DbScaleShow;
        #endregion
        //矢量地图
        private Map vectorMap;
        //高亮显示层
        public GraphicsLayer selectHigh_GraLayer;
        //企业详细信息页面
        GraphicsLayer ShowBusiness_Layer;
        private int intloader;
        //动画
        WaitAnimationWindow waitanimationwindow;
        #region 行政区域参数
        //定义行政区划中文字段
        string strregionname;

        //定义市域级服务地址
        string strsyurl;
        string sycode;
        string syname;
        //定义区县级服务地址
        string strqxurl;
        string qxcode;
        string qxname;
        //市域级名称列表
        Dictionary<string, string> Dict_sy;
        Dictionary<string, Graphic> Dict_sygra;
        //区县级名称列表
        Dictionary<string, string> Dict_qx;
        Dictionary<string, Graphic> Dict_qxgra;
        #endregion
        #region 网格查看参数
        string gridcode;
        string gridname;
        string subgridcode;
        string subgridname;
        string subcodefid;

        GraphicsLayer GridLayer;
        #endregion
        /// <summary>
        /// 初始化
        /// </summary>
        public MapNavControl()
        {
            InitializeComponent();
            Dict_ckb = new Dictionary<CheckBox, bool>();
            Dict_map = new Dictionary<string, string>();
            vectorMap = App.mainMap;
            //vectorMap.Background = new SolidColorBrush(new Color() { A = 255, R = 255, G = 0, B = 0 });
            vectorMap.SnapToLevels = false;
            vectorMap.IsLogoVisible = false;
            try
            {
                LayoutRoot.Children.Add(vectorMap);
            }
            catch (Exception ex)
            { }
            vectorMap.ExtentChanged += new EventHandler<ExtentEventArgs>(vectorMap_ExtentChanged);

            App.lstthematic = new List<Graphic>();
            //20120824
            App.maplayoutroot = this.LayoutRoot;

            
            ShowNorMap();
        }


        void vectorMap_ExtentChanged(object sender, ExtentEventArgs e)
        {
            if (this.flag)
            {
                flag = false;
            }
            else
            {
                base.NavigationService.Navigate
                    (new Uri(string.Format(CultureInfo.InvariantCulture,
                        "#extent={0},{1},{2},{3}",
                        new object[]
				{
					e.NewExtent.XMin,
					e.NewExtent.YMin,
					e.NewExtent.XMax,
					e.NewExtent.YMax
				}), UriKind.RelativeOrAbsolute));
            }
        }

        /// <summary>
        /// 浏览器导航
        /// </summary>
        /// <param name="e"></param>
        protected override void OnFragmentNavigation(FragmentNavigationEventArgs e)
        {
            if (e.Fragment.StartsWith("extent="))
            {
                try
                {
                    string[] array = e.Fragment.Substring(7).Split(new char[]
					{
						','
					});
                    double num = double.Parse(array[0], CultureInfo.InvariantCulture);
                    double num2 = double.Parse(array[1], CultureInfo.InvariantCulture);
                    double num3 = double.Parse(array[2], CultureInfo.InvariantCulture);
                    double num4 = double.Parse(array[3], CultureInfo.InvariantCulture);
                    Envelope extent = vectorMap.Extent;
                    Envelope envelope = new Envelope(num, num2, num3, num4);
                    if (extent == null || !MapNavControl.AreEqual(extent, envelope, 1E-09))
                    {
                        flag = true;
                        vectorMap.Extent = envelope;
                    }
                }
                catch
                {
                }
            }
            base.OnFragmentNavigation(e);
        }

        private static bool AreEqual(Envelope e1, Envelope e2, double tolerance)
        {
            return Math.Abs(e1.XMin - e2.XMin) < tolerance && Math.Abs(e1.YMin - e2.YMin) < tolerance && Math.Abs(e1.XMax - e2.XMax) < tolerance && Math.Abs(e1.YMax - e2.YMax) < tolerance;
        }

        /// <summary>
        /// 显示默认地图
        /// </summary>
        public void ShowNorMap()
        {
            this.Visibility = Visibility.Visible;
            waitanimationwindow = new WaitAnimationWindow("地图服务加载中，请稍候...");
            waitanimationwindow.Show();
            LoadVectorService();
        }

        /// <summary>
        /// 加载矢量服务
        /// </summary>
        private void LoadVectorService()
        {
            LoadService(vectorMap, "Vector");
        }

        /// <summary>
        /// 加载服务 
        /// </summary>
        /// <param name="map"></param>
        /// <param name="groupName"></param>
        private void LoadService(Map map, string groupName)
        {
            int i = 0;
            intloader = 0;
            XElement xele = PFApp.Extent;
            if (xele.Element("MapServices").Attribute("RMax").Value != "")
            {
                vectorMap.MaximumResolution = double.Parse(xele.Element("MapServices").Attribute("RMax").Value);
            }
            if (xele.Element("MapServices").Attribute("RMin").Value != "")
            {
                vectorMap.MinimumResolution = double.Parse(xele.Element("MapServices").Attribute("RMin").Value);
            }
            var mapServices = (from item in xele.Element("MapServices").Elements("MapService")
                               where item.Attribute("Group").Value == groupName
                               select new
                               {
                                   Type = item.Attribute("Type").Value,
                                   Url = item.Attribute("Url").Value,
                                   RMin = item.Attribute("RMin").Value,
                                   RMax = item.Attribute("RMax").Value,
                                   Name = item.Attribute("Name").Value,
                                   Visible = item.Attribute("IsVisible").Value,
                                   UriPattern = item.Attribute("UriPattern").Value
                               }).ToList();

            foreach (var item in mapServices)
            {
                Layer layer = null;                
                switch (item.Type)
                {
                    case "TDT":
                        XElement xele_tdt = xele.Element("MapServices");
                        var tdt = (from t in xele.Element("MapServices").Elements("MapService")
                                   where t.Attribute("Name").Value == item.Name
                                   select new
                                   {
                                       Value = t
                                   }).ToList();
                        var xele_par = tdt[0].Value.Elements("Parameter").ToList();
                        Dictionary<string, string> dict_par = new Dictionary<string, string>();
                        for (int n = 0; n < xele_par.Count; n++)
                        {
                            dict_par.Add(xele_par[n].Attribute("Name").Value, xele_par[n].Attribute("Value").Value);
                        }
                        layer = new ArcGISTiledLayerForTDT()
                        {                            
                            _baseURL = item.UriPattern,
                            _serviceMode = dict_par["ServiceMode"],
                            _imageFormat = dict_par["ImageFormat"],
                            _layerId = dict_par["LayerId"],
                            _tileMatrixSetId = dict_par["TileMatrixSetId"],
                            _initlevel = dict_par["InitLevel"],
                            _streve = dict_par["Envelope"],
                            ID = item.Name,
                            Visible = bool.Parse(item.Visible)
                        };
                        
                        break;
                    case "Baidu":
                        {
                            vectorMap.Background = new SolidColorBrush(new Color() { A = 255, R = 245, G = 243, B = 240 });
                            layer = new BaiduMapLayer()
                            {
                                UriPattern = item.UriPattern,
                                buri = true,
                                ID = item.Name,
                                Visible = bool.Parse(item.Visible)
                            };                       
                            //layer = new TiledLayerForBaidu()
                            //{
                            //    MapUrl = item.Url,
                            //    ID = item.Name,
                            //    Visible = bool.Parse(item.Visible)
                            //};
                            //layer = new ArcGISTiledLayerForBaidu()
                            //{
                            //    MapUrl = item.Url,
                            //    ID = item.Name,
                            //    Visible = bool.Parse(item.Visible)
                            //};
                        }
                        break;
                    case "Tiled":
                        {
                            layer = new ArcGISTiledMapServiceLayer()
                            {
                                Url = item.Url,
                                ID = item.Name,
                                Visible = bool.Parse(item.Visible)
                            };
                            break;
                        }
                    case "Image":
                        {
                            layer = new ArcGISImageServiceLayer()
                            {
                                Url = item.Url,
                                ID = item.Name,
                                Visible = bool.Parse(item.Visible)
                            };
                            break;
                        }
                    case "Dynamic":
                        {
                            layer = new ArcGISDynamicMapServiceLayer()
                            {
                                Url = item.Url,
                                ID = item.Name,
                                Visible = bool.Parse(item.Visible)
                            };
                            break;
                        }
                    case "Feature":
                        {
                            layer = new FeatureLayer()
                            {
                                ID = item.Name,
                                Url = item.Url,
                                Mode = FeatureLayer.QueryMode.OnDemand,
                                AutoSave = true,
                                Where = "1=1"
                            };
                            break;
                        }
                }
                Dict_map.Add(item.Name, item.Url);
                if (!string.IsNullOrEmpty(item.RMin))
                {
                    layer.MinimumResolution = double.Parse(item.RMin);
                }
                if (!string.IsNullOrEmpty(item.RMax))
                {
                    layer.MaximumResolution = double.Parse(item.RMax);
                }
                layer.InitializationFailed += new EventHandler<EventArgs>(
                    delegate(object sender, EventArgs e)
                    {
                        intloader = intloader + 1;
                        dynamic faileLayer = sender as Layer;
                        AYKJ.GISDevelop.Platform.ToolKit.Message.Show(string.Format("地图服务{0}加载出错，系统将移除该服务！", faileLayer.Url));
                        map.Layers.Remove(faileLayer);
                    });
                layer.Initialized += new EventHandler<EventArgs>(
                    delegate(object sender, EventArgs e)
                    {
                        intloader = intloader + 1;
                        if (intloader == mapServices.Count)
                        {
                            if (PFApp.MapServerType != enumMapServerType.Baidu)
                            {
                                xele = PFApp.Extent;
                                Envelope eve = new Envelope()
                                {
                                    XMax = double.Parse(xele.Element("MapExtent").Attribute("XMax").Value),
                                    XMin = double.Parse(xele.Element("MapExtent").Attribute("XMin").Value),
                                    YMax = double.Parse(xele.Element("MapExtent").Attribute("YMax").Value),
                                    YMin = double.Parse(xele.Element("MapExtent").Attribute("YMin").Value),
                                    SpatialReference = map.SpatialReference
                                };
                                map.ZoomTo(eve);
                            }
                            try
                            {
                                LoadThematicData();
                                //20131008
                                (Application.Current as IApp).strImageType = "defaultClick";
                            }
                            catch (Exception ex)
                            {
                                waitanimationwindow.Close();
                            }
                        }
                    });

                map.Layers.Insert(i++, layer);
            }
        }

        #region 加载专题数据
        void LoadThematicData()
        {
            bool bl = rtnToBussinesspage("getSysSession", "");
            string loginRole = string.Empty;
            if (bl)
            {
                HtmlElement elem = HtmlPage.Document.GetElementById("mysession");
                loginRole = elem.GetAttribute("value");
            }

            #region //20150327:江宁智慧安监项目，测试时的默认登录角色。
            if (loginRole.Equals(string.Empty))
                (Application.Current as IApp).sysRole = "ajj";
            else
                (Application.Current as IApp).sysRole = loginRole;

            #endregion

            //20120803zc:注释
            AykjClient = new AykjDataService.AykjDataClient();
            xele = PFApp.Extent;
            var dataServices = (from item in xele.Element("DataServices").Elements("DataService")
                                where item.Attribute("Name").Value == "专题数据"
                                select new
                                {
                                    Type = item.Attribute("Type").Value,
                                    Url = item.Attribute("Url").Value,
                                    Scale = item.Attribute("Scale").Value,
                                    RealUrl = item.Attribute("RealUrl").Value,
                                }).ToList();
            if (dataServices.Count == 0)
            {
                waitanimationwindow.Close();
                return;
            }
            waitanimationwindow.Change("专题数据加载中，请稍候...");
            DbScaleShow = double.Parse(dataServices[0].Scale);
            if (dataServices[0].Type == "OneTime")
            {
                BasicHttpBinding binding = new BasicHttpBinding();
                binding.MaxReceivedMessageSize = 8192000;
                AykjClient = new AykjDataService.AykjDataClient(binding, new EndpointAddress(dataServices[0].Url));
                AykjClient.GetAllDataCompleted -= AykjClient_GetAllDataCompleted;
                AykjClient.GetAllDataCompleted += new EventHandler<AykjDataService.GetAllDataCompletedEventArgs>(AykjClient_GetAllDataCompleted);
                AykjClient.GetAllDataAsync();
            }
            else if (dataServices[0].Type == "RealTime")
            {
                EndpointAddress address = new EndpointAddress(dataServices[0].RealUrl);
                PollingDuplexHttpBinding binding = new PollingDuplexHttpBinding(PollingDuplexMode.MultipleMessagesPerPoll);
                AykjRealClient = new AykjRealData.Service1Client(binding, address);
                AykjRealClient.ReceiveMessagesReceived -= AykjRealClient_ReceiveMessagesReceived;
                AykjRealClient.ReceiveMessagesReceived += new EventHandler<AykjRealData.ReceiveMessagesReceivedEventArgs>(AykjRealClient_ReceiveMessagesReceived);
                AykjRealClient.GetNewDataAsync();
            }
        }

        /// <summary>
        /// 服务器推送专题数据
        /// </summary>
        /// <param name="receivedData">专题数据</param>
        void AykjRealClient_ReceiveMessagesReceived(object sender, AykjRealData.ReceiveMessagesReceivedEventArgs e)
        {
            string strdata = e.FullIds;

            try
            {
                GetThematicData(strdata);
            }
            catch (Exception ex)
            {
                AYKJ.GISDevelop.Platform.ToolKit.Message.ShowErrorInfo("专题数据加载失败",ex.Message);
                waitanimationwindow.Close();
            }

            if (PFApp.MapServerType == enumMapServerType.Baidu)
            {
                xele = PFApp.Extent;
                Envelope eve = new Envelope()
                {
                    XMax = double.Parse(xele.Element("MapExtent").Attribute("XMax").Value),
                    XMin = double.Parse(xele.Element("MapExtent").Attribute("XMin").Value),
                    YMax = double.Parse(xele.Element("MapExtent").Attribute("YMax").Value),
                    YMin = double.Parse(xele.Element("MapExtent").Attribute("YMin").Value),
                    SpatialReference = vectorMap.SpatialReference
                };
                vectorMap.ZoomTo(eve);
            }
        }

        void AykjClient_GetAllDataCompleted(object sender, AykjDataService.GetAllDataCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                AYKJ.GISDevelop.Platform.ToolKit.Message.ShowInfo(e.Error.ToString());
                waitanimationwindow.Close();
            }
            else if (e.Result.ToString().Contains("获取数据失败"))
            {
                AYKJ.GISDevelop.Platform.ToolKit.Message.ShowErrorInfo("获取专题数据失败",e.Result);
                waitanimationwindow.Close();
            }
            else
            {
                try
                {
                    GetThematicData(e.Result);
                }
                catch (Exception ex)
                {
                    AYKJ.GISDevelop.Platform.ToolKit.Message.ShowErrorInfo("专题数据加载失败",ex.Message);
                    waitanimationwindow.Close();
                }
            }
            if (PFApp.MapServerType == enumMapServerType.Baidu)
            {
                xele = PFApp.Extent;
                Envelope eve = new Envelope()
                {
                    XMax = double.Parse(xele.Element("MapExtent").Attribute("XMax").Value),
                    XMin = double.Parse(xele.Element("MapExtent").Attribute("XMin").Value),
                    YMax = double.Parse(xele.Element("MapExtent").Attribute("YMax").Value),
                    YMin = double.Parse(xele.Element("MapExtent").Attribute("YMin").Value),
                    SpatialReference = vectorMap.SpatialReference
                };
                vectorMap.ZoomTo(eve);
            }

        }

        void GetThematicData(string strdata)
        {
            var lc = (from item in xele.Element("DataServices").Elements("Parameter")
                      where item.Attribute("Name").Value == "点位代码"
                      select new
                      {
                          value = item.Attribute("Value").Value,
                      }).ToList();
            string c = lc[0].value;
            //配置文件中没有点位信息则从数据库获取
            if (c.Trim() == "")
            {
                AykjClient.GetContrastDataAsync(strdata);                
                AykjClient.GetContrastDataCompleted += new EventHandler<AykjDataService.GetContrastDataCompletedEventArgs>(client_GetContrastDataCompleted);
            }
            else
            {
                var ln = (from item in xele.Element("DataServices").Elements("Parameter")
                          where item.Attribute("Name").Value == "点位名称"
                          select new
                          {
                              value = item.Attribute("Value").Value,
                          }).ToList();
                var lx = (from item in xele.Element("DataServices").Elements("Parameter")
                          where item.Attribute("Name").Value == "点位类型"
                          select new
                          {
                              value = item.Attribute("Value").Value,
                          }).ToList();
                string[] ary_name = ln[0].value.Split('|');
                string[] ary_code = lc[0].value.Split('|');
                string[] ary_type = lx[0].value.Split('|');
                GetDict_EnCn(ary_name, ary_code, ary_type, strdata);
            }
        }

        //从数据库获取点位代码
        void client_GetContrastDataCompleted(object sender, AykjDataService.GetContrastDataCompletedEventArgs e)
        {
            string str = "";
            if (e.Result.Contains("success"))
            {
                str = e.Result.Split(':')[1];
            }
            else 
            {
                str = "@@";
                //中英文对照信息加载失败会导致专题图层为空
                AYKJ.GISDevelop.Platform.ToolKit.Message.ShowErrorInfo("专题图层生成失败",e.Result);
            }
                
                string[] strs = str.Split('@');
                string strdata = e.UserState as string;

                #region 从配置文件读取传感器类型和名称的对应关系。
                var SensorCode = strs[0];
                IEnumerable<XElement> ln = (from item in xele.Element("DataServices").Elements("Parameter")
                                            where item.Attribute("Name").Value == "点位代码"
                                            select item);
                if (ln.Count() > 0)
                {
                    XElement xe = ln.First();
                    xe.SetAttributeValue("Value", SensorCode);
                }
                var SensorName = strs[1];
                var lc = (from item in xele.Element("DataServices").Elements("Parameter")
                          where item.Attribute("Name").Value == "点位名称"
                          select item);
                if (lc.Count() > 0)
                {
                    XElement xe = lc.First();
                    xe.SetAttributeValue("Value", SensorName);
                }
                var SensorType = strs[2];
                var lx = (from item in xele.Element("DataServices").Elements("Parameter")
                          where item.Attribute("Name").Value == "点位类型"
                          select item);
                if (lx.Count() > 0)
                {
                    XElement xe = lx.First();
                    xe.SetAttributeValue("Value", SensorType);
                }
                string[] ary_type = SensorType.Split('|');
                string[] ary_code = SensorCode.Split('|');

                string[] ary_name = SensorName.Split('|');
                //加载专题数据和其他信息
                GetDict_EnCn(ary_name, ary_code, ary_type, strdata);
                #endregion
            
        }

        /// <summary>
        /// 获取中英文对照信息并进行下一步操作
        /// </summary>
        /// <param name="ary_name">中文信息</param>
        /// <param name="ary_code">英文信息</param>
        /// <param name="strdata">专题信息</param>
        void GetDict_EnCn(string[] ary_name, string[] ary_code, string[] ary_type, string strdata)
        {
            //容器初始化
            ThematicSp = new StackPanel();

            #region 中英文对照信息
            Dict_EnCn = new Dictionary<string, string>();
            Dict_EnType = new Dictionary<string, string>();
            Dict_layer = new Dictionary<string, GraphicsLayer>();
            try
            {
                for (int ai = 0; ai < ary_code.Length && ai < ary_name.Length; ai++)
                {
                    if (ary_code[ai] != "" && ary_name[ai] != "")
                    {
                        Dict_EnCn.Add(ary_code[ai], ary_name[ai]);
                        Dict_EnType.Add(ary_code[ai], ary_type[ai]);
                        //创建图层
                        if (!Dict_layer.Keys.ToArray().Contains(ary_name[ai]))
                        {
                            GraphicsLayer tmpgl = new GraphicsLayer();
                            tmpgl.ID = ary_name[ai];
                            Dict_layer.Add(ary_name[ai], tmpgl);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AYKJ.GISDevelop.Platform.ToolKit.Message.ShowErrorInfo("专题信息配置错误",ex.Message);
            }
            App.dictthematicencn = Dict_EnCn;
            App.dictthematicentype = Dict_EnType;
            #endregion
            #region 专题信息
            dict_thematic = new Dictionary<string, List<clsThematic>>();
            dict_missthematic = new Dictionary<string, List<clsThematic>>();

            //后台正确返回必带字段success
            //兼容性保证：部分wcf返回为{"zttc":.......}
            if (strdata.Contains("success") || strdata.Contains("zttc"))
            {
                var buffer = System.Text.Encoding.UTF8.GetBytes(strdata);
                var ms = new MemoryStream(buffer);
                var jsonObject = System.Json.JsonObject.Load(ms) as System.Json.JsonObject;

                #region 解析Json
                for (int i = 0; i < jsonObject["zttc"].Count; i++)
                {
                    //如果中英文对照表中不存在此种WXYTYPE，添加到dict_missthematic
                    string tmp_wxyid = jsonObject["zttc"][i]["WXYID"].ToString().Replace("\"", "");
                    string tmp_wxytype = jsonObject["zttc"][i]["WXYTYPE"].ToString().Replace("\"", "");
                    string tmp_x = jsonObject["zttc"][i]["X"].ToString().Replace("\"", "");
                    string tmp_y = jsonObject["zttc"][i]["Y"].ToString().Replace("\"", "");
                    string tmp_dwdm = jsonObject["zttc"][i]["DWDM"].ToString().Replace("\"", "");
                    string tmp_remark = jsonObject["zttc"][i]["REMARK"].ToString().Replace("\"", "");
                    string tmp_note = jsonObject["zttc"][i]["NOTE"].ToString().Replace("\"", "");
                    string tmp_order = jsonObject["zttc"][i]["ORDER"].ToString().Replace("\"", "");
                    string tmp_watch = jsonObject["zttc"][i]["WATCH"].ToString().Replace("\"", "");
                    string tmp_street = jsonObject["zttc"][i]["STREET"].ToString().Replace("\"", "");
                    string tmp_enttype = jsonObject["zttc"][i]["ENTTYPE"].ToString().Replace("\"", "");

                    #region 正确的专题信息
                    if (Dict_EnCn.ContainsKey(tmp_wxytype))
                    {
                        //此处因为数据库中表的字段中不存在传感器类型对应的中文，将导致无法正常设置此处图层名，因此替换此处的tmp_cntype赋值方式。
                        string tmp_cntype = Dict_EnCn[tmp_wxytype];
                        #region 获取专题数据
                        List<clsThematic> lsttmp;
                        if (!dict_thematic.Keys.ToArray().Contains(tmp_cntype))
                        {

                            lsttmp = new List<clsThematic>();
                            dict_thematic.Add(tmp_cntype, lsttmp);
                        }
                        else
                        {
                            lsttmp = dict_thematic[tmp_cntype];
                        }
                        lsttmp.Add(new clsThematic()
                        {
                            str_wxyid = tmp_wxyid,
                            str_wxytype = tmp_wxytype,
                            str_x = tmp_x,
                            str_y = tmp_y,
                            str_dwdm = tmp_dwdm,
                            str_remark = tmp_remark,
                            str_note = tmp_note,
                            str_order = tmp_order,
                            str_cntype = tmp_cntype,
                            str_street = tmp_street,//江宁项目
                            str_enttype = tmp_enttype,//江宁项目
                            str_watch = tmp_watch
                        });
                        #region 将专题加到地图上去
                        GraphicsLayer tmpgl = Dict_layer[tmp_cntype];
                        tmpgl.ID = tmp_cntype;
                        Graphic tmpgra = new Graphic();
                        MapPoint mp = new MapPoint()
                        {
                            X = double.Parse(tmp_x),
                            Y = double.Parse(tmp_y),
                            SpatialReference = vectorMap.SpatialReference
                        };
                        tmpgra.Geometry = mp;
                        tmpgra.Attributes.Add("StaRemark", tmp_remark);
                        tmpgra.Attributes.Add("StaSource", "/Image/DataImages/" + tmp_wxytype + ".png");
                        tmpgra.Attributes.Add("StaTag", tmp_wxytype + "|" + tmp_wxyid + "|" + tmp_dwdm + "|" + tmp_remark + "|" + tmp_street + "|" + tmp_enttype);
                        //监测值
                        //tmpgra.Attributes.Add("StaMonitor", tmp_watch);
                        //是否报警
                        tmpgra.Attributes.Add("StaState", Visibility.Collapsed);

                        //20150327:江宁，如果是企业类型的点，要根据当前登录角色设置是否显示。
                        if (tmp_wxytype == "enterprise")
                        {
                            if ((Application.Current as IApp).sysRole == "ajj" || (Application.Current as IApp).sysRole == tmp_street)
                            {
                                //如果是安监局账户，或者当前企业点属于登录角色的管辖范围
                                tmpgra.Attributes["mVisible"] = Visibility.Visible;
                            }
                            else
                            {
                                tmpgra.Attributes["mVisible"] = Visibility.Collapsed;
                            }
                        }

                        if (tmp_wxytype == "zdwxy")
                        {
                            //20150422:添加“重大危险源”，单独样式，默认高亮。（固定的几个，不考虑新打点。）
                            tmpgra.Symbol = ThematicSymbol_WXY;
                        }
                        else
                        {
                            tmpgra.Symbol = ThematicSymbol;
                        }
                        tmpgl.Graphics.Add(tmpgra);

                        App.lstthematic.Add(tmpgra);
                        #endregion
                        #endregion


                    }
                    #endregion
                    #region 不匹配的专题信息
                    else
                    {
                        if (dict_missthematic.ContainsKey(tmp_wxytype))
                        {
                            dict_missthematic[tmp_wxytype].Add(new clsThematic()
                            {
                                str_wxyid = tmp_wxyid,
                                str_wxytype = tmp_wxytype,
                                str_x = tmp_x,
                                str_y = tmp_y,
                                str_dwdm = tmp_dwdm,
                                str_remark = tmp_remark,
                                str_note = tmp_note,
                                str_order = tmp_order,
                                str_cntype = "",
                                str_street = tmp_street,
                                str_enttype = tmp_enttype,
                                str_watch = tmp_watch
                            });
                        }
                        else
                        {
                            List<clsThematic> lsttmp = new List<clsThematic>();
                            lsttmp.Add(new clsThematic()
                            {
                                str_wxyid = tmp_wxyid,
                                str_wxytype = tmp_wxytype,
                                str_x = tmp_x,
                                str_y = tmp_y,
                                str_dwdm = tmp_dwdm,
                                str_remark = tmp_remark,
                                str_note = tmp_note,
                                str_order = tmp_order,
                                str_cntype = "",
                                str_street = tmp_street,
                                str_enttype = tmp_enttype,
                                str_watch = tmp_watch
                            });
                            dict_missthematic.Add(tmp_wxytype, lsttmp);
                        }
                    }
                    #endregion
                }
                App.dict_Thematic = dict_thematic;
                App.dict_MissThematic = dict_missthematic;
                #endregion

                #region 注册子页面信息
                //20120802zc:向子页面传动标注的所有信息
                string n = "AYKJ.GISExtension";//设置发送数据的页面名
                foreach (var item in App.Menus)
                {
                    if (item.MenuName == n)
                    {
                        IPart myPart = PFApp.UIS[n] as IPart;
                        myPart.LinkReturnGisPlatform("DataQueryKey", Dict_EnCn, dict_thematic);
                        //myPart.LinkReturnGisPlatform("DataQueryKey", jsonObject.ToString());
                    }
                }
                #endregion
            }
            else
            {
                //20130918:数据库中无数据，首次打点保证该项不能为null
                App.dict_Thematic = new Dictionary<string, List<clsThematic>>();
                App.dict_MissThematic = new Dictionary<string, List<clsThematic>>();
            }
            //中英文信息表中有，但是实际专题信息中没有值的传感器类型，补充对应图层
            //for (int bi = 0; bi < ary_name.Length; bi++)
            //{
            //    if (!Dict_layer.Keys.ToArray().Contains(ary_name[bi]))
            //    {
            //        GraphicsLayer tmpgl = new GraphicsLayer();
            //        tmpgl.ID = ary_name[bi];
            //        Dict_layer.Add(ary_name[bi], tmpgl);
            //    }
            //}
            #region 添加图层
            for (int i = vectorMap.Layers.Count() - 1; i > 0; i--)
            {
                if (ary_name.Contains(vectorMap.Layers[i].ID))
                {
                    vectorMap.Layers.RemoveAt(i);
                }
            }
            #endregion

            //20150618zc:专题数据全体控制
            StackPanel sp_all = new StackPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Orientation = Orientation.Horizontal,
                Margin= new Thickness(5),
                Background = new SolidColorBrush(Color.FromArgb(30, 246, 119, 50)),
                Width = 225,
            };
            CheckBox ckb_all = new CheckBox();
            ckb_all.FontFamily = new System.Windows.Media.FontFamily("NSimSun");
            ckb_all.FontSize = 12;
            //ckb_all.FontWeight = 
            ckb_all.Content = "全体专题图层控制";
            ckb_all.IsChecked = false;
            ckb_all.Tag = "全体专题图层控制";
            ckb_all.Checked += new RoutedEventHandler(ckb_AllChecked);
            ckb_all.Unchecked += new RoutedEventHandler(ckb_AllUnchecked);
            ckb_all.Style = this.Resources["CheckBoxStyle"] as Style;
            ckb_all.BorderBrush = null;
            ckb_all.BorderThickness = new Thickness(0);
            ckb_all.Foreground = new SolidColorBrush() { Color = Colors.Orange };
            ckb_all.HorizontalAlignment = HorizontalAlignment.Left;
            sp_all.Children.Add(ckb_all);
            ThematicSp.Children.Add(sp_all);


            //如果中英文对照信息加载失败
            //无法生成专题图层列表
            for (int i = 0; i < Dict_layer.Count(); i++)
            {
                GraphicsLayer tmpgl = Dict_layer.Values.ToArray()[i];
                vectorMap.Layers.Insert(Dict_map.Count, tmpgl);

                //将专题图层加到图层控制器上
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

                //20130918:在checkbox前显示对应的图例（类型图标）
                Image ckImg = new Image();
                string el_type = string.Empty;
                foreach (var item in Dict_EnCn)
                {
                    if (item.Value == Dict_layer.Keys.ToArray()[i])
                    {
                        el_type = item.Key;
                    }
                }
                ckImg.Source = new BitmapImage(new Uri("/Image/DataImages/" + el_type + ".png", UriKind.RelativeOrAbsolute));
                ckImg.Width = 18;
                ckImg.Height = 18;
                sp.Children.Add(ckImg);

                CheckBox ckb = new CheckBox();
                ckb.FontFamily = new System.Windows.Media.FontFamily("NSimSun");
                ckb.FontSize = 12;
                ckb.Content = Dict_layer.Keys.ToArray()[i];
                ckb.IsChecked = true;
                ckb.Tag = tmpgl;
                ckb.Checked += new RoutedEventHandler(ckb_Checked);
                ckb.Unchecked += new RoutedEventHandler(ckb_Unchecked);
                //20150615zc:修改为初始化显示重大危险源。
                if (el_type.Equals("zdwxy"))
                {
                    ckb.IsChecked = true;
                    //ckb.IsEnabled = false;
                }
                ckb.Style = this.Resources["CheckBoxStyle"] as Style;
                ckb.BorderBrush = null;
                ckb.BorderThickness = new Thickness(0);
                ckb.Foreground = new SolidColorBrush() { Color = Colors.White };

                sp.Children.Add(ckb);
                grid.Children.Add(sp);
                ThematicSp.Children.Add(grid);
                Dict_ckb.Add(ckb, ckb.IsChecked.Value);
            }

            //是否可见
            if (vectorMap.Scale >= DbScaleShow)
            {
                for (int i = 0; i < Dict_ckb.Count; i++)
                {
                    CheckBox ckb = Dict_ckb.Keys.ToList()[i];
                    //20150615zc:重大危险源不受控制
                    if (ckb.Content.Equals("重大危险源")) continue;
                    ckb.IsChecked = false;
                }
            }
            else
            {
                for (int i = 0; i < Dict_ckb.Count; i++)
                {
                    CheckBox ckb = Dict_ckb.Keys.ToList()[i];
                    //20150615zc:重大危险源不受控制
                    if (ckb.Content.Equals("重大危险源")) continue;
                    ckb.IsChecked = Dict_ckb.Values.ToList()[i];
                }
            }
            App.dict_thematiclayer = Dict_layer;
            //专题数据加载结束
            waitanimationwindow.Close();
            //加载行政区划

            rtnToBussinesspage("InitMapFinished", "success");
            
            QueryXZQH();
            #endregion
        }


        #region 控制全体专题图层的显示和隐藏
        //控制专题图层的显示和隐藏
        void ckb_AllChecked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < Dict_ckb.Count; i++)
            {
                CheckBox ckb = Dict_ckb.Keys.ToList()[i];
                //20150615zc:重大危险源不受控制
                if (ckb.Content.Equals("重大危险源")) continue;
                ckb.IsChecked = Dict_ckb.Values.ToList()[i];
            }
        }

        void ckb_AllUnchecked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < Dict_ckb.Count; i++)
            {
                CheckBox ckb = Dict_ckb.Keys.ToList()[i];
                //20150615zc:重大危险源不受控制
                if (ckb.Content.Equals("重大危险源")) continue;
                ckb.IsChecked = false;
            }
        }
        #endregion

        #region 控制专题图层的显示和隐藏
        void ckb_Unchecked(object sender, RoutedEventArgs e)
        {
            ((sender as CheckBox).Tag as GraphicsLayer).Visible = false;
            //Dict_ckb.Remove(sender as CheckBox);
            //Dict_ckb.Add(sender as CheckBox, false);
        }

        void ckb_Checked(object sender, RoutedEventArgs e)
        {
            ((sender as CheckBox).Tag as GraphicsLayer).Visible = true;
            //Dict_ckb.Remove(sender as CheckBox);
            //Dict_ckb.Add(sender as CheckBox, true);
        }
        #endregion

        #region 加载行政区域
        /// <summary>
        /// 查询行政区划
        /// </summary>
        void QueryXZQH()
        {
            try
            {

                Dict_sy = new Dictionary<string, string>();
                Dict_sygra = new Dictionary<string, Graphic>();
                Dict_qx = new Dictionary<string, string>();
                Dict_qxgra = new Dictionary<string, Graphic>();

                XElement xele = PFApp.Extent;
                var SYUrl = (from item in xele.Element("ExtensionServices").Elements("ExtensionService")
                             where item.Attribute("Name").Value == "行政区域"
                             select new
                             {
                                 Name = item.Attribute("Name").Value,
                                 Url = item.Attribute("Url").Value,
                                 XZNAME = item.Attribute("XZNAME").Value,
                                 XZCODE = item.Attribute("XZCODE").Value,
                             }).ToList();
                if (SYUrl.Count > 0)
                {
                    strsyurl = SYUrl[0].Url;
                    sycode = SYUrl[0].XZCODE;
                    syname = SYUrl[0].XZNAME;
                }
                var QXUrl = (from item in xele.Element("ExtensionServices").Elements("ExtensionService")
                             where item.Attribute("Name").Value == "行政区域区县"
                             select new
                             {
                                 Name = item.Attribute("Name").Value,
                                 Url = item.Attribute("Url").Value,
                                 XZNAME = item.Attribute("XZNAME").Value,
                                 XZCODE = item.Attribute("XZCODE").Value,
                             }).ToList();
                if (QXUrl.Count > 0)
                {
                    strqxurl = QXUrl[0].Url;
                    qxcode = QXUrl[0].XZCODE;
                    qxname = QXUrl[0].XZNAME;
                }

                if (strsyurl != null && strsyurl.Trim() != "")
                {
                    QueryTask querytask = new QueryTask(strsyurl);
                    querytask.ExecuteCompleted -= querytask_ExecuteCompleted;
                    querytask.ExecuteCompleted += new EventHandler<QueryEventArgs>(querytask_ExecuteCompleted);
                    querytask.Failed -= querytask_Failed;
                    querytask.Failed += new EventHandler<TaskFailedEventArgs>(querytask_Failed);
                    Query query = new Query();
                    query.Where = "1=1";
                    query.OutFields.Add(sycode);
                    query.OutFields.Add(syname);
                    query.ReturnGeometry = true;
                    querytask.ExecuteAsync(query, "sy");
                    waitanimationwindow = new WaitAnimationWindow("行政区划加载中，请稍候...");
                    waitanimationwindow.Show();
                }
                else
                {
                    App.dict_xzqz_qx = null;
                    App.dict_xzqz_qxgra = null;
                    App.dict_xzqz_sy = null;
                    App.dict_xzqz_sygra = null;
                    App.isXZQYFinished = false;
                    QueryGrid();
                }
            }
            catch (Exception ex)
            {
                App.dict_xzqz_qx = null;
                App.dict_xzqz_qxgra = null;
                App.dict_xzqz_sy = null;
                App.dict_xzqz_sygra = null;
                App.isXZQYFinished = false;
                AYKJ.GISDevelop.Platform.ToolKit.Message.Show("行政区划获取失败");
                QueryGrid();
            }
        }

        /// <summary>
        /// 行政区划获取成功
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void querytask_ExecuteCompleted(object sender, QueryEventArgs e)
        {
            List<Graphic> lst_tmp = e.FeatureSet.Features.ToList();
            if (e.UserState.ToString() == "sy")
            {
                for (int i = 0; i < lst_tmp.Count(); i++)
                {
                    Dict_sy.Add(lst_tmp[i].Attributes[sycode].ToString(), lst_tmp[i].Attributes[syname].ToString());
                    Dict_sygra.Add(lst_tmp[i].Attributes[sycode].ToString(), lst_tmp[i]);
                }
                if (strqxurl != null && strqxurl.Trim() != "")
                {
                    QueryTask querytask = new QueryTask(strqxurl);
                    querytask.ExecuteCompleted -= querytask_ExecuteCompleted;
                    querytask.ExecuteCompleted += new EventHandler<QueryEventArgs>(querytask_ExecuteCompleted);
                    querytask.Failed -= querytask_Failed;
                    querytask.Failed += new EventHandler<TaskFailedEventArgs>(querytask_Failed);
                    Query query = new Query();
                    query.Where = "1=1";
                    query.OutFields.Add(qxcode);
                    query.OutFields.Add(qxname);
                    query.ReturnGeometry = true;
                    querytask.ExecuteAsync(query, "qx");
                }
                else
                {
                    waitanimationwindow.Close();
                    App.dict_xzqz_qx = null;
                    App.dict_xzqz_qxgra = null;
                    App.dict_xzqz_sy = Dict_sy;
                    App.dict_xzqz_sygra = Dict_sygra;
                    App.isXZQYFinished = false;
                    QueryGrid();
                }
            }
            else
            {
                for (int i = 0; i < lst_tmp.Count(); i++)
                {
                    Dict_qx.Add(lst_tmp[i].Attributes[qxcode].ToString(), lst_tmp[i].Attributes[qxname].ToString());
                    Dict_qxgra.Add(lst_tmp[i].Attributes[qxcode].ToString(), lst_tmp[i]);
                }
                waitanimationwindow.Close();
                App.dict_xzqz_qx = Dict_qx;
                App.dict_xzqz_qxgra = Dict_qxgra;
                App.dict_xzqz_sy = Dict_sy;
                App.dict_xzqz_sygra = Dict_sygra;
                App.isXZQYFinished = true;
                QueryGrid();
            }
        }

        /// <summary>
        /// 行政区划获取失败
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void querytask_Failed(object sender, TaskFailedEventArgs e)
        {
            waitanimationwindow.Close();
            AYKJ.GISDevelop.Platform.ToolKit.Message.Show("行政区划获取失败");
        }
        #endregion

        #region 加载网格数据
        void QueryGrid()
        {
            xele = PFApp.Extent;
            var urlList = (from item in xele.Element("ExtensionServices").Elements("ExtensionService")
                           where item.Attribute("Name").Value == "网格化查询"
                           select new
                           {
                               Url = item.Attribute("Url").Value,
                               XZNAME = item.Attribute("XZNAME").Value,
                               XZCODE = item.Attribute("XZCODE").Value,
                           }).ToList();
            string queryurl = "";
            if (urlList.Count > 0)
            {
                queryurl = urlList[0].Url;
                gridcode = urlList[0].XZCODE;
                gridname = urlList[0].XZNAME;
            }
            if (queryurl.Trim() != "")
            {
                QueryTask querytask = new QueryTask(queryurl);
                Query query = new Query();
                query.ReturnGeometry = true;
                query.Where = "1=1";
                query.OutFields.Add(gridcode);
                query.OutFields.Add(gridname);
                querytask.ExecuteCompleted += new EventHandler<QueryEventArgs>(querytask_GridExecuteCompleted);
                querytask.Failed += new EventHandler<TaskFailedEventArgs>(querytask_Failed);
                waitanimationwindow = new WaitAnimationWindow("网格数据加载中，请稍后……");
                waitanimationwindow.Show();
                querytask.ExecuteAsync(query);
            }
            else
            {
                App.isGridFinished = false;
            }
        }

        void querytask_GridExecuteCompleted(object sender, QueryEventArgs e)
        {
            Dictionary<string, Graphic> dicgra = new Dictionary<string, Graphic>();
            Dictionary<string, string> dicstr = new Dictionary<string, string>();
            foreach (Graphic gc in e.FeatureSet.Features)
            {
                dicgra.Add(gc.Attributes[gridcode].ToString(), gc);
                dicstr.Add(gc.Attributes[gridcode].ToString(), gc.Attributes[gridname].ToString());
            }

            App.dict_grid_gra = dicgra;
            App.dict_grid = dicstr;

            xele = PFApp.Extent;
            var urlList = (from item in xele.Element("ExtensionServices").Elements("ExtensionService")
                           where item.Attribute("Name").Value == "网格化查询1"
                           select new
                           {
                               Url = item.Attribute("Url").Value,
                               XZNAME = item.Attribute("XZNAME").Value,
                               XZCODE = item.Attribute("XZCODE").Value,
                               FID = item.Attribute("FID").Value,
                           }).ToList();
            string queryurl = "";
            if (urlList.Count > 0)
            {
                queryurl = urlList[0].Url;
                subgridcode = urlList[0].XZCODE;
                subgridname = urlList[0].XZNAME;
                subcodefid = urlList[0].FID;
            }
            if (queryurl.Trim() != "")
            {
                QueryTask querytask = new QueryTask(queryurl);
                Query query = new Query();
                query.ReturnGeometry = true;
                query.Where = "1=1";
                query.OutFields.Add(subgridcode);
                query.OutFields.Add(subgridname);
                query.OutFields.Add(subcodefid);
                querytask.ExecuteCompleted += new EventHandler<QueryEventArgs>(querytask_SubGridExecuteCompleted);
                querytask.Failed += new EventHandler<TaskFailedEventArgs>(querytask_Failed);
                querytask.ExecuteAsync(query);
            }
            else
            {
                App.isGridFinished = false;
                waitanimationwindow.Close();
            }
        }

        void querytask_SubGridExecuteCompleted(object sender, QueryEventArgs e)
        {
            Dictionary<string, Graphic> dicgra = new Dictionary<string, Graphic>();
            Dictionary<string, string> dicstr = new Dictionary<string, string>();
            Dictionary<string, string> dicfid = new Dictionary<string, string>();
            foreach (Graphic gc in e.FeatureSet.Features)
            {
                dicgra.Add(gc.Attributes[subgridcode].ToString(), gc);
                dicstr.Add(gc.Attributes[subgridcode].ToString(), gc.Attributes[subgridname].ToString());
                dicfid.Add(gc.Attributes[subgridcode].ToString(), gc.Attributes[subcodefid].ToString());
            }

            App.dict_subgrid = dicstr;
            App.dict_subgrid_gra = dicgra;
            App.dict_subcode_fid = dicfid;
            App.isGridFinished = true;

            GridLayer = new GraphicsLayer();
            GridLayer.ID = "GridLayer";
            foreach (KeyValuePair<string, Graphic> kv in App.dict_grid_gra)
            {
                kv.Value.Symbol = new SimpleFillSymbol() { BorderThickness = 2, BorderBrush = new SolidColorBrush() { Color = new Color() { A = 255, B = 255, G = 0, R = 0 } } };
                GridLayer.Graphics.Add(kv.Value);
            }
            App.mainMap.Layers.Add(GridLayer);

            waitanimationwindow.Close();

        }
        #endregion
        #endregion

        //20120824
        private void Image_Loaded(object sender, RoutedEventArgs e)
        {
            Image img = sender as Image;
            if (App.sysListThisImg == null)
                App.sysListThisImg = new List<Image>();
            App.sysListThisImg.Add(img);
            return;
        }

        private void Image_Unloaded(object sender, RoutedEventArgs e)
        {

        }


        #region 鼠标单击图标事件
        string m_wxytype, m_wxyid, m_dwdm, m_remark;

        /// <summary>
        /// 鼠标单击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void theme_btn_click(object sender, RoutedEventArgs e)
        {
            string type = PFApp.ClickType;

            strImg = (sender as Button).Tag.ToString();

            m_wxytype = strImg.Split('|')[0];
            m_wxyid = strImg.Split('|')[1];
            m_dwdm = strImg.Split('|')[2];
            m_remark = strImg.Split('|')[3];

            //return;
            switch (type)
            {
                case "searchData"://显示详细
                    break;
                case "deleteData":

                    //20120927
                    MessageWindow msw = new MessageWindow(MsgType.Info, strImg.Split('|')[3], "删除？");
                    
                    msw.Closed -= msw_Closed;
                    msw.Closed += new EventHandler(msw_Closed);

                    msw.Show();
                    break;
                default:
                    defaultClick();
                    break;
            }
        }

        void defaultClick()
        {
            xele = PFApp.Extent;
            var dataServices = (from item in xele.Element("DataServices").Elements("DataService")
                                where item.Attribute("Name").Value == "专题数据"
                                select new
                                {
                                    Type = item.Attribute("Type").Value,
                                    Url = item.Attribute("Url").Value,
                                    Scale = item.Attribute("Scale").Value,
                                    RealUrl = item.Attribute("RealUrl").Value,
                                }).ToList();
            if (dataServices.Count == 0)
            {
                AYKJ.GISDevelop.Platform.ToolKit.Message.ShowErrorInfo("缺少数据服务", "请检查配置信息，数据服务是否为空");
                return;
            }

            if (dataServices[0].Type == "OneTime")
            {
                BasicHttpBinding binding = new BasicHttpBinding();
                binding.MaxReceivedMessageSize = 8192000;
                AykjClient = new AykjDataService.AykjDataClient(binding, new EndpointAddress(dataServices[0].Url));
                AykjClient.getDataLocationCompleted -= AykjClient_getDataLocationCompleted;
                AykjClient.getDataLocationCompleted += AykjClient_getDataLocationCompleted;
                if (m_wxytype.Equals("zdwxy"))
                {
                    //20150514zc：重大危险源也是一种企业。
                    AykjClient.getDataLocationAsync(m_wxyid, "enterprise", m_dwdm);
                }
                else
                {
                    AykjClient.getDataLocationAsync(m_wxyid, m_wxytype, m_dwdm);
                }
            }
        }


        string imgunload = "/Image/unload.jpg";
        string swfdefault = "/swf/1.swf";
        string strImg;
        void AykjClient_getDataLocationCompleted(object sender, AykjDataService.getDataLocationCompletedEventArgs e)
        {
            if (e.Result.Contains("成功"))
            {
                string[] results = e.Result.ToString().Split(',');
                MapPoint mp = new MapPoint(
                    Double.Parse(results[1]),
                    Double.Parse(results[2])
                );
                mp.SpatialReference = vectorMap.SpatialReference;
                string[] ary_str = strImg.Split('|');
                if (strImg.Contains("enterprise"))
                {
                    #region//20130926:查询企业位置，在wcf配置文件中含有独立查询语句的。
                    //"成功:查询的数据位置," + x + "," + y+","+name+","+add+","+lp+","+phone;
                    try
                    {
                        if (ShowBusiness_Layer == null)
                        {
                            ShowBusiness_Layer = new GraphicsLayer();
                            ShowBusiness_Layer.ID = "ShowBusiness_Layer";
                        }
                        else
                        {
                            ShowBusiness_Layer.Graphics.Clear();
                        }
                        bool IsExit = false;
                        foreach (Layer layer in vectorMap.Layers)
                        {
                            if (layer.ID == "ShowBusiness_Layer")
                                IsExit = true;
                        }
                        //string[] ary_identify = e.Result.ToString().Split(',')[2].Split('|');
                        Graphic tmpgra = new Graphic();
                        tmpgra.Geometry = mp;
                        tmpgra.Attributes.Add("WxySource", "/Image/DataImages/" + ary_str[0] + ".png");
                        tmpgra.Attributes.Add("StaName", results[3]);
                        tmpgra.Attributes.Add("StaAddress", results[4]);
                        tmpgra.Attributes.Add("StaPerson", results[5]);
                        tmpgra.Attributes.Add("StaPhone", results[6]);
                        var urlList = (from item in xele.Element("ShowPages").Elements("ShowPage")
                                       where item.Attribute("Name").Value == "GIS_IMAGE"
                                       select new
                                       {
                                           Url = item.Attribute("Url").Value,
                                           Name = item.Attribute("Name").Value,
                                           Default = item.Attribute("DefaultUrl").Value,
                                       }).ToList();
                        if (urlList.Count != 0)
                        {
                            string strimgurl = "";
                            string mediaurl = "";
                            //获取静态图片地址
                            var list = (from item in xele.Element("ShowVideo").Elements("Url")
                                        where item.Attribute("id").Value == ary_str[1]
                                        select new
                                        {
                                            SimulateUrl = item.Attribute("SimulateUrl").Value,
                                            VideoUrl = item.Attribute("VideoUrl").Value,
                                        }).ToList();
                            if (list.Count > 0)
                            {
                                strimgurl = list[0].SimulateUrl;
                                mediaurl = list[0].VideoUrl;
                            }

                            if (strimgurl == null || strimgurl == "")
                                strimgurl = imgunload;
                            if (mediaurl == null || mediaurl == "")
                                mediaurl = swfdefault;
                            tmpgra.Attributes.Add("PmtSource", strimgurl);

                            tmpgra.Attributes.Add("MediaSource", mediaurl);
                        }
                        //平面图按钮显示-隐藏
                        tmpgra.Attributes.Add("IsPmtShow", Visibility.Visible);
                        //视频按钮显示-隐藏
                        tmpgra.Attributes.Add("IsVideoShow", Visibility.Collapsed);
                        //模拟量按钮显示-隐藏
                        tmpgra.Attributes.Add("IsMoniShow", Visibility.Collapsed);
                        tmpgra.Symbol = BusinessSymbol;
                        Graphic tmpgra2 = new Graphic();
                        tmpgra2.Geometry = mp;
                        tmpgra2.Symbol = HighMarkerStyle;
                        ShowBusiness_Layer.Graphics.Add(tmpgra2);
                        ShowBusiness_Layer.Graphics.Add(tmpgra);
                        if (vectorMap.Layers["ShowBusiness_Layer"] == null)
                        {
                            vectorMap.Layers.Add(ShowBusiness_Layer);
                        }
                        else
                        {
                            vectorMap.Layers.Remove(ShowBusiness_Layer);
                            vectorMap.Layers.Insert(vectorMap.Layers.Count, ShowBusiness_Layer);
                        }
                    }
                    catch (Exception ex)
                    {
                        AYKJ.GISDevelop.Platform.ToolKit.Message.ShowErrorInfo("查看详细信息出错", ex.Message);
                    }
                    #endregion
                }
                else if (strImg.Contains("zdwxy"))//“重大危险源”也是一种企业。
                {
                    #region//20130926:查询企业位置，在wcf配置文件中含有独立查询语句的。
                    //"成功:查询的数据位置," + x + "," + y+","+name+","+add+","+lp+","+phone;
                    try
                    {
                        if (ShowBusiness_Layer == null)
                        {
                            ShowBusiness_Layer = new GraphicsLayer();
                            ShowBusiness_Layer.ID = "ShowBusiness_Layer";
                        }
                        else
                        {
                            ShowBusiness_Layer.Graphics.Clear();
                        }
                        bool IsExit = false;
                        foreach (Layer layer in vectorMap.Layers)
                        {
                            if (layer.ID == "ShowBusiness_Layer")
                                IsExit = true;
                        }
                        //string[] ary_identify = e.Result.ToString().Split(',')[2].Split('|');
                        Graphic tmpgra = new Graphic();
                        tmpgra.Geometry = mp;
                        tmpgra.Attributes.Add("WxySource", "/Image/DataImages/" + ary_str[0] + ".png");
                        tmpgra.Attributes.Add("StaName", results[3]);
                        tmpgra.Attributes.Add("StaAddress", results[4]);
                        tmpgra.Attributes.Add("StaPerson", results[5]);
                        tmpgra.Attributes.Add("StaPhone", results[6]);
                        var urlList = (from item in xele.Element("ShowPages").Elements("ShowPage")
                                       where item.Attribute("Name").Value == "GIS_IMAGE"
                                       select new
                                       {
                                           Url = item.Attribute("Url").Value,
                                           Name = item.Attribute("Name").Value,
                                           Default = item.Attribute("DefaultUrl").Value,
                                       }).ToList();
                        if (urlList.Count != 0)
                        {
                            string strimgurl = "";
                            string mediaurl = "";
                            //获取静态图片地址
                            var list = (from item in xele.Element("ShowVideo").Elements("Url")
                                        where item.Attribute("id").Value == ary_str[1]
                                        select new
                                        {
                                            SimulateUrl = item.Attribute("SimulateUrl").Value,
                                            VideoUrl = item.Attribute("VideoUrl").Value,
                                        }).ToList();
                            if (list.Count > 0)
                            {
                                strimgurl = list[0].SimulateUrl;
                                mediaurl = list[0].VideoUrl;
                            }

                            if (strimgurl == null || strimgurl == "")
                                strimgurl = imgunload;
                            if (mediaurl == null || mediaurl == "")
                                mediaurl = swfdefault;
                            tmpgra.Attributes.Add("PmtSource", strimgurl);

                            tmpgra.Attributes.Add("MediaSource", mediaurl);
                        }
                        //平面图按钮显示-隐藏
                        tmpgra.Attributes.Add("IsPmtShow", Visibility.Visible);
                        //视频按钮显示-隐藏
                        tmpgra.Attributes.Add("IsVideoShow", Visibility.Visible);
                        //模拟量按钮显示-隐藏
                        tmpgra.Attributes.Add("IsMoniShow", Visibility.Visible);
                        tmpgra.Symbol = BusinessSymbol;
                        Graphic tmpgra2 = new Graphic();
                        tmpgra2.Geometry = mp;
                        tmpgra2.Symbol = HighMarkerStyle;
                        ShowBusiness_Layer.Graphics.Add(tmpgra2);
                        ShowBusiness_Layer.Graphics.Add(tmpgra);
                        if (vectorMap.Layers["ShowBusiness_Layer"] == null)
                        {
                            vectorMap.Layers.Add(ShowBusiness_Layer);
                        }
                        else
                        {
                            vectorMap.Layers.Remove(ShowBusiness_Layer);
                            vectorMap.Layers.Insert(vectorMap.Layers.Count, ShowBusiness_Layer);
                        }
                    }
                    catch (Exception ex)
                    {
                        AYKJ.GISDevelop.Platform.ToolKit.Message.ShowErrorInfo("查看详细信息出错", ex.Message);
                    }
                    #endregion
                }
                else
                {
                    string result = "{'module':'sp','wxytype':'" + ary_str[0] + "','wxyid':'" + ary_str[1] + "','dwdm':'" + ary_str[2] + "'}";
                    if (!rtnToBussinesspage("openDetailInfoPage", result))
                    {
                        //20150116:配置文件中有页面链接的类型都弹出来对应的链接。
                        string t_type = strImg.Split('|')[0];
                        var all_urlList = (from item in xele.Element("ShowPages").Elements("ShowPage")
                                       select new
                                       {
                                           Url = item.Attribute("Url").Value,
                                           Name = item.Attribute("Name").Value,
                                       }).ToList();
                        foreach (char t in all_urlList[0].Name)
                        {
                            if (t.ToString() == t_type)
                            {


                                break;
                            }
                        }

                        
                        //兼容老版本，业务系统没有自己打开详细页面的方法
                        #region 其他类型
                        if (strImg.Contains("accident"))
                        {
                            var urlList = (from item in xele.Element("ShowPages").Elements("ShowPage")
                                           where item.Attribute("Name").Value == "accident"
                                           select new
                                           {
                                               Url = item.Attribute("Url").Value,
                                               Name = item.Attribute("Name").Value,
                                           }).ToList();
                            if (urlList.Count != 0)
                            {
                                OpenPage3(ary_str[1], urlList[0].Url);
                            }
                        }
                        else if (strImg.Contains("boiler"))
                        {
                            var urlList = (from item in xele.Element("ShowPages").Elements("ShowPage")
                                           where item.Attribute("Name").Value == "boiler"
                                           select new
                                           {
                                               Url = item.Attribute("Url").Value,
                                               Name = item.Attribute("Name").Value,
                                           }).ToList();
                            if (urlList.Count != 0)
                            {
                                OpenPage1(ary_str[1], "Boiler", urlList[0].Url);
                            }
                        }
                        else if (strImg.Contains("cars"))
                        {
                            var urlList = (from item in xele.Element("ShowPages").Elements("ShowPage")
                                           where item.Attribute("Name").Value == "cars"
                                           select new
                                           {
                                               Url = item.Attribute("Url").Value,
                                               Name = item.Attribute("Name").Value,
                                           }).ToList();
                            if (urlList.Count != 0)
                            {
                                OpenPage2(ary_str[1], "CarsResource", urlList[0].Url);
                            }
                        }
                        else if (strImg.Contains("colliery"))
                        {
                            var urlList = (from item in xele.Element("ShowPages").Elements("ShowPage")
                                           where item.Attribute("Name").Value == "colliery"
                                           select new
                                           {
                                               Url = item.Attribute("Url").Value,
                                               Name = item.Attribute("Name").Value,
                                           }).ToList();
                            if (urlList.Count != 0)
                            {
                                OpenPage1(ary_str[1], "CollieryHazards", urlList[0].Url);
                            }
                        }
                        else if (strImg.Contains("communication"))
                        {
                            //通讯保障 wu
                            var urlList = (from item in xele.Element("ShowPages").Elements("ShowPage")
                                           where item.Attribute("Name").Value == "communication"
                                           select new
                                           {
                                               Url = item.Attribute("Url").Value,
                                               Name = item.Attribute("Name").Value,
                                           }).ToList();
                            if (urlList.Count != 0)
                            {
                                //OpenPage(ary_str[1], ary_str[0], ary_str[2], urlList[0].Url);
                            }
                        }
                        else if (strImg.Contains("dangerousplace"))
                        {
                            var urlList = (from item in xele.Element("ShowPages").Elements("ShowPage")
                                           where item.Attribute("Name").Value == "dangerousplace"
                                           select new
                                           {
                                               Url = item.Attribute("Url").Value,
                                               Name = item.Attribute("Name").Value,
                                           }).ToList();
                            if (urlList.Count != 0)
                            {
                                OpenPage1(ary_str[1], "dangerplace", urlList[0].Url);
                            }
                        }
                        else if (strImg.Contains("expert"))
                        {
                            var urlList = (from item in xele.Element("ShowPages").Elements("ShowPage")
                                           where item.Attribute("Name").Value == "expert"
                                           select new
                                           {
                                               Url = item.Attribute("Url").Value,
                                               Name = item.Attribute("Name").Value,
                                           }).ToList();
                            if (urlList.Count != 0)
                            {
                                OpenPage2(ary_str[1], "rescueExpert", urlList[0].Url);
                            }
                        }
                        else if (strImg.Contains("gov_jy_wzck"))
                        {
                            //物资仓库
                            var urlList = (from item in xele.Element("ShowPages").Elements("ShowPage")
                                           where item.Attribute("Name").Value == "gov_jy_wzck"
                                           select new
                                           {
                                               Url = item.Attribute("Url").Value,
                                               Name = item.Attribute("Name").Value,
                                           }).ToList();
                            if (urlList.Count != 0)
                            {
                                //OpenPage(ary_str[1], ary_str[0], ary_str[2], urlList[0].Url);
                            }
                        }


                        else if (strImg.Contains("mdeical"))
                        {
                            //医疗保障
                            var urlList = (from item in xele.Element("ShowPages").Elements("ShowPage")
                                           where item.Attribute("Name").Value == "mdeical"
                                           select new
                                           {
                                               Url = item.Attribute("Url").Value,
                                               Name = item.Attribute("Name").Value,
                                           }).ToList();
                            if (urlList.Count != 0)
                            {
                                //OpenPage(ary_str[1], ary_str[0], "", urlList[0].Url);
                            }
                        }
                        else if (strImg.Contains("mines"))
                        {
                            var urlList = (from item in xele.Element("ShowPages").Elements("ShowPage")
                                           where item.Attribute("Name").Value == "mines"
                                           select new
                                           {
                                               Url = item.Attribute("Url").Value,
                                               Name = item.Attribute("Name").Value,
                                           }).ToList();
                            if (urlList.Count != 0)
                            {
                                OpenPage1(ary_str[1], "MinesHazards", urlList[0].Url);
                            }
                        }
                        else if (strImg.Contains("penstock"))
                        {
                            var urlList = (from item in xele.Element("ShowPages").Elements("ShowPage")
                                           where item.Attribute("Name").Value == "penstock"
                                           select new
                                           {
                                               Url = item.Attribute("Url").Value,
                                               Name = item.Attribute("Name").Value,
                                           }).ToList();
                            if (urlList.Count != 0)
                            {
                                OpenPage1(ary_str[1], "TbHazhardsPenstock", urlList[0].Url);
                            }
                        }
                        else if (strImg.Contains("pressurevessel"))
                        {
                            var urlList = (from item in xele.Element("ShowPages").Elements("ShowPage")
                                           where item.Attribute("Name").Value == "pressurevessel"
                                           select new
                                           {
                                               Url = item.Attribute("Url").Value,
                                               Name = item.Attribute("Name").Value,
                                           }).ToList();
                            if (urlList.Count != 0)
                            {
                                OpenPage1(ary_str[1], "PressureVesselHazards", urlList[0].Url);
                            }
                        }
                        else if (strImg.Contains("reservoirarea"))
                        {
                            var urlList = (from item in xele.Element("ShowPages").Elements("ShowPage")
                                           where item.Attribute("Name").Value == "reservoirarea"
                                           select new
                                           {
                                               Url = item.Attribute("Url").Value,
                                               Name = item.Attribute("Name").Value,
                                           }).ToList();
                            if (urlList.Count != 0)
                            {
                                OpenPage1(ary_str[1], "ReservoirAreaHazards", urlList[0].Url);
                            }
                        }
                        else if (strImg.Contains("responseteam"))
                        {
                            var urlList = (from item in xele.Element("ShowPages").Elements("ShowPage")
                                           where item.Attribute("Name").Value == "responseteam"
                                           select new
                                           {
                                               Url = item.Attribute("Url").Value,
                                               Name = item.Attribute("Name").Value,
                                           }).ToList();
                            if (urlList.Count != 0)
                            {
                                OpenPage2(ary_str[1], "rescueTeam", urlList[0].Url);
                            }
                        }
                        else if (strImg.Contains("shelter"))
                        {
                            var urlList = (from item in xele.Element("ShowPages").Elements("ShowPage")
                                           where item.Attribute("Name").Value == "shelter"
                                           select new
                                           {
                                               Url = item.Attribute("Url").Value,
                                               Name = item.Attribute("Name").Value,
                                           }).ToList();
                            if (urlList.Count != 0)
                            {
                                OpenPage2(ary_str[1], "TbRescueShelter", urlList[0].Url);
                            }
                        }
                        else if (strImg.Contains("supplies"))
                        {
                            //救援物资
                            var urlList = (from item in xele.Element("ShowPages").Elements("ShowPage")
                                           where item.Attribute("Name").Value == "supplies"
                                           select new
                                           {
                                               Url = item.Attribute("Url").Value,
                                               Name = item.Attribute("Name").Value,
                                           }).ToList();
                            if (urlList.Count != 0)
                            {
                                OpenPage(ary_str[1], ary_str[0], ary_str[2], urlList[0].Url);
                            }
                        }

                        else if (strImg.Contains("tailingspond"))
                        {
                            var urlList = (from item in xele.Element("ShowPages").Elements("ShowPage")
                                           where item.Attribute("Name").Value == "tailingspond"
                                           select new
                                           {
                                               Url = item.Attribute("Url").Value,
                                               Name = item.Attribute("Name").Value,
                                           }).ToList();
                            if (urlList.Count != 0)
                            {
                                OpenPage1(ary_str[1], "HazhardsTailingspond", urlList[0].Url);
                            }
                        }
                        else if (strImg.Contains("tank"))
                        {
                            var urlList = (from item in xele.Element("ShowPages").Elements("ShowPage")
                                           where item.Attribute("Name").Value == "tank"
                                           select new
                                           {
                                               Url = item.Attribute("Url").Value,
                                               Name = item.Attribute("Name").Value,
                                           }).ToList();
                            if (urlList.Count != 0)
                            {
                                OpenPage1(ary_str[1], "TankAraeHazards", urlList[0].Url);
                            }
                        }
                        else if (strImg.Contains("technical"))
                        {
                            //技术支持
                            var urlList = (from item in xele.Element("ShowPages").Elements("ShowPage")
                                           where item.Attribute("Name").Value == "technical"
                                           select new
                                           {
                                               Url = item.Attribute("Url").Value,
                                               Name = item.Attribute("Name").Value,
                                           }).ToList();
                            if (urlList.Count != 0)
                            {
                                OpenPage(ary_str[1], ary_str[0], ary_str[2], urlList[0].Url);
                            }
                        }
                        else if (strImg.Contains("transport"))
                        {
                            //运输保障
                            var urlList = (from item in xele.Element("ShowPages").Elements("ShowPage")
                                           where item.Attribute("Name").Value == "transport"
                                           select new
                                           {
                                               Url = item.Attribute("Url").Value,
                                               Name = item.Attribute("Name").Value,
                                           }).ToList();
                            if (urlList.Count != 0)
                            {
                                OpenPage(ary_str[1], ary_str[0], "", urlList[0].Url);
                            }
                        }
                        else if (strImg.Contains("workplace"))
                        {
                            var urlList = (from item in xele.Element("ShowPages").Elements("ShowPage")
                                           where item.Attribute("Name").Value == "workplace"
                                           select new
                                           {
                                               Url = item.Attribute("Url").Value,
                                               Name = item.Attribute("Name").Value,
                                           }).ToList();
                            if (urlList.Count != 0)
                            {
                                OpenPage1(ary_str[1], "TbHazhardsWorkplace", urlList[0].Url);
                            }
                        }
                        else if (strImg.Contains("yjgzwl"))
                        {
                            //应急机构
                            var urlList = (from item in xele.Element("ShowPages").Elements("ShowPage")
                                           where item.Attribute("Name").Value == "yjgzwl"
                                           select new
                                           {
                                               Url = item.Attribute("Url").Value,
                                               Name = item.Attribute("Name").Value,
                                           }).ToList();
                            if (urlList.Count != 0)
                            {
                                OpenPage(ary_str[1], ary_str[0], ary_str[2], urlList[0].Url);
                            }
                        }
                        #endregion
                    }
                }
            }

        }

        #region 兼容老版本，通过silverlight打开详细信息页面
         void OpenPage(string str_url)
        {
            //弹出新窗体
            HtmlPopupWindowOptions option = new HtmlPopupWindowOptions();
            option.Directories = false;//是否开启ie地址栏
            option.Height = 300;//浏览器窗口高度
            option.Width = 300;//浏览器窗口宽度
            option.Status = true;//状态栏是否可见
            option.Location = true;//是否弹出窗口
            option.Menubar = true;//菜单栏是否可见
            option.Resizeable = true;//是否可调整窗口高宽度
            option.Scrollbars = true;//滚动条是否可见
            option.Toolbar = true;//工具栏是否可见
            option.Left = option.Width / 2;//窗口的X坐标
            option.Top = option.Height / 2;//窗口的Y坐标
            HtmlPage.Window.Navigate(new Uri(str_url), "blank");
        }

        void OpenPage3(string id, string strurl)
        {
            if (id.Trim() == "" || strurl.Trim() == "")
                return;
            string str_url = strurl + "?id=" + id;
            OpenPage(str_url);
        }

        void OpenPage2(string resId, string nameSpace, string strurl)
        {
            if (resId.Trim() == "" || nameSpace.Trim() == "" || strurl.Trim() == "")
                return;
            string str_url = strurl + "?resId=" + resId + "&nameSpace=" + nameSpace;
            OpenPage(str_url);
        }

        void OpenPage1(string ids, string types, string strurl)
        {
            if (ids.Trim() == "" || types.Trim() == "" || strurl.Trim() == "")
                return;
            string str_url = strurl + "?ids=" + ids + "&types=" + types;
            OpenPage(str_url);
        }

        void OpenPage(string wxyid, string wxytype, string dwdm, string strurl)
        {
            if (wxyid.Trim() == "" || wxytype.Trim() == "" || strurl.Trim() == "")
                return;
            //弹出新窗体
            HtmlPopupWindowOptions option = new HtmlPopupWindowOptions();
            option.Directories = false;//是否开启ie地址栏
            option.Height = 300;//浏览器窗口高度
            option.Width = 300;//浏览器窗口宽度
            option.Status = true;//状态栏是否可见
            option.Location = true;//是否弹出窗口
            option.Menubar = true;//菜单栏是否可见
            option.Resizeable = true;//是否可调整窗口高宽度
            option.Scrollbars = true;//滚动条是否可见
            option.Toolbar = true;//工具栏是否可见
            option.Left = option.Width / 2;//窗口的X坐标
            option.Top = option.Height / 2;//窗口的Y坐标
            string str_url;
            if (dwdm.Trim() != "")
            {
                str_url = strurl + "?wxyid=" + wxyid + "&wxytype=" + wxytype + "&dwdm=" + dwdm;
            }
            else
            {
                str_url = strurl + "?wxyid=" + wxyid + "&wxytype=" + wxytype;
            }
            //方式1(不适用于引用外部（跨域）xap包)
            //HtmlPage.PopupWindow(new Uri(ary_url[0]), "_blank", option);
            //方式2(不适用于引用外部（跨域）xap包)
            HtmlPage.Window.Navigate(new Uri(str_url), "blank");
        }
        #endregion

        #region 企业详细信息页面内事件
        void btn_gra_close_Click(object sender, RoutedEventArgs e)
        {
            App.mainMap.Layers.Remove(ShowBusiness_Layer);
        }

        //20150618ZC
        void btn_MNL_Click(object sender, RoutedEventArgs e)
        {
            string[] ary_str = strImg.Split('|');
            string result = "{'module':'mnl','wxytype':'" + ary_str[0] + "','wxyid':'" + ary_str[1] + "','dwdm':'" + ary_str[2] + "'}";
            rtnToBussinesspage("rtnMoNiLiangFromSL", result);
        }

        void btn_JY_Click(object sender, RoutedEventArgs e)
        {
            var urlList = (from item in xele.Element("ShowPages").Elements("ShowPage")
                           where item.Attribute("Name").Value == "yuan"
                           select new
                           {
                               Url = item.Attribute("Url").Value,
                               Name = item.Attribute("Name").Value,
                           }).ToList();
            if (urlList.Count != 0)
            {
                string[] ary_str = strImg.Split('|');
                if (ary_str[2].Trim() == "")
                    return;
                //弹出新窗体
                HtmlPopupWindowOptions option = new HtmlPopupWindowOptions();
                option.Directories = false;//是否开启ie地址栏
                option.Height = 300;//浏览器窗口高度
                option.Width = 300;//浏览器窗口宽度
                option.Status = true;//状态栏是否可见
                option.Location = true;//是否弹出窗口
                option.Menubar = true;//菜单栏是否可见
                option.Resizeable = true;//是否可调整窗口高宽度
                option.Scrollbars = true;//滚动条是否可见
                option.Toolbar = true;//工具栏是否可见
                option.Left = option.Width / 2;//窗口的X坐标
                option.Top = option.Height / 2;//窗口的Y坐标
                string str_url = urlList[0].Url;

                //方式1(不适用于引用外部（跨域）xap包)
                //HtmlPage.PopupWindow(new Uri(ary_url[0]), "_blank", option);
                //方式2(不适用于引用外部（跨域）xap包)
                HtmlPage.Window.Navigate(new Uri(str_url), "blank");
            }
        }

        void btn_PMT_Click(object sender, RoutedEventArgs e)
        {
            //获取图片
            bool bl = rtnToBussinesspage("getPmtUrl", strImg);
        }

        void btn_SP_Click(object sender, RoutedEventArgs e)
        {
            string[] ary_str = strImg.Split('|');
            string result = "{'module':'sp','wxytype':'" + ary_str[0] + "','wxyid':'" + ary_str[1] + "','dwdm':'" + ary_str[2] + "'}";
            rtnToBussinesspage("rtnShiPinFromSL", result);
        }

        void btn_XX_Click(object sender, RoutedEventArgs e)
        {
            var urlList = (from item in xele.Element("ShowPages").Elements("ShowPage")
                           where item.Attribute("Name").Value == "enterprise"
                           select new
                           {
                               Url = item.Attribute("Url").Value,
                               Name = item.Attribute("Name").Value,
                           }).ToList();
            if (urlList.Count != 0)
            {
                string[] ary_str = strImg.Split('|');
                if (ary_str[2].Trim() == "")
                    return;
                //弹出新窗体
                HtmlPopupWindowOptions option = new HtmlPopupWindowOptions();
                option.Directories = false;//是否开启ie地址栏
                option.Height = 300;//浏览器窗口高度
                option.Width = 300;//浏览器窗口宽度
                option.Status = true;//状态栏是否可见
                option.Location = true;//是否弹出窗口
                option.Menubar = true;//菜单栏是否可见
                option.Resizeable = true;//是否可调整窗口高宽度
                option.Scrollbars = true;//滚动条是否可见
                option.Toolbar = true;//工具栏是否可见
                option.Left = option.Width / 2;//窗口的X坐标
                option.Top = option.Height / 2;//窗口的Y坐标
                string str_url = urlList[0].Url + "?id=" + ary_str[1];

                //方式1(不适用于引用外部（跨域）xap包)
                //HtmlPage.PopupWindow(new Uri(ary_url[0]), "_blank", option);
                //方式2(不适用于引用外部（跨域）xap包)
                HtmlPage.Window.Navigate(new Uri(str_url), "blank");
            }
        }
        #endregion


        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void msw_Closed(object sender, EventArgs e)
        {
            MessageWindow cb_msw = sender as MessageWindow;
            
            if (cb_msw.DialogResult == true)
            {
                xele = PFApp.Extent;
                var dataServices = (from item in xele.Element("DataServices").Elements("DataService")
                                    where item.Attribute("Name").Value == "专题数据"
                                    select new
                                    {
                                        Type = item.Attribute("Type").Value,
                                        Url = item.Attribute("Url").Value,
                                        Scale = item.Attribute("Scale").Value,
                                        RealUrl = item.Attribute("RealUrl").Value,
                                    }).ToList();
                if (dataServices.Count == 0)
                {
                    AYKJ.GISDevelop.Platform.ToolKit.Message.ShowErrorInfo("缺少数据服务", "请检查配置信息，数据服务是否为空");
                    return;
                }

                if (dataServices[0].Type == "OneTime")
                {
                    BasicHttpBinding binding = new BasicHttpBinding();
                    binding.MaxReceivedMessageSize = 8192000;
                    AykjClient = new AykjDataService.AykjDataClient(binding, new EndpointAddress(dataServices[0].Url));
                    AykjClient.deleteSqlDataByIdCompleted -= AykjClient_deleteSqlDataByIdCompleted;
                    AykjClient.deleteSqlDataByIdCompleted += AykjClient_deleteSqlDataByIdCompleted;
                    AykjClient.deleteSqlDataByIdAsync(m_wxyid, m_wxytype, m_dwdm);
                }               
            }
        }

        /// <summary>
        /// 删除点位信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void AykjClient_deleteSqlDataByIdCompleted(object sender, AykjDataService.deleteSqlDataByIdCompletedEventArgs e)
        {
            if (e.Result.Contains("不存在"))
            {
                AYKJ.GISDevelop.Platform.ToolKit.Message.Show("指定数据不存在");
                return;
            }

            if (e.Result.Contains("成功"))
            {
                //刷新地图显示
                try
                {
                    //删除数据所在的图层名
                    string s = (Application.Current as IApp).DictThematicEnCn[m_wxytype];
                    GraphicsLayer gl = (Application.Current as IApp).Dict_ThematicLayer[s];

                    for (int i = 0; i < gl.Graphics.Count; i++)
                    {
                        Graphic g = gl.Graphics[i];
                        string stag = g.Attributes["StaTag"].ToString();
                        if (stag.Split('|')[0] == m_wxytype && stag.Split('|')[1] == m_wxyid && stag.Split('|')[2] == m_dwdm && stag.Split('|')[3] == m_remark)
                        {
                            (Application.Current as IApp).lstThematic.Remove(g);

                            gl.Graphics.Remove(g);

                            //20131008:删除系统内存储的数据
                            List<clsThematic> ddd = (Application.Current as IApp).dict_thematic[s];
                            foreach (clsThematic cls in ddd)
                            {
                                if (cls.str_wxytype == m_wxytype
                                        && cls.str_wxyid == m_wxyid
                                        && cls.str_dwdm == m_dwdm
                                        && cls.str_remark == m_remark)
                                {
                                    (Application.Current as IApp).dict_thematic[s].Remove(cls);

                                    break;
                                }
                            }

                            break;
                        }
                    }
                    //if (selectHigh_GraLayer != null)
                    //{
                    //    selectHigh_GraLayer.Graphics.Clear();
                    //}
                }
                catch (Exception ex)
                {
                    AYKJ.GISDevelop.Platform.ToolKit.Message.Show("数据删除成功，地图没有自动刷新，请手动刷新页面。");
                    return;
                }
                AYKJ.GISDevelop.Platform.ToolKit.Message.Show("删除成功");
            }
            else
            {
                AYKJ.GISDevelop.Platform.ToolKit.Message.ShowErrorInfo("删除数据失败", e.Result);
            }
        }
        #endregion

        /// <summary>
        /// 初始右键菜单
        /// </summary>
        ContextMenu cm;
        
        /// <summary>
        /// 鼠标进入按钮的时候修改右键内容
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //新设鼠标右键事件
            cm = ContextMenuService.GetContextMenu(App.mainMap);
            ContextMenu tmp = new ContextMenu();

            MenuItem MI_ZoomOut = new MenuItem();
            MI_ZoomOut.FontFamily = new System.Windows.Media.FontFamily("NSimSun");
            MI_ZoomOut.FontSize = 12;
            MI_ZoomOut.Header = "删除数据";
            ToolTipService.SetToolTip(MI_ZoomOut, "删除数据");
            MI_ZoomOut.Cursor = Cursors.Hand;
            Image image_ZoomOut = new Image() { Width = 16, Height = 16 };
            image_ZoomOut.Source = new BitmapImage(new Uri("../Image/MapTools/删除数据.png", UriKind.Relative));
            MI_ZoomOut.Icon = image_ZoomOut;
            MI_ZoomOut.Click += new RoutedEventHandler(MI_ZoomOut_Click);
            MI_ZoomOut.Tag = (sender as Button).Tag;
            tmp.Items.Add(MI_ZoomOut);

            ContextMenuService.SetContextMenu(App.mainMap, tmp);
        }

        private void MI_ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            strImg = (sender as MenuItem).Tag.ToString();
            string[] tmp = strImg.Split('|');
            m_wxytype = tmp[0];
            m_wxyid = tmp[1];
            m_dwdm = tmp[2];
            m_remark = tmp[3];
            
            MessageWindow msw = new MessageWindow(MsgType.Info, m_remark, "删除？");
            
            msw.Closed -= msw_Closed;
            msw.Closed += new EventHandler(msw_Closed);

            msw.Show();
        }

        /// <summary>
        /// 鼠标离开按钮的时候恢复右键内容
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //还原鼠标右键事件
            ContextMenuService.SetContextMenu(App.mainMap, cm);
        }

        //将信息返回给aspx页面中的特定方法
        bool rtnToBussinesspage(string func, string mess)
        {
            ScriptObject queryfinished = HtmlPage.Window.GetProperty(func) as ScriptObject;
            if (queryfinished != null)
            {
                queryfinished.InvokeSelf(mess);
                return true;
            }
            return false;
        }
    }
}
