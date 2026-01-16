using GameServer.Application;
using GameServer.Application.Interface;
using GameServer.Infrastracture;
using GameServer.InterfaceAdapter.Interface;
using GameServer.Utility;
using GameServer.Domain;

namespace GameServer.InterfaceAdapter
{
    public class InspectPresenter
    {
        readonly ServerInspectUseCase inspectUseCase;
        readonly IRoomRegistry _registry;
        readonly LoggerUseCase logger;

        public InspectPresenter(ServerInspectUseCase inspectUseCase, IRoomRegistry registry, LoggerUseCase logger)
        {
            this.inspectUseCase = inspectUseCase;
            _registry = registry;
            this.logger = logger;
        }

        public async Task HandleInspectRequest(string rawJson, string connectionId)
        {
            var packet = PacketSerializer.Deserialize<InspectObjectRequest>(rawJson);

            var roomId = packet.Payload.RoomId;
            var objectId = packet.Payload.ObjectId;
            var selectedChoice = packet.Payload.SelectedChoiceLabel;
            var playerId = packet.Payload.PlayerId;
            var elapsedTime = packet.Payload.ElapsedInspectTime;

            var result = inspectUseCase.Inspect(roomId, objectId, selectedChoice, playerId);
            if(result.Success)
            {
                var log = new InGameLog(
                                TimeSpan.Zero,
                                result.RoomName,
                                connectionId,
                                result.PlayerName,
                                $"[Inspect]: Risk={selectedChoice}, ObjectId={objectId}, ElapsedTime={elapsedTime}",
                                LogCategory.Inspect
                               );
                await logger.SaveAsync_InGameLog(log, result.RoomId);
            }

            var entity = result.Entity;
            SyncObjectPacket objData = new SyncObjectPacket();

            List<IGameComponentDTO> componentDatas = new();
            foreach (var component in entity.AllComponents)
            {
                componentDatas.Add(ComponentDataFactory.ToData(component));
            }

            objData.objectId = entity.Id;
            objData.Components = componentDatas;

            var responsePacket = new PacketModel<InspectObjectResponse>
            {
                PacketId = PacketId.InspectObjectResponse,
                Payload = new InspectObjectResponse
                {
                    ObjectId = objectId,
                    SyncData = objData
                }
            };

            var json = PacketSerializer.Serialize(responsePacket);
            Console.WriteLine(json);
            await _registry.BroadcastAsync(result.RoomId, json);
        }
    }
}
