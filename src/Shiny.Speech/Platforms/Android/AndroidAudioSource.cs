#if ANDROID
using Android.Media;
using Microsoft.Extensions.Logging;

namespace Shiny.Speech;

public class AndroidAudioSource(ILogger<AndroidAudioSource> logger) : IAudioSource
{
    AudioRecord? audioRecord;
    CancellationTokenSource? recordingCts;
    PipeStream? pipe;

    public Task<System.IO.Stream> StartCaptureAsync(CancellationToken cancellationToken = default)
    {
        const int sampleRate = 16000;
        const ChannelIn channelConfig = ChannelIn.Mono;
        const Encoding audioFormat = Encoding.Pcm16bit;

        var bufferSize = AudioRecord.GetMinBufferSize(sampleRate, channelConfig, audioFormat);
        if (bufferSize <= 0)
            bufferSize = 4096;

        audioRecord = new AudioRecord(
            AudioSource.Mic,
            sampleRate,
            channelConfig,
            audioFormat,
            bufferSize
        );

        if (audioRecord.State != State.Initialized)
            throw new InvalidOperationException("Failed to initialize AudioRecord");

        pipe = new PipeStream();
        recordingCts = new CancellationTokenSource();
        audioRecord.StartRecording();

        // Background task to read audio data
        var token = recordingCts.Token;
        _ = Task.Run(() =>
        {
            var buffer = new byte[bufferSize];
            while (!token.IsCancellationRequested)
            {
                var bytesRead = audioRecord.Read(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    try
                    {
                        pipe.Write(buffer, 0, bytesRead);
                    }
                    catch (ObjectDisposedException)
                    {
                        break;
                    }
                }
            }
        }, token);

        logger.LogDebug("Android audio capture started");
        return Task.FromResult<System.IO.Stream>(pipe);
    }

    public Task StopCaptureAsync()
    {
        recordingCts?.Cancel();
        recordingCts?.Dispose();
        recordingCts = null;

        if (audioRecord != null)
        {
            if (audioRecord.RecordingState == RecordState.Recording)
                audioRecord.Stop();

            audioRecord.Release();
            audioRecord = null;
        }

        pipe?.Dispose();
        pipe = null;

        logger.LogDebug("Android audio capture stopped");
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await StopCaptureAsync();
        GC.SuppressFinalize(this);
    }
}
#endif
