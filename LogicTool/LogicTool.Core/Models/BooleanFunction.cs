using LogicTool.Core.Enums;
using LogicTool.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogicTool.Core.Models
{
    /// <summary>
    /// Представляет булеву функцию и предоставляет методы для работы с ней
    /// </summary>
    public class BooleanFunction
    {
        /// <summary>
        /// Количество переменных функции
        /// </summary>
        public int VariableCount { get; private set; }

        /// <summary>
        /// Таблица истинности функции
        /// </summary>
        public List<TruthTableRow> TruthTable { get; private set; }

        /// <summary>
        /// Порядок переменных в таблице
        /// </summary>
        public IReadOnlyList<string> VariableNames { get; private set; } = Array.Empty<string>();

        /// <summary>
        /// Дизъюнктивная нормальная форма (ДНФ)
        /// </summary>
        public string DNF { get; private set; }

        /// <summary>
        /// Конъюнктивная нормальная форма (КНФ)
        /// </summary>
        public string KNF { get; private set; }

        /// <summary>
        /// Исходная формула (если функция задана формулой)
        /// </summary>
        public string OriginalFormula { get; private set; }

        /// <summary>
        /// Номер функции (если функция задана номером)
        /// </summary>
        public long FunctionNumber { get; private set; }

        /// <summary>
        /// Уровень вычислительной сложности
        /// </summary>
        public ComplexityLevel Complexity { get; private set; }

        /// <summary>
        /// Создает булеву функцию по номеру
        /// </summary>
        /// <param name="variableCount">Количество переменных</param>
        /// <param name="functionNumber">Номер функции</param>
        /// <returns>Новый экземпляр BooleanFunction</returns>
        public static BooleanFunction FromNumber(int variableCount, long functionNumber)
        {
            var function = new BooleanFunction();
            function.GenerateFromNumber(variableCount, functionNumber);
            return function;
        }

        /// <summary>
        /// Создает булеву функцию по формуле
        /// </summary>
        /// <param name="formula">Логическая формула</param>
        /// <returns>Новый экземпляр BooleanFunction</returns>
        public static BooleanFunction FromFormula(string formula)
        {
            var function = new BooleanFunction();
            function.GenerateFromFormula(formula);
            return function;
        }

        /// <summary>
        /// Генерирует функцию по номеру
        /// </summary>
        /// <param name="variableCount">Количество переменных</param>
        /// <param name="functionNumber">Номер функции</param>
        private void GenerateFromNumber(int variableCount, long functionNumber)
        {
            VariableCount = variableCount;
            FunctionNumber = functionNumber;
            OriginalFormula = string.Empty;
            Complexity = CalculateComplexityLevel(variableCount);
            VariableNames = Enumerable.Range(1, variableCount)
                .Select(i => $"x{i}")
                .ToList();

            int rowCount = 1 << variableCount; // 2^variableCount

            ValidateNumberRange(functionNumber, rowCount);
            GenerateTruthTableFromNumber(variableCount, functionNumber, rowCount);
            GenerateNormalForms();
        }

        /// <summary>
        /// Генерирует функцию по формуле
        /// </summary>
        /// <param name="formula">Логическая формула</param>
        private void GenerateFromFormula(string formula)
        {
            if (string.IsNullOrWhiteSpace(formula))
                throw new ArgumentException("Формула не может быть пустой или содержать только пробелы");

            OriginalFormula = formula;

            var parser = new FormulaParser();
            var tokens = parser.Tokenize(formula);
            var rpn = parser.ToRPN(tokens);

            var variables = ExtractVariables(tokens);
            VariableCount = variables.Count;
            VariableNames = variables;
            FunctionNumber = -1; // Не определен для функций по формуле
            Complexity = CalculateComplexityLevel(VariableCount);

            GenerateTruthTableFromFormula(rpn, parser);
            GenerateNormalForms();
        }

        /// <summary>
        /// Генерирует таблицу истинности по номеру функции
        /// </summary>
        /// <param name="n">Количество переменных</param>
        /// <param name="number">Номер функции</param>
        /// <param name="rowCount">Количество строк в таблице</param>
        private void GenerateTruthTableFromNumber(int n, long number, int rowCount)
        {
            TruthTable = new List<TruthTableRow>();

            for (int i = 0; i < rowCount; i++)
            {
                var values = new Dictionary<string, bool>();

                // Генерируем значения переменных для текущей строки
                for (int j = 0; j < n; j++)
                {
                    string varName = VariableNames[j];
                    // Вычисляем значение переменной из битового представления номера строки
                    bool value = ((i >> (n - 1 - j)) & 1) == 1;
                    values[varName] = value;
                }

                // Вычисляем результат функции для данного набора переменных
                // Биты номера функции соответствуют строкам таблицы в лексикографическом порядке
                bool result = ((number >> (rowCount - 1 - i)) & 1) == 1;
                TruthTable.Add(new TruthTableRow(values, result));
            }
        }

        /// <summary>
        /// Генерирует таблицу истинности по формуле
        /// </summary>
        /// <param name="variables">Список переменных</param>
        /// <param name="rpn">Обратная польская запись формулы</param>
        /// <param name="parser">Парсер формул</param>
        private void GenerateTruthTableFromFormula(List<string> rpn, FormulaParser parser)
        {
            TruthTable = new List<TruthTableRow>();
            int n = VariableNames.Count;
            int rowCount = 1 << n;

            for (int i = 0; i < rowCount; i++)
            {
                var values = new Dictionary<string, bool>();

                // Устанавливаем значения переменных для текущей строки
                for (int j = 0; j < n; j++)
                {
                    string varName = VariableNames[j];
                    bool value = ((i >> (n - 1 - j)) & 1) == 1;
                    values[varName] = value;
                }

                // Вычисляем значение формулы для данного набора переменных
                bool result = parser.EvaluateRPN(rpn, values);
                TruthTable.Add(new TruthTableRow(values, result));
            }
        }

        /// <summary>
        /// Генерирует нормальные формы (ДНФ и КНФ) на основе таблицы истинности
        /// </summary>
        private void GenerateNormalForms()
        {
            DNF = BuildDNF();
            KNF = BuildKNF();
        }

        /// <summary>
        /// Строит совершенную ДНФ по таблице истинности
        /// </summary>
        /// <returns>Строковое представление ДНФ</returns>
        private string BuildDNF()
        {
            // Выбираем строки где функция равна true (1)
            var trueRows = TruthTable.Where(row => row.Result).ToList();

            if (trueRows.Count == 0)
                return "0"; // Константа false

            var dnfTerms = new List<string>();

            foreach (var row in trueRows)
            {
                // Для каждой true-строки строим совершенную конъюнкцию
                var literals = new List<string>();

                foreach (var variable in VariableNames)
                {
                    bool value = row.Values[variable];
                    string literal = value ? variable : $"¬{variable}";
                    literals.Add(literal);
                }

                // Объединяем литералы в конъюнкцию
                string term = $"({string.Join(" ∧ ", literals)})";
                dnfTerms.Add(term);
            }

            // Объединяем все конъюнкции в дизъюнкцию
            return string.Join(" ∨ ", dnfTerms);
        }

        /// <summary>
        /// Строит совершенную КНФ по таблице истинности
        /// </summary>
        /// <returns>Строковое представление КНФ</returns>
        private string BuildKNF()
        {
            // Выбираем строки где функция равна false (0)
            var falseRows = TruthTable.Where(row => !row.Result).ToList();

            if (falseRows.Count == 0)
                return "1"; // Константа true

            var knfTerms = new List<string>();

            foreach (var row in falseRows)
            {
                // Для каждой false-строки строим совершенную дизъюнкцию
                var literals = new List<string>();

                foreach (var variable in VariableNames)
                {
                    bool value = row.Values[variable];
                    string literal = !value ? variable : $"¬{variable}";
                    literals.Add(literal);
                }

                // Объединяем литералы в дизъюнкцию
                string term = $"({string.Join(" ∨ ", literals)})";
                knfTerms.Add(term);
            }

            // Объединяем все дизъюнкции в конъюнкцию
            return string.Join(" ∧ ", knfTerms);
        }

        /// <summary>
        /// Извлекает список переменных из токенов формулы
        /// </summary>
        /// <param name="tokens">Список токенов</param>
        /// <returns>Отсортированный список уникальных переменных</returns>
        private List<string> ExtractVariables(List<string> tokens)
        {
            var variables = new HashSet<string>();

            foreach (string token in tokens)
            {
                // Переменные начинаются с буквы и не являются операторами
                if (token.Length > 0 && char.IsLetter(token[0]) &&
                    !FormulaParser.Operators.ContainsKey(token))
                {
                    variables.Add(token);
                }
            }

            return variables.OrderBy(v => v).ToList();
        }

        /// <summary>
        /// Проверяет корректность номера функции для заданного количества переменных
        /// </summary>
        /// <param name="number">Номер функции</param>
        /// <param name="rowCount">Количество строк в таблице истинности</param>
        private void ValidateNumberRange(long number, int rowCount)
        {
            long maxNumber = (1L << rowCount) - 1;
            if (number < 0 || number > maxNumber)
            {
                throw new ArgumentException(
                    $"Номер функции должен быть в диапазоне [0, {maxNumber}] для {VariableCount} переменных. " +
                    $"Получено: {number}");
            }
        }

        /// <summary>
        /// Вычисляет уровень сложности на основе количества переменных
        /// </summary>
        /// <param name="variableCount">Количество переменных</param>
        /// <returns>Уровень сложности</returns>
        private ComplexityLevel CalculateComplexityLevel(int variableCount)
        {
            if (variableCount <= 4) return ComplexityLevel.Low;
            if (variableCount <= 6) return ComplexityLevel.Medium;
            if (variableCount <= 8) return ComplexityLevel.High;
            if (variableCount <= 11) return ComplexityLevel.VeryHigh;
            return ComplexityLevel.Critical;
        }

        /// <summary>
        /// Возвращает нормальную форму указанного типа
        /// </summary>
        /// <param name="formType">Тип нормальной формы</param>
        /// <returns>Строковое представление нормальной формы</returns>
        public string GetNormalForm(NormalFormType formType)
        {
            switch (formType)
            {
                case NormalFormType.DNF:
                case NormalFormType.PerfectDNF:
                    return DNF;
                case NormalFormType.KNF:
                case NormalFormType.PerfectKNF:
                    return KNF;
                default:
                    throw new ArgumentException($"Неизвестный тип нормальной формы: {formType}");
            }
        }

        /// <summary>
        /// Вычисляет метрики сложности формулы
        /// </summary>
        /// <returns>Объект FormulaMetrics с вычисленными метриками</returns>
        public FormulaMetrics CalculateMetrics()
        {
            return new FormulaMetrics(DNF, KNF);
        }

        /// <summary>
        /// Возвращает предупреждение о сложности вычислений
        /// </summary>
        /// <returns>Текст предупреждения или пустая строка если сложность низкая</returns>
        public string GetComplexityWarning()
        {
            switch (Complexity)
            {
                case ComplexityLevel.Low:
                    return string.Empty;
                case ComplexityLevel.Medium:
                    return "Внимание: средняя сложность вычислений";
                case ComplexityLevel.High:
                    return "Предупреждение: высокая сложность вычислений (O(2^n))";
                case ComplexityLevel.VeryHigh:
                    return "Сильное предупреждение: очень высокая сложность вычислений";
                case ComplexityLevel.Critical:
                    return "КРИТИЧЕСКОЕ ПРЕДУПРЕЖДЕНИЕ: Экспоненциальная сложность может привести к зависанию";
                default:
                    return "Неизвестный уровень сложности";
            }
        }

        /// <summary>
        /// Возвращает текст с пояснением соответствия битов и строк таблицы истинности
        /// </summary>
        public string DescribeBinaryMapping()
        {
            if (TruthTable == null || TruthTable.Count == 0 || FunctionNumber < 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            string binary = Convert.ToString(FunctionNumber, 2).PadLeft(TruthTable.Count, '0');
            sb.AppendLine($"Бинарный код функции ({TruthTable.Count} бит): {binary}");
            sb.AppendLine("Строки читаются слева направо, в лексикографическом порядке переменных.");

            for (int i = 0; i < TruthTable.Count; i++)
            {
                var row = TruthTable[i];
                var tuple = string.Join(", ", VariableNames.Select(name => $"{name}={BoolToText(row.Values[name])}"));
                sb.AppendLine($"{i + 1}. ({tuple}) → f = {BoolToText(row.Result)} ↔ бит {binary[i]}");
            }

            return sb.ToString();
        }

        private static string BoolToText(bool value) => value ? "1" : "0";
    }
}