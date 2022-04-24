using System.ComponentModel;

namespace SearchListUtils.Models.ListSearch
{
    public class InSearchObj
    {
        [DisplayName("查詢字串")]
        public string? SegmentStringbyblank { get; set; } = null;
        [DisplayName("字串分割")]
        public string KeywordSeparator { get; set; } = " ";
    }
}
