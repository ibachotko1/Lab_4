using LogicTool.Core.Models;

namespace LogicTool.Business.Interfaces
{
    /// <summary>
    /// Сервис для работы с булевыми функциями.
    /// </summary>
    public interface IBooleanFunctionService
    {
        /// <summary>
        /// Создает булеву функцию по номеру.
        /// </summary>
        /// <param name="variableCount">Количество переменных</param>
        /// <param name="functionNumber">Номер функции</param>
        /// <returns>Созданная булева функция</returns>
        /// <exception cref="System.ArgumentException">Выбрасывается при недопустимых параметрах</exception>
        BooleanFunction CreateFromNumber(int variableCount, long functionNumber);

        /// <summary>
        /// Создает булеву функцию по формуле.
        /// </summary>
        /// <param name="formula">Логическая формула</param>
        /// <returns>Созданная булева функция</returns>
        /// <exception cref="System.ArgumentException">Выбрасывается при пустой или некорректной формуле</exception>
        BooleanFunction CreateFromFormula(string formula);

        /// <summary>
        /// Вычисляет метрики сложности формулы.
        /// </summary>
        /// <param name="formula">Логическая формула</param>
        /// <returns>Метрики сложности формулы</returns>
        FormulaMetrics CalculateMetrics(string formula);

        /// <summary>
        /// Проверяет корректность формулы.
        /// </summary>
        /// <param name="formula">Формула для проверки</param>
        /// <returns>Результат парсинга с информацией об ошибках</returns>
        ParsingResult ValidateFormula(string formula);
    }
}