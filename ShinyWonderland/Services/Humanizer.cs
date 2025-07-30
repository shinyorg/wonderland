namespace ShinyWonderland.Services;

[Singleton]
public class Humanizer(
    TimeProvider timeProvider,
    HumanizerLocalized localized
)
{
    public string TimeAgo(DateTimeOffset? date)
    {
        if (date == null)
            return localized.Never;

        var timeSpan = timeProvider.GetLocalNow() - date.Value;
        
        if (timeSpan.TotalSeconds < 60)
        {
            var seconds = (int)timeSpan.TotalSeconds;
            if (seconds <= 0)
                return localized.Never;
            
            return seconds == 1 ? $"1 {localized.Second}" : $"{seconds} {localized.Seconds}";
        }
        
        if (timeSpan.TotalMinutes < 60)
        {
            var minutes = (int)timeSpan.TotalMinutes;
            return minutes == 1 ? $"1 {localized.Minute}" : $"{minutes} {localized.Minutes}";
        }
        
        if (timeSpan.TotalHours < 24)
        {
            var hours = (int)timeSpan.TotalHours;
            return hours == 1 ? $"1 {localized.Hour}" : $"{hours} {localized.Hours}";
        }
        
        if (timeSpan.TotalDays < 30)
        {
            var days = (int)timeSpan.TotalDays;
            return days == 1 ? $"1 {localized.Day}" : $"{days} {localized.Days}";
        }
        
        if (timeSpan.TotalDays < 365)
        {
            var months = (int)(timeSpan.TotalDays / 30);
            return months == 1 ? $"1 {localized.Month}" : $"{months} {localized.Months}";
        }
        
        var years = (int)(timeSpan.TotalDays / 365);
        return years == 1 ? $"1 {localized.Year}" : $"{years} {localized.Years}";
    }
}