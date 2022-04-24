using SearchListUtils.Models.Eunms;

namespace SearchListUtils.Utils.LambdaExtensions
{
    public static class LinqExtension
    {
        /// <summary>
        /// 是否有資料
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool HasValues<T>(this IEnumerable<T> source)
        {
            return source != null && source.Count() > 0;
        }
        /// <summary>
        /// 判斷此型別是String或一般數值
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsValueTypeOrString(this Type type)
        {
            if (type != null)
            {
                return type.IsValueType || type == typeof(string);
            }
            return false;
        }

        public static TypeClassification CheckThisType(this Type type)
        {
            if (type.IsValueType)
            {
                return TypeClassification.TypeValue;
            }
            else if (type == typeof(string))
            {
                return TypeClassification.TypeString;
            }
            else if (type.GenericTypeArguments.Length == 0
                 && !type.IsArray
                 && type.IsClass)
            {
                return TypeClassification.TypeClass;
            }
            else
            {
                var enumofType = type.GetIEnumofClass();
                if (enumofType is null)
                {
                    return TypeClassification.TypeIsNotFind;
                }
                if (enumofType.IsValueTypeOrString())
                {
                    return TypeClassification.TypeIEnumValue;
                }
                else
                {
                    return TypeClassification.TypeIEnumClass;
                }
            }
        }
        public static Type GetIEnumofClass(this Type type)
        {
            var enumofType = type.GenericTypeArguments.FirstOrDefault()
                          ?? type.GetElementType();
            return enumofType;
        }
    }
}
