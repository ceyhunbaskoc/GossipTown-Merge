using System;
using System.Collections.Generic;
using Core.Discovery;
using Core.GridSystem;
using Core.PoolSystem;
using Data;
using Data.EventChannels;
using UI.ItemDetail;
using UnityEngine;

namespace Core.ItemDetail
{
    public class ItemDetailPanelPresenter : IDisposable
    {
        private readonly ItemDetailEventChannelSO _itemDetailEventChannel;
        private readonly ItemDetailEventChannelSO _spawnerDetailEventChannel;
        private readonly ItemDetailPanelView _itemDetailPanelView;
        private readonly ItemDatabaseSO _itemDatabase;
        private readonly IObjectPool _objectPool;
        private readonly IReadOnlyItemDiscovery _itemDiscovery;
        
        private readonly List<ItemDetailSlotView> _spawnedSlotElements = new List<ItemDetailSlotView>();
        
        public ItemDetailPanelPresenter(
            ItemDetailEventChannelSO itemDetailEventChannel, 
            ItemDetailEventChannelSO spawnerDetailEventChannel,
            ItemDetailPanelView itemDetailPanelView, 
            ItemDatabaseSO itemDatabase,
            IObjectPool objectPool,
            IReadOnlyItemDiscovery itemDiscovery)
        {
            _itemDetailEventChannel = itemDetailEventChannel;
            _spawnerDetailEventChannel = spawnerDetailEventChannel;
            _itemDetailPanelView = itemDetailPanelView;
            _itemDatabase = itemDatabase;
            _objectPool = objectPool;
            _itemDiscovery = itemDiscovery;

            _itemDetailEventChannel.OnEventRaised += _handleItemDetailRequested;
            _itemDetailPanelView.OnSpawnerDetailRequested += _handleSpawnerDetailRequested;
        }

        private void _handleItemDetailRequested(ItemDetailRequest itemDetailRequest)
        {
            BaseItemDefinitionSO itemDef = _itemDatabase.GetItemDef(itemDetailRequest.ItemId);
            if (itemDef == null) return;

            string itemName = itemDef.ItemName;
            int clickedLevel = itemDetailRequest.ItemLevel;

            SpawnerDefinitionSO sourceSpawnerDef = null;
            Sprite spawnerIcon = null;

            if (itemDef is ItemDefinitionSO normalItemDef)
            {
                sourceSpawnerDef = normalItemDef.SourceSpawner;
                    if (sourceSpawnerDef != null)
                    {
                        int discoveredSpawnerLevel = _itemDiscovery.GetMaxUnlockedLevelFor(sourceSpawnerDef.Id); 
                        spawnerIcon = sourceSpawnerDef.GetIcon(discoveredSpawnerLevel);
                    }
            }
            string spawnerId = sourceSpawnerDef != null ? sourceSpawnerDef.Id : string.Empty;
            _itemDetailPanelView.Setup(itemName, spawnerId, spawnerIcon);
            int maxDiscoveredLevel = _itemDiscovery.GetMaxUnlockedLevelFor(itemDef.Id);
            PopulateLevelElements(itemDef, clickedLevel, maxDiscoveredLevel);
            _itemDetailPanelView.Show();
        }

        private void _handleSpawnerDetailRequested(string spawnerId)
        {
            if (string.IsNullOrEmpty(spawnerId)) return;
            
            _spawnerDetailEventChannel.RaiseEvent(new ItemDetailRequest { ItemId = spawnerId, ItemLevel = 1 });
        }
        
        private void PopulateLevelElements(BaseItemDefinitionSO itemDef, int clickedLevel, int maxDiscoveredLevel)
        {
            DespawnAllElements();

            for (int i = 1; i <= itemDef.MaxLevel; i++)
            {
                ItemDetailSlotView elementView = _objectPool.SpawnUI<ItemDetailSlotView>(PoolObjectType.ItemDetailSlotView);
                bool isUnlocked = i <= maxDiscoveredLevel;
                bool isCurrentLevel = i == clickedLevel;
                elementView.Setup(itemDef.GetIcon(i), i, isUnlocked, isCurrentLevel);
                _itemDetailPanelView.AddSlotElement(elementView.transform);
                _spawnedSlotElements.Add(elementView);
            }
        }
        
        private void DespawnAllElements()
        {
            foreach (var element in _spawnedSlotElements)
            {
                _objectPool.Despawn(PoolObjectType.ItemDetailSlotView, element);
            }
            _spawnedSlotElements.Clear();
        }
        
        public void Dispose()
        {
            _itemDetailEventChannel.OnEventRaised -= _handleItemDetailRequested;
            _itemDetailPanelView.OnSpawnerDetailRequested -= _handleSpawnerDetailRequested;
            DespawnAllElements();
        }
    }
}