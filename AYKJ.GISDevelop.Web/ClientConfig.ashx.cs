using System.Configuration;
using System.IO;
using System.Text;
using System.Web;
using System.Xml.Linq;

namespace AYKJ.GISDevelop.Web
{
    /// <summary>
    /// ClientConfig 的摘要说明
    /// </summary>
    public class ClientConfig : IHttpHandler
    {
        private double filesSize = 0;

        //系统目录
        private readonly string ClientBin = ConfigurationManager.AppSettings["ClientBin"];

        public void ProcessRequest(HttpContext context)
        {
            string xmlString = null;
            //读取XML
            using (StreamReader sr = new StreamReader(context.Server.MapPath(string.Format("{0}/Config/Plat.XML", ClientBin))))
            {
                //获得xml配置文件的内容
                string rootPath = context.Server.MapPath(ClientBin);
                xmlString = sr.ReadToEnd();

                //根目录
                XElement root = (XDocument.Parse(xmlString.Trim())).Root;

                //获取System系统文件目录下的文件，设置System节点的Source属性
                string sysAss = getFilesName(string.Format("{0}Assembly\\System\\", rootPath)).Replace(rootPath, "").Replace('\\', '/');
                XElement newName = root.Element("Assembly").Element("System");
                newName.SetAttributeValue("Source", sysAss);

                //获取Part系统文件目录下的文件，设置System节点的Source属性
                string partAss = getFilesName(string.Format("{0}Assembly\\Parts\\", rootPath)).Replace(rootPath, "").Replace('\\', '/');
                XElement partEle = root.Element("Assembly").Element("Part");
                partEle.SetAttributeValue("Source", partAss);

                //获取Style系统文件目录下的文件，设置System节点的Source属性
                string styleAss = getFilesName(string.Format("{0}Assembly\\Style\\", rootPath)).Replace(rootPath, "").Replace('\\', '/');
                XElement styleEle = root.Element("Assembly").Element("Style");
                styleEle.SetAttributeValue("Source", styleAss);

                //获取标绘系统文件目录下的文件,设置节点
                string plotAss = getImageName(string.Format("{0}Image\\Plot\\Thematic\\", rootPath)).Replace(rootPath, "").Replace('\\', '/');
                if (plotAss != "")
                {
                    XElement plotEle = root.Element("Extent").Element("PlotImage");
                    if (plotEle != null)
                    {
                        plotEle.SetAttributeValue("Source", plotAss);
                    }
                }

                //大小
                XElement AssEle = root.Element("Assembly");
                AssEle.SetAttributeValue("Size", filesSize);

                //总的xml字符串
                xmlString = root.ToString(SaveOptions.None);
            }
            //File.AppendAllText(context.Server.MapPath("~/info.log"), xmlString);
            context.Response.Write(xmlString);
        }


        /// <summary>
        /// 获得某个目录下的文件
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        private string getFilesName(string directory)
        {
            //拼文件名字符串
            StringBuilder files = new StringBuilder();
            //遍历目录下的文件
            foreach (var item in Directory.GetFiles(directory))
            {
                //排除非dll和xaml的文件
                if (!item.EndsWith(".dll") && !item.EndsWith(".xaml"))
                    continue;
                files.Append(item);
                //分号连接
                files.Append(';');
                FileInfo fi = new FileInfo(item);


                filesSize += fi.Length;
            }

            //遍历目录下的目录，迭代
            foreach (var item in Directory.GetDirectories(directory))
            {
                files.Append(getFilesName(item));
            }
            return files.ToString();
        }

        /// <summary>
        /// 获得某个目录下的图片
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        private string getImageName(string directory)
        {
            //拼文件名字符串
            StringBuilder files = new StringBuilder();
            if (!Directory.Exists(directory))
                return "";
            //遍历目录下的文件
            foreach (var item in Directory.GetFiles(directory))
            {
                //排除非dll和xaml的文件
                if (!item.EndsWith(".png"))
                    continue;
                files.Append(item);
                //分号连接
                files.Append(';');
                FileInfo fi = new FileInfo(item);


                filesSize += fi.Length;
            }

            //遍历目录下的目录，迭代
            foreach (var item in Directory.GetDirectories(directory))
            {
                files.Append(getFilesName(item));
            }
            return files.ToString();
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}