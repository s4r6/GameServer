using System.Net.WebSockets;

namespace GameServer.Domain
{
    public class Player
    {
        public string Id { get; }
        public string Name { get; private set; }

        public string currentLookingObject = string.Empty;
        public string currentCarringObject = string.Empty;

        public Player(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
