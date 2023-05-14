namespace B2Broker.MessageBusBufferization.Implementations;

public class TwoStreamSemaphoreSlimBusMessageWriter
{
    private readonly IBusConnection _connection;
    private readonly MemoryStream _firstBuffer = new();
    private readonly MemoryStream _secondBuffer = new();
    private readonly SemaphoreSlim _semaphore;

    private readonly int _minBufferSize;
    private readonly int _maxBufferSize;

    private bool _isFirstBuffer = true;
    private MemoryStream _currentBuffer;
    private Task _publishTask = Task.CompletedTask;

    public TwoStreamSemaphoreSlimBusMessageWriter(
        IBusConnection connection, 
        int minBufferSize = 1000, 
        int maxBufferSize = 5000)
    {
        _connection = connection;
        _minBufferSize = minBufferSize;
        _maxBufferSize = maxBufferSize;
        _semaphore = new SemaphoreSlim(1);
        _currentBuffer = _firstBuffer;
    }

    public async Task SendMessageAsync(byte[] nextMessage)
    {
        try
        {
            await _semaphore.WaitAsync();
            _currentBuffer.Write(nextMessage, 0, nextMessage.Length);

            if (_publishTask.Status is TaskStatus.Running or TaskStatus.WaitingForActivation
                        && _currentBuffer.Length < _maxBufferSize)
                return;

            if (_currentBuffer.Length >= _minBufferSize)
            {
                await _publishTask;
                _publishTask = Publish(_currentBuffer);
                if (_isFirstBuffer)
                {
                    _isFirstBuffer = false;
                    _currentBuffer = _secondBuffer;
                }
                else
                {
                    _isFirstBuffer = false;
                    _currentBuffer = _firstBuffer;
                }
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    async Task Publish(MemoryStream buffer)
    {
        await _connection.PublishAsync(buffer.ToArray());
        buffer.SetLength(0);
    }
}

