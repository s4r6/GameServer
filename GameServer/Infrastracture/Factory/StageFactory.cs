using System.Runtime.InteropServices;
using GameServer.Application;
using GameServer.Domain;
using GameServer.Domain.Object;
using GameServer.Domain.Object.Components;
using GameServer.Infrastracture.Repository;

namespace GameServer.Infrastracture.Factory
{
    public class StageFactory
    {
        private readonly IObjectRepository _repository;
        Dictionary<int, StageTemplate> StageTemplateMap = new();

        public StageFactory(IObjectRepository repository)
        {
            _repository = repository;
            var entityTemplates = _repository.GetAll();
            //Template作成
            foreach (var kvp in entityTemplates)
            {
                var stageTemplate = CreateTemplate(kvp.Key, kvp.Value);
                StageTemplateMap.Add(kvp.Key, stageTemplate);
            }
        }

        public Stage Create(int stageId)
        {
            var stageTemplate = StageTemplateMap[stageId];

            Dictionary<string, ObjectEntity> clone_Entities = new();
            foreach(var entity in stageTemplate.Entities.Values)
            {
                var clone_Entity = entity.Clone();
                clone_Entities.Add(clone_Entity.Id, clone_Entity);
            }
            var maxRiskAmount = stageTemplate.MaxRiskAmount;
            var maxActionPoint = stageTemplate.MaxActionPoint;

            Console.WriteLine(clone_Entities.Count());
            return new Stage(Guid.NewGuid().ToString(), stageId, maxRiskAmount, maxActionPoint, clone_Entities);
        }

        StageTemplate CreateTemplate(int stageId, List<ObjectEntity> entities)
        {
            Dictionary<string, ObjectEntity> copy = new();
            foreach (var entity in entities) 
            { 
                copy.Add(entity.Id, entity.Clone());
            }

            var maxrisk = CalcMaxRiskAmount(copy.Values.ToList());
            var maxaction = CalcMaxActionPoint(copy.Values.ToList());

            return new StageTemplate(stageId, maxrisk, maxaction, copy);
        }

        private int CalcMaxRiskAmount(IReadOnlyList<ObjectEntity> entities)
        {
            return entities
                .Where(e => e.HasComponent<ChoicableComponent>())
                .Select(e =>
                {
                    var choicable = e.GetComponent<ChoicableComponent>();
                    return choicable.Choices
                        .Where(c => c.OverrideActions.Any())
                        .Select(c => c.OverrideActions.Min(a => a.riskChange))
                        .DefaultIfEmpty(0)
                        .Min();
                })
                .Sum(minRisk => -minRisk);
        }

        private int CalcMaxActionPoint(IReadOnlyList<ObjectEntity> entities)
        {
            return entities
                .Where(e => e.HasComponent<ChoicableComponent>())
                .Select(e =>
                {
                    var choicable = e.GetComponent<ChoicableComponent>();
                    var minRiskAction = choicable.Choices
                        .Where(c => c.OverrideActions != null && c.OverrideActions.Any())
                        .SelectMany(c => c.OverrideActions)
                        .OrderBy(a => a.riskChange)
                        .FirstOrDefault();

                    return minRiskAction?.actionPointCost ?? 0;
                })
                .Sum();
        }
    }
}
