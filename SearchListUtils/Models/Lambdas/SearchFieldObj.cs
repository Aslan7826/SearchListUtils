using SearchListUtils.Models.Interfaces;

namespace SearchListUtils.Models.Lambdas
{
    public class SearchFieldObj
    {
        public List<string> SQLField { get; set; }
        public List<string> NoSearchField { get; set; }
        public List<ISearchStringObj> SearchData { get; set; }
    }
}
