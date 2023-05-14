using B2Broker.MessageBusBufferization;
using BenchmarkDotNet.Running;

BenchmarkRunner.Run<BusBenchmark>();

Console.ReadLine();