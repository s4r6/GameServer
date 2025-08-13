using GameServer.Application.Interface;
using GameServer.Infrastracture;
using GameServer.Utility;
using Microsoft.Win32;

namespace GameServer.InterfaceAdapter
{
    public class SyncPositionPresenter
    {
        readonly IRoomRegistry _registry;
        public SyncPositionPresenter(IRoomRegistry registry) 
        { 
            _registry = registry;
        }

        public async Task HandlePositionUpdate(string rawJson, string connectionId)
        {
            var packet = PacketSerializer.Deserialize<PositionUpdate>(rawJson);

            var roomId = packet.Payload.RoomId;
            
            //送信者以外にブロードキャスト
            await _registry.BroadcastAsync(roomId, rawJson, connectionId);
        }
    }
}
