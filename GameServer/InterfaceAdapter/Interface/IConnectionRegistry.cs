namespace GameServer.InterfaceAdapter.Interface
{
    public interface ISocketConnection
    {
        Task SendAsync(string message);
        Task CloseAsync();
    }

    public interface IConnectionRegistry
    {
        void Register(string connectionId, ISocketConnection connection);
        void Unregister(string connectionId);
        bool Exists(string connectionId);
        ISocketConnection? Get(string connectionId);
        Task SendAsync(string connectionId, string message);
    }
}
