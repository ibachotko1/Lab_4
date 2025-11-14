using System;
using System.Collections.Generic;
using System.Linq;

namespace LogicTool.Core.Models
{
    /// <summary>
    /// Представляет одну строку таблицы истинности
    /// </summary>
    public class TruthTableRow
    {
        /// <summary>
        /// Словарь значений переменных для данной строки
        /// </summary>
        public Dictionary<string, bool> Values { get; }

        /// <summary>
        /// Результат вычисления функции для данного набора переменных
        /// </summary>
        public bool Result { get; }

        /// <summary>
        /// Создает новую строку таблицы истинности
        /// </summary>
        /// <param name="values">Значения переменных</param>
        /// <param name="result">Результат функции</param>
        public TruthTableRow(Dictionary<string, bool> values, bool result)
        {
            Values = new Dictionary<string, bool>(values);
            Result = result;
        }

        /// <summary>
        /// Проверяет, совпадает ли строка с заданными значениями переменных
        /// </summary>
        /// <param name="testValues">Тестовые значения переменных</param>
        /// <returns>True если значения совпадают, иначе False</returns>
        public bool Matches(Dictionary<string, bool> testValues)
        {
            foreach (var kvp in testValues)
            {
                if (!Values.ContainsKey(kvp.Key) || Values[kvp.Key] != kvp.Value)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Возвращает строковое представление строки таблицы
        /// </summary>
        /// <returns>Строковое представление</returns>
        public override string ToString()
        {
            var valuesStr = string.Join(", ", Values.Select(v => $"{v.Key}={v.Value}"));
            return $"{valuesStr} → {Result}";
        }
    }
}