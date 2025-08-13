using GameServer.Domain;

namespace GameServer.Application.Interface
{
    public interface IRoomRegistry
    {
        bool Exists(string roomId);
        void Save(Room room);
        void Remove(string roomId);
        Room? Get(string roomId);
        Room? GetByPlayerId(string playerId);
        List<Room> GetAll();
        Task BroadcastAsync(string roomId, string message, string? excludeId = null);
    }
}
