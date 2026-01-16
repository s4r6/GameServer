namespace GameServer.Domain
{
    public class LogEntity
    {
        public enum LogCategory
        {
            Inspect,
            Action,
            System,
            Error
        }

        /// <summary>ゲーム内の 1 行ログ。</summary>
        public sealed class LogEntry
        {
            public TimeSpan ElapsedTime { get; }
            public string RoomName { get; }
            public string PlayerId { get; }
            public string Message { get; }
            public LogCategory Category { get; }
            

            public LogEntry(TimeSpan elapsedTime, string roomName, string playerId, string message, LogCategory category)
            {
                ElapsedTime = elapsedTime;
                RoomName = roomName;
                PlayerId = playerId;
                Message = message;
                Category = category;
            }
        }
    }
}
