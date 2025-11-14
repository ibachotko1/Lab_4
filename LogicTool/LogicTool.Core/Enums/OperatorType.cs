namespace LogicTool.Core.Enums
{
    /// <summary>
    /// Типы логических операторов
    /// </summary>
    public enum OperatorType
    {
        /// <summary>
        /// Унарный оператор (один операнд)
        /// </summary>
        Unary,

        /// <summary>
        /// Бинарный оператор (два операнда)  
        /// </summary>
        Binary,

        /// <summary>
        /// Специальный оператор (скобки и т.д.)
        /// </summary>
        Special
    }
}