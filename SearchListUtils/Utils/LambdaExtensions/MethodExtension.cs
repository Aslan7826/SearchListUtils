using System.Linq.Expressions;
using System.Reflection;

namespace SearchListUtils.Utils.LambdaExtensions
{
    public static class MethodExtension
    {
        private static MethodInfo _ContainsMethod;
        private static MethodInfo MethodContains
        {
            get
            {
                if (_ContainsMethod is null)
                {
                    _ContainsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                }
                return _ContainsMethod;
            }
        }
        private static MethodInfo _AnyMethod;
        private static MethodInfo MethodAny
        {
            get
            {
                if (_AnyMethod is null)
                {
                    _AnyMethod = typeof(Enumerable).GetMethods().Single(m => m.Name == "Any" && m.GetParameters().Length == 2);
                }
                return _AnyMethod;
            }
        }
        private static MethodInfo _ToLowerMethod;
        private static MethodInfo MethodToLower
        {
            get
            {
                if (_ToLowerMethod is null)
                {
                    _ToLowerMethod = typeof(string).GetMethod("ToLower", new Type[] { });
                }
                return _ToLowerMethod;
            }
        }
        private static MethodInfo _ToStringMethod;
        private static MethodInfo MethodToString
        {
            get
            {
                if (_ToStringMethod is null)
                {
                    _ToStringMethod = typeof(object).GetMethod("ToString");
                }
                return _ToStringMethod;
            }
        }
        /// <summary>
        /// member.Any((type)o=>lambda)
        /// </summary>
        /// <param name="member"></param>
        /// <param name="type"></param>
        /// <param name="lambda"></param>
        /// <returns></returns>
        public static Expression AnyEx(this MemberExpression member, Type type, LambdaExpression lambda)
        {
            if (member is null || lambda is null)
            {
                return null;
            }
            var method = MethodAny.MakeGenericMethod(type);
            var result = Expression.Call(method, member, lambda);
            return result;
        }
        /// <summary>
        /// member.ToString()
        /// </summary>
        /// <param name="type"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        public static Expression ToStringEx(this Expression member, Type type)
        {
            if (member is null)
            {
                return null;
            }
            if (type != typeof(string))
            {
                member = Expression.Call(member, MethodToString);
            }
            return member;
        }
        /// <summary>
        /// member != null
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public static Expression IsNotNull(this MemberExpression member)
        {
            if (member is null)
            {
                return null;
            }
            var result = Expression.NotEqual(member, Expression.Constant(null));
            return result;
        }
        /// <summary>
        /// 將堆疊的判斷資料使用Or並在一起
        /// </summary>
        /// <param name="exs"></param>
        /// <returns></returns>
        public static Expression ComposeOrElse(this List<Expression> exs) => Compose(exs, false);
        /// <summary>
        /// 將堆疊的判斷資料使用And並在一起
        /// </summary>
        /// <param name="exs"></param>
        /// <returns></returns>
        public static Expression ComposeAnd(this List<Expression> exs) => Compose(exs, true);
        /// <summary>
        /// 將資料堆疊使用And或Or
        /// </summary>
        /// <param name="exs"></param>
        /// <param name="isAnd"></param>
        /// <returns></returns>
        private static Expression Compose(List<Expression> exs, bool isAnd)
        {
            if (exs.Count == 0)
            {
                return null;
            }
            var expression = exs[0];
            for (var i = 1; i < exs.Count; i++)
            {
                expression = isAnd
                           ? Expression.And(expression, exs[i])
                           : Expression.OrElse(expression, exs[i]);

            }
            return expression;
        }
        /// <summary>
        ///  member.ToLower()
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public static Expression ToLowerEx(this Expression member)
        {
            return Expression.Call(member, MethodToLower);
        }
        /// <summary>
        /// Searchs 轉換 contains
        /// </summary>
        /// <param name="searchs"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        public static Expression Contains(this Expression member, List<string> searchs)
        {
            if (member is null)
            {
                return null;
            }
            var ans = searchs.Select(qt =>
                Expression.Call(member, MethodContains, Expression.Constant(qt, typeof(string)))
            ).ToList<Expression>();
            var result = ans.ComposeOrElse();
            return result;
        }

        /// <summary>
        /// Search 轉換 contains
        /// </summary>
        /// <param name="member"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        public static Expression Contains(this Expression member, string search)
        {
            if (member is null)
            {
                return null;
            }
            var result = Expression.Call(member, MethodContains, Expression.Constant(search, typeof(string)));
            return result;
        }
        /// <summary>
        /// Search 轉換 equal
        /// </summary>
        /// <param name="searchs"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        public static Expression Equal(this Expression member, List<string> searchs)
        {
            if (member is null)
            {
                return null;
            }
            var ans = searchs.Select(qt =>
                Expression.Equal(member, Expression.Constant(qt, typeof(string)))
            ).ToList<Expression>();
            var result = ans.ComposeAnd();
            return result;
        }
        /// <summary>
        /// Search 轉換 equal
        /// </summary>
        /// <param name="member"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        public static Expression Equal(this Expression member, string search)
        {
            if (member is null)
            {
                return null;
            }
            var result = Expression.Equal(member, Expression.Constant(search, typeof(string)));
            return result;
        }

    }
}
