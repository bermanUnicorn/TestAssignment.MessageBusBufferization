# TestAssignment.MessageBusBufferization

Assumptions:
1. Without a correct retention implementation we will never send the last bytes which count are less than minimal buffer size. For testing purposes I used fixed and perfect datasets;
2. Two-stream implementations are also enriched with the maximum buffer size, because otherwise first publish contain 1000 bytes and next one all other bytes;


BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1702/22H2/2022Update/SunValley2)
Intel Core i7-10510U CPU 1.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=7.0.203
  [Host]     : .NET 6.0.16 (6.0.1623.17311), X64 RyuJIT AVX2 [AttachedDebugger]
  DefaultJob : .NET 6.0.16 (6.0.1623.17311), X64 RyuJIT AVX2

1000 messages, 100 bytes each, 50ms overall connection latency

|                 Method |    Mean |    Error |   StdDev | Completed Work Items | Lock Contentions |  Allocated |
|----------------------- |--------:|---------:|---------:|---------------------:|-----------------:|-----------:|
|           SingleThread | 6.281 s | 0.0380 s | 0.0355 s |             100.0000 |                - |  208.57 KB |
|          SemaphoreSlim | 6.276 s | 0.0283 s | 0.0251 s |            1100.0000 |           6.0000 | 1128.73 KB |
| TwoStreamSemaphoreSlim | 1.253 s | 0.0142 s | 0.0118 s |            1021.0000 |                - | 1056.11 KB |
|                Channel | 6.332 s | 0.0183 s | 0.0162 s |             112.0000 |           4.0000 |  244.94 KB |
|       TwoStreamChannel | 1.318 s | 0.0169 s | 0.0150 s |              32.0000 |           3.0000 |   89.41 KB |

1000 messages, 100 bytes each, 10ms overall connection latency

|                 Method |       Mean |    Error |   StdDev | Completed Work Items | Lock Contentions |  Allocated |
|----------------------- |-----------:|---------:|---------:|---------------------:|-----------------:|-----------:|
|           SingleThread | 1,559.8 ms |  7.04 ms |  5.88 ms |             100.0000 |                - |  208.57 KB |
|          SemaphoreSlim | 1,585.2 ms | 25.24 ms | 23.61 ms |            1100.0000 |           4.0000 | 1129.07 KB |
| TwoStreamSemaphoreSlim |   310.0 ms |  1.67 ms |  1.40 ms |            1021.0000 |           2.0000 |  1058.1 KB |
|                Channel | 1,575.7 ms |  5.89 ms |  5.22 ms |             112.0000 |           3.0000 |  244.75 KB |
|       TwoStreamChannel |   329.3 ms |  6.56 ms |  6.44 ms |              31.0000 |           3.0000 |  102.54 KB |

1000 messages, 500 bytes each, 10ms overall connection latency

|                 Method |    Mean |    Error |   StdDev | Completed Work Items | Lock Contentions |  Allocated |
|----------------------- |--------:|---------:|---------:|---------------------:|-----------------:|-----------:|
|           SingleThread | 7.902 s | 0.0671 s | 0.0627 s |             500.0000 |           1.0000 | 1011.55 KB |
|          SemaphoreSlim | 7.864 s | 0.0317 s | 0.0281 s |            1508.0000 |           2.0000 | 1795.83 KB |
| TwoStreamSemaphoreSlim | 1.565 s | 0.0136 s | 0.0120 s |            1109.0000 |           1.0000 | 1143.02 KB |
|                Channel | 7.889 s | 0.0599 s | 0.0531 s |             511.0000 |           2.0000 | 1044.45 KB |
|       TwoStreamChannel | 1.581 s | 0.0161 s | 0.0134 s |             112.0000 |           4.0000 |   184.4 KB |
