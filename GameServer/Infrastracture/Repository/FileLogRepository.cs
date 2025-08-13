using GameServer.Application;
using GameServer.Domain;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using static GameServer.Domain.LogEntity;

namespace GameServer.Infrastracture.Repository
{
    public class FileLogRepository
    {
        private readonly string _filePath;
        private readonly JsonSerializerSettings _settings;

        public FileLogRepository(string logDirectory, string roomName, string timestamp)
        {
            Directory.CreateDirectory(logDirectory);
            _filePath = GenerateUniqueLogFilePath(logDirectory, roomName, timestamp);

            // 例: キャメルケース & Enum 文字列化など
            _settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = { new Newtonsoft.Json.Converters.StringEnumConverter() }
            };
        }
        string GenerateUniqueLogFilePath(string logDirectory, string roomName, string timestamp)
        {
            int counter = 1;
            string filePath;
            do
            {
                string fileName = $"{timestamp}_{roomName}_{counter:D3}.log";
                filePath = Path.Combine(logDirectory, fileName);
                counter++;
            } while (File.Exists(filePath));

            Console.WriteLine($"[Log] ログファイルを生成: {filePath}");
            return filePath;
        }

        public async Task SaveAsync(LogEntry entry)
        {
            string json = JsonConvert.SerializeObject(entry, _settings);
            await File.AppendAllTextAsync(_filePath, json + '\n'); // JSON Lines 形式で追記
        }
    }
}
