namespace GameServer.Domain.Object.Components
{
    public interface IInteractable
    {
        void Interact();
        bool CanInteract();
    }
}