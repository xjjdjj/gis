using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml.Linq;
using AYKJ.GISDevelop.Platform;
using AYKJ.GISDevelop.Platform.ToolKit;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using System.Text;
using System.IO;
using ESRI.ArcGIS.Client.Tasks;

namespace AYKJ.GISInterface
{
    public partial class EvacuationRoute : UserControl
    {
        //定义一个Map用来接收平台的Map
        Map mainmap;
        //绘制的Draw
        Draw draw;
        //geometry地址
        string strGeometryurl;
        //临时图层
        GraphicsLayer TemporaryLayer;
        GraphicsLayer RouteLayer;
        //选中高亮图层
        GraphicsLayer HighLayer;
        //动画
        WaitAnimationWindow waitanimationwindow;
        //火灾类
        clsFireLeak clsfireleak;
        //爆炸类
        clsExplosionLeak clsexplosionleak;
        //瞬时泄漏类
        clsInstantaneousLeak clsinstantaneousleak;
        //连续泄漏类
        clsContinuousLeak clscontinuousleak;
        //空间查询方法
        clsRangeQueryExtension clsrangequeryextension;
        //返回空间查询结果
        List<Dictionary<clstipwxy, Graphic>> lst_Return;
        //风向控件
        WindPage wc;
        //中毒和燃烧类型
        string strcontinuetype;
        //使用计时器对象来更新播放进度
        DispatcherTimer leaktimer;
        //需要进行分析的受体
        List<Graphic> lstRecipient;
        //事故发生点位
        MapPoint Mp_Accident;
        //最短分析地址
        string strnetworkurl;
        //最近点分析地址
        string strcfurl;
        //路径查询地址
        string strqueryurl;
        //障碍
        List<Graphic> lstBar;
        //逃离范围
        double dbleaklen;
        //瞬时泄漏的半径集合
        double[] aryInstantaneousSize;
        Dictionary<Graphic, Graphic> Dict_LeakGra;
        //事故类型
        string ModeType;
        //数据
        List<object> lstLeakData;
        //救援点集合
        string[] aryjyd;
        //疏散点集合
        string[] aryssd;

        public EvacuationRoute()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 界面初始化
        /// </summary>
        void InitEvacuationRoute()
        {
            //设置面板的起始位置
            this.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            this.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            this.Margin = new Thickness() { Top = 10, Right = 13 };
            Storyboard_Close.Completed += new EventHandler(Storyboard_Close_Completed);

            XElement xele = PFApp.Extent;
            strGeometryurl = (from item in PFApp.Extent.Elements("GeometryService")
                              select item.Attribute("Url").Value).ToArray()[0];
            var NetWorkUrl = (from item in xele.Element("ExtensionServices").Elements("ExtensionService")
                              where item.Attribute("Name").Value == "网络分析"
                              select new
                              {
                                  Name = item.Attribute("Name").Value,
                                  Url = item.Attribute("Url").Value,
                              }).ToList();
            strnetworkurl = NetWorkUrl[0].Url;
            var QueryUrl = (from item in xele.Element("ExtensionServices").Elements("ExtensionService")
                            where item.Attribute("Name").Value == "路线查询"
                            select new
                            {
                                Name = item.Attribute("Name").Value,
                                Url = item.Attribute("Url").Value,
                            }).ToList();
            strqueryurl = QueryUrl[0].Url;
            var cfurl = (from item in xele.Element("ExtensionServices").Elements("ExtensionService")
                         where item.Attribute("Name").Value == "网络设施点分析"
                         select new
                         {
                             Name = item.Attribute("Name").Value,
                             Url = item.Attribute("Url").Value,
                         }).ToList();
            strcfurl = cfurl[0].Url;
            mainmap = (Application.Current as IApp).MainMap;

            TemporaryLayer = new GraphicsLayer();
            RouteLayer = new GraphicsLayer();
            HighLayer = new GraphicsLayer();
            mainmap.Layers.Add(TemporaryLayer);
            mainmap.Layers.Add(RouteLayer);
            mainmap.Layers.Add(HighLayer);

            lst_Return = new List<Dictionary<clstipwxy, Graphic>>();
            lstBar = new List<Graphic>();
            lstRecipient = new List<Graphic>();

            draw = new Draw(mainmap);
            draw.DrawComplete += new EventHandler<DrawEventArgs>(draw_DrawComplete);
            draw.DrawMode = DrawMode.Point;
            draw.IsEnabled = false;

            clsfireleak = new clsFireLeak();
            clsfireleak.FireLeakEvent += new FireLeakDelegate(clsfireleak_FireLeakEvent);
            clsfireleak.FireLeakFaildEvent += new ExplosionLeakDelegate(clsfireleak_FireLeakFaildEvent);

            clsexplosionleak = new clsExplosionLeak();
            clsexplosionleak.ExplosionLeakFaildEvent += new ExplosionLeakDelegate(clsexplosionleak_ExplosionLeakFaildEvent);
            clsexplosionleak.ExplosionLeakEvent += new ExplosionLeakDelegate(clsexplosionleak_ExplosionLeakEvent);

            clsrangequeryextension = new clsRangeQueryExtension();
            clsrangequeryextension.QueryExtensionFaildEvent += new QueryExtensionDelegate(clsrangequeryextension_QueryExtensionFaildEvent);
            clsrangequeryextension.QueryExtensionEvent += new QueryExtensionDelegate(clsrangequeryextension_QueryExtensionEvent);

            clsinstantaneousleak = new clsInstantaneousLeak();
            clsinstantaneousleak.InstantaneousLeakEvent += new InstantaneousLeakDelegate(clsinstantaneousleak_InstantaneousLeakEvent);
            clsinstantaneousleak.InstantaneousLeakFaildEvent += new InstantaneousLeakDelegate(clsinstantaneousleak_InstantaneousLeakFaildEvent);

            clscontinuousleak = new clsContinuousLeak();
            clscontinuousleak.ContinuousLeakEvent += new ContinuousLeakDelegate(clscontinuousleak_ContinuousLeakEvent);
            clscontinuousleak.ContinuousLeakFaildEvent += new ContinuousLeakDelegate(clscontinuousleak_ContinuousLeakFaildEvent);

            wc = new WindPage();
            wc.LeakFaildEvent += new LeakDelegate(wc_LeakFaildEvent);
            wc.LeakEvent += new LeakDelegate(wc_LeakEvent);

            leaktimer = new DispatcherTimer();
        }

        /// <summary>
        /// 地图绘制事发点进行事故模拟
        /// </summary>
        /// <param name="modetype">事故类别</param>
        /// <param name="lst">事故参数</param>
        public void CallDrawAccidentSimulation(string modetype, List<object> lst, List<string[]> lsttmp)
        {
            CreateLeakPage(modetype, lst);
            aryjyd = lsttmp[0];
            aryssd = lsttmp[1];
            txt_title.Text = txt_title.Text + "-突发事故";
            Dict_LeakGra = new Dictionary<Graphic, Graphic>();
            lstRecipient = new List<Graphic>();
            lstBar = new List<Graphic>();
            ClearGraphic();
            data_result.ItemsSource = null;
            draw.IsEnabled = true;
            lstRecipient = new List<Graphic>();
        }

        /// <summary>
        /// 根据已知点进行事故模拟
        /// </summary>
        /// <param name="modetype">事故类别</param>
        /// <param name="strname">事故点位</param>
        /// <param name="lst">事故参数</param>
        public void CallNameAccidentSimulation(string modetype, string strname, List<object> lst, List<string[]> lsttmp)
        {
            aryjyd = lsttmp[0];
            aryssd = lsttmp[1];
            var buffer = System.Text.Encoding.UTF8.GetBytes(strname);
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
                Message.Show("没有找到该设施点，请重新输入");
                return;
            }
            CreateLeakPage(modetype, lst);
            if (PFApp.Root.Children.Count > 0)
            {
                for (int i = 0; i < PFApp.Root.Children.Count; i++)
                {
                    if (PFApp.Root.Children[i].GetType().Name == "EvacuationRoute")
                    {
                        PFApp.Root.Children.RemoveAt(i);
                    }
                }
            }
            PFApp.Root.Children.Add(this);
            Storyboard_Show.Begin();
            MapPoint mp = Explosion[0].Graphic.Geometry as MapPoint;
            Mp_Accident = mp;
            CallLeakModel(mp.X, mp.Y, ModeType);
        }

        /// <summary>
        /// 根据模拟类型更改界面
        /// </summary>
        /// <param name="modetype"></param>
        /// <param name="lst"></param>
        void CreateLeakPage(string modetype, List<object> lst)
        {
            lstLeakData = lst;
            InitEvacuationRoute();
            ModeType = modetype;
            data_result.ItemsSource = null;
            ClearGraphic();
            lstRecipient = new List<Graphic>();
            Sp_FireExplosion.Visibility = System.Windows.Visibility.Collapsed;
            Sp_Leak.Visibility = System.Windows.Visibility.Collapsed;
            Sp_Leak2.Visibility = System.Windows.Visibility.Collapsed;
            sd.Visibility = System.Windows.Visibility.Collapsed;
            btn_play.Visibility = System.Windows.Visibility.Collapsed;
            switch (ModeType)
            {
                case "火灾":
                    txt_title.Text = "火灾模拟";
                    Sp_FireExplosion.Visibility = System.Windows.Visibility.Visible;
                    break;
                case "爆炸":
                    txt_title.Text = "爆炸模拟";
                    Sp_FireExplosion.Visibility = System.Windows.Visibility.Visible;
                    break;
                case "瞬时泄漏":
                    txt_title.Text = "瞬时泄漏模拟";
                    sd.Visibility = System.Windows.Visibility.Visible;
                    btn_play.Visibility = System.Windows.Visibility.Visible;
                    Sp_Leak2.Visibility = System.Windows.Visibility.Visible;
                    break;
                case "连续泄漏":
                    txt_title.Text = "连续泄漏模拟";
                    Sp_Leak.Visibility = System.Windows.Visibility.Visible;
                    break;
            }
        }

        /// <summary>
        /// 绘制完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void draw_DrawComplete(object sender, DrawEventArgs e)
        {
            if (PFApp.Root.Children.Count > 0)
            {
                for (int i = 0; i < PFApp.Root.Children.Count; i++)
                {
                    if (PFApp.Root.Children[i].GetType().Name == "EvacuationRoute")
                    {
                        PFApp.Root.Children.RemoveAt(i);
                    }
                }
            }
            PFApp.Root.Children.Add(this);
            Storyboard_Show.Begin();
            MapPoint mp = e.Geometry as MapPoint;
            Mp_Accident = mp;
            CallLeakModel(mp.X, mp.Y, ModeType);
            draw.IsEnabled = false;
        }

        /// <summary>
        /// 调用模拟模型
        /// </summary>
        /// <param name="dbx"></param>
        /// <param name="dby"></param>
        /// <param name="strtype"></param>
        void CallLeakModel(double dbx, double dby, string strtype)
        {
            switch (strtype)
            {
                case "火灾":
                    waitanimationwindow = new WaitAnimationWindow("火灾模拟中，请稍候...");
                    waitanimationwindow.Show();
                    Dictionary<string, double> Dict_Fire = new Dictionary<string, double>();
                    double[] dbfire = lstLeakData[0] as double[];
                    Dict_Fire.Add("死亡", dbfire[0]);
                    Dict_Fire.Add("重伤", dbfire[1]);
                    Dict_Fire.Add("轻伤", dbfire[2]);
                    Dict_Fire.Add("安全", dbfire[3]);
                    dbleaklen = dbfire[3] + 50;
                    clsfireleak.LeakCircle(mainmap, strGeometryurl, dbx, dby, Dict_Fire);
                    break;
                case "爆炸":
                    waitanimationwindow = new WaitAnimationWindow("爆炸模拟中，请稍候...");
                    waitanimationwindow.Show();
                    Dictionary<string, double> Dict_Explosion = new Dictionary<string, double>();
                    double[] dbexplosione = lstLeakData[0] as double[];
                    Dict_Explosion.Add("死亡", dbexplosione[0]);
                    Dict_Explosion.Add("重伤", dbexplosione[1]);
                    Dict_Explosion.Add("轻伤", dbexplosione[2]);
                    Dict_Explosion.Add("安全", dbexplosione[3]);
                    dbleaklen = dbexplosione[3] + 50;
                    clsexplosionleak.LeakCircle(mainmap, strGeometryurl, dbx, dby, Dict_Explosion);
                    break;
                case "瞬时泄漏":
                    double[] ary1 = lstLeakData[0] as double[];
                    double[] ary2 = lstLeakData[1] as double[];
                    aryInstantaneousSize = ary2;
                    if (lstLeakData[2] == null)
                    {
                        CallInstantaneousLeak(ary1, ary2, dbx, dby);
                    }
                    else
                    {
                        if (((double[])(lstLeakData[2])).Length == 0)
                        {
                            CallInstantaneousLeak(ary1, ary2, dbx, dby);
                        }
                        else
                        {
                            CallInstantaneousLeak(ary1, ary2, dbx, dby, ((double[])(lstLeakData[2]))[0]);
                        }
                    }
                    break;
                case "连续泄漏":
                    double[] ary4 = lstLeakData[0] as double[];
                    double[] ary5 = lstLeakData[1] as double[];
                    object[] atyobj = lstLeakData[2] as object[];
                    //燃烧区步长
                    double dbCombustionStep = Double.Parse(atyobj[0].ToString());
                    //中毒区步长
                    double dbPoisoningStep = Double.Parse(atyobj[1].ToString());
                    //燃烧偏移量
                    double dbCombustionStart = Double.Parse(atyobj[2].ToString());
                    //燃烧区面积
                    double CombustionArea = Double.Parse(atyobj[3].ToString());
                    //中毒偏移量
                    double dbPoisoningStart = Double.Parse(atyobj[4].ToString());
                    //中毒区面积
                    double PoisoningArea = Double.Parse(atyobj[5].ToString());
                    //为0则展示燃烧和中毒，1为燃烧，2为中毒
                    strcontinuetype = atyobj[6].ToString();
                    rbtn_zd.Visibility = System.Windows.Visibility.Visible;
                    rbtn_rs.Visibility = System.Windows.Visibility.Visible;
                    if (strcontinuetype == "1")
                    {
                        rbtn_zd.Visibility = System.Windows.Visibility.Collapsed;
                    }
                    else if (strcontinuetype == "2")
                    {
                        rbtn_rs.Visibility = System.Windows.Visibility.Collapsed;
                    }
                    if (atyobj.Length == 7)
                    {
                        CallContinuousLeak(ary4, ary5, dbx, dby, dbCombustionStep, dbPoisoningStep, dbCombustionStart, CombustionArea,
                                                dbPoisoningStart, PoisoningArea, strcontinuetype);
                    }
                    else if (atyobj.Length == 8)
                    {
                        CallContinuousLeak(ary4, ary5, dbx, dby, dbCombustionStep, dbPoisoningStep, dbCombustionStart, CombustionArea,
                                                  dbPoisoningStart, PoisoningArea, strcontinuetype, Double.Parse(atyobj[7].ToString()));
                    }
                    break;
            }
        }

        #region 泄漏模拟

        #region 连续泄漏模拟

        #region 手动设置风向连续泄漏模拟
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
        #endregion

        #region 动态接入风向连续泄漏模拟
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
        /// <param name="dbrangle">风向</param>
        public void CallContinuousLeak(double[] aryCombustion, double[] aryPoisoning, double dbx, double dby, double dbCombustionStep,
            double dbPoisoningStep, double dbCombustionStart, double CombustionArea, double dbPoisoningStart, double PoisoningArea, string strtype, double dbrangle)
        {
            waitanimationwindow = new WaitAnimationWindow("连续泄漏模拟中，请稍候...");
            waitanimationwindow.Show();
            clscontinuousleak.LeakContinued(mainmap, strGeometryurl, dbx, dby, dbCombustionStep, aryCombustion, dbCombustionStart,
                 dbPoisoningStep, aryPoisoning, dbPoisoningStart, dbrangle, strtype);
        }

        /// <summary>
        /// 连续泄漏模拟失败
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clscontinuousleak_ContinuousLeakFaildEvent(object sender, EventArgs e)
        {
            Message.Show("连续泄漏模拟失败");
            waitanimationwindow.Close();
        }

        /// <summary>
        /// 连续泄漏模拟成功
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clscontinuousleak_ContinuousLeakEvent(object sender, EventArgs e)
        {
            List<Graphic> lsttmp = (sender as clsContinuousLeak).lst_Return;
            switch (strcontinuetype)
            {
                case "0":
                    //将中毒区加载到地图上
                    Graphic Poisongratmp = lsttmp[0];
                    Poisongratmp.Attributes.Add("StaFill", PoisonGradientBrush());
                    Poisongratmp.Symbol = ContinueSymbol;
                    TemporaryLayer.Graphics.Add(Poisongratmp);
                    //将燃烧区加载到地图上
                    Graphic firegratmp = lsttmp[1];
                    firegratmp.Attributes.Add("StaFill", FireGradientBrush());
                    firegratmp.Symbol = ContinueSymbol;
                    TemporaryLayer.Graphics.Add(firegratmp);
                    break;
                case "1":
                    //将燃烧区加载到地图上
                    Graphic firegra = lsttmp[0];
                    firegra.Attributes.Add("StaFill", FireGradientBrush());
                    firegra.Symbol = ContinueSymbol;
                    TemporaryLayer.Graphics.Add(firegra);
                    break;
                case "2":
                    //将中毒区加载到地图上
                    Graphic Poisongra = lsttmp[0];
                    Poisongra.Attributes.Add("StaFill", PoisonGradientBrush());
                    Poisongra.Symbol = ContinueSymbol;
                    TemporaryLayer.Graphics.Add(Poisongra);
                    break;
            }
            List<Graphic> lsttxt = (sender as clsContinuousLeak).lst_Txt;
            for (int i = 0; i < lsttxt.Count; i++)
            {
                Graphic txtgra = lsttxt[i];
                txtgra.Symbol = ConcentricCircleSymbol;
                TemporaryLayer.Graphics.Add(txtgra);
            }
            clsrangequeryextension.QueryExtension(strGeometryurl, lsttmp);
        }
        #endregion

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

        #region 瞬时泄漏处理结果

        #region 手动输入风向
        /// <summary>
        /// 瞬时泄漏方法
        /// </summary>
        /// <param name="arysize">步长</param>
        /// <param name="aryradius">每个节点半径</param>
        /// <param name="dbx">起点X坐标</param>
        /// <param name="dby">起点Y坐标</param>
        public void CallInstantaneousLeak(double[] arysize, double[] aryradius, double dbx, double dby)
        {
            sd.Tag = null;
            wc.strleaktype = "Instantaneous";
            wc.LeakMap = mainmap;
            wc.strurl = strGeometryurl;
            wc.dbx = dbx;
            wc.dby = dby;
            wc.arysize = arysize;
            wc.aryradius = aryradius;
            wc.Show();
        }
        #endregion

        #region 动态接入风向硬件
        /// <summary>
        /// 瞬时泄漏方法
        /// </summary>
        /// <param name="arysize">步长</param>
        /// <param name="aryradius">每个节点半径</param>
        /// <param name="dbx">起点X坐标</param>
        /// <param name="dby">起点Y坐标</param>
        /// <param name="dbangle">扩散方向Y坐标</param>
        public void CallInstantaneousLeak(double[] arysize, double[] aryradius, double dbx, double dby, double dbangle)
        {
            sd.Tag = null;
            waitanimationwindow = new WaitAnimationWindow("瞬时泄漏模拟中，请稍候...");
            waitanimationwindow.Show();
            clsinstantaneousleak.LeakShaft(mainmap, strGeometryurl, dbx, dby, arysize, aryradius, dbangle);
        }

        /// <summary>
        /// 瞬时泄漏模拟成功
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clsinstantaneousleak_InstantaneousLeakEvent(object sender, EventArgs e)
        {
            List<Graphic> lsttmp = (sender as clsInstantaneousLeak).lst_Return;
            sd.Maximum = lsttmp.Count - 1;
            sd.Value = 0;
            sd.Tag = lsttmp;
            Graphic gra = lsttmp[0];
            SimpleFillSymbol sfs = new SimpleFillSymbol();
            sfs.Fill = new SolidColorBrush() { Color = Colors.Red, Opacity = 0.4 };
            sfs.BorderThickness = 0;
            gra.Symbol = sfs;
            TemporaryLayer.Graphics.Add(gra);
            clsrangequeryextension.QueryExtension(strGeometryurl, lsttmp);
        }

        /// <summary>
        /// 瞬时泄漏模拟失败
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clsinstantaneousleak_InstantaneousLeakFaildEvent(object sender, EventArgs e)
        {
            Message.Show("瞬时泄漏模拟失败");
            waitanimationwindow.Close();
        }
        #endregion

        #region 模拟结果展示
        /// <summary>
        /// 时间轴控制
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void sd_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            List<Graphic> lsttmp = (sender as Slider).Tag as List<Graphic>;
            if (lsttmp == null)
                return;
            int value = Convert.ToInt32(sd.Value);
            //txt.Text = value.ToString();           
            List<Graphic> tmp = new List<Graphic>();
            Dictionary<string, clstipwxy> Dict_data = new Dictionary<string, clstipwxy>();
            for (int i = lsttmp.Count - 1; i > value; i--)
            {
                TemporaryLayer.Graphics.Remove(lsttmp[i]);
            }
            for (int i = 0; i < value + 1; i++)
            {
                Graphic gra = lsttmp[i];
                SimpleFillSymbol sfs = new SimpleFillSymbol();
                sfs.Fill = new SolidColorBrush() { Color = Colors.Red, Opacity = 0.4 };
                sfs.BorderThickness = 0;
                gra.Symbol = sfs;
                tmp.Add(gra);
                if (!TemporaryLayer.Graphics.Contains(gra))
                {
                    TemporaryLayer.Graphics.Add(gra);
                }

                List<clstipwxy> tmpdata = lst_Return[i].Keys.ToList();
                for (int m = 0; m < tmpdata.Count; m++)
                {

                    if (!Dict_data.Keys.ToList().Contains(tmpdata[m].wxyid))
                        Dict_data.Add(tmpdata[m].wxyid, tmpdata[m]);
                }
            }
            data_result.ItemsSource = null;
            data_result.ItemsSource = Dict_data.Values.ToList();
        }

        /// <summary>
        /// 播放进程
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_play_Click(object sender, RoutedEventArgs e)
        {
            //设置事件引发的时间间隔
            leaktimer.Interval = TimeSpan.FromMilliseconds(1000);
            //计时器对象事件
            leaktimer.Tick -= timer_Tick;
            leaktimer.Tick += new EventHandler(timer_Tick);
            if (btn_play.Content.ToString() == "播放")
            {
                //开始计时
                leaktimer.Start();
                btn_play.Content = "暂停";
            }
            else if (btn_play.Content.ToString() == "暂停")
            {
                leaktimer.Stop();
                btn_play.Content = "播放";
            }
        }

        /// <summary>
        /// 每秒自动播放
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void timer_Tick(object sender, EventArgs e)
        {
            sd.Value = sd.Value + 1;
            if (sd.Value == sd.Maximum)
            {
                leaktimer.Stop();
                btn_play.Content = "播放";
            }
        }
        #endregion

        #endregion

        #region 手动设置方向泄漏结果
        /// <summary>
        /// 模拟成功
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void wc_LeakEvent(object sender, EventArgs e)
        {
            List<Graphic> lsttmp = (sender as WindPage).lst_Return;
            if (wc.strleaktype == "Instantaneous")
            {
                lstBar.Add(lsttmp[0]);
                sd.Maximum = lsttmp.Count - 1;
                sd.Value = 0;
                sd.Tag = lsttmp;
                Graphic gra = lsttmp[0];
                SimpleFillSymbol sfs = new SimpleFillSymbol();
                sfs.Fill = new SolidColorBrush() { Color = Colors.Red, Opacity = 0.4 };
                sfs.BorderThickness = 0;
                gra.Symbol = sfs;
                TemporaryLayer.Graphics.Add(gra);
                clsrangequeryextension.QueryExtension(strGeometryurl, lsttmp);
            }
            else if (wc.strleaktype == "Continuous")
            {
                switch (strcontinuetype)
                {
                    case "0":
                        lstBar.Add(lsttmp[1]);
                        //将中毒区加载到地图上
                        Graphic Poisongratmp = lsttmp[0];
                        Poisongratmp.Attributes.Add("StaFill", PoisonGradientBrush());
                        Poisongratmp.Symbol = ContinueSymbol;
                        TemporaryLayer.Graphics.Add(Poisongratmp);
                        //将燃烧区加载到地图上
                        Graphic firegratmp = lsttmp[1];
                        firegratmp.Attributes.Add("StaFill", FireGradientBrush());
                        firegratmp.Symbol = ContinueSymbol;
                        TemporaryLayer.Graphics.Add(firegratmp);
                        break;
                    case "1":
                        lstBar.Add(lsttmp[0]);
                        //将燃烧区加载到地图上
                        Graphic firegra = lsttmp[0];
                        firegra.Attributes.Add("StaFill", FireGradientBrush());
                        firegra.Symbol = ContinueSymbol;
                        TemporaryLayer.Graphics.Add(firegra);
                        break;
                    case "2":
                        lstBar.Add(lsttmp[0]);
                        //将中毒区加载到地图上
                        Graphic Poisongra = lsttmp[0];
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
                    TemporaryLayer.Graphics.Add(txtgra);
                }
                clsrangequeryextension.QueryExtension(strGeometryurl, lsttmp);
            }
        }

        /// <summary>
        /// 模拟失败
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void wc_LeakFaildEvent(object sender, EventArgs e)
        {
            if (wc.strleaktype == "Instantaneous")
            {
                Message.Show("瞬时泄漏模拟失败");
                wc.waitanimationwindow.Close();
            }
            else if (wc.strleaktype == "Instantaneous")
            {
                Message.Show("连续泄漏模拟失败");
                wc.waitanimationwindow.Close();
            }
        }
        #endregion

        #endregion

        #region 火灾处理结果
        /// <summary>
        /// 返回火灾模拟结果
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
            lstBar.Add(tmp.ReturnCircleRing[3]);
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
                    TemporaryLayer.Graphics.Add(graCircleTxt);
                }
            }
            for (int i = 0; i < tmp.ReturnCircleCenter.Count; i++)
            {
                Graphic graCircleCenter = tmp.ReturnCircleCenter[i];
                graCircleCenter.Symbol = FireSymbol;
                TemporaryLayer.Graphics.Add(graCircleCenter);
            }
            clsrangequeryextension.QueryExtension(strGeometryurl, tmp.ReturnCircleRing);
        }

        /// <summary>
        /// 火灾模拟出错
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clsfireleak_FireLeakFaildEvent(object sender, EventArgs e)
        {
            Message.Show("火灾模拟出错");
            waitanimationwindow.Close();
        }
        #endregion

        #region 爆炸处理结果
        /// <summary>
        /// 返回爆炸结果
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
            lstBar.Add(tmp.ReturnCircleRing[3]);
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
                    TemporaryLayer.Graphics.Add(graCircleTxt);
                }
            }
            for (int i = 0; i < tmp.ReturnCircleCenter.Count; i++)
            {
                Graphic graCircleCenter = tmp.ReturnCircleCenter[i];
                graCircleCenter.Symbol = ExplosionSymbol;
                TemporaryLayer.Graphics.Add(graCircleCenter);
            }
            clsrangequeryextension.QueryExtension(strGeometryurl, tmp.ReturnCircleRing);
        }

        /// <summary>
        /// 爆炸模拟出错
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clsexplosionleak_ExplosionLeakFaildEvent(object sender, EventArgs e)
        {
            Message.Show("爆炸模拟出错");
            waitanimationwindow.Close();
        }

        #endregion

        #region 数据处理
        /// <summary>
        /// 返回查询的结果
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clsrangequeryextension_QueryExtensionEvent(object sender, EventArgs e)
        {
            lst_Return = EllipsisResult((sender as clsRangeQueryExtension).lst_Return);
            string strtype = ModeType;
            List<clstipwxy> lsttmp = new List<clstipwxy>();
            if (strtype == "火灾" || strtype == "爆炸")
            {
                if (rbtn_sw.IsChecked == true)
                {
                    lsttmp = lst_Return[0].Keys.ToList();
                }
                else if (rbtn_zs.IsChecked == true)
                {
                    lsttmp = lst_Return[1].Keys.ToList();
                }
                else if (rbtn_qs.IsChecked == true)
                {
                    lsttmp = lst_Return[2].Keys.ToList();
                }
                else if (rbtn_aq.IsChecked == true)
                {
                    lsttmp = lst_Return[3].Keys.ToList();
                }
                waitanimationwindow.Close();
            }
            else if (strtype == "瞬时泄漏")
            {
                lsttmp = lst_Return[0].Keys.ToList();
                if (wc.waitanimationwindow != null)
                {
                    wc.waitanimationwindow.Close();
                }
                if (waitanimationwindow != null)
                {
                    waitanimationwindow.Close();
                }
            }
            else if (strtype == "连续泄漏")
            {
                if (rbtn_zd.IsChecked == true)
                {
                    lsttmp = lst_Return[1].Keys.ToList();
                }
                else if (rbtn_rs.IsChecked == true)
                {
                    lsttmp = lst_Return[0].Keys.ToList();
                }
                if (wc.waitanimationwindow != null)
                {
                    wc.waitanimationwindow.Close();
                }
                if (waitanimationwindow != null)
                {
                    waitanimationwindow.Close();
                }
            }
            data_result.ItemsSource = null;
            data_result.ItemsSource = lsttmp;
        }

        /// <summary>
        /// 火灾爆炸数据切换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbtnFireExplosion_Checked(object sender, RoutedEventArgs e)
        {
            lstRecipient = new List<Graphic>();
            List<clstipwxy> lsttmp = new List<clstipwxy>();
            if (lst_Return == null)
                return;
            if (lst_Return.Count == 0)
                return;
            if (rbtn_sw.IsChecked == true)
            {
                lsttmp = lst_Return[0].Keys.ToList();
            }
            else if (rbtn_zs.IsChecked == true)
            {
                lsttmp = lst_Return[1].Keys.ToList();
            }
            else if (rbtn_qs.IsChecked == true)
            {
                lsttmp = lst_Return[2].Keys.ToList();
            }
            else if (rbtn_aq.IsChecked == true)
            {
                lsttmp = lst_Return[3].Keys.ToList();
            }
            data_result.ItemsSource = null;
            data_result.ItemsSource = lsttmp;
        }

        /// <summary>
        /// 泄漏数据进行切换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbtnLeak_Checked(object sender, RoutedEventArgs e)
        {
            lstRecipient = new List<Graphic>();
            List<clstipwxy> lsttmp = new List<clstipwxy>();
            if (lst_Return == null)
                return;
            if (lst_Return.Count == 0)
                return;
            if (rbtn_zd.IsChecked == true)
            {
                lsttmp = lst_Return[1].Keys.ToList();
            }
            else if (rbtn_rs.IsChecked == true)
            {
                lsttmp = lst_Return[0].Keys.ToList();
            }
            data_result.ItemsSource = null;
            data_result.ItemsSource = lsttmp;
        }

        /// <summary>
        /// 数据查询出错
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clsrangequeryextension_QueryExtensionFaildEvent(object sender, EventArgs e)
        {
            Message.Show("数据分析出错");
            waitanimationwindow.Close();
        }

        /// <summary>
        /// 选择数据定位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_postion_Click(object sender, RoutedEventArgs e)
        {
            HighLayer.Graphics.Clear();
            if (data_result.SelectedItem != null)
            {
                Graphic gra = new Graphic();
                if (Sp_FireExplosion.Visibility == Visibility.Visible)
                {
                    if (rbtn_sw.IsChecked == true)
                    {
                        gra.Geometry = lst_Return[0][data_result.SelectedItem as clstipwxy].Geometry;
                    }
                    else if (rbtn_zs.IsChecked == true)
                    {
                        gra.Geometry = lst_Return[1][data_result.SelectedItem as clstipwxy].Geometry;
                    }
                    else if (rbtn_qs.IsChecked == true)
                    {
                        gra.Geometry = lst_Return[2][data_result.SelectedItem as clstipwxy].Geometry;
                    }
                    else if (rbtn_aq.IsChecked == true)
                    {
                        gra.Geometry = lst_Return[3][data_result.SelectedItem as clstipwxy].Geometry;
                    }
                }
                else if (Sp_Leak.Visibility == Visibility.Visible)
                {
                    if (rbtn_zd.IsChecked == true)
                    {
                        gra.Geometry = lst_Return[1][data_result.SelectedItem as clstipwxy].Geometry;
                    }
                    else if (rbtn_rs.IsChecked == true)
                    {
                        gra.Geometry = lst_Return[0][data_result.SelectedItem as clstipwxy].Geometry;
                    }
                }
                else
                {
                    string strremark = (data_result.SelectedItem as clstipwxy).wxytip;
                    var data = (from item in (Application.Current as IApp).lstThematic
                                where item.Attributes["StaRemark"].ToString() == strremark
                                select new
                                {
                                    Gra = item,
                                }).ToList();
                    gra = new Graphic() { Geometry = data[0].Gra.Geometry };
                }
                gra.Symbol = HighMarkerStyle;
                HighLayer.Graphics.Add(gra);

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
        #endregion

        /// <summary>
        /// 清除临时图层内容
        /// </summary>
        void ClearGraphic()
        {
            foreach (var item in mainmap.Layers)
            {
                if ((item is GraphicsLayer) && !(Application.Current as IApp).Dict_ThematicLayer.Keys.ToList().Contains(item.ID))
                {
                    (item as GraphicsLayer).ClearGraphics();
                }
            }
        }

        /// <summary>
        /// 清除状态
        /// </summary>
        public void Reset()
        {
            if (draw != null)
            {
                draw.IsEnabled = false;
                draw = null;
                ClearGraphic();
            }
        }

        #region 对受体和事故发生点进行路径分析
        private void btn_route_Click(object sender, RoutedEventArgs e)
        {
            if (lstRecipient == null)
            {
                Message.Show("请先选择受体");
                return;
            }
            if (lstRecipient.Count == 0)
            {
                Message.Show("请先选择受体");
                return;
            }

            waitanimationwindow = new WaitAnimationWindow("救援逃生路径计算中，请稍候...");
            waitanimationwindow.Show();

            RouteLayer.Graphics.Clear();
    
            string[] ary1 = aryjyd;// "漳州师范学院预防与处置应急事件救援队|华侨大学义务消防队".Split('|');
            string[] ary2 = aryssd;//"福建检验检疫局卫生处理技术服务队|福建检验检疫局防控高致病性禽流感应急预备队".Split('|');
            List<Graphic> lst1 = new List<Graphic>();
            for (int i = 0; i < ary1.Length; i++)
            {
                var buffer = System.Text.Encoding.UTF8.GetBytes(ary1[i]);
                var ms = new MemoryStream(buffer);
                var jsonObject = System.Json.JsonObject.Load(ms) as System.Json.JsonObject;
                string tmp_wxyid = jsonObject["wxyid"].ToString().Replace("\"", "");
                string tmp_wxytype = jsonObject["wxytype"].ToString().Replace("\"", "");
                string tmp_dwdm = jsonObject["dwdm"].ToString().Replace("\"", "");
                string statag = tmp_wxytype + "|" + tmp_wxyid + "|" + tmp_dwdm;

                var data = (from item in (Application.Current as IApp).lstThematic
                            where item.Attributes["StaTag"].ToString().Contains(statag)
                            select new
                            {
                                gra = item,
                            }).ToList();
                lst1.Add(data[0].gra);
            }
            List<Graphic> lst2 = new List<Graphic>();
            for (int i = 0; i < ary2.Length; i++)
            {
                var buffer = System.Text.Encoding.UTF8.GetBytes(ary2[i]);
                var ms = new MemoryStream(buffer);
                var jsonObject = System.Json.JsonObject.Load(ms) as System.Json.JsonObject;
                string tmp_wxyid = jsonObject["wxyid"].ToString().Replace("\"", "");
                string tmp_wxytype = jsonObject["wxytype"].ToString().Replace("\"", "");
                string tmp_dwdm = jsonObject["dwdm"].ToString().Replace("\"", "");
                string statag = tmp_wxytype + "|" + tmp_wxyid + "|" + tmp_dwdm;

                var data = (from item in (Application.Current as IApp).lstThematic
                            where item.Attributes["StaTag"].ToString().Contains(statag)
                            select new
                            {
                                gra = item,
                            }).ToList();
                lst2.Add(data[0].gra);
            }

            string strtype = ModeType;
            if (strtype == "火灾" || strtype == "爆炸")
            {
                clsCircleRouteAnalysis clscirclerouteanalysis = new clsCircleRouteAnalysis();
                clscirclerouteanalysis.RouteAnalysisEvent += new CircleRouteAnalysisDelegate(clscirclerouteanalysis_RouteAnalysisEvent);
                clscirclerouteanalysis.RouteAnalysisFaildEvent += new CircleRouteAnalysisDelegate(clsrouteanalysis_RouteAnalysisFaildEvent);
                if (PFApp.MapServerType == enumMapServerType.Baidu)
                {
                    var xele = PFApp.Extent;
                    var dataServices = (from item in xele.Element("ExtensionServices").Elements("ExtensionService")
                                        where item.Attribute("Name").Value == "路线查询"
                                        select new
                                        {
                                               Url = item.Attribute("Url").Value,
                                        }).ToList();
                    if (dataServices.Count > 0 && dataServices[0].Url != "")
                        clscirclerouteanalysis.Route(lst1, lstRecipient, lst2, lstBar[0], Mp_Accident, mainmap, dataServices[0].Url, strnetworkurl, strcfurl, strqueryurl, dbleaklen);
                    else
                        Message.Show("疏散路径分析失败！请检查配置文件是否正确。");

                }
                else
                    clscirclerouteanalysis.Route(lst1, lstRecipient, lst2, lstBar[0], Mp_Accident, mainmap, strGeometryurl, strnetworkurl, strcfurl, strqueryurl, dbleaklen);
            }
            else if (strtype == "瞬时泄漏")
            {
                clsInstantaneousRouteAnalysis clsinstantaneousrouteanalysis = new clsInstantaneousRouteAnalysis();
                clsinstantaneousrouteanalysis.RouteAnalysisEvent += new InstantaneousRouteAnalysisDelegate(clsinstantaneousrouteanalysis_RouteAnalysisEvent);
                clsinstantaneousrouteanalysis.RouteAnalysisFaildEvent += new InstantaneousRouteAnalysisDelegate(clsrouteanalysis_RouteAnalysisFaildEvent);

                List<double> lstlen = new List<double>();
                Dict_LeakGra = new Dictionary<Graphic, Graphic>();
                //lstBar = wc.lst_Return;
                List<int> rbcount = new List<int>();
                for (int i = 0; i < lst_Return.Count; i++)
                {
                    for (int m = 0; m < lst_Return[i].Count; m++)
                    {
                        if (!Dict_LeakGra.Keys.Contains(lst_Return[i].Values.ToList()[m]))
                        {
                            Dict_LeakGra.Add(lst_Return[i].Values.ToList()[m], wc.lst_Return[i]);
                            lstlen.Add(aryInstantaneousSize[i] + 50);
                            rbcount.Add(i);
                        }
                    }
                }
                List<Graphic> lsttmp = new List<Graphic>();
                for (int i = 0; i < lstRecipient.Count; i++)
                {
                    lsttmp.Add(Dict_LeakGra[lstRecipient[i]]);
                }
                if (PFApp.MapServerType == enumMapServerType.Baidu)
                {
                    var xele = PFApp.Extent;
                    var dataServices = (from item in xele.Element("ExtensionServices").Elements("ExtensionService")
                                        where item.Attribute("Name").Value == "路线查询"
                                        select new
                                        {
                                            Url = item.Attribute("Url").Value,
                                        }).ToList();
                    if (dataServices.Count > 0 && dataServices[0].Url != "")
                        clsinstantaneousrouteanalysis.Route(lst1, lstRecipient, lst2, lsttmp, Mp_Accident, wc.lst_Return,
                         mainmap, dataServices[0].Url, strnetworkurl, strcfurl, strqueryurl, lstlen.ToArray(), rbcount);
                    else
                        Message.Show("疏散路径分析失败！请检查配置文件是否正确。");

                }
                else
                    clsinstantaneousrouteanalysis.Route(lst1, lstRecipient, lst2, lsttmp, Mp_Accident, wc.lst_Return,
                         mainmap, strGeometryurl, strnetworkurl, strcfurl, strqueryurl, lstlen.ToArray(), rbcount);
            }
            else
            {
                clsContinueRouteAnalysis clscontinuerouteanalysis = new clsContinueRouteAnalysis();
                clscontinuerouteanalysis.RouteAnalysisFaildEvent += new ContinueRouteAnalysisDelegate(clsrouteanalysis_RouteAnalysisFaildEvent);
                clscontinuerouteanalysis.RouteAnalysisEvent += new ContinueRouteAnalysisDelegate(clscontinuerouteanalysis_RouteAnalysisEvent);

                Polygon tmppolygon = new Polygon();
                tmppolygon.Rings.Add((wc.lst_Return[0].Geometry as Polygon).Rings[0]);
                tmppolygon.Rings.Add((wc.lst_Return[1].Geometry as Polygon).Rings[0]);
                tmppolygon.SpatialReference = Mp_Accident.SpatialReference;
                Graphic tmpgra = new Graphic() { Geometry = tmppolygon };
                if (PFApp.MapServerType == enumMapServerType.Baidu)
                {
                    var xele = PFApp.Extent;
                    var dataServices = (from item in xele.Element("ExtensionServices").Elements("ExtensionService")
                                        where item.Attribute("Name").Value == "路线查询"
                                        select new
                                        {
                                            Url = item.Attribute("Url").Value,
                                        }).ToList();
                    if (dataServices.Count > 0 && dataServices[0].Url != "")
                        clscontinuerouteanalysis.Route(lst1, lstRecipient, lst2, Mp_Accident, wc.lst_Return[0],
                         mainmap, dataServices[0].Url, strnetworkurl, strcfurl, strqueryurl);
                    else
                        Message.Show("疏散路径分析失败！请检查配置文件是否正确。");

                }
                else
                    clscontinuerouteanalysis.Route(lst1, lstRecipient, lst2, Mp_Accident, wc.lst_Return[0],
                         mainmap, strGeometryurl, strnetworkurl, strcfurl, strqueryurl);
                //clscontinuerouteanalysis.Route(lst1, lstRecipient, lst2, Mp_Accident, tmpgra,
                //         mainmap, strGeometryurl, strnetworkurl, strcfurl, strqueryurl);
            }
        }

        void clscontinuerouteanalysis_RouteAnalysisEvent(object sender, EventArgs e)
        {
            List<Graphic> lst1 = (sender as clsContinueRouteAnalysis).lst_ReturnEscape;
            List<Graphic> lst2 = (sender as clsContinueRouteAnalysis).lst_ReturnRescue;
            foreach (Graphic gra in lst1)
            {
                if (gra != null)
                {
                    gra.Symbol = EscapeRouteLineStyle;
                    RouteLayer.Graphics.Add(gra);
                }
            }
            foreach (Graphic gra in lst2)
            {
                if (gra != null)
                {
                    gra.Symbol = RescueRouteLineStyle;
                    RouteLayer.Graphics.Add(gra);
                }
            }
            waitanimationwindow.Close();
        }

        /// <summary>
        /// 火灾爆炸路径分析成功
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clscirclerouteanalysis_RouteAnalysisEvent(object sender, EventArgs e)
        {
            List<Graphic> lst1 = (sender as clsCircleRouteAnalysis).lst_ReturnEscape;
            List<Graphic> lst2 = (sender as clsCircleRouteAnalysis).lst_ReturnRescue;
            foreach (Graphic gra in lst1)
            {
                if (gra != null)
                {
                    gra.Symbol = EscapeRouteLineStyle;
                    RouteLayer.Graphics.Add(gra);
                }
            }
            foreach (Graphic gra in lst2)
            {
                if (gra != null)
                {
                    gra.Symbol = RescueRouteLineStyle;
                    RouteLayer.Graphics.Add(gra);
                }
            }
            waitanimationwindow.Close();
        }

        /// <summary>
        /// 瞬时泄漏路径分析成功
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clsinstantaneousrouteanalysis_RouteAnalysisEvent(object sender, EventArgs e)
        {
            List<Graphic> lst1 = (sender as clsInstantaneousRouteAnalysis).lst_ReturnEscape;
            List<Graphic> lst2 = (sender as clsInstantaneousRouteAnalysis).lst_ReturnRescue;
            foreach (Graphic gra in lst1)
            {
                if (gra != null)
                {
                    gra.Symbol = EscapeRouteLineStyle;
                    RouteLayer.Graphics.Add(gra);
                }
            }
            foreach (Graphic gra in lst2)
            {
                if (gra != null)
                {
                    gra.Symbol = RescueRouteLineStyle;
                    RouteLayer.Graphics.Add(gra);
                }
            }
            waitanimationwindow.Close();
        }

        /// <summary>
        /// 路径分析失败
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clsrouteanalysis_RouteAnalysisFaildEvent(object sender, EventArgs e)
        {
            Message.Show("路径分析出错");
            waitanimationwindow.Close();
        }

        /// <summary>
        /// 不选择受体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ck_Checked(object sender, RoutedEventArgs e)
        {
            string strremark = (data_result.SelectedItem as clstipwxy).wxytip;
            var data = (from item in (Application.Current as IApp).lstThematic
                        where item.Attributes["StaRemark"].ToString() == strremark
                        select new
                        {
                            Gra = item,
                        }).ToList();
            if (!lstRecipient.Contains(data[0].Gra))
            {
                lstRecipient.Add(data[0].Gra);
            }
        }

        /// <summary>
        /// 选择受体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ck_Unchecked(object sender, RoutedEventArgs e)
        {
            string strremark = (data_result.SelectedItem as clstipwxy).wxytip;
            var data = (from item in (Application.Current as IApp).lstThematic
                        where item.Attributes["StaRemark"].ToString() == strremark
                        select new
                        {
                            Gra = item,
                        }).ToList();
            if (lstRecipient.Contains(data[0].Gra))
            {
                lstRecipient.Remove(data[0].Gra);
            }
        }
        #endregion

        void Storyboard_Close_Completed(object sender, EventArgs e)
        {
            Reset();
            if (PFApp.Root.Children.Contains(this))
            {
                PFApp.Root.Children.Remove(this);
            }
        }

        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            Storyboard_Close.Begin();
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

        /// <summary>
        /// 替换过长点位名称
        /// </summary>
        /// <param name="tmplst"></param>
        /// <returns></returns>
        List<Dictionary<clstipwxy, Graphic>> EllipsisResult(List<Dictionary<clswxy, Graphic>> tmplst)
        {
            List<Dictionary<clstipwxy, Graphic>> lst = new List<Dictionary<clstipwxy, Graphic>>();
            for (int i = 0; i < tmplst.Count; i++)
            {
                Dictionary<clstipwxy, Graphic> dict = new Dictionary<clstipwxy, Graphic>();
                for (int m = 0; m < tmplst[i].Count; m++)
                {
                    Graphic gra = tmplst[i].Values.ToList()[m];
                    clstipwxy clstipwxy = new clstipwxy()
                    {
                        wxyid = tmplst[i].Keys.ToList()[m].wxyid,
                        wxydwdm = tmplst[i].Keys.ToList()[m].wxydwdm,
                        wxyname = EllipsisName(tmplst[i].Keys.ToList()[m].wxyname),
                        wxytip = tmplst[i].Keys.ToList()[m].wxyname,
                        wxytype = tmplst[i].Keys.ToList()[m].wxytype
                    };
                    dict.Add(clstipwxy, gra);
                }
                lst.Add(dict);
            }
            return lst;
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
            if (length < 39)
            {
                tmpstr = str;
            }
            else
            {
                tmpstr = str.Substring(0, 16) + "...";
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
