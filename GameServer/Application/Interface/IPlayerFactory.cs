using GameServer.Domain;

namespace GameServer.Application
{
    public interface IPlayerFactory
    {
        Player Create(string name, string id);
    }
}
