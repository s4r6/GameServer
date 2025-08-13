using GameServer.Application;

namespace GameServer.InterfaceAdapter
{
    public class GameClock : IGameClock
    {
        private DateTime _startTimeUtc;

        public void Start() => _startTimeUtc = DateTime.UtcNow;

        public TimeSpan ElapsedSinceGameStart() => DateTime.UtcNow - _startTimeUtc;
    }
}
