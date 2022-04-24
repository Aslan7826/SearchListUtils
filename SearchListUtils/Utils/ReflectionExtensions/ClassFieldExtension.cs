using System.Reflection;
using System.Text.RegularExpressions;

namespace SearchListUtils.Utils.ReflectionExtensions
{
    /// <summary>
    /// 反射物件
    /// </summary>
    public static class ClassFieldExtension
    {
        /// <summary>
        /// 取得這個Type所有欄位資訊
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<PropertyInfo> GetThisFieldProperties(this Type type)
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase).AsEnumerable();
        }
        /// <summary>
        /// 取得這個Type指定的欄位資訊
        /// </summary>
        /// <param name="type"></param>
        /// <param name="fileName">指定欄位</param>
        /// <returns></returns>
        public static PropertyInfo GetThisFieldProperty(this Type type, string fileName)
        {
            return type.GetProperty(fileName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        }
        /// <summary>
        /// 尋找IQueryable在SQL時使用的查詢名稱
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static List<string> GetSourceSQLHasField<T>(this IEnumerable<T> source)
        {
            var sql = source.AsQueryable().Expression.ToString();
            Regex regex = new Regex("^[A-Za-z]+$");
            var seachFile = sql.Split(',', '=', ' ').Distinct().Where(o => regex.IsMatch(o)).ToList();
            return seachFile;
        }

    }
}
