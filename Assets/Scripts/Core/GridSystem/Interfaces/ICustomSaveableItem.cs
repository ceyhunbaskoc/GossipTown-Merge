namespace Core.GridSystem
{
    public interface ICustomSaveableItem
    {
        string GetCustomStateJson();
        void LoadCustomStateFromJson(string json);
    }
}