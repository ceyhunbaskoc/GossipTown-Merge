using System;
using System.Collections.Generic;
using UnityEngine;

namespace Data.Story
{
    [Serializable]
    public struct DialogueLine
    {
        public SpeakerDefinitionSO Speaker; 
        public bool IsPlayerSide; 
        [TextArea(2, 5)] public string Text;
        
        [Tooltip("True ise, diyalogdan sonra UI kapanır. Bir sonraki tetikleyiciye kadar hikaye bekler.")]
        public bool IsPausePoint; 
    }

    [Serializable]
    public class StoryChapter
    {
        public string ChapterId;
        public string ChapterTitle;
        public List<DialogueLine> Lines = new List<DialogueLine>();
    }

    [CreateAssetMenu(fileName = "GlobalStoryDatabase", menuName = "GameData/Story/GlobalStoryDatabase")]
    public class GlobalStoryDatabaseSO : ScriptableObject
    {
        public List<StoryChapter> Chapters = new List<StoryChapter>();
    }
}