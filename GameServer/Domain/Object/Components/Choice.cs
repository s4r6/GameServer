namespace GameServer.Domain.Object.Components
{
    public class Choice
    {
        public string Label { get; set; }
        public string RiskId { get; set; }

        public List<ActionEntity> OverrideActions { get; set; } = new();
    }
}