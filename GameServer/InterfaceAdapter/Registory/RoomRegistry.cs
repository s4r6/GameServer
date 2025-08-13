using System.Collections.Concurrent;
using GameServer.Application.Interface;
using GameServer.Domain;
using GameServer.Infrastracture.Factory;
using GameServer.InterfaceAdapter.Interface;
using GameServer.InterfaceAdapter.Registory;

namespace GameServer.Infrastracture.Repository
{
    public class RoomRegistry : IRoomRegistry
    {
        readonly IConnectionRegistry connRegistry;
        readonly Dictionary<string, Room> _rooms = new();

        public RoomRegistry(IConnectionRegistry connRegistry) 
        { 
            this.connRegistry = connRegistry;
        }

        public Room? Get(string roomId)
        {
            return _rooms.TryGetValue(roomId, out var room) ? room : null;
        }

        public Room? GetByPlayerId(string playerId)
        {
            return _rooms.Values.ToList().Find(r => r.GetPlayer(playerId) != null);
        }

        public List<Room> GetAll() => _rooms.Values.ToList();

        public bool Exists(string roomId)
        {
            return _rooms.ContainsKey(roomId);
        }

        public void Save(Room room)
        {
            _rooms[room.Id] = room;
        }

        public void Remove(string roomId)
        {
            _rooms.Remove(roomId);
        }

        public async Task BroadcastAsync(string roomId, string message, string? excludeId = null)
        {
            if(!_rooms.TryGetValue(roomId, out var room))
            {
                throw new Exception($"{roomId}が存在しません.");
            }

            var tasks = room.GetPlayers()
                                .Where(p => p.Id != excludeId)
                                .Select(p => connRegistry.SendAsync(p.Id, message));

            await Task.WhenAll(tasks);
        }
    }
}
