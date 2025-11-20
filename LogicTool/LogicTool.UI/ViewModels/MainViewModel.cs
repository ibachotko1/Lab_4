using LogicTool.Business.Interfaces;
using LogicTool.UI.Services;

namespace LogicTool.UI.ViewModels
{
    /// <summary>
    /// Корневая модель представления для главного окна.
    /// </summary>
    public class MainViewModel : BaseViewModel
    {
        public MainViewModel(
            IBooleanFunctionService functionService,
            IComparisonService comparisonService,
            IClipboardService clipboardService)
        {
            // Создаём три независимые вкладки и сразу прогоняем пресеты
            // — интерфейс открывается уже с готовыми примерами из задания.
            NumberTab = new NumberTabViewModel(functionService, clipboardService);
            FormulaTab = new FormulaTabViewModel(functionService, clipboardService);
            ComparisonTab = new ComparisonTabViewModel(functionService, comparisonService);

            NumberTab.ApplyPreset();
            FormulaTab.ApplyPreset();
            ComparisonTab.ApplyPreset();
        }

        /// <summary>
        /// Вкладка работы по номеру.
        /// </summary>
        public NumberTabViewModel NumberTab { get; }

        /// <summary>
        /// Вкладка работы по формуле.
        /// </summary>
        public FormulaTabViewModel FormulaTab { get; }

        /// <summary>
        /// Вкладка сравнения функций.
        /// </summary>
        public ComparisonTabViewModel ComparisonTab { get; }
    }
}

