using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;  
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LogicTool.Business.Interfaces;
using LogicTool.Business.Services;
using LogicTool.Core.Models;

namespace LogicTool.UI
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly IBooleanFunctionService _booleanFunctionService;
        private readonly IComparisonService _comparisonService;

        // --- По номеру ---
        private int _variableCount = 3;
        private long _functionNumber = 11;
        private string _binaryRepresentation = "";
        private BooleanFunction _currentFunctionNumber;

        // --- По формуле ---
        private string _formula = "(x1 | x2) -> x3";
        private BooleanFunction _currentFunctionFormula;

        // --- Сравнение ---
        private bool _isFormula1 = true;
        private bool _isNumber1 = false;
        private string _formula1 = "x1 & !x2 | x3";
        private int _varCount1 = 3;
        private long _number1 = 0;

        private bool _isFormula2 = true;
        private bool _isNumber2 = false;
        private string _formula2 = "";
        private int _varCount2 = 3;
        private long _number2 = 0;

        // --- Общие ---
        private string _resultText = "";
        private string _status = "";
        private Brush _statusColor = Brushes.Black;
        private string _complexityWarning = "";
        private string _comparisonResult = "";
        private string _counterExample = "";

        public MainWindow()
        {
            InitializeComponent();

            // Регистрируем конвертеры
            Resources.Add("StatusStyleConverter", new StatusStyleConverter());
            Resources.Add("ResultColorConverter", new ResultColorConverter());

            DataContext = this;
            _booleanFunctionService = new BooleanFunctionService();
            _comparisonService = new ComparisonService();

            UpdateBinaryRepresentation();
        }

        // === Свойства ===
        public int VariableCount { get => _variableCount; set { _variableCount = value; OnPropertyChanged(); UpdateBinaryRepresentation(); } }
        public long FunctionNumber { get => _functionNumber; set { _functionNumber = value; OnPropertyChanged(); UpdateBinaryRepresentation(); } }
        public string BinaryRepresentation { get => _binaryRepresentation; set { _binaryRepresentation = value; OnPropertyChanged(); } }
        public string Formula { get => _formula; set { _formula = value; OnPropertyChanged(); } }
        public string ResultText { get => _resultText; set { _resultText = value; OnPropertyChanged(); } }
        public string Status { get => _status; set { _status = value; OnPropertyChanged(); } }
        public Brush StatusColor { get => _statusColor; set { _statusColor = value; OnPropertyChanged(); } }
        public string ComplexityWarning { get => _complexityWarning; set { _complexityWarning = value; OnPropertyChanged(); } }

        public bool IsFormula1 { get => _isFormula1; set { _isFormula1 = value; _isNumber1 = !value; OnPropertyChanged(); OnPropertyChanged(nameof(IsNumber1)); } }
        public bool IsNumber1 { get => _isNumber1; set { _isNumber1 = value; _isFormula1 = !value; OnPropertyChanged(); OnPropertyChanged(nameof(IsFormula1)); } }
        public string Formula1 { get => _formula1; set { _formula1 = value; OnPropertyChanged(); } }
        public int VarCount1 { get => _varCount1; set { _varCount1 = value; OnPropertyChanged(); } }
        public long Number1 { get => _number1; set { _number1 = value; OnPropertyChanged(); } }

        public bool IsFormula2 { get => _isFormula2; set { _isFormula2 = value; _isNumber2 = !value; OnPropertyChanged(); OnPropertyChanged(nameof(IsNumber2)); } }
        public bool IsNumber2 { get => _isNumber2; set { _isNumber2 = value; _isFormula2 = !value; OnPropertyChanged(); OnPropertyChanged(nameof(IsFormula2)); } }
        public string Formula2 { get => _formula2; set { _formula2 = value; OnPropertyChanged(); } }
        public int VarCount2 { get => _varCount2; set { _varCount2 = value; OnPropertyChanged(); } }
        public long Number2 { get => _number2; set { _number2 = value; OnPropertyChanged(); } }

        public string ComparisonResult { get => _comparisonResult; set { _comparisonResult = value; OnPropertyChanged(); } }
        public string CounterExample { get => _counterExample; set { _counterExample = value; OnPropertyChanged(); } }
        public bool HasCounterExample => !string.IsNullOrEmpty(_counterExample);

        // === Команды ===
        public RelayCommand GenerateCommand => new RelayCommand(async () => await GenerateAsync());
        public RelayCommand ShowTruthTableCommand => new RelayCommand(() => ShowTruthTable());
        public RelayCommand ShowDNFCommand => new RelayCommand(() => ShowDNF());
        public RelayCommand ShowKNFCommand => new RelayCommand(() => ShowKNF());
        public RelayCommand ParseCommand => new RelayCommand(async () => await ParseAsync());
        public RelayCommand CopyResultCommand => new RelayCommand(() => Clipboard.SetText(ResultText));
        public RelayCommand CompareCommand => new RelayCommand(async () => await CompareAsync());

        public RelayCommand Preset1Command => new RelayCommand(() =>
        {
            VariableCount = 3;
            FunctionNumber = 11;
            GenerateAsync().ConfigureAwait(false);
        });

        public RelayCommand Preset2Command => new RelayCommand(() =>
        {
            Formula = "(x1 | x2) -> x3";
            ParseAsync().ConfigureAwait(false);
        });

        public RelayCommand Preset3Command => new RelayCommand(() =>
        {
            Formula1 = "(x1 & !x2) | x3";
            Formula2 = "";
            IsFormula1 = true;
            IsFormula2 = true;
        });

        // Вспомогательные конвертеры
        public class StatusStyleConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is string status)
                {
                    if (status.StartsWith("✅")) return (Style)Application.Current.Resources["StatusSuccess"];
                    if (status.StartsWith("❌")) return (Style)Application.Current.Resources["StatusError"];
                }
                return null;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
        }

        public class ResultColorConverter : IValueConverter
        {
            public static SolidColorBrush Success = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"));
            public static SolidColorBrush Error = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"));

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is string result)
                {
                    return result.Contains("эквивалентны") ? Success : Error;
                }
                return Brushes.Black;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
        }
        // === Логика ===
        private async Task GenerateAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    _currentFunctionNumber = _booleanFunctionService.CreateFromNumber(VariableCount, FunctionNumber);
                });
                ComplexityWarning = _currentFunctionNumber.GetComplexityWarning();
                Status = "✅ Успешно";
                StatusColor = Brushes.Green;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Status = "❌ Ошибка";
                StatusColor = Brushes.Red;
            }
        }

        private void ShowTruthTable()
        {
            if (_currentFunctionNumber == null) return;
            ResultText = FormatTruthTable(_currentFunctionNumber.TruthTable);
        }

        private void ShowDNF()
        {
            if (_currentFunctionNumber == null) return;
            ResultText = _currentFunctionNumber.DNF;
        }

        private void ShowKNF()
        {
            if (_currentFunctionNumber == null) return;
            ResultText = _currentFunctionNumber.KNF;
        }

        private async Task ParseAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    _currentFunctionFormula = _booleanFunctionService.CreateFromFormula(Formula);
                });
                ComplexityWarning = _currentFunctionFormula.GetComplexityWarning();
                Status = "✅ Успешно";
                StatusColor = Brushes.Green;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Status = "❌ Ошибка";
                StatusColor = Brushes.Red;
            }
        }

        private async Task CompareAsync()
        {
            try
            {
                ComparisonResult result = null;

                if (IsNumber1 && IsNumber2)
                {
                    var f1 = _booleanFunctionService.CreateFromNumber(VarCount1, Number1);
                    var f2 = _booleanFunctionService.CreateFromNumber(VarCount2, Number2);
                    result = _comparisonService.CompareFunctions(f1, f2);
                }
                else if (IsFormula1 && IsFormula2)
                {
                    result = await Task.Run(() => _comparisonService.CompareByFormula(Formula1, Formula2));
                }
                else if (IsNumber1 && IsFormula2)
                {
                    result = await Task.Run(() => _comparisonService.CompareByNumberAndFormula(VarCount1, Number1, Formula2));
                }
                else if (IsFormula1 && IsNumber2)
                {
                    result = await Task.Run(() => _comparisonService.CompareByNumberAndFormula(VarCount2, Number2, Formula1));
                }

                ComparisonResult = result?.AreEquivalent == true
                    ? "✅ Функции эквивалентны"
                    : "❌ Функции НЕ эквивалентны";
                CounterExample = result?.CounterExample ?? "";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сравнении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateBinaryRepresentation()
        {
            int rowCount = 1 << VariableCount;
            long maxNum = (1L << rowCount) - 1;

            if (FunctionNumber < 0 || FunctionNumber > maxNum)
            {
                BinaryRepresentation = "Некорректный номер функции";
                return;
            }

            string binary = Convert.ToString(FunctionNumber, 2).PadLeft(rowCount, '0');
            BinaryRepresentation = $"Битовое представление ({rowCount} бит): {binary}";
        }

        private string FormatTruthTable(System.Collections.Generic.List<TruthTableRow> table)
        {
            if (table == null || table.Count == 0) return "Таблица пуста";

            var headers = table[0].Values.Keys;
            var sb = new System.Text.StringBuilder();

            sb.AppendLine(string.Join(" | ", headers) + " | F");
            sb.AppendLine(new string('-', headers.Count * 4 + 3));

            foreach (var row in table)
            {
                var vals = row.Values.Values.Select(v => v ? "1" : "0");
                sb.AppendLine(string.Join(" | ", vals) + " | " + (row.Result ? "1" : "0"));
            }
            return sb.ToString();
        }

        // === INotifyPropertyChanged ===
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    // Простая реализация ICommand
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;

        public RelayCommand(Action execute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter) => _execute();

        // ✅ Реализуем событие — даже если не используем
        public event EventHandler CanExecuteChanged;
    }
}