using GameServer.Application;
using GameServer.Domain;

namespace GameServer.Infrastracture.Repository
{
    public class LogRepository : ILogRepository
    {
        private readonly Dictionary<string, FileLogRepository> _fileLogs = new();
        private readonly string _baseDir;

        public LogRepository(string baseDir)
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

        public Task SaveAsync(ILog log, string Id, string Name, string subFolder)
        {
            if (!_fileLogs.TryGetValue(Id, out var repo))
            {
                repo = CreateFileLogRepository(Name, subFolder);
                _fileLogs[Id] = repo;
            }

            return _fileLogs[Id].SaveAsync(log);
        }

        FileLogRepository CreateFileLogRepository(string Name, string subFolder)
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string directoryPath = Path.Combine(_baseDir, subFolder);

            return new FileLogRepository(directoryPath, Name, timestamp);
        }
    }
}
