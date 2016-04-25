/// <summary>  
/// 作者：陈锋 
/// 时间：2012/5/21 16:05:24  
/// 公司:南京安元科技有限公司  
/// 版权：2012-2020  
/// CLR版本：4.0.30319.261  
/// IWelcomePage说明：系统欢迎页的接口
/// 唯一标识：f007ae9f-a7e8-45e7-9ad3-c8cec9351b60  
/// </summary>

namespace AYKJ.GISDevelop.Platform
{
    public interface IWelcomePage
    {

        /// <summary>
        /// 改变加载状态
        /// </summary>
        /// <param name="statu"></param>
        void SetStatu(string statu);


        /// <summary>
        /// 进度显示 
        /// </summary>
        /// <param name="res"></param>
        /// <param name="all"></param>
        void SetStatu(double res, double all);
    }
}
