#if ANDROID
using Android.Media;
using Microsoft.Extensions.Logging;
using Stream = System.IO.Stream;

namespace Shiny.Speech.Cloud;

public class AndroidAudioPlayer(ILogger<AndroidAudioPlayer> logger) : IAudioPlayer
{
    MediaPlayer? mediaPlayer;
    TaskCompletionSource? playbackTcs;

    public bool IsPlaying => mediaPlayer?.IsPlaying ?? false;

    public async Task PlayAsync(Stream audioStream, CancellationToken cancellationToken = default)
    {
        await StopAsync();

        var tempFile = Path.Combine(
            Android.App.Application.Context.CacheDir!.AbsolutePath,
            $"tts_{Guid.NewGuid()}.mp3"
        );

        try
        {
            await using (var fs = File.Create(tempFile))
                await audioStream.CopyToAsync(fs, cancellationToken);

            mediaPlayer = new MediaPlayer();
            playbackTcs = new TaskCompletionSource();

            mediaPlayer.Completion += OnCompletion;
            mediaPlayer.Error += OnError;

            await mediaPlayer.SetDataSourceAsync(tempFile);
            mediaPlayer.Prepare();

            using var reg = cancellationToken.Register(() =>
            {
                mediaPlayer?.Stop();
                playbackTcs?.TrySetResult();
            });

            mediaPlayer.Start();
            logger.LogDebug("Android audio playback started");

            await playbackTcs.Task;
            logger.LogDebug("Android audio playback finished");
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    void OnCompletion(object? sender, EventArgs e)
        => playbackTcs?.TrySetResult();

    void OnError(object? sender, MediaPlayer.ErrorEventArgs e)
    {
        logger.LogWarning("Android audio playback error: {What} {Extra}", e.What, e.Extra);
        playbackTcs?.TrySetException(new InvalidOperationException($"Audio playback error: {e.What}"));
    }

    public Task StopAsync()
    {
        if (mediaPlayer != null)
        {
            if (mediaPlayer.IsPlaying)
                mediaPlayer.Stop();

            mediaPlayer.Release();
            mediaPlayer.Dispose();
            mediaPlayer = null;
            playbackTcs?.TrySetResult();
            logger.LogDebug("Android audio playback stopped");
        }
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        mediaPlayer?.Release();
        mediaPlayer?.Dispose();
        mediaPlayer = null;
        return ValueTask.CompletedTask;
    }
}
#endif
