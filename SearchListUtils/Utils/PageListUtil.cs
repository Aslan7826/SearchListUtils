using Microsoft.EntityFrameworkCore;
using SearchListUtils.Models.Eunms;
using SearchListUtils.Models.Interfaces;
using SearchListUtils.Models.Lambdas;
using SearchListUtils.Models.ListSearch;
using SearchListUtils.Utils.LambdaExtensions;
using SearchListUtils.Utils.ReflectionExtensions;
using System.Globalization;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace SearchListUtils.Utils
{
    public static class PageListUtil
    {

        /// <summary>
        /// 列表分頁
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">列表來源</param>
        /// <param name="inPageObj">分頁物件</param>
        /// <returns></returns>
        public static ResponseListPageResult ToPage<T>(this IEnumerable<T> source, ReqPageObj inPageObj)
        {
            var count = source.Count();
            var items = source.Skip((inPageObj.Page - 1) * inPageObj.Size).Take(inPageObj.Size);
            ResponseListPageResult result = new()
            {
                Data = items,
                Total = count,
                TotalPage = (int)Math.Ceiling(count / (double)inPageObj.Size),
                Page = inPageObj.Page
            };
            return result;
        }


        /// <summary>
        /// 列表分頁-只有EF取得資料的才能呼叫
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">列表來源</param>
        /// <param name="inPageObj">分頁物件</param>
        /// <returns></returns>
        public static async Task<ResponseListPageResult> ToPageAsync<T>(this IEnumerable<T> source, ReqPageObj inPageObj)
        {
            var count = await source.AsQueryable().CountAsync();
            var items = await source.AsQueryable().Skip((inPageObj.Page - 1) * inPageObj.Size).Take(inPageObj.Size).ToListAsync();
            ResponseListPageResult result = new()
            {
                Data = items,
                Total = count,
                TotalPage = (int)Math.Ceiling(count / (double)inPageObj.Size),
                Page = inPageObj.Page
            };
            return result;
        }
        /// <summary>
        /// 列表排序
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">列表來源</param>
        /// <param name="inOrderByObj">列表排序物件</param>
        /// <returns></returns>
        public static IEnumerable<T> ToOrderBy<T>(this IEnumerable<T> source, InOrderByObj inOrderByObj)
        {
            if (source != null && inOrderByObj != null && !string.IsNullOrWhiteSpace(inOrderByObj.OrderName))
            {
                Type entityType = typeof(T);
                var thisField = entityType.GetThisFieldProperty(inOrderByObj.OrderName);
                if (thisField is null || (thisField.PropertyType.IsGenericType && thisField.PropertyType.GetGenericTypeDefinition() == (typeof(IEnumerable<>))))
                {
                    return source;
                }
                //針對欄位不管傳入大小寫，
                var param = Expression.Parameter(entityType, "o");
                var member = Expression.Property(param, inOrderByObj.OrderName);
                var lambda = Expression.Lambda<Func<T, object>>(Expression.Convert(member, typeof(object)), param);
                //因為是泛型Mehtod要呼叫MakeGenericMethod決定泛型型別
                source = inOrderByObj.IsDesc
                       ? source.AsQueryable().OrderByDescending(lambda)
                       : source.AsQueryable().OrderBy(lambda);
            }
            return source;
        }

        /// <summary>
        /// 列表查詢-模糊搜尋
        /// </summary>
        /// <typeparam name="Tsource">查詢來源型別</typeparam>
        /// <param name="source">查詢來源</param>
        /// <param name="inSearchObj">要查詢的字串</param>
        /// <param name="isAnd">設定And模式還是Or模式 True:And,False:Or</param>
        /// <param name="noSearchFields">設定不查詢的資料欄位</param>
        /// <param name="recursiveMax">設定最大遞迴次數</param>
        /// <returns></returns>
        public static IEnumerable<Tsource> ToSearchBar<Tsource>(this IEnumerable<Tsource> source, InSearchObj inSearchObj, bool isAnd = false, List<string> noSearchFields = null, int recursiveMax = 1)
        {
            if (source != null && inSearchObj != null && !string.IsNullOrEmpty(inSearchObj.SegmentStringbyblank))
            {
                var seachFile = source.GetSourceSQLHasField();
                Regex regex = new Regex("^\"(.*)\"");
                var searchs = inSearchObj.SegmentStringbyblank
                              .ToLower()
                              .Split(inSearchObj.KeywordSeparator)
                              .Distinct()
                              .Select(o =>
                              {
                                  var ans = regex.IsMatch(o);
                                  if (ans)
                                  {
                                      o = o.TrimStart('"');
                                      o = o.TrimEnd('"');
                                  }
                                  return new SearchStringObj() { IsVague = !ans, Value = o };
                              })
                              .ToList();
                var sc = new SearchFieldObj()
                {
                    SQLField = seachFile,
                    NoSearchField = noSearchFields,
                    SearchData = searchs.Cast<ISearchStringObj>().ToList()
                };
                SearchEnterObj searchEnter = new(typeof(Tsource), isAnd, recursiveMax, true) { Search = sc };
                source = GetSearchLambda(source, searchEnter);
            }
            return source;
        }

        /// <summary>
        /// 列表查詢-精準搜尋
        /// </summary>
        /// <typeparam name="Tsource">查詢來源型別</typeparam>
        /// <typeparam name="Tsearch">比對用的資料型別</typeparam>
        /// <param name="source">查詢來源型別</param>
        /// <param name="search">比對用的資料</param>
        /// <param name="inSearchClassObj">比對用的查詢資料</param>
        /// <param name="isAnd">設定And模式還是Or模式 True:And,False:Or</param>
        /// <param name="noSearchFields">設定不查詢的資料欄位</param>
        /// <param name="recursiveMax">設定最大遞迴次數</param>
        /// <returns></returns>
        public static IEnumerable<Tsource> ToClassSearch<Tsource, Tsearch>(this IEnumerable<Tsource> source, Tsearch search, InSearchClassObj<Tsearch> inSearchClassObj, bool isAnd = false, List<string> noSearchFields = null, int recursiveMax = 1) where Tsearch : class
        {
            if (source != null && search != null)
            {
                var seachFile = source.GetSourceSQLHasField();
                var isQuery = source.GetType().Name.Contains("InternalDbSet");
                List<SearchClassObj> searchField = GetSearchField(search, isQuery);
                if (inSearchClassObj != null)
                {
                    searchField.AddRange(GetSearchField(inSearchClassObj.Vague, isQuery));
                    searchField.AddRange(GetSearchField(inSearchClassObj.Precise, isQuery, false));
                }
                var sc = new SearchFieldObj()
                {
                    SQLField = seachFile,
                    NoSearchField = noSearchFields,
                    SearchData = searchField.Cast<ISearchStringObj>().ToList()
                };
                SearchEnterObj searchEnter = new(typeof(Tsource), isAnd, recursiveMax, false) { Search = sc };
                source = GetSearchLambda(source, searchEnter);
            }
            return source;
        }
        private static IEnumerable<Tsource> GetSearchLambda<Tsource>(IEnumerable<Tsource> source, SearchEnterObj searchEnter)
        {
            var expression = LambdaFlowSelecter.Get(searchEnter);
            if (expression != null)
            {
                source = source.AsQueryable().Where(expression as Expression<Func<Tsource, bool>>);
            }

            return source;
        }
        /// <summary>
        /// 取得要查詢的物件中，真的有要查的資料
        /// </summary>
        /// <typeparam name="Tsearch"></typeparam>
        /// <param name="search"></param>
        /// <param name="isQuery"></param>
        /// <param name="isVague">是否為模糊搜尋,true為模糊,false為精準</param>
        /// <returns></returns>
        public static List<SearchClassObj> GetSearchField<Tsearch>(Tsearch search, bool isQuery, bool isVague = true)
        {
            if (search is null)
            {
                return new List<SearchClassObj>();
            }
            var datetimes = new List<Type>() { typeof(DateTime), typeof(DateTime?) };
            var result = new List<SearchClassObj>();
            foreach (var propert in typeof(Tsearch).GetThisFieldProperties())
            {
                var value = propert.GetValue(search);
                var check = false;
                if (propert.PropertyType.IsValueType)
                {
                    check = value != null && value.ToString() != Convert.ToString(Activator.CreateInstance(propert.PropertyType));
                }
                else if (propert.PropertyType == typeof(string))
                {
                    check = !string.IsNullOrEmpty(value?.ToString());
                }
                if (check)
                {
                    SearchClassObj data = new()
                    {
                        IsVague = isVague,
                        SelectFilterType = propert.PropertyType,
                        SelectName = propert.Name,
                        Value = value.ToString()
                    };
                    if (isQuery && datetimes.Contains(propert.PropertyType))   //轉DB模型的情況下 DateTime 格式要依照DB格式去做模糊搜尋
                    {
                        data.Value = Convert.ToDateTime(data.Value).ToString("MMM  d yyyy hh:mmtt", CultureInfo.CreateSpecificCulture("en-US"));
                    }
                    result.Add(data);
                }
            }
            return result;
        }

        /// <summary>
        /// 查詢時間區間的共用方法
        /// </summary>
        /// <typeparam name="Tsource">要查的型別</typeparam>
        /// <param name="source">要查的列表</param>
        /// <param name="timeFilter">查詢的時間欄位 o=>o.day</param>
        /// <param name="inSearchTimeObj">查詢物件</param>
        /// <returns></returns>

        public static IEnumerable<Tsource> ToDateSearch<Tsource>(this IEnumerable<Tsource> source, Expression<Func<Tsource, DateTime?>> timeFilter, InSearchTimeObj inSearchTimeObj)
        {
            var expression = (timeFilter.Body as MemberExpression).IsNotNull();
            var member = timeFilter.Parameters.Single();
            var lambda = Expression.Lambda(expression, member);
            source = source.AsQueryable().Where(lambda as Expression<Func<Tsource, bool>>);
            var exp = Expression.Property(timeFilter.Body, "Value");
            var filter = Expression.Lambda(exp, member) as Expression<Func<Tsource, DateTime>>;
            return ToDateSearch(source, filter, inSearchTimeObj);
        }
        /// <summary>
        /// 查詢時間區間的共用方法
        /// </summary>
        /// <typeparam name="Tsource">要查的型別</typeparam>
        /// <param name="source">要查的列表</param>
        /// <param name="timeFilter">查詢的時間欄位 o=>o.day</param>
        /// <param name="inSearchTimeObj">查詢物件</param>
        /// <returns></returns>
        public static IEnumerable<Tsource> ToDateSearch<Tsource>(this IEnumerable<Tsource> source, Expression<Func<Tsource, DateTime>> timeFilter, InSearchTimeObj inSearchTimeObj)
        {
            var check = new List<TimeperiodType>() { TimeperiodType.Day, TimeperiodType.Month, TimeperiodType.Year };
            if (inSearchTimeObj != null && inSearchTimeObj.StartTime.HasValue
               && check.Any(o => o == inSearchTimeObj.TimePeriodType))
            {
                var expressionlist = new List<Expression>();
                if (inSearchTimeObj.EndTime.HasValue)
                {
                    var theValue = Expression.Property(timeFilter.Body, "Date");
                    var startDayValue = typeof(DateTime).GetProperty("Date").GetValue(inSearchTimeObj.StartTime, null);
                    var ExpressionStartDay = Expression.Constant(startDayValue, typeof(DateTime));
                    var endDayValue = typeof(DateTime).GetProperty("Date").GetValue(inSearchTimeObj.EndTime, null);
                    var ExpressionEndDay = Expression.Constant(endDayValue, typeof(DateTime));
                    expressionlist.Add(Expression.GreaterThanOrEqual(theValue, ExpressionStartDay));
                    expressionlist.Add(Expression.LessThanOrEqual(theValue, ExpressionEndDay));
                }
                else
                {
                    var dictTimeSearch = new Dictionary<TimeperiodType, List<string>>()
                    {
                        {TimeperiodType.Day  ,new List<string>(){ TimeperiodType.Day.ToString()  ,TimeperiodType.Month.ToString(),TimeperiodType.Year.ToString()} },
                        {TimeperiodType.Month,new List<string>(){ TimeperiodType.Month.ToString(),TimeperiodType.Year.ToString()} },
                        {TimeperiodType.Year ,new List<string>(){ TimeperiodType.Year.ToString()} }
                    };
                    foreach (var value in dictTimeSearch[inSearchTimeObj.TimePeriodType])
                    {
                        var theValue = Expression.Property(timeFilter.Body, value);
                        var startDayValue = typeof(DateTime).GetProperty(value).GetValue(inSearchTimeObj.StartTime, null);
                        var ExpressionStartDay = Expression.Constant(startDayValue, typeof(int));
                        var ans = Expression.Equal(theValue, ExpressionStartDay);
                        expressionlist.Add(ans);
                    }
                }
                var member = timeFilter.Parameters.Single();
                var lambda = Expression.Lambda(expressionlist.ComposeAnd(), member);
                source = source.AsQueryable().Where(lambda as Expression<Func<Tsource, bool>>);
            }
            return source;
        }

        /// <summary>
        /// 文字分割工具,不包含空值與重複
        /// </summary>
        public static List<string> SplitArray(this string listStr, char separator)
        {
            List<string> result = new List<string>();
            if (!string.IsNullOrEmpty(listStr))
            {
                result = listStr.Split(separator).Distinct().Where(o => !string.IsNullOrWhiteSpace(o)).ToList();
            }
            return result;
        }
    }
}
