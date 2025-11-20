using LogicTool.Business.Interfaces;
using LogicTool.Core.Models;
using LogicTool.Core.Services;
using LogicTool.UI.Commands;
using LogicTool.UI.Services;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace LogicTool.UI.ViewModels
{
    /// <summary>
    /// ViewModel вкладки «По формуле».
    /// </summary>
    public class FormulaTabViewModel : BaseViewModel
    {
        private readonly IBooleanFunctionService _functionService;
        private readonly IClipboardService _clipboardService;
        private readonly FormulaParser _parser;

        private BooleanFunction _currentFunction;

        private string _formula = "(x1 | x2) -> x3";
        private string _resultTitle = "Результат";
        private string _resultText = "Вставьте формулу и нажмите «Разобрать».";
        private string _statusText = "Ожидание ввода.";
        private Brush _statusBrush = Brushes.DimGray;
        private string _complexityWarning = string.Empty;
        private string _metricsSummary = string.Empty;
        private string _basisText = string.Empty;
        private string _pipelineText = string.Empty;

        public FormulaTabViewModel(IBooleanFunctionService functionService, IClipboardService clipboardService)
        {
            _functionService = functionService;
            _clipboardService = clipboardService;
            _parser = new FormulaParser();

            ParseCommand = new RelayCommand(async () => await ParseAsync());
            CopyCommand = new RelayCommand(() => _clipboardService.SetText(ResultText ?? string.Empty),
                () => !string.IsNullOrWhiteSpace(ResultText));
            PresetCommand = new RelayCommand(ApplyPreset);
        }

        /// <summary>
        /// Исходная логическая формула.
        /// </summary>
        public string Formula
        {
            get => _formula;
            set => SetField(ref _formula, value);
        }

        /// <summary>
        /// Заголовок блока результата.
        /// </summary>
        public string ResultTitle
        {
            get => _resultTitle;
            set => SetField(ref _resultTitle, value);
        }

        /// <summary>
        /// Итоговый текст (таблица + ДНФ/КНФ/метрики).
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
        /// Статус последнего разбора.
        /// </summary>
        public string StatusText
        {
            get => _statusText;
            set => SetField(ref _statusText, value);
        }

        /// <summary>
        /// Цвет статуса.
        /// </summary>
        public Brush StatusBrush
        {
            get => _statusBrush;
            set => SetField(ref _statusBrush, value);
        }

        /// <summary>
        /// Предупреждение о сложности.
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
        /// Флаг наличия предупреждения.
        /// </summary>
        public bool HasComplexityWarning => !string.IsNullOrWhiteSpace(ComplexityWarning);

        /// <summary>
        /// Сводка по стоимости формулы.
        /// </summary>
        public string MetricsSummary
        {
            get => _metricsSummary;
            set => SetField(ref _metricsSummary, value);
        }

        /// <summary>
        /// Формула, переписанная в базис {¬, ∧, ∨}.
        /// </summary>
        public string BasisText
        {
            get => _basisText;
            set
            {
                if (SetField(ref _basisText, value))
                {
                    RaisePropertyChanged(nameof(HasBasis));
                }
            }
        }

        /// <summary>
        /// Флаг наличия представления в базисе.
        /// </summary>
        public bool HasBasis => !string.IsNullOrWhiteSpace(BasisText);

        /// <summary>
        /// Текст с лексемами и ОПЗ.
        /// </summary>
        public string PipelineText
        {
            get => _pipelineText;
            set
            {
                if (SetField(ref _pipelineText, value))
                {
                    RaisePropertyChanged(nameof(HasPipeline));
                }
            }
        }

        /// <summary>
        /// Флаг наличия данных по конвейеру.
        /// </summary>
        public bool HasPipeline => !string.IsNullOrWhiteSpace(PipelineText);

        /// <summary>
        /// Подсказка по обозначениям.
        /// </summary>
        public string Legend => "¬/!/not — НЕ, ∧/&/and — И, ∨/|/or — ИЛИ, ^ — XOR, → — импликация, ↔/=/=> — эквивалентность";

        /// <summary>
        /// Команда разбора формулы.
        /// </summary>
        public RelayCommand ParseCommand { get; }

        /// <summary>
        /// Команда копирования результата.
        /// </summary>
        public RelayCommand CopyCommand { get; }

        /// <summary>
        /// Команда вставки пресета с импликацией.
        /// </summary>
        public RelayCommand PresetCommand { get; }

        /// <summary>
        /// Устанавливает формулу пресета.
        /// </summary>
        public void ApplyPreset()
        {
            // Задаём формулу из обязательного пресета и запускаем разбор asinхронно.
            Formula = "(x1 | x2) -> x3";
            _ = ParseAsync();
        }

        /// <summary>
        /// Выполняет полный цикл разбора формулы и вычислений.
        /// </summary>
        private async Task ParseAsync()
        {
            try
            {
                // 1) Лексер: разбиваем формулу на токены.
                var tokens = await Task.Run(() => _parser.Tokenize(Formula));
                // 2) Алгоритм сортировочной станции → обратная польская запись.
                var rpn = _parser.ToRPN(tokens);
                // 3) Переписываем формулу в чистый базис, чтобы показать пользователю.
                var basis = _parser.ToBasicBasis(Formula);
                // 4) Строим булеву функцию (таблица, ДНФ, КНФ).
                var function = await Task.Run(() => _functionService.CreateFromFormula(Formula));

                _currentFunction = function;

                ComplexityWarning = function.GetComplexityWarning();
                MetricsSummary = function.CalculateMetrics().ToString();
                BasisText = basis;
                PipelineText = BuildPipeline(tokens, rpn);

                ResultTitle = "Таблица + ДНФ + КНФ";
                ResultText = BuildDetailedReport(function);
                StatusText = "Разбор и вычисление выполнены.";
                StatusBrush = Brushes.ForestGreen;
            }
            catch (Exception ex)
            {
                _currentFunction = null;
                ComplexityWarning = string.Empty;
                MetricsSummary = string.Empty;
                BasisText = string.Empty;
                PipelineText = string.Empty;

                // Показываем текст ошибки в блоке результатов, чтобы студент увидел подробности разбора.
                ResultTitle = "Диагностика";
                ResultText = ex.Message;
                StatusText = $"Ошибка: {ex.Message}";
                StatusBrush = Brushes.Firebrick;
            }
            finally
            {
                CopyCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Формирует текст с токенами и обратной польской записью.
        /// </summary>
        private string BuildPipeline(System.Collections.Generic.List<string> tokens, System.Collections.Generic.List<string> rpn)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Лексемы:");
            sb.AppendLine(string.Join(" ", tokens));
            sb.AppendLine();
            sb.AppendLine("ОПЗ:");
            sb.AppendLine(string.Join(" ", rpn));
            return sb.ToString();
        }

        /// <summary>
        /// Формирует текст отчёта с таблицей и нормальными формами.
        /// </summary>
        private string BuildDetailedReport(BooleanFunction function)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Таблица истинности:");
            sb.AppendLine(BuildTruthTable(function));
            sb.AppendLine();
            sb.AppendLine("Совершенная ДНФ:");
            sb.AppendLine(function.DNF);
            sb.AppendLine();
            sb.AppendLine("Совершенная КНФ:");
            sb.AppendLine(function.KNF);
            sb.AppendLine();
            sb.AppendLine(MetricsSummary);
            return sb.ToString();
        }

        /// <summary>
        /// Строит текст таблицы истинности для функции.
        /// </summary>
        private string BuildTruthTable(BooleanFunction function)
        {
            if (function.TruthTable == null || function.TruthTable.Count == 0)
            {
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
    }
}

