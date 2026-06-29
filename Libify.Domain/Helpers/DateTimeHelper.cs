namespace Libify.Domain.Helpers
{
    public static class DateTimeHelper
    {
        private static readonly TimeSpan Offset = TimeSpan.FromHours(-3);

        /// <summary>
        /// Retorna DateTime.UtcNow ajustado para -3 horas (fuso horário Brasil).
        /// </summary>
        public static DateTime UtcNow => DateTime.UtcNow.Add(Offset);

        /// <summary>
        /// Retorna DateTime.Now ajustado para -3 horas (fuso horário Brasil).
        /// </summary>
        public static DateTime Now => DateTime.Now.Add(Offset);
    }
}
