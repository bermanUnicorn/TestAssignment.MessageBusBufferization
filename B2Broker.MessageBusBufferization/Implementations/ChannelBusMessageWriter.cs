using System.Threading.Channels;

namespace B2Broker.MessageBusBufferization.Implementations;

public class ChannelBusMessageWriter
{
    private readonly IBusConnection _connection;
    private readonly MemoryStream _buffer = new();
    private readonly Channel<byte[]> _channel;
    private readonly Task readerTask;

    private readonly int _minBufferSize;

    public ChannelBusMessageWriter(IBusConnection connection, int minBufferSize = 1000)
    {
        _connection = connection;
        _minBufferSize = minBufferSize;
        _channel = Channel.CreateUnbounded<byte[]>(new UnboundedChannelOptions
        {
            AllowSynchronousContinuations = true
        });
        readerTask = ReadAsync();
    }

    public ValueTask SendMessageAsync(byte[] nextMessage)
        => _channel.Writer.WriteAsync(nextMessage);

    private async Task ReadAsync()
    {
        await Task.Yield();
        while (true)
        {
            if (!await _channel.Reader.WaitToReadAsync())
            {
                await Publish(_buffer);
                break;
            }

            if (_channel.Reader.TryRead(out var item))
            {
                _buffer.Write(item, 0, item.Length);
                if (_buffer.Length >= _minBufferSize)
                    await Publish(_buffer);
            }
        }
    }

    async Task Publish(MemoryStream buffer)
    {
        await _connection.PublishAsync(buffer.ToArray());
        buffer.SetLength(0);
    }

    public async Task CompleteAsync()
    {
        _channel.Writer.Complete();
        await readerTask;
    }
}
