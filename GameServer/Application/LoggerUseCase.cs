using GameServer.Application.Interface;
using static GameServer.Domain.LogEntity;

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

        public async Task SaveAsync(LogEntry entry, string roomId)
        {
            var clock = _registry.Get(roomId)?.Clock;
            if (clock == null) return;

            // Clock から経過時間を取得して新しいエントリを作成
            var stamped = new LogEntry(
                clock.ElapsedSinceGameStart(),
                entry.RoomName,
                entry.PlayerId,
                entry.Message,
                entry.Category
            );

            await _repository.SaveAsync(stamped, entry.RoomName);
        }
    }
}
