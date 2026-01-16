namespace GameServer.Domain
{
    public enum LogCategory
    {
        Inspect,
        Action,
        System,
        Error
    }

    public interface ILog
    {
        public string ClientId { get; }
        public string Message { get; }
    }

    public class InGameLog : ILog
    {
        public string ElapsedTime { get; }
        public string RoomName { get; }
        public string ClientId { get; }
        public string ClientName { get; }
        public string Message { get; }
        public LogCategory Category { get; }


        public InGameLog(TimeSpan elapsedTime, string roomName, string clientId, string clientName, string message, LogCategory category)
        {
            ElapsedTime = elapsedTime.ToString(@"hh\:mm\:ss");
            RoomName = roomName;
            ClientId = clientId;
            ClientName = clientName;
            Message = message;
            Category = category;
        }
    }

    public class ConnectionLog : ILog
    {
        public string Time { get; }
        public string ClientId { get; }
        public string Message { get; }
        public LogCategory Category { get; }


        public ConnectionLog(DateTime elapsedTime, string clientId, string message, LogCategory category)
        {
            Time = elapsedTime.ToString(@"hh\:mm\:ss");
            ClientId = clientId;
            Message = message;
            Category = category;
        }
    }
}
