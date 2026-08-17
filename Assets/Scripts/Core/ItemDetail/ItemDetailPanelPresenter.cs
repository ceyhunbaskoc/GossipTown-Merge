using System;
using System.Collections.Generic;
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
        private readonly ItemDetailPanelView _itemDetailPanelView;
        private readonly ItemDatabaseSO _itemDatabase;
        private readonly IObjectPool _objectPool;
        
        private readonly List<ItemDetailSlotView> _spawnedSlotElements = new List<ItemDetailSlotView>();
        

        public ItemDetailPanelPresenter(ItemDetailEventChannelSO itemDetailEventChannel, 
            ItemDetailPanelView itemDetailPanelView, 
            ItemDatabaseSO itemDatabase,
            IObjectPool objectPool)
        {
            _itemDetailEventChannel = itemDetailEventChannel;
            _itemDetailPanelView = itemDetailPanelView;
            _itemDatabase = itemDatabase;
            _objectPool = objectPool;

            _itemDetailEventChannel.OnEventRaised += _handleItemDetailRequested;
        }

        private void _handleItemDetailRequested(ItemDetailRequest itemDetailRequest)
        {
            BaseItemDefinitionSO itemDef = _itemDatabase.GetItemDef(itemDetailRequest.ItemId);
            string itemName = itemDef.ItemName;
            int level = itemDetailRequest.ItemLevel;
            
            _itemDetailPanelView.Setup(itemName);
            PopulateLevelElements(itemDef, level);
            _itemDetailPanelView.Show();
        }
        
        private void PopulateLevelElements(BaseItemDefinitionSO itemDef, int currentLevel)
        {
            DespawnAllElements();

            for (int i = 1; i <= itemDef.MaxLevel; i++)
            {
                ItemDetailSlotView elementView = _objectPool.SpawnUI<ItemDetailSlotView>(PoolObjectType.ItemDetailSlotView);
                
                elementView.Setup(itemDef.GetIcon(i), i, isUnlocked: i <= currentLevel , isCurrentLevel: i==currentLevel);
                
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
        }
    }
}