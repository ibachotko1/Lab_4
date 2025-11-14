using System.Collections.Generic;
using LogicTool.Core.Enums;

namespace LogicTool.Core.Models
{
    /// <summary>
    /// Результат парсинга логической формулы
    /// </summary>
    public class ParsingResult
    {
        /// <summary>
        /// Флаг успешного парсинга
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// Исходная формула
        /// </summary>
        public string Formula { get; }

        /// <summary>
        /// Список токенов (при успешном парсинге)
        /// </summary>
        public List<Token> Tokens { get; }

        /// <summary>
        /// Обратная польская запись (при успешном парсинге)
        /// </summary>
        public List<string> RPN { get; }

        /// <summary>
        /// Сообщение об ошибке (при неуспешном парсинге)
        /// </summary>
        public string ErrorMessage { get; }

        /// <summary>
        /// Уровень серьезности ошибки
        /// </summary>
        public ErrorSeverity ErrorSeverity { get; }

        /// <summary>
        /// Создает успешный результат парсинга
        /// </summary>
        /// <param name="formula">Исходная формула</param>
        /// <param name="tokens">Список токенов</param>
        /// <param name="rpn">Обратная польская запись</param>
        public ParsingResult(string formula, List<Token> tokens, List<string> rpn)
        {
            IsSuccess = true;
            Formula = formula;
            Tokens = tokens ?? new List<Token>();
            RPN = rpn ?? new List<string>();
            ErrorMessage = string.Empty;
            ErrorSeverity = ErrorSeverity.Info;
        }

        /// <summary>
        /// Создает неуспешный результат парсинга
        /// </summary>
        /// <param name="formula">Исходная формула</param>
        /// <param name="errorMessage">Сообщение об ошибке</param>
        /// <param name="severity">Уровень серьезности ошибки</param>
        public ParsingResult(string formula, string errorMessage, ErrorSeverity severity = ErrorSeverity.Error)
        {
            IsSuccess = false;
            Formula = formula;
            ErrorMessage = errorMessage ?? "Неизвестная ошибка";
            ErrorSeverity = severity;
            Tokens = new List<Token>();
            RPN = new List<string>();
        }

        /// <summary>
        /// Создает успешный результат парсинга
        /// </summary>
        /// <param name="formula">Исходная формула</param>
        /// <param name="tokens">Список токенов</param>
        /// <param name="rpn">Обратная польская запись</param>
        /// <returns>Успешный результат парсинга</returns>
        public static ParsingResult Success(string formula, List<Token> tokens, List<string> rpn)
        {
            return new ParsingResult(formula, tokens, rpn);
        }

        /// <summary>
        /// Создает неуспешный результат парсинга
        /// </summary>
        /// <param name="formula">Исходная формула</param>
        /// <param name="errorMessage">Сообщение об ошибке</param>
        /// <param name="severity">Уровень серьезности ошибки</param>
        /// <returns>Неуспешный результат парсинга</returns>
        public static ParsingResult Error(string formula, string errorMessage, ErrorSeverity severity = ErrorSeverity.Error)
        {
            return new ParsingResult(formula, errorMessage, severity);
        }

        /// <summary>
        /// Возвращает строковое представление результата
        /// </summary>
        /// <returns>Строковое представление</returns>
        public override string ToString()
        {
            if (IsSuccess)
            {
                return $"Успешный парсинг: {Formula} -> {string.Join(" ", RPN)}";
            }
            else
            {
                return $"Ошибка парсинга: {ErrorMessage}";
            }
        }
    }
}