using B2Broker.MessageBusBufferization.Implementations;
using BenchmarkDotNet.Attributes;

namespace B2Broker.MessageBusBufferization;

[MemoryDiagnoser]
[ThreadingDiagnoser]
public class BusBenchmark
{
    private static readonly IBusConnection _busConnection = new BusConnection(50);
    private static readonly IReadOnlyCollection<byte[]> _dataParts = Enumerable
        .Repeat(Enumerable.Range(0, 100).Select(x => (byte)x).ToArray(), 1000)
        .ToArray();

    [Benchmark]
    public async Task SingleThread()
    {
        var writer = new InvalidBusMessageWriter(_busConnection);
        foreach (var dataPart in _dataParts)
            await writer.SendMessageAsync(dataPart);
    }
}