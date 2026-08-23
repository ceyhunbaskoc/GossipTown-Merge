using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Data;
using Data.Level;
using Data.Quests;

namespace Editor.Progression
{
    public class LevelRewardGeneratorWindow : EditorWindow
    {
        private LevelRewardSettingsSO _targetSO;

        private int _maxLevels = 200;
        
        [Header("Economy Settings")]
        private int _baseGold = 50;
        private int _goldIncrementPerLevel = 10;
        
        [Header("Chest Mechanics")]
        private BaseItemDefinitionSO _chestDefinition;
        private int _chestDropInterval = 10;
        
        [Header("Gem Mechanics")]
        private int _gemDropInterval = 5;
        private int _gemBaseAmount = 5;

        [MenuItem("Tools/Economy/Level Reward Generator")]
        public static void ShowWindow()
        {
            GetWindow<LevelRewardGeneratorWindow>("Reward Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("Level Reward Automator", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            _targetSO = (LevelRewardSettingsSO)EditorGUILayout.ObjectField("Target SO", _targetSO, typeof(LevelRewardSettingsSO), false);
            
            EditorGUILayout.Space();
            _maxLevels = EditorGUILayout.IntSlider("Max Levels", _maxLevels, 10, 1000);
            
            EditorGUILayout.Space();
            GUILayout.Label("Gold Economy (Base + (Level * Increment))", EditorStyles.label);
            _baseGold = EditorGUILayout.IntField("Base Gold", _baseGold);
            _goldIncrementPerLevel = EditorGUILayout.IntField("Gold Increment/Level", _goldIncrementPerLevel);

            EditorGUILayout.Space();
            GUILayout.Label("Milestone Rewards", EditorStyles.boldLabel);
            _chestDefinition = (BaseItemDefinitionSO)EditorGUILayout.ObjectField("Chest Item Def", _chestDefinition, typeof(BaseItemDefinitionSO), false);
            _chestDropInterval = EditorGUILayout.IntSlider("Chest Interval (Levels)", _chestDropInterval, 1, 50);
            
            EditorGUILayout.Space();
            _gemDropInterval = EditorGUILayout.IntSlider("Gem Interval (Levels)", _gemDropInterval, 1, 20);
            _gemBaseAmount = EditorGUILayout.IntField("Gem Amount", _gemBaseAmount);

            EditorGUILayout.Space();
            
            GUI.enabled = _targetSO != null;
            if (GUILayout.Button("Generate Rewards", GUILayout.Height(40)))
            {
                GenerateRewards();
            }
            GUI.enabled = true;
        }

        private void GenerateRewards()
        {
            Undo.RecordObject(_targetSO, "Generate Level Rewards");

            var generatedLevels = new LevelRewardSettings[_maxLevels];

            for (int i = 0; i < _maxLevels; i++)
            {
                int currentLevel = i + 1;
                var rewardList = new List<QuestRewardConfig>();

                int calculatedGold = _baseGold + (currentLevel * _goldIncrementPerLevel);
                rewardList.Add(new QuestRewardConfig
                {
                    Category = RewardCategory.Gold,
                    Amount = calculatedGold
                });

                if (currentLevel % _gemDropInterval == 0)
                {
                    rewardList.Add(new QuestRewardConfig
                    {
                        Category = RewardCategory.Gem,
                        Amount = _gemBaseAmount + (currentLevel / 10)
                    });
                }

                if (_chestDefinition != null && currentLevel % _chestDropInterval == 0)
                {
                    rewardList.Add(new QuestRewardConfig
                    {
                        Category = RewardCategory.Chest,
                        ItemDefinition = _chestDefinition,
                        Amount = 1,
                        Level = 1
                    });
                }

                generatedLevels[i] = new LevelRewardSettings
                {
                    Level = currentLevel,
                    RewardConfig = rewardList
                };
            }

            _targetSO.Editor_SetLevelRewards(generatedLevels);

            EditorUtility.SetDirty(_targetSO);
            AssetDatabase.SaveAssets();
        }
    }
}