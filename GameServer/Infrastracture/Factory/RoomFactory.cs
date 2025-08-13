using GameServer.Application.Interface;
using GameServer.Domain;

namespace GameServer.Infrastracture.Factory
{
    public class RoomFactory : IRoomFactory
    {
        StageFactory stageFactory;
        public RoomFactory(StageFactory factory) 
        {
            stageFactory = factory;
        }

        public Room Create(string roomId, int stageId)
        {
            var stage = stageFactory.Create(stageId);
            var id = Guid.NewGuid().ToString();
            return new Room(id, roomId, stage);
        }
    }
}
