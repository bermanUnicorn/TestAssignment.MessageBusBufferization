namespace B2Broker.MessageBusBufferization.Implementations;

public class SemaphoreSlimBusMessageWriter
{
    private readonly IBusConnection _connection;
    private readonly MemoryStream _buffer = new();
    private readonly SemaphoreSlim _semaphore;

    private readonly int _minBufferSize;

    public SemaphoreSlimBusMessageWriter(IBusConnection connection, int minBufferSize = 1000)
    {
        _connection = connection;
        _minBufferSize = minBufferSize;
        _semaphore = new SemaphoreSlim(1);
    }

    public async Task SendMessageAsync(byte[] nextMessage)
    {
        try
        {
            await _semaphore.WaitAsync();
            _buffer.Write(nextMessage, 0, nextMessage.Length);
            if (_buffer.Length >= _minBufferSize)
            {
                await _connection.PublishAsync(_buffer.ToArray());
                _buffer.SetLength(0);
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }
}

