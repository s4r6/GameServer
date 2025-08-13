using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using GameServer.Application;
using GameServer.Domain.Object;
using GameServer.Infrastracture.Factory;

namespace GameServer.Infrastracture.Repository
{
    public class ObjectRepository : IObjectRepository
    {
        private readonly Dictionary<int, List<ObjectEntity>> StageObjectMap = new();
        string contentRoot;
        public ObjectRepository(IHostEnvironment env, IEntityFactory factory, string basePath = "Master/")
        {
            contentRoot = env.ContentRootPath;
            var rawObjects = LoadJson(basePath + "Objects.json");
            List<ObjectEntity> entities = new List<ObjectEntity>();
            foreach (var obj in rawObjects)
            {
                var entity = factory.CreateEntityFromJson((JObject)obj);
                entities.Add(entity);
            }
            StageObjectMap.Add(1, entities);
        }

        public List<ObjectEntity> GetByStageId(int stageId)
        {
            return StageObjectMap.TryGetValue(stageId, out var entity) ? entity : null;
        }

        public IReadOnlyDictionary<int, List<ObjectEntity>> GetAll() => StageObjectMap;

        public JArray LoadJson(string path)
        {
            var fullPath = Path.Combine(contentRoot, path);
            var json = File.ReadAllText(fullPath);
            return JsonConvert.DeserializeObject<JArray>(json); // Newtonsoft‚ÅJArray‚É•ÏŠ·
        }
    }
}