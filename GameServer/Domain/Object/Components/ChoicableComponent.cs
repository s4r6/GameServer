namespace GameServer.Domain.Object.Components
{
    public class ChoicableComponent : GameComponent
    {
        public List<Choice> Choices = new();
        // �I�����ꂽChoice��ێ��iUseCase�ōX�V�����j
        public Choice SelectedChoice;

        public override GameComponent Clone()
        {
            // Choices ���f�B�[�v�R�s�[�iOverrideActions ���܂߂āj
            var clonedChoices = Choices.Select(originalChoice =>
            {
                var clonedActions = originalChoice.OverrideActions
                    .Select(a => new ActionEntity
                    {
                        id = a.id,
                        label = a.label,
                        riskChange = a.riskChange,
                        actionPointCost = a.actionPointCost,
                        target = a.target,
                        ObjectAttributes = a.ObjectAttributes.ToList(),
                        Explanation = a.Explanation
                    })
                    .ToList();

                return new Choice
                {
                    Label = originalChoice.Label,
                    RiskId = originalChoice.RiskId,
                    OverrideActions = clonedActions
                };
            }).ToList();

            // SelectedChoice �����x����v�ŒT���čĐݒ�
            Choice newSelected = null;
            if (SelectedChoice != null)
            {
                newSelected = clonedChoices.FirstOrDefault(c => c.Label == SelectedChoice.Label);
            }

            return new ChoicableComponent
            {
                Choices = clonedChoices,
                SelectedChoice = newSelected,
            };
        }
    }
}