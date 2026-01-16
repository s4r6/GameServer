using GameServer.Application.Interface;
using GameServer.Domain;

namespace GameServer.Application
{
    public class LoggerUseCase
    {
        private readonly ILogRepository _repository;
        private readonly IRoomRegistry _registry;

        public LoggerUseCase(ILogRepository repository, IRoomRegistry registry)
        {
            _repository = repository;
            _registry = registry;
        }

        public async Task SaveAsync_InGameLog(InGameLog log, string roomId)
        {
            var clock = _registry.Get(roomId)?.Clock;
            if (clock == null) return;

            // Clock から経過時間を取得して新しいエントリを作成
            var stamped = new InGameLog(
                clock.ElapsedSinceGameStart(),
                log.RoomName,
                log.ClientId,
                log.ClientName,
                log.Message,
                log.Category
            );

            await _repository.SaveAsync(stamped, roomId, log.RoomName, "InGame");
        }

        public async Task SavaAsync_ConnectionLog(ConnectionLog log, string clientId)
        {
            await _repository.SaveAsync(log, clientId, log.ClientId, "Connection");
        }
    }
}
