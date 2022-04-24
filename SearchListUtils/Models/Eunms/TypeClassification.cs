namespace SearchListUtils.Models.Eunms
{
    /// <summary>
    /// 類型集合
    /// </summary>
    public enum TypeClassification
    {
        /// <summary>
        /// 包太多層的型別
        /// </summary>
        TypeIsNotFind,

        /// <summary>
        /// 字串類型
        /// </summary>
        TypeString,
        /// <summary>
        /// 數值類型
        /// </summary>
        TypeValue,
        /// <summary>
        /// Class型別類型
        /// </summary>
        TypeClass,
        /// <summary>
        ///  可列舉數值
        /// </summary>
        TypeIEnumValue,
        /// <summary>
        /// 可列舉Class
        /// </summary>
        TypeIEnumClass
    }
}
