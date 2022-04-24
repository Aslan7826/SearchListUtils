using SearchListUtils.Models.Eunms;

namespace SearchListUtils.Models.ListSearch
{
    public class InSearchTimeObj
    {
        public TimeperiodType TimePeriodType { get; set; } = TimeperiodType.Day;
        private DateTime? startTime;
        public DateTime? StartTime
        {
            get
            {
                if (startTime.HasValue)
                {
                    switch (TimePeriodType)
                    {
                        case TimeperiodType.Month:
                            return new DateTime(startTime.Value.Year, startTime.Value.Month, 1);
                        case TimeperiodType.Year:
                            return new DateTime(startTime.Value.Year, 1, 1);
                    }
                }
                return startTime;
            }
            set { startTime = value; }
        }

        private DateTime? endTimd;
        public DateTime? EndTime
        {
            get
            {
                if (endTimd.HasValue)
                {
                    switch (TimePeriodType)
                    {
                        case TimeperiodType.Month:
                            return new DateTime(endTimd.Value.Year, endTimd.Value.Month, 1).AddMonths(1).AddDays(-1);
                        case TimeperiodType.Year:
                            return new DateTime(endTimd.Value.Year, 12, 31);
                    }
                }
                return endTimd;
            }
            set { endTimd = value; }
        }
    }
}
