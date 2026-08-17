using System.IO;
using UnityEngine;

namespace Core.SaveSystem
{
    public static class SaveManager
    {
        private static string SavePath => Application.persistentDataPath + "/MergeGameSave.json";

        public static void SaveGame(GameSaveData saveData)
        {
            saveData.LastSaveTimeTicks = System.DateTime.UtcNow.Ticks;
            string json = JsonUtility.ToJson(saveData, true);
            
            File.WriteAllText(SavePath, json);
            Debug.Log($"Game Saved: {SavePath}");
        }

        public static GameSaveData LoadGame()
        {
            if (File.Exists(SavePath))
            {
                string json = File.ReadAllText(SavePath);
                return JsonUtility.FromJson<GameSaveData>(json);
            }
            
            return null;
        }

        public static void DeleteSave()
        {
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
            }
        }
    }
}