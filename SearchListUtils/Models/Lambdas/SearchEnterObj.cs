namespace SearchListUtils.Models.Lambdas
{
    public class SearchEnterObj
    {
        public SearchEnterObj()
        {
            InType = new InTypeObj();
            Recursive = new RecursiveObj();
        }
        public SearchEnterObj(Type type)
        {
            InType = new InTypeObj()
            {
                ThisType = type
            };
            Recursive = new RecursiveObj();
        }
        public SearchEnterObj(Type type, bool isAnd)
        {
            InType = new InTypeObj()
            {
                ThisType = type
            };
            Recursive = new RecursiveObj();
            this.IsAnd = isAnd;
        }
        public SearchEnterObj(Type type, bool isAnd, int maxLayer, bool isListStringSearch)
        {
            InType = new InTypeObj()
            {
                ThisType = type
            };
            Recursive = new RecursiveObj()
            {
                MaxLayer = maxLayer
            };
            this.IsAnd = isAnd;
            IsListStringSearch = isListStringSearch;
        }

        public InTypeObj InType { get; set; }
        public RecursiveObj Recursive { get; set; }
        public SearchFieldObj Search { get; set; }
        public bool IsAnd { get; set; }
        public bool IsListStringSearch { get; set; }
        public SearchEnterObj DeepCopy()
        {
            SearchEnterObj result = new SearchEnterObj(this.InType.ThisType, this.IsAnd, this.Recursive.MaxLayer, IsListStringSearch);
            result.Recursive.ThisLayer = this.Recursive.ThisLayer;
            return result;
        }

    }
}
