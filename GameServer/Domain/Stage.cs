using System;
using GameServer.Application.DTO;
using GameServer.Domain.Object;
using GameServer.Domain.Object.Components;

namespace GameServer.Domain
{
    public struct ActionHistory
    {
        public string ObjectName { get; }
        public string SelectedRiskLable { get; }
        public string ExecutedActionLabel { get; }

        public int RiskChange { get; } // 実行による変化量

        public int ActionCost { get; } // 使用したアクションポイント
        public string Explanation { get; }

        public ActionHistory(
            string objectName,
            string riskLabel,
            string actionLabel,
            int riskChange,
            int actionCost,
            string explanation)
        {
            ObjectName = objectName;
            SelectedRiskLable = riskLabel;
            ExecutedActionLabel = actionLabel;
            RiskChange = riskChange;
            ActionCost = actionCost;
            Explanation = explanation;
        }
    }

    public enum ActionResultType
    {
        Success,
        ShortageActionPoint,
        Unknown
    }
    public struct RiskAssessmentHistory
    {
        public string ObjectName { get; set; }
        public string SelectedRiskLabel { get; set; }
        public string ExecutedActionLabel { get; set; }

        public int RiskChange { get; set; } // 実行による変化量
        public int CurrentRisk { get; set; }
        public int MaxRisk { get; set; }

        public int ActionCost { get; set; }// 使用したアクションポイント
        public int CurrentActionPoint { get; set; }
        public int MaxActionPoint { get; set; }

        public string Explanation { get; set; }

        public RiskAssessmentHistory(
            string objectName,
            string riskLabel,
            string actionLabel,
            int riskChange,
            int currentRisk,
            int maxRisk,
            int actionCost,
            int currentAP,
            int maxAP,
            string explanation)
        {
            ObjectName = objectName;
            SelectedRiskLabel = riskLabel;
            ExecutedActionLabel = actionLabel;
            RiskChange = riskChange;
            CurrentRisk = currentRisk;
            MaxRisk = maxRisk;
            ActionCost = actionCost;
            CurrentActionPoint = currentAP;
            MaxActionPoint = maxAP;
            Explanation = explanation;
        }
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
        public List<RiskAssessmentHistory> histories = new();

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

            var riskAssesmentHis = new RiskAssessmentHistory(history.ObjectName, history.SelectedRiskLable, history.ExecutedActionLabel, history.RiskChange, CurrentRiskAmount, maxRiskAmount, history.ActionCost, CurrentActionPoint, maxActionPoint, history.Explanation);
            histories.Add(riskAssesmentHis);
        }

        /*public ActionResultData ApplyAction(string objectId, string selectedActionLabel, TargetType type, string roomId)
        {
            if (!Entities.TryGetValue(objectId, out var entity))
            {
                Console.WriteLine($"{objectId}が存在しません");
                return new ActionResultData
                {
                    result = ActionResultType.Unknown
                };
            }

            if (!entity.TryGetComponent<ChoicableComponent>(out var choicable))
            {
                Console.WriteLine($"{entity.Id}はChoicableComponentを持っていません");
                return new ActionResultData
                {
                    result = ActionResultType.Unknown
                };
            }

            var action = choicable.SelectedChoice.OverrideActions.Find(a => a.label == selectedActionLabel);
            if (action == null) return null;
            if (CurrentActionPoint < action.actionPointCost)
            {
                Console.WriteLine("ActionPointが足りません.");
                return new ActionResultData
                {
                    result = ActionResultType.ShortageActionPoint
                };
            }

            //Action済みにする
            if (!entity.TryGetComponent<InspectableComponent>(out var inspectable))
                return null;
            if (inspectable.IsActioned) return null;
            inspectable.IsActioned = true;

            CurrentActionPoint -= action.actionPointCost;
            CurrentRiskAmount += action.riskChange;

            var selectedRiskLabel = choicable.SelectedChoice.Label;
            var history = new RiskAssessmentHistory(inspectable.DisplayName, selectedRiskLabel, action.label, action.riskChange, CurrentRiskAmount, maxRiskAmount, action.actionPointCost, CurrentActionPoint, maxActionPoint, action.Explanation);
            histories.Add(history);

            return new ActionResultData
            {
                result = ActionResultType.Success,
                target = action.target,
                actionId = action.id,
                entity = entity,
                RoomId = roomId,
                currentRiskAmount = CurrentRiskAmount,
                currentActionPointAmount = CurrentActionPoint,
                histories = histories,
            };
        }*/
    }
}
