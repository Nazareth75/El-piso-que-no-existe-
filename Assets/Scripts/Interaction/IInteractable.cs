namespace Piso7.Interaction
{
    public interface IInteractable
    {
        string Prompt { get; }
        bool CanInteract { get; }
        void Interact(PlayerInteractor interactor);
    }
}
