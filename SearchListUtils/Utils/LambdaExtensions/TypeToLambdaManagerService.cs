using SearchListUtils.Models.Eunms;
using SearchListUtils.Models.Interfaces;
using SearchListUtils.Models.Lambdas;
using System.Linq.Expressions;
using System.Reflection;

namespace SearchListUtils.Utils.LambdaExtensions
{
    /// <summary>
    /// Type轉換至Lambda的管理器
    /// </summary>
    public class TypeToLambdaManagerService
    {
        static Dictionary<TypeClassification, ITypeToLambdaService> _DictTypeLambda;
        public TypeToLambdaManagerService()
        {
            if (_DictTypeLambda is null)
            {
                _DictTypeLambda = new Dictionary<TypeClassification, ITypeToLambdaService>()
                {
                    { TypeClassification.TypeClass, new ClassSelectType() }
                   ,{ TypeClassification.TypeIEnumClass, new IEnumOfClassType() }
                   ,{ TypeClassification.TypeIEnumValue, new IEnumOfValueType() }
                   ,{ TypeClassification.TypeValue, new ValueTypeOrStringSelectType() }
                   ,{ TypeClassification.TypeString, new ValueTypeOrStringSelectType() }
                };
            }
        }

        public Expression GetExpression(PropertyInfo propertyInfo, Expression entity, SearchEnterObj searchData)
        {
            var typeClass = propertyInfo.PropertyType.CheckThisType();
            if (_DictTypeLambda.TryGetValue(typeClass, out ITypeToLambdaService action))
            {
                var result = action.RunExpression(propertyInfo, entity, searchData);
                return result;
            }
            return null;
        }
    }


    /// <summary>
    /// 基礎模型
    /// </summary>
    internal abstract class TypeToLambdaService : ITypeToLambdaService
    {
        /// <summary>
        /// 執行轉換比對流程
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <param name="entity"></param>
        /// <param name="searchData"></param>
        /// <returns></returns>
        public Expression RunExpression(PropertyInfo propertyInfo, Expression entity, SearchEnterObj searchData)
        {
            var memberExpression = Expression.Property(entity, propertyInfo);
            var exp = GetExpression(propertyInfo, memberExpression, searchData);
            if (exp != null)
            {
                return CheckAddNotNull(propertyInfo, memberExpression, exp);
            }
            return null;
        }
        /// <summary>
        /// 實做判斷功能
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <param name="memberExpression"></param>
        /// <param name="SearchData"></param>
        /// <returns></returns>
        protected abstract Expression GetExpression(PropertyInfo propertyInfo, MemberExpression memberExpression, SearchEnterObj SearchData);

        /// <summary>
        /// 確認型別後加入不為null的判斷
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <param name="memberExpression"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected Expression CheckAddNotNull(PropertyInfo propertyInfo, MemberExpression memberExpression, Expression expression)
        {
            if (!propertyInfo.PropertyType.IsValueType
                || propertyInfo.PropertyType.GenericTypeArguments.Length != 0
               )
            {
                expression = Expression.AndAlso(memberExpression.IsNotNull(), expression);
            }
            return expression;
        }
        /// <summary>
        /// 取得實值Lambda string或數值的資料回傳
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="type"></param>
        /// <param name="searchData"></param>
        /// <returns></returns>
        protected Expression GetTypeToStrings(Expression expression, Type type, SearchEnterObj searchData)
        {
            List<Expression> expressions = new List<Expression>();
            foreach (var searchString in searchData.Search.SearchData)
            {
                var exp = searchString.IsVague
                        ? expression.ToStringEx(type).ToLowerEx().Contains(searchString.Value.ToLower())
                        : expression.ToStringEx(type).Equal(searchString.Value);
                expressions.Add(exp);
            }
            var ans = searchData.IsAnd
                    ? expressions.ComposeAnd()
                    : expressions.ComposeOrElse();
            return ans;
        }

    }
    /// <summary>
    /// 處理數值型的Lambda
    /// </summary>
    internal class ValueTypeOrStringSelectType : TypeToLambdaService
    {
        protected override Expression GetExpression(PropertyInfo propertyInfo, MemberExpression memberExpression, SearchEnterObj searchData)
        {
            return GetTypeToStrings(memberExpression, propertyInfo.PropertyType, searchData);
        }
    }
    /// <summary>
    /// 處理類別型的Lambda
    /// </summary>
    internal class ClassSelectType : TypeToLambdaService
    {
        protected override Expression GetExpression(PropertyInfo propertyInfo, MemberExpression memberExpression, SearchEnterObj SearchData)
        {
            var copydata = SearchData.DeepCopy();
            copydata.InType.Member = memberExpression;
            copydata.InType.ThisType = propertyInfo.PropertyType;
            copydata.Recursive.UpThisLayer();
            copydata.Search = SearchData.Search;
            var result = LambdaFlowSelecter.Get(copydata);
            return result;
        }
    }
    /// <summary>
    /// 處理可列舉數值的Lambda
    /// </summary>
    internal class IEnumOfValueType : TypeToLambdaService
    {
        protected override Expression GetExpression(PropertyInfo propertyInfo, MemberExpression memberExpression, SearchEnterObj SearchData)
        {
            var enumOfType = propertyInfo.PropertyType.GetIEnumofClass();
            var theEntity = Expression.Parameter(enumOfType, $"o{SearchData.Recursive.ThisLayer + 1}");
            var ans = GetTypeToStrings(theEntity, enumOfType, SearchData);
            var lambda = Expression.Lambda(ans, theEntity);
            var result = memberExpression.AnyEx(enumOfType, (LambdaExpression)lambda);
            return result;
        }
    }
    /// <summary>
    /// 處理可列舉型別的Lambda
    /// </summary>
    internal class IEnumOfClassType : TypeToLambdaService
    {
        protected override Expression GetExpression(PropertyInfo propertyInfo, MemberExpression memberExpression, SearchEnterObj SearchData)
        {
            var enumOfType = propertyInfo.PropertyType.GetIEnumofClass();
            var copydata = SearchData.DeepCopy();
            copydata.InType.ThisType = enumOfType;
            copydata.Recursive.UpThisLayer();
            copydata.Search = SearchData.Search;
            var lambda = LambdaFlowSelecter.Get(copydata);
            var result = memberExpression.AnyEx(enumOfType, (LambdaExpression)lambda);
            return result;
        }
    }

}
