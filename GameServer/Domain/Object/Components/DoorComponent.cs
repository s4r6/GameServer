namespace GameServer.Domain.Object.Components
{
    public class DoorComponent : GameComponent, IInteractable
    {
        public bool IsOpen = true;
        public bool IsLock = false;

        public void Open()
        {
            IsOpen = !IsOpen; 
        }

        public bool CanUse()
        {
            return !IsLock;
        }

        public void Interact()
        {
            Open();
        }

        public bool CanInteract()
        {
            return CanUse();
        }

        public override GameComponent Clone()
        {
            return new DoorComponent { IsOpen = IsOpen, IsLock = IsLock };
        }
    }
}