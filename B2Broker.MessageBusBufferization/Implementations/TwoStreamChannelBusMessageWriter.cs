using System.Threading.Channels;

namespace B2Broker.MessageBusBufferization.Implementations;

public class TwoStreamChannelBusMessageWriter
{
    private readonly IBusConnection _connection;
    private readonly MemoryStream _firstBuffer = new();
    private readonly MemoryStream _secondBuffer = new();
    private readonly Channel<byte[]> _channel;
    private readonly Task readerTask;

    private readonly int _minBufferSize;
    private readonly int _maxBufferSize;

    private bool _isFirstBuffer = true;
    private Task _publishTask = Task.CompletedTask;

    public TwoStreamChannelBusMessageWriter(
        IBusConnection connection, 
        int minBufferSize = 1000, 
        int maxBufferSize = 5000)
    {
        _connection = connection;
        _minBufferSize = minBufferSize;
        _maxBufferSize = maxBufferSize;
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
        MemoryStream currentBuffer = _firstBuffer;
        while (true)
        {
            if (!await _channel.Reader.WaitToReadAsync())
            {
                await _publishTask;
                if (currentBuffer.Length > 0)
                    await Publish(currentBuffer);
                break;
            }

            if (_channel.Reader.TryRead(out var item))
            {
                currentBuffer.Write(item, 0, item.Length);

                if (_publishTask.Status is TaskStatus.Running or TaskStatus.WaitingForActivation
                    && currentBuffer.Length < _maxBufferSize)
                    continue;

                if (currentBuffer.Length >= _minBufferSize)
                {
                    await _publishTask;
                    _publishTask = Publish(currentBuffer);
                    if (_isFirstBuffer)
                    {
                        _isFirstBuffer = false;
                        currentBuffer = _secondBuffer;
                    }
                    else
                    {
                        _isFirstBuffer = false;
                        currentBuffer = _firstBuffer;
                    }
                }
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
