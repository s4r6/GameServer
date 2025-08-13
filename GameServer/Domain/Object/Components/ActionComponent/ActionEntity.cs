namespace GameServer.Domain.Object.Components
{
    public class ActionEntity
    {
        public string id;
        public string label;
        public int riskChange;
        public int actionPointCost;

        public TargetType target = TargetType.Self;
        public List<string> ObjectAttributes = new();
        public string Explanation;
        public override string ToString()
        {
            return $"ActionEntity: {id}, {riskChange}, {actionPointCost}";
        }
    }
}