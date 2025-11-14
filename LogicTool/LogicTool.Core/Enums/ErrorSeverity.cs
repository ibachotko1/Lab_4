namespace LogicTool.Core.Enums
{
    /// <summary>
    /// Уровни серьезности ошибок при обработке логических формул
    /// </summary>
    public enum ErrorSeverity
    {
        /// <summary>
        /// Информационное сообщение
        /// </summary>
        Info,

        /// <summary>
        /// Предупреждение
        /// </summary>
        Warning,

        /// <summary>
        /// Ошибка
        /// </summary>
        Error,

        /// <summary>
        /// Критическая ошибка
        /// </summary>
        Critical
    }
}