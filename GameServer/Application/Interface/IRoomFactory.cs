using GameServer.Domain;

namespace GameServer.Application.Interface
{
    public interface IRoomFactory
    {
        Room Create(string roomId, int stageId);
    }
}
