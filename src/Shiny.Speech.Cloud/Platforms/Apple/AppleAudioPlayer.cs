#if APPLE
using AVFoundation;
using Foundation;
using Microsoft.Extensions.Logging;

namespace Shiny.Speech.Cloud;

public class AppleAudioPlayer(ILogger<AppleAudioPlayer> logger) : IAudioPlayer
{
    AVAudioPlayer? player;
    TaskCompletionSource? playbackTcs;

    public bool IsPlaying => player?.Playing ?? false;

    public async Task PlayAsync(Stream audioStream, CancellationToken cancellationToken = default)
    {
        await StopAsync();

        using var ms = new MemoryStream();
        await audioStream.CopyToAsync(ms, cancellationToken);
        var data = NSData.FromArray(ms.ToArray());

        player = AVAudioPlayer.FromData(data);
        if (player == null)
            throw new InvalidOperationException("Failed to create audio player from data");

        var session = AVAudioSession.SharedInstance();
        session.SetCategory(AVAudioSessionCategory.Playback, AVAudioSessionCategoryOptions.DefaultToSpeaker, out _);
        session.SetActive(true, out _);

        playbackTcs = new TaskCompletionSource();
        player.FinishedPlaying += OnFinishedPlaying;

        using var reg = cancellationToken.Register(() =>
        {
            player?.Stop();
            playbackTcs?.TrySetResult();
        });

        player.Play();
        logger.LogDebug("Apple audio playback started");

        await playbackTcs.Task;
        logger.LogDebug("Apple audio playback finished");

        player.FinishedPlaying -= OnFinishedPlaying;
    }

    void OnFinishedPlaying(object? sender, AVStatusEventArgs e)
        => playbackTcs?.TrySetResult();

    public Task StopAsync()
    {
        if (player != null)
        {
            player.Stop();
            player.Dispose();
            player = null;
            playbackTcs?.TrySetResult();
            logger.LogDebug("Apple audio playback stopped");
        }
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        player?.Stop();
        player?.Dispose();
        player = null;
        return ValueTask.CompletedTask;
    }
}
#endif
