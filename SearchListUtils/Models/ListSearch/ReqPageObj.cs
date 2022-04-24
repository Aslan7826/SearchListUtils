using System.ComponentModel.DataAnnotations;

namespace SearchListUtils.Models.ListSearch
{
    /// <summary>
    /// 通用平台API分頁查詢格式
    /// </summary>
    public class ReqPageObj
    {
        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;
        [Range(1, int.MaxValue)]
        public int Size { get; set; } = 20;
    }
}
