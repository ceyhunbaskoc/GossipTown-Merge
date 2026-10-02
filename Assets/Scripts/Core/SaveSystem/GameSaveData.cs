using System;
using System.Collections.Generic;
using Order;
using UnityEngine;

namespace Core.SaveSystem
{
    [Serializable]
    public class ItemSaveData
    {
        public string Id;
        public int Level;
        
        public string CustomDataJson;
    }
    
    public enum TutorialStep
    {
        NotStarted = 0,
        MergeItems = 1,
        CompleteOrder = 2,
        Backpack = 3,
        BackToMainMenu = 4,
        ShowCoreLoop = 5,
        Completed = 6
    }

    [Serializable]
    public class TutorialSaveData
    {
        public TutorialStep CurrentStep = TutorialStep.NotStarted;
        
        public bool HasClaimedFreeEnergyRefill;
        public bool HasClaimedFreeSpawnerRefill;
    }
    
    [Serializable]
    public class SpawnerSaveState
    {
        public int CurrentCapacity;
        public long CooldownEndTimeTicks;
    }

    [Serializable]
    public class CellSaveData
    {
        public Vector2Int Position;
        public bool IsLocked;
        public bool HasItem;
        
        public ItemSaveData ItemData;
    }

    [Serializable]
    public class OrderItemSaveData
    {
        public string ItemId;
        public int Level;
        public int RequiredCount;
    }

    [Serializable]
    public class OrderSaveData
    {
        public List<OrderItemSaveData> RequiredItems = new List<OrderItemSaveData>();
        public int RewardAmount;
    }

    [Serializable]
    public class OrderMilestoneSaveData
    {
        public int CompletedOrdersInSeries;
    }

    [Serializable]
    public class RewardSaveData
    {
        public string ItemId;
        public int Level;
    }
    
    [Serializable]
    public class ItemDiscoverySaveData
    {
        public string ItemId;
        public int Level;
    }

    [Serializable]
    public class ClaimedAchievementSaveData
    {
        public string ItemId;
        public int Level;
    }
    
    [Serializable]
    public class ChestSaveState
    {
        public int CurrentState;
        public long UnlockTargetTimeTicks;
    }
    
    [Serializable]
    public class RoadmapSaveData
    {
        public List<NodeSaveData> UnlockedNodes = new List<NodeSaveData>();
    }

    [Serializable]
    public class NodeSaveData
    {
        public string NodeId;
        public int CurrentLevel;
        
        public int LastClaimedRewardLevel;
    }
    
    [Serializable]
    public class QuestProgressData
    {
        public string QuestId;
        public int CurrentAmount;
        public bool IsCompleted;
        public bool IsRewardClaimed;
    }

    [Serializable]
    public class WeeklyQuestSaveData
    {
        public string ActiveWeekId;
        public int CurrentActiveDayIndex;
        public long LastDayUpdateTimestampTicks;
        public List<QuestProgressData> QuestProgressList = new List<QuestProgressData>();
    }
    
    [Serializable]
    public class DailyLoginSaveData
    {
        public long LastClaimTimestampTicks; 
        public int TotalClaimedDays; 
    }
    
    [Serializable]
    public class MilestoneSaveData
    {
        public int CurrentMedals;
        public List<string> ClaimedTierIds = new List<string>();
    }

    [Serializable]
    public class StorySaveData
    {
        public int CurrentChapterIndex;
        public int CurrentLineIndex;
        public bool IsStoryCompleted;

        public StorySaveData()
        {
            CurrentChapterIndex = 0;
            CurrentLineIndex = 0;
            IsStoryCompleted = false;
        }
    }

    [Serializable]
    public class LevelSaveData
    {
        public int CurrentLevel;
        public int CurrentExperience;
    }
    
    [Serializable]
    public class SettingsSaveData
    {
        public float MusicVolume = 1f;
        public float SfxVolume = 1f;
        public bool IsMusicOn = true;
        public bool IsSfxOn = true;
        public bool IsHapticOn = true;
    }

    [Serializable]
    public class AdPackageWatchData
    {
        public string PackageId;
        public int WatchCount;
    }

    [Serializable]
    public class AdShopSaveData
    {
        public List<AdPackageWatchData> WatchCounts = new List<AdPackageWatchData>();
        
        public long LastResetTimestampTicks; 
    }

    [Serializable]
    public class GameSaveData
    {
        public int SaveVersion;
        public int Energy;
        public int Gem;
        public int Gold;
        public List<CellSaveData> GridCells = new List<CellSaveData>();
        public List<CellSaveData> BackpackGridCells = new List<CellSaveData>();
        public List<OrderSaveData> Orders = new List<OrderSaveData>();
        public List<RewardSaveData> PendingRewards = new List<RewardSaveData>();
        public OrderMilestoneSaveData MilestoneData;
        public RoadmapSaveData RoadmapData = new RoadmapSaveData();
        public DailyLoginSaveData DailyLoginData = new DailyLoginSaveData();
        public MilestoneSaveData WeeklyQuestMilestoneData = new MilestoneSaveData();
        public MilestoneSaveData DailyLoginMilestoneData = new MilestoneSaveData();
        public StorySaveData StoryData = new StorySaveData();
        public LevelSaveData LevelData = new LevelSaveData();
        public SettingsSaveData SettingsData = new SettingsSaveData();
        public AdShopSaveData AdShopData = new AdShopSaveData();
        public TutorialSaveData TutorialData = new TutorialSaveData();
        
        public List<ItemDiscoverySaveData> DiscoveredItems = new List<ItemDiscoverySaveData>();
        public List<ClaimedAchievementSaveData> ClaimedAchievements = new List<ClaimedAchievementSaveData>();
        public WeeklyQuestSaveData WeeklyQuests = new WeeklyQuestSaveData();
        
        public long LastSaveTimeTicks; 
        public long EnergyTargetTimeTicks;
    }
}