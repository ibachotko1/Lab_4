using System;

namespace LogicTool.Core.Exceptions
{
    /// <summary>
    /// Исключение, возникающее при ошибках парсинга логических формул.
    /// </summary>
    public class FormulaParseException : Exception
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса FormulaParseException.
        /// </summary>
        public FormulaParseException()
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса FormulaParseException с указанным сообщением.
        /// </summary>
        /// <param name="message">Сообщение об ошибке</param>
        public FormulaParseException(string message) : base(message)
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса FormulaParseException с указанным сообщением и внутренним исключением.
        /// </summary>
        /// <param name="message">Сообщение об ошибке</param>
        /// <param name="innerException">Внутреннее исключение</param>
        public FormulaParseException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}