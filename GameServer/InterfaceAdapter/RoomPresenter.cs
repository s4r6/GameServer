using GameServer.Application;
using GameServer.Application.DTO;
using GameServer.Application.Interface;
using GameServer.Domain;
using GameServer.Domain.Object;
using GameServer.Infrastracture;
using GameServer.Infrastracture.Repository;
using GameServer.InterfaceAdapter.Interface;
using GameServer.Utility;

namespace GameServer.InterfaceAdapter
{
    public class RoomPresenter
    {
        readonly RoomUseCase roomUseCase;
        readonly LoggerUseCase logger;

        readonly IConnectionRegistry _registry;
        readonly IRoomRegistry _roomRegistry;

        public RoomPresenter(RoomUseCase roomUseCase, LoggerUseCase logger, IConnectionRegistry registry, IRoomRegistry roomRegistry)
        {
            this.roomUseCase = roomUseCase;
            this.logger = logger;

            _roomRegistry = roomRegistry;
            _registry = registry;
        }

        //Room生成リクエスト
        public async Task HandleCreateRequest(string rawJson, string connectionId)
        {
            var packet = PacketSerializer.Deserialize<CreateRoomRequest>(rawJson);

            var input = new CreateRoomInputData
            {
                RoomName = packet.Payload.RoomName,
                PlayerName = packet.Payload.PlayerName,
                StageId = packet.Payload.StageId,
                PlayerId = connectionId
            };

            var result = roomUseCase.HandleCreate(input);

            if(result.Success)
            {
                //Logの生成処理
                var createLog = new InGameLog(
                                TimeSpan.Zero,  //一旦0
                                result.RoomName,
                                connectionId,
                                result.PlayerName,
                                $"Room created",
                                LogCategory.System
                               );
                await logger.SaveAsync_InGameLog(createLog, result.RoomId);
            }

            List<SyncObjectPacket> objDatas = ConvertToSendableData(result.Entities);

            var responsePacket = new PacketModel<CreateRoomResponse>
            {
                PacketId = PacketId.CreateRoomResponse,
                Payload = new CreateRoomResponse
                {
                    Success = result.Success,
                    RoomId = result.RoomId,
                    RoomName = result.RoomName,
                    StageId = result.StageId,
                    PlayerId = result.PlayerId,
                    PlayerName = result.PlayerName,
                    SyncData = objDatas,
                    MaxRiskAmount = result.MaxRiskAmount,
                    MaxActionPointAmount = result.MaxActionPointAmount
                }
            };

            var json = PacketSerializer.Serialize(responsePacket);
            await _registry.SendAsync(connectionId, json);

            //通信ログ出力
            var log = new ConnectionLog(
                            DateTime.Now,
                            connectionId,
                            $"[Receive] packetId = {responsePacket.PacketId}, data = {json}",
                            LogCategory.System);
            await logger.SavaAsync_ConnectionLog(log, connectionId);
        }

        public async Task HandleJoinRequest(string rawJson, string connectionId)
        {
            var packet = PacketSerializer.Deserialize<JoinRequest>(rawJson);

            var input = new JoinRoomInputData
            {
                RoomId = packet.Payload.RoomId,
                PlayerId = connectionId,
                PlayerName = packet.Payload.PlayerName,
                
            };

            var result = roomUseCase.HandleJoin(input);

            if (result.Success)
            {
                //Logの生成処理
                var joinLog = new InGameLog(
                                TimeSpan.Zero,  //一旦0
                                result.RoomName,
                                connectionId,
                                result.PlayerName,
                                $"Room joined",
                                LogCategory.System
                               );
                await logger.SaveAsync_InGameLog(joinLog, result.RoomId);
            }

            List<SyncObjectPacket> objDatas = ConvertToSendableData(result.Entities);

            var responsePacket = new PacketModel<JoinResponse>
            {
                PacketId = PacketId.JoinResponse,
                Payload = new JoinResponse
                {
                    Success = result.Success,
                    RoomId = result.RoomId,
                    RoomName = result.RoomName,
                    PlayerId = connectionId,
                    PlayerName = input.PlayerName,
                    StageId = result.StageId,
                    Players = result.RoomMembers,
                    SyncData = objDatas,
                    MaxRiskAmount = result.MaxRiskAmount,
                    MaxActionPointAmount = result.MaxActionPointAmount
                }
            };

            var json = PacketSerializer.Serialize(responsePacket);
            //Joinの結果を送信
            await _registry.SendAsync(connectionId, json);
            if (!result.Success) return;

            //通信ログ出力
            var log = new ConnectionLog(
                            DateTime.Now,
                            connectionId,
                            $"[Receive] packetId = {responsePacket.PacketId}, data = {json}",
                            LogCategory.System);
            await logger.SavaAsync_ConnectionLog(log, connectionId);


            var notifierPacket = new PacketModel<JoinPlayerNotifier>
            {
                PacketId = PacketId.JoinPlayerNotifier,
                Payload = new JoinPlayerNotifier
                {
                    JoinedPlayerId = connectionId,
                    JoinedPlayerName = result.PlayerName
                }
            };
            json = PacketSerializer.Serialize(notifierPacket);
            await _roomRegistry.BroadcastAsync(input.RoomId, json, connectionId);
        }

        public async Task HandleSearchRequest(string connectionId)
        {
            var result = roomUseCase.HandleSearch();

            var responsePacket = new PacketModel<SearchRoomResponse>
            {
                PacketId = PacketId.SearchRoomResponse,
                Payload = new SearchRoomResponse
                {
                    RoomList = result
                }
            };

            var json = PacketSerializer.Serialize(responsePacket);
            await _registry.SendAsync(connectionId, json);

            //通信ログ出力
            var log = new ConnectionLog(
                            DateTime.Now,
                            connectionId,
                            $"[Receive] packetId = {responsePacket.PacketId}, data = {json}",
                            LogCategory.System);
            await logger.SavaAsync_ConnectionLog(log, connectionId);
        }

        public async Task HandlePlayerDisconnected(string connectionId)
        {
            Console.WriteLine("切断処理");
            var result = roomUseCase.HandleDisconnect(connectionId);
            if (result == null)
                return;

            if(_roomRegistry.Exists(result.RoomId))
            {
                var packet = new PacketModel<DisconnectNotifier>
                {
                    PacketId = PacketId.DisconnectNotifier,
                    Payload = new DisconnectNotifier
                    {
                        DisconnectedId = result.DisconnectedId
                    }
                };

                var json = PacketSerializer.Serialize(packet);

                await _roomRegistry.BroadcastAsync(result.RoomId, json, result.DisconnectedId);
            }
        }

        List<SyncObjectPacket> ConvertToSendableData(IReadOnlyList<ObjectEntity> entities)
        {
            List<SyncObjectPacket> objDatas = new List<SyncObjectPacket>();
            foreach (var obj in entities)
            {
                List<IGameComponentDTO> componentDatas = new();
                foreach (var component in obj.AllComponents)
                {
                    componentDatas.Add(ComponentDataFactory.ToData(component));
                }

                objDatas.Add(new SyncObjectPacket
                {
                    objectId = obj.Id,
                    Components = componentDatas
                });
            }

            return objDatas;
        }
    }
}
