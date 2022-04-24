using System.Linq.Expressions;

namespace SearchListUtils.Models.Lambdas
{
    public class InTypeObj
    {
        public Type ThisType { get; set; }
        public MemberExpression Member { get; set; } = null;
    }
}
