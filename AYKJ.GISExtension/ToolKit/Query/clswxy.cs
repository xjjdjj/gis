using System.ComponentModel.DataAnnotations;

namespace AYKJ.GISExtension
{
    public class clswxy
    {
        [Display(Name = "ID", GroupName = "clswxy")]
        public string wxyid { set; get; }
        [Display(Name = "名称", GroupName = "clswxy")]
        public string wxyname { set; get; }
        [Display(Name = "类别", GroupName = "clswxy")]
        public string wxytype { set; get; }
        [Display(Name = "单位代码", GroupName = "clswxy")]
        public string wxydwdm { set; get; }
    }
}
