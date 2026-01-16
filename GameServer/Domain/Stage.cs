using System;
using GameServer.Application.DTO;
using GameServer.Domain.Object;
using GameServer.Domain.Object.Components;

namespace GameServer.Domain
{
    public struct ActionHistory
    {
        public string ObjectId { get; }
        public string Description { get; }
        public string SelectedRiskLable { get; }
        public string ExecutedActionLabel { get; }

        public int RiskChange { get; } // 実行による変化量

        public int ActionCost { get; } // 使用したアクションポイント
        public List<string> RiskLabels { get; }
        public List<ActionEntity> Actions { get; }
        public string Explanation { get; }

        public ActionHistory(
            string objectId,
            string description,
            string riskLabel,
            string actionLabel,
            int riskChange,
            int actionCost,
            List<string> riskLabels,
            List<ActionEntity> actions,
            string explanation)
        {
            ObjectId = objectId;
            Description = description;
            SelectedRiskLable = riskLabel;
            ExecutedActionLabel = actionLabel;
            RiskChange = riskChange;
            ActionCost = actionCost;
            RiskLabels = riskLabels;
            Actions = actions;
            Explanation = explanation;
        }
    }

    public enum ActionResultType
    {
        Success,
        ShortageActionPoint,
        Unknown
    }
    public struct SurmmaryDetailDTO
    {
        public string DisplayName;  //オブジェクトの表示名
        public string RiskLabel;    //選択したリスク名
        public string ActionLabel;  //実行した対応策名
        public int RiskChange;
        public int ActionCost;
        public string Explanation;  //解説
        public string Description;     //状況説明

        public List<string> RiskLabels;
        //DisplayName, <RiskChange, ActionCost>
        public List<(string label, (int, int))> ActionLabels;
    }

    public class StageTemplate
    {
        public readonly int StageId;
        public readonly int MaxRiskAmount;
        public readonly int MaxActionPoint;
        public Dictionary<string, ObjectEntity> Entities { get; } = new Dictionary<string, ObjectEntity>();

        public StageTemplate(int stageId, int maxRiskAmount, int maxActionPoint, Dictionary<string, ObjectEntity> entities)
        {
            StageId = stageId;
            MaxRiskAmount = maxRiskAmount;
            MaxActionPoint = maxActionPoint;
            Entities = entities;
        }
    }

    public class Stage
    {

        readonly int maxRiskAmount;
        readonly int maxActionPoint;
        public string Id { get; private set; }         //Entityごとの固有の識別子
        public int StageId { get; private set; }    //読み込まれているStageId
        public int CurrentRiskAmount { get; private set; }
        public int CurrentActionPoint { get; private set; }

        public Dictionary<string, ObjectEntity> Entities { get; private set; } = new Dictionary<string, ObjectEntity>();

        //----------------------リザルト表示用--------------------------
        public List<SurmmaryDetailDTO> histories = new();

        public Stage(string id, int stageId, int maxRiskAmount, int maxActionPoint, Dictionary<string, ObjectEntity> entities)
        {
            Id = id;
            StageId = stageId;

            this.maxRiskAmount = maxRiskAmount;
            this.maxActionPoint = maxActionPoint;

            CurrentRiskAmount = maxRiskAmount;
            CurrentActionPoint = maxActionPoint;

            Entities = entities;
            foreach (var entity in Entities.Values)
            {
                Console.WriteLine($"{entity.Id}:{entity.HasConponentNum}");
            }
        }

        public ObjectEntity TryGetEntity(string id)
        {
            if(Entities.TryGetValue(id, out var entity)) return entity;

            return null;
        }

        public ObjectEntity Inspect(string objectId, string selectedChoice)
        {
            if (!Entities.TryGetValue(objectId, out var entity))
            {
                return null;
            }

            if (!entity.TryGetComponent<InspectableComponent>(out var inspectable) || !entity.TryGetComponent<ChoicableComponent>(out var choicable)) return null;

            Choice choice = choicable.Choices.Find(x => x.Label == selectedChoice);

            if (!inspectable.IsActioned)
            {
                choicable.SelectedChoice = choice;
            }

            if (choicable.SelectedChoice.OverrideActions.Any(a => a.target == TargetType.Self) && !entity.HasComponent<ActionSelf>())
            {
                entity.Add(new ActionSelf());
            }

            return entity.Clone();
        }

        public void OnExecuteAction(ActionHistory history)
        {
            CurrentActionPoint -= history.ActionCost;
            CurrentRiskAmount += history.RiskChange;

            var detailDTO = new SurmmaryDetailDTO()
            {
                DisplayName = history.ObjectId,
                Description = history.Description,
                RiskLabel = history.SelectedRiskLable,
                ActionLabel = history.ExecutedActionLabel,
                RiskChange = history.RiskChange,
                ActionCost = history.ActionCost,
                RiskLabels = history.RiskLabels,
                ActionLabels = history.Actions.Select(action => (action.label, (action.riskChange, action.actionPointCost))).ToList(),
                Explanation = history.Explanation,
            };

            histories.Add(detailDTO);
        }
    }
}
