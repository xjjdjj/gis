/// <summary>  
/// 作者：陈锋 
/// 时间：2012/5/21 15:46:17  
/// 公司:南京安元科技有限公司  
/// 版权：2012-2020  
/// CLR版本：4.0.30319.261  
/// ConfigManage说明：加载配置信息，解析配置信息
/// 唯一标识：cd40a80a-05d3-43d2-bdc4-7bc1ae004ef8  
/// </summary>

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using AYKJ.GISDevelop.Platform.Config.Entity;
using AYKJ.GISDevelop.Platform.ToolKit;
using System;

namespace AYKJ.GISDevelop.Platform.Config
{
    public class ConfigManage
    {
        //配置xml文件的根目录
        private XElement configRoot;

        /// <summary>
        /// 获取指定XPath下的Element（XPath，在NET,JAVA中经常用的xml节点表达）
        /// 已知BUG：只有在XPATH的最内层可以有重复的节点
        /// </summary>
        /// <param name="XPath">节点路径</param>
        /// <returns></returns>
        public IEnumerable<XElement> ConfigElement(string XPath)
        {
            IEnumerable<XElement> elements = null;
            //将XPath中的每一个节点组成数组
            string[] paths = XPath.Split(Constant.Dot);
            elements = configRoot.Elements(paths[0]);
            //遍历节点直到最内层
            for (int i = 1; i < paths.Length; i++)
            {
                elements = elements.Elements(paths[i]);
            }
            return elements;
        }

        /// <summary>
        /// 根据节点获得属性值
        /// </summary>
        /// <param name="attributeName">属性的名称</param>
        /// <param name="element">节点信息</param>
        /// <returns></returns>
        public string ConfigValue(string attributeName, XElement element)
        {
            string value = string.Empty;
            value = element.Attribute(attributeName).Value;
            return value;
        }

        /// <summary>
        /// 根据节点获得节点值
        /// </summary>
        /// <param name="element">节点信息</param>
        /// <returns></returns>
        public string ConfigValue(XElement element)
        {
            string value = string.Empty;
            value = element.Value;
            return value;
        }

        public ConfigData LoadConfig(XDocument configDoc)
        {
            configRoot = configDoc.Root;
            //实例化ConfigData并返回
            ConfigData config = new ConfigData();
            string AssmblyPath = Constant.AssmblyPath;
            //文件大小
            bool parse = true;
            string error = "", warning = "";
            
            try
            {
                config.FilesSize = double.Parse(configRoot.Element(AssmblyPath).Attribute(Constant.AssmblySize).Value);
            }
            catch (Exception ex)
            {
                parse = false;
                warning += "无法获取文件大小:" + ex.Message;
            }
            //提取程序集的信息
            try
            {
                config.SystemEntities = (from item in ConfigElement(Constant.SystemPath).ElementAt(0).Attribute(Constant.AttributeSource).Value.Split(Constant.Semicolon)
                                         where item != string.Empty
                                         select new SystemEntity()
                                         {
                                             Source = item
                                         }).ToList();
            }
            catch (Exception ex) {
                parse = false;
                error += "无法获取程序集信息:" + ex.Message;
            }
            //提取菜单信息
            try
            {
                config.MenuEntityies = (from item in ConfigElement(Constant.MenuPath)
                                        select new MenuEntity
                                        {
                                            Title = item.Attribute(Constant.AttributeTitle).Value,
                                            MenuName = item.Attribute(Constant.AttributeMenuName).Value,
                                            Describe = item.Attribute(Constant.AttributeDescribe).Value,
                                            Type = item.Attribute(Constant.AttributeType).Value,
                                            Visible = item.Attribute(Constant.AttributeVisible).Value
                                        }).ToList();
            }
            catch (Exception ex)
            {
                parse = false;
                error += "无法获取菜单信息:" + ex.Message;
            }
            
            //提取样式字典即功能页的信息
            try
            {
                config.StyleEntities = (from item in ConfigElement(Constant.StylePath).ElementAt(0).Attribute(Constant.AttributeSource).Value.Split(Constant.Semicolon)
                                        where item != string.Empty
                                        select new StyleEntity()
                                        {
                                            Source = item
                                        }).ToList();

            }
            catch (Exception ex)
            {
                parse = false;
                warning += "无法提取功能插件样式:" + ex.Message;
            }
            //提取Part即功能页的信息
            //if (ConfigElement(Constant.PartPath).Elements().ToList().Count() != 0)
            //{
            try
            {
                config.PartEntities = (from item in ConfigElement(Constant.PartPath).ElementAt(0).Attribute(Constant.AttributeSource).Value.Split(Constant.Semicolon)
                                       where item != string.Empty
                                       select new PartEntity()
                                       {
                                           Source = item
                                       }).ToList();
                //}
            }
            catch (Exception ex)
            {
                parse = false;
                error += "无法提取功能插件:" + ex.Message;
            }

            //获取扩展信息
            try
            {
                config.Extents = ConfigElement(Constant.ExtentPath).ElementAt(0);
            }
            catch (Exception ex) {
                parse = false;
                error += "无法提取扩展信息:" + ex.Message;
            }
            try
            {
                config.Debugs = ConfigElement(Constant.DebugsPath).ElementAt(0);
            }
            catch (Exception ex)
            {
                config.Debugs = new XElement("Debugs", new XAttribute("Open", "True"));
                parse = false;
                warning += "无法提取扩展信息:" + ex.Message;
            }

            if (!parse)
            {
                string info = "";
                if (warning != "")
                {
                    info += "警告信息:"+warning;
                }
                if (error != "")
                {
                    info += "错误信息:" + error;
                }              
                Message.ShowErrorInfo("解析配置文件出错", info);
                if (error != "")
                    return null;
            }

            return config;
        }

        /// <summary>
        /// 加载配置信息
        /// </summary>
        /// <param name="configUrl">地址</param>
        /// <returns></returns>
        public ConfigData LoadConfig(string configUrl)
        {
            Ajax ajax = Ajax.GetInstanse();
            string responseText = ajax.Send("ClientBin/Config/Plat.xml");
            //string responseText = ajax.Send("http://10.0.0.119/TY/ClientBin/Config/Plat.xml");
            //string responseText = ajax.Send("ClientConfig.ashx");
            //string responseText = ajax.Send("http://10.0.0.119/AYKJ/Develop/ClientConfig.ashx");
            configRoot = XDocument.Parse(responseText.Trim()).Root;

            //实例化ConfigData并返回
            ConfigData config = new ConfigData();

            //文件大小
            config.FilesSize = double.Parse(configRoot.Element(Constant.AssmblyPath).Attribute(Constant.AssmblySize).Value);

            //提取程序集的信息
            config.SystemEntities = (from item in ConfigElement(Constant.SystemPath).ElementAt(0).Attribute(Constant.AttributeSource).Value.Split(Constant.Semicolon)
                                     where item != string.Empty
                                     select new SystemEntity()
                                     {
                                         Source = item
                                     }).ToList();

            //提取菜单信息
            config.MenuEntityies = (from item in ConfigElement(Constant.MenuPath)
                                    select new MenuEntity
                                    {
                                        Title = item.Attribute(Constant.AttributeTitle).Value,
                                        MenuName = item.Attribute(Constant.AttributeMenuName).Value,
                                        Describe = item.Attribute(Constant.AttributeDescribe).Value,
                                        Type = item.Attribute(Constant.AttributeType).Value,
                                        Visible=item.Attribute(Constant.AttributeVisible).Value
                                    }).ToList();


            //提取样式字典即功能页的信息
            config.StyleEntities = (from item in ConfigElement(Constant.StylePath).ElementAt(0).Attribute(Constant.AttributeSource).Value.Split(Constant.Semicolon)
                                    where item != string.Empty
                                    select new StyleEntity()
                                    {
                                        Source = item
                                    }).ToList();


            //提取Part即功能页的信息
            //if (ConfigElement(Constant.PartPath).Elements().ToList().Count() != 0)
            //{
            config.PartEntities = (from item in ConfigElement(Constant.PartPath).ElementAt(0).Attribute(Constant.AttributeSource).Value.Split(Constant.Semicolon)
                                   where item != string.Empty
                                   select new PartEntity()
                                   {
                                       Source = item
                                   }).ToList();
            //}


            //获取扩展信息
            config.Extents = ConfigElement(Constant.ExtentPath).ElementAt(0);
            try
            {
                config.Debugs = ConfigElement(Constant.DebugsPath).ElementAt(0);
            }
            catch (Exception ex)
            {
                config.Debugs = new XElement("Debugs", new XAttribute("Open", "False"));  
            }
            return config;
        }

        public ConfigData LoadXml(string str)
        {
            configRoot = XElement.Parse(str);

            //实例化ConfigData并返回
            ConfigData config = new ConfigData();

            //文件大小
            config.FilesSize = double.Parse(configRoot.Element(Constant.AssmblyPath).Attribute(Constant.AssmblySize).Value);

            //提取程序集的信息
            config.SystemEntities = (from item in ConfigElement(Constant.SystemPath).ElementAt(0).Attribute(Constant.AttributeSource).Value.Split(Constant.Semicolon)
                                     where item != string.Empty
                                     select new SystemEntity()
                                     {
                                         Source = item
                                     }).ToList();

            //提取菜单信息
            config.MenuEntityies = (from item in ConfigElement(Constant.MenuPath)
                                    select new MenuEntity
                                    {
                                        Title = item.Attribute(Constant.AttributeTitle).Value,
                                        MenuName = item.Attribute(Constant.AttributeMenuName).Value,
                                        Describe = item.Attribute(Constant.AttributeDescribe).Value,
                                        Type = item.Attribute(Constant.AttributeType).Value
                                    }).ToList();

            //提取样式字典即功能页的信息
            config.StyleEntities = (from item in ConfigElement(Constant.StylePath).ElementAt(0).Attribute(Constant.AttributeSource).Value.Split(Constant.Semicolon)
                                    where item != string.Empty
                                    select new StyleEntity()
                                    {
                                        Source = item
                                    }).ToList();


            //提取Part即功能页的信息
            config.PartEntities = (from item in ConfigElement(Constant.PartPath).ElementAt(0).Attribute(Constant.AttributeSource).Value.Split(Constant.Semicolon)
                                   where item != string.Empty
                                   select new PartEntity()
                                   {
                                       Source = item
                                   }).ToList();
            //获取扩展信息
            config.Extents = ConfigElement(Constant.ExtentPath).ElementAt(0);
            config.Debugs = ConfigElement(Constant.DebugsPath).ElementAt(0);
            return config;
        }
    }
}
