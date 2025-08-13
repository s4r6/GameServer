namespace GameServer.Domain.Object.Components
{
    public class CarryableComponent : GameComponent
    {
        public override GameComponent Clone()
        {
            return new CarryableComponent();
        }
    }
}