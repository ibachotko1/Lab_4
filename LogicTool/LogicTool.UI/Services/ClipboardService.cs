using System;
using System.Windows;

namespace LogicTool.UI.Services
{
    /// <summary>
    /// Реализация сервиса работы с буфером обмена WPF.
    /// </summary>
    public class ClipboardService : IClipboardService
    {
        /// <summary>
        /// Потокобезопасно копирует текст в буфер обмена приложения.
        /// </summary>
        public void SetText(string text)
        {
            if (Application.Current == null)
            {
                return;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    Clipboard.SetText(text ?? string.Empty);
                }
                catch
                {
                    // Игнорируем системные исключения буфера обмена
                }
            });
        }
    }
}

