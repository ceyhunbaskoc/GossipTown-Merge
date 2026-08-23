using System;
using System.Collections.Generic;
using Core.Factories;
using Core.GridSystem;
using Core.SaveSystem;
using Core.Services;
using Data;
using UnityEngine;

namespace Core.Bootstrap
{
    public class GameSessionBuilder
    {
        private readonly GridDataModel _gridModel;
        private readonly GridDataModel _backpackGridModel;
        private readonly GridItemDataFactory _dataFactory;
        private readonly ITimeManager _timeManager;

        public GameSessionBuilder(GridDataModel gridModel, GridItemDataFactory dataFactory, ITimeManager timeManager, GridDataModel backpackGridModel)
        {
            _gridModel = gridModel;
            _backpackGridModel = backpackGridModel;
            _dataFactory = dataFactory;
            _timeManager = timeManager;
        }

        public void BuildBoard(StartingBoardSetupSO defaultSetup)
        {
            GameSaveData savedData = SaveManager.LoadGame();

            if (savedData != null)
            {
                InitializeBoardFromSave(savedData.GridCells, _gridModel);
                InitializeBoardFromSave(savedData.BackpackGridCells, _backpackGridModel);
            }
            else
            {
                InitializeBoardFromStartingBoardData(defaultSetup.InitialCells, _gridModel);
                InitializeBoardFromStartingBoardData(defaultSetup.BackpackCells,  _backpackGridModel);
            }
        }
        
        private void InitializeBoardFromSave(List<CellSaveData> savedData, GridDataModel gridDataModel)
        {
            foreach (var cellSave in savedData)
            {
                gridDataModel.InitializeCellLock(cellSave.Position, cellSave.IsLocked);

                if (cellSave.HasItem && cellSave.ItemData != null)
                {
                    IGridItem loadedItem = _dataFactory.CreateItemDataFromSave(cellSave.ItemData);
            
                    if (loadedItem is ITimeTrackable timeTrackable && timeTrackable.TargetTimeTicks > DateTime.UtcNow.Ticks)
                    {
                        _timeManager.RegisterTimer(timeTrackable);
                    }

                    gridDataModel.TryPlaceObject(cellSave.Position, loadedItem);
                }
            }
        }
        
        private void InitializeBoardFromStartingBoardData(List<CellSetupData> setupData, GridDataModel gridDataModel)
        {
            if (setupData == null) return;

            foreach (var cellData in setupData)
            {
                gridDataModel.InitializeCellLock(cellData.Position, cellData.IsLocked);

                if (cellData.ItemDef != null && !string.IsNullOrEmpty(cellData.ItemDef.Id))
                {
                    IGridItem newItem = _dataFactory.CreateItemData(cellData.ItemDef.Id, cellData.ItemLevel);

                    gridDataModel.TryPlaceObject(cellData.Position, newItem);
                }
            }
        }
    }
}