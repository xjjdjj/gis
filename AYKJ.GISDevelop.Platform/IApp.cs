/// <summary>  
/// 作者：陈锋 
/// 时间：2012/5/21 16:03:25  
/// 公司:南京安元科技有限公司  
/// 版权：2012-2020  
/// CLR版本：4.0.30319.261  
/// IApp说明：
/// 唯一标识：a0c8a4b5-b8de-4074-a62f-041d89c17c86  
/// </summary>

using System;
using ESRI.ArcGIS.Client;
using System.Collections.Generic;
using System.Windows.Controls;

namespace AYKJ.GISDevelop.Platform
{
    public delegate void ClearAll();
    public interface IApp
    {
        Map MainMap { get; }

        event ClearAll ClearAll;

        IMainPage MainPage { get; }

        List<Graphic> lstThematic { get; }

        Dictionary<string, string> DictThematicEnCn { get; }

        Dictionary<string, GraphicsLayer> Dict_ThematicLayer { get; }
        #region 行政区划
        Dictionary<string, string> Dict_Xzqz_sy { get; }
        Dictionary<string, Graphic> Dict_Xzqz_sygra { get; }
        Dictionary<string, string> Dict_Xzqz_qx { get; }
        Dictionary<string, Graphic> Dict_Xzqz_qxgra { get; }
        #endregion
        #region 网格查看
        bool IsGridFinished { get; }
        Dictionary<string, string> Dict_SubCode_FID { get; }
        Dictionary<string, string> Dict_subgrid{get;}
        Dictionary<string, string> Dict_grid { get; }
        Dictionary<string, Graphic> Dict_subgrid_gra { get; }
        Dictionary<string, Graphic> Dict_grid_gra { get; }
        #endregion
        //20120815
        string strImageType { get; set; }
        string strAddMark { get; set; }
        string strAddData { get; set; }

        //20120816
        Dictionary<string, List<clsThematic>> dict_thematic { get; }

        //20120824
        Grid mapLayoutRoot { get; }

        //20121008
        List<Image> syslistthisimg { get; set; }

        //20120918
        string strWcfMark { get; set; }

        //20150327：江宁智慧安监
        string sysRole { get; set; }//系统登录角色
        List<string> iListSelectStreet { get; set; }//已选择显示的“街道”
        List<string> iListSelectEnterprise { get; set; }//已选择显示的“企业类型”
    }
}
