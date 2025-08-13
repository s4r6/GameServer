using GameServer.Application;
using GameServer.Application.DTO;
using GameServer.Application.Interface;
using GameServer.Domain;
using GameServer.Infrastracture;
using GameServer.InterfaceAdapter.Interface;
using GameServer.Utility;
using Microsoft.Extensions.Logging.Abstractions;
using static GameServer.Domain.LogEntity;

namespace GameServer.InterfaceAdapter
{


    public class ActionPresenter
    {
        readonly LoggerUseCase logger;
        readonly ServerActionUseCase actionUseCase;
        readonly IRoomRegistry _registry;

        public ActionPresenter(ServerActionUseCase actionUseCase, IRoomRegistry registry, LoggerUseCase logger)
        {
            this.logger = logger;
            this.actionUseCase = actionUseCase;
            _registry = registry;
        }

        public async Task HandleActionRequest(string rawJson, string connectionId)
        {
            var packet = PacketSerializer.Deserialize<ActionRequest>(rawJson);

            var roomId = packet.Payload.RoomId;
            var playerId = packet.Payload.PlayerId;
            var objectId = packet.Payload.ObjectId;
            var heldId = packet.Payload.HeldId;
            var selectedAction = packet.Payload.SelectedActionLabel;

            var input = new ActionInputData
            {
                RoomId = roomId,
                PlayerId = playerId,
                TargetId = objectId,
                HeldId = heldId,
                ActionLabel = selectedAction,
                Type = packet.Payload.Type
            };
            var result = actionUseCase.Action(input);

            PacketModel<ActionResponse> responsePacket;
            string json = string.Empty;
            if(result.result != ActionResultType.Success)
            {
                responsePacket = new PacketModel<ActionResponse>
                {
                    PacketId = PacketId.ActionResponse,
                    Payload = new ActionResponse
                    {
                        Result = result.result,
                    }
                };

                json = PacketSerializer.Serialize(responsePacket);
                await _registry.BroadcastAsync(result.RoomId, json);

                return;
            }


            var log = new LogEntry(
                            TimeSpan.Zero,
                            result.RoomName,
                            result.PlayerName,
                            $"[Action]: Action={result.actionId}, ObjectId={objectId}",
                            LogCategory.Action
                           );
            await logger.SaveAsync(log, result.RoomId);

            SyncObjectPacket objData = new SyncObjectPacket();

            List<IGameComponentDTO> componentDatas = new();
            foreach (var component in result.entity.AllComponents)
            {
                componentDatas.Add(ComponentDataFactory.ToData(component));
            }

            objData.objectId = result.entity.Id;
            objData.Components = componentDatas;

            responsePacket = new PacketModel<ActionResponse>
            {
                PacketId = PacketId.ActionResponse,
                Payload = new ActionResponse
                {
                    Result = ActionResultType.Success,
                    Target = result.target,
                    ActionId = result.actionId,
                    TargetId = packet.Payload.ObjectId,
                    HeldId = packet.Payload.HeldId,
                    SyncData = objData,
                    currentRiskAmount = result.currentRiskAmount,
                    currentActionPointAmount = result.currentActionPointAmount,
                    histories = result.histories,
                }
            };

            json = PacketSerializer.Serialize(responsePacket);
            await _registry.BroadcastAsync(result.RoomId, json);

            return;
        }
    }
}
