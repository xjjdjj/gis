/// <summary>  
/// 作者：陈锋 
/// 时间：2012/5/21 15:52:02  
/// 公司:南京安元科技有限公司  
/// 版权：2012-2020  
/// CLR版本：4.0.30319.261  
/// Ajax说明：
/// 唯一标识：c2972629-ffdf-4616-accf-d249a87d13a8  
/// </summary>

using System.Windows.Browser;
namespace AYKJ.GISDevelop.Platform.ToolKit
{
    /// <summary>
    /// js异步请求
    /// </summary>
    public class Ajax
    {
        /// <summary>
        /// AJAX的调用函数
        /// </summary>
        private const string ajaxHelper = "function ajaxHelper(url){var objXmlHttp=null;try{objXmlHttp = new XMLHttpRequest();}catch(e){try{objXmlHttp = new ActiveXObject('Microsoft.XMLHTTP');}catch(e){try{objXmlHttp = new ActiveXObject('Msxml2.XMLHTTP');}catch(e){alert('error opening XMLHTTP');}}}objXmlHttp.open('GET',url,false);objXmlHttp.send();while(true){if(objXmlHttp.readyState==4&&objXmlHttp.status==200){return objXmlHttp.responseText;}}}";

        /// <summary>
        /// 请求Ajax
        /// </summary>
        private const string ajaxSend = "ajaxHelper('{0}')";

        /// <summary>
        /// 自生实例
        /// </summary>
        private static Ajax ins;

        /// <summary>
        /// 线程锁
        /// </summary>
        private static readonly object lockHelper = new object();

        /// <summary>
        /// 初始化方法
        /// </summary>
        private Ajax()
        {
            HtmlPage.Window.Eval(ajaxHelper);
        }

        /// <summary>
        /// 获取实例
        /// </summary>
        /// <returns></returns>
        public static Ajax GetInstanse()
        {
            if (ins == null)
            {
                lock (lockHelper)
                {
                    if (ins == null)
                        ins = new Ajax();
                }
            }
            return ins;
        }

        /// <summary>
        /// 发送请求
        /// </summary>
        /// <param name="url"></param>
        public string Send(string url)
        {
            return HtmlPage.Window.Eval(string.Format(ajaxSend, url)).ToString();
        }
    }
}
