namespace B2Broker.MessageBusBufferization.Implementations;

public interface IBusConnection
{
    Task PublishAsync(byte[] nextMessage);
}