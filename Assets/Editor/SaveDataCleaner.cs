#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class SaveDataCleaner
    {
        [MenuItem("Tools/Clear Save Data")]
        public static void ClearSaveData()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("[SaveDataCleaner] PlayerPrefs cleared successfully.");

            string persistentPath = Application.persistentDataPath;
            DirectoryInfo directoryInfo = new DirectoryInfo(persistentPath);

            if (!directoryInfo.Exists)
            {
                Debug.LogWarning("[SaveDataCleaner] Persistent data path does not exist yet.");
                return;
            }

            foreach (FileInfo fileInfo in directoryInfo.GetFiles())
            {
                fileInfo.Delete();
            }

            foreach (DirectoryInfo subDirectory in directoryInfo.GetDirectories())
            {
                subDirectory.Delete(true);
            }

            Debug.Log($"[SaveDataCleaner] All save files located in {persistentPath} have been deleted.");
        }
    }
}
#endif