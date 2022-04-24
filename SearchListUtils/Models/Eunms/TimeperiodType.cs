using System.ComponentModel;

namespace SearchListUtils.Models.Eunms
{
    public enum TimeperiodType
    {
        None = 0,
        [Description("秒")]
        Seconds = 1,
        [Description("分")]
        Minute = 2,
        [Description("小時")]
        Hour = 3,
        [Description("天")]
        Day = 4,
        [Description("周")]
        Week = 5,
        [Description("月")]
        Month = 6,
        [Description("年")]
        Year = 7,
    }
}
