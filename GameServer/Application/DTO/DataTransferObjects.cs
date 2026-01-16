using GameServer.Domain;
using GameServer.Domain.Object;
using GameServer.Domain.Object.Components;

namespace GameServer.Application.DTO
{
    // UseCase - Presenter
    public class CreateRoomInputData
    {
        public string RoomName { get; init; }
        public int StageId { get; init; }
        public string PlayerName { get; init; }
        public string PlayerId { get; init; }
    }

    public class CreateRoomOutputData
    {
        public bool Success { get; init; }
        public string RoomId { get; init; }
        public string RoomName { get; init; }
        public string PlayerId { get; init; }
        public string PlayerName {  get; init; }
        public int StageId { get; init; }   
        public IReadOnlyList<ObjectEntity> Entities { get; init; }
        public int MaxRiskAmount {  get; init; }
        public int MaxActionPointAmount {  get; init; }
    }

    public class JoinRoomInputData
    {
        public string RoomId { get; init; }
        public string PlayerName { get; init; }
        public string PlayerId { get; init; }
    }

    public class JoinRoomOutputData
    {
        public bool Success { get; init; }
        public string RoomId { get; init; }
        public string RoomName { get; init; }
        public string PlayerId { get; init; }
        public string PlayerName { get; init; }
        public List<PlayerSession> RoomMembers { get; init; } = [];
        public int StageId { get; init; }
        public IReadOnlyList<ObjectEntity> Entities { get; init; }
        public int MaxRiskAmount { get; init; }
        public int MaxActionPointAmount { get; init; }
    }

    public class DisconnectedData
    {
        public string RoomId { get; init; }
        public string DisconnectedId { get; init; }
    }

    public class InspectResultData
    {
        public bool Success { get; init; }
        public string RoomId { get; init; }
        public string RoomName { get; init; }
        public string PlayerName { get; init; }
        public ObjectEntity Entity { get; init; }

    }

    public class ActionInputData
    {
        public string RoomId { get; init; }
        public string PlayerId { get; init; }
        public string TargetId {  get; init; }
        public string HeldId {  get; init; }
        public string ActionLabel {  get; init; }
        public TargetType Type { get; init; }
    }

    public class ActionResultData
    {
        public ActionResultType result {  get; init; }
        public TargetType target { get; init; }
        public string actionId {  get; init; }
        public string RoomId { get; init; }
        public string RoomName { get; init; }
        public string PlayerName { get; init; } 
        public ObjectEntity entity { get; init; }
        public int currentRiskAmount {  get; init; }
        public int currentActionPointAmount {  get; init; }
        public List<RiskAssessmentHistory> histories { get; init; }
    }

    public class PlayerSession
    {
        public string Id {  get; init; }
        public string Name { get; init; }
    }

    public class RoomSession
    {
        public string Id { get; init; }
        public string Name { get; init; }
        public List<PlayerSession> Players { get; init; }
        public int StageId { get; init; }
    }

    public class StartVoteInputData
    {
        public string RoomId { get; init; }
        public string PlayerId { get; init; }
    }

    public class CastVoteInput
    {
        public string RoomId { get; init; }
        public string PlayerId { get; init; }
        public VoteChoice Choice { get; init; }
    }

    public class VoteOutputData
    {
        public bool IsSuccess { get; init; }
        public int Yes {  get; init; }
        public int No {  get; init; }
        public int Total { get; init; }
        public VoteResult Result {  get; init; }
    }
        
}
