/// <summary>  
/// 作者：陈锋 
/// 时间：2012/5/21 15:55:54  
/// 公司:南京安元科技有限公司  
/// 版权：2012-2020  
/// CLR版本：4.0.30319.261  
/// Extensions说明：
/// 唯一标识：af680056-247d-48a4-8c4a-8f08138ad5b0  
/// </summary>

using System.Linq;
using System.Xml.Linq;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;

namespace AYKJ.GISDevelop.Platform.ToolKit
{
    public static class Extensions
    {
        /// <summary>
        /// 生成graphic
        /// </summary>
        /// <param name="graphic"></param>
        /// <param name="item"></param>
        public static void InitGraphicWithXElement(this Graphic graphic, XElement item)
        {
            string _namespace = item.Name.Namespace.NamespaceName;
            XElement entity = item.Elements().First();
            foreach (var properItem in entity.Elements())
            {
                if (properItem.Name.LocalName.ToUpper() == "GEOMETRY")
                {
                    XElement geoElement = properItem.Elements().First();
                    graphic.Geometry = InitGeometryWithXElement(geoElement);
                }
                else
                {
                    graphic.Attributes.Add(properItem.Name.LocalName, properItem.Value);
                }
            }

        }

        /// <summary>
        /// 更据Xelement生成几何图形
        /// </summary>
        /// <param name="geoElement"></param>
        /// <returns></returns>
        public static Geometry InitGeometryWithXElement(XElement geoElement)
        {
            ESRI.ArcGIS.Client.Geometry.Geometry geoRe = null;
            string nsn = geoElement.Name.NamespaceName;

            string geoType = geoElement.Name.LocalName;
            switch (geoType)
            {
                case "Polygon":
                    {
                        Polygon pog = new Polygon();
                        foreach (var item in geoElement.Element(XName.Get("outerBoundaryIs", nsn)).Elements(XName.Get("LinearRing", nsn)))
                        {
                            ESRI.ArcGIS.Client.Geometry.PointCollection pointColl = new ESRI.ArcGIS.Client.Geometry.PointCollection();
                            string pointsStr = item.Element(XName.Get("coordinates", nsn)).Value.Replace("\n", "").Trim();
                            string[] pointxyStr = pointsStr.Split(' ');
                            foreach (var xyarray in pointxyStr)
                            {
                                string[] xyStr = xyarray.Split(',');
                                MapPoint mp = new MapPoint(double.Parse(xyStr[0]), double.Parse(xyStr[1]));
                                pointColl.Add(mp);
                            }
                            pog.Rings.Add(pointColl);
                        }
                        geoRe = pog;
                        break;
                    }
                case "Point":
                    {
                        string pointsStr = geoElement.Element(XName.Get("coordinates", nsn)).Value.Replace("\n", "").Trim();
                        string[] pointxyStr = pointsStr.Split(',');
                        MapPoint pog = new MapPoint(double.Parse(pointxyStr[0]), double.Parse(pointxyStr[1]));
                        geoRe = pog;
                        break;
                    }
                case "PolyLine":
                    {
                        break;
                    }
            }
            return geoRe;
        }

        public static string TrimSprit(this string ins)
        {

            ins = ins.Replace("\"", "");
            return ins;
        }

        public static string TrimDoubleSprit(this string ins)
        {

            ins = ins.Replace("\\", "");
            return ins;
        }
    }
}
