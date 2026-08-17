using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;
using Data.Story;

namespace Editor
{
    public static class StoryDatabaseImporter
    {
        private const string XML_PATH = "Assets/Data/StoryData.xml";
        private const string DATABASE_PATH = "Assets/Data/Story/GlobalStoryDatabase.asset";

        [MenuItem("Story/Import XML to Database")]
        public static void ImportStoryData()
        {
            if (!File.Exists(XML_PATH))
            {
                Debug.LogError($"[Story Importer] XML file not found! Path: {XML_PATH}");
                return;
            }

            GlobalStoryDatabaseSO database = AssetDatabase.LoadAssetAtPath<GlobalStoryDatabaseSO>(DATABASE_PATH);
            if (database == null)
            {
                database = ScriptableObject.CreateInstance<GlobalStoryDatabaseSO>();
                AssetDatabase.CreateAsset(database, DATABASE_PATH);
            }

            Dictionary<string, SpeakerDefinitionSO> speakerCache = LoadAllSpeakers();

            XDocument xmlDoc = XDocument.Load(XML_PATH);
            database.Chapters.Clear();

            foreach (XElement chapterNode in xmlDoc.Root.Elements("Chapter"))
            {
                StoryChapter newChapter = new StoryChapter
                {
                    ChapterId = chapterNode.Attribute("id")?.Value,
                    ChapterTitle = chapterNode.Attribute("title")?.Value,
                    Lines = new List<DialogueLine>()
                };

                foreach (XElement lineNode in chapterNode.Elements("Line"))
                {
                    string speakerId = lineNode.Attribute("speaker")?.Value;
                    bool isPlayerSide = bool.Parse(lineNode.Attribute("isPlayerSide")?.Value ?? "false");
                    bool isPausePoint = bool.Parse(lineNode.Attribute("isPausePoint")?.Value ?? "false");
                    string text = lineNode.Value;

                    speakerCache.TryGetValue(speakerId, out SpeakerDefinitionSO speakerDef);
                    
                    if (speakerDef == null)
                    {
                        Debug.LogWarning($"[Story Importer] Warning: '{speakerId}' not found!");
                    }

                    newChapter.Lines.Add(new DialogueLine
                    {
                        Speaker = speakerDef,
                        IsPlayerSide = isPlayerSide,
                        IsPausePoint = isPausePoint,
                        Text = text
                    });
                }

                database.Chapters.Add(newChapter);
            }

            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"<color=green>[Story Importer] Completed!</color> {database.Chapters.Count} chapters imported.");
        }
        private static Dictionary<string, SpeakerDefinitionSO> LoadAllSpeakers()
        {
            Dictionary<string, SpeakerDefinitionSO> cache = new Dictionary<string, SpeakerDefinitionSO>();
            
            string[] guids = AssetDatabase.FindAssets("t:SpeakerDefinitionSO");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                SpeakerDefinitionSO speaker = AssetDatabase.LoadAssetAtPath<SpeakerDefinitionSO>(path);
                
                if (speaker != null && !string.IsNullOrEmpty(speaker.SpeakerId))
                {
                    if (!cache.ContainsKey(speaker.SpeakerId))
                    {
                        cache.Add(speaker.SpeakerId, speaker);
                    }
                }
            }
            return cache;
        }
    }
}