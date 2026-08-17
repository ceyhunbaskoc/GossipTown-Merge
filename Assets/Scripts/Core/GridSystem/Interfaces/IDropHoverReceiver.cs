namespace Core.GridSystem
{
    public interface IDropHoverReceiver
    {
        void OnHoverEnter(IViewItem viewItem);
        void OnHoverExit(IViewItem viewItem);
    }
}