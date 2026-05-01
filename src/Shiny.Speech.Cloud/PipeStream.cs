namespace Shiny.Speech.Cloud;

/// <summary>
/// A thread-safe stream that allows one thread to write and another to read.
/// Used to bridge audio capture (producer) with cloud speech providers (consumer).
/// </summary>
public sealed class PipeStream : Stream
{
    readonly System.IO.Pipelines.Pipe pipe = new();

    public override bool CanRead => true;
    public override bool CanWrite => true;
    public override bool CanSeek => false;
    public override long Length => throw new NotSupportedException();
    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        var memory = pipe.Writer.GetMemory(count);
        buffer.AsSpan(offset, count).CopyTo(memory.Span);
        pipe.Writer.Advance(count);
        pipe.Writer.FlushAsync().AsTask().GetAwaiter().GetResult();
    }

    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        var memory = pipe.Writer.GetMemory(count);
        buffer.AsSpan(offset, count).CopyTo(memory.Span);
        pipe.Writer.Advance(count);
        await pipe.Writer.FlushAsync(cancellationToken);
    }

    public override int Read(byte[] buffer, int offset, int count)
        => ReadAsync(buffer, offset, count).GetAwaiter().GetResult();

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        var result = await pipe.Reader.ReadAsync(cancellationToken);
        var readBuffer = result.Buffer;

        var bytesToCopy = (int)Math.Min(count, readBuffer.Length);
        var slice = readBuffer.Slice(0, bytesToCopy);
        var destination = buffer.AsSpan(offset, bytesToCopy);
        var pos = 0;
        foreach (var segment in slice)
        {
            segment.Span.CopyTo(destination[pos..]);
            pos += segment.Length;
        }
        pipe.Reader.AdvanceTo(readBuffer.GetPosition(bytesToCopy));

        if (result.IsCompleted && bytesToCopy == 0)
            return 0;

        return bytesToCopy;
    }

    public override void Flush() { }
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            pipe.Writer.Complete();
            pipe.Reader.Complete();
        }
        base.Dispose(disposing);
    }
}
