namespace B2Broker.MessageBusBufferization.Implementations;

public class InvalidBusMessageWriter
{
    private readonly IBusConnection _connection;
    private readonly MemoryStream _buffer = new();

    public InvalidBusMessageWriter(IBusConnection connection)
        => _connection = connection;

    // how to make this method thread safe?
    public async Task SendMessageAsync(byte[] nextMessage)
    {
        _buffer.Write(nextMessage, 0, nextMessage.Length);
        if (_buffer.Length >= 1000)
        {
            await _connection.PublishAsync(_buffer.ToArray());
            _buffer.SetLength(0);
        }
    }
}