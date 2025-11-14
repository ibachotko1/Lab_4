namespace LogicTool.Core.Enums
{
    /// <summary>
    /// Результат сравнения двух булевых функций
    /// </summary>
    public enum ComparisonResultType
    {
        /// <summary>
        /// Функции эквивалентны
        /// </summary>
        Equivalent,

        /// <summary>
        /// Функции не эквивалентны
        /// </summary>
        NotEquivalent,

        /// <summary>
        /// Сравнение не удалось выполнить
        /// </summary>
        Error
    }
}