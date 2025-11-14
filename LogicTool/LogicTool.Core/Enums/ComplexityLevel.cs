namespace LogicTool.Core.Enums
{
    /// <summary>
    /// Уровни вычислительной сложности для булевых функций
    /// </summary>
    public enum ComplexityLevel
    {
        /// <summary>
        /// Низкая сложность (1-4 переменные)
        /// </summary>
        Low,

        /// <summary>
        /// Средняя сложность (5-6 переменных)
        /// </summary>
        Medium,

        /// <summary>
        /// Высокая сложность (7-8 переменных)
        /// </summary>
        High,

        /// <summary>
        /// Очень высокая сложность (9-11 переменных)
        /// </summary>
        VeryHigh,

        /// <summary>
        /// Критическая сложность (12+ переменных)
        /// </summary>
        Critical
    }
}