using Core.GridSystem;

namespace Core.Rules
{
    public interface IMergeValidator
    {
        bool CanMerge(IGridItem item1, IGridItem item2);
    }
}