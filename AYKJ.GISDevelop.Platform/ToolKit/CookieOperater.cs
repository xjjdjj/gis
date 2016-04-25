/// <summary>  
/// 作者：陈锋 
/// 时间：2012/5/21 15:53:38  
/// 公司:南京安元科技有限公司  
/// 版权：2012-2020  
/// CLR版本：4.0.30319.261  
/// CookieOperater说明：
/// 唯一标识：787f97c0-ea4d-4a9c-a6af-88505be002d9  
/// </summary>

using System;
using System.Windows.Browser;

namespace AYKJ.GISDevelop.Platform.ToolKit
{
    public class CookieOperater
    {
        #region Cookie相关操作函数

        #region 设置持久时间长的Cookie

        /// <summary>
        /// 设置cookies
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void SetCookie(string key, string value)
        {
            string oldCookie = HtmlPage.Document.GetProperty("cookie") as String;
            DateTime expiration = DateTime.UtcNow + TimeSpan.FromDays(2000);
            string cookie = String.Format("{0}={1};expires={2}", key, value, expiration.ToString("R"));
            HtmlPage.Document.SetProperty("cookie", cookie);
        }
        #endregion

        #region 读取一个已经存在的Cookie
        /// <summary>
        /// 读取一个已经存在的Cookie
        /// </summary>
        /// <param name="key">cookie key</param>
        /// <returns>null if the cookie does not exist, otherwise the cookie value</returns>
        public static string GetCookie(string key)
        {
            string[] cookies = HtmlPage.Document.Cookies.Split(';');
            key += '=';
            foreach (string cookie in cookies)
            {
                string cookieStr = cookie.Trim();
                if (cookieStr.StartsWith(key, StringComparison.OrdinalIgnoreCase))
                {
                    string[] vals = cookieStr.Split('=');
                    if (vals.Length >= 2)
                    {
                        return vals[1];
                    }
                    return string.Empty;
                }
            }
            return null;
        }
        #endregion

        #region 删除特定的Cookie(清空它的Value值，过期值设置为-1天)
        /// <summary>
        /// 删除特定的Cookie(清空它的Value值，过期值设置为-1天)
        /// </summary>
        /// <param name="key">the cookie key to delete</param>
        public static void DeleteCookie(string key)
        {
            string oldCookie = HtmlPage.Document.GetProperty("cookie") as String;
            DateTime expiration = DateTime.UtcNow - TimeSpan.FromDays(1);
            string cookie = String.Format("{0}=;expires={1}", key, expiration.ToString("R"));
            HtmlPage.Document.SetProperty("cookie", cookie);
        }
        #endregion

        #region 判定指定的key-value对是否在cookie中存在
        /// <summary>
        /// 判定指定的key-value对是否在cookie中存在
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool Exists(string key, string value)
        {
            return HtmlPage.Document.Cookies.Contains(
                String.Format("{0}={1}", key, value)
                );
        }
        #endregion


        #region 判定指定的key是否在cookie中存在
        /// <summary>
        /// 判定指定的key-value对是否在cookie中存在
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool Exists(string key)
        {
            if (GetCookie(key) == null)
                return false;
            else
                return true;
        }
        #endregion

        #region 获取当前cookie内容
        /// <summary>
        /// 获取当前cookie内容
        /// </summary>
        /// <returns></returns>
        public static string GetCookieContent()
        {
            return HtmlPage.Document.Cookies;
        }
        #endregion

        #endregion
    }
}
