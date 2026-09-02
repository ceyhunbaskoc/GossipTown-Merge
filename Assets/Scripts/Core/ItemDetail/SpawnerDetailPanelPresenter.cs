using System;
using System.Collections.Generic;
using Core.Discovery;
using Core.PoolSystem;
using Data;
using Data.EventChannels;
using UI.ItemDetail;
using UnityEngine;

namespace Core.ItemDetail
{
    public class SpawnerDetailPanelPresenter : IDisposable
    {
        private readonly ItemDetailEventChannelSO _spawnerDetailEventChannel;
        private readonly SpawnerDetailPanelView _spawnerDetailPanelView;
        private readonly ItemDatabaseSO _itemDatabase;
        private readonly IObjectPool _objectPool;
        private readonly IReadOnlyItemDiscovery _itemDiscovery;
        
        private readonly List<SpawnerDetailSlotView> _spawnedSlotElements = new List<SpawnerDetailSlotView>();
        
        public SpawnerDetailPanelPresenter(
            ItemDetailEventChannelSO spawnerDetailEventChannel,
            SpawnerDetailPanelView spawnerDetailPanelView, 
            ItemDatabaseSO itemDatabase,
            IObjectPool objectPool,
            IReadOnlyItemDiscovery itemDiscovery)
        {
            _spawnerDetailEventChannel = spawnerDetailEventChannel;
            _spawnerDetailPanelView = spawnerDetailPanelView;
            _itemDatabase = itemDatabase;
            _objectPool = objectPool;
            _itemDiscovery = itemDiscovery;

            _spawnerDetailEventChannel.OnEventRaised += _handleSpawnerDetailRequested;
        }

        private void _handleSpawnerDetailRequested(ItemDetailRequest spawnerDetailRequest)
        {
            Debug.Log("Spawner detail requested");
            BaseItemDefinitionSO itemDef = _itemDatabase.GetItemDef(spawnerDetailRequest.ItemId);
            if (itemDef == null) return;

            string itemName = itemDef.ItemName;
            
            _spawnerDetailPanelView.Setup(itemName);
            int maxDiscoveredLevel = _itemDiscovery.GetMaxUnlockedLevelFor(itemDef.Id);
            PopulateLevelElements(itemDef, maxDiscoveredLevel);
            _spawnerDetailPanelView.Show();
        }
        
        private void PopulateLevelElements(BaseItemDefinitionSO itemDef, int maxDiscoveredLevel)
        {
            DespawnAllElements();

            for (int i = 1; i <= itemDef.MaxLevel; i++)
            {
                SpawnerDetailSlotView elementView = _objectPool.SpawnUI<SpawnerDetailSlotView>(PoolObjectType.SpawnerDetailSlotView);
                bool isUnlocked = i <= maxDiscoveredLevel;
                elementView.Setup(itemDef.GetIcon(i), i, isUnlocked);
                _spawnerDetailPanelView.AddSlotElement(elementView.transform);
                _spawnedSlotElements.Add(elementView);
            }
        }
        
        private void DespawnAllElements()
        {
            foreach (var element in _spawnedSlotElements)
            {
                _objectPool.Despawn(PoolObjectType.SpawnerDetailSlotView, element);
            }
            _spawnedSlotElements.Clear();
        }


        public void Dispose()
        {
            _spawnerDetailEventChannel.OnEventRaised -= _handleSpawnerDetailRequested;
            DespawnAllElements();
        }
    }
}