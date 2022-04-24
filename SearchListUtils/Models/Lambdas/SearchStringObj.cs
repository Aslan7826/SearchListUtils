using SearchListUtils.Models.Interfaces;

namespace SearchListUtils.Models.Lambdas
{
    public class SearchStringObj : ISearchStringObj
    {
        public string Value { get; set; }
        public bool IsVague { get; set; }
    }
}
