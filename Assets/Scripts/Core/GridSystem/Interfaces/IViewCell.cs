namespace Core.GridSystem
{
    public enum CellVisualState
    {
        Unlocked,
        LockedObscured, 
        LockedUnlockable
    }
    public interface IViewCell
    {
        void SetState(CellVisualState state);
    }
}