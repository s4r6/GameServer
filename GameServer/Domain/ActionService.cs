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
                //Action�\���m�F
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

        //���s�\�ȃA�N�V�����̃��X�g���擾
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

        //�I�������A�N�V������K�p
        public ActionResultType ApplyAction(string selectedActionLabel, ObjectEntity entity, Stage stage)
        {
            if (!entity.TryGetComponent<ChoicableComponent>(out var choicable))
            {
                Console.WriteLine($"{entity.Id}��ChoicableComponent�������Ă��܂���");
                return ActionResultType.Unknown;
            }

            var action = choicable.SelectedChoice.OverrideActions.Find(a => a.label == selectedActionLabel);
            if (action == null)
            {
                Console.WriteLine("Action�����݂��܂���");
                return ActionResultType.Unknown;
            }

            if (!CanApplyAction(action, stage))
            {
                Console.WriteLine("ActionPoint������܂���");
                return ActionResultType.ShortageActionPoint;
            }

            SetActionFlag(entity);

            var selectedRiskLabel = choicable.SelectedChoice.Label;
            var history = new ActionHistory(entity.Id, selectedRiskLabel, selectedActionLabel, action.riskChange, action.actionPointCost, action.Explanation);
            //�A�N�V������Stage�ɓK�p���ė�����ۑ�
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