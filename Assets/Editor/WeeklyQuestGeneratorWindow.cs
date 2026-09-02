#if UNITY_EDITOR
using System.IO;
using Data; 
using Data.Quests;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class WeeklyQuestGeneratorWindow : EditorWindow
    {
        private string _baseSavePath = "Assets/Data/Quests/Generated";
        private WeeklyQuestDatabaseSO _questDatabase;
        private BaseItemDefinitionSO _chestItemDefinition;
        private int _weekIndex = 1;
        
        [MenuItem("Tools/Quest Generator")]
        public static void ShowWindow()
        {
            GetWindow<WeeklyQuestGeneratorWindow>("Quest Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("Weekly Quest Automation System", EditorStyles.boldLabel);
            
            EditorGUILayout.Space();
            _baseSavePath = EditorGUILayout.TextField("Base Save Path", _baseSavePath);
            _questDatabase = (WeeklyQuestDatabaseSO)EditorGUILayout.ObjectField("Target Database", _questDatabase, typeof(WeeklyQuestDatabaseSO), false);
            
            EditorGUILayout.Space();
            _chestItemDefinition = (BaseItemDefinitionSO)EditorGUILayout.ObjectField("Chest Item Definition", _chestItemDefinition, typeof(BaseItemDefinitionSO), false);
            
            EditorGUILayout.Space();
            _weekIndex = EditorGUILayout.IntSlider("Week Index", _weekIndex, 1, 52);
            
            EditorGUILayout.Space();
            
            GUI.enabled = _questDatabase != null && _chestItemDefinition != null && !string.IsNullOrEmpty(_baseSavePath);
            if (GUILayout.Button($"Generate Quests for Week {_weekIndex}", GUILayout.Height(40)))
            {
                GenerateWeek();
            }
            GUI.enabled = true;

            if (_chestItemDefinition == null)
            {
                EditorGUILayout.HelpBox("Please create a Chest Definition SO for the chest rewards.", MessageType.Warning);
            }
        }

        private void GenerateWeek()
        {
            int startDay = (_weekIndex - 1) * 7 + 1;
            int endDay = startDay + 6;
            
            string weekFolderName = $"Week_{_weekIndex}";
            string weekFolderPath = $"{_baseSavePath}/{weekFolderName}";

            if (!Directory.Exists(_baseSavePath)) Directory.CreateDirectory(_baseSavePath);
            if (!Directory.Exists(weekFolderPath)) Directory.CreateDirectory(weekFolderPath);
            AssetDatabase.Refresh();

            string configPath = $"{weekFolderPath}/{weekFolderName}_Config.asset";
            WeeklyQuestConfigSO weeklyConfig = AssetDatabase.LoadAssetAtPath<WeeklyQuestConfigSO>(configPath);

            if (weeklyConfig == null)
            {
                weeklyConfig = CreateInstance<WeeklyQuestConfigSO>();
                AssetDatabase.CreateAsset(weeklyConfig, configPath);
            }

            SerializedObject configSO = new SerializedObject(weeklyConfig);
            configSO.FindProperty("WeekId").stringValue = weekFolderName;

            SerializedProperty dailyGroupsProp = configSO.FindProperty("DailyGroups");
            dailyGroupsProp.ClearArray();

            for (int currentDay = startDay; currentDay <= endDay; currentDay++)
            {
                dailyGroupsProp.arraySize++;
                SerializedProperty newGroupProp = dailyGroupsProp.GetArrayElementAtIndex(dailyGroupsProp.arraySize - 1);
                
                newGroupProp.FindPropertyRelative("DayIndex").intValue = currentDay;

                QuestDefinitionSO q1 = CreateQuest(weekFolderPath, currentDay, QuestType.MergeItem);
                QuestDefinitionSO q2 = CreateQuest(weekFolderPath, currentDay, QuestType.CompleteOrder);
                QuestDefinitionSO q3 = CreateQuest(weekFolderPath, currentDay, QuestType.SpendGold);
                QuestDefinitionSO q4 = CreateQuest(weekFolderPath, currentDay, QuestType.UpgradeBuilding);

                SerializedProperty questsProp = newGroupProp.FindPropertyRelative("Quests");
                questsProp.ClearArray();
                questsProp.arraySize = 4;
                questsProp.GetArrayElementAtIndex(0).objectReferenceValue = q1;
                questsProp.GetArrayElementAtIndex(1).objectReferenceValue = q2;
                questsProp.GetArrayElementAtIndex(2).objectReferenceValue = q3;
                questsProp.GetArrayElementAtIndex(3).objectReferenceValue = q4;

                SerializedProperty rewardsProp = newGroupProp.FindPropertyRelative("DayCompletionRewards");
                rewardsProp.ClearArray();
                rewardsProp.arraySize = 1;
                SerializedProperty rewardConfigProp = rewardsProp.GetArrayElementAtIndex(0);

                RewardCategory dailyRewardCategory = currentDay % 7 == 0 ? RewardCategory.Gem : RewardCategory.Energy;
                int dailyRewardAmount = 0;

                if (dailyRewardCategory == RewardCategory.Gem)
                {
                    dailyRewardAmount = Mathf.Clamp(8 + (_weekIndex * 2), 10, 20); 
                }
                else
                {
                    dailyRewardAmount = Mathf.Clamp(10 + (_weekIndex * 5), 15, 30);
                }

                rewardConfigProp.FindPropertyRelative("Category").enumValueIndex = (int)dailyRewardCategory;
                rewardConfigProp.FindPropertyRelative("Amount").intValue = dailyRewardAmount;
                rewardConfigProp.FindPropertyRelative("Level").intValue = 1;
                rewardConfigProp.FindPropertyRelative("ItemDefinition").objectReferenceValue = null; 
            }

            configSO.ApplyModifiedProperties();

            AddWeekToDatabase(_questDatabase, weeklyConfig);

            EditorUtility.SetDirty(weeklyConfig);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void AddWeekToDatabase(WeeklyQuestDatabaseSO database, WeeklyQuestConfigSO weeklyConfig)
        {
            if (database == null) return;
            
            SerializedObject serializedObject = new SerializedObject(database);
            SerializedProperty allWeeksProp = serializedObject.FindProperty("<AllWeeks>k__BackingField");

            for (int i = 0; i < allWeeksProp.arraySize; i++)
            {
                if (allWeeksProp.GetArrayElementAtIndex(i).objectReferenceValue == weeklyConfig)
                    return; 
            }

            allWeeksProp.arraySize++;
            allWeeksProp.GetArrayElementAtIndex(allWeeksProp.arraySize - 1).objectReferenceValue = weeklyConfig;
            
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(database);
        }

        private QuestDefinitionSO CreateQuest(string folderPath, int absoluteDay, QuestType type)
{
    QuestDefinitionSO quest = CreateInstance<QuestDefinitionSO>();
    
    int targetAmount = 0;
    int medalReward = 0;
    QuestRewardConfig rewardConfig = new QuestRewardConfig();

    int currentWeek = ((absoluteDay - 1) / 7) + 1;

    switch (type)
    {
        case QuestType.MergeItem:
            targetAmount = 10 + (absoluteDay * 5);
            medalReward = 10 + currentWeek;
            
            int energyReward = Mathf.Clamp(5 + (currentWeek * 5), 10, 25);
            rewardConfig = new QuestRewardConfig { Category = RewardCategory.Energy, Amount = energyReward, Level = 1 };
            break;
            
        case QuestType.CompleteOrder:
            targetAmount = 2 + (absoluteDay / 3);
            medalReward = 15 + currentWeek;
            int goldReward = Mathf.Clamp(10 + ((currentWeek - 1) * 20), 10, 75);
            rewardConfig = new QuestRewardConfig { Category = RewardCategory.Gold, Amount = goldReward, Level = 1 };
            break;
            
        case QuestType.SpendGold:
            targetAmount = 100 * currentWeek;
            medalReward = 20 + currentWeek;
            
            int gemReward = Mathf.Clamp(2 + currentWeek, 3, 10);
            rewardConfig = new QuestRewardConfig { Category = RewardCategory.Gem, Amount = gemReward, Level = 1 };
            break;
            
        case QuestType.UpgradeBuilding:
            targetAmount = 1 + (absoluteDay / 5);
            medalReward = 30 + (currentWeek * 5);
            int chestLevel = Mathf.Clamp(1 + (absoluteDay / 7), 1, 3);
            
            rewardConfig = new QuestRewardConfig { 
                Category = RewardCategory.Chest, 
                Level = chestLevel, 
                Amount = 1,
                ItemDefinition = _chestItemDefinition 
            };
            break;
    }

    SerializedObject so = new SerializedObject(quest);
    so.FindProperty("<QuestId>k__BackingField").stringValue = $"Day{absoluteDay}_{type}";
    so.FindProperty("<Type>k__BackingField").enumValueIndex = (int)type;
    so.FindProperty("<TargetAmount>k__BackingField").intValue = targetAmount;
    so.FindProperty("<MedalReward>k__BackingField").intValue = medalReward;
    
    SerializedProperty rewardsList = so.FindProperty("<Rewards>k__BackingField");
    rewardsList.arraySize = 1;
    SerializedProperty firstReward = rewardsList.GetArrayElementAtIndex(0);
    
    firstReward.FindPropertyRelative("Category").enumValueIndex = (int)rewardConfig.Category;
    firstReward.FindPropertyRelative("Amount").intValue = rewardConfig.Amount;
    firstReward.FindPropertyRelative("Level").intValue = rewardConfig.Level;
    firstReward.FindPropertyRelative("ItemDefinition").objectReferenceValue = rewardConfig.ItemDefinition;

    so.ApplyModifiedProperties();

    string assetName = $"Day{absoluteDay}_{type}.asset";
    string fullPath = AssetDatabase.GenerateUniqueAssetPath($"{folderPath}/{assetName}");
    
    AssetDatabase.CreateAsset(quest, fullPath);
    return quest;
}
    }
}
#endif