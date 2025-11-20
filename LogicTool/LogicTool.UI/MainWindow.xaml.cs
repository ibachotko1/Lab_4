using System.Windows;
using LogicTool.Business.Services;
using LogicTool.UI.Services;
using LogicTool.UI.ViewModels;

namespace LogicTool.UI
{
    /// <summary>
    /// Главное окно приложения логических функций.
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Создаёт главное окно и подключает корневую модель.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            // В качестве DataContext используем корневую модель, чтобы XAML оставался декларативным.
            DataContext = new MainViewModel(
                new BooleanFunctionService(),
                new ComparisonService(),
                new ClipboardService());
        }
    }
}