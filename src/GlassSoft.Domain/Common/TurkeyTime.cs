namespace GlassSoft.Domain.Common;

/// <summary>
/// İstanbul / Türkiye saati (UTC+3) için yardımcı sınıf.
/// Sunucu UTC veya farklı bir timezone'da olsa bile her zaman İstanbul saatini döner.
/// </summary>
public static class TurkeyTime
{
    private static readonly TimeZoneInfo _tz = GetTimeZone();

    private static TimeZoneInfo GetTimeZone()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time"); }
        catch
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul"); }
            catch { return TimeZoneInfo.CreateCustomTimeZone("UTC+3", TimeSpan.FromHours(3), "UTC+3", "UTC+3"); }
        }
    }

    public static DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _tz);

    public static DateTime Today => Now.Date;

    /// <summary>
    /// Eğer verilen tarihte saat bilgisi yoksa (00:00:00), şu anki Türkiye saatini ekler.
    /// Formlardan gelen tarih-saat bilgisi kaybolmasın diye kullanılır.
    /// </summary>
    public static DateTime WithCurrentTime(DateTime date)
    {
        if (date.TimeOfDay == TimeSpan.Zero)
        {
            var now = Now;
            return date.Date.Add(now.TimeOfDay);
        }
        return date;
    }
}
