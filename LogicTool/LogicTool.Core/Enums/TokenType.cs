namespace LogicTool.Core.Enums
{
    /// <summary>
    /// Типы токенов при парсинге логических выражений
    /// </summary>
    public enum TokenType
    {
        /// <summary>
        /// Логическая переменная (x1, x2, a, b)
        /// </summary>
        Variable,

        /// <summary>
        /// Логический оператор (∧, ∨, ¬, →, ↔)
        /// </summary>
        Operator,

        /// <summary>
        /// Логическая константа (0 - ложь, 1 - истина)
        /// </summary>
        Constant,

        /// <summary>
        /// Левая круглая скобка
        /// </summary>
        LeftParenthesis,

        /// <summary>
        /// Правая круглая скобка
        /// </summary>
        RightParenthesis
    }
}