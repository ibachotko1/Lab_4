using LogicTool.Core.Enums;

namespace LogicTool.Core.Models
{
    /// <summary>
    /// Результат сравнения двух булевых функций
    /// </summary>
    public class ComparisonResult
    {
        /// <summary>
        /// Тип результата сравнения
        /// </summary>
        public ComparisonResultType ResultType { get; }

        /// <summary>
        /// Контрпример (набор переменных, на котором функции различаются)
        /// </summary>
        public string CounterExample { get; }

        /// <summary>
        /// Сообщение с описанием результата
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Флаг эквивалентности функций
        /// </summary>
        public bool AreEquivalent
        {
            get { return ResultType == ComparisonResultType.Equivalent; }
        }

        /// <summary>
        /// Создает новый результат сравнения
        /// </summary>
        /// <param name="resultType">Тип результата</param>
        /// <param name="counterExample">Контрпример</param>
        /// <param name="message">Сообщение</param>
        public ComparisonResult(ComparisonResultType resultType, string counterExample = "", string message = "")
        {
            ResultType = resultType;
            CounterExample = counterExample ?? "";
            Message = message ?? "";
        }

        /// <summary>
        /// Создает результат для эквивалентных функций
        /// </summary>
        /// <returns>Результат сравнения</returns>
        public static ComparisonResult Equivalent()
        {
            return new ComparisonResult(
                ComparisonResultType.Equivalent,
                "",
                "Функции эквивалентны");
        }

        /// <summary>
        /// Создает результат для неэквивалентных функций
        /// </summary>
        /// <param name="counterExample">Контрпример</param>
        /// <returns>Результат сравнения</returns>
        public static ComparisonResult NotEquivalent(string counterExample)
        {
            return new ComparisonResult(
                ComparisonResultType.NotEquivalent,
                counterExample,
                "Функции не эквивалентны");
        }

        /// <summary>
        /// Создает результат с ошибкой сравнения
        /// </summary>
        /// <param name="errorMessage">Сообщение об ошибке</param>
        /// <returns>Результат сравнения</returns>
        public static ComparisonResult Error(string errorMessage)
        {
            return new ComparisonResult(
                ComparisonResultType.Error,
                "",
                errorMessage ?? "Произошла ошибка при сравнении функций");
        }

        /// <summary>
        /// Возвращает строковое представление результата
        /// </summary>
        /// <returns>Строковое представление</returns>
        public override string ToString()
        {
            return Message + (string.IsNullOrEmpty(CounterExample) ? "" : $". Контрпример: {CounterExample}");
        }
    }
}