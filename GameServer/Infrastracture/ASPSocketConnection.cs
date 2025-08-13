using System.Net.WebSockets;
using System.Text;
using GameServer.InterfaceAdapter.Interface;

namespace GameServer.Infrastracture
{
    public class ASPSocketConnection : ISocketConnection
    {
        private readonly WebSocket _socket;

        public ASPSocketConnection(WebSocket socket)
        {
            _socket = socket;
        }

        public async Task SendAsync(string message)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            var segment = new ArraySegment<byte>(buffer);
            await _socket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public async Task CloseAsync()
        {
            await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
        }
    }
}
