using CommunityToolkit.Maui.Media;

namespace ShinyWonderland;


public static class Extensions
{
    public static async Task<string?> WaitForSpeechToText(
        this ISpeechToText speechToText,
        CancellationToken cancellationToken = default
    )
    {
        var tcs = new TaskCompletionSource<string?>();
        using var ctr = cancellationToken.Register(() => tcs.TrySetResult(null));

        void OnCompleted(object? sender, SpeechToTextRecognitionResultCompletedEventArgs e)
            => tcs.TrySetResult(e.RecognitionResult.Text);

        speechToText.RecognitionResultCompleted += OnCompleted;
        try
        {
            await speechToText.StartListenAsync(new SpeechToTextOptions
            {
                Culture = System.Globalization.CultureInfo.CurrentCulture,
                ShouldReportPartialResults = false,
                AutoStopSilenceTimeout = TimeSpan.FromSeconds(2)
            }, cancellationToken);

            var result = await tcs.Task.ConfigureAwait(false);
            return result;
        }
        finally
        {
            speechToText.RecognitionResultCompleted -= OnCompleted;
            await speechToText.StopListenAsync();
        }
    }
    
    public static async Task<bool> IsWithinPark(
        this IGpsManager gpsManager, 
        ParkOptions parkOptions, 
        CancellationToken cancellationToken = default
    )
    {
#if DEBUG
        return true;
#else
        var reading = await gpsManager
            .GetCurrentPosition()
            .Timeout(TimeSpan.FromSeconds(15))
            .ToTask(cancellationToken);

        return reading.IsWithinPark(parkOptions);
#endif
    }


    public static bool IsWithinPark(this GpsReading reading, ParkOptions parkOptions)
    {
#if DEBUG
        return true;
#else
        var distance = reading.Position.GetDistanceTo(parkOptions.CenterOfPark);
        var within = distance.TotalKilometers <= parkOptions.NotificationDistance.TotalKilometers;
        return within;
#endif
    }
}