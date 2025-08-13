using GameServer.Application;

namespace GameServer.Domain
{
    public class PlayerFactory : IPlayerFactory
    {
        public Player Create(string name, string id)
        {
            return new Player(id, name);
        }
    }
}
