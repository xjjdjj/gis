/// <summary>  
/// 作者：陈锋 
/// 时间：2012/5/21 15:56:29  
/// 公司:南京安元科技有限公司  
/// 版权：2012-2020  
/// CLR版本：4.0.30319.261  
/// Message说明：
/// 唯一标识：26a6e9ac-948e-4489-8643-1b677afbea44  
/// </summary>
using System.Linq;

namespace AYKJ.GISDevelop.Platform.ToolKit
{
    public class Message
    {
        /// <summary>
        /// 显示普通信息提示窗口
        /// </summary>
        /// <param name="info">信息内容</param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static bool Show(string info, string title = "提示信息")
        {
            MessageWindow msg = new MessageWindow(MsgType.Info, title, info);
            msg.MessageBtnOpera("only");
            //DebugOpen(msg,info);

            msg.Show();
            return true;
        }

        public static void DebugOpen(MessageWindow msg,string info){
            try
            {
                //不开启调试功能
                if (PFApp.Debugs.Attribute("Open").Value == "True")
                {
                    var debug = (from item in PFApp.Debugs.Elements("Debug")
                                 where item.Attribute("Tile").Value == info
                                 select new
                                 {
                                     Code = item.Attribute("Code").Value,
                                     Info = item.Attribute("Info").Value,
                                     Open = item.Attribute("Open").Value
                                 }).ToList();
                    if (debug.Count > 0 && debug[0].Open == "True")
                    {
                        msg.setMoreInfo(debug[0].Code, debug[0].Info);
                    }
                }
            }
            catch
            {
                msg.setMoreInfo("0000", info);
            }
        }

        public static void DebugOpen(MessageWindow msg, string title, string info)
        {
            try
            {
                //当配置文件初始化出错的时候，会有问题
                if (PFApp.Debugs.Attribute("Open").Value == "True")
                {
                    var debug = (from item in PFApp.Debugs.Elements("Debug")
                                 where item.Attribute("Tile").Value == title
                                 select new
                                 {
                                     Code = item.Attribute("Code").Value,
                                     Open = item.Attribute("Open").Value
                                 }).ToList();
                    if (debug.Count > 0 && debug[0].Open == "True")
                    {
                        msg.setMoreInfo(debug[0].Code, info);
                    }
                    //配置文件中不存在，但是有传入错误描述title和错误内容info
                    else if (debug.Count == 0 && title != "" && info != "")
                    {
                        msg.setMoreInfo("0000", info);
                    }
                }
            }
            catch
            {
                msg.setMoreInfo("0000", info);
            }
        }

        public static bool ShowInfo(string info, string title = "提示信息")
        {
            MessageWindow msg = new MessageWindow(MsgType.Info, title, info);

            DebugOpen(msg,info);

            msg.Show();
            return true;
        }

        public static bool ShowErrorInfo(string error, string info, string title = "错误信息")
        {
            MessageWindow msg = new MessageWindow(MsgType.Error, title, error);
            msg.MessageBtnOpera("only");
            DebugOpen(msg, error, info);
            msg.Show();
            return true;
        }

        public static bool ShowError(string error, string title = "错误信息")
        {
            MessageWindow msg = new MessageWindow(MsgType.Error, title, error);
            msg.MessageBtnOpera("only");
            DebugOpen(msg,error);
            msg.Show();
            return true;
        }
    }
}
