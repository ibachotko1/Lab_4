using System;

namespace LogicTool.Core.Exceptions
{
    /// <summary>
    /// Исключение, возникающее при ошибках генерации булевых функций.
    /// </summary>
    public class FunctionGenerationException : Exception
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса FunctionGenerationException.
        /// </summary>
        public FunctionGenerationException()
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса FunctionGenerationException с указанным сообщением.
        /// </summary>
        /// <param name="message">Сообщение об ошибке</param>
        public FunctionGenerationException(string message) : base(message)
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса FunctionGenerationException с указанным сообщением и внутренним исключением.
        /// </summary>
        /// <param name="message">Сообщение об ошибке</param>
        /// <param name="innerException">Внутреннее исключение</param>
        public FunctionGenerationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}