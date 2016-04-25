using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using ESRI.ArcGIS.Client.Geometry;
using System.Collections.Generic;
using ESRI.ArcGIS.Client;

namespace AYKJ.GISDevelop.Platform.ToolKit.ToolKitExtentLayer
{
    public class ToolKitForNonArcGISLayer
    {
        /// <summary>
        /// 黄金分割
        /// </summary>
        public static double GoldenRatio = 0.618;
        /// <summary>
        /// 贝叶斯曲线期望点数
        /// </summary>
        public static int BezierExpPointNum = 100;

        #region 简单拓扑
        /// <summary>
        /// 点是否在多边形中
        /// </summary>
        /// <param name="region"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static bool CheckRegion(Polygon region, MapPoint point)
        {
            PointCollection points = region.Rings[0];

            double pValue = double.NaN;
            int i = 0;
            int j = 0;

            double yValue = double.NaN;
            int m = 0;
            int n = 0;

            double iPointX = double.NaN;
            double iPointY = double.NaN;
            double jPointX = double.NaN;
            double jPointY = double.NaN;

            int k = 0;
            int p = 0;

            yValue = points[0].Y - points[(points.Count - 1)].Y;
            if (yValue < 0)
            {
                p = 1;
            }
            else if (yValue > 0)
            {
                p = 0;
            }
            else
            {
                m = points.Count - 2;
                n = m + 1;
                while (points[m].Y == points[n].Y)
                {
                    m--;
                    n--;
                    if (m == 0)
                    {
                        return true;
                    }
                }
                yValue = points[n].Y - points[m].Y;
                if (yValue < 0)
                {
                    p = 1;
                }
                else if (yValue > 0)
                {
                    p = 0;
                }
            }


            //使多边形封闭
            int count = points.Count;
            i = 0;
            j = count - 1;
            while (i < count)
            {
                iPointX = points[j].X;
                iPointY = points[j].Y;
                jPointX = points[i].X;
                jPointY = points[i].Y;
                if (point.Y > iPointY)
                {
                    if (point.Y < jPointY)
                    {
                        pValue = (point.Y - iPointY) * (jPointX - iPointX) / (jPointY - iPointY) + iPointX;
                        if (point.X < pValue)
                        {
                            k++;
                        }
                        else if (point.X == pValue)
                        {
                            return true;
                        }
                    }
                    else if (point.X == jPointY)
                    {
                        p = 0;
                    }
                }
                else if (point.Y < iPointY)
                {
                    if (point.Y > jPointY)
                    {
                        pValue = (point.Y - iPointY) * (jPointX - iPointX) / (jPointY - iPointY) + iPointX;
                        if (point.X < pValue)
                        {
                            k++;
                        }
                        else if (point.X == pValue)
                        {
                            return true;
                        }
                    }
                    else if (point.Y == jPointY)
                    {
                        p = 1;
                    }
                }
                else
                {
                    if (point.X == iPointX)
                    {
                        return true;
                    }
                    if (point.Y < jPointY)
                    {
                        if (p != 1)
                        {
                            if (point.X < iPointX)
                            {
                                k++;
                            }
                        }
                    }
                    else if (point.Y > jPointY)
                    {
                        if (p > 0)
                        {
                            if (point.X < iPointX)
                            {
                                k++;
                            }
                        }
                    }
                    else
                    {
                        if (point.X > iPointX && point.X <= jPointX)
                        {
                            return true;
                        }
                        if (point.X < iPointX && point.X >= jPointX)
                        {
                            return true;
                        }
                    }
                }
                j = i;
                i++;
            }

            if (k % 2 != 0)
            {
                return true;
            }
            return false;
        }
        #endregion

        #region 简单拓扑
        /// <summary>
        /// 点是否在某一些多边形中
        /// </summary>
        /// <param name="region"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static int CheckMutilRegion(List<Polygon> list_region, MapPoint point)
        {
            int index = -1;//如果返回-1，表示该点不在这些多边形中。否则返回该点所在多边形的序号。
            for (int q = 0; q < list_region.Count;q++ )
            {
                Polygon region = list_region[q];

                PointCollection points = region.Rings[0];

                double pValue = double.NaN;
                int i = 0;
                int j = 0;

                double yValue = double.NaN;
                int m = 0;
                int n = 0;

                double iPointX = double.NaN;
                double iPointY = double.NaN;
                double jPointX = double.NaN;
                double jPointY = double.NaN;

                int k = 0;
                int p = 0;

                yValue = points[0].Y - points[(points.Count - 1)].Y;
                if (yValue < 0)
                {
                    p = 1;
                }
                else if (yValue > 0)
                {
                    p = 0;
                }
                else
                {
                    m = points.Count - 2;
                    n = m + 1;
                    while (points[m].Y == points[n].Y)
                    {
                        m--;
                        n--;
                        if (m == 0)
                        {
                            return q;
                        }
                    }
                    yValue = points[n].Y - points[m].Y;
                    if (yValue < 0)
                    {
                        p = 1;
                    }
                    else if (yValue > 0)
                    {
                        p = 0;
                    }
                }


                //使多边形封闭
                int count = points.Count;
                i = 0;
                j = count - 1;
                while (i < count)
                {
                    iPointX = points[j].X;
                    iPointY = points[j].Y;
                    jPointX = points[i].X;
                    jPointY = points[i].Y;
                    if (point.Y > iPointY)
                    {
                        if (point.Y < jPointY)
                        {
                            pValue = (point.Y - iPointY) * (jPointX - iPointX) / (jPointY - iPointY) + iPointX;
                            if (point.X < pValue)
                            {
                                k++;
                            }
                            else if (point.X == pValue)
                            {
                                return q;
                            }
                        }
                        else if (point.X == jPointY)
                        {
                            p = 0;
                        }
                    }
                    else if (point.Y < iPointY)
                    {
                        if (point.Y > jPointY)
                        {
                            pValue = (point.Y - iPointY) * (jPointX - iPointX) / (jPointY - iPointY) + iPointX;
                            if (point.X < pValue)
                            {
                                k++;
                            }
                            else if (point.X == pValue)
                            {
                                return q;
                            }
                        }
                        else if (point.Y == jPointY)
                        {
                            p = 1;
                        }
                    }
                    else
                    {
                        if (point.X == iPointX)
                        {
                            return q;
                        }
                        if (point.Y < jPointY)
                        {
                            if (p != 1)
                            {
                                if (point.X < iPointX)
                                {
                                    k++;
                                }
                            }
                        }
                        else if (point.Y > jPointY)
                        {
                            if (p > 0)
                            {
                                if (point.X < iPointX)
                                {
                                    k++;
                                }
                            }
                        }
                        else
                        {
                            if (point.X > iPointX && point.X <= jPointX)
                            {
                                return q;
                            }
                            if (point.X < iPointX && point.X >= jPointX)
                            {
                                return q;
                            }
                        }
                    }
                    j = i;
                    i++;
                }

                if (k % 2 != 0)
                {
                    return q;
                }
            }
            return index;
        }
        #endregion

        #region 简单环、岛操作
        /// <summary>
        /// 切环
        /// </summary>
        /// <param name="circle"></param>
        /// <param name="polyline"></param>
        public static Graphic cutGeometry(Graphic circle, Polyline polyline)
        {
            (circle.Geometry as Polygon).Rings.Add(polyline.Paths[0]);
            Graphic gra = new Graphic();

            Polygon polygon = new Polygon();
            polygon.Rings.Add(new ESRI.ArcGIS.Client.Geometry.PointCollection((circle.Geometry as Polygon).Rings[0]));
            polygon.Rings.Add(new ESRI.ArcGIS.Client.Geometry.PointCollection(polyline.Paths[0]));
            gra.Geometry = polygon;
            return gra;

        }

        public static Dictionary<Graphic, string> RingArea(List<Graphic> CircleTxt, List<Graphic> CircleRing, Dictionary<string, double> Dict_Level)
        {
            List<Graphic> lst = new List<Graphic>();
            for (int i = 0; i < CircleRing.Count; i++)
            {
                if (CircleRing[i] != null)
                {
                    lst.Add(CircleRing[i]);
                }
            }
            List<Graphic> lsttxt = new List<Graphic>();
            for (int i = 0; i < CircleTxt.Count; i++)
            {
                if (CircleTxt[i] != null)
                {
                    lsttxt.Add(CircleTxt[i]);
                }
            }

            int j = 0;
            double prearea = 0;

            Dictionary<Graphic, string> ReturnCircleArea = new Dictionary<Graphic, string>();
            foreach (KeyValuePair<string, double> kv in Dict_Level)
            {
                double b = kv.Value;
                double area = Math.PI * b * b;
                lsttxt[j].Attributes.Add("StaArea", Math.Round(area - prearea, 2) + "平方米");
                ReturnCircleArea.Add(lst[j], Math.Round(area - prearea, 2) + "平方米");
                prearea = area;
                j++;
            }

            return ReturnCircleArea;
        }

        /// <summary>
        /// 根据角度来计算扩散点经纬度
        /// </summary>
        /// <param name="dbx">起始扩散点经度</param>
        /// <param name="dby">起始扩散点纬度</param>
        /// <param name="arysize">每个扩散点步长</param>
        /// <param name="aryradius">每个扩散点的半径</param>
        /// <param name="radian">角度</param>
        public static List<Graphic> CalculationShaft(double dbx, double dby, double[] arysize, double[] aryradius, double radian,Map LeakMap)
        {
            MapPoint mpstart = new MapPoint() { X = dbx, Y = dby, SpatialReference = LeakMap.SpatialReference };

            //经纬度转换成地方坐标系
            List<Graphic> lsttmp = new List<Graphic>();
            Graphic gratmp = new Graphic() { Geometry = mpstart };
            lsttmp.Add(gratmp);
            List<double[]> lstobj = new List<double[]>();
            lstobj.Add(arysize);
            lstobj.Add(aryradius);
            double[] aryradian = new double[1];
            aryradian[0] = radian;
            lstobj.Add(aryradian);

            return CalculationMapPoint(mpstart, lstobj[0], lstobj[1], lstobj[2]);

            //            geometrytask.ProjectAsync(lsttmp, new SpatialReference(21481), lstobj);
        }

        /// <summary>
        /// 根据转换后的地方坐标和步长、角度来计算地方坐标系下面的扩散点
        /// </summary>
        /// <param name="mp">地方坐标系下起始扩散点坐标</param>
        /// <param name="arysize">步长</param>
        /// <param name="aryradius">扩散半径</param>
        /// <param name="aryradian">角度</param>
        public static List<Graphic> CalculationMapPoint(MapPoint mp, double[] arysize, double[] aryradius, double[] aryradian)
        {
            List<Graphic> lstmp = new List<Graphic>();
            lstmp.Add(new Graphic() { Geometry = mp });

            for (int i = 0; i < arysize.Length; i++)
            {
                MapPoint slopemp = new MapPoint();
                double dbx = Math.Sin(aryradian[0] * Math.PI / 180) * arysize[i];
                double dby = Math.Cos(aryradian[0] * Math.PI / 180) * arysize[i];
                double x = (lstmp[i].Geometry as MapPoint).X;
                double y = (lstmp[i].Geometry as MapPoint).Y;

                x = (lstmp[i].Geometry as MapPoint).X - dbx;
                y = (lstmp[i].Geometry as MapPoint).Y - dby;

                slopemp.X = x;
                slopemp.Y = y;
                slopemp.SpatialReference = lstmp[i].Geometry.SpatialReference;
                lstmp.Add(new Graphic() { Geometry = slopemp });
            }

            return lstmp;
        }
        #endregion

        #region 辅助算法，数学公式

        public List<MapPoint> GetCirclePoints(MapPoint centerpoint, double r)
        {
            //centerpoint = BaiduMeature.ProjBaidu2LonLat(centerpoint);
            List<MapPoint> rtn = new List<MapPoint>();
            double sita = 0;

            double fromangle = 0;
            double toangle = 360;

            for (double i = fromangle; i <= toangle; i += 0.2)
            {
                sita = i * Math.PI / 180;
                MapPoint tmp = new MapPoint(centerpoint.X + r * Math.Cos(sita), centerpoint.Y + r * Math.Sin(sita));
                rtn.Add(tmp);
            }
            return rtn;
        }

        public List<MapPoint> GetFanshapedPoints(MapPoint centerpoint, double r, double fromangle, double toangle)
        {
            List<MapPoint> rtn = new List<MapPoint>();
            double sita = 0;

            if (toangle < fromangle)
            {
                double tmp = toangle;
                toangle = fromangle;
                fromangle = tmp;
            }

            for (double i = fromangle; i <= toangle; i += 0.2)
            {
                sita = i * Math.PI / 180;
                rtn.Add(new MapPoint(centerpoint.X + r * Math.Cos(sita), centerpoint.Y + r * Math.Sin(sita)));
            }
            return rtn;
        }


        /// <summary>
        /// 过线段外一点做垂线，返回交点
        /// </summary>
        /// <param name="StartPoint">线段的起点:double</param>
        /// <param name="EndPoint">线段的终点:double</param>
        /// <param name="OutPoint">线外一点:double</param>
        /// <returns>交点:double</returns>
        public MapPoint GetVerticalPoint(MapPoint StartPoint, MapPoint EndPoint, MapPoint OutPoint)
        {
            MapPoint rtnPoint = new MapPoint();
            double y1 = StartPoint.Y;
            double x1 = StartPoint.X;
            double y2 = EndPoint.Y;
            double x2 = EndPoint.X;
            double y3 = OutPoint.Y;
            double x3 = OutPoint.X;
            //如果直线水平
            if (y1 == y2)
            {
                rtnPoint.Y = StartPoint.Y;
                rtnPoint.X = OutPoint.X;
            }
            //如果直线垂直
            else if (x1 == x2)
            {
                rtnPoint.X = StartPoint.X;
                rtnPoint.Y = OutPoint.Y;
            }
            //斜线
            else
            {
                //直线的斜率
                double k = (y2 - y1) / (x2 - x1);
                //垂线的斜率 -1/k
                double l = -1 / k;

                rtnPoint.X = (y3 - y1 + k * x1 - l * x3) / (k - l);
                rtnPoint.Y = k * rtnPoint.X + y1 - k * x1;

            }
            return rtnPoint;
        }

        /// <summary>
        /// 返回线段的中点
        /// </summary>
        /// <param name="StartPoint">线段的起点:double</param>
        /// <param name="EndPoint">线段的终点:double</param>
        /// <returns>线段的中点:double</returns>
        public MapPoint GetMidPoint(MapPoint StartPoint, MapPoint EndPoint)
        {
            MapPoint rtnPoint = new MapPoint();

            rtnPoint.X = StartPoint.X - (StartPoint.X - EndPoint.X) / 2;
            rtnPoint.Y = StartPoint.Y - (StartPoint.Y - EndPoint.Y) / 2;

            return rtnPoint;
        }

        public bool GetIsRightPointToLine(MapPoint startpoint, MapPoint endpoint, MapPoint outpoint)
        {
            double xonline = (outpoint.Y - startpoint.Y) * (endpoint.X - startpoint.X) / (endpoint.Y - startpoint.Y) + startpoint.X;
            if (xonline - outpoint.X > 0)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 已知两点斜率K、两点距离L和点外点相对原点方位IsRight，求点位点坐标
        /// </summary>
        /// <param name="oPoint"></param>
        /// <param name="k"></param>
        /// <param name="L"></param>
        /// <param name="IsRight"></param>
        /// <returns></returns>
        public MapPoint GetPointOutPoint(MapPoint oPoint, double k, double L, bool IsRight)
        {
            double x0 = 0;
            double y0 = 0;
            double param = L / Math.Sqrt(k * k + 1);
            if (IsRight)
            {
                x0 = oPoint.X + param;
            }
            else
            {
                x0 = oPoint.X - param;
            }

            y0 = k * (x0 - oPoint.X) + oPoint.Y;

            return new MapPoint(x0, y0);
        }
        /// <summary>
        /// StartPoint,EndPoint,rtnPoint构成直角三角形，StartPointrtnPoint⊥EndPointStartPoint
        /// </summary>
        /// <param name="StartPoint"></param>
        /// <param name="EndPoint"></param>
        /// <param name="sita">SE与OE的夹角</param>
        /// <param name="IsRight">返回点的x坐标＞startpoint.x</param>
        /// <returns></returns>
        public MapPoint GetPointOutLineLR(MapPoint StartPoint, MapPoint EndPoint, double sita, bool IsRight)
        {
            MapPoint rtnPoint = new MapPoint();

            double x0 = StartPoint.X;
            double y0 = StartPoint.Y;
            double x1 = EndPoint.X;
            double y1 = EndPoint.Y;

            double k = (y1 - y0) / (x1 - x0);
            double l = -1 / k;
            double sqrL = Math.Pow(x0 - x1, 2) + Math.Pow(y0 - y1, 2);
            double L = Math.Sqrt(sqrL);

            double dis = Math.Tanh(sita) * L;

            if (IsRight)
            {
                rtnPoint.X = x0 + dis / Math.Sqrt(1 + l * l);
            }
            else
            {
                rtnPoint.X = x0 - dis / Math.Sqrt(1 + l * l);
            }
            rtnPoint.Y = l * (rtnPoint.X - x0) + y0;
            return rtnPoint;
        }

        public MapPoint GetPointOutLineUD(MapPoint StartPoint, MapPoint EndPoint, double sita, bool IsUp)
        {
            MapPoint rtnPoint = new MapPoint();

            double x0 = StartPoint.X;
            double y0 = StartPoint.Y;
            double x1 = EndPoint.X;
            double y1 = EndPoint.Y;

            double k = (y1 - y0) / (x1 - x0);
            double l = -1 / k;
            double sqrL = Math.Pow(x0 - x1, 2) + Math.Pow(y0 - y1, 2);
            double L = Math.Sqrt(sqrL);

            double dis = Math.Tanh(sita) * L;

            if (IsUp)
            {
                rtnPoint.Y = y0 + dis * Math.Abs(l) / Math.Sqrt(1 + l * l);
            }
            else
            {
                rtnPoint.Y = y0 - dis * Math.Abs(l) / Math.Sqrt(1 + l * l);
            }
            rtnPoint.X = (rtnPoint.Y - y0) / l + x0;
            return rtnPoint;
        }

        public double GetAnglePointToLine(MapPoint StartPoint, MapPoint EndPoint, MapPoint OutPoint)
        {
            double L = (Math.Pow(OutPoint.X - StartPoint.X, 2) + Math.Pow(OutPoint.Y - StartPoint.Y, 2)) / (Math.Pow(StartPoint.X - EndPoint.X, 2) + Math.Pow(StartPoint.Y - EndPoint.Y, 2));
            return Math.Atan(Math.Sqrt(L));
        }

        /// <summary>
        /// 线外一点的对称点
        /// Ax+By+C=0
        /// A(x0+x1)/2+B(y0+y1)/2+C=0
        /// A(y1-y0)=B(x1-x0) 
        /// SR垂直SE
        /// </summary>
        /// <param name="StartPoint">线段起点</param>
        /// <param name="EndPoint">线段终点</param>
        /// <param name="OriginalPoint">线外一点</param>
        /// <returns>对称点</returns>
        public MapPoint GetSymmetricPoint(MapPoint StartPoint, MapPoint EndPoint, MapPoint OriginalPoint)
        {
            MapPoint rtnPoint = new MapPoint();

            rtnPoint.X = StartPoint.X - (OriginalPoint.X - StartPoint.X);
            rtnPoint.Y = StartPoint.Y - (OriginalPoint.Y - StartPoint.Y);

            return rtnPoint;
        }
        /// <summary>
        /// OPoint是原点，OriginalPoint是rtnpoint关于OPoint的对照点。rtnPoint在OO的延长线上
        /// </summary>
        /// <param name="OPoint"></param>
        /// <param name="OriginalPoint"></param>
        /// <returns></returns>
        public MapPoint GetSymmetricPoint(MapPoint OPoint, MapPoint OriginalPoint)
        {
            MapPoint rtnPoint = new MapPoint();

            rtnPoint.X = OPoint.X - (OriginalPoint.X - OPoint.X);
            rtnPoint.Y = OPoint.Y - (OriginalPoint.Y - OPoint.Y);

            return rtnPoint;
        }

        public double GetDisPointToPoint(MapPoint StartPoint, MapPoint EndPoint)
        {
            double dis = 0.0;

            dis = Math.Sqrt(Math.Pow(StartPoint.X - EndPoint.X, 2) + Math.Pow(StartPoint.Y - EndPoint.Y, 2));

            return dis;
        }

        /// <summary>
        /// 点到线的距离
        /// </summary>
        /// <param name="StartPoint">线段起点</param>
        /// <param name="EndPoint">线段终点</param>
        /// <param name="OriginalPoint">线外一点</param>
        /// <returns></returns>
        public double GetDisPointToLine(MapPoint StartPoint, MapPoint EndPoint, MapPoint OriginalPoint)
        {
            double dis = 0;

            double k = (StartPoint.Y - EndPoint.Y) / (StartPoint.X - EndPoint.X);
            double A = (EndPoint.Y - StartPoint.Y);
            double B = (StartPoint.X - EndPoint.X);
            double C = k * StartPoint.X - StartPoint.Y;

            return dis;
        }

        /// <summary>
        /// 在线段上按比例取点X py
        /// </summary>
        /// <param name="StartPoint">起点</param>
        /// <param name="EndPoint">终点</param>
        /// <param name="ratio">SX/SE ratio ＜ 1</param>
        /// <returns></returns>
        public MapPoint GetRatioPoint(MapPoint StartPoint, MapPoint EndPoint, double ratio)
        {
            MapPoint rtnPoint = new MapPoint();

            rtnPoint.X = StartPoint.X + (EndPoint.X - StartPoint.X) * ratio;
            rtnPoint.Y = StartPoint.Y + (EndPoint.Y - StartPoint.Y) * ratio;

            return rtnPoint;
        }

        /// <summary>
        /// 四点构成矩形
        /// </summary>
        /// <param name="LeftBottom"></param>
        /// <param name="RightBottom"></param>
        /// <param name="RightTop"></param>
        /// <returns></returns>
        public MapPoint GetRectanglePoint(MapPoint LeftBottom, MapPoint RightBottom, MapPoint RightTop)
        {
            MapPoint rtnPoint = new MapPoint();

            rtnPoint.X = RightTop.X - (RightBottom.X - LeftBottom.X);
            rtnPoint.Y = RightTop.Y - (RightBottom.Y - LeftBottom.Y);

            return rtnPoint;
        }
        #endregion

        #region 画图
        /// <summary>
        /// 通过等边三角形上方顶点，返回三个点的坐标
        /// 底边总是平行X轴
        /// </summary>
        /// <param name="point"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public MapPoint[] DrawEquilateralTriangle(MapPoint point, double height)
        {
            MapPoint[] rtnpoint = new MapPoint[3];

            rtnpoint[1] = point;

            rtnpoint[0] = new MapPoint(point.X - height * Math.Sqrt(3) / 3, point.Y - height);
            rtnpoint[2] = new MapPoint(point.X + height * Math.Sqrt(3) / 3, point.Y - height);

            return rtnpoint;
        }
        #endregion

        #region 二次三次bezier曲线实现
        /// <summary>
        /// bezier曲线
        /// </summary>
        /// <param name="controlp">控制点：起点、控制点1、控制点2、终点</param>
        /// <param name="numofpoints">期望曲线的点个数</param>
        /// <param name="curvep">期望曲线的点</param>
        public List<MapPoint> BezierCurver(MapPoint[] controlp, int numofpoints)
        {
            List<MapPoint> curvep = new List<MapPoint>();
            int num = controlp.Length;
            switch (num)
            {
                case 3:
                    curvep = GetPointOnDoubleBezier(controlp, numofpoints);
                    break;
                case 4:
                    curvep = GetPointOnThripleBezier(controlp, numofpoints);
                    break;
            }
            return curvep;
        }

        /// <summary>
        ///三次bezier曲线上的点 
        ///B(t)=(-P0+3P1-3P2+P3)MATH.POW(T,3)+(3P0-6P1+3P2)MATH.POW(T,2)+(-3PO+3P1)T+PO
        /// </summary>
        /// <param name="cp">控制点：起点、控制点1、控制点2、终点</param>
        /// <param name="needofpoint">期望曲线点数，密集程度</param>
        /// <returns>返回曲线上所有点</returns>
        List<MapPoint> GetPointOnThripleBezier(MapPoint[] cp, int needofpoint)
        {
            List<MapPoint> rtnp = new List<MapPoint>();
            double x0 = cp[0].X;
            double x1 = cp[1].X;
            double x2 = cp[2].X;
            double x3 = cp[3].X;

            double y0 = cp[0].Y;
            double y1 = cp[1].Y;
            double y2 = cp[2].Y;
            double y3 = cp[3].Y;

            double ax = 3 * x1 + x3 - x0 - 3 * x2;
            double bx = 3 * x0 + 3 * x2 - 6 * x1;
            double cx = 3 * x1 - 3 * x0;
            double dx = x0;

            double ay = 3 * y1 + y3 - y0 - 3 * y2;
            double by = 3 * y0 + 3 * y2 - 6 * y1;
            double cy = 3 * y1 - 3 * y0;
            double dy = y0;

            double dt = 1.0 / (needofpoint - 1);

            for (int i = 0; i < needofpoint; i++)
            {
                MapPoint tmp = new MapPoint();
                tmp.X = GetPointOnThripleBezierByT(ax, bx, cx, dx, dt * i);
                tmp.Y = GetPointOnThripleBezierByT(ay, by, cy, dy, dt * i);
                rtnp.Add(tmp);
            }

            return rtnp;
        }
        /// <summary>
        /// 二次bezier曲线上的点
        /// B(t)=(p0-2p1+p2)MATH.POW(T,2)-2(p0-p1)*t+p0
        /// </summary>
        /// <param name="cp">控制点：起点、控制点1、终点</param>
        /// <param name="needofpoint">期望曲线点数，密集程度</param>
        /// <returns>返回曲线上所有点</returns>
        List<MapPoint> GetPointOnDoubleBezier(MapPoint[] cp, int needofpoint)
        {
            List<MapPoint> rtnp = new List<MapPoint>();
            double x0 = cp[0].X;
            double x1 = cp[1].X;
            double x2 = cp[2].X;

            double y0 = cp[0].Y;
            double y1 = cp[1].Y;
            double y2 = cp[2].Y;

            double ax = (x0 - 2 * x1 + x2);
            double bx = -2 * (x0 - x1);
            double cx = x0;

            double ay = (y0 - 2 * y1 + y2);
            double by = -2 * (y0 - y1);
            double cy = y0;

            double dt = 1.0 / (needofpoint - 1);

            for (int i = 0; i < needofpoint; i++)
            {
                MapPoint tmp = new MapPoint();
                tmp.X = GetPointOnDoubleBezierByT(ax, bx, cx, dt * i);
                tmp.Y = GetPointOnDoubleBezierByT(ay, by, cy, dt * i);
                rtnp.Add(tmp);
            }

            return rtnp;
        }

        /// <summary>
        /// 三次bezier曲线方程实现
        /// 展开成B(t)=(-P0+3P1-3P2+P3)MATH.POW(T,3)+(3P0-6P1+3P2)MATH.POW(T,2)+(-3PO+3P1)T+PO
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        double GetPointOnThripleBezierByT(double a, double b, double c, double d, double t)
        {
            double rtnd = 0.0;
            double sqrt = Math.Pow(t, 2);
            double cubedt = t * sqrt;

            rtnd = a * cubedt + b * sqrt + c * t + d;

            return rtnd;
        }

        double GetPointOnDoubleBezierByT(double a, double b, double c, double t)
        {
            double rtnd = 0.0;

            double sqrt = t * t;

            rtnd = a * sqrt + b * t + c;

            return rtnd;
        }
        #endregion
    }
}
