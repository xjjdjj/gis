/// <summary>  
/// 作者：陈锋 
/// 时间：2012/5/21 15:55:09  
/// 公司:南京安元科技有限公司  
/// 版权：2012-2020  
/// CLR版本：4.0.30319.261  
/// DrawTool说明：画圆的工具
/// 唯一标识：1190aecb-2f61-459d-9593-f907ebd33e2b  
/// </summary>

using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;

namespace AYKJ.GISDevelop.Platform.ToolKit
{
    public class DrawTool
    {
        private static GraphicsLayer graphicLayer;
        private MapPoint startMapPoint;
        private PointCollection circlePoints;
        private Polygon circleGeometry;
        private Graphic circleGraphic;
        private bool DisplayRadius = false;


        private const int NUM = 360;
        //组合对象的一部分
        private Draw draw;

        #region 组合开始画的事件
        /// <summary>
        /// 组合开始画的事件
        /// </summary>
        private event EventHandler drawBegin;
        public event EventHandler DrawBegin
        {
            add
            {
                this.drawBegin += value;
                draw.DrawBegin += value;
            }
            remove
            {
                this.drawBegin -= value;
                draw.DrawBegin -= value;
            }
        }
        #endregion

        #region 组合画结束的事件
        /// <summary>
        /// 组合画结束的事件
        /// </summary>
        private event EventHandler<DrawEventArgs> drawComplete;
        public event EventHandler<DrawEventArgs> DrawComplete
        {
            add
            {
                this.drawComplete += value;
                draw.DrawComplete += value;
            }
            remove
            {
                this.drawComplete -= value;
                draw.DrawComplete -= value;
            }
        }
        #endregion

        #region 组合初始化方法
        /// <summary>
        /// 初始化方法
        /// </summary>
        public DrawTool()
        {
            draw = new Draw();
        }


        /// <summary>
        ///  初始化方法
        /// </summary>
        /// <param name="map"></param>
        public DrawTool(Map map)
        {
            draw = new Draw(map);
            this.map = map;
        }
        #endregion

        #region 画的图形的方式
        /// <summary>
        /// 画的图形的方式
        /// </summary>
        private DrawToolMode drawMode;
        public DrawToolMode DrawMode
        {
            get
            {
                return drawMode;
            }
            set
            {
                drawMode = value;
                if (drawMode != DrawToolMode.Circle)
                {
                    draw.DrawMode = (DrawMode)value;
                }
                if (drawMode == DrawToolMode.Circle)
                {
                    circlePoints = new PointCollection();
                    circleGeometry = new Polygon();
                    circleGeometry.Rings = new ObservableCollection<PointCollection>();
                    circleGeometry.Rings.Add(circlePoints);
                    draw.DrawMode = ESRI.ArcGIS.Client.DrawMode.Rectangle;
                }
            }
        }
        #endregion

        #region 设置是否有效
        /// <summary>
        /// 设置是否有效
        /// </summary>
        private bool isEnabled;
        public bool IsEnabled
        {
            get
            {
                return isEnabled;
            }
            set
            {
                isEnabled = value;
                draw.IsEnabled = value;
                if (value)
                {
                    map.MouseLeftButtonDown += new MouseButtonEventHandler(map_MouseLeftButtonDown);
                }
            }
        }
        #endregion

        #region 设置地图
        /// <summary>
        /// 
        /// </summary>
        private Map map;
        public Map Map
        {
            get { return map; }
            set { map = value; draw.Map = value; }
        }

        #endregion

        private void map_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (drawMode != DrawToolMode.Circle)
            {
                return;
            }
            //if (drawMode == DrawToolMode.Circle)
            //{
            //    draw.DrawMode = ESRI.ArcGIS.Client.DrawMode.Rectangle;
            //}

            //保留第一个点
            Point point = e.GetPosition(null);
            startMapPoint = map.ScreenToMap(point);

            //园形 
            //circleGeometry.SpatialReference = map.SpatialReference;
            //促发开始画的事件
            //drawBegin(this,new EventArgs());
            //this.dispatchEvent(new DrawEvent("drawStart", m_graphic));

            //Graphic图层
            if (graphicLayer == null)
            {
                graphicLayer = new GraphicsLayer();
                map.Layers.Add(graphicLayer);
            }
            if (!map.Layers.Contains(graphicLayer))
            {
                map.Layers.Add(graphicLayer);
            }
            circleGraphic = new Graphic();
            circleGraphic.Geometry = circleGeometry;
            circleGraphic.Symbol = new FillSymbol()
            {
                BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red) { },
                BorderThickness = 2,
                Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green) { Opacity = 1 }
            };
            graphicLayer.Graphics.Add(circleGraphic);

            //gra.Graphics.Add(m_graphic);

            //if (displayRadius)
            //{
            //    //create the point for the text showing the radius
            //    MapPoint textPoint = new MapPoint(m_center.X, m_center.Y);
            //    m_textGraphic = new Graphic();
            //    m_textGraphic.Geometry = textPoint;
            //    m_textGraphic.Symbol = new SimpleMarkerSymbol()
            //    {
            //        Color = new SolidColorBrush(Colors.Red),
            //        Size = 10,
            //        Style = ESRI.ArcGIS.Client.Symbols.SimpleMarkerSymbol.SimpleMarkerStyle.Circle
            //    };
            //    gra.Graphics.Add(m_textGraphic);
            //}

            map.MouseMove += new MouseEventHandler(map_MouseMove);
            map.MouseLeftButtonUp += new MouseButtonEventHandler(map_MouseLeftButtonUp);
        }

        private void map_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            map.MouseMove -= new MouseEventHandler(map_MouseMove);
            map.MouseLeftButtonUp -= new MouseButtonEventHandler(map_MouseLeftButtonUp);
            map.MouseLeftButtonDown -= new MouseButtonEventHandler(map_MouseLeftButtonDown);
            DrawEventArgs dea = new DrawEventArgs();

            //dea.Geometry = circleGeometry;
            //graphicLayer.ClearGraphics();
            drawComplete(this, dea);
        }

        private void map_MouseMove(object sender, MouseEventArgs e)
        {
            Point point = e.GetPosition(null);
            MapPoint mpt = map.ScreenToMap(point);
            double radius = calculateRadius(mpt);

            circleGeometry.Rings.Clear();


            circleGeometry.Rings.Add(createCirclePoints(mpt, radius));

        }


        #region 圆周上的点
        /// <summary>
        /// 创建圆周点序列
        /// </summary>
        /// <returns></returns>
        private PointCollection createCirclePoints(MapPoint star, double radius)
        {
            double cosinus;
            double sinus;
            double x;
            double y;

            circlePoints.Clear();
            for (int i = 0; i < NUM; i++)
            {
                sinus = Math.Sin((Math.PI * 2.0) * (i / NUM));
                cosinus = Math.Cos((Math.PI * 2.0) * (i / NUM));
                x = star.X + radius * cosinus;
                y = star.Y + radius * sinus;
                circlePoints.Add(new MapPoint(x, y));
            }
            circlePoints.Add(circlePoints[0]);
            return circlePoints;
        }
        #endregion

        #region 计算圆周的半径
        /// <summary>
        /// 计算半径
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        private double calculateRadius(MapPoint point)
        {
            double dx = point.X - startMapPoint.X;
            double dy = point.Y - startMapPoint.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
        #endregion
    }
    //public class DrawEventArgs
    //{
    //}

    /// <summary>
    /// 画的方式，添加了圆的功能
    /// </summary>
    public enum DrawToolMode
    {
        Freehand = DrawMode.Freehand,
        None = DrawMode.None,
        Point = DrawMode.Point,
        Polygon = DrawMode.Polygon,
        Polyline = DrawMode.Polyline,
        Rectangle = DrawMode.Rectangle,
        Circle,
    }
}
