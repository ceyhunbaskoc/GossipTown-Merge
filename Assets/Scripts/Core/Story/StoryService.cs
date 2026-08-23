using System;
using Data.Story;
using Core.SaveSystem;
using UnityEngine;

namespace Core.Story
{
    public class StoryService
    {
        private readonly GlobalStoryDatabaseSO _database;
        private StorySaveData _saveData;

        public StoryService(GlobalStoryDatabaseSO database)
        {
            _database = database;
            _saveData = new StorySaveData();
        }

        public void LoadSaveData(StorySaveData savedData)
        {
            _saveData = savedData ?? new StorySaveData();
            if (_database == null || _database.Chapters == null || _database.Chapters.Count == 0)
            {
                Debug.LogError("[StoryService] GlobalStoryDatabaseSO is missing or empty!");
                return;
            }

            if (_saveData.CurrentChapterIndex >= _database.Chapters.Count)
            {
                _saveData.IsStoryCompleted = true;
            }
            else
            {
                int currentChapterLineCount = _database.Chapters[_saveData.CurrentChapterIndex].Lines.Count;
                if (_saveData.CurrentLineIndex >= currentChapterLineCount)
                {
                    _saveData.CurrentLineIndex = currentChapterLineCount;
                }
            }
        }

        public StorySaveData GetSaveData() => _saveData;
        
        public bool IsStoryCompleted() => _saveData.IsStoryCompleted;

        public bool TryGetNextLine(out DialogueLine nextLine, out string currentChapterTitle)
        {
            nextLine = default;
            currentChapterTitle = string.Empty;

            if (_saveData.IsStoryCompleted || _saveData.CurrentChapterIndex >= _database.Chapters.Count)
            {
                _saveData.IsStoryCompleted = true;
                return false;
            }

            StoryChapter currentChapter = _database.Chapters[_saveData.CurrentChapterIndex];
            currentChapterTitle = currentChapter.ChapterTitle;
            
            if (_saveData.CurrentLineIndex >= currentChapter.Lines.Count)
            {
                _saveData.CurrentChapterIndex++;
                _saveData.CurrentLineIndex = 0;
                
                return TryGetNextLine(out nextLine, out currentChapterTitle);
            }

            nextLine = currentChapter.Lines[_saveData.CurrentLineIndex];
            _saveData.CurrentLineIndex++;
            
            return true;
        }
    }
}