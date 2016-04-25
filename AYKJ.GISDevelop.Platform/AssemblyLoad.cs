/// <summary>  
/// 作者：陈锋 
/// 时间：2012/5/21 16:01:53  
/// 公司:南京安元科技有限公司  
/// 版权：2012-2020  
/// CLR版本：4.0.30319.261  
/// AssemblyLoad说明：加载程序集
/// 唯一标识：1409ff1f-cc8f-468d-a0a4-7314e71cf640  
/// </summary>

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows;
using AYKJ.GISDevelop.Platform.Config;
using AYKJ.GISDevelop.Platform.Config.Entity;
using AYKJ.GISDevelop.Platform.Part;
using System.Xml.Linq;
using AYKJ.GISDevelop.Platform.ToolKit;

namespace AYKJ.GISDevelop.Platform
{
    public class AssemblyLoad
    {
        //用于下载资源
        private WebClient client;
        private double downed;
        private double size;
        //在构建循环异步下载必要的参数，当前下载和总量
        private int index;
        private int lenght;
        //系统资源，样式资源和应用资源的信息，主要是目录
        private IList<SystemEntity> systems;
        private IList<StyleEntity> styles;
        IList<PartEntity> parts;
        //地图使用的版本
        private enumMapServerType MapServerType = enumMapServerType.Esri;

        //初始化方法
        public AssemblyLoad(ConfigData configData)
        {
            //设置系统资源，样式资源和应用资源
            systems = configData.SystemEntities;
            parts = configData.PartEntities;
            styles = configData.StyleEntities;
            size = configData.FilesSize;
        }

        //外部调用的加载
        public void Load()
        {
            XElement xele = PFApp.Extent;
            string type = xele.Attribute("Type").Value;
            if (type == "Baidu")
                MapServerType = enumMapServerType.Baidu;
            PFApp.MapServerType = MapServerType;
            downed = 0;
            //提示改变状态
            PFApp.setStatu(Constant.LoadSysTip);
            //加载系统资源
            LoadSystems();
        }

        #region 加载系统程序集
        /// <summary>
        /// 加载系统的程序集
        /// </summary>
        private void LoadSystems()
        {
            //初始化循环控制参数
            index = 0;
            lenght = systems.Count;
            if (lenght > 0)
            {
                //实例化下载组件，增加监听
                client = new WebClient();
                client.OpenReadCompleted += new OpenReadCompletedEventHandler(client_OpenReadSysCompleted);
                client.DownloadProgressChanged +=
                    new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                //加载即循环体
                //MessageBox.Show(systems[index].Source);
     
                load(systems[index].Source);
            }
            else
            {
                LoadStyle();
            }
        }



        /// <summary>
        /// 加载一个程序系成功的处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void client_OpenReadSysCompleted(object sender, OpenReadCompletedEventArgs e)
        {
                downed += e.Result.Length;
                PFApp.setStatu(string.Format("— {0}% —", (int)((downed / size) * 100)));
                PFApp.setStatu(downed, size);

                //解析系统资源，即加入到程序中
                AssemblyPart assemblyPart = new AssemblyPart();

                Assembly assembly = assemblyPart.Load(e.Result);

            //判断是不是最后一个资源 
            if (++index < lenght)
            {
                //执行下一个循环
                load(systems[index].Source);
            }
            else
            {
                //加载样式
                LoadStyle();
            }
        }
        #endregion

        #region 加载样式字典
        /// <summary>
        /// 加载样式资源字典
        /// </summary>
        private void LoadStyle()
        {
            // PFApp.setStatu(Constant.LoadStyleTip);
            index = 0;
            lenght = styles.Count;
            if (lenght > 0)
            {
                client = new WebClient();
                client.OpenReadCompleted += new OpenReadCompletedEventHandler(client_OpenReadStyleCompleted);
                client.DownloadProgressChanged +=
                        new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                //加载即循环体
                load(styles[index].Source);
            }
            else
            {
                LoadParts();
            }
        }

        /// <summary>
        ///  加载一个样式资源成功
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void client_OpenReadStyleCompleted(object sender, OpenReadCompletedEventArgs e)
        {

            downed += e.Result.Length;
            PFApp.setStatu(string.Format("— {0}% —", (int)((downed / size) * 100)));
            PFApp.setStatu(downed, size);
            //加载xaml类型的资源字典
            if (styles[index].Source.EndsWith(".xaml"))
            {
                //读取加载到的资源
                using (StreamReader streamRead = new StreamReader(e.Result, true))
                {
                    //取得xaml字符串，并加载到app中
                    LoadFromXaml(streamRead.ReadToEnd());
                }
            }
            //加载dll形式的资源字典 
            else
            {
                //读取加载到的资源
                AssemblyPart assemblyPart = new AssemblyPart();
                Assembly assembly = assemblyPart.Load(e.Result);
                foreach (string item in assembly.GetManifestResourceNames())
                {
                    if (!item.EndsWith(".xaml"))
                        return;
                    //读取加载到的资源
                    using (StreamReader streamRead = new StreamReader(assembly.GetManifestResourceStream(item)))
                    {
                        //取得xaml字符串，并加载到app中
                        LoadFromXaml(streamRead.ReadToEnd());
                    }

                }
            }

            //判断是不是最后一个资源
            if (++index < lenght)
            {
                load(styles[index].Source);
            }
            else
            {
                //加载应用
                LoadParts();

            }
        }

        /// <summary>
        /// 加载Xaml文件到app的资源字典中
        /// </summary>
        /// <param name="xaml"></param>
        private void LoadFromXaml(string xaml)
        {
            ResourceDictionary dic = (ResourceDictionary)System.Windows.Markup.XamlReader.Load(xaml);
            foreach (var item in dic.Keys)
            {
                if ((PFApp.Current as PFApp).Resources.Contains(item))
                {
                    (PFApp.Current as PFApp).Resources.Remove(item);
                }
                (PFApp.Current as PFApp).Resources.Add(item, dic[item]);
            }

        }
        #endregion

        #region 加载Part程序集
        /// <summary>
        ///  加载Part
        /// </summary>
        private void LoadParts()
        {
            //PFApp.setStatu(Constant.LoadPartTip);
            index = 0;
            if (parts != null)
            {
                lenght = parts.Count;
            }
            else
            {
                lenght = 0;
            }
            if (lenght > 0)
            {

                client = new WebClient();
                client.OpenReadCompleted += new OpenReadCompletedEventHandler(client_OpenReadPartCompleted);
                client.DownloadProgressChanged +=
                            new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                load(parts[index].Source);
            }
            else
            {
                //加载首页，本应该用事件，因为构建在平台内，就直接调用了
                (PFApp.Current as PFApp).LoadMainPage();
            }
        }


        /// <summary>
        ///  加载一个Part成功
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void client_OpenReadPartCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            try
            {
                downed += e.Result.Length;
                PFApp.setStatu(string.Format("— {0}% —", (int)((downed / size) * 100)));
                PFApp.setStatu(downed, size);
                //解析Part资源，反射出自描述信息，对系统ConfigData的Part资源进行补充
                AssemblyPart assemblyPart = new AssemblyPart();
                Assembly assembly = assemblyPart.Load(e.Result);

                //遍历模块，找到应用首页
                foreach (var type in assembly.GetTypes())
                {

                    Type inter = type.GetInterface(Constant.IPartName, false);
                    if (inter != null)
                    {
                        //反射出应用的信息，并补充配置信息
                        IPart part = assembly.CreateInstance(type.FullName) as IPart;


                        PartDescriptor pd = part.Descri;
                        pd.typeName = type.FullName;
                        pd.type = type;
                        string path = parts[index].Source;
                        path = path.Remove(path.LastIndexOf(Constant.Sprit));
                        path = path.Remove(0, path.LastIndexOf(Constant.Sprit) + 1);
                        if (path.IndexOf(Constant.Horizontal) == -1)
                        {

                        }
                        else
                        {
                            int GroupIndex = int.Parse(path.Substring(0, path.IndexOf(Constant.Horizontal)));
                            string GroupName = path.Remove(0, path.IndexOf(Constant.Horizontal) + 1);
                            pd.GroupIndex = GroupIndex;
                            pd.GroupName = GroupName;

                            if (PFApp.parts.ContainsKey(GroupName))
                            {
                                PFApp.parts[pd.GroupName].Add(pd);
                            }
                            else
                            {
                                IList<PartDescriptor> lst = new List<PartDescriptor>();
                                lst.Add(pd);
                                PFApp.parts.Add(GroupName, lst);
                            }

                            if (!PFApp.partsIndex.ContainsKey(GroupIndex))
                            {
                                PFApp.partsIndex.Add(GroupIndex, GroupName);
                            }
                        }
                        if (PFApp.UIS.ContainsKey(part.Descri.Name))
                        {
                            //part.Descri.Name = DateTime.Now.Ticks.ToString();
                            //PFApp.UIS.Add(part.Descri.Name, (UIElement)part);
                        }
                        else
                        {
                            PFApp.UIS.Add(part.Descri.Name, (UIElement)part);
                        }
                    }

                }

                //判断是不是最后一个资源
                if (++index < lenght)
                {
                    load(parts[index].Source);
                }
                else
                {
                    //加载首页，本应该用事件，因为构建在平台内，就直接调用了
                    (PFApp.Current as PFApp).LoadMainPage();
                }
            }
            catch (Exception ex)
            {
                Message.ShowErrorInfo("下载地图平台失败", ex.Message);
            }
        }
        #endregion


        /// <summary>
        /// 异步加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage != 100)
            {
                double res = downed + e.BytesReceived;
                PFApp.setStatu(string.Format("— {0}% —", (int)((res / size) * 100)));
                PFApp.setStatu(res, size);
            }
        }

        /// <summary>
        /// 根据路径，加载实际资源或程序集
        /// </summary>
        /// <param name="Source"></param>
        private void load(string Source)
        {
            if (!client.IsBusy)
            {
                //获得相对的地址
                string baseUrl = client.BaseAddress.Remove(client.BaseAddress.LastIndexOf(Constant.Sprit) + 1);
                //MessageBox.Show(baseUrl + Source);
                client.OpenReadAsync(new Uri(baseUrl + Source));

            }
            else
            {
                load(Source);
            }
        }
    }
}
