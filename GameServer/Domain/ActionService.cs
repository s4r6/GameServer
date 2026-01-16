using GameServer.Domain.Object;
using GameServer.Domain.Object.Components;

namespace GameServer.Domain
{
    public class ActionService
    {
        public bool CanAction(ObjectEntity target, ObjectEntity held)
        {
            if (target.TryGetComponent<ActionHeld>(out var actionHeld) && held != null)
            {
                //Action可能か確認
                if (actionHeld.IsMatch(held) && !(held.TryGetComponent<InspectableComponent>(out var inspectable) && inspectable.IsActioned))
                {
                    return true;
                }
            }

            if (target.TryGetComponent<ActionSelf>(out var actionSelf))
            {
                if (actionSelf.IsMatch(target) && !(target.TryGetComponent<InspectableComponent>(out var targetInsp) && targetInsp.IsActioned))
                {
                    return true;
                }
            }

            return false;
        }

        //実行可能なアクションのリストを取得
        public List<ActionEntity> GetAvailableActions(ObjectEntity target, ObjectEntity held)
        {
            List<ActionEntity> availebleActions = new();

            if (target.TryGetComponent<ActionHeld>(out var actionHeld) && held != null)
            {
                var isMatch = actionHeld.IsMatch(held);
                if (held.TryGetComponent<InspectableComponent>(out var inspectable))
                {
                    if (inspectable.IsActioned)
                        isMatch = false;
                }


                if (isMatch)
                    availebleActions.AddRange(actionHeld?.GetAvailableActions(held));
            }

            if (target.TryGetComponent<ActionSelf>(out var actionSelf) && target != null)
            {
                var isMatch = actionSelf.IsMatch(target);
                if (target.TryGetComponent<InspectableComponent>(out var inspectable))
                {
                    if (inspectable.IsActioned)
                        isMatch = false;
                }


                if (isMatch)
                    availebleActions.AddRange(actionSelf?.GetAvailableActions(target));
            }

            return availebleActions;
        }

        //選択したアクションを適用
        public ActionResultType ApplyAction(string selectedActionLabel, ObjectEntity entity, Stage stage)
        {
            if (!entity.TryGetComponent<ChoicableComponent>(out var choicable))
            {
                Console.WriteLine($"{entity.Id}はChoicableComponentを持っていません");
                return ActionResultType.Unknown;
            }

            var action = choicable.SelectedChoice.OverrideActions.Find(a => a.label == selectedActionLabel);
            if (action == null)
            {
                Console.WriteLine("Actionが存在しません");
                return ActionResultType.Unknown;
            }

            if (!CanApplyAction(action, stage))
            {
                Console.WriteLine("ActionPointが足りません");
                return ActionResultType.ShortageActionPoint;
            }

            SetActionFlag(entity);

            var selectedRiskLabel = choicable.SelectedChoice.Label;
            var history = new ActionHistory(entity.Id, selectedRiskLabel, selectedActionLabel, action.riskChange, action.actionPointCost, action.Explanation);
            //アクションをStageに適用して履歴を保存
            stage.OnExecuteAction(history);

            return ActionResultType.Success;
        }

        public bool CanApplyAction(ActionEntity action, Stage stage)
        {
            return stage.CurrentActionPoint >= action.actionPointCost;
        }

        void SetActionFlag(ObjectEntity entity)
        {
            if (!entity.TryGetComponent<InspectableComponent>(out var inspectable))
                return;

            if (inspectable.IsActioned)
                return;

            inspectable.IsActioned = true;
        }
    }
}