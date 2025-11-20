using LogicTool.Business.Interfaces;
using LogicTool.Core.Models;
using LogicTool.UI.Commands;
using System;
using System.Threading.Tasks;

namespace LogicTool.UI.ViewModels
{
    /// <summary>
    /// Тип ввода функции на вкладке сравнения.
    /// </summary>
    public enum FunctionInputKind
    {
        Formula,
        Number
    }

    /// <summary>
    /// ViewModel вкладки «Сравнение».
    /// </summary>
    public class ComparisonTabViewModel : BaseViewModel
    {
        private readonly IBooleanFunctionService _functionService;
        private readonly IComparisonService _comparisonService;

        private FunctionInputKind _firstKind = FunctionInputKind.Formula;
        private FunctionInputKind _secondKind = FunctionInputKind.Formula;

        private string _formula1 = "(x1 & !x2) | x3";
        private string _formula2 = "(x3) | (x1 & !x2)";
        private int _varCount1 = 3;
        private int _varCount2 = 3;
        private long _number1;
        private long _number2;

        private string _comparisonSummary = "Введите данные и нажмите «Сравнить».";
        private string _counterExample = string.Empty;
        private string _statusDetail = string.Empty;

        public ComparisonTabViewModel(IBooleanFunctionService functionService, IComparisonService comparisonService)
        {
            _functionService = functionService;
            _comparisonService = comparisonService;

            CompareCommand = new RelayCommand(async () => await CompareAsync());
            PresetCommand = new RelayCommand(ApplyPreset);
        }

        /// <summary>
        /// Режим ввода первой функции.
        /// </summary>
        public FunctionInputKind FirstKind
        {
            get => _firstKind;
            set
            {
                if (SetField(ref _firstKind, value))
                {
                    RaisePropertyChanged(nameof(ShowFormula1));
                    RaisePropertyChanged(nameof(ShowNumber1));
                }
            }
        }

        /// <summary>
        /// Режим ввода второй функции.
        /// </summary>
        public FunctionInputKind SecondKind
        {
            get => _secondKind;
            set
            {
                if (SetField(ref _secondKind, value))
                {
                    RaisePropertyChanged(nameof(ShowFormula2));
                    RaisePropertyChanged(nameof(ShowNumber2));
                }
            }
        }

        /// <summary>
        /// Отображать ли поле формулы для первой функции.
        /// </summary>
        public bool ShowFormula1 => FirstKind == FunctionInputKind.Formula;

        /// <summary>
        /// Отображать ли поля номера для первой функции.
        /// </summary>
        public bool ShowNumber1 => FirstKind == FunctionInputKind.Number;

        /// <summary>
        /// Отображать ли поле формулы для второй функции.
        /// </summary>
        public bool ShowFormula2 => SecondKind == FunctionInputKind.Formula;

        /// <summary>
        /// Отображать ли поля номера для второй функции.
        /// </summary>
        public bool ShowNumber2 => SecondKind == FunctionInputKind.Number;

        /// <summary>
        /// Формула для первой функции.
        /// </summary>
        public string Formula1
        {
            get => _formula1;
            set => SetField(ref _formula1, value);
        }

        /// <summary>
        /// Формула для второй функции.
        /// </summary>
        public string Formula2
        {
            get => _formula2;
            set => SetField(ref _formula2, value);
        }

        /// <summary>
        /// Число переменных первой числовой функции.
        /// </summary>
        public int VarCount1
        {
            get => _varCount1;
            set => SetField(ref _varCount1, Math.Max(1, Math.Min(10, value)));
        }

        /// <summary>
        /// Число переменных второй числовой функции.
        /// </summary>
        public int VarCount2
        {
            get => _varCount2;
            set => SetField(ref _varCount2, Math.Max(1, Math.Min(10, value)));
        }

        /// <summary>
        /// Номер первой функции.
        /// </summary>
        public long Number1
        {
            get => _number1;
            set => SetField(ref _number1, Math.Max(0, value));
        }

        /// <summary>
        /// Номер второй функции.
        /// </summary>
        public long Number2
        {
            get => _number2;
            set => SetField(ref _number2, Math.Max(0, value));
        }

        /// <summary>
        /// Текст с итогом сравнения.
        /// </summary>
        /// <summary>
        /// Текст с итогом сравнения.
        /// </summary>
        public string ComparisonSummary
        {
            get => _comparisonSummary;
            private set => SetField(ref _comparisonSummary, value);
        }

        /// <summary>
        /// Дополнительные сведения (ошибки, пояснения).
        /// </summary>
        public string StatusDetail
        {
            get => _statusDetail;
            private set => SetField(ref _statusDetail, value);
        }

        /// <summary>
        /// Текст с найденным контрпримером.
        /// </summary>
        public string CounterExample
        {
            get => _counterExample;
            private set
            {
                if (SetField(ref _counterExample, value))
                {
                    RaisePropertyChanged(nameof(HasCounterExample));
                }
            }
        }

        /// <summary>
        /// Флаг наличия контрпримера.
        /// </summary>
        public bool HasCounterExample => !string.IsNullOrWhiteSpace(CounterExample);

        /// <summary>
        /// Подсказка по операторам.
        /// </summary>
        public string Legend => "¬/!/not — НЕ, ∧/&/and — И, ∨/|/or — ИЛИ, ^ — XOR, → — импликация, ↔/=/=> — эквивалентность";

        /// <summary>
        /// Команда сравнения функций.
        /// </summary>
        public RelayCommand CompareCommand { get; }

        /// <summary>
        /// Команда установки пресета эквивалентности.
        /// </summary>
        public RelayCommand PresetCommand { get; }

        /// <summary>
        /// Заполняет поля пресетом из ДНФ.
        /// </summary>
        public void ApplyPreset()
        {
            // Используем обязательный пресет: сравнение формулы с её ДНФ.
            FirstKind = FunctionInputKind.Formula;
            SecondKind = FunctionInputKind.Formula;
            Formula1 = "(x1 & !x2) | x3";
            Formula2 = "(x3) | (x1 & !x2)";
            ComparisonSummary = "Пример готов. Нажмите «Сравнить».";
            CounterExample = string.Empty;
            StatusDetail = string.Empty;
        }

        /// <summary>
        /// Выполняет сравнение выбранных функций.
        /// </summary>
        private async Task CompareAsync()
        {
            try
            {
                Core.Models.ComparisonResult result;

                if (FirstKind == FunctionInputKind.Number && SecondKind == FunctionInputKind.Number)
                {
                    // Оба значения заданы по номеру — можно обойтись без парсера.
                    result = CompareNumbers();
                }
                else if (FirstKind == FunctionInputKind.Formula && SecondKind == FunctionInputKind.Formula)
                {
                    // Формула против формулы — используем сервис сравнения формул.
                    result = await Task.Run(() => _comparisonService.CompareByFormula(Formula1, Formula2));
                }
                else if (FirstKind == FunctionInputKind.Number && SecondKind == FunctionInputKind.Formula)
                {
                    // Смешанный режим: номер против формулы.
                    result = await Task.Run(() =>
                        _comparisonService.CompareByNumberAndFormula(VarCount1, Number1, Formula2));
                }
                else if (FirstKind == FunctionInputKind.Formula && SecondKind == FunctionInputKind.Number)
                {
                    // Второй случай смешанного режима — меняем аргументы местами.
                    result = await Task.Run(() =>
                        _comparisonService.CompareByNumberAndFormula(VarCount2, Number2, Formula1));
                }
                else
                {
                    result = Core.Models.ComparisonResult.Error("Неизвестный режим сравнения.");
                }

                StatusDetail = result.Message;
                CounterExample = result.CounterExample;

                switch (result.ResultType)
                {
                    case LogicTool.Core.Enums.ComparisonResultType.Equivalent:
                        ComparisonSummary = "✅ Функции эквивалентны";
                        break;
                    case LogicTool.Core.Enums.ComparisonResultType.NotEquivalent:
                        ComparisonSummary = "❌ Функции различаются";
                        break;
                    default:
                        // Сервис вернул ошибку (например, слишком много переменных).
                        ComparisonSummary = $"⚠️ {result.Message}";
                        break;
                }
            }
            catch (Exception ex)
            {
                // Не скрываем исключение: сообщение идёт напрямую к пользователю,
                // чтобы он увидел, что именно пошло не так.
                ComparisonSummary = $"⚠️ Ошибка: {ex.Message}";
                CounterExample = string.Empty;
                StatusDetail = string.Empty;
            }
        }

        /// <summary>
        /// Сравнивает две функции, заданные номерами.
        /// </summary>
        private ComparisonResult CompareNumbers()
        {
            var func1 = _functionService.CreateFromNumber(VarCount1, Number1);
            var func2 = _functionService.CreateFromNumber(VarCount2, Number2);
            return _comparisonService.CompareFunctions(func1, func2);
        }
    }
}

