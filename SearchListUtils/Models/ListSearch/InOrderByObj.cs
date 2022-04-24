using System.ComponentModel;

namespace SearchListUtils.Models.ListSearch
{
    public class InOrderByObj
    {
        [DisplayName("回傳名稱")]
        public string? OrderName { get; set; } = null;
        [DisplayName("排列順序")]
        public bool IsDesc { get; set; } = false;
    }
}
