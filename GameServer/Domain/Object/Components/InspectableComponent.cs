namespace GameServer.Domain.Object.Components
{
    public class InspectableComponent : GameComponent
    {
        public string DisplayName;
        public string Description;
        
        public bool IsActioned = false;

        public override GameComponent Clone()
        {
            return new InspectableComponent { DisplayName = this.DisplayName, Description = this.Description, IsActioned = this.IsActioned };
        }
    }
}