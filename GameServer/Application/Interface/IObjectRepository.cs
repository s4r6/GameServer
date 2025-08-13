using GameServer.Domain.Object;

namespace GameServer.Application
{
    public interface IObjectRepository
    {
        List<ObjectEntity> GetByStageId(int stageId);
        IReadOnlyDictionary<int, List<ObjectEntity>> GetAll();
    }
}