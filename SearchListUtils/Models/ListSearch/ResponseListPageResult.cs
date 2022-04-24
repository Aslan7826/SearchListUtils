namespace SearchListUtils.Models.ListSearch
{
    /// <summary>
    /// 通用平台API格式
    /// </summary>
    public class ResponseListPageResult 
    {
        public object Data { get; set; }    
        public int Total { get; set; }
        public int Page { get; set; }
        public int TotalPage { get; set; }

        public bool HasPreviousPage { get => Page > 1; }
        public string PreviousPage { get; set; }
        public bool HasNextPage { get => Page < TotalPage; }
        public string NextPage { get; set; }

    }
}
