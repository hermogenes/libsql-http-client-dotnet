using FluentAssertions;
using LibSql.Http.Client.Buffer;

namespace LibSql.Http.Client.Tests.Buffer;

public class PooledByteBufferWriterTests
{
    [Fact]
    public void ShouldPreventAdvanceToMoreThanActualBufferLength()
    {
        using var buffer = new PooledByteBufferWriter();

        var action = () => buffer.Advance(Int32.MaxValue);

        action.Should().ThrowExactly<InvalidOperationException>();
    }

    [Fact]
    public void ShouldPreventAdvanceNegativeCount()
    {
        using var buffer = new PooledByteBufferWriter();

        var action = () => buffer.Advance(-1);

        action.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void ShouldPreventBiggerThanArrayMaxLength()
    {
        using var buffer = new PooledByteBufferWriter();

        var action = () =>
        {
            buffer.GetSpan(Array.MaxLength + 1);
            return 1;
        };

        action.Should().ThrowExactly<InvalidOperationException>();
    }

    [Fact]
    public void ShouldGetSpan()
    {
        using var buffer = new PooledByteBufferWriter();

        var span = buffer.GetSpan(257);

        span.Length.Should().BeGreaterThanOrEqualTo(257);
    }
}