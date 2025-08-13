using GameServer.Application;
using GameServer.Application.DTO;
using GameServer.Domain;
using GameServer.Domain.Object;
using GameServer.Domain.Object.Components;

namespace GameServer.Utility
{
    public enum PacketId
    {
        None = 0,
        JoinRequest,
        JoinResponse,
        JoinPlayerNotifier,
        CreateRoomRequest,
        CreateRoomResponse,
        SearchRoomRequest,
        SearchRoomResponse,
        DisconnectNotifier,
        SyncObjectDataRequest,
        SyncObjectDataResponse,
        InspectObjectRequest,
        InspectObjectResponse,
        ActionRequest,
        ActionResponse,
        PositionUpdate,
        StartVoteRequest,
        VoteNotifier,
        VoteChoiceRequest,
        VoteEndNotifier,
        Ping = 99,
        Pong = 100,
        Error,
    }

    public interface IPacketModel
    {
        PacketId PacketId { get; }
        object Payload { get; }
    }
    public struct PacketModel<T> : IPacketModel
    {
        public PacketId PacketId { get; set; }
        public T Payload { get; set; }
        object IPacketModel.Payload => Payload!;
    }

    public class CreateRoomRequest
    {
        public string RoomName { get; set; }
        public string PlayerName { get; set; }
        public int StageId { get; set; }
    }

    public class CreateRoomResponse
    {
        public bool Success { get; set; }
        public string RoomId { set; get; }
        public string RoomName { get; set; }
        public string PlayerId { get; set; }
        public string PlayerName { get; set; }
        public int StageId { set; get; }
        public List<SyncObjectPacket> SyncData { get; set; }
        public int MaxRiskAmount { get; set; }
        public int MaxActionPointAmount { get; set; }
    }

    public class JoinRequest
    {
        public string RoomId { get; set; }
        public string PlayerName { get; set; }
    }

    public class JoinResponse
    {
        public bool Success { get; set; }
        public string RoomId { get; set; }
        public string RoomName { get; set; }
        public string PlayerId { get; set; }
        public string PlayerName { get; set; }
        public int StageId { set; get; }
        public List<PlayerSession> Players { get; set; }
        public List<SyncObjectPacket> SyncData { get; set; }
        public int MaxRiskAmount { get; set; }
        public int MaxActionPointAmount { get; set; }
    }

    public class JoinPlayerNotifier
    {
        public string JoinedPlayerId { get; set; }
        public string JoinedPlayerName { get; set; }
    }

    public class SearchRoomRequest
    {

    }

    public class DisconnectNotifier
    {
        public string DisconnectedId { get; set; }
    }

    public class SearchRoomResponse
    {
        public List<RoomSession> RoomList { get; set; }
    }

    public class SyncObjectPacket
    {
        public string objectId { get; set; }
        public List<IGameComponentDTO> Components { get; set; } = new();
    }

    public interface IGameComponentDTO
    {
        string Type { get; }
    }

    public class ActionComponentData : IGameComponentDTO
    {
        public string Type { get; set; }
        public string NeedAttribute { get; set; }
    }

    public class CarriableComponentData : IGameComponentDTO
    {
        public string Type { get; set; }
    }

    public class ChoicableComponentData : IGameComponentDTO
    {
        public string Type { get; set; }
        public List<Choice> Choices { get; set; }
        public Choice SelectedChoice { get; set; }
    }

    public class InspectableComponentData : IGameComponentDTO
    {
        public string Type { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool IsActioned { get; set; }
    }

    public class DoorComponentData : IGameComponentDTO
    {
        public string Type { get; set; }
        public bool IsOpen { get; set; }
        public bool IsLock { get; set; }
    }

    public class InspectObjectRequest
    {
        public string PlayerId { get; set; }
        public string RoomId { get; set; }
        public string ObjectId { get; set; }
        public string SelectedChoiceLabel { get; set; }
        public TimeSpan ElapsedInspectTime { get; set; }    //選択肢を確認してから選ぶのにかかった時間
    }

    public class InspectObjectResponse
    {
        public string ObjectId { get; set; }
        public SyncObjectPacket SyncData { get; set; }
    }

    public class ActionRequest
    {
        public string PlayerId { get; set; }
        public string RoomId { get; set; }
        public string ObjectId { get; set; }    //見ているオブジェクトのId
        public string HeldId { get; set; }     //手に持っているオブジェクトのId
        public TargetType Type { get; set; }
        public string SelectedActionLabel { get; set; }
    }

    public class ActionResponse
    {
        public ActionResultType Result { get; set; }
        public TargetType Target { get; set; }
        public string ActionId { get; set; }
        public string TargetId { get; set; }   //見ているオブジェクトのId
        public string HeldId { get; set; }     //手に持っているオブジェクトのId
        public SyncObjectPacket SyncData { get; set; }
        public int currentRiskAmount { get; set; }
        public int currentActionPointAmount { get; set; }
        public List<RiskAssessmentHistory> histories { get; set; }
    }

    public class StartVoteRequest
    {
        public string PlayerId { get; set; }
        public string RoomId { get; set; }
    }

    public class VoteNotifier
    {
        public int Yes { get; set; }
        public int No { get; set; }
        public int Total { get; set; }
    }

    public class VoteChoiceRequest
    {
        public string PlayerId { get; set; }
        public string RoomId { get; set; }
        public VoteChoice Choice { get; set; }
    }

    public class VoteEndNotifier
    {
        public VoteResult Result { get; set; }
    }

    public class PositionUpdate
    {
        public string PlayerId { get; set; }
        public string RoomId { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }

    public class Ping
    {

    }

    public class Pong
    {

    }
}
