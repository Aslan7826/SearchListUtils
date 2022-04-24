using SearchListUtils.Models.Interfaces;
using SearchListUtils.Models.Lambdas;
using SearchListUtils.Utils.ReflectionExtensions;
using System.Linq.Expressions;
using System.Reflection;

namespace SearchListUtils.Utils.LambdaExtensions
{
    public class LambdaFlowSelecter
    {
        public static Expression Get(SearchEnterObj searchData)
        {
            if (searchData.IsListStringSearch)
            {
                return new StringsSearch().FlowGet(searchData);
            }
            else
            {
                return new ClassSearch().FlowGet(searchData);
            }
        }
    }

    internal abstract class LambdaExpressionFlow
    {
        internal Expression FlowGet(SearchEnterObj searchData)
        {
            if (searchData.Recursive.MaxLayer < searchData.Recursive.ThisLayer)
            {
                return null;
            }
            var needField = GetPropertyInfos(searchData.InType, searchData.Search);
            //要遞迴的資料
            List<Expression> AllExpressionCalls = new();
            Expression entity = (Expression)searchData.InType.Member
                              ?? Expression.Parameter(searchData.InType.ThisType, $"o{searchData.Recursive.ThisLayer}");
            foreach (var property in needField)
            {
                var expression = GetExpression(property, entity, searchData);
                if (expression != null)
                {
                    AllExpressionCalls.Add(expression);
                }
            }
            //總結果的加總
            if (AllExpressionCalls.Count > 0)
            {
                var ans = searchData.IsAnd
                        ? AllExpressionCalls.ComposeAnd()
                        : AllExpressionCalls.ComposeOrElse();
                return searchData.InType.Member is null
                       ? Expression.Lambda(ans, (ParameterExpression)entity)
                       : ans;
            }
            return null;
        }
        /// <summary>
        /// 所需欄位的檢查
        /// </summary>
        /// <param name="inType"></param>
        /// <param name="searchField"></param>
        /// <returns></returns>
        protected abstract List<PropertyInfo> GetPropertyInfos(InTypeObj inType, SearchFieldObj searchField);
        /// <summary>
        /// 型別轉換
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <param name="entity"></param>
        /// <param name="searchData"></param>
        /// <returns></returns>
        protected abstract Expression GetExpression(PropertyInfo propertyInfo, Expression entity, SearchEnterObj searchData);

    }
    internal class StringsSearch : LambdaExpressionFlow
    {
        protected override Expression GetExpression(PropertyInfo propertyInfo, Expression entity, SearchEnterObj searchData)
        {
            return new TypeToLambdaManagerService().GetExpression(propertyInfo, entity, searchData);
        }
        protected override List<PropertyInfo> GetPropertyInfos(InTypeObj inType, SearchFieldObj searchField)
        {
            if (inType.ThisType is null)
                return new List<PropertyInfo>();
            var result = inType.ThisType.GetThisFieldProperties();
            if (searchField.SQLField.HasValues())
            {
                // result = result.Where(o => searchField.SQLField.Contains(o.Name));
            }
            if (searchField.NoSearchField.HasValues())
            {
                result = result.Where(o => !searchField.NoSearchField.Contains(o.Name));
            }
            return result.ToList();
        }
    }

    internal class ClassSearch : LambdaExpressionFlow
    {
        protected override Expression GetExpression(PropertyInfo propertyInfo, Expression entity, SearchEnterObj searchData)
        {
            var data = searchData.Search.SearchData.Cast<SearchClassObj>();
            var has = data.Where(o => o.SelectName == propertyInfo.Name && o.SelectFilterType == propertyInfo.PropertyType)
                      .ToList();
            if (has.Count > 0)
            {
                var son = searchData.DeepCopy();
                son.Search = new SearchFieldObj() { SearchData = has.Cast<ISearchStringObj>().ToList() };
                var expression = new TypeToLambdaManagerService().GetExpression(propertyInfo, entity, son);
                return expression;
            };
            return null;
        }
        protected override List<PropertyInfo> GetPropertyInfos(InTypeObj inType, SearchFieldObj searchClassField)
        {
            if (inType.ThisType is null)
                return new List<PropertyInfo>();
            var result = inType.ThisType.GetThisFieldProperties();
            if (searchClassField.SQLField.HasValues())
            {
                // result = result.Where(o => searchClassField.SQLField.Contains(o.Name));
            }
            if (searchClassField.SearchData.HasValues())
            {
                var data = searchClassField.SearchData.Cast<SearchClassObj>();
                var needSearchField = data.Select(o => o.SelectName);
                result = result.Where(o => needSearchField.Contains(o.Name));
            }
            if (searchClassField.NoSearchField.HasValues())
            {
                result = result.Where(o => !searchClassField.NoSearchField.Contains(o.Name));
            }
            return result.ToList();
        }
    }
}
