namespace IstanbulSenin.HELPER
{
    public static class DateTimeHelper
    {
        private static readonly TimeZoneInfo TurkeyTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");

        /// Türkiye saat diliminde şu anki zamanı döndürür (UTC+3)

        public static DateTime GetTurkeyNow()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TurkeyTimeZone);
        }

        public static DateTime ConvertToTurkey(DateTime utcDateTime)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, TurkeyTimeZone);
        }

        public static DateTime GetTurkeyEndOfDay(DateTime date)
        {
            return date.Date.AddDays(1).AddSeconds(-1);
        }


        public static DateTime GetTurkeyStartOfDay(DateTime date)
        {
            return date.Date;
        }
    }
}
