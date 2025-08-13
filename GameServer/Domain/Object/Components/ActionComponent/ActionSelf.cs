namespace GameServer.Domain.Object.Components
{
    public class ActionSelf : GameComponent
    {
        public bool IsMatch(ObjectEntity self)
        {
            if (!self.TryGetComponent<ChoicableComponent>(out var choicable))
                return false;

            if(choicable.SelectedChoice == null) return false;

            return true;
        }

        public List<ActionEntity> GetAvailableActions(ObjectEntity self)
        {
            if (!self.TryGetComponent<ChoicableComponent>(out var choicable))
                return new();

            if (choicable.SelectedChoice == null) return new();

            var availableActions = choicable.SelectedChoice.OverrideActions
                                                                .FindAll(action => action.target == TargetType.Self);

            return availableActions;
        }

        public override GameComponent Clone()
        {
            return new ActionSelf();
        }
    }
}