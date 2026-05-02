#if WINDOWS
using Microsoft.Extensions.Logging;
using Windows.Media;
using Windows.Media.Audio;
using Windows.Media.MediaProperties;
using Windows.Media.Render;

namespace Shiny.Speech;

public class WindowsAudioSource(ILogger<WindowsAudioSource> logger) : IAudioSource
{
    AudioGraph? audioGraph;
    AudioDeviceInputNode? inputNode;
    AudioFrameOutputNode? outputNode;
    PipeStream? pipe;

    public async Task<Stream> StartCaptureAsync(CancellationToken cancellationToken = default)
    {
        var encoding = AudioEncodingProperties.CreatePcm(16000, 1, 16);

        var settings = new AudioGraphSettings(AudioRenderCategory.Speech)
        {
            EncodingProperties = encoding,
            DesiredRenderDeviceAudioProcessing = AudioProcessing.Default
        };

        var graphResult = await AudioGraph.CreateAsync(settings);
        if (graphResult.Status != AudioGraphCreationStatus.Success)
            throw new InvalidOperationException($"Failed to create audio graph: {graphResult.Status}");

        audioGraph = graphResult.Graph;

        var inputResult = await audioGraph.CreateDeviceInputNodeAsync(MediaCategory.Speech, encoding);
        if (inputResult.Status != AudioDeviceNodeCreationStatus.Success)
            throw new InvalidOperationException($"Failed to create input node: {inputResult.Status}");

        inputNode = inputResult.DeviceInputNode;
        outputNode = audioGraph.CreateFrameOutputNode(encoding);
        inputNode.AddOutgoingConnection(outputNode);

        pipe = new PipeStream();

        audioGraph.QuantumStarted += (graph, _) =>
        {
            var frame = outputNode.GetFrame();
            ProcessAudioFrame(frame);
        };

        audioGraph.Start();
        logger.LogDebug("Windows audio capture started");
        return pipe;
    }

    void ProcessAudioFrame(AudioFrame frame)
    {
        using var buffer = frame.LockBuffer(AudioBufferAccessMode.Read);
        using var reference = buffer.CreateReference();
        unsafe
        {
            ((Windows.Foundation.IMemoryBufferByteAccess)reference).GetBuffer(out var dataPtr, out var capacity);
            if (capacity > 0)
            {
                var data = new byte[capacity];
                System.Runtime.InteropServices.Marshal.Copy((IntPtr)dataPtr, data, 0, (int)capacity);
                try
                {
                    pipe?.Write(data, 0, data.Length);
                }
                catch (ObjectDisposedException)
                {
                    // Stream was disposed
                }
            }
        }
    }

    public Task StopCaptureAsync()
    {
        audioGraph?.Stop();
        inputNode?.Dispose();
        outputNode?.Dispose();
        audioGraph?.Dispose();
        pipe?.Dispose();

        inputNode = null;
        outputNode = null;
        audioGraph = null;
        pipe = null;

        logger.LogDebug("Windows audio capture stopped");
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await StopCaptureAsync();
        GC.SuppressFinalize(this);
    }
}
#endif
