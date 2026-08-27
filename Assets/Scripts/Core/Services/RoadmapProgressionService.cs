using System;
using System.Collections.Generic;
using Core.Economy;
using Core.LevelSystem;
using Core.SaveSystem;
using Data.Roadmap;
using UnityEngine;

namespace Core.Services
{
    public class RoadmapProgressionService : IDisposable
    {
        public event Action<MapNodeDefinitionSO, BuildingLevelData> OnNodeUpgraded;
        public event Action<MapNodeDefinitionSO> OnNodeUnlocked; 
        
        public event Action OnDataLoaded; 
        public event Action OnRewardSave; 

        private readonly Dictionary<string, MapNodeDefinitionSO> _allNodesMap;
        private readonly Dictionary<string, NodeSaveData> _nodeSaveMap;
        
        private RoadmapSaveData _currentSaveData;
        private readonly PlayerEconomyModel _economyModifier;
        private readonly IReadOnlyLevel _readOnlyLevel;

        public RoadmapProgressionService(
            IEnumerable<MapNodeDefinitionSO> allNodes, 
            PlayerEconomyModel economyModifier,
            IReadOnlyLevel readOnlyLevel)
        {
            _economyModifier = economyModifier;

            _economyModifier.OnGoldChanged += CheckAnyNodeUpgradeable;
            _readOnlyLevel = readOnlyLevel;
            
            _allNodesMap = new Dictionary<string, MapNodeDefinitionSO>();
            foreach(var node in allNodes)
            {
                _allNodesMap[node.NodeId] = node;
            }
            _nodeSaveMap = new Dictionary<string, NodeSaveData>();
            
            LoadSaveData(null);
        }

        public event Action<bool> OnAnyBuildingUpgradeCheck;

        public RoadmapSaveData GetSaveData()
        {
            return _currentSaveData;
        }

        public void LoadSaveData(RoadmapSaveData savedData)
        {
            _nodeSaveMap.Clear();
    
            _currentSaveData = savedData ?? new RoadmapSaveData();
            if (_currentSaveData.UnlockedNodes == null)
            {
                _currentSaveData.UnlockedNodes = new List<NodeSaveData>();
            }
            foreach(var nodeSave in _currentSaveData.UnlockedNodes)
            {
                _nodeSaveMap[nodeSave.NodeId] = nodeSave;
            }
    
            OnDataLoaded?.Invoke();
        }
        
        public void MarkRewardAsClaimed(string nodeId, int level)
        {
            if (_nodeSaveMap.TryGetValue(nodeId, out var nodeSave))
            {
                if (level > nodeSave.LastClaimedRewardLevel)
                {
                    nodeSave.LastClaimedRewardLevel = level;
                    OnRewardSave?.Invoke();
                }
            }
        }

        public int GetLastClaimedRewardLevel(string nodeId)
        {
            return _nodeSaveMap.TryGetValue(nodeId, out var data) ? data.LastClaimedRewardLevel : 0;
        }

        public int GetNodeCurrentLevel(string nodeId)
        {
            return _nodeSaveMap.TryGetValue(nodeId, out var data) ? data.CurrentLevel : 0;
        }

        public int GetNodeRequiredLevel(string nodeId)
        {
            if (!_allNodesMap.TryGetValue(nodeId, out var nodeDef)) return 0;
            return nodeDef.RequiredPlayerLevel;
        }

        public bool CanUpgradeNode(string nodeId)
        {
            if (!_allNodesMap.TryGetValue(nodeId, out var nodeDef)) return false;

            if (_readOnlyLevel.CurrentLevel < nodeDef.RequiredPlayerLevel)
            {
                return false;
            }
            if (nodeDef.RequiredPreviousNode != null)
            {
                int prevNodeLevel = GetNodeCurrentLevel(nodeDef.RequiredPreviousNode.NodeId);
                if (prevNodeLevel < nodeDef.RequiredPreviousNodeLevel)
                {
                    return false; 
                }
            }

            int currentLevel = GetNodeCurrentLevel(nodeId);
            if (currentLevel >= nodeDef.Building.MaxLevel) return false; 

            var nextLevelData = nodeDef.Building.GetLevelData(currentLevel + 1);
            if (nextLevelData == null || _economyModifier.Golds < nextLevelData.UpgradeCost) return false;

            return true;
        }

        public void CheckAnyNodeUpgradeable(int currentGold = 0)
        {
            foreach (var node in _allNodesMap)
            {
                MapNodeDefinitionSO nodeDef = node.Value;
                if (_readOnlyLevel.CurrentLevel < nodeDef.RequiredPlayerLevel)
                {
                    continue;
                }
                if (nodeDef.RequiredPreviousNode != null)
                {
                    int prevNodeLevel = GetNodeCurrentLevel(nodeDef.RequiredPreviousNode.NodeId);
                    if (prevNodeLevel < nodeDef.RequiredPreviousNodeLevel)
                    {
                        continue;
                    }
                }

                int currentLevel = GetNodeCurrentLevel(node.Key);
                if (currentLevel >= nodeDef.Building.MaxLevel) continue; 

                var nextLevelData = nodeDef.Building.GetLevelData(currentLevel + 1);
                if (nextLevelData == null || _economyModifier.Golds < nextLevelData.UpgradeCost) continue;

                OnAnyBuildingUpgradeCheck?.Invoke(true);
                return;
            }
            OnAnyBuildingUpgradeCheck?.Invoke(false);
        }
        
        public bool IsNodeUnlocked(string nodeId)
        {
            if (!_allNodesMap.TryGetValue(nodeId, out var nodeDef)) return false;

            if (GetNodeCurrentLevel(nodeId) > 0) return true;
            if (_readOnlyLevel.CurrentLevel < nodeDef.RequiredPlayerLevel)
            {
                return false;
            }

            if (nodeDef.RequiredPreviousNode != null)
            {
                int prevNodeLevel = GetNodeCurrentLevel(nodeDef.RequiredPreviousNode.NodeId);
                return prevNodeLevel >= nodeDef.RequiredPreviousNodeLevel;
            }

            return true;
        }

        public bool IsEnoughLevel(string nodeId)
        {
            if (!_allNodesMap.TryGetValue(nodeId, out var nodeDef)) return false;

            if (GetNodeCurrentLevel(nodeId) > 0) return true;
            if (_readOnlyLevel.CurrentLevel < nodeDef.RequiredPlayerLevel)
            {
                return false;
            }

            return true;
        }
        
        public void TryUpgradeNode(string nodeId)
        {
            if (!CanUpgradeNode(nodeId))
            {
                Debug.LogWarning($"[RoadmapProgression] Illegal Upgrade Trying: {nodeId}");
                return;
            }

            var nodeDef = _allNodesMap[nodeId];
            int currentLevel = GetNodeCurrentLevel(nodeId);
            var nextLevelData = nodeDef.Building.GetLevelData(currentLevel + 1);
            if (_currentSaveData == null || _currentSaveData.UnlockedNodes == null)
            {
                Debug.LogError("[RoadmapProgression] Save data not initialized!");
                return; 
            }
            if (!_economyModifier.TrySpendGold(nextLevelData.UpgradeCost))
            {
                return; 
            }

            bool isFirstUnlock = false;
            if (!_nodeSaveMap.TryGetValue(nodeId, out var nodeSave))
            {
                nodeSave = new NodeSaveData { NodeId = nodeId, CurrentLevel = 0 };
                _nodeSaveMap[nodeId] = nodeSave;
                _currentSaveData.UnlockedNodes.Add(nodeSave);
                isFirstUnlock = true;
            }
            nodeSave.CurrentLevel++;
            OnNodeUpgraded?.Invoke(nodeDef, nextLevelData);
            
            if (isFirstUnlock)
            {
                OnNodeUnlocked?.Invoke(nodeDef);
            }
        }


        public void Dispose()
        {
            _economyModifier.OnGoldChanged -= CheckAnyNodeUpgradeable;
        }
    }
}