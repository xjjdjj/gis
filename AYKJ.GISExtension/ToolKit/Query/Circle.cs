using System;
using ESRI.ArcGIS.Client.Geometry;

namespace AYKJ.GISExtension
{
    public class Circle : ESRI.ArcGIS.Client.Geometry.Polygon
    {
        private double m_centerLon;
        private double m_centerLat;
        private double m_radiusInMeters;
        public static ESRI.ArcGIS.Client.Geometry.PointCollection rings1 = null;
        ESRI.ArcGIS.Client.Geometry.PointCollection ring = new ESRI.ArcGIS.Client.Geometry.PointCollection();
        public Circle(double centerLon, double centerLat, double radiusInMeters)
        {
            m_centerLon = centerLon;
            m_centerLat = centerLat;
            m_radiusInMeters = radiusInMeters;
        }

        public ESRI.ArcGIS.Client.Geometry.PointCollection rings
        {
            get
            {
                if (ring == null)
                {

                    MapPoint origPoint = new MapPoint(m_centerLat, m_centerLat);
                    for (double ang = 0; ang <= 360; ang++)
                    {
                        MapPoint mapPoint = new MapPoint();
                        //SpatialReference ref1 = new SpatialReference(102113);
                        SpatialReference ref1 = DataQueryRadius.mainmap.SpatialReference;
                        mapPoint = GeodeticPoint(origPoint, ang, m_radiusInMeters, ref1);
                        ring.Add(mapPoint);
                    }
                    return ring;
                }

                return ring;
            }

        }

        public int numberOfCirclePoints = 100;

        public ESRI.ArcGIS.Client.Geometry.PointCollection createCirclePoints()
        {
            double cosinus;
            double sinus;
            double x;
            double y;
            ESRI.ArcGIS.Client.Geometry.PointCollection arrayOfPoints = new ESRI.ArcGIS.Client.Geometry.PointCollection();
            for (int i = 0; i < numberOfCirclePoints; i++)
            {
                double j = Convert.ToDouble(i);
                sinus = Math.Sin((Math.PI * 2.0) * (j / numberOfCirclePoints));
                cosinus = Math.Cos((Math.PI * 2.0) * (j / numberOfCirclePoints));
                x = m_centerLon + m_radiusInMeters * cosinus;
                y = m_centerLat + m_radiusInMeters * sinus;
                arrayOfPoints.Add(new MapPoint(x, y));
            }
            arrayOfPoints.Add(arrayOfPoints[0]);
            return arrayOfPoints;
        }

        public double centerLon
        {
            get { return m_centerLon; }
            set { m_centerLon = value; }
        }

        private double rad2degree(double rad)
        {
            return rad * 180 / Math.PI;
        }

        private double degree2rad(double degree)
        {
            return degree * Math.PI / 180;
        }

        public MapPoint GeodeticPoint(MapPoint start, double startBearing, double distance, SpatialReference ref1)
        {
            double a = 6378137.0;
            double b = 6356752.3142;
            double aSquared = a * a;
            double bSquared = b * b;
            double f = 1 / 298.257224;
            double phi1 = degree2rad(start.Y);
            double alpha1 = degree2rad(startBearing);
            double cosAlpha1 = Math.Cos(alpha1);
            double sinAlpha1 = Math.Sin(alpha1);
            double s = distance;
            double tanU1 = (1.0 - f) * Math.Tan(phi1);
            double cosU1 = 1.0 / Math.Sqrt(1.0 + tanU1 * tanU1);
            double sinU1 = tanU1 * cosU1;

            // eq. 1
            double sigma1 = Math.Atan2(tanU1, cosAlpha1);

            // eq. 2
            double sinAlpha = cosU1 * sinAlpha1;

            double sin2Alpha = sinAlpha * sinAlpha;
            double cos2Alpha = 1 - sin2Alpha;
            double uSquared = cos2Alpha * (aSquared - bSquared) / bSquared;

            // eq. 3
            double A = 1 + (uSquared / 16384) * (4096 + uSquared * (-768 + uSquared * (320 - 175 * uSquared)));

            // eq. 4
            double B = (uSquared / 1024) * (256 + uSquared * (-128 + uSquared * (74 - 47 * uSquared)));

            // iterate until there is a negligible change in sigma
            double deltaSigma;
            double sOverbA = s / (b * A);
            double sigma = sOverbA;
            double sinSigma;
            double prevSigma = sOverbA;
            double sigmaM2;
            double cosSigmaM2;
            double cos2SigmaM2;

            for (; ; )
            {
                // eq. 5
                sigmaM2 = 2.0 * sigma1 + sigma;
                cosSigmaM2 = Math.Cos(sigmaM2);
                cos2SigmaM2 = cosSigmaM2 * cosSigmaM2;
                sinSigma = Math.Sin(sigma);
                double cosSignma = Math.Cos(sigma);

                // eq. 6
                deltaSigma = B * sinSigma * (cosSigmaM2 + (B / 4.0) * (cosSignma * (-1 + 2 * cos2SigmaM2)
                    - (B / 6.0) * cosSigmaM2 * (-3 + 4 * sinSigma * sinSigma) * (-3 + 4 * cos2SigmaM2)));

                // eq. 7
                sigma = sOverbA + deltaSigma;

                // break after converging to tolerance
                if (Math.Abs(sigma - prevSigma) < 0.0000000000001)
                {
                    break;
                }

                prevSigma = sigma;
            }

            sigmaM2 = 2.0 * sigma1 + sigma;
            cosSigmaM2 = Math.Cos(sigmaM2);
            cos2SigmaM2 = cosSigmaM2 * cosSigmaM2;

            double cosSigma = Math.Cos(sigma);
            sinSigma = Math.Sin(sigma);

            // eq. 8
            double phi2 = Math.Atan2(sinU1 * cosSigma + cosU1 * sinSigma * cosAlpha1,
                                     (1.0 - f) * Math.Sqrt(sin2Alpha + Math.Pow(sinU1 * sinSigma - cosU1 * cosSigma * cosAlpha1, 2.0)));

            // eq. 9

            double lambda = Math.Atan2(sinSigma * sinAlpha1, cosU1 * cosSigma - sinU1 * sinSigma * cosAlpha1);

            // eq. 10
            double C = (f / 16) * cos2Alpha * (4 + f * (4 - 3 * cos2Alpha));

            // eq. 11
            double L = lambda - (1 - C) * f * sinAlpha * (sigma + C * sinSigma * (cosSigmaM2 + C * cosSigma * (-1 + 2 * cos2SigmaM2)));

            // eq. 12
            double alpha2 = Math.Atan2(sinAlpha, -sinU1 * sinSigma + cosU1 * cosSigma * cosAlpha1);

            // build result
            double latitude;
            double longitude;

            double lat2r = phi2;
            double lon2r = degree2rad(start.X) + L;

            latitude = rad2degree(lat2r);
            longitude = rad2degree(lon2r);
            // endBearing = new Angle();
            //endBearing.Radians = alpha2;

            //return new MapPoint(latitude, longitude);
            return new MapPoint(longitude, latitude, ref1);
        }
    }
}
