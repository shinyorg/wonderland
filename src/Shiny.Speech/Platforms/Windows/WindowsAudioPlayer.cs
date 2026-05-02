#if WINDOWS
using Microsoft.Extensions.Logging;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage.Streams;

namespace Shiny.Speech;

public class WindowsAudioPlayer(ILogger<WindowsAudioPlayer> logger) : IAudioPlayer
{
    MediaPlayer? mediaPlayer;
    TaskCompletionSource? playbackTcs;

    public bool IsPlaying => mediaPlayer?.PlaybackSession?.PlaybackState == MediaPlaybackState.Playing;

    public async Task PlayAsync(Stream audioStream, CancellationToken cancellationToken = default)
    {
        await StopAsync();

        var ras = new InMemoryRandomAccessStream();
        await audioStream.CopyToAsync(ras.AsStreamForWrite(), cancellationToken);
        ras.Seek(0);

        mediaPlayer = new MediaPlayer();
        mediaPlayer.Source = MediaSource.CreateFromStream(ras, "audio/mpeg");

        playbackTcs = new TaskCompletionSource();
        mediaPlayer.MediaEnded += OnMediaEnded;
        mediaPlayer.MediaFailed += OnMediaFailed;

        using var reg = cancellationToken.Register(() =>
        {
            mediaPlayer?.Pause();
            playbackTcs?.TrySetResult();
        });

        mediaPlayer.Play();
        logger.LogDebug("Windows audio playback started");

        await playbackTcs.Task;
        logger.LogDebug("Windows audio playback finished");
    }

    void OnMediaEnded(MediaPlayer sender, object args)
        => playbackTcs?.TrySetResult();

    void OnMediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
    {
        logger.LogWarning("Windows audio playback failed: {Error}", args.ErrorMessage);
        playbackTcs?.TrySetException(new InvalidOperationException($"Audio playback failed: {args.ErrorMessage}"));
    }

    public Task StopAsync()
    {
        if (mediaPlayer != null)
        {
            mediaPlayer.Pause();
            mediaPlayer.Dispose();
            mediaPlayer = null;
            playbackTcs?.TrySetResult();
            logger.LogDebug("Windows audio playback stopped");
        }
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        mediaPlayer?.Dispose();
        mediaPlayer = null;
        return ValueTask.CompletedTask;
    }
}
#endif
