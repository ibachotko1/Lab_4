using LogicTool.Core.Enums;

namespace LogicTool.Core.Models
{
    /// <summary>
    /// Представляет токен при лексическом анализе логического выражения
    /// </summary>
    public class Token
    {
        /// <summary>
        /// Значение токена
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Тип токена
        /// </summary>
        public TokenType Type { get; }

        /// <summary>
        /// Позиция токена в исходной строке
        /// </summary>
        public int Position { get; }

        /// <summary>
        /// Создает новый экземпляр токена
        /// </summary>
        /// <param name="value">Значение токена</param>
        /// <param name="type">Тип токена</param>
        /// <param name="position">Позиция в строке</param>
        public Token(string value, TokenType type, int position)
        {
            Value = value;
            Type = type;
            Position = position;
        }

        /// <summary>
        /// Возвращает строковое представление токена
        /// </summary>
        /// <returns>Строковое представление</returns>
        public override string ToString()
        {
            return $"{Value} ({Type}) at {Position}";
        }
    }
}