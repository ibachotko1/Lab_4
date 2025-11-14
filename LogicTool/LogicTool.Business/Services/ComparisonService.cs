using System;
using System.Collections.Generic;
using System.Linq;
using LogicTool.Business.Interfaces;
using LogicTool.Core.Models;
using LogicTool.Core.Services;
using LogicTool.Core.Exceptions;

namespace LogicTool.Business.Services
{
    /// <summary>
    /// Реализация сервиса для сравнения булевых функций.
    /// </summary>
    public class ComparisonService : IComparisonService
    {
        private readonly FormulaParser _parser;

        /// <summary>
        /// Инициализирует новый экземпляр сервиса сравнения.
        /// </summary>
        public ComparisonService()
        {
            _parser = new FormulaParser();
        }

        /// <summary>
        /// Сравнивает две булевы функции.
        /// </summary>
        /// <param name="func1">Первая функция</param>
        /// <param name="func2">Вторая функция</param>
        /// <returns>Результат сравнения</returns>
        public ComparisonResult CompareFunctions(BooleanFunction func1, BooleanFunction func2)
        {
            try
            {
                // Получаем все уникальные переменные из обеих функций
                var allVariables = GetAllVariables(func1, func2);

                // Проверяем сложность вычислений
                if (allVariables.Count > 8)
                {
                    return ComparisonResult.Error(
                        $"Слишком много переменных ({allVariables.Count}) для сравнения. " +
                        "Рекомендуется использовать не более 8 переменных.");
                }

                // Генерируем все возможные наборы значений переменных
                foreach (var testCase in GenerateAllTestCases(allVariables))
                {
                    bool result1 = EvaluateFunction(func1, testCase);
                    bool result2 = EvaluateFunction(func2, testCase);

                    if (result1 != result2)
                    {
                        // Нашли контрпример - функции различаются на этом наборе
                        string counterExample = FormatCounterExample(testCase);
                        return ComparisonResult.NotEquivalent(counterExample);
                    }
                }

                // Все наборы совпали - функции эквивалентны
                return ComparisonResult.Equivalent();
            }
            catch (Exception ex)
            {
                return ComparisonResult.Error($"Ошибка при сравнении функций: {ex.Message}");
            }
        }

        /// <summary>
        /// Сравнивает две функции, заданные формулами.
        /// </summary>
        /// <param name="formula1">Формула первой функции</param>
        /// <param name="formula2">Формула второй функции</param>
        /// <returns>Результат сравнения</returns>
        public ComparisonResult CompareByFormula(string formula1, string formula2)
        {
            try
            {
                var func1 = BooleanFunction.FromFormula(formula1);
                var func2 = BooleanFunction.FromFormula(formula2);
                return CompareFunctions(func1, func2);
            }
            catch (Exception ex)
            {
                return ComparisonResult.Error($"Ошибка при сравнении формул: {ex.Message}");
            }
        }

        /// <summary>
        /// Сравнивает функцию по номеру и функцию по формуле.
        /// </summary>
        /// <param name="varCount">Количество переменных</param>
        /// <param name="number">Номер функции</param>
        /// <param name="formula">Формула функции</param>
        /// <returns>Результат сравнения</returns>
        public ComparisonResult CompareByNumberAndFormula(int varCount, long number, string formula)
        {
            try
            {
                var func1 = BooleanFunction.FromNumber(varCount, number);
                var func2 = BooleanFunction.FromFormula(formula);
                return CompareFunctions(func1, func2);
            }
            catch (Exception ex)
            {
                return ComparisonResult.Error($"Ошибка при сравнении: {ex.Message}");
            }
        }

        /// <summary>
        /// Извлекает все уникальные переменные из двух функций.
        /// </summary>
        /// <param name="func1">Первая функция</param>
        /// <param name="func2">Вторая функция</param>
        /// <returns>Отсортированный список уникальных переменных</returns>
        private List<string> GetAllVariables(BooleanFunction func1, BooleanFunction func2)
        {
            var variables = new HashSet<string>();

            // Извлекаем переменные из таблиц истинности
            if (func1.TruthTable != null && func1.TruthTable.Count > 0)
            {
                foreach (var key in func1.TruthTable[0].Values.Keys)
                {
                    variables.Add(key);
                }
            }

            if (func2.TruthTable != null && func2.TruthTable.Count > 0)
            {
                foreach (var key in func2.TruthTable[0].Values.Keys)
                {
                    variables.Add(key);
                }
            }

            return variables.OrderBy(v => v).ToList();
        }

        /// <summary>
        /// Генерирует все возможные наборы значений для заданного списка переменных.
        /// </summary>
        /// <param name="variables">Список переменных</param>
        /// <returns>Перечисление словарей (наборов) значений переменных</returns>
        /// <remarks>
        /// Количество наборов равно 2^n, где n - количество переменных.
        /// Каждый набор представлен словарем [имя переменной] -> [значение].
        /// </remarks>
        private IEnumerable<Dictionary<string, bool>> GenerateAllTestCases(List<string> variables)
        {
            int count = variables.Count;
            int totalCases = 1 << count; // 2^count

            for (int i = 0; i < totalCases; i++)
            {
                var testCase = new Dictionary<string, bool>();
                for (int j = 0; j < count; j++)
                {
                    // Устанавливаем значение переменной на основе бита в i
                    // Старшие биты соответствуют первым переменным в списке
                    testCase[variables[j]] = ((i >> (count - 1 - j)) & 1) == 1;
                }
                yield return testCase;
            }
        }

        /// <summary>
        /// Вычисляет значение функции для заданного набора переменных.
        /// </summary>
        /// <param name="function">Булева функция</param>
        /// <param name="testCase">Набор значений переменных</param>
        /// <returns>Значение функции</returns>
        /// <exception cref="System.InvalidOperationException">Выбрасывается при невозможности вычислить функцию</exception>
        private bool EvaluateFunction(BooleanFunction function, Dictionary<string, bool> testCase)
        {
            // Если функция была создана по формуле, используем парсер для вычисления
            if (!string.IsNullOrEmpty(function.OriginalFormula))
            {
                try
                {
                    var tokens = _parser.Tokenize(function.OriginalFormula);
                    var rpn = _parser.ToRPN(tokens);
                    return _parser.EvaluateRPN(rpn, testCase);
                }
                catch (FormulaParseException ex)
                {
                    throw new InvalidOperationException($"Не удалось вычислить формулу: {ex.Message}");
                }
            }
            else
            {
                // Ищем соответствующую строку в таблице истинности
                foreach (var row in function.TruthTable)
                {
                    if (row.Matches(testCase))
                    {
                        return row.Result;
                    }
                }

                // Если не нашли, значит набор переменных не соответствует функции
                throw new InvalidOperationException(
                    $"Не удалось найти соответствующую строку в таблице истинности для набора: {FormatCounterExample(testCase)}");
            }
        }

        /// <summary>
        /// Форматирует набор переменных в строку для отображения.
        /// </summary>
        /// <param name="testCase">Набор переменных</param>
        /// <returns>Строковое представление набора</returns>
        private string FormatCounterExample(Dictionary<string, bool> testCase)
        {
            return string.Join(", ", testCase.Select(kvp => $"{kvp.Key} = {kvp.Value}"));
        }
    }
}