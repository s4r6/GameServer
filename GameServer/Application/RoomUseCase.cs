using GameServer.Application.DTO;
using GameServer.Application.Interface;
using GameServer.Domain;
using GameServer.Domain.Object;
using GameServer.Infrastracture.Factory;

namespace GameServer.Application
{
    public class RoomUseCase
    {
        private readonly IRoomRegistry _roomRegistry;
        private readonly IPlayerFactory _playerFactory;
        readonly IRoomFactory roomFactory;

        public RoomUseCase(IRoomRegistry roomRegistry, IPlayerFactory playerFactory, IRoomFactory roomFactory)
        {
            _roomRegistry = roomRegistry;
            _playerFactory = playerFactory;
            this.roomFactory = roomFactory;
        }

        

        public CreateRoomOutputData HandleCreate(CreateRoomInputData data)
        {
            var roomName = data.RoomName;
            var stageId = data.StageId;
            var exists = _roomRegistry.Exists(roomName);

            if (exists)
            {
                return new CreateRoomOutputData
                {
                    Success = false,
                    RoomName = data.RoomName,
                    Entities = null
                };
            }

            var room = roomFactory.Create(roomName, stageId);
            _roomRegistry.Save(room);
            var stage = room.GetStage();

            var player = new Player(data.PlayerId, data.PlayerId);
            room.AddPlayer(player);

            return new CreateRoomOutputData
            {
                Success = true,
                RoomId = room.Id,
                RoomName = data.RoomName,
                PlayerName = data.PlayerName,
                PlayerId = data.PlayerId,
                StageId = stageId,
                Entities = room.GetAllEntities(),
                MaxRiskAmount = stage.CurrentRiskAmount,
                MaxActionPointAmount = stage.CurrentActionPoint
            };
        }
        public JoinRoomOutputData HandleJoin(JoinRoomInputData data)
        {
            var roomId = data.RoomId;
            var player = _playerFactory.Create(data.PlayerName, data.PlayerId);

            var room = _roomRegistry.Get(roomId);
            Stage stage = null;
            bool success = false;
            if (room == null)
            {
                success = false;
            }
            else
            {
                success = room.AddPlayer(player);
                stage = room.GetStage();
            }

            List<PlayerSession> sessions = new List<PlayerSession>();
            foreach (var playerData in room.GetPlayers()) 
            { 
                var playerSession = new PlayerSession
                {
                    Id = playerData.Id,
                    Name = playerData.Name
                };

                sessions.Add(playerSession);
            }

            return new JoinRoomOutputData
            {
                Success = success,
                RoomId = roomId,
                RoomName = room.Name,
                PlayerId = player.Id,
                PlayerName = player.Name,
                RoomMembers = sessions,
                StageId = stage?.StageId ?? -1,
                Entities = room.GetAllEntities(),
                MaxRiskAmount = stage?.CurrentRiskAmount ?? -1,
                MaxActionPointAmount= stage?.CurrentActionPoint ?? -1
            };
        }

        public List<RoomSession> HandleSearch()
        {
            var rooms = _roomRegistry.GetAll();
            var sessions = new List<RoomSession>();

            //RoomをRoomSessionに書き換え
            foreach (var room in rooms) 
            {
                List<PlayerSession> players = new List<PlayerSession>();
                foreach(var player in room.GetPlayers())
                {
                    players.Add(new PlayerSession
                    {
                        Id = player.Id,
                        Name = player.Name,
                    });
                }

                sessions.Add(new RoomSession
                {
                    Id = room.Id,
                    Name = room.Name,
                    Players = players,
                    StageId = room.GetStage().StageId
                });
            }

            return sessions;
        }

        public DisconnectedData HandleDisconnect(string connectionId)
        {
            var room = _roomRegistry.GetByPlayerId(connectionId);
            if (room == null)
            {
                Console.WriteLine($"connectionId:{connectionId}が所属している部屋は存在しません.");
            }
            else
            {
                room.RemovePlayer(connectionId);
                if (room.IsEmpty())
                {
                    _roomRegistry.Remove(room.Id);
                    Console.WriteLine($"[Info] Room {room.Id} removed because it's empty.");
                }
                else
                {
                    Console.WriteLine($"[Info] Player {connectionId} left room {room.Id}.");
                }

                return new DisconnectedData
                {
                    RoomId = room.Id,
                    DisconnectedId = connectionId
                };
            }

            return null;
        }
    }
}
