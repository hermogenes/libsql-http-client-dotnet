using System.Buffers;

namespace LibSql.Http.Client.Buffer;

internal sealed class PooledByteBufferWriter(int initialCapacity) : IBufferWriter<byte>, IDisposable
{
    private const int MinimumBufferSize = 256;

    private byte[] _buffer = ArrayPool<byte>.Shared.Rent(Math.Max(initialCapacity, MinimumBufferSize));

    public PooledByteBufferWriter() : this(MinimumBufferSize)
    {
    }

    public ReadOnlyMemory<byte> WrittenMemory => _buffer.AsMemory(0, WrittenCount);

    private int WrittenCount { get; set; }

    private int FreeCapacity => _buffer.Length - WrittenCount;

    public void Advance(int count)
    {
        if (count < 0)
            throw new ArgumentException(null, nameof(count));
        if (WrittenCount > _buffer.Length - count)
            ThrowInvalidOperationException_AdvancedTooFar(_buffer.Length);
        WrittenCount += count;
    }

    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        CheckAndResizeBuffer(sizeHint);
        return _buffer.AsMemory(WrittenCount);
    }

    public Span<byte> GetSpan(int sizeHint = 0)
    {
        CheckAndResizeBuffer(sizeHint);
        return _buffer.AsSpan(WrittenCount);
    }

    public void Dispose()
    {
        Clear();
        ArrayPool<byte>.Shared.Return(_buffer);
    }

    public ReadOnlySpan<byte> AsSpan(long[] marks) => _buffer.AsSpan((int)marks[0], (int)marks[1]);

    private void Clear()
    {
        _buffer.AsSpan(0, WrittenCount).Clear();
        WrittenCount = 0;
    }

    private void CheckAndResizeBuffer(int sizeHint)
    {
        if (sizeHint < 0) throw new ArgumentException(nameof(sizeHint));

        sizeHint = Math.Max(sizeHint, 1);

        if (sizeHint <= FreeCapacity)
            return;

        var length = _buffer.Length;

        var val1 = Math.Max(sizeHint, length == 0 ? MinimumBufferSize : length);

        var newSize = length + val1;

        if ((uint)newSize > int.MaxValue)
        {
            var capacity = (uint)(length - FreeCapacity + sizeHint);
            if (capacity > Array.MaxLength)
                ThrowOutOfMemoryException(capacity);
            newSize = Array.MaxLength;
        }

        var oldBuffer = _buffer;

        _buffer = ArrayPool<byte>.Shared.Rent(newSize);

        var oldBufferAsSpan = oldBuffer.AsSpan(0, WrittenCount);

        oldBufferAsSpan.CopyTo(_buffer);
        oldBufferAsSpan.Clear();
        ArrayPool<byte>.Shared.Return(oldBuffer);
    }

    private static void ThrowInvalidOperationException_AdvancedTooFar(int capacity)
    {
        throw new InvalidOperationException($"BufferWriterAdvancedTooFar. Capacity:{capacity}");
    }

    private static void ThrowOutOfMemoryException(uint capacity)
    {
        throw new InvalidOperationException($"Buffer maximum size exceeded. Capacity:{capacity}");
    }
}
