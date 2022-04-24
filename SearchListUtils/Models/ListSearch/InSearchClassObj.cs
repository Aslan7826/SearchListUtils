namespace SearchListUtils.Models.ListSearch
{
    public class InSearchClassObj<T> where T : class
    {
        /// <summary>
        /// 模糊搜尋
        /// </summary>
        public T Vague { get; set; }
        /// <summary>
        /// 精準搜尋
        /// </summary>
        public T Precise { get; set; }
    }
}
