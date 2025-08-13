using GameServer.Application;
using GameServer.Domain.Object;
using GameServer.InterfaceAdapter;

namespace GameServer.Domain
{
    public class Room
    {
        public string Id { get; }
        public string Name { get; }

        //Key: Id,Value: Player
        private readonly Dictionary<string, Player> _players = new();
        private readonly Stage stage;
        public GameClock Clock { get; } = new();

        VoteSession voteSession;


        public Room(string roomId, string name, Stage stage)
        {
            Id = roomId;
            Name = name;
            this.stage = stage;

            Clock.Start();
        }

        public bool AddPlayer(Player session)
        {
            if (_players.ContainsKey(session.Id))
                return false;

            _players[session.Id] = session;
        
            return true;
        }

        public Player GetPlayer(string Id)
        {
            if(_players.TryGetValue(Id, out Player player))
            {
                return player;
            }
            return null;
        }

        public void RemovePlayer(string playerId)
        {
            _players.Remove(playerId);
        }

        public bool IsEmpty()
        {
            return _players.Count <= 0;
        }

        public bool HasVote => voteSession != null && voteSession.IsActive;

        public bool TryStartVote(string initiatorId, TimeSpan duration, float requiredRatio, out VoteProgress progress)
        {
            progress = default;
            if (HasVote) return false;

            var playerIds = _players.Values.Select(p => p.Id);

            voteSession = new VoteSession(initiatorId, playerIds, duration, requiredRatio, out progress);
            
            return true;
        }

        public bool TryCastVote(string playerId, VoteChoice choice, out VoteProgress progress)
        {
            progress = default;
            return voteSession?.TryCastVote(playerId, choice, out progress) == true;
        }

        public VoteResult? EvaluateVote()
        {
            if (voteSession == null) return null;
            return voteSession.Evaluate();
        }

        public VoteProgress? GetVoteProgress()
        {
            return voteSession?.GetProgress();
        }

        public void ClearVote()
        {
            voteSession = null;
        }

        public IEnumerable<Player> GetPlayers() => _players.Values;
        public List<ObjectEntity> GetAllEntities() => stage.Entities.Values.ToList();
        public Stage GetStage() => stage;
    }
}
