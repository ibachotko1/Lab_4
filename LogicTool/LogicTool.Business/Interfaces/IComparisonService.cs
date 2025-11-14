using LogicTool.Core.Models;

namespace LogicTool.Business.Interfaces
{
    /// <summary>
    /// Сервис для сравнения булевых функций.
    /// </summary>
    public interface IComparisonService
    {
        /// <summary>
        /// Сравнивает две булевы функции.
        /// </summary>
        /// <param name="func1">Первая функция</param>
        /// <param name="func2">Вторая функция</param>
        /// <returns>Результат сравнения</returns>
        ComparisonResult CompareFunctions(BooleanFunction func1, BooleanFunction func2);

        /// <summary>
        /// Сравнивает две функции, заданные формулами.
        /// </summary>
        /// <param name="formula1">Формула первой функции</param>
        /// <param name="formula2">Формула второй функции</param>
        /// <returns>Результат сравнения</returns>
        ComparisonResult CompareByFormula(string formula1, string formula2);

        /// <summary>
        /// Сравнивает функцию по номеру и функцию по формуле.
        /// </summary>
        /// <param name="varCount">Количество переменных</param>
        /// <param name="number">Номер функции</param>
        /// <param name="formula">Формула функции</param>
        /// <returns>Результат сравнения</returns>
        ComparisonResult CompareByNumberAndFormula(int varCount, long number, string formula);
    }
}