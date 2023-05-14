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

    [Fact]
    public async Task SemaphoreSlimBusMessageWriter_InMultipleThreads_SendsBytesCorrectly()
    {
        //  Arrange
        var connectionMock = new Mock<IBusConnection>();
        var writer = new SemaphoreSlimBusMessageWriter(connectionMock.Object);
        var data = _fixture.CreateMany<byte>(10000).ToArray();
        var dataParts = new List<byte[]>(100);
        for (var i = 0; i < data.Length; i += 100)
            dataParts.Add(data[i..(i + 100)]);

        //  Act
        await Parallel.ForEachAsync(dataParts, new ParallelOptions()
        {
            MaxDegreeOfParallelism = 10
        },
        async (part, _) => await writer.SendMessageAsync(part));

        //  Assert
        var result = connectionMock.Invocations.SelectMany(x => (IEnumerable<byte>)x.Arguments.Single()).ToArray();
        result.Should().HaveCount(data.Length);
        dataParts.ForEach(part => part.Should().BeSubsetOf(result));
    }
}