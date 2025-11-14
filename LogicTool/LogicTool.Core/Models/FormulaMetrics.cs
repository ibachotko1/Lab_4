using System.Linq;

namespace LogicTool.Core.Models
{
    /// <summary>
    /// Представляет метрики сложности логической формулы
    /// </summary>
    public class FormulaMetrics
    {
        /// <summary>
        /// Количество литералов (переменных и их отрицаний) в формуле
        /// </summary>
        public int LiteralCount { get; }

        /// <summary>
        /// Количество конъюнкций (логических И) в формуле
        /// </summary>
        public int ConjunctionCount { get; }

        /// <summary>
        /// Количество дизъюнкций (логических ИЛИ) в формуле  
        /// </summary>
        public int DisjunctionCount { get; }

        /// <summary>
        /// Общая стоимость формулы (сумма всех метрик)
        /// </summary>
        public int TotalCost { get; }

        /// <summary>
        /// Создает новый экземпляр метрик формулы
        /// </summary>
        /// <param name="dnf">Дизъюнктивная нормальная форма</param>
        /// <param name="knf">Конъюнктивная нормальная форма</param>
        public FormulaMetrics(string dnf, string knf)
        {
            LiteralCount = CountLiterals(dnf) + CountLiterals(knf);
            ConjunctionCount = CountOccurrences(dnf, "∧");
            DisjunctionCount = CountOccurrences(knf, "∨");
            TotalCost = LiteralCount + ConjunctionCount + DisjunctionCount;
        }

        /// <summary>
        /// Подсчитывает количество литералов в формуле
        /// </summary>
        /// <param name="formula">Логическая формула</param>
        /// <returns>Количество литералов</returns>
        private int CountLiterals(string formula)
        {
            if (string.IsNullOrEmpty(formula) || formula == "0" || formula == "1")
                return 0;

            int count = 0;
            bool inVariable = false;

            for (int i = 0; i < formula.Length; i++)
            {
                char c = formula[i];

                if (c == 'x' && i + 1 < formula.Length && char.IsDigit(formula[i + 1]))
                {
                    // Нашли переменную
                    if (!inVariable)
                    {
                        count++;
                        inVariable = true;
                    }
                }
                else if (c == '¬' || c == '!')
                {
                    // Отрицание - тоже литерал
                    if (i + 1 < formula.Length && formula[i + 1] == 'x')
                    {
                        count++;
                        i++; // Пропускаем следующий символ (начало переменной)
                        inVariable = true;
                    }
                }
                else if (!char.IsLetterOrDigit(c))
                {
                    inVariable = false;
                }
            }

            return count;
        }

        /// <summary>
        /// Подсчитывает количество вхождений оператора в формулу
        /// </summary>
        /// <param name="formula">Логическая формула</param>
        /// <param name="operation">Оператор для поиска</param>
        /// <returns>Количество вхождений оператора</returns>
        private int CountOccurrences(string formula, string operation)
        {
            if (string.IsNullOrEmpty(formula))
                return 0;

            int count = 0;
            int index = 0;

            while ((index = formula.IndexOf(operation, index)) != -1)
            {
                count++;
                index += operation.Length;
            }

            return count;
        }

        /// <summary>
        /// Возвращает строковое представление метрик
        /// </summary>
        /// <returns>Строковое представление</returns>
        public override string ToString()
        {
            return $"Литералы: {LiteralCount}, Конъюнкции: {ConjunctionCount}, " +
                   $"Дизъюнкции: {DisjunctionCount}, Общая стоимость: {TotalCost}";
        }
    }
}