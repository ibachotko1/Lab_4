namespace LogicTool.UI.Services
{
    /// <summary>
    /// Абстракция для взаимодействия с буфером обмена.
    /// </summary>
    public interface IClipboardService
    {
        /// <summary>
        /// Копирует текст в буфер обмена.
        /// </summary>
        void SetText(string text);
    }
}

