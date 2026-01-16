using GameServer.Domain;

namespace GameServer.Application
{
    public interface ILogRepository
    {
        Task SaveAsync(ILog log, string Id, string Name, string SubFolder);
    }
}
