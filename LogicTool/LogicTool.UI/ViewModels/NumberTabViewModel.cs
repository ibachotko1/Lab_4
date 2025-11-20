using LogicTool.Business.Interfaces;
using LogicTool.Core.Models;
using LogicTool.UI.Commands;
using LogicTool.UI.Services;
using LogicTool.UI.ViewModels;
using System;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;


namespace LogicTool.UI.ViewModels
{
    /// <summary>
    /// ViewModel вкладки «По номеру».
    /// </summary>
    public class NumberTabViewModel : BaseViewModel
    {
        private readonly IBooleanFunctionService _functionService;
        private readonly IClipboardService _clipboardService;

        private BooleanFunction _currentFunction;
        private int _variableCount = 3;
        private long _functionNumber = 11;
        private string _binaryRepresentation = string.Empty;
        private string _binaryExplanation = string.Empty;
        private string _complexityWarning = string.Empty;
        private string _resultTitle = "Результат";
        private string _resultText = "Нажмите «Вычислить», чтобы построить таблицу.";
        private string _metricsSummary = string.Empty;

        public NumberTabViewModel(IBooleanFunctionService functionService, IClipboardService clipboardService)
        {
            _functionService = functionService;
            _clipboardService = clipboardService;

            // Команды в конструкторе сразу связываются с обработчиками,
            // чтобы UI оставался максимально «тонким».
            GenerateCommand = new RelayCommand(async () => await GenerateAsync());
            ShowTruthTableCommand = new RelayCommand(ShowTruthTable, () => _currentFunction != null);
            ShowDnfCommand = new RelayCommand(ShowDnf, () => _currentFunction != null);
            ShowKnfCommand = new RelayCommand(ShowKnf, () => _currentFunction != null);
            CopyCommand = new RelayCommand(() => _clipboardService.SetText(ResultText ?? string.Empty),
                () => !string.IsNullOrWhiteSpace(ResultText));
            PresetCommand = new RelayCommand(ApplyPreset);

            UpdateBinaryHints();
        }

        /// <summary>
        /// Количество переменных функции.
        /// </summary>
        public int VariableCount
        {
            get => _variableCount;
            set
            {
                int clamped = Math.Max(1, Math.Min(10, value));
                if (SetField(ref _variableCount, clamped))
                {
                    UpdateBinaryHints();
                }
            }
        }

        /// <summary>
        /// Номер булевой функции (в десятичном виде).
        /// </summary>
        public long FunctionNumber
        {
            get => _functionNumber;
            set
            {
                if (value < 0)
                {
                    value = 0;
                }

                if (SetField(ref _functionNumber, value))
                {
                    UpdateBinaryHints();
                }
            }
        }

        /// <summary>
        /// Краткое описание диапазона и двоичного вида номера.
        /// </summary>
        public string BinaryRepresentation
        {
            get => _binaryRepresentation;
            set => SetField(ref _binaryRepresentation, value);
        }

        /// <summary>
        /// Расшифровка соответствия битов и строк таблицы.
        /// </summary>
        public string BinaryExplanation
        {
            get => _binaryExplanation;
            set
            {
                if (SetField(ref _binaryExplanation, value))
                {
                    RaisePropertyChanged(nameof(HasBinaryExplanation));
                }
            }
        }

        /// <summary>
        /// Флаг видимости расшифровки.
        /// </summary>
        public bool HasBinaryExplanation => !string.IsNullOrWhiteSpace(BinaryExplanation);

        /// <summary>
        /// Текст предупреждения о сложности вычислений.
        /// </summary>
        public string ComplexityWarning
        {
            get => _complexityWarning;
            set
            {
                if (SetField(ref _complexityWarning, value))
                {
                    RaisePropertyChanged(nameof(HasComplexityWarning));
                }
            }
        }

        /// <summary>
        /// Флаг видимости предупреждения.
        /// </summary>
        public bool HasComplexityWarning => !string.IsNullOrWhiteSpace(ComplexityWarning);

        /// <summary>
        /// Заголовок блока результата.
        /// </summary>
        public string ResultTitle
        {
            get => _resultTitle;
            set => SetField(ref _resultTitle, value);
        }

        /// <summary>
        /// Текст результата (таблица/ДНФ/КНФ).
        /// </summary>
        public string ResultText
        {
            get => _resultText;
            set
            {
                if (SetField(ref _resultText, value))
                {
                    CopyCommand.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// Сводка по метрикам формулы.
        /// </summary>
        public string MetricsSummary
        {
            get => _metricsSummary;
            set => SetField(ref _metricsSummary, value);
        }

        /// <summary>
        /// Подсказка по обозначениям логических операций.
        /// </summary>
        public string Legend => "¬/!/not — НЕ, ∧/&/and — И, ∨/|/or — ИЛИ, ^ — XOR, → — импликация, ↔/=/=> — эквивалентность";

        /// <summary>
        /// Команда генерации функции по номеру.
        /// </summary>
        public RelayCommand GenerateCommand { get; }

        /// <summary>
        /// Команда показа таблицы истинности.
        /// </summary>
        public RelayCommand ShowTruthTableCommand { get; }

        /// <summary>
        /// Команда показа ДНФ.
        /// </summary>
        public RelayCommand ShowDnfCommand { get; }

        /// <summary>
        /// Команда показа КНФ.
        /// </summary>
        public RelayCommand ShowKnfCommand { get; }

        /// <summary>
        /// Команда копирования результата.
        /// </summary>
        public RelayCommand CopyCommand { get; }

        /// <summary>
        /// Команда применения пресета n=3, num=11.
        /// </summary>
        public RelayCommand PresetCommand { get; }

        /// <summary>
        /// Устанавливает параметры пресета.
        /// </summary>
        public void ApplyPreset()
        {
            VariableCount = 3;
            FunctionNumber = 11;
            _ = GenerateAsync();
        }

        /// <summary>
        /// Выполняет построение функции по текущим параметрам.
        /// </summary>
        private async Task GenerateAsync()
        {
            try
            {
                // Запускаем построение функции в пуле потоков, чтобы не блокировать UI.
                var function = await Task.Run(() => _functionService.CreateFromNumber(VariableCount, FunctionNumber));
                _currentFunction = function;

                ComplexityWarning = function.GetComplexityWarning();
                BinaryExplanation = function.DescribeBinaryMapping();
                MetricsSummary = function.CalculateMetrics().ToString();

                ResultTitle = "Таблица истинности";
                ResultText = BuildTruthTable(function);

                RefreshCommandStates();
            }
            catch (Exception ex)
            {
                // В случае ошибки очищаем состояние и показываем диагностическое сообщение.
                _currentFunction = null;
                ComplexityWarning = string.Empty;
                BinaryExplanation = string.Empty;
                MetricsSummary = string.Empty;
                ResultTitle = "Ошибка";
                ResultText = ex.Message;
                RefreshCommandStates();
            }
        }

        /// <summary>
        /// Выводит таблицу истинности.
        /// </summary>
        private void ShowTruthTable()
        {
            if (_currentFunction == null)
            {
                return;
            }

            ResultTitle = "Таблица истинности";
            ResultText = BuildTruthTable(_currentFunction);
        }

        /// <summary>
        /// Выводит совершенную ДНФ.
        /// </summary>
        private void ShowDnf()
        {
            if (_currentFunction == null)
            {
                return;
            }

            ResultTitle = "Совершенная ДНФ";
            ResultText = $"{_currentFunction.DNF}\n\n{MetricsSummary}";
        }

        /// <summary>
        /// Выводит совершенную КНФ.
        /// </summary>
        private void ShowKnf()
        {
            if (_currentFunction == null)
            {
                return;
            }

            ResultTitle = "Совершенная КНФ";
            ResultText = $"{_currentFunction.KNF}\n\n{MetricsSummary}";
        }

        /// <summary>
        /// Пересчитывает бинарное представление номера.
        /// </summary>
        private void UpdateBinaryHints()
        {
            if (VariableCount > 15)
            {
                // При слишком большом n предупреждаем пользователя,
                // вместо того чтобы пытаться формировать гигантские строки.
                BinaryRepresentation = "n > 15 не поддерживается.";
                return;
            }

            int rows = 1 << VariableCount;
            long maxNum = (1L << rows) - 1;
            string rangeText = $"2^{VariableCount} = {rows} строк, num ∈ [0, {maxNum}]";

            if (FunctionNumber < 0 || FunctionNumber > maxNum)
            {
                BinaryRepresentation = $"{rangeText}. Текущий num={FunctionNumber} вне диапазона.";
                return;
            }

            string binary = Convert.ToString(FunctionNumber, 2).PadLeft(rows, '0');
            BinaryRepresentation = $"{rangeText}. num={FunctionNumber} = {binary}_2";
        }

        /// <summary>
        /// Формирует текст таблицы истинности.
        /// </summary>
        private string BuildTruthTable(BooleanFunction function)
        {
            if (function.TruthTable == null || function.TruthTable.Count == 0)
            {
                // Возвращаем небольшое пояснение вместо пустой строки,
                // что упрощает диагностику внутри UI.
                return "Таблица пуста.";
            }

            var sb = new StringBuilder();
            sb.AppendLine(string.Join(" | ", function.VariableNames) + " | f");
            sb.AppendLine(new string('-', function.VariableNames.Count * 4 + 3));

            foreach (var row in function.TruthTable)
            {
                var values = function.VariableNames
                    .Select(name => row.Values[name] ? "1" : "0");
                sb.AppendLine($"{string.Join(" | ", values)} | {(row.Result ? "1" : "0")}");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Обновляет состояние команд, зависящих от наличия функции.
        /// </summary>
        private void RefreshCommandStates()
        {
            ShowTruthTableCommand.RaiseCanExecuteChanged();
            ShowDnfCommand.RaiseCanExecuteChanged();
            ShowKnfCommand.RaiseCanExecuteChanged();
        }
    }
}

