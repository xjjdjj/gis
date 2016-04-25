/// <summary>  
/// 作者：陈锋 
/// 时间：2012/5/24 11:17:21  
/// 公司:南京安元科技有限公司  
/// 版权：2012-2020  
/// CLR版本：4.0.30319.261  
/// Measure说明：量测代码
/// 唯一标识：3d99bf65-df86-4713-b870-816979b1004f  
/// </summary>


using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using AYKJ.GISDevelop.Platform;
using AYKJ.GISDevelop.Platform.ToolKit;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Tasks;

namespace AYKJ.GISDevelop.Control
{
    public partial class Measure : UserControl
    {
        #region 变量定义
        Graphic gra;
        GeometryService geoService;
        BaiduMeature baiduMeature;
        //用于百度测量
        List<MapPoint> measurePoints;
        Map map;//地图
        //增加到
        GraphicsLayer graphics = new GraphicsLayer();
        GraphicsLayer graphicsMouserPt = new GraphicsLayer();
        //定义地形Draw
        Draw drawTool;
        //点集 
        private List<MapPoint> mapPoints = new List<MapPoint>();
        //是否全部
        bool istwopoint = false;
        //总长
        double alllength = 0;
        //当前的线
        private ESRI.ArcGIS.Client.Geometry.Polyline m_currline = new ESRI.ArcGIS.Client.Geometry.Polyline();
        //当前量线或面
        bool IsMeasurerLine = false;
        bool IsSelf = false;
        //起点
        MapPoint m_startpt = new MapPoint();
        //当前点
        private MapPoint m_currPoint = new MapPoint();
        //开始画的graphic
        Graphic gra_begin = new Graphic();

        //线的样式
        public SimpleLineSymbol linesymbol = new SimpleLineSymbol()
        {
            Color = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)),
            Width = 2,
            Style = SimpleLineSymbol.LineStyle.Solid
        };
        //面的样式
        public SimpleFillSymbol fillsymbol = new SimpleFillSymbol()
        {
            Fill = new SolidColorBrush(Color.FromArgb(0x22, 0, 255, 0)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)),
            BorderThickness = 2
        };
        #endregion

        public Measure()
        {
            InitializeComponent();
            //有Esri服务则启用

            //接收配置文件
            XElement element = PFApp.Extent;
            //读取geomtetry的地址参数
            string geoServiceUrl = (from item in element.Elements("GeometryService")
                                    select item.Attribute("Url").Value).ToArray()[0];
            if (PFApp.MapServerType == enumMapServerType.Esri || geoServiceUrl.Trim() != "")
            {
                //定义一个GeometryService
                geoService = new GeometryService(geoServiceUrl);
                //注册空间坐标转换事件
                geoService.ProjectCompleted += new EventHandler<GraphicsEventArgs>(geoService_ProjectCompleted);
                //注册面积测量事件
                geoService.AreasAndLengthsCompleted += new EventHandler<AreasAndLengthsEventArgs>(geoService_AreasAndLengthsCompleted);
                //注册长度量测事件
                geoService.LengthsCompleted += new EventHandler<LengthsEventArgs>(geoService_LengthsCompleted);
            }
            if (PFApp.MapServerType == enumMapServerType.Baidu)
            {
                baiduMeature = new BaiduMeature();
            }
        }

        #region 完成绘制
        void drawTool_DrawBegin(object sender, EventArgs e)
        {
            //清空点集合
            mapPoints = new List<MapPoint>();
            //往点图层上增加起点
            graphicsMouserPt.Graphics.Add(gra_begin);
            measurePoints.Add(gra_begin.Geometry as MapPoint);
            //设置值，执行地图的左击事件
            IsSelf = true;
        }

        void drawTool_DrawComplete(object sender, DrawEventArgs args)
        {
            //设置值，取消地图的左击事件
            IsSelf = false;
            List<Graphic> graphicList = new List<Graphic>();
            switch (args.DrawMode)
            {
                case DrawMode.Polygon:
                    if ((args.Geometry as ESRI.ArcGIS.Client.Geometry.Polygon).Rings[0].Count < 4)
                    {                        
                        Message.Show("多边形顶点数不能小于3");
                        graphicsMouserPt.Graphics.Clear();
                        return;
                    }
                    else
                    {
                        gra = new Graphic() { Symbol = fillsymbol, Geometry = args.Geometry };
                        //为Graphics集添加新项                       
                        graphics.Graphics.Add(gra);
                        graphicList.Add(gra);

                        //执行坐标转换
                        if (PFApp.MapServerType == enumMapServerType.Baidu)
                            MathLengthAndAreaForBaidu();
                        else if (PFApp.MapServerType == enumMapServerType.Esri)
                        {
                            if (!geoService.IsBusy)
                                geoService.ProjectAsync(graphicList, new SpatialReference(2385));//计算面积
                        }
                        else if (geoService != null)
                        {
                            if (!geoService.IsBusy)
                                geoService.ProjectAsync(graphicList, new SpatialReference(2385));//计算面积
                        }
                    }
                    break;
                case DrawMode.Polyline:
                    gra = new Graphic() { Symbol = linesymbol, Geometry = args.Geometry };
                    graphics.Graphics.Add(gra);
                    graphicList.Add(gra);
                    break;
                default:
                    break;
            }

        }
        #endregion

        /// <summary>
        /// 关闭量测
        /// </summary>
        public void CloseMeasure()
        {
            //graphics.Graphics.Clear();
            //graphicsMouserPt.Graphics.Clear();
            //if (map != null)
            //{
            //    //移除地形的grapiclayer（点）
            //    map.Layers.Remove(graphics);
            //    map.Layers.Remove(graphicsMouserPt);
            //}
            if (drawTool != null)
            {

                //置空地形的draw
                drawTool.DrawMode = DrawMode.None;
                ////地形的draw设为不可用
                drawTool.IsEnabled = false;
            }
        }

        /// <summary>
        /// 开始量测
        /// </summary>
        /// <param name="strtype">距离还是面积量测方法</param>
        public void StartMeasure(string strtype)
        {
            measurePoints = new List<MapPoint>();
            map = App.mainMap;
            map.Layers.Add(graphics);
            map.Layers.Add(graphicsMouserPt);
            map.Cursor = Cursors.Arrow;
            map.MouseLeftButtonDown -= MainMap_MouseLeftButtonDown;
            map.MouseLeftButtonDown += new MouseButtonEventHandler(MainMap_MouseLeftButtonDown);
            map.MouseLeftButtonDown -= map_MouseLeftButtonDown;
            map.MouseLeftButtonDown += new MouseButtonEventHandler(map_MouseLeftButtonDown);

            drawTool = new Draw(map);
            drawTool.DrawComplete -= drawTool_DrawComplete;
            drawTool.DrawComplete += new EventHandler<DrawEventArgs>(drawTool_DrawComplete);
            drawTool.DrawBegin -= drawTool_DrawBegin;
            drawTool.DrawBegin += new EventHandler(drawTool_DrawBegin);

            //设置绘制的线样式
            drawTool.LineSymbol = linesymbol;
            //绘制的面样式
            drawTool.FillSymbol = fillsymbol;
            //设置draw不可用
            drawTool.IsEnabled = false;
            //画线
            if (strtype == "Polyline")
            {
                drawTool.IsEnabled = true;
                //设置draw的画方式是线
                drawTool.DrawMode = DrawMode.Polyline;
                //设置总长为0
                alllength = 0;
                //设置IsMeasurerLine
                IsMeasurerLine = true;
            }
            else if (strtype == "Polygon")
            {
                drawTool.IsEnabled = true;
                //设置draw的画方式是面
                drawTool.DrawMode = DrawMode.Polygon;
                //设置IsMeasurerLine
                IsMeasurerLine = false;
            }
        }

        #region 完成测量
        void geoService_LengthsCompleted(object sender, LengthsEventArgs e)
        {
            try
            {
                if (e.Results.Count == 1)
                {
                    if (istwopoint == true)
                    {
                        //增加总长
                        alllength = e.Results[0] + alllength;
                        //定义一个graphic
                        Graphic gc = new Graphic();
                        //赋予空间对象
                        gc.Geometry = m_currline.Paths[0][1];
                        //增加属性
                        gc.Attributes.Add("dict_len", KmAndM(alllength));
                        //如果为量测
                        if (IsMeasurerLine == true)
                        {
                            //设置点样式
                            gc.Symbol = MarkerSymbolMidPoint;
                        }           
                        //在点面上增加点
                        graphicsMouserPt.Graphics.Add(gc);
                        //设置当前点为起始点
                        m_currPoint = m_startpt;
                    }
                }
                else
                {
                    //报错信息
                    Message.Show("提示信息", "几何计算发生错误！");
                }
            }
            catch { }
        }

        void geoService_AreasAndLengthsCompleted(object sender, AreasAndLengthsEventArgs args)
        {
            double miles = args.Results.Lengths[0];
            double sqmi = Math.Abs(args.Results.Areas[0]);

            Graphic tmpgra = new Graphic();

            //增加面积属性
            tmpgra.Attributes.Add("dict_area", KmtAndMt(Math.Abs(args.Results.Areas[0])));
            //增加长度属性
            tmpgra.Attributes.Add("dict_len", KmAndM(Math.Abs(args.Results.Lengths[0])));
            //增加点位信息
            tmpgra.Attributes.Add("dict_gra", (graphicsMouserPt.Graphics.Count() - (gra.Geometry as ESRI.ArcGIS.Client.Geometry.Polygon).Rings[0].Count()).ToString() + ";" + graphicsMouserPt.Graphics.Count().ToString() + ";" + (graphics.Graphics.Count() - 1).ToString());
            //设置空间地形
            tmpgra.Geometry = (gra.Geometry as ESRI.ArcGIS.Client.Geometry.Polygon).Rings[0][(gra.Geometry as ESRI.ArcGIS.Client.Geometry.Polygon).Rings[0].Count() - 2];
            //设置样式
            tmpgra.Symbol = MarkerSymbolAreaEndPoint;
            //增加该图形属于的哪个map
            tmpgra.Attributes.Add("dict_sy", "map");
            //往点面上增加点
            graphicsMouserPt.Graphics.Add(tmpgra);
        }

        void geoService_ProjectCompleted(object sender, GraphicsEventArgs args)
        {
            //执行量测面积
            geoService.AreasAndLengthsAsync(args.Results, LinearUnit.Meter, LinearUnit.Meter, null);
        }
        //百度测量
        void MathLengthAndAreaForBaidu()
        {
            double[] lengthAndArea = baiduMeature.GetAreaAndLength(measurePoints.ToArray());
            double area = lengthAndArea[0];
            double length = lengthAndArea[1];

            Graphic tmpgra = new Graphic();

            //增加面积属性
            tmpgra.Attributes.Add("dict_area", KmtAndMt(area));
            //增加长度属性
            tmpgra.Attributes.Add("dict_len", KmAndM(length));
            //增加点位信息
            tmpgra.Attributes.Add("dict_gra", (graphicsMouserPt.Graphics.Count() - (gra.Geometry as ESRI.ArcGIS.Client.Geometry.Polygon).Rings[0].Count()).ToString() + ";" + graphicsMouserPt.Graphics.Count().ToString() + ";" + (graphics.Graphics.Count() - 1).ToString());
            //设置空间地形
            tmpgra.Geometry = (gra.Geometry as ESRI.ArcGIS.Client.Geometry.Polygon).Rings[0][(gra.Geometry as ESRI.ArcGIS.Client.Geometry.Polygon).Rings[0].Count() - 2];
            //设置样式
            tmpgra.Symbol = MarkerSymbolAreaEndPoint;
            //增加该图形属于的哪个map
            tmpgra.Attributes.Add("dict_sy", "map");
            //往点面上增加点
            graphicsMouserPt.Graphics.Add(tmpgra);
        }

        void MathLengthForBaidu()
        {
            MapPoint startpoint = this.measurePoints[this.measurePoints.Count - 1];
            MapPoint endpoint = this.measurePoints[this.measurePoints.Count - 2];
            BaiduMeature bm = new BaiduMeature();
            double length = baiduMeature.GetDistance(startpoint, endpoint);
            if (istwopoint == true)
            {
                //增加总长
                alllength += length;
                //定义一个graphic
                Graphic gc = new Graphic();
                //赋予空间对象
                gc.Geometry = m_currline.Paths[0][1];
                //增加属性
                gc.Attributes.Add("dict_len", KmAndM(alllength));
                //如果为量测
                if (IsMeasurerLine == true)
                {
                    //设置点样式
                    gc.Symbol = MarkerSymbolMidPoint;
                }
                //在点面上增加点
                graphicsMouserPt.Graphics.Add(gc);
                //设置当前点为起始点
                m_currPoint = m_startpt;
            }
        }

        /// <summary>
        /// 平方公里自动转换
        /// </summary>
        /// <param name="tmpdb"></param>
        /// <returns></returns>
        string KmtAndMt(double tmpdb)
        {
            //如果小于一千万平方米
            if (tmpdb < 1000 * 10000)
            {
                return tmpdb.ToString("0") + "平方米";
            }
            //转换成平方公里
            else
            {
                return (tmpdb / 1000000).ToString("0.00") + "平方公里";
            }
        }
        /// <summary>
        /// 千米自动转换
        /// </summary>
        /// <param name="tmpdb"></param>
        /// <returns></returns>
        string KmAndM(double tmpdb)
        {
            //如果小于10W米
            if (tmpdb < 100000)
            {
                return tmpdb.ToString("0") + "米";
            }
            //转换成公里
            else
            {
                return (tmpdb / 1000).ToString("0.00") + "公里";
            }
        }

        #endregion

        #region 地图左击事件
        int IntMainDraw = 0;
        /// <summary>
        /// 定义地形鼠标左击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MainMap_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if (IsSelf != true)
                    return;
                //draw禁用不执行
                if (!drawTool.IsEnabled)
                    return;
                if (IsMeasurerLine == true)
                {
                    //draw为空不执行
                    if (drawTool.DrawMode == DrawMode.None)
                    {
                        return;
                    }
                    IntMainDraw = IntMainDraw + 1;
                    if (IntMainDraw == 1)
                    {
                        //增加点集合
                        mapPoints.Add(m_startpt);
                    }
                    //设置当前线的起点给当前鼠标的左击点
                    m_startpt = map.ScreenToMap(e.GetPosition(map));
                    //增加点集合
                    mapPoints.Add(m_startpt);
                    measurePoints.Add(m_startpt);
                    //获取当前点所在点集合的位置
                    int pointIndex = mapPoints.IndexOf(m_startpt);
                    //判断是否为起点
                    if (pointIndex == 0)
                    {
                        //如果是起点
                        Graphic gc = new Graphic();
                        gc.Geometry = mapPoints[0];
                        //增加属性
                        gc.Attributes.Add("dict_len", "起点");
                        //定义起点样式
                        gc.Symbol = MarkerSymbolMidPoint;
                        //增加到点图层上
                        graphicsMouserPt.Graphics.Add(gc);
                    }
                    else
                    {
                        //判断最后两点是否相同
                        if ((mapPoints[mapPoints.Count() - 1].X == mapPoints[mapPoints.Count() - 2].X) && (mapPoints[mapPoints.Count() - 1].Y == mapPoints[mapPoints.Count() - 2].Y))
                        {
                            //清除掉倒数第二点
                            graphicsMouserPt.Graphics.RemoveAt(graphicsMouserPt.Graphics.Count() - 1);
                            //获取起始点位置
                            int FromdrawLayerMouse = graphicsMouserPt.Graphics.Count() - mapPoints.Count() + 2;
                            //获取终点位置
                            int TodrawLayerMouse = graphicsMouserPt.Graphics.Count();
                            //获取线位置
                            int FromdrawLayer = graphics.Graphics.Count();
                            //定义一个graphic
                            Graphic tmpgra = new Graphic();
                            //将最后一点的坐标赋值于他
                            tmpgra.Geometry = mapPoints[mapPoints.Count() - 1];
                            //增加长度属性
                            tmpgra.Attributes.Add("dict_len", KmAndM(alllength));
                            //增加属于map
                            tmpgra.Attributes.Add("dict_sy", "Map");
                            //增加位置信息
                            tmpgra.Attributes.Add("dict_gra", FromdrawLayerMouse.ToString() + ";" + TodrawLayerMouse.ToString() + ";" + FromdrawLayer);
                            //设置样式
                            tmpgra.Symbol = MarkerSymbolEndPoint;
                            //增加到点集合上
                            graphicsMouserPt.Graphics.Add(tmpgra);
                            //重新声明点集合
                            mapPoints = new List<MapPoint>();
                            //总长度为0
                            alllength = 0;
                            //点好为0
                            IntMainDraw = 0;
                            return;
                        }
                        //定义一个graphic集合
                        IList<Graphic> lst = new List<Graphic>();
                        //重新生成一段线段
                        ESRI.ArcGIS.Client.Geometry.PointCollection pointcol = new ESRI.ArcGIS.Client.Geometry.PointCollection();
                        //增加该线段的终点
                        pointcol.Add(mapPoints[mapPoints.Count() - 2]);
                        //增加该线段的起点
                        pointcol.Add(mapPoints[mapPoints.Count() - 1]);
                        ESRI.ArcGIS.Client.Geometry.Polyline tmppline = new ESRI.ArcGIS.Client.Geometry.Polyline();
                        //将点集合增加到path里面
                        tmppline.Paths.Add(pointcol);
                        //设置坐标参考
                        tmppline.SpatialReference = mapPoints[mapPoints.Count() - 1].SpatialReference;
                        //tmppline.SpatialReference.WKID = 4326;
                        ESRI.ArcGIS.Client.Geometry.Geometry geo = tmppline;
                        //geo.SpatialReference.WKID = 4326;
                        gra = new Graphic();
                        gra.Geometry = geo;
                        //加入到集合
                        lst.Add(gra);
                        m_currline = tmppline;
                        istwopoint = true;
                        //调用服务计算长度
                        //百度地图
                        if (PFApp.MapServerType == enumMapServerType.Baidu)
                        {
                            MathLengthForBaidu();
                        }
                        //esri地图
                        else if(PFApp.MapServerType==enumMapServerType.Esri)
                            geoService.LengthsAsync(lst, LinearUnit.Meter, true, null);
                        //geo服务发布
                        else if(geoService!=null)
                            geoService.LengthsAsync(lst, LinearUnit.Meter, true, null);
                    }
                }
                //画面
                else
                {
                    //往点集合里面增加当前坐标点
                    mapPoints.Add(map.ScreenToMap(e.GetPosition(map)));
                    measurePoints.Add(map.ScreenToMap(e.GetPosition(map)));
                    //判断如果点数不大2则返回
                    if (mapPoints.Count() > 2)
                    {
                        if ((mapPoints[mapPoints.Count() - 1].X == mapPoints[mapPoints.Count() - 2].X) && (mapPoints[mapPoints.Count() - 1].Y == mapPoints[mapPoints.Count() - 2].Y))
                        {
                            //measurePoints.Remove(measurePoints[measurePoints.Count - 1]);
                            return;
                        }
                    }
                    //新建一个graphic
                    Graphic gc = new Graphic();
                    //坐标位置为当前点位
                    gc.Geometry = map.ScreenToMap(e.GetPosition(map));
                    //gc.Attributes.Add("dict_len", "起点");
                    //设置样式
                    gc.Symbol = MarkerSymbolAreaPoint;
                    //增加节点
                    graphicsMouserPt.Graphics.Add(gc);
                }
            }
            catch
            {
                Message.Show("测量出错，请检查几何服务是否可用");
            }
        }

        void map_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                m_startpt = map.ScreenToMap(e.GetPosition(map));
                //graphic重新赋值
                gra_begin = new Graphic();
                //将空间坐标给graphic
                gra_begin.Geometry = m_startpt;
                //增加属性
                gra_begin.Attributes.Add("dict_len", "起点");
                //设置样式
                gra_begin.Symbol = MarkerSymbolMidPoint;
            }
            catch
            {
                return;
            }
        }
        #endregion

        /// <summary>
        /// 关闭面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_AreaClose_Click(object sender, RoutedEventArgs e)
        {
            //获取btn的tag值
            string[] ary = (sender as Button).Tag.ToString().Split(';');
            //遍历面的所有点对象
            for (int i = int.Parse(ary[0]) + 1; i < int.Parse(ary[1]) + 1; i++)
            {
                Graphic gra = new Graphic();
                //清除点
                graphicsMouserPt.Graphics.RemoveAt(i);
                //在原位置增加一个空对象
                graphicsMouserPt.Graphics.Insert(i, gra);
            }
            Graphic gar = new Graphic();
            //在原位置增加一个空对象
            graphics.Graphics.Insert(int.Parse(ary[2]), gar);
            //清除线
            graphics.Graphics.RemoveAt(int.Parse(ary[2]) + 1);
        }

        /// <summary>
        /// 关闭线
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_LenClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //获取btn的tag值
                string[] ary = (sender as Button).Tag.ToString().Split(';');
                //取值
                int m = Convert.ToInt32(ary[0]);
                //清空线
                if (graphics.Graphics.Count() != 0)
                {
                    //删除线
                    graphics.Graphics.RemoveAt(Convert.ToInt32(ary[2]));
                    //在原位置增加一个空对象
                    Graphic tmpgra = new Graphic();
                    graphics.Graphics.Insert(Convert.ToInt32(ary[2]), tmpgra);
                }
                //清点
                for (int i = m; i < Convert.ToInt32(ary[1]) + 1; i++)
                {
                    //删除点
                    graphicsMouserPt.Graphics.RemoveAt(i);
                    //在原位置增加一个空对象
                    Graphic tmpgra = new Graphic();
                    graphicsMouserPt.Graphics.Insert(i, tmpgra);
                }
            }
            catch
            {
            }
        }
    }
}
