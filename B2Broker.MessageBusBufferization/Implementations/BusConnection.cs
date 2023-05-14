namespace B2Broker.MessageBusBufferization.Implementations;

public class BusConnection : IBusConnection
{
    private readonly int _delay;

    public BusConnection(int delay)
        => _delay = delay;

    public async Task PublishAsync(byte[] nextMessage) => await Task.Delay(_delay);
}