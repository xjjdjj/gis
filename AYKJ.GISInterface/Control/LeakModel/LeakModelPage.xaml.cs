using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Linq;
using AYKJ.GISDevelop.Platform;
using AYKJ.GISDevelop.Platform.ToolKit;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Media.Animation;

namespace AYKJ.GISInterface
{
    public partial class LeakModelPage : UserControl
    {
        //Map
        Map mainmap;
        //geometry地址
        string strGeometryurl;
        //网络分析服务地址
        string strnetworkurl;
        //路线查询地址
        string strrouteurl;
        //连续泄露扩散类
        public static clsContinuousLeak clscontinuousleak;
        //瞬时泄露扩散类
        public static clsInstantaneousLeak clsinstantaneousleak;
        //爆炸类
        public static clsExplosionLeak clsexplosionleak;
        //火灾类
        public static clsFireLeak clsfireleak;
        //最短逃生路径类
        public static clsEscapeRoute clsescaperoute;
        //逃生路径颜色（20150106：可在配置文件的路线查询内设置颜色属性）
        public static String escapeColor;
        //逃生宽度
        public static String escapeWidth;
        //逃生速度
        public static int escapeSpeed;
        //最短救援路径类
        public static clsRescueRoute clsrescueroute;
        //救援路径颜色（20150106：可在配置文件的路线查询内设置颜色属性）
        public static String rescueColor;
        //救援宽度
        public static String rescueWidth;
        //救援速度
        public static int rescueSpeed;
        //临时图层
        GraphicsLayer TemporaryLayer;
        //动画
        WaitAnimationWindow waitanimationwindow;
        //风向控件
        WindPage wc;

        public LeakModelPage()
        {
            InitializeComponent();

            mainmap = (Application.Current as IApp).MainMap;
            XElement xele = PFApp.Extent;

            strGeometryurl = xele.Element("GeometryService").Attribute("Url").Value;
            
            var NetWorkUrl = (from item in xele.Element("ExtensionServices").Elements("ExtensionService")
                              where item.Attribute("Name").Value == "网络分析"
                              select new
                              {
                                  Name = item.Attribute("Name").Value,
                                  Url = item.Attribute("Url").Value,
                              }).ToList();
            strnetworkurl = NetWorkUrl[0].Url;

            TemporaryLayer = new GraphicsLayer();
            mainmap.Layers.Add(TemporaryLayer);

            wc = new WindPage();
            wc.LeakFaildEvent += new LeakDelegate(wc_LeakFaildEvent);
            wc.LeakEvent += new LeakDelegate(wc_LeakEvent);
            clscontinuousleak = new clsContinuousLeak();
            clsinstantaneousleak = new clsInstantaneousLeak();

            clsexplosionleak = new clsExplosionLeak();
            clsexplosionleak.ExplosionLeakEvent += new ExplosionLeakDelegate(clsexplosionleak_ExplosionLeakEvent);
            clsexplosionleak.ExplosionLeakFaildEvent += new ExplosionLeakDelegate(clsexplosionleak_ExplosionLeakFaildEvent);

            clsfireleak = new clsFireLeak();
            clsfireleak.FireLeakEvent += new FireLeakDelegate(clsfireleak_FireLeakEvent);
            clsfireleak.FireLeakFaildEvent += new ExplosionLeakDelegate(clsfireleak_FireLeakFaildEvent);

            clsescaperoute = new clsEscapeRoute();
            clsescaperoute.EscapeRouteEvent += new EscapeRouteDelegate(clsescaperoute_EscapeRouteEvent);
            clsescaperoute.EscapeRouteFaildEvent += new EscapeRouteDelegate(clsescaperoute_EscapeRouteFaildEvent);
            clsrescueroute = new clsRescueRoute();
            clsrescueroute.RescueRouteEvent += new RescueRouteDelegate(clsrescueroute_RescueRouteEvent);
            clsrescueroute.RescueRouteFaildEvent += new RescueRouteDelegate(clsrescueroute_RescueRouteFaildEvent);

            this.Loaded += new RoutedEventHandler(LeakModelPage_Loaded);
        }

        void LeakModelPage_Loaded(object sender, RoutedEventArgs e)
        {
            //设置面板的起始位置
            this.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            this.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            this.Margin = new Thickness(0, 0, 0, 0);
        }

        #region //20120903:响应业务系统接口代码
        /// <summary>
        /// 对指定点的爆炸模拟
        /// </summary>
        /// <param name="sDeath">死亡</param>
        /// <param name="sSeriouslyInjured">重伤</param>
        /// <param name="sMinorInjuries">轻伤</param>
        /// <param name="sSafe">安全</param>
        /// <param name="remark">指定点信息</param>
        public void linkBtn_Explosion_Click(double sDeath, double sSeriouslyInjured, double sMinorInjuries, double sSafe, string remark)
        {
            waitanimationwindow = new WaitAnimationWindow("爆炸模拟中，请稍候...");
            waitanimationwindow.Show();
            ClearGraphic();
            Dictionary<string, double> Dict_Explosion = new Dictionary<string, double>();
            Dict_Explosion.Add("死亡", sDeath);
            Dict_Explosion.Add("重伤", sSeriouslyInjured);
            Dict_Explosion.Add("轻伤", sMinorInjuries);
            Dict_Explosion.Add("安全", sSafe);
            //clsexplosionleak.LeakCircle(mainmap, strGeometryurl, 118.095, 24.481, Dict_Explosion);

            var buffer = System.Text.Encoding.UTF8.GetBytes(remark);
            var ms = new MemoryStream(buffer);
            var jsonObject = System.Json.JsonObject.Load(ms) as System.Json.JsonObject;
            string tmp_wxyid = jsonObject["wxyid"].ToString().Replace("\"", "");
            string tmp_wxytype = jsonObject["wxytype"].ToString().Replace("\"", "");
            string tmp_dwdm = jsonObject["dwdm"].ToString().Replace("\"", "");
            string statag = tmp_wxytype + "|" + tmp_wxyid + "|" + tmp_dwdm;
            var Explosion = (from item in (Application.Current as IApp).lstThematic
                             where item.Attributes["StaTag"].ToString().Contains(statag)
                             select new
                             {
                                 Name = item.Attributes["remark"],
                                 Graphic = item,
                             }).ToList();

            if (Explosion.Count == 0)
            {
                waitanimationwindow.Close();
                Message.Show("没有查到该企业单位");
            }
            else
            {
                clsexplosionleak.LeakCircle(mainmap, strGeometryurl, Explosion[0].Graphic.Geometry.Extent.XMax, Explosion[0].Graphic.Geometry.Extent.YMax, Dict_Explosion);
            }
        }

        /// <summary>
        /// 响应业务系统对火灾模拟功能的调用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void linkBtn_Fire_Click(double sDeath, double sSeriouslyInjured, double sMinorInjuries, double sSafe, string remark)
        {
            waitanimationwindow = new WaitAnimationWindow("火灾模拟中，请稍候...");
            waitanimationwindow.Show();
            ClearGraphic();
            Dictionary<string, double> Dict_Fire = new Dictionary<string, double>();
            Dict_Fire.Add("死亡", sDeath);
            Dict_Fire.Add("重伤", sSeriouslyInjured);
            Dict_Fire.Add("轻伤", sMinorInjuries);
            Dict_Fire.Add("安全", sSafe);
            //clsfireleak.LeakCircle(mainmap, strGeometryurl, 118.095, 24.481, Dict_Fire);

            var buffer = System.Text.Encoding.UTF8.GetBytes(remark);
            var ms = new MemoryStream(buffer);
            var jsonObject = System.Json.JsonObject.Load(ms) as System.Json.JsonObject;
            string tmp_wxyid = jsonObject["wxyid"].ToString().Replace("\"", "");
            string tmp_wxytype = jsonObject["wxytype"].ToString().Replace("\"", "");
            string tmp_dwdm = jsonObject["dwdm"].ToString().Replace("\"", "");
            string statag = tmp_wxytype + "|" + tmp_wxyid + "|" + tmp_dwdm;
            var Fire = (from item in (Application.Current as IApp).lstThematic
                             where item.Attributes["StaTag"].ToString().Contains(statag)
                             select new
                             {
                                 Name = item.Attributes["remark"],
                                 Graphic = item,
                             }).ToList();

            if (Fire.Count == 0)
            {
				waitanimationwindow.Close();
                Message.Show("没有查到该企业单位");
            }
            else
            {
                clsfireleak.LeakCircle(mainmap, strGeometryurl, Fire[0].Graphic.Geometry.Extent.XMax, Fire[0].Graphic.Geometry.Extent.YMax, Dict_Fire);
            }
        }

        /// <summary>
        /// 响应业务系统对最短逃生路径功能的调用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void linkBtn_EscapeRoute_Click(List<Graphic> lstStart, List<Graphic> lstEnd)
        {
            ClearGraphic();
            waitanimationwindow = new WaitAnimationWindow("最短逃生路径计算中，请稍候...");
            waitanimationwindow.Show();

            CreateEscapePoint(lstStart, lstEnd);

            //初始化需要的服务地址
            XElement xele = PFApp.Extent;
            if (strGeometryurl == null)
            {
                strGeometryurl = (from item in PFApp.Extent.Elements("GeometryService")
                                  select item.Attribute("Url").Value).ToArray()[0];

            }
            if (strnetworkurl == null)
            {
                var NetWorkUrl = (from item in xele.Element("ExtensionServices").Elements("ExtensionService")
                                  where item.Attribute("Name").Value == "网络分析"
                                  select new
                                  {
                                      Name = item.Attribute("Name").Value,
                                      Url = item.Attribute("Url").Value,
                                  }).ToList();
                strnetworkurl = NetWorkUrl[0].Url;
            }
            if (strrouteurl == null)
            {
                var RouteUrl = (from item in xele.Element("ExtensionServices").Elements("ExtensionService")
                                where item.Attribute("Name").Value == "路线查询"
                                select new
                                {
                                    Name = item.Attribute("Name").Value,
                                    Url = item.Attribute("Url").Value,
                                    EscapeColor = item.Attribute("EscapeColor").Value,
                                    RescueColor = item.Attribute("RescueColor").Value,
                                    EscapeWidth = item.Attribute("EscapeWidth").Value,
                                    RescueWidth = item.Attribute("RescueWidth").Value,
                                    EscapeSpeed = item.Attribute("EscapeSpeed").Value,
                                    RescueSpeed = item.Attribute("RescueSpeed").Value,
                                }).ToList();
                strrouteurl = RouteUrl[0].Url;
                //20150106:获取配置文件中设置的颜色
                escapeColor = RouteUrl[0].EscapeColor;
                rescueColor = RouteUrl[0].RescueColor;
                //20150115:获取配置文件中设置的宽度
                escapeWidth = RouteUrl[0].EscapeWidth;
                rescueWidth = RouteUrl[0].RescueWidth;
                //20150115:获取配置文件中设置的速度
                escapeSpeed = int.Parse(RouteUrl[0].EscapeSpeed);
                rescueSpeed = int.Parse(RouteUrl[0].RescueSpeed);
            }
            if (mainmap == null)
            {
                mainmap = (Application.Current as IApp).MainMap;
            }
            if (PFApp.MapServerType == enumMapServerType.Baidu)
                clsescaperoute.RouteInit(mainmap, lstStart, lstEnd, null, strrouteurl, false);
            else if (strnetworkurl == null || strnetworkurl == "" || strGeometryurl == null || strGeometryurl == "")
                Message.Show("请检查Url");
            else 
                clsescaperoute.RouteInit(mainmap, lstStart, lstEnd, null, strnetworkurl, strGeometryurl, true);
        }

        /// <summary>
        /// 响应业务系统对最短救援路径的调用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void linkBtn_RescuePath_Click(List<Graphic> lstStart, List<Graphic> lstEnd)
        {
            ClearGraphic();
            waitanimationwindow = new WaitAnimationWindow("最短救援路径计算中，请稍候...");
            waitanimationwindow.Show();

            CreateRescuePoint(lstStart, lstEnd);

            //初始化需要的服务地址
            XElement xele = PFApp.Extent;
            if (strGeometryurl == null)
            {
                strGeometryurl = (from item in PFApp.Extent.Elements("GeometryService")
                                  select item.Attribute("Url").Value).ToArray()[0];

            }
            if (strnetworkurl == null)
            {
                var NetWorkUrl = (from item in xele.Element("ExtensionServices").Elements("ExtensionService")
                                  where item.Attribute("Name").Value == "网络分析"
                                  select new
                                  {
                                      Name = item.Attribute("Name").Value,
                                      Url = item.Attribute("Url").Value,
                                  }).ToList();
                strnetworkurl = NetWorkUrl[0].Url;
            }
            if (strrouteurl == null)
            {
                var RouteUrl = (from item in xele.Element("ExtensionServices").Elements("ExtensionService")
                                where item.Attribute("Name").Value == "路线查询"
                                select new
                                {
                                    Name = item.Attribute("Name").Value,
                                    Url = item.Attribute("Url").Value,
                                    EscapeColor = item.Attribute("EscapeColor").Value,
                                    RescueColor = item.Attribute("RescueColor").Value,
                                    EscapeWidth = item.Attribute("EscapeWidth").Value,
                                    RescueWidth = item.Attribute("RescueWidth").Value,
                                    EscapeSpeed = item.Attribute("EscapeSpeed").Value,
                                    RescueSpeed = item.Attribute("RescueSpeed").Value,
                                }).ToList();
                strrouteurl = RouteUrl[0].Url;
                //20150106:获取配置文件中设置的颜色
                escapeColor = RouteUrl[0].EscapeColor;
                rescueColor = RouteUrl[0].RescueColor;
                //20150115:获取配置文件中设置的宽度
                escapeWidth = RouteUrl[0].EscapeWidth;
                rescueWidth = RouteUrl[0].RescueWidth;
                //20150115:获取配置文件中设置的速度
                escapeSpeed = int.Parse(RouteUrl[0].EscapeSpeed);
                rescueSpeed = int.Parse(RouteUrl[0].RescueSpeed);
            }
            if (mainmap == null)
            {
                mainmap = (Application.Current as IApp).MainMap;
            }
            if (PFApp.MapServerType == enumMapServerType.Baidu)
                clsrescueroute.RouteInit(mainmap, lstStart, lstEnd, null, strrouteurl, false);
            else if (strnetworkurl == null || strnetworkurl == "" || strGeometryurl == null || strGeometryurl == "")
                Message.Show("请检查Url");
            else
                clsrescueroute.RouteInit(mainmap, lstStart, lstEnd, null, strnetworkurl, strGeometryurl, false);
        }

        #endregion

        #region 两侧面板的展示和关闭
        /// <summary>
        /// 量测面板展开
        /// </summary>
        public void Show()
        {
            PFApp.Root.Children.Add(this);
        }
        /// <summary>
        /// 面板关闭方法
        /// </summary>
        public void Close()
        {
            PFApp.Root.Children.Remove(this);
        }
        #endregion

        #region 泄漏模拟
        /// <summary>
        /// 连续泄漏模拟
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_ContinuousLeak_Click(object sender, RoutedEventArgs e)
        {
            #region 初始化数据
            double[] ary4 = new double[101];
            ary4[0] = 0.0;
            ary4[1] = 1.9299999999999318;
            ary4[2] = 3.6199999999998957;
            ary4[3] = 4.979999999999867;
            ary4[4] = 6.159999999999842;
            ary4[5] = 7.20999999999982;
            ary4[6] = 8.1699999999998;
            ary4[7] = 9.05999999999978;
            ary4[8] = 9.889999999999763;
            ary4[9] = 10.669999999999746;
            ary4[10] = 11.40999999999973;
            ary4[11] = 12.109999999999715;
            ary4[12] = 12.779999999999724;
            ary4[13] = 13.419999999999824;
            ary4[14] = 14.02999999999992;
            ary4[15] = 14.620000000000012;
            ary4[16] = 15.1800000000001;
            ary4[17] = 15.730000000000185;
            ary4[18] = 16.250000000000266;
            ary4[19] = 16.750000000000345;
            ary4[20] = 17.24000000000042;
            ary4[21] = 17.710000000000495;
            ary4[22] = 18.160000000000565;
            ary4[23] = 18.590000000000632;
            ary4[24] = 19.010000000000698;
            ary4[25] = 19.41000000000076;
            ary4[26] = 19.80000000000082;
            ary4[27] = 20.18000000000088;
            ary4[28] = 20.540000000000937;
            ary4[29] = 20.89000000000099;
            ary4[30] = 21.220000000001043;
            ary4[31] = 21.540000000001093;
            ary4[32] = 21.850000000001142;
            ary4[33] = 22.140000000001187;
            ary4[34] = 22.42000000000123;
            ary4[35] = 22.690000000001273;
            ary4[36] = 22.950000000001314;
            ary4[37] = 23.200000000001353;
            ary4[38] = 23.43000000000139;
            ary4[39] = 23.650000000001423;
            ary4[40] = 23.860000000001456;
            ary4[41] = 24.060000000001487;
            ary4[42] = 24.240000000001515;
            ary4[43] = 24.420000000001544;
            ary4[44] = 24.58000000000157;
            ary4[45] = 24.730000000001592;
            ary4[46] = 24.860000000001612;
            ary4[47] = 24.990000000001633;
            ary4[48] = 25.10000000000165;
            ary4[49] = 25.210000000001667;
            ary4[50] = 25.30000000000168;
            ary4[51] = 25.370000000001692;
            ary4[52] = 25.440000000001703;
            ary4[53] = 25.49000000000171;
            ary4[54] = 25.530000000001717;
            ary4[55] = 25.560000000001722;
            ary4[56] = 25.580000000001725;
            ary4[57] = 25.580000000001725;
            ary4[58] = 25.570000000001723;
            ary4[59] = 25.55000000000172;
            ary4[60] = 25.510000000001714;
            ary4[61] = 25.460000000001706;
            ary4[62] = 25.400000000001697;
            ary4[63] = 25.320000000001684;
            ary4[64] = 25.23000000000167;
            ary4[65] = 25.120000000001653;
            ary4[66] = 25.000000000001634;
            ary4[67] = 24.860000000001612;
            ary4[68] = 24.71000000000159;
            ary4[69] = 24.540000000001562;
            ary4[70] = 24.360000000001534;
            ary4[71] = 24.160000000001503;
            ary4[72] = 23.94000000000147;
            ary4[73] = 23.70000000000143;
            ary4[74] = 23.450000000001392;
            ary4[75] = 23.170000000001348;
            ary4[76] = 22.880000000001303;
            ary4[77] = 22.560000000001253;
            ary4[78] = 22.2300000000012;
            ary4[79] = 21.870000000001145;
            ary4[80] = 21.480000000001084;
            ary4[81] = 21.07000000000102;
            ary4[82] = 20.640000000000953;
            ary4[83] = 20.17000000000088;
            ary4[84] = 19.680000000000803;
            ary4[85] = 19.16000000000072;
            ary4[86] = 18.590000000000632;
            ary4[87] = 18.00000000000054;
            ary4[88] = 17.36000000000044;
            ary4[89] = 16.670000000000332;
            ary4[90] = 15.940000000000218;
            ary4[91] = 15.150000000000095;
            ary4[92] = 14.299999999999962;
            ary4[93] = 13.369999999999816;
            ary4[94] = 12.34999999999971;
            ary4[95] = 11.239999999999734;
            ary4[96] = 9.97999999999976;
            ary4[97] = 8.55999999999979;
            ary4[98] = 6.8899999999998265;
            ary4[99] = 4.819999999999871;
            ary4[100] = 0;
            #endregion

            double[] ary5 = new double[101];
            for (int i = 0; i < ary5.Length; i++)
            {
                ary5[i] = ary4[i] + 10;
            }
            ary5[100] = 0;

            CallContinuousLeak(ary4, ary5, 118.095, 24.481, 2, 4, 0, 356, 0, 1860, "0");
        }

        /// <summary>
        /// 连续泄漏方法
        /// </summary>
        /// <param name="aryCombustion">燃烧区点位</param>
        /// <param name="aryPoisoning">中毒区点位</param>
        /// <param name="dbx">起点X坐标</param>
        /// <param name="dby">起点Y坐标</param>
        /// <param name="dbCombustionStep">燃烧区步长</param>
        /// <param name="dbPoisoningStep">中毒区步长</param>
        /// <param name="dbCombustionStart">燃烧偏移量</param>
        /// <param name="CombustionArea">燃烧区面积</param>
        /// <param name="dbPoisoningStart">中毒偏移量</param>
        /// <param name="PoisoningArea">中毒区面积</param>
        /// <param name="strtype">为0则展示燃烧和中毒，1为燃烧，2为中毒</param>
        public void CallContinuousLeak(double[] aryCombustion, double[] aryPoisoning, double dbx, double dby, double dbCombustionStep,
            double dbPoisoningStep, double dbCombustionStart, double CombustionArea, double dbPoisoningStart, double PoisoningArea, string strtype)
        {
            ClearGraphic();
            wc.strleaktype = "Continuous";
            wc.LeakMap = mainmap;
            wc.strurl = strGeometryurl;
            wc.dbx = dbx;
            wc.dby = dby;
            wc.dbCombustionStep = dbCombustionStep;
            wc.aryCombustionValue = aryCombustion;
            wc.dbCombustionStart = dbCombustionStart;
            wc.dbCombustionArea = CombustionArea;
            wc.dbPoisoningStep = dbPoisoningStep;
            wc.aryPoisoningValue = aryPoisoning;
            wc.dbPoisoningStart = dbPoisoningStart;
            wc.dbPoisoningArea = PoisoningArea;
            wc.strContinuedType = strtype;
            wc.Show();
        }

        /// <summary>
        /// 瞬时泄漏模拟
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_InstantaneousLeak_Click(object sender, RoutedEventArgs e)
        {
            ClearGraphic();
            double[] ary1 = new double[5];
            ary1[0] = 160;
            ary1[1] = 200;
            ary1[2] = 240;
            ary1[3] = 200;
            ary1[4] = 280;
            double[] ary2 = new double[6];
            ary2[0] = 50;
            ary2[1] = 80;
            ary2[2] = 100;
            ary2[3] = 120;
            ary2[4] = 80;
            ary2[5] = 40;
            CallInstantaneousLeak(ary1, ary2, 118.095, 24.481);
        }

        /// <summary>
        /// 瞬时泄漏方法
        /// </summary>
        /// <param name="arysize">步长</param>
        /// <param name="aryradius">每个节点半径</param>
        /// <param name="dbx">起点X坐标</param>
        /// <param name="dby">起点Y坐标</param>
        public void CallInstantaneousLeak(double[] arysize, double[] aryradius, double dbx, double dby)
        {
            wc.strleaktype = "Instantaneous";
            wc.LeakMap = mainmap;
            wc.strurl = strGeometryurl;
            wc.dbx = dbx;
            wc.dby = dby;
            wc.arysize = arysize;
            wc.aryradius = aryradius;
            wc.Show();
        }

        /// <summary>
        /// 泄漏模拟失败
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void wc_LeakFaildEvent(object sender, EventArgs e)
        {
            if (wc.strleaktype == "Instantaneous")
            {
                Message.Show("瞬时泄漏模拟失败");
            }
            else if (wc.strleaktype == "Continuous")
            {
                Message.Show("连续泄漏模拟失败");
            }
            wc.waitanimationwindow.Close();
        }

        /// <summary>
        /// 泄漏模拟成功
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void wc_LeakEvent(object sender, EventArgs e)
        {
            List<Graphic> lsttmp = (sender as WindPage).lst_Return;
            if (wc.strleaktype == "Instantaneous")
            {
                for (int i = 0; i < lsttmp.Count; i++)
                {
                    Graphic gra = lsttmp[i];
                    SimpleFillSymbol sfs = new SimpleFillSymbol();
                    sfs.Fill = new SolidColorBrush() { Color = Colors.Red, Opacity = 0.4 };
                    sfs.BorderThickness = 0;
                    gra.Symbol = sfs;
                    TemporaryLayer.Graphics.Add(gra);
                }
            }
            else if (wc.strleaktype == "Continuous")
            {
                switch (wc.strContinuedType)
                {
                    case "0":
                        //将中毒区加载到地图上
                        Graphic Poisongratmp = (sender as WindPage).lst_Return[0];
                        Poisongratmp.Attributes.Add("StaFill", PoisonGradientBrush());
                        Poisongratmp.Symbol = ContinueSymbol;
                        TemporaryLayer.Graphics.Add(Poisongratmp);
                        //将燃烧区加载到地图上
                        Graphic firegratmp = (sender as WindPage).lst_Return[1];
                        firegratmp.Attributes.Add("StaFill", FireGradientBrush());
                        firegratmp.Symbol = ContinueSymbol;
                        TemporaryLayer.Graphics.Add(firegratmp);
                        break;
                    case "1":
                        //将燃烧区加载到地图上
                        Graphic firegra = (sender as WindPage).lst_Return[0];
                        firegra.Attributes.Add("StaFill", FireGradientBrush());
                        firegra.Symbol = ContinueSymbol;
                        TemporaryLayer.Graphics.Add(firegra);
                        break;
                    case "2":
                        //将中毒区加载到地图上
                        Graphic Poisongra = (sender as WindPage).lst_Return[0];
                        Poisongra.Attributes.Add("StaFill", PoisonGradientBrush());
                        Poisongra.Symbol = ContinueSymbol;
                        TemporaryLayer.Graphics.Add(Poisongra);
                        break;
                }
                List<Graphic> lsttxt = (sender as WindPage).lst_Txt;
                for (int i = 0; i < lsttxt.Count; i++)
                {
                    Graphic txtgra = lsttxt[i];
                    txtgra.Symbol = ConcentricCircleSymbol;
                    txtgra.SetZIndex(10);
                    TemporaryLayer.Graphics.Add(txtgra);
                }
            }

            wc.waitanimationwindow.Close();

        }

        /// <summary>
        /// 中毒渐变样式
        /// </summary>
        /// <returns></returns>
        LinearGradientBrush PoisonGradientBrush()
        {
            LinearGradientBrush lineargradientbrush = new LinearGradientBrush();
            if (wc.windDirection >= 0 && wc.windDirection < 45)
            {
                lineargradientbrush.StartPoint = new Point(0, 0);
                lineargradientbrush.EndPoint = new Point(0, 1);
            }
            else if (wc.windDirection >= 45 && wc.windDirection < 135)
            {
                lineargradientbrush.StartPoint = new Point(1, 0);
                lineargradientbrush.EndPoint = new Point(0, 0);
            }
            else if (wc.windDirection >= 135 && wc.windDirection < 225)
            {
                lineargradientbrush.StartPoint = new Point(0, 1);
                lineargradientbrush.EndPoint = new Point(0, 0);
            }
            else if (wc.windDirection >= 225 && wc.windDirection < 315)
            {
                lineargradientbrush.StartPoint = new Point(0, 1);
                lineargradientbrush.EndPoint = new Point(1, 1);
            }
            else if (wc.windDirection >= 315 && wc.windDirection < 360)
            {
                lineargradientbrush.StartPoint = new Point(0, 0);
                lineargradientbrush.EndPoint = new Point(0, 1);
            }

            GradientStop gradientstop1 = new GradientStop();
            gradientstop1.Color = new Color() { A = 150, R = 255, G = 100, B = 0 };
            gradientstop1.Offset = 0;

            GradientStop gradientstop2 = new GradientStop();
            gradientstop2.Color = new Color() { A = 150, R = 255, G = 255, B = 0 };
            gradientstop2.Offset = 1.2;

            lineargradientbrush.GradientStops.Add(gradientstop1);
            lineargradientbrush.GradientStops.Add(gradientstop2);

            return lineargradientbrush;
        }

        /// <summary>
        /// 燃烧渐变样式
        /// </summary>
        /// <returns></returns>
        LinearGradientBrush FireGradientBrush()
        {
            LinearGradientBrush lineargradientbrush = new LinearGradientBrush();
            if (wc.windDirection >= 0 && wc.windDirection < 45)
            {
                lineargradientbrush.StartPoint = new Point(0, 0);
                lineargradientbrush.EndPoint = new Point(0, 1);
            }
            else if (wc.windDirection >= 45 && wc.windDirection < 135)
            {
                lineargradientbrush.StartPoint = new Point(1, 0);
                lineargradientbrush.EndPoint = new Point(0, 0);
            }
            else if (wc.windDirection >= 135 && wc.windDirection < 225)
            {
                lineargradientbrush.StartPoint = new Point(0, 1);
                lineargradientbrush.EndPoint = new Point(0, 0);
            }
            else if (wc.windDirection >= 225 && wc.windDirection < 315)
            {
                lineargradientbrush.StartPoint = new Point(0, 1);
                lineargradientbrush.EndPoint = new Point(1, 1);
            }
            else if (wc.windDirection >= 315 && wc.windDirection < 360)
            {
                lineargradientbrush.StartPoint = new Point(0, 0);
                lineargradientbrush.EndPoint = new Point(0, 1);
            }

            GradientStop gradientstop1 = new GradientStop();
            gradientstop1.Color = new Color() { A = 150, R = 255, G = 0, B = 200 };
            gradientstop1.Offset = 0;

            GradientStop gradientstop2 = new GradientStop();
            gradientstop2.Color = new Color() { A = 150, R = 255, G = 0, B = 0 };
            gradientstop2.Offset = 1.2;

            lineargradientbrush.GradientStops.Add(gradientstop1);
            lineargradientbrush.GradientStops.Add(gradientstop2);

            return lineargradientbrush;
        }
        #endregion

        #region 爆炸模拟
        /// <summary>
        /// 爆炸模拟
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Explosion_Click(object sender, RoutedEventArgs e)
        {
            waitanimationwindow = new WaitAnimationWindow("爆炸模拟中，请稍候...");
            waitanimationwindow.Show();
            ClearGraphic();
            Dictionary<string, double> Dict_Explosion = new Dictionary<string, double>();
            Dict_Explosion.Add("死亡", 300);
            Dict_Explosion.Add("重伤", 500);
            Dict_Explosion.Add("轻伤", 700);
            Dict_Explosion.Add("安全", 800);
            //clsexplosionleak.LeakCircle(mainmap, strGeometryurl, 118.095, 24.481, Dict_Explosion);
            var Explosion = (from item in (Application.Current as IApp).lstThematic
                             where item.Attributes["StaRemark"].ToString() == "红星煤矿"
                             select new
                             {
                                 Name = item.Attributes["remark"],
                                 Graphic = item,
                             }).ToList();
            if (Explosion.Count == 0)
            {
                waitanimationwindow.Close();
                Message.Show("没有查到该企业单位");
            }
            else
            {
                clsexplosionleak.LeakCircle(mainmap, strGeometryurl, Explosion[0].Graphic.Geometry.Extent.XMax, Explosion[0].Graphic.Geometry.Extent.YMax, Dict_Explosion);
            }
        }

        void clsexplosionleak_ExplosionLeakFaildEvent(object sender, EventArgs e)
        {
            waitanimationwindow.Close();
            Message.Show("爆炸模拟出错");
        }

        /// <summary>
        /// 爆炸模拟返回结果
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clsexplosionleak_ExplosionLeakEvent(object sender, EventArgs e)
        {
            clsExplosionLeak tmp = sender as clsExplosionLeak;
            List<Color> lst_color = new List<Color>();
            lst_color.Add(new Color() { A = 255, R = 255, G = 0, B = 0 });
            lst_color.Add(new Color() { A = 255, R = 180, G = 0, B = 0 });
            lst_color.Add(new Color() { A = 255, R = 120, G = 120, B = 0 });
            lst_color.Add(new Color() { A = 255, R = 80, G = 255, B = 0 });
            #region 将返回的数据赋予样式
            for (int i = 0; i < tmp.ReturnCircleRing.Count; i++)
            {
                Graphic graCircleRing = tmp.ReturnCircleRing[i];
                SimpleFillSymbol sfsRing = new SimpleFillSymbol()
                {
                    BorderThickness = 0,
                    Fill = new SolidColorBrush()
                    {
                        Color = lst_color[i],
                        Opacity = 0.5
                    }
                };
                if (graCircleRing != null)
                {
                    graCircleRing.Symbol = sfsRing;
                    TemporaryLayer.Graphics.Add(graCircleRing);
                }
            }
            for (int i = 0; i < tmp.ReturnCircle.Count; i++)
            {
                Graphic graCircle = tmp.ReturnCircle[i];
                SimpleFillSymbol sfsCircle = new SimpleFillSymbol()
                {
                    BorderThickness = 2,
                    BorderBrush = new SolidColorBrush()
                    {
                        Color = lst_color[i],
                    }
                };
                if (graCircle != null)
                {
                    graCircle.Symbol = sfsCircle;
                    TemporaryLayer.Graphics.Add(graCircle);
                }
            }
            for (int i = 0; i < tmp.ReturnCircleTxt.Count; i++)
            {
                Graphic graCircleTxt = tmp.ReturnCircleTxt[i];
                if (graCircleTxt != null)
                {
                    graCircleTxt.Symbol = TxtSymbol;
                    graCircleTxt.SetZIndex(10);
                    TemporaryLayer.Graphics.Add(graCircleTxt);
                }
            }
            for (int i = 0; i < tmp.ReturnCircleCenter.Count; i++)
            {
                Graphic graCircleCenter = tmp.ReturnCircleCenter[i];
                graCircleCenter.Symbol = FireSymbol;
                TemporaryLayer.Graphics.Add(graCircleCenter);
            }
            #endregion
            waitanimationwindow.Close();
        }
        #endregion

        #region 火灾模拟
        /// <summary>
        /// 火灾模拟
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Fire_Click(object sender, RoutedEventArgs e)
        {
            waitanimationwindow = new WaitAnimationWindow("火灾模拟中，请稍候...");
            waitanimationwindow.Show();
            ClearGraphic();
            Dictionary<string, double> Dict_Fire = new Dictionary<string, double>();
            Dict_Fire.Add("死亡", 500);
            Dict_Fire.Add("重伤", 1000);
            Dict_Fire.Add("轻伤", 1500);
            Dict_Fire.Add("安全", 2000);
            //clsfireleak.LeakCircle(mainmap, strGeometryurl, 118.095, 24.481, Dict_Fire);
            var Fire = (from item in (Application.Current as IApp).lstThematic
                        where item.Attributes["StaRemark"].ToString() == "红星煤矿"
                        select new
                        {
                            Name = item.Attributes["remark"],
                            Graphic = item,
                        }).ToList();
            if (Fire.Count == 0)
            {
                waitanimationwindow.Close();
                Message.Show("没有查到该企业单位");
            }
            else
            {
                clsfireleak.LeakCircle(mainmap, strGeometryurl, Fire[0].Graphic.Geometry.Extent.XMax, Fire[0].Graphic.Geometry.Extent.YMax, Dict_Fire);
            }
        }

        void clsfireleak_FireLeakFaildEvent(object sender, EventArgs e)
        {
            waitanimationwindow.Close();
            Message.Show("火灾模拟出错");
        }

        /// <summary>
        /// 火灾模拟返回结果
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clsfireleak_FireLeakEvent(object sender, EventArgs e)
        {
            clsFireLeak tmp = sender as clsFireLeak;
            List<Color> lst_color = new List<Color>();
            lst_color.Add(new Color() { A = 255, R = 255, G = 0, B = 0 });
            lst_color.Add(new Color() { A = 255, R = 180, G = 0, B = 0 });
            lst_color.Add(new Color() { A = 255, R = 120, G = 120, B = 0 });
            lst_color.Add(new Color() { A = 255, R = 80, G = 255, B = 0 });
            #region 将返回的数据赋予样式
            for (int i = 0; i < tmp.ReturnCircleRing.Count; i++)
            {
                Graphic graCircleRing = tmp.ReturnCircleRing[i];
                SimpleFillSymbol sfsRing = new SimpleFillSymbol()
                {
                    BorderThickness = 0,
                    Fill = new SolidColorBrush()
                    {
                        Color = lst_color[i],
                        Opacity = 0.5
                    }
                };
                if (graCircleRing != null)
                {
                    graCircleRing.Symbol = sfsRing;
                    TemporaryLayer.Graphics.Add(graCircleRing);
                }
            }
            for (int i = 0; i < tmp.ReturnCircle.Count; i++)
            {
                Graphic graCircle = tmp.ReturnCircle[i];
                SimpleFillSymbol sfsCircle = new SimpleFillSymbol()
                {
                    BorderThickness = 2,
                    BorderBrush = new SolidColorBrush()
                    {
                        Color = lst_color[i],
                    }
                };
                if (graCircle != null)
                {
                    graCircle.Symbol = sfsCircle;
                    TemporaryLayer.Graphics.Add(graCircle);
                }
            }
            for (int i = 0; i < tmp.ReturnCircleTxt.Count; i++)
            {
                Graphic graCircleTxt = tmp.ReturnCircleTxt[i];
                if (graCircleTxt != null)
                {
                    graCircleTxt.Symbol = TxtSymbol;
                    graCircleTxt.SetZIndex(10);
                    TemporaryLayer.Graphics.Add(graCircleTxt);
                }
            }
            for (int i = 0; i < tmp.ReturnCircleCenter.Count; i++)
            {
                Graphic graCircleCenter = tmp.ReturnCircleCenter[i];
                graCircleCenter.Symbol = FireSymbol;
                TemporaryLayer.Graphics.Add(graCircleCenter);
            }
            #endregion
            waitanimationwindow.Close();
        }
        #endregion

        /// <summary>
        /// 最短逃生路径
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_EscapeRoute_Click(object sender, RoutedEventArgs e)
        {
            ClearGraphic();
            waitanimationwindow = new WaitAnimationWindow("最短逃生路径计算中，请稍候...");
            waitanimationwindow.Show();
            List<Graphic> lstStart = new List<Graphic>();
            lstStart.Add(new Graphic()
            {
                Geometry = new MapPoint()
                {
                    X = 119.274,
                    Y = 26.069,
                    SpatialReference =
                    mainmap.SpatialReference
                }
            });
            List<Graphic> lstEnd = new List<Graphic>();
            lstEnd.Add(new Graphic() { Geometry = new MapPoint() { X = 119.326, Y = 26.096, SpatialReference = mainmap.SpatialReference } });
            lstEnd.Add(new Graphic() { Geometry = new MapPoint() { X = 119.314, Y = 26.055, SpatialReference = mainmap.SpatialReference } });
            lstEnd.Add(new Graphic() { Geometry = new MapPoint() { X = 119.252, Y = 26.050, SpatialReference = mainmap.SpatialReference } });
            CreateEscapePoint(lstStart, lstEnd);
            
            clsescaperoute.RouteInit(mainmap, lstStart, lstEnd,null, strnetworkurl, strGeometryurl, true);
        }

        void clsescaperoute_EscapeRouteFaildEvent(object sender, EventArgs e)
        {
            waitanimationwindow.Close();
        }

        void clsescaperoute_EscapeRouteEvent(object sender, EventArgs e)
        {
            List<Graphic> lst = (sender as clsEscapeRoute).lst_Result;
            for (int i = 0; i < lst.Count; i++)
            {
                Graphic gra = lst[i];
                gra.Attributes.Add("mColor", escapeColor);
                gra.Attributes.Add("mWidth", escapeWidth);
                gra.Symbol = EscapeRouteLineStyle;
                TemporaryLayer.Graphics.Add(gra);


                if (escapeSpeed != 0)
                {
                    //简单样式（不用）
                    SimpleMarkerSymbol myMarkerSymbol = new SimpleMarkerSymbol();
                    myMarkerSymbol.Size = 10;
                    myMarkerSymbol.Style = SimpleMarkerSymbol.SimpleMarkerStyle.Circle;
                    myMarkerSymbol.Color = new SolidColorBrush(Color.FromArgb(220, 255, 200, 100));

                    Polyline pol = gra.Geometry as Polyline;
                    ESRI.ArcGIS.Client.Geometry.PointCollection myPointCollection0 = pol.Paths[pol.Paths.Count - 1];
                    ESRI.ArcGIS.Client.Geometry.PointCollection myPointCollection = new ESRI.ArcGIS.Client.Geometry.PointCollection();
                    myPointCollection.Add(myPointCollection0[0]);
                    myPointCollection.Add(myPointCollection0[1]);
                    for (int k = 2; k < myPointCollection0.Count - 1; k++)
                    {
                        myPointCollection.Add(myPointCollection0[k + 1]);
                        k++;
                    }

                    ArrowGraphic myArrowGraphic = new ArrowGraphic();//点对象
                    myArrowGraphic.SetZIndex(1000);
                    myArrowGraphic.Attributes.Add("mTypeImg", "/Image/DataImages/" + gra.Attributes["info-start"].ToString().Split('|')[0] + ".png");
                    myArrowGraphic.Symbol = RouteSymbol;
                    myArrowGraphic.Geometry = new ESRI.ArcGIS.Client.Geometry.MapPoint(myPointCollection[0].X, myPointCollection[0].Y);//设置起始位置
                    TemporaryLayer.Graphics.Add(myArrowGraphic);//添加到地图上

                    Storyboard myStoryboard = new Storyboard();//定义画板

                    showImg("Escape", myArrowGraphic, myPointCollection, myStoryboard, 0);
                }
            }
            waitanimationwindow.Close();
        }

        //20150109
        private void showImg(String mark, ArrowGraphic mArrowGraphic, ESRI.ArcGIS.Client.Geometry.PointCollection mPointCollection, Storyboard mStoryboard, int index)
        {
            //PointAnimationUsingPath myPointAnimationUsingPath = new PointAnimationUsingPath();

            PointAnimation mPointAnimation = new PointAnimation();
            mPointAnimation.From = new Point(mPointCollection[index].X, mPointCollection[index].Y);
            mPointAnimation.To = new Point(mPointCollection[index + 1].X, mPointCollection[index + 1].Y);
            mPointAnimation.Duration = new Duration(TimeSpan.FromSeconds(5));
            mPointAnimation.RepeatBehavior = RepeatBehavior.Forever;

            //
            PointAnimationUsingKeyFrames mPointAnimationUsingKeyFrames = new PointAnimationUsingKeyFrames();
            //mPointAnimationUsingKeyFrames.Duration = TimeSpan.FromSeconds(7);
            mPointAnimationUsingKeyFrames.RepeatBehavior = RepeatBehavior.Forever;

            BaiduMeature baiduMeature = new BaiduMeature();

            int speed = 80;//速度(km/h)
            if (mark == "Escape")
                speed = escapeSpeed;
            else
            {
                speed = rescueSpeed;
            }

            double t = 0.0;//初始时间

            for (int k = 0; k < mPointCollection.Count; k++)
            {
                LinearPointKeyFrame mLinearPointKeyFrame = new LinearPointKeyFrame();
                if (k == 0)
                {
                    mLinearPointKeyFrame.KeyTime = TimeSpan.FromSeconds(0);
                }
                else
                {
                    double dis = baiduMeature.GetDistance(mPointCollection[k - 1], mPointCollection[k]);
                    t += Math.Floor(dis / (speed * 1000 / 3600));
                    mLinearPointKeyFrame.KeyTime = TimeSpan.FromSeconds(t);
                }
                mLinearPointKeyFrame.Value = new Point(mPointCollection[k].X, mPointCollection[k].Y);
                mPointAnimationUsingKeyFrames.KeyFrames.Add(mLinearPointKeyFrame);
            }
            //LinearPointKeyFrame mLinearPointKeyFrame1 = new LinearPointKeyFrame();
            //mLinearPointKeyFrame1.KeyTime = TimeSpan.FromSeconds(1);
            //mLinearPointKeyFrame1.Value = new Point(mPointCollection[index].X, mPointCollection[index].Y);
            //mPointAnimationUsingKeyFrames.KeyFrames.Add(mLinearPointKeyFrame1);

            //LinearPointKeyFrame mLinearPointKeyFrame2 = new LinearPointKeyFrame();
            //mLinearPointKeyFrame2.KeyTime = TimeSpan.FromSeconds(5);
            //mLinearPointKeyFrame2.Value = new Point(mPointCollection[index + 1].X, mPointCollection[index + 1].Y);
            //mPointAnimationUsingKeyFrames.KeyFrames.Add(mLinearPointKeyFrame2);

            //LinearPointKeyFrame mLinearPointKeyFrame3 = new LinearPointKeyFrame();
            //mLinearPointKeyFrame3.KeyTime = TimeSpan.FromSeconds(15);
            //mLinearPointKeyFrame3.Value = new Point(mPointCollection[index + 3].X, mPointCollection[index + 3].Y);
            //mPointAnimationUsingKeyFrames.KeyFrames.Add(mLinearPointKeyFrame3);

            //
            mStoryboard.Children.Add(mPointAnimationUsingKeyFrames);//把动画加到画板上                    
            Storyboard.SetTargetProperty(mStoryboard, new PropertyPath(ArrowGraphic.PointProperty));//关联动画对象和运动点的一个属性


            Storyboard.SetTarget(mStoryboard, mArrowGraphic);//关联画板和要运行的对象
            mStoryboard.Begin();//启动动画
        }

        public class ArrowGraphic : Graphic
        {
            public Point Point
            {
                get { return (Point)GetValue(PointProperty); }
                set { SetValue(PointProperty, value); }
            }

            public static readonly DependencyProperty PointProperty = DependencyProperty.Register
                ("Point", typeof(Point), typeof(ArrowGraphic), new PropertyMetadata(OnPointChanged));

            public static void OnPointChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            {
                ArrowGraphic myArrowGraphic = d as ArrowGraphic;
                MapPoint myMapPoint = new MapPoint(myArrowGraphic.Point.X, myArrowGraphic.Point.Y);
                myArrowGraphic.Geometry = myMapPoint;
            }

            public ArrowGraphic()
            {
                SimpleMarkerSymbol myMarkerSymbol = new SimpleMarkerSymbol();
                myMarkerSymbol.Size = 10;
                myMarkerSymbol.Style = SimpleMarkerSymbol.SimpleMarkerStyle.Circle;
                myMarkerSymbol.Color = new SolidColorBrush(Color.FromArgb(220, 255, 200, 100));
                this.Symbol = myMarkerSymbol;
            }
        }

        /// <summary>
        /// 最短救援路径
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_RescuePath_Click(object sender, RoutedEventArgs e)
        {
            ClearGraphic();
            waitanimationwindow = new WaitAnimationWindow("最短救援路径计算中，请稍候...");
            waitanimationwindow.Show();
            List<Graphic> lstStart = new List<Graphic>();
            lstStart.Add(new Graphic() { Geometry = new MapPoint() { X = 119.326, Y = 26.096, SpatialReference = mainmap.SpatialReference } });
            lstStart.Add(new Graphic() { Geometry = new MapPoint() { X = 119.314, Y = 26.055, SpatialReference = mainmap.SpatialReference } });
            lstStart.Add(new Graphic() { Geometry = new MapPoint() { X = 119.252, Y = 26.050, SpatialReference = mainmap.SpatialReference } });
            List<Graphic> lstEnd = new List<Graphic>();
            lstEnd.Add(new Graphic()
            {
                Geometry = new MapPoint()
                {
                    X = 119.274,
                    Y = 26.069,
                    SpatialReference =
                    mainmap.SpatialReference
                }
            });
            CreateRescuePoint(lstStart, lstEnd);
            clsrescueroute.RouteInit(mainmap, lstStart, lstEnd,null, strnetworkurl, strGeometryurl, false);
        }

        void clsrescueroute_RescueRouteFaildEvent(object sender, EventArgs e)
        {
            waitanimationwindow.Close();
        }

        void clsrescueroute_RescueRouteEvent(object sender, EventArgs e)
        {
            List<Graphic> lst = (sender as clsRescueRoute).lst_Result;
            for (int i = 0; i < lst.Count; i++)
            {
                Graphic gra = lst[i];
                gra.Attributes.Add("mColor", rescueColor);
                gra.Attributes.Add("mWidth", rescueWidth);
                gra.Symbol = RescueRouteLineStyle;
                TemporaryLayer.Graphics.Add(gra);


                if (rescueSpeed != 0)
                {
                    //简单样式（不用）
                    SimpleMarkerSymbol myMarkerSymbol = new SimpleMarkerSymbol();
                    myMarkerSymbol.Size = 10;
                    myMarkerSymbol.Style = SimpleMarkerSymbol.SimpleMarkerStyle.Circle;
                    myMarkerSymbol.Color = new SolidColorBrush(Color.FromArgb(220, 255, 200, 100));

                    Polyline pol = gra.Geometry as Polyline;
                    ESRI.ArcGIS.Client.Geometry.PointCollection myPointCollection0 = pol.Paths[pol.Paths.Count - 1];
                    ESRI.ArcGIS.Client.Geometry.PointCollection myPointCollection = new ESRI.ArcGIS.Client.Geometry.PointCollection();
                    myPointCollection.Add(myPointCollection0[0]);
                    myPointCollection.Add(myPointCollection0[1]);
                    for (int k = 2; k < myPointCollection0.Count - 1; k++)
                    {
                        myPointCollection.Add(myPointCollection0[k + 1]);
                        k++;
                    }

                    ArrowGraphic myArrowGraphic = new ArrowGraphic();//点对象
                    myArrowGraphic.SetZIndex(1000);
                    myArrowGraphic.Attributes.Add("mTypeImg", "/Image/DataImages/" + gra.Attributes["info-start"].ToString().Split('|')[0] + ".png");
                    myArrowGraphic.Symbol = RouteSymbol;
                    myArrowGraphic.Geometry = new ESRI.ArcGIS.Client.Geometry.MapPoint(myPointCollection[0].X, myPointCollection[0].Y);//设置起始位置
                    TemporaryLayer.Graphics.Add(myArrowGraphic);//添加到地图上

                    Storyboard myStoryboard = new Storyboard();//定义画板

                    showImg("Rescue", myArrowGraphic, myPointCollection, myStoryboard, 0);
                }
            }
            waitanimationwindow.Close();
        }

        /// <summary>
        /// 创建点位
        /// </summary>
        /// <param name="lstStart"></param>
        /// <param name="lstEnd"></param>
        void CreateRescuePoint(List<Graphic> lstStart, List<Graphic> lstEnd)
        {
            PictureMarkerSymbol start_symbol = new PictureMarkerSymbol()
            {
                OffsetX = 15,
                OffsetY = 34,
                Source = new BitmapImage(new Uri("/Image/LeakModel/RescueStart.png", UriKind.Relative))
            };
            PictureMarkerSymbol end_symbol = new PictureMarkerSymbol()
            {
                OffsetX = 15,
                OffsetY = 34,
                Source = new BitmapImage(new Uri("/Image/LeakModel/RescueFinish.png", UriKind.Relative))
            };
            foreach (Graphic tmp in lstStart)
            {
                Graphic gra = new Graphic();
                gra.Geometry = tmp.Geometry;
                gra.Symbol = start_symbol;
                TemporaryLayer.Graphics.Add(gra);
            }
            foreach (Graphic tmp in lstEnd)
            {
                Graphic gra = new Graphic();
                gra.Geometry = tmp.Geometry;
                gra.Symbol = end_symbol;
                TemporaryLayer.Graphics.Add(gra);
            }
        }

        /// <summary>
        /// 创建点位
        /// </summary>
        /// <param name="lstStart"></param>
        /// <param name="lstEnd"></param>
        void CreateEscapePoint(List<Graphic> lstStart, List<Graphic> lstEnd)
        {
            PictureMarkerSymbol start_symbol = new PictureMarkerSymbol()
            {
                OffsetX = 15,
                OffsetY = 34,
                Source = new BitmapImage(new Uri("/Image/LeakModel/EscapeStart.png", UriKind.Relative))
            };
            PictureMarkerSymbol end_symbol = new PictureMarkerSymbol()
            {
                OffsetX = 15,
                OffsetY = 34,
                Source = new BitmapImage(new Uri("/Image/LeakModel/EscapeFinish.png", UriKind.Relative))
            };
            foreach (Graphic tmp in lstStart)
            {
                Graphic gra = new Graphic();
                gra.Geometry = tmp.Geometry;
                gra.Symbol = start_symbol;
                TemporaryLayer.Graphics.Add(gra);
            }
            foreach (Graphic tmp in lstEnd)
            {
                Graphic gra = new Graphic();
                gra.Geometry = tmp.Geometry;
                gra.Symbol = end_symbol;
                TemporaryLayer.Graphics.Add(gra);
            }
        }

        public void ClearGraphic()
        {
            if ((Application.Current as IApp).Dict_ThematicLayer == null)
                return;
            foreach (var item in mainmap.Layers)
            {
                if ((item is GraphicsLayer) && !(Application.Current as IApp).Dict_ThematicLayer.Keys.ToList().Contains(item.ID))
                {
                    (item as GraphicsLayer).ClearGraphics();
                }
            }
        }
    }
}

