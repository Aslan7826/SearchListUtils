using SearchListUtils.Models.Lambdas;
using System.Linq.Expressions;
using System.Reflection;

namespace SearchListUtils.Models.Interfaces
{
    internal interface ITypeToLambdaService
    {
        Expression RunExpression(PropertyInfo propertyInfo, Expression entity, SearchEnterObj searchData);
    }
}
