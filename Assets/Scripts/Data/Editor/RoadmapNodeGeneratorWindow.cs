using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Data.Roadmap;
using Data.Quests;

namespace Data.Editor
{
    public class RoadmapNodeGeneratorWindow : EditorWindow
    {
        private string _nodeName = "Coffee";
        private int _levelCount = 6; 
        private int _buildingIndex = 1; 
        private Vector2 _scrollPosition;
        
        private class RewardEntrySetup
        {
            public RewardCategory Category = RewardCategory.Experience;
            public int Amount = 50;
            public ScriptableObject RewardItem; 
            public int ItemLevel = 1; 
        }

        private class LevelRewardSetup
        {
            public bool Foldout = true;
            public int UpgradeCost = 100; 
            public Sprite LevelSprite;
            public List<RewardEntrySetup> Rewards = new List<RewardEntrySetup>();
        }

        private List<LevelRewardSetup> _levelRewardSetups = new List<LevelRewardSetup>();
        private RoadmapDatabaseSO _targetDatabase;

        private const string BASE_PATH = "Assets/Data/Map/Nodes";

        [MenuItem("Tools/Roadmap/Node Generator")]
        public static void ShowWindow()
        {
            GetWindow<RoadmapNodeGeneratorWindow>("Node Generator");
        }

        private void OnEnable()
        {
            SyncRewardSetupList();
        }

        private void OnGUI()
        {
            GUILayout.Label("Roadmap Node & Algorithmic Economy Automation", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical("box");
            _nodeName = EditorGUILayout.TextField("Node Name", _nodeName);
            
            EditorGUI.BeginChangeCheck();
            _levelCount = EditorGUILayout.IntSlider("Level Count", _levelCount, 1, 10);
            if (EditorGUI.EndChangeCheck())
            {
                SyncRewardSetupList();
            }
            
            _buildingIndex = EditorGUILayout.IntSlider("Building Index", _buildingIndex, 1, 20);
            
            EditorGUILayout.Space();
            
            GUI.backgroundColor = new Color(0.2f, 0.8f, 0.2f);
            if (GUILayout.Button("Auto-Fill Economy (Based on Building Index)", GUILayout.Height(30)))
            {
                ApplyAlgorithmicEconomy();
            }
            GUI.backgroundColor = Color.white;
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            GUILayout.Label("Per-Level Configuration", EditorStyles.boldLabel);
            
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(300));
            bool isRewardConfigurationValid = true;

            for (int i = 0; i < _levelCount; i++)
            {
                LevelRewardSetup levelSetup = _levelRewardSetups[i];
                
                EditorGUILayout.BeginVertical("helpbox");
                EditorGUILayout.BeginHorizontal();
                
                levelSetup.Foldout = EditorGUILayout.Foldout(levelSetup.Foldout, $"Level {i + 1} Configuration", true);
                
                if (GUILayout.Button("+ Add Reward", GUILayout.Width(90)))
                {
                    levelSetup.Rewards.Add(new RewardEntrySetup());
                }
                EditorGUILayout.EndHorizontal();

                if (levelSetup.Foldout)
                {
                    EditorGUI.indentLevel++;
                    
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Level Sprite:", GUILayout.Width(130));
                    levelSetup.LevelSprite = (Sprite)EditorGUILayout.ObjectField(levelSetup.LevelSprite, typeof(Sprite), false, GUILayout.Width(150));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Upgrade Cost (Gold):", GUILayout.Width(130));
                    levelSetup.UpgradeCost = EditorGUILayout.IntField(levelSetup.UpgradeCost, GUILayout.Width(80));
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.Space(5);
                    
                    for (int j = 0; j < levelSetup.Rewards.Count; j++)
                    {
                        RewardEntrySetup reward = levelSetup.Rewards[j];

                        EditorGUILayout.BeginHorizontal();
                        reward.Category = (RewardCategory)EditorGUILayout.EnumPopup(reward.Category, GUILayout.Width(100));

                        bool isCurrency = reward.Category == RewardCategory.Gold || 
                                          reward.Category == RewardCategory.Gem || 
                                          reward.Category == RewardCategory.Energy || 
                                          reward.Category == RewardCategory.Experience;

                        if (!isCurrency)
                        {
                            reward.RewardItem = (ScriptableObject)EditorGUILayout.ObjectField(reward.RewardItem, typeof(BaseItemDefinitionSO), false, GUILayout.Width(110));
                            
                            GUILayout.Label("Lvl:", GUILayout.Width(25));
                            reward.ItemLevel = EditorGUILayout.IntField(reward.ItemLevel, GUILayout.Width(35));

                            if (reward.RewardItem == null)
                            {
                                isRewardConfigurationValid = false;
                            }
                        }

                        GUILayout.Label("Amt:", GUILayout.Width(30));
                        reward.Amount = EditorGUILayout.IntField(reward.Amount, GUILayout.Width(40));

                        if (GUILayout.Button("X", GUILayout.Width(25)))
                        {
                            levelSetup.Rewards.RemoveAt(j);
                            break;
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();
            _targetDatabase = (RoadmapDatabaseSO)EditorGUILayout.ObjectField("Target Database", _targetDatabase, typeof(RoadmapDatabaseSO), false);

            EditorGUILayout.Space();

            bool isValid = !string.IsNullOrWhiteSpace(_nodeName) && _targetDatabase != null && isRewardConfigurationValid;
            
            GUI.enabled = isValid;
            if (GUILayout.Button("Generate Node Suite", GUILayout.Height(40)))
            {
                GenerateNodeSuite();
            }
            GUI.enabled = true;

            if (!isValid)
            {
                EditorGUILayout.HelpBox("Validation Failed: Check node name, target database, and ensure all non-currency rewards have an assigned item reference.", MessageType.Error);
            }
        }

        private void SyncRewardSetupList()
        {
            while (_levelRewardSetups.Count < _levelCount)
            {
                _levelRewardSetups.Add(new LevelRewardSetup());
            }
            while (_levelRewardSetups.Count > _levelCount)
            {
                _levelRewardSetups.RemoveAt(_levelRewardSetups.Count - 1);
            }
        }

        private void ApplyAlgorithmicEconomy()
        {
            float baseCost = 50f;
            float buildingCostMultiplier = 1.6f;
            float levelCostMultiplier = 2.2f;
            
            float baseExp = 10f;
            float buildingExpMultiplier = 1.8f;
            float levelExpMultiplier = 2.0f;

            for (int i = 0; i < _levelCount; i++)
            {
                int currentLevel = i + 1;
                LevelRewardSetup setup = _levelRewardSetups[i];
                
                float rawCost = baseCost * Mathf.Pow(buildingCostMultiplier, _buildingIndex - 1) * Mathf.Pow(levelCostMultiplier, currentLevel - 1);
                setup.UpgradeCost = Mathf.RoundToInt(rawCost / 10f) * 10;

                setup.Rewards.Clear();

                float rawExp = baseExp * Mathf.Pow(buildingExpMultiplier, _buildingIndex - 1) * Mathf.Pow(levelExpMultiplier, currentLevel - 1);
                setup.Rewards.Add(new RewardEntrySetup
                {
                    Category = RewardCategory.Experience,
                    Amount = Mathf.RoundToInt(rawExp / 5f) * 5
                });

                if (currentLevel == _levelCount)
                {
                    setup.Rewards.Add(new RewardEntrySetup
                    {
                        Category = RewardCategory.Gem,
                        Amount = 5 * _buildingIndex
                    });
                }
                else if (currentLevel % 2 == 0)
                {
                    setup.Rewards.Add(new RewardEntrySetup
                    {
                        Category = RewardCategory.Energy,
                        Amount = Mathf.Clamp(10 * _buildingIndex, 10, 100)
                    });
                }
            }
            
            Debug.Log($"[Economy Engine] Economy successfully calculated for Building {_buildingIndex}.");
        }

        private void GenerateNodeSuite()
        {
            string formattedName = _nodeName.Replace(" ", "").ToLowerInvariant();
            string buildingId = $"building_{formattedName}";
            string nodeId = $"node_{formattedName}";
            
            string folderName = $"Building_{_nodeName.Replace(" ", "")}";
            string folderPath = $"{BASE_PATH}/{folderName}";

            EnsureFolderExists(BASE_PATH);
            EnsureFolderExists(folderPath);

            string buildingAssetPath = $"{folderPath}/BuildingDef_{_nodeName}.asset";
            BuildingDefinitionSO buildingDef = CreateBuildingDefinition(buildingAssetPath, buildingId);

            string nodeAssetPath = $"{folderPath}/MapNode_{_nodeName}.asset";
            MapNodeDefinitionSO mapNodeDef = CreateMapNodeDefinition(nodeAssetPath, buildingDef, nodeId);

            AddNodeToDatabase(mapNodeDef);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = mapNodeDef;

            Debug.Log($"[Roadmap Generator] Successfully generated '{_nodeName}'. Building ID: {buildingId}, Node ID: {nodeId}");
        }

        private BuildingDefinitionSO CreateBuildingDefinition(string assetPath, string buildingId)
        {
            BuildingDefinitionSO buildingDef = ScriptableObject.CreateInstance<BuildingDefinitionSO>();

            SetPrivatePropertyValue(buildingDef, "BuildingId", buildingId);
            SetPrivatePropertyValue(buildingDef, "BuildingName", _nodeName);

            List<BuildingLevelData> prefilledLevels = new List<BuildingLevelData>();
            
            for (int i = 0; i < _levelCount; i++)
            {
                var rewards = new List<BuildingRewardDefinition>();
                LevelRewardSetup levelSetup = _levelRewardSetups[i];

                foreach (var rewardEntry in levelSetup.Rewards)
                {
                    bool isCurrency = rewardEntry.Category == RewardCategory.Gold || 
                                      rewardEntry.Category == RewardCategory.Gem || 
                                      rewardEntry.Category == RewardCategory.Energy || 
                                      rewardEntry.Category == RewardCategory.Experience;

                    rewards.Add(new BuildingRewardDefinition
                    {
                        Category = rewardEntry.Category,
                        RewardItem = isCurrency ? null : rewardEntry.RewardItem as dynamic,
                        Level = isCurrency ? 1 : rewardEntry.ItemLevel,
                        Amount = rewardEntry.Amount
                    });
                }

                prefilledLevels.Add(new BuildingLevelData
                {
                    LevelSprite = levelSetup.LevelSprite,
                    UpgradeCost = levelSetup.UpgradeCost, 
                    LevelRewards = rewards
                });
            }
            SetPrivatePropertyValue(buildingDef, "Levels", prefilledLevels);

            AssetDatabase.CreateAsset(buildingDef, assetPath);
            return buildingDef;
        }

        private MapNodeDefinitionSO CreateMapNodeDefinition(string assetPath, BuildingDefinitionSO linkedBuilding, string nodeId)
        {
            MapNodeDefinitionSO mapNodeDef = ScriptableObject.CreateInstance<MapNodeDefinitionSO>();

            SetPrivatePropertyValue(mapNodeDef, "NodeId", nodeId);
            SetPrivatePropertyValue(mapNodeDef, "Building", linkedBuilding);

            AssetDatabase.CreateAsset(mapNodeDef, assetPath);
            return mapNodeDef;
        }

        private void AddNodeToDatabase(MapNodeDefinitionSO newNode)
        {
            Undo.RecordObject(_targetDatabase, "Add Node to Roadmap Database");

            List<MapNodeDefinitionSO> currentNodes = _targetDatabase.AllNodes;
            if (currentNodes == null)
            {
                currentNodes = new List<MapNodeDefinitionSO>();
            }

            currentNodes.Add(newNode);
            SetPrivatePropertyValue(_targetDatabase, "AllNodes", currentNodes);

            EditorUtility.SetDirty(_targetDatabase);
        }

        private void EnsureFolderExists(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parentFolder = Path.GetDirectoryName(path).Replace("\\", "/");
                string newFolderName = Path.GetFileName(path);
                
                if (!AssetDatabase.IsValidFolder(parentFolder))
                {
                    EnsureFolderExists(parentFolder);
                }

                AssetDatabase.CreateFolder(parentFolder, newFolderName);
            }
        }

        private void SetPrivatePropertyValue(object target, string propertyName, object value)
        {
            PropertyInfo prop = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(target, value);
            }
            else
            {
                FieldInfo field = target.GetType().GetField($"<{propertyName}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(target, value);
                }
                else
                {
                    Debug.LogError($"[Roadmap Generator] Property '{propertyName}' not found or is read-only.");
                }
            }
        }
    }
}