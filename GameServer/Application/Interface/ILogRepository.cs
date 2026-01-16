using static GameServer.Domain.LogEntity;

namespace GameServer.Application
{
    public interface ILogRepository
    {
        Task SaveAsync(LogEntry entry, string roomId);
    }
}
