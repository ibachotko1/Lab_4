using LogicTool.Business.Interfaces;
using LogicTool.Core.Enums;
using LogicTool.Core.Exceptions;
using LogicTool.Core.Models;
using LogicTool.Core.Services;
using System;

namespace LogicTool.Business.Services
{
    /// <summary>
    /// Реализация сервиса для работы с булевыми функциями.
    /// </summary>
    public class BooleanFunctionService : IBooleanFunctionService
    {
        private readonly FormulaParser _parser;

        /// <summary>
        /// Инициализирует новый экземпляр сервиса булевых функций.
        /// </summary>
        public BooleanFunctionService()
        {
            _parser = new FormulaParser();
        }

        /// <summary>
        /// Создает булеву функцию по номеру.
        /// </summary>
        /// <param name="variableCount">Количество переменных</param>
        /// <param name="functionNumber">Номер функции</param>
        /// <returns>Созданная булева функция</returns>
        /// <exception cref="System.ArgumentException">Выбрасывается при недопустимых параметрах</exception>
        public BooleanFunction CreateFromNumber(int variableCount, long functionNumber)
        {
            ValidateVariableCount(variableCount);
            return BooleanFunction.FromNumber(variableCount, functionNumber);
        }

        /// <summary>
        /// Создает булеву функцию по формуле.
        /// </summary>
        /// <param name="formula">Логическая формула</param>
        /// <returns>Созданная булева функция</returns>
        /// <exception cref="System.ArgumentException">Выбрасывается при пустой или некорректной формуле</exception>
        public BooleanFunction CreateFromFormula(string formula)
        {
            if (string.IsNullOrWhiteSpace(formula))
            {
                throw new ArgumentException("Формула не может быть пустой или содержать только пробелы.");
            }

            return BooleanFunction.FromFormula(formula);
        }

        /// <summary>
        /// Вычисляет метрики сложности формулы.
        /// </summary>
        /// <param name="formula">Логическая формула</param>
        /// <returns>Метрики сложности формулы</returns>
        public FormulaMetrics CalculateMetrics(string formula)
        {
            var function = CreateFromFormula(formula);
            return function.CalculateMetrics();
        }

        /// <summary>
        /// Проверяет корректность формулы.
        /// </summary>
        /// <param name="formula">Формула для проверки</param>
        /// <returns>Результат парсинга с информацией об ошибках</returns>
        public ParsingResult ValidateFormula(string formula)
        {
            try
            {
                var tokens = _parser.TokenizeWithTypes(formula);
                var rpn = _parser.ToRPN(tokens.ConvertAll(t => t.Value));
                return ParsingResult.Success(formula, tokens, rpn);
            }
            catch (FormulaParseException ex)
            {
                return ParsingResult.Error(formula, ex.Message, ErrorSeverity.Error);
            }
            catch (Exception ex)
            {
                return ParsingResult.Error(formula, $"Неожиданная ошибка: {ex.Message}", ErrorSeverity.Critical);
            }
        }

        /// <summary>
        /// Проверяет корректность количества переменных.
        /// </summary>
        /// <param name="variableCount">Количество переменных для проверки</param>
        /// <exception cref="System.ArgumentException">Выбрасывается при недопустимом количестве переменных</exception>
        private void ValidateVariableCount(int variableCount)
        {
            if (variableCount < 1)
            {
                throw new ArgumentException("Количество переменных должно быть не менее 1.");
            }

            if (variableCount > 10)
            {
                throw new ArgumentException(
                    $"Количество переменных ({variableCount}) слишком велико. " +
                    "Рекомендуется использовать не более 10 переменных для обеспечения производительности.");
            }
        }
    }
}