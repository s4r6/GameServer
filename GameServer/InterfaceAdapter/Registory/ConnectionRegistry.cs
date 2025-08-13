using GameServer.InterfaceAdapter.Interface;
using System.Collections.Concurrent;

namespace GameServer.InterfaceAdapter.Registory
{
    public class ConnectionRegistry : IConnectionRegistry
    {
        private readonly ConcurrentDictionary<string, ISocketConnection> _connections = new();

        public void Register(string connectionId, ISocketConnection connection)
        {
            _connections[connectionId] = connection;
        }

        public void Unregister(string connectionId)
        {
            _connections.TryRemove(connectionId, out _);
        }

        public bool Exists(string connectionId)
        {
            return _connections.ContainsKey(connectionId);
        }

        public ISocketConnection? Get(string connectionId)
        {
            return _connections.TryGetValue(connectionId, out var conn) ? conn : null;
        }

        public async Task SendAsync(string connectionId, string message)
        {
            var conn = Get(connectionId);
            if (conn != null)
            {
                await conn.SendAsync(message);
            }
        }
    }
}
