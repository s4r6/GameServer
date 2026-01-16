using GameServer.Application;
using GameServer.Domain;
using static GameServer.Domain.LogEntity;

namespace GameServer.Infrastracture.Repository
{
    public class RoomLogRepository : ILogRepository
    {
        private readonly Dictionary<string, FileLogRepository> _fileLogs = new();
        private readonly string _baseDir;

        public RoomLogRepository(string baseDir)
        {
            _baseDir = baseDir;


            // 既に「ファイル」として存在していたらエラー
            if (File.Exists(_baseDir))
            {
                throw new IOException($"'{_baseDir}' はファイルとして存在しており、ログ出力用のディレクトリとして使用できません。");
            }

            // ディレクトリが存在しなければ作成（存在すればスキップ）
            if (!Directory.Exists(_baseDir))
            {
                Directory.CreateDirectory(_baseDir);
            }
        }

        private string GetAvailableDirectory(string baseDir)
        {
            if (!Directory.Exists(baseDir))
                return baseDir;

            int counter = 1;
            string candidate;
            do
            {
                candidate = $"{baseDir}_{counter}";
                counter++;
            } while (Directory.Exists(candidate));

            return candidate;
        }

        public Task SaveAsync(LogEntry entry, string roomName)
        {
            if (!_fileLogs.TryGetValue(roomName, out var repo))
            {
                repo = CreateFileLogRepository(roomName);
                _fileLogs[roomName] = repo;
            }

            return _fileLogs[roomName].SaveAsync(entry);
        }

        FileLogRepository CreateFileLogRepository(string roomName)
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            /*int index = 1;
            string filePath;

            do
            {
                var suffix = index.ToString("D3"); // 001, 002, ...
                var fileName = $"{timestamp}_{roomName}_{suffix}.log";
                filePath = Path.Combine(_baseDir, fileName);
                index++;
            } while (File.Exists(filePath));*/

            return new FileLogRepository(_baseDir, roomName, timestamp);
        }
    }
}
