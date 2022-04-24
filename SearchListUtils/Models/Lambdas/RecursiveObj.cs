namespace SearchListUtils.Models.Lambdas
{
    public class RecursiveObj
    {

        public RecursiveObj()
        {
            this.MaxLayer = 1;

        }
        public RecursiveObj(int maxLayer)
        {
            this.MaxLayer = maxLayer;
        }

        public int MaxLayer { get; set; }
        public int ThisLayer { get; set; } = 0;
        public void UpThisLayer()
        {
            this.ThisLayer += 1;
        }
    }
}
