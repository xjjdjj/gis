/// <summary>  
/// 作者：陈锋 
/// 时间：2012/5/21 15:48:10  
/// 公司:南京安元科技有限公司  
/// 版权：2012-2020  
/// CLR版本：4.0.30319.261  
/// IPart说明：Part的接口，约束自描述信息(包含JS接口)
/// 唯一标识：02ab651c-7e0c-4c68-8807-28548580b1c7  
/// </summary>

using System.Windows;
using System.Collections.Generic;
using System.Windows.Controls;

namespace AYKJ.GISDevelop.Platform.Part
{
    public delegate void PartEventHander(IPart sender);

    public interface IPart
    {
        event PartEventHander ReInitEnd;
        event PartEventHander ShowEnd;
        event PartEventHander CloseEnd;

        bool IsOpen { get; }
        PartDescriptor Descri { get; }
        void ReInit();
        void Show();
        void Close();

        #region JS接口
        event PartEventHander LinkGisPlatformEnd;
        string LinkReturnGisPlatform(string mark, string s);
        string LinkReturnGisPlatform(string mark, object obj1, object obj2);
        //20120912
        string LinkFromGiPlatform(string oAction, string oStr, object oArr, object oCls, object[] oArrStr, object[] oArrArr, object[] oArrCls);
        #endregion
    }
}
