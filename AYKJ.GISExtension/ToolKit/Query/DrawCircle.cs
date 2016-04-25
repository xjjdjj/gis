using System;
using System.Windows.Input;
using System.Windows.Media;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Samples;

namespace AYKJ.GISExtension
{
    /// <summary>
    /// 委托
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void DrawCircleDelegate(object sender, EventArgs e);

    public class DrawCircle
    {
        //定义事件
        public event DrawCircleDelegate DrawCircleEvent;

        private bool isActivated;
        SimpleMarkerSymbol markerSymbol;

        public MapPoint formap = new MapPoint();
        public bool mapflag = false;

        public double NumberDecimals { get; set; }
        public Map Map { get; set; }
        private GraphicsLayer GraphicsLayer { get; set; }

        public double TotalLength { get; set; }
        public double TotalArea { get; set; }
        public double len = 0;

        public FillSymbol FillSymbol { get; set; }
        public LineSymbol LineSymbol { get; set; }

        public static ESRI.ArcGIS.Client.Geometry.Polygon m_polygon = new ESRI.ArcGIS.Client.Geometry.Polygon();
        public ESRI.ArcGIS.Client.Geometry.Polygon CirclereturnPolygon { get; set; }
        GraphicsLayer CircleGra = new GraphicsLayer();

        void ProcessAction(object sender, EventArgs e)
        {
            if (DrawCircleEvent != null)
                DrawCircleEvent(sender, e);
        }

        public DrawCircle(GraphicsLayer temp_gra)
        {
            //Set up defaults
            CircleGra = temp_gra;
            //MainPage.m_map.Layers.Add(CircleGra);
            GraphicsLayer = new GraphicsLayer();
            LineSymbol = new SimpleLineSymbol()
            {
                Color = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)),
                Width = 2,
                Style = SimpleLineSymbol.LineStyle.Solid
            };
            FillSymbol = new SimpleFillSymbol()
            {
                // new SolidColorBrush(new Color() { R = 0, G = 255, B = 0, A = 40 })
                Fill = new SolidColorBrush(Color.FromArgb(0x22, 0, 255, 0)),
                BorderBrush = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)),
                BorderThickness = 2
            };
            markerSymbol = new SimpleMarkerSymbol()
            {
                Color = new SolidColorBrush(Color.FromArgb(0x66, 255, 0, 0)),
                Size = 5,
                Style = SimpleMarkerSymbol.SimpleMarkerStyle.Circle
            };
        }

        public bool IsActivated
        {
            get { return isActivated; }
            set
            {
                if (isActivated != value)
                {
                    isActivated = value;
                    if (value)
                    {
                        Map.MouseMove -= map_MouseMove;
                        Map.MouseMove += map_MouseMove;
                        Map.MouseLeftButtonUp -= map_MouseLeftButtonUp;
                        Map.MouseLeftButtonUp += map_MouseLeftButtonUp;
                        Map.Layers.Add(GraphicsLayer);
                        //Map.Cursor = Cursors.Hand;
                    }
                    else
                    {
                        Map.Cursor = Cursors.Arrow;
                        Map.MouseLeftButtonUp -= map_MouseLeftButtonUp;
                        Map.MouseMove -= map_MouseMove;
                        Map.Layers.Remove(GraphicsLayer);
                        //ResetValues();
                    }
                }
            }
        }

        public ESRI.ArcGIS.Client.Geometry.Polygon Getpolygon()
        {
            return CirclereturnPolygon;
        }

        /// <summary>
        /// 鼠标左键点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void map_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CircleGra.Graphics.Clear();
            if (mapflag == false)
            {
                formap = Map.ScreenToMap(e.GetPosition(Map));
                Graphic gra = new Graphic() { Geometry = formap, Symbol = markerSymbol };

                ESRI.ArcGIS.Client.Geometry.PointCollection points = new ESRI.ArcGIS.Client.Geometry.PointCollection();
                points.Add(formap);
                points.Add(formap);
                ESRI.ArcGIS.Client.Geometry.Polyline ply1 = new ESRI.ArcGIS.Client.Geometry.Polyline();
                ply1.Paths.Add(points);
                Graphic grane = new Graphic() { Geometry = ply1, Symbol = LineSymbol };
                CircleGra.Graphics.Add(grane);

                double xCoord = formap.X;
                double yCoord = formap.Y;
                double radius = 0;
                Circle circle = new Circle(formap.X, formap.Y, radius);

                ESRI.ArcGIS.Client.Geometry.Polygon m_circleGeometry = new ESRI.ArcGIS.Client.Geometry.Polygon();
                m_circleGeometry.Rings.Add(circle.createCirclePoints());
                CirclereturnPolygon = m_circleGeometry;

                DataQueryRadius.CirlePolygon = m_circleGeometry;
                Graphic gra1 = new Graphic() { Geometry = m_circleGeometry, Symbol = FillSymbol };
                CircleGra.Graphics.Add(gra);

                TextSymbol teSymbol = new TextSymbol()
                {
                    FontSize = 12.5,
                    FontFamily = new System.Windows.Media.FontFamily("Simsun"),
                    Foreground = new System.Windows.Media.SolidColorBrush(Colors.Red)
                };
                Graphic textGraphic = new Graphic()
                {
                    Geometry = formap,
                    Symbol = new RotatingTextSymbol()
                };
                textGraphic.SetZIndex(1);
                CircleGra.Graphics.Add(textGraphic);
                mapflag = !mapflag;
            }
            else
            {
                MapPoint maptemp = new MapPoint();
                maptemp = Map.ScreenToMap(e.GetPosition(Map));

                double xCoord = (maptemp.X + formap.X) / 2;
                double yCoord = (maptemp.Y + formap.Y) / 2;

                double radius = Math.Sqrt((maptemp.X - formap.X) * (maptemp.X - formap.X) + (maptemp.Y - formap.Y) * (maptemp.Y - formap.Y));

                Circle circle = new Circle(formap.X, formap.Y, radius);

                ESRI.ArcGIS.Client.Geometry.Polygon m_circleGeometry = new ESRI.ArcGIS.Client.Geometry.Polygon();
                m_circleGeometry.Rings.Add(circle.createCirclePoints());
                CirclereturnPolygon = m_circleGeometry;
                DataQueryRadius.CirlePolygon = m_circleGeometry;
                m_polygon = m_circleGeometry;

                //Graphic gra = new Graphic() { Geometry = m_circleGeometry, Symbol = FillSymbol };
                //CircleGra.Graphics.Add(gra);
                mapflag = !mapflag;
                ProcessAction(this, EventArgs.Empty);
            }
        }

        public ESRI.ArcGIS.Client.Geometry.Polygon Getreturngeometry
        {
            get
            {
                return m_polygon;
            }
            set
            {
                m_polygon = value;
            }
        }

        /// <summary>
        /// 鼠标移动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void map_MouseMove(object sender, MouseEventArgs e)
        {
            if (mapflag)
            {
                MapPoint pttemp = new MapPoint();

                pttemp = Map.ScreenToMap(e.GetPosition(Map));

                //   CircleGra.Graphics.Clear();
                int account = CircleGra.Graphics.Count - 1;
                //     CircleGra.Graphics.Remove(CircleGra.Graphics[account]);

                ESRI.ArcGIS.Client.Geometry.Polyline pl1 = new ESRI.ArcGIS.Client.Geometry.Polyline();
                ESRI.ArcGIS.Client.Geometry.PointCollection points = new ESRI.ArcGIS.Client.Geometry.PointCollection();
                points.Add(formap);
                points.Add(pttemp);
                pl1.Paths.Add(points);

                Graphic gra = new Graphic() { Geometry = pl1, Symbol = LineSymbol };

                (CircleGra.Graphics[0].Geometry as ESRI.ArcGIS.Client.Geometry.Polyline).Paths[0][1] = pttemp;

                double xCoord = (pttemp.X + formap.X) / 2;
                double yCoord = (pttemp.Y + formap.Y) / 2;

                double radius = Math.Sqrt((pttemp.X - formap.X) * (pttemp.X - formap.X) + (pttemp.Y - formap.Y) * (pttemp.Y - formap.Y));
                //  Circle circle = new Circle(xCoord - radius / 2, yCoord - radius / 2, radius / 2);
                Circle circle = new Circle(formap.X, formap.Y, radius);
                ESRI.ArcGIS.Client.Geometry.Polygon m_circleGeometry = new ESRI.ArcGIS.Client.Geometry.Polygon();
                m_circleGeometry.Rings.Add(circle.createCirclePoints());
                CircleGra.Graphics[1].Geometry = m_circleGeometry;
                CircleGra.Graphics[1].Symbol = FillSymbol;

                MapPoint Middlepoint = new MapPoint();
                Middlepoint.X = (formap.X + pttemp.X) / 2;
                Middlepoint.Y = (formap.Y + pttemp.Y) / 2 + 5;


                if (Map.SpatialReference.WKID > 2400 && Map.SpatialReference.WKID < 2443)
                {
                    len = DistanceOfTwoPoints(formap.X, formap.Y, pttemp.X, pttemp.Y, GaussSphere.Beijing54);
                }
                else if (Map.SpatialReference.WKID > 2326 && Map.SpatialReference.WKID < 2391)
                {
                    len = DistanceOfTwoPoints(formap.X, formap.Y, pttemp.X, pttemp.Y, GaussSphere.Xian80);
                }
                else if (Map.SpatialReference.WKID == 4326)
                {
                    len = DistanceOfTwoPoints(formap.X, formap.Y, pttemp.X, pttemp.Y, GaussSphere.WGS84);
                }
                else
                {
                    len = DistanceOfTwoPoints(formap.X, formap.Y, pttemp.X, pttemp.Y, GaussSphere.Place);
                }
                string strLen = Math.Round(len, 4).ToString() + " 米";



                double angle = Math.Atan2((pttemp.X - formap.X), (pttemp.Y - formap.Y)) / Math.PI * 180 - 90;
                if (angle > 90 || angle < -90) angle -= 180;
                RotatingTextSymbol symb = CircleGra.Graphics[2].Symbol as RotatingTextSymbol;
                symb.Angle = angle;
                symb.Text = strLen.ToString();


                Graphic txtgra = new Graphic();
                MapPoint txtmp = new MapPoint() { X = Middlepoint.X, Y = Middlepoint.Y, SpatialReference = Map.SpatialReference };
                txtgra.Geometry = txtmp;
                txtgra.Symbol = symb;
                //GraphicsLayer.Graphics.Add(txtgra);

                //(CircleGra.Graphics[1].Geometry as ESRI.ArcGIS.Client.Geometry.Polygon).Rings[0] = circle.createCirclePoints();
                //int countn = (CircleGra.Graphics[1].Geometry as ESRI.ArcGIS.Client.Geometry.Polygon).Rings[0].Count;
                //CircleGra.Graphics.Add(gra);
                //GraphicsLayer.Graphics.Add(gra);
            }
        }

        public static double DistanceOfTwoPoints(double lng1, double lat1, double lng2, double lat2, GaussSphere gs)
        {
            if (gs == GaussSphere.Place)
            {
                return Math.Sqrt((lng1 - lng2) * (lng1 - lng2) + (lat1 - lat2) * (lat1 - lat2));
            }
            else
            {
                double radLat1 = Rad(lat1);
                double radLat2 = Rad(lat2);
                double a = radLat1 - radLat2;
                double b = Rad(lng1) - Rad(lng2);
                double s = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(a / 2), 2) + Math.Cos(radLat1) * Math.Cos(radLat2) * Math.Pow(Math.Sin(b / 2), 2)));
                s = s * (gs == GaussSphere.WGS84 ? 6378137.0 : (gs == GaussSphere.Xian80 ? 6378140.0 : 6378245.0));
                s = Math.Round(s * 10000) / 10000;
                return s;
            }
        }

        private static double Rad(double d)
        {
            return d * Math.PI / 180.0;
        }

        /// <summary>
        /// 坐标参考
        /// </summary>
        public enum GaussSphere
        {
            Beijing54,
            Xian80,
            WGS84,
            Place,
        }
    }
}
