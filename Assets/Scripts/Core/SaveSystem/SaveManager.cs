using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Core.SaveSystem
{
    public static class SaveManager
    {
        private const int CurrentSaveVersion = 1;

        private static string SavePath => Application.persistentDataPath + "/MergeGameSave.json";
        private static string TempPath => SavePath + ".tmp";
        private static string BackupPath => SavePath + ".bak";
        private static string CorruptPath => SavePath + ".corrupt";

        public static void SaveGame(GameSaveData saveData)
        {
            saveData.SaveVersion = CurrentSaveVersion;
            saveData.LastSaveTimeTicks = DateTime.UtcNow.Ticks;
            string json = JsonUtility.ToJson(saveData, true);

            try
            {
                WriteAndFlushToDisk(TempPath, json);

                if (File.Exists(SavePath))
                {
                    if (File.Exists(BackupPath)) File.Delete(BackupPath);
                    File.Move(SavePath, BackupPath);
                }

                File.Move(TempPath, SavePath);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Save failed: {e}");
            }
        }

        public static GameSaveData LoadGame()
        {
            if (TryLoadFrom(SavePath, out GameSaveData saveData))
            {
                return saveData;
            }

            bool mainSaveWasUnreadable = File.Exists(SavePath);
            if (mainSaveWasUnreadable)
            {
                QuarantineUnreadableSave();
            }

            if (TryLoadFrom(BackupPath, out saveData))
            {
                Debug.LogWarning("[SaveManager] Main save missing or unreadable, restored from backup.");
                return saveData;
            }

            if (mainSaveWasUnreadable)
            {
                Debug.LogError("[SaveManager] Save and backup are both unreadable, starting a new game.");
            }

            return null;
        }

        public static void DeleteSave()
        {
            DeleteIfExists(SavePath);
            DeleteIfExists(BackupPath);
            DeleteIfExists(TempPath);
        }

        private static void WriteAndFlushToDisk(string path, string json)
        {
            byte[] bytes = new UTF8Encoding(false).GetBytes(json);

            using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush(true);
            }
        }

        private static bool TryLoadFrom(string path, out GameSaveData saveData)
        {
            saveData = null;

            try
            {
                if (!File.Exists(path)) return false;

                string json = File.ReadAllText(path);
                if (string.IsNullOrWhiteSpace(json)) return false;

                saveData = JsonUtility.FromJson<GameSaveData>(json);
                if (saveData == null) return false;

                Migrate(saveData);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Could not read '{path}': {e}");
                saveData = null;
                return false;
            }
        }

        private static void QuarantineUnreadableSave()
        {
            try
            {
                DeleteIfExists(CorruptPath);
                File.Move(SavePath, CorruptPath);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Could not quarantine unreadable save: {e}");
            }
        }

        private static void DeleteIfExists(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        private static void Migrate(GameSaveData saveData)
        {
            if (saveData.SaveVersion >= CurrentSaveVersion) return;

            if (saveData.SaveVersion < 1)
            {
                MigrateLegacyTutorialStep(saveData);
            }

            saveData.SaveVersion = CurrentSaveVersion;
        }

        private static void MigrateLegacyTutorialStep(GameSaveData saveData)
        {
            if (saveData.TutorialData == null) return;

            bool hasRoadmapProgress = saveData.RoadmapData != null &&
                                      saveData.RoadmapData.UnlockedNodes != null &&
                                      saveData.RoadmapData.UnlockedNodes.Count > 0;
            if (!hasRoadmapProgress) return;

            const int legacyShowCoreLoop = 3;
            const int legacyCompleted = 4;

            switch ((int)saveData.TutorialData.CurrentStep)
            {
                case legacyShowCoreLoop:
                    saveData.TutorialData.CurrentStep = TutorialStep.ShowCoreLoop;
                    break;
                case legacyCompleted:
                    saveData.TutorialData.CurrentStep = TutorialStep.Completed;
                    break;
            }
        }
    }
}
