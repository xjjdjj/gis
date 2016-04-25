using System;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using System.Windows;

namespace AYKJ.GISDevelop.Platform
{
    //ArcGIS Server API V2.4
    public class ArcGISTiledLayerForBaidu : ArcGISTiledMapServiceLayer
    {
        //      切图的范围，也就是FullExtent；
        private double coor = 20037508.3427892;
        //      SpatialReference；
        private int _wkid = 102100;
        //      TileInfo，包括切图的大小，级数，以及每级的Resolution；
        private TileInfo tileInfo;
        //        5、最后就是重写GetTileUrl方法。
        public string MapUrl;
        // 百度地图类别


        public override void Initialize()
        {
            this.FullExtent = new Envelope(-21889024, 3883008, 1703936, 16269312);//(-coor, -coor, coor, coor);

            this.SpatialReference = new SpatialReference(this._wkid);
            //this.InitialExtent = this.FullExtent;
            this.TileInfo = tileInfo = new TileInfo()
            {
                Height = 256,
                Width = 256,
                //(-coor, coor)
                Origin = new MapPoint(-20037508.342787, 20037508.342787)
                {
                    SpatialReference = new SpatialReference(this._wkid)
                },
                Lods = new Lod[20]//[14]
            };

            double resolution = Math.Pow(2, 14);//156543.033928;
            for (int i = 0; i < TileInfo.Lods.Length; i++)
            {
                TileInfo.Lods[i] = new Lod() { Resolution = resolution };
                resolution /= 2;
            }
            this.Url = "http://services.arcgisonline.com/ArcGIS/rest/services/World_Street_Map/MapServer";
            try
            {
                base.Initialize();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public override string GetTileUrl(int level, int row, int col)
        {
            int zoom = level - 1;
            int offsetX = (int)Math.Pow(2, zoom);
            int offsetY = offsetX - 1;
            int numX = col - offsetX;
            int numY = (-row) + offsetY;
            zoom = level + 1;
            int num = (col + row) % 8 + 1;
            string url = string.Empty;
            if (MapUrl == "矢量")
            {
                url = "http://shangetu1.map.bdimg.com/it/u=x=" + numX + ";y=" + numY + ";z=" + zoom + ";v=017;type=web&fm=44&udt=20130712";
            }
            else if (MapUrl == "栅格")
            {
                url = "http://shangetu0.map.bdimg.com/it/u=x=" + numX + ";y=" + numY + ";z=" + zoom + ";v=009;type=sate&fm=46&udt=20130506";
            }
            else if (MapUrl == "路网")
            {
                url = "http://online0.map.bdimg.com/tile/?qt=tile&x=" + numX + "&y=" + numY + "&z=" + zoom + "&styles=sl&v=015&udt=20130509";
            }
            return url;
        }
    }
    //ArcGIS Server API V3.1
    public class TiledLayerForBaidu : TiledMapServiceLayer
    {
        //      切图的范围，也就是FullExtent；
        private double coor = 20037508.3427892;
        //      SpatialReference；
        private int _wkid = 102100;
        //      TileInfo，包括切图的大小，级数，以及每级的Resolution；
        private TileInfo tileInfo;
        //        5、最后就是重写GetTileUrl方法。
        public string MapUrl;
        // 百度地图类别


        public override void Initialize()
        {
            this.FullExtent = new Envelope(0, 0, coor, coor);//(-coor, -coor, coor, coor);
            this.SpatialReference = new SpatialReference(this._wkid);
            //this.InitialExtent = this.FullExtent;
            this.TileInfo = tileInfo = new TileInfo()
            {
                Height = 256,
                Width = 256,
                //(-coor, coor)
                Origin = new MapPoint(-coor, coor)
                {
                    SpatialReference = new SpatialReference(this._wkid)
                },
                Lods = new Lod[19]//[14]
            };
            double resolution = coor * 2 / 256;
            for (int i = 0; i < TileInfo.Lods.Length; i++)
            {
                //TileInfo.Lods[i] = new Lod() { Resolution = Math.Pow(2, 18 - i) };
                TileInfo.Lods[i] = new Lod() { Resolution = resolution };
                resolution /= 2;
            }
            try
            {
                base.Initialize();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public override string GetTileUrl(int level, int row, int col)
        {
            int zoom = level - 1;
            int offsetX = (int)Math.Pow(2, zoom);
            int offsetY = offsetX - 1;
            int numX = col - offsetX;
            int numY = (-row) + offsetY;
            zoom = level + 1;
            int num = (col + row) % 8 + 1;
            string url = string.Empty;
            if (MapUrl == "矢量")
            {
                url = "http://shangetu1.map.bdimg.com/it/u=x=" + numX + ";y=" + numY + ";z=" + zoom + ";v=017;type=web&fm=44&udt=20130712";
            }
            else if (MapUrl == "栅格")
            {
                url = "http://shangetu0.map.bdimg.com/it/u=x=" + numX + ";y=" + numY + ";z=" + zoom + ";v=009;type=sate&fm=46&udt=20130506";
            }
            else if (MapUrl == "路网")
            {
                url = "http://online0.map.bdimg.com/tile/?qt=tile&x=" + numX + "&y=" + numY + "&z=" + zoom + "&styles=sl&v=015&udt=20130509";
            }
            return url;
        }
    }

    public class BaiduMeature
    {
        public const double EARTHRADIUS = 6370996.81;
        public const double BAIDURADIUS = 156543.03392804;//131072;

        public double GetDistance(MapPoint p1, MapPoint p2)
        {
            //从百度墨卡托近似转换到经纬度
            double x1 = p1.X / BAIDURADIUS;
            double y1 = p1.Y / BAIDURADIUS;
            double x2 = p2.X / BAIDURADIUS;
            double y2 = p2.Y / BAIDURADIUS;

            //控制经纬度范围
            x1 = getLoop(x1, -180, 180);
            y1 = getRange(y1, -74, 74);
            x2 = getLoop(x2, -180, 180);
            y2 = getRange(y2, -74, 74);

            return getDistance(toRadians(x1), toRadians(x2), toRadians(y1), toRadians(y2));
        }

        public double[] GetAreaAndLength(MapPoint[] points)
        {
            double[] rtn = new double[2];
            MapPoint p = new MapPoint(0, 0);
            int i = 0;
            double area = getArea(points, "m");
            double lenth = 0;
            while (true)
            {
                if (i == points.Length - 1)
                    break;

                lenth += GetDistance(points[i], points[i + 1]);
                i++;
            }
            lenth += GetDistance(points[i], points[0]);
            rtn[0] = Math.Abs(area);
            rtn[1] = lenth;
            return rtn;
        }

        double getArea(MapPoint[] rPoints, string unit)
        {
            double tRadius = EARTHRADIUS;//地球半径
            int tCount = rPoints.Length;
            double delta = Math.PI / 180.0;

            double area = 0.0;
            if (unit == "m")
            {
                for (int i = 0; i < tCount - 1; i++)
                {
                    double p1x = rPoints[i].X;
                    double p1y = rPoints[i].Y;
                    double p2x = rPoints[i + 1].X;
                    double p2y = rPoints[i + 1].Y;
                    area += (p1x * p2y - p2x * p1y);
                }
                area += rPoints[tCount - 1].X * rPoints[0].Y - rPoints[tCount - 1].Y * rPoints[0].X;
                return area / 4;
            }
            else
            {
                //经纬度坐标下的球面多边形
                double xA = 0.0;
                double yA = 0.0;
                double xB = 0.0; //球面角度，角ABC,是交点B的两条切线夹角
                double yB = 0.0;
                double xC = 0.0;
                double yC = 0.0;

                double anglesum = 0;
                double sum1 = 0;
                double sum2 = 0;
                int count1 = 0;
                int count2 = 0;
                for (int i = 0; i != tCount; ++i)
                {
                    #region 百度墨卡托到经纬度到弧度
                    //处理末尾点起始点和第二点夹角
                    if (i == 0)
                    {
                        xA = rPoints[tCount - 1].X / BAIDURADIUS * delta;
                        yA = rPoints[tCount - 1].Y / BAIDURADIUS * delta;
                        xB = rPoints[0].X / BAIDURADIUS * delta;
                        yB = rPoints[0].Y / BAIDURADIUS * delta;
                        xC = rPoints[1].X / BAIDURADIUS * delta;
                        yC = rPoints[1].Y / BAIDURADIUS * delta;
                    }//处理倒数第二点和最后点和起始点夹角
                    else if (i == tCount - 1)
                    {
                        xA = rPoints[tCount - 2].X / BAIDURADIUS * delta;
                        yA = rPoints[tCount - 2].Y / BAIDURADIUS * delta;
                        xB = rPoints[tCount - 1].X / BAIDURADIUS * delta;
                        yB = rPoints[tCount - 1].Y / BAIDURADIUS * delta;
                        xC = rPoints[0].X / BAIDURADIUS * delta;
                        yC = rPoints[0].Y / BAIDURADIUS * delta;
                    }//处理其余点
                    else
                    {
                        xA = rPoints[i - 1].X / BAIDURADIUS * delta;
                        yA = rPoints[i - 1].Y / BAIDURADIUS * delta;
                        xB = rPoints[i].X / BAIDURADIUS * delta;
                        yB = rPoints[i].Y / BAIDURADIUS * delta;
                        xC = rPoints[i + 1].X / BAIDURADIUS * delta;
                        yC = rPoints[i + 1].Y / BAIDURADIUS * delta;
                    }
                    #endregion

                    double am = Math.Cos(yB) * Math.Cos(xB);
                    double bm = Math.Cos(yB) * Math.Sin(xB);
                    double cm = Math.Sin(yB);

                    double al = Math.Cos(yA) * Math.Cos(xA);
                    double bl = Math.Cos(yA) * Math.Sin(xA);
                    double cl = Math.Sin(yA);

                    double ah = Math.Cos(yC) * Math.Cos(xC);
                    double bh = Math.Cos(yC) * Math.Sin(xC);
                    double ch = Math.Sin(yC);

                    double cfl = (am * am + bm * bm + cm * cm) / (am * al + bm * bl + cm * cl);
                    double cfh = (am * am + bm * bm + cm * cm) / (am * ah + bm * bh + cm * ch);

                    double alt = cfl * al - am;
                    double blt = cfl * bl - bm;
                    double clt = cfl * cl - cm;

                    double aht = cfh * ah - am;
                    double bht = cfh * bh - bm;
                    double cht = cfh * ch - cm;

                    double angleCos = (aht * alt + bht * blt + cht * clt) / (Math.Sqrt(aht * aht + bht * bht + cht * cht) * Math.Sqrt(alt * alt + blt * blt + clt * clt));
                    angleCos = Math.Acos(angleCos);

                    double anl = bht * clt - cht * blt;
                    double bnl = 0 - (aht * clt - cht * alt);
                    double cnl = aht * blt - bht * alt;

                    double oritentationValue = 0;
                    if (am != 0)
                        oritentationValue = anl / am;
                    else if (bm != 0)
                        oritentationValue = bnl / bm;
                    else
                        oritentationValue = cnl / cm;

                    if (oritentationValue > 0)
                    {
                        sum1 += angleCos;
                        count1++;
                    }
                    else
                    {
                        sum2 += angleCos;
                        count2++;
                    }
                }
                if (sum1 > sum2)
                    anglesum = sum1 + (2 * Math.PI * count2 - sum2);
                else
                    anglesum = (2 * Math.PI * count1 - sum1) + sum2;
                //球面n边形面积公式
                //S=R^2 *[A1+A2+……+An-(n-2)π]其中Ai为球面n边形的第i个内角
                return (anglesum - (tCount - 2) * 3.1415926) * tRadius * tRadius;
            }
        }

        /// <summary>
        /// 控制经度在范围b c 内
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        double getLoop(double a, double b, double c)
        {

            while (a > c)
            {
                a -= c - b;
            }

            while (a < b)
            {
                a += c - b;
            }
            return a;
        }

        /// <summary>
        /// 控制纬度在范围b c 内
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        double getRange(double a, double b, double c)
        {
            if (b != 1)
            {
                if (b > a)
                    a = b;
            }
            if (c != 1)
            {
                if (c < a)
                    a = c;
            }
            return a;
        }

        /// <summary>
        /// 将°转成弧度
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        double toRadians(double a)
        {
            return Math.PI * a / 180;
        }

        /// <summary>
        /// 返回距离
        /// </summary>
        /// <param name="a">x1</param>
        /// <param name="b">y1</param>
        /// <param name="c">x2</param>
        /// <param name="d">y2</param>
        /// <returns></returns>
        double getDistance(double a, double b, double c, double d)
        {
            double p = Math.Sin(c) * Math.Sin(d) + Math.Cos(c) * Math.Cos(d) * Math.Cos(b - a);
            return EARTHRADIUS * Math.Acos(p);
        }

    }

    #region baidu
    public class BaiduMapLayer : TiledMapServiceLayer
    {
        string type = "web";// web|sate
        string udt = "20130712";
        string v = "017";
        string fm = "44";
        public string UriPattern = "http://shangetu{0}.map.bdimg.com/it/u=x={5};y={6};z={7};v={1};type={2}&fm={3}&udt={4} ";
        public bool buri = false;

        protected virtual string[] TileServerNums { get; set; }

        protected virtual string GetServerNum(int x, int y)
        {
            if (TileServerNums == null || TileServerNums.Length == 0) return null;
            return TileServerNums[Math.Abs(x + 2 * y) % TileServerNums.Length];
        }

        public override void Initialize()
        {
            TileServerNums = new string[] { "0", "1", "2", "3" };

            this.FullExtent = new ESRI.ArcGIS.Client.Geometry.Envelope(-Math.Pow(2, 25), -Math.Pow(2, 25), Math.Pow(2, 25), Math.Pow(2, 25))
            {
                SpatialReference = new SpatialReference(102113)
            };
            // This layer's spatial reference
            this.SpatialReference = new SpatialReference(102113);
            // Set up tile information. Each tile is 256x256px, 19 levels.
            this.TileInfo = new TileInfo()
            {
                Height = 256,
                Width = 256,
                Origin = new MapPoint(-Math.Pow(2, 25), Math.Pow(2, 25)) { SpatialReference = new ESRI.ArcGIS.Client.Geometry.SpatialReference(102113) },
                SpatialReference = new ESRI.ArcGIS.Client.Geometry.SpatialReference(102113),
                Lods = new Lod[17]//20131008：修改"19"->"17"。
            };

            // Set the resolutions for each level. Each level is half the resolution of the previous one.
            double resolution = Math.Pow(2, 15);
            for (int i = 0; i < TileInfo.Lods.Length; i++)
            {
                TileInfo.Lods[i] = new Lod() { Resolution = resolution };
                resolution /= 2;
            }
            if (UriPattern.Contains("%20"))
            {
               UriPattern= UriPattern.Replace("%20", "&");
            }

            base.Initialize();

        }

        public override string GetTileUrl(int level, int row, int col)
        {
            double res = this.TileInfo.Lods[level].Resolution;
            //col = col - (int)(Math.Pow(2, 25) / 256 / res);
            //row = row - (int)(Math.Pow(2, 25) / 256 / res);
            col = col - (int)(Math.Pow(2, 17) / res);
            row = row - (int)(Math.Pow(2, 17) / res);
            level = level + 3;
            row = -1 - row;
            string url = "";
            if (buri)
            {
                url = string.Format(UriPattern, GetServerNum(col, row), col >= 0 ? col.ToString() : ("M" + (-col)), row >= 0 ? row.ToString() : ("M" + (-row)), level);
            }
            else
                url = string.Format(UriPattern, GetServerNum(col, row), v, type, fm, udt, col >= 0 ? col.ToString() : ("M" + (-col)), row >= 0 ? row.ToString() : ("M" + (-row)), level);
            return url;
        }
    }

    public class Coordinate
    {
        public Coordinate()
        {
        }
        public Coordinate(double x, double y)
        {
            X = x;
            Y = y;
        }
        public double X { get; set; }
        public double Y { get; set; }
    }

    public class BaiduProjector
    {
        static double[] MCBAND = new double[] { 12890594.86, 8362377.87, 5591021, 3481989.83, 1678043.12, 0 };
        static double[] LLBAND = new double[] { 75, 60, 45, 30, 15, 0 };
        static double[][] MC2LL = new double[6][]{
            new double[10]{1.410526172116255e-8, 0.00000898305509648872, -1.9939833816331, 200.9824383106796, -187.2403703815547, 91.6087516669843, -23.38765649603339, 2.57121317296198, -0.03801003308653, 17337981.2}, 
		    new double[10]{-7.435856389565537e-9, 0.000008983055097726239, -0.78625201886289, 96.32687599759846, -1.85204757529826, -59.36935905485877, 47.40033549296737, -16.50741931063887, 2.28786674699375, 10260144.86}, 
		    new double[10]{-3.030883460898826e-8, 0.00000898305509983578, 0.30071316287616, 59.74293618442277, 7.357984074871, -25.38371002664745, 13.45380521110908, -3.29883767235584, 0.32710905363475, 6856817.37}, 
		    new double[10]{-1.981981304930552e-8, 0.000008983055099779535, 0.03278182852591, 40.31678527705744, 0.65659298677277, -4.44255534477492, 0.85341911805263, 0.12923347998204, -0.04625736007561, 4482777.06}, 
		    new double[10]{3.09191371068437e-9, 0.000008983055096812155, 0.00006995724062, 23.10934304144901, -0.00023663490511, -0.6321817810242, -0.00663494467273, 0.03430082397953, -0.00466043876332, 2555164.4},
		    new double[10]{2.890871144776878e-9, 0.000008983055095805407, -3.068298e-8, 7.47137025468032, -0.00000353937994, -0.02145144861037, -0.00001234426596, 0.00010322952773, -0.00000323890364, 826088.5}
        };
        static double[][] LL2MC = new double[6][]{
		    new double[]{-0.0015702102444, 111320.7020616939, 1704480524535203, -10338987376042340, 26112667856603880, -35149669176653700, 26595700718403920, -10725012454188240, 1800819912950474, 82.5}, 
		    new double[]{0.0008277824516172526, 111320.7020463578, 647795574.6671607, -4082003173.641316, 10774905663.51142, -15171875531.51559, 12053065338.62167, -5124939663.577472, 913311935.9512032, 67.5},
		    new double[]{0.00337398766765, 111320.7020202162, 4481351.045890365, -23393751.19931662, 79682215.47186455, -115964993.2797253, 97236711.15602145, -43661946.33752821, 8477230.501135234, 52.5}, 
		    new double[]{0.00220636496208, 111320.7020209128, 51751.86112841131, 3796837.749470245, 992013.7397791013, -1221952.21711287, 1340652.697009075, -620943.6990984312, 144416.9293806241, 37.5},
		    new double[]{-0.0003441963504368392, 111320.7020576856, 278.2353980772752, 2485758.690035394, 6070.750963243378, 54821.18345352118, 9540.606633304236, -2710.55326746645, 1405.483844121726, 22.5}, 
		    new double[]{-0.0003218135878613132, 111320.7020701615, 0.00369383431289, 823725.6402795718, 0.46104986909093, 2351.343141331292, 1.58060784298199, 8.77738589078284, 0.37238884252424, 7.45}
        };

        static Coordinate Convertor(Coordinate cD, double[] cE)
        {
            var T = cE[0] + cE[1] * Math.Abs(cD.X);
            var cC = Math.Abs(cD.Y) / cE[9];
            var cF = cE[2] + cE[3] * cC + cE[4] * cC * cC + cE[5] * cC * cC * cC + cE[6] * cC * cC * cC * cC + cE[7] * cC * cC * cC * cC * cC + cE[8] * cC * cC * cC * cC * cC * cC;
            T *= (cD.X < 0 ? -1 : 1);
            cF *= (cD.Y < 0 ? -1 : 1);
            return new Coordinate() { X = T, Y = cF };
        }

        public static Coordinate ConvertLL2MC(Coordinate T)
        {
            double[] cE = null;
            if (T.X > 180)
            {
                while (T.X > 180) T.X -= 360;
            }
            if (T.X < -180)
            {
                while (T.X < -180) T.X += 360;
            }
            if (T.Y > 74) T.Y = 74;
            if (T.Y < -74) T.Y = -74;
            for (var cD = 0; cD < LLBAND.Length; cD++)
            {
                if (T.Y >= LLBAND[cD])
                {
                    cE = LL2MC[cD];
                    break;
                }
            }
            if (cE == null)
            {
                for (var cD = LLBAND.Length - 1; cD >= 0; cD--)
                {
                    if (T.Y <= -LLBAND[cD])
                    {
                        cE = LL2MC[cD];
                        break;
                    }
                }
            }
            var cF = Convertor(T, cE);
            return cF;
        }

        public static Coordinate ConvertMC2LL(Coordinate cC)
        {
            double[] cF = null;
            var cD = new Coordinate() { X = Math.Abs(cC.X), Y = Math.Abs(cC.Y) };
            for (var cE = 0; cE < MCBAND.Length; cE++)
            {
                if (cD.Y >= MCBAND[cE])
                {
                    cF = MC2LL[cE];
                    break;
                }
            }
            var T = Convertor(cC, cF);
            return T;
        }
    }
    #endregion
}
