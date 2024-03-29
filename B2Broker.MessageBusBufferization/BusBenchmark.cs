﻿using B2Broker.MessageBusBufferization.Implementations;
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

    [Benchmark]
    public async Task SemaphoreSlim()
    {
        var writer = new SemaphoreSlimBusMessageWriter(_busConnection);
        await Parallel.ForEachAsync(_dataParts, new ParallelOptions()
        {
            MaxDegreeOfParallelism = 10
        },
        async (part, _) => await writer.SendMessageAsync(part));
    }

    [Benchmark]
    public async Task TwoStreamSemaphoreSlim()
    {
        var writer = new TwoStreamSemaphoreSlimBusMessageWriter(_busConnection);
        await Parallel.ForEachAsync(_dataParts, new ParallelOptions()
        {
            MaxDegreeOfParallelism = 10
        },
        async (part, _) => await writer.SendMessageAsync(part));
    }

    [Benchmark]
    public async Task Channel()
    {
        var writer = new ChannelBusMessageWriter(_busConnection);
        await Parallel.ForEachAsync(_dataParts, new ParallelOptions()
        {
            MaxDegreeOfParallelism = 10
        },
        async (part, _) => await writer.SendMessageAsync(part));
        await writer.CompleteAsync();
    }

    [Benchmark]
    public async Task TwoStreamChannel()
    {
        var writer = new TwoStreamChannelBusMessageWriter(_busConnection);
        await Parallel.ForEachAsync(_dataParts, new ParallelOptions()
        {
            MaxDegreeOfParallelism = 10
        },
        async (part, _) => await writer.SendMessageAsync(part));
        await writer.CompleteAsync();
    }
}