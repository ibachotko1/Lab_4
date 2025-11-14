namespace LogicTool.Core.Enums
{
    /// <summary>
    /// Типы нормальных форм булевых функций
    /// </summary>
    public enum NormalFormType
    {
        /// <summary>
        /// Дизъюнктивная нормальная форма
        /// </summary>
        DNF,

        /// <summary>
        /// Конъюнктивная нормальная форма  
        /// </summary>
        KNF,

        /// <summary>
        /// Совершенная дизъюнктивная нормальная форма
        /// </summary>
        PerfectDNF,

        /// <summary>
        /// Совершенная конъюнктивная нормальная форма
        /// </summary>
        PerfectKNF
    }
}