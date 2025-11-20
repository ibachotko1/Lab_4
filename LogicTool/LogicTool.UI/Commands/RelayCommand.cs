using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LogicTool.UI.Commands
{
    /// <summary>
    /// Универсальная реализация ICommand с поддержкой синхронных и асинхронных действий.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Func<object, bool> _canExecute;
        private readonly Func<object, Task> _executeAsync;
        private readonly Action<object> _executeSync;

        /// <summary>
        /// Создаёт команду на основе синхронного действия без параметра.
        /// </summary>
        public RelayCommand(Action execute, Func<bool> canExecute = null)
            : this(execute != null ? new Action<object>(_ => execute()) : null,
                  canExecute != null ? new Func<object, bool>(_ => canExecute()) : null)
        {
        }

        /// <summary>
        /// Создаёт команду на основе асинхронного действия без параметра.
        /// </summary>
        public RelayCommand(Func<Task> executeAsync, Func<bool> canExecute = null)
            : this(executeAsync != null ? new Func<object, Task>(_ => executeAsync()) : null,
                  canExecute != null ? new Func<object, bool>(_ => canExecute()) : null)
        {
        }

        /// <summary>
        /// Создаёт команду на основе синхронного действия с параметром.
        /// </summary>
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _executeSync = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Создаёт команду на основе асинхронного действия с параметром.
        /// </summary>
        public RelayCommand(Func<object, Task> executeAsync, Func<object, bool> canExecute = null)
        {
            _executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Событие обновления доступности выполнения команды.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Возвращает способность команды выполниться с указанным параметром.
        /// </summary>
        public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;

        /// <summary>
        /// Запускает действие команды.
        /// </summary>
        public async void Execute(object parameter)
        {
            if (!CanExecute(parameter))
            {
                return;
            }

            if (_executeAsync != null)
            {
                await _executeAsync(parameter);
            }
            else
            {
                _executeSync?.Invoke(parameter);
            }
        }

        /// <summary>
        /// Извещает подписчиков о том, что доступность команды изменилась.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
