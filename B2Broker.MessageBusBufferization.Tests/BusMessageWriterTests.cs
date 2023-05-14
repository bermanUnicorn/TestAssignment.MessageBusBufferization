using AutoFixture;
using B2Broker.MessageBusBufferization.Implementations;
using FluentAssertions;
using Moq;
using Xunit;

namespace B2Broker.MessageBusBufferization.Tests;

public sealed class BusMessageWriterTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task InvalidBusMessageWriter_InSingleThread_SendsBytesCorrectly()
    {
        //  Arrange
        var connectionMock = new Mock<IBusConnection>();
        var writer = new InvalidBusMessageWriter(connectionMock.Object);
        var data = _fixture.CreateMany<byte>(10000).ToArray();

        //  Act
        for (var i = 0; i < data.Length; i += 100)
            await writer.SendMessageAsync(data[i..(i + 100)]);

        //  Assert
        var result = connectionMock.Invocations.SelectMany(x => (IEnumerable<byte>)x.Arguments.Single()).ToArray();
        result.Should().HaveCount(data.Length);
        result.Should().Equal(data);
    }
}