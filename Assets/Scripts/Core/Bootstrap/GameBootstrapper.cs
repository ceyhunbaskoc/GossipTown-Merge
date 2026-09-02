using System.Threading.Tasks;
using Core.Achievements;
using Core.AdService;
using Core.Architecture;
using Core.Audio;
using Core.Backpack;
using Core.CameraSystem;
using Core.Controllers;
using Core.Discovery;
using Core.Economy;
using Core.Economy.Exchange;
using Core.Economy.Offers;
using Core.Economy.Purchasing;
using Core.Economy.Shop;
using Core.Factories;
using Core.GridSystem;
using Core.Haptics;
using Core.Interaction;
using Core.ItemDetail;
using Core.LevelSystem;
using Core.Login;
using Core.Map;
using Core.Milestones;
using Core.Orchestration;
using Core.PoolSystem;
using Core.Quests;
using Core.Reward;
using Core.Roadmap;
using Core.Rules;
using Core.SaveSystem;
using Core.Services;
using Core.Settings;
using Core.StateMachine.States;
using Core.Story;
using Core.Tutorial;
using Core.Views;
using Data;
using Data.Audio;
using Data.Economy.Shop;
using Data.EventChannels;
using Data.Level;
using Data.Login;
using Data.Milestones;
using Data.Quests;
using Data.Reward;
using Data.Roadmap;
using Data.Story;
using Data.Tutorial;
using Data.UI;
using Order;
using UI;
using UI.Backpack;
using UI.Boot;
using UI.Chest;
using UI.Collectible;
using UI.Components;
using UI.Economy;
using UI.FlightSystem;
using UI.Hints;
using UI.Info;
using UI.ItemDetail;
using UI.Level;
using UI.Login;
using UI.Map;
using UI.Milestones;
using UI.Orders;
using UI.Quests;
using UI.Rewards;
using UI.Settings;
using UI.Spawner;
using UI.Story;
using UI.Tutorial;
using UnityEngine;
using UnityEngine.Audio;

namespace Core.Bootstrap
{
    public class GameBootstrapper : MonoBehaviour
    {
        [Header("Grid Configuration")]
        [SerializeField, Min(1)] private int _gridWidth = 5;
        [SerializeField, Min(1)] private int _gridHeight = 5;
        [SerializeField, Min(0.1f)] private float _cellSize = 1f;
        
        [Header("Databases")]
        [SerializeField] private ItemDatabaseSO _itemDatabase;
        [SerializeField] private RewardSelectionConfigSO _rewardSelectionConfig;
        [SerializeField] private RoadmapDatabaseSO _roadmapDatabase;
        [SerializeField] private WeeklyQuestDatabaseSO _weeklyQuestDatabase;
        [SerializeField] private DailyLoginConfigSO _dailyLoginConfig;
        [SerializeField] private MilestoneConfigSO _milestoneLoginConfig;
        [SerializeField] private MilestoneConfigSO _milestoneQuestConfig;
        [SerializeField] private GlobalRewardIconDatabaseSO _globalRewardIconDatabase;
        [SerializeField] private GlobalStoryDatabaseSO _globalStoryDatabase;
        [SerializeField] private LevelProgressionSettingsSO _levelProgressionSettings;
        [SerializeField] private LevelRewardSettingsSO _levelRewardSettings;
        [SerializeField] private CharacterSpriteDatabaseSO _characterSpriteDatabase;
        [SerializeField] private FloatingTextConfigSO _floatingTextConfig;
        [SerializeField] private ExchangePairSO _exchangePair;
        [SerializeField] private AdShopPackagesDatabaseSO _adShopPackagesDatabase;
        [SerializeField] private TutorialHelperTextsSO _tutorialHelperTexts;
        [SerializeField] private SfxDatabaseSO _sfxDatabase;
        
        [Header("Event Channels")]
        [SerializeField] private ItemDetailEventChannelSO _itemDetailEventChannel;
        [SerializeField] private ItemDetailEventChannelSO _spawnerDetailEventChannel;
        
        [Header("Starting Data")]
        [SerializeField] private StartingBoardSetupSO _currentStartingBoardData;
        
        [Header("Tutorial System")]
        [SerializeField] private StartingOrderSetupSO _startingOrderSetup;
        [SerializeField] private TutorialUIView _tutorialUIView;

        [Header("Controllers")]
        [SerializeField] private MainBoardController _mainBoardController;
        [SerializeField] private ObjectPoolManager _objectPoolManager;
        [SerializeField] private MergeItemFactory _mergeItemFactory;
        [SerializeField] private MapCameraController _mapCameraController;
        [SerializeField] private CurrencyFlightService _currencyFlightService;
        [SerializeField] private CollectibleCollectOrchestrator _collectibleCollectOrchestrator;
        
        [Header("UI Managers")]
        [SerializeField] private UIManager _uiManager;
        [SerializeField] private HUDManager _hudManager;
        [SerializeField] private GridVisualizer _gridVisualizer;
        [SerializeField] private RewardPresentationManager _rewardPresentationManager;
        [SerializeField] private RewardQueueView _rewardQueueView;
        [SerializeField] private ChestRewardHandler _chestRewardHandler;
        [SerializeField] private SpawnerRestingHandler _spawnerRestingHandler;
        [SerializeField] private ChestOpeningOrchestrator _chestOpeningOrchestrator;
        [SerializeField] private BackpackDropZone _backpackDropZone;
        [SerializeField] private BackpackPanelController _backpackPanelController;
        [SerializeField] private AchievementPanelController _achievementPanelController;
        [SerializeField] private ItemInfoPanelController _itemInfoPanelController;
        [SerializeField] private RoadmapUIController _roadmapUIController;
        [SerializeField] private BuildingUpgradePresenter _buildingUpgradePresenter;
        [SerializeField] private BoardBackdropView _boardBackdropView;
        [SerializeField] private WeeklyQuestPanelPresenter _weeklyQuestPanelPresenter;
        [SerializeField] private QuestNotificationButtonPresenter _questNotificationButtonPresenter;
        [SerializeField] private DailyLoginPanelPresenter _dailyLoginPanelPresenter;
        [SerializeField] private MilestoneProgressBarPresenter _milestoneLoginProgressBarPresenter;
        [SerializeField] private MilestoneProgressBarPresenter _milestoneQuestProgressBarPresenter;
        [SerializeField] private StoryPanelPresenter _storyPanelPresenter;
        [SerializeField] private BoardSelectionVisualizer _boardSelectionVisualizer;
        [SerializeField] private BuildingLevelCheckPresenter _buildingLevelCheckPresenter;
        [SerializeField] private LevelUIPresenter _levelUIPresenter;
        [SerializeField] private SingleWarningTextView _singleWarningTextView;
        [SerializeField] private SettingsUIPresenter _settingsUIPresenter;
        [SerializeField] private ExchangeItemUIPresenter _exchangeItemUIPresenter;
        [SerializeField] private ShopAdUIPresenter _shopAdUIPresenter;
        [SerializeField] private ItemDetailPanelView _itemDetailPanelView;
        [SerializeField] private SpawnerDetailPanelView _spawnerDetailPanelView;
        [SerializeField] private BuildingUpgradeReadyButtonPresenter _buildingUpgradeReadyButtonPresenter;
        [SerializeField] private FreeSpawnerRefillPanelPresenter _freeSpawnerRefillPanelPresenter;
        [SerializeField] private FreeEnergyRefillPanelPresenter _freeEnergyRefillPanelPresenter;
        [SerializeField] private EnergyTimerHUDView _energyTimerHUDView;
        [SerializeField] private SplashPanelView _splashPanelView;

        [SerializeField] private GlobalInputBlockerView _globalInputBlockerView;
        
        [Header("Order System")]
        [SerializeField] private OrderFulfillmentOrchestrator _fulfillmentOrchestrator;
        [SerializeField] private OrderUIManager _orderUIManager;
        
        [Header("State Machine")]
        [SerializeField] private StateController _stateController;

        [Header("Ad")] 
        [SerializeField] private string _adUnitId;

        [Header("Map")] 
        [SerializeField] private ChunkManager _chunkManager;

        // Core Models & Services
        private PlayerEconomyModel _economyModel;
        private GridDataModel _gridModel;
        private GridDataModel _backpackGridModel;
        private OrderDataModel _orderDataModel;
        private PendingRewardModel _pendingRewardModel;
        private AchievementRewardModel _achievementRewardModel;
        private ItemDiscoveryModel _itemDiscoveryModel;
        
        // Trackers & Controllers (Class Fields for Lifecycle Management)
        private DiscoveryController _discoveryController;
        private BoardInventoryTracker _inventoryTracker;
        private OrderMilestoneTracker _milestoneTracker;
        private RewardPlacementController _rewardPlacementController;
        private ITimeManager _timeManager;
        private RoadmapRewardIntegrator _roadmapRewardIntegrator;
        private WeeklyQuestService _weeklyQuestService;
        private AdMobRewardedService _adService;
        
        [Header("Hint System")]
        [SerializeField] private HintVisualOrchestrator _hintVisualOrchestrator;
        
        [Header("Audio")]
        [SerializeField] private AudioMixer _audioMixer;
        [SerializeField] private AudioClip _backgroundMusic;
        [SerializeField] private AudioSource _audioMusicSource;
        [SerializeField] private AudioSource _audioSFXSource;
        
        [SerializeField] private int _minimumSplashTimeMs = 1500;
        private IdleMonitorService _idleMonitorService;
        private IdleHintController _idleHintController;

        private RoadmapProgressionService _roadmapProgressionService;
        private QuestEventIntegrator _questEventIntegrator;
        private DailyLoginService _dailyLoginService;
        private DailyLoginPopupOrchestrator _dailyLoginPopupOrchestrator;
        private MilestoneService _milestoneServiceLogin;
        private MilestoneService _milestoneServiceQuest;
        private StoryService _storyService;
        private StoryTriggerController _storyTriggerController;
        private LevelService _levelService;
        private LevelRewardService _levelRewardService;
        private ISettingsService _settingsService;
        private AdShopService _adShopService;
        private ItemDetailPanelPresenter _itemDetailPanelPresenter;
        private SpawnerDetailPanelPresenter _spawnerDetailPanelPresenter;
        private TutorialOrchestrator _tutorialOrchestrator;
        private FreeEnergyRefillService _freeEnergyRefillService;
        private EnergyRegenerationService _energyRegenerationService;
        
        private OrderWaveController _waveController;
        private OrderGenerationService _orderGenerator;
        private RewardDispatcherService _rewardDispatcher;
        private RewardSelectionService _rewardSelectionService;
        private AudioService _audioService;
        private EconomyAudioController _economyAudioController;
        private IHapticService _hapticService;

        private Camera _mainCamera;
        
        private async Task InitializeGameAsync()
        {
            _splashPanelView.Show();
            Task mapLoadTask = _chunkManager.WaitForInitialLoadAsync();
            Task minimumDelayTask = Task.Delay(_minimumSplashTimeMs);

            if(_tutorialOrchestrator.IsCompleted)
                await Task.WhenAll(mapLoadTask, minimumDelayTask);
            else
                await Task.WhenAll(minimumDelayTask);
            
            _splashPanelView.FadeOutAndHide(() => 
            {
                Debug.Log("[Bootstrapper] Map loaded and splash closed.");
            });
        }

        public async void InitializeGame()
        {
            _mainCamera = Camera.main;
            _objectPoolManager.InitializePools();
            _gridVisualizer.Initialize(_objectPoolManager);

            _audioService = new AudioService(_sfxDatabase, _audioMusicSource, _audioSFXSource, _backgroundMusic);
            ServiceLocator.Register<IAudioService>(_audioService);
            
            IMergeValidator mergeValidator = new StandardMergeValidator(_itemDatabase);
            _mergeItemFactory.Initialize(_objectPoolManager, mergeValidator);
            
            _itemDatabase.InitializeDatabase();

            _timeManager = new TimeManager();
            IInputLockService inputLockService = new InputLockService();
            _globalInputBlockerView.Initialize(inputLockService);
            
            GridItemDataFactory gridItemDataFactory = new GridItemDataFactory(_itemDatabase);
            
            GameSaveData savedData = SaveManager.LoadGame();

            int startEnergy;
            int startGem;
            int startGold;
            if (savedData != null)
            {
                startEnergy = savedData.Energy;
                startGem = savedData.Gem;
                startGold = savedData.Gold;
            }
            else
            {
                startEnergy = 35;
                startGem = 5;
                startGold = 0;
            }
            _economyModel = new PlayerEconomyModel(startEnergy, startGem, startGold, 100);
            _economyAudioController = new EconomyAudioController(_economyModel, _audioService);

            long savedEnergyTicks = savedData != null ? savedData.EnergyTargetTimeTicks : 0;
            _energyRegenerationService = new EnergyRegenerationService(
                _economyModel, 
                _economyModel, 
                _timeManager, 
                120,
                savedEnergyTicks
            );
            _energyTimerHUDView.Initialize(_energyRegenerationService, _economyModel);
            
            _gridModel = new GridDataModel(_gridWidth, _gridHeight);
            _backpackGridModel = new GridDataModel(4, 8);

            _orderDataModel = new OrderDataModel();
            if (savedData != null && savedData.Orders != null && savedData.Orders.Count > 0)
            {
                _orderDataModel.LoadSaveData(savedData.Orders);
            }

            SettingsSaveData settingsSaveData = savedData != null ? savedData.SettingsData : null;
            _settingsService = new SettingsService(_audioMixer, settingsSaveData);
            
            _hapticService = new NiceVibrationsWrapperService(_settingsService);
            
            IWarningMessageService warningMessageService = new WarningMessageService(_singleWarningTextView, _floatingTextConfig, _hapticService);
            
            _settingsUIPresenter.Initialize(_settingsService);
            
            _currencyFlightService.Initialize(_hudManager, _objectPoolManager, _globalRewardIconDatabase);
            
            CurrencyExchangeService currencyExchangeService = new CurrencyExchangeService(_economyModel);
            _exchangeItemUIPresenter.Initialize(_exchangePair, currencyExchangeService, _currencyFlightService, warningMessageService);

            _adService = new AdMobRewardedService(_adUnitId);
            MainThreadDispatcher.Initialize();
            IPurchaseStrategy purchaseStrategy = new RewardedAdPurchaseStrategy(_adService);
            _adShopService = new AdShopService(purchaseStrategy, _economyModel);
            if (savedData != null && savedData.AdShopData != null)
            {
                _adShopService.LoadSaveData(savedData.AdShopData);
            }
            _shopAdUIPresenter.Initialize(_adShopService, _adShopPackagesDatabase, _objectPoolManager);
            
            

            _itemDiscoveryModel = new ItemDiscoveryModel();
            if (savedData != null && savedData.DiscoveredItems != null)
            {
                _itemDiscoveryModel.LoadSaveData(savedData.DiscoveredItems);
            }
            _discoveryController = new DiscoveryController(_gridModel, _itemDiscoveryModel);
            
            GameSessionBuilder sessionBuilder = new GameSessionBuilder(_gridModel, gridItemDataFactory,_timeManager, _backpackGridModel);
            sessionBuilder.BuildBoard(_currentStartingBoardData);
            
            MergeService mergeService = new MergeService(_gridModel, _gridModel, mergeValidator, gridItemDataFactory);
            GeneratorService generatorService = new GeneratorService(_gridModel, _gridModel, _itemDatabase, _economyModel, gridItemDataFactory,_timeManager);
            
            ILootGenerationService lootGenerationService = new LootGenerationService();

            ChestInteractionService chestInteractionService = new ChestInteractionService(lootGenerationService, _gridModel, _itemDatabase, gridItemDataFactory, _mainBoardController,
                _chestOpeningOrchestrator, _economyModel, _timeManager, _mergeItemFactory, _adService, _audioService);
            
            ItemInteractionService interactionService = new ItemInteractionService(_gridModel, 
                _itemDatabase, 
                _economyModel,
                generatorService, 
                chestInteractionService, 
                _audioService,
                _hapticService);

            BoardTransferService boardTransferService = new BoardTransferService(_gridModel, _backpackGridModel);

            BoardSelectionService boardSelectionService = new BoardSelectionService(_gridModel, _itemDatabase);
            
            _itemInfoPanelController.Initialize(boardSelectionService, _gridModel);

            _chestRewardHandler.Initialize(boardSelectionService, chestInteractionService, warningMessageService);

            MergeVFXOrchestrator mergeVFXOrchestrator = new MergeVFXOrchestrator(_objectPoolManager);
            
            TutorialSaveData tutorialSaveData = savedData != null ? savedData.TutorialData : new TutorialSaveData();
            
            _tutorialOrchestrator = new TutorialOrchestrator(
                tutorialSaveData, 
                _gridModel, 
                _orderDataModel, 
                _tutorialUIView,
                _orderUIManager,
                _tutorialHelperTexts,
                _cellSize
            );
            
            SpawnerRestingService spawnerRestingService =
                new SpawnerRestingService(_itemDatabase, _economyModel, _adService);

            IFirstTimeOfferService firstTimeOfferService =
                new FirstTimeOfferService(tutorialSaveData, _tutorialOrchestrator, AutoSave);
            
            _spawnerRestingHandler.Initialize(boardSelectionService, spawnerRestingService, warningMessageService, firstTimeOfferService, _freeSpawnerRefillPanelPresenter);
            
            _freeEnergyRefillService = new FreeEnergyRefillService(_economyModel, _freeEnergyRefillPanelPresenter, firstTimeOfferService, 20);

            IMoveValidator moveValidator= new MoveValidator(_tutorialOrchestrator, _currentStartingBoardData);
            _mainBoardController.InitializeMainBoard(_gridModel, mergeService, interactionService, boardTransferService, boardSelectionService, warningMessageService, _collectibleCollectOrchestrator, _currencyFlightService ,_itemDatabase, mergeVFXOrchestrator, moveValidator, _audioService, _hapticService,  _gridWidth, _gridHeight, _cellSize);
            
            _backpackDropZone.Initialize(boardTransferService, _mainBoardController, warningMessageService, moveValidator,_audioService,_hapticService);
            
            _boardSelectionVisualizer.Initialize(boardSelectionService, _mainBoardController);

            BackpackUnlockerService backpackUnlockerService = new BackpackUnlockerService(_economyModel, _backpackGridModel);
            _backpackPanelController.Initialize(_backpackGridModel, boardTransferService, _itemDatabase, backpackUnlockerService, warningMessageService);
            
            LevelSaveData levelSaveData = savedData != null ? savedData.LevelData : null;
            _levelService = new LevelService(_levelProgressionSettings, levelSaveData);
            
            // AutoSave Bindings
            _settingsService.OnSettingsChanged += AutoSave;
            _gridModel.OnItemPlaced += (pos, item) => AutoSave();
            _gridModel.OnItemMoved += (fromPos, toPos, item) => AutoSave();
            _gridModel.OnItemSpawned += (fromPos, toPos, item) => AutoSave();
            _gridModel.OnCellCleared += (pos) => AutoSave();
            _gridModel.OnCellsUnlocked += (cells) => AutoSave();
            _economyModel.OnEnergyChanged += (newEnergy) => AutoSave();

            _hudManager.Initialize(_economyModel);
            
            _inventoryTracker = new BoardInventoryTracker(_gridModel, _orderDataModel);
            
            _fulfillmentOrchestrator.Initialize(_mergeItemFactory);
            
            IOrderFulfillmentService fulfillmentService = new OrderFulfillmentController(
                _mainBoardController, 
                _fulfillmentOrchestrator, 
                _orderDataModel, 
                _economyModel,
                _objectPoolManager,
                _audioService,
                _hapticService
            );
            IOrderCharacterSelector characterSelector = new OrderRandomCharacterSelector(_characterSpriteDatabase);
            
            _orderUIManager.Initialize(_orderDataModel, fulfillmentService, _objectPoolManager, _itemDatabase, _currencyFlightService, characterSelector, _itemDetailEventChannel);

            ScarcityWeightedSelectionStrategy scarcityWeightedSelectionStrategy =
                new ScarcityWeightedSelectionStrategy();

            DampedExponentialPricingStrategy orderPricingStrategy = new DampedExponentialPricingStrategy();
            _orderGenerator = new OrderGenerationService(
                _itemDiscoveryModel,
                _itemDatabase, 
                _orderDataModel,
                _inventoryTracker,
                scarcityWeightedSelectionStrategy,
                orderPricingStrategy
            );
            
            _pendingRewardModel = new PendingRewardModel();
            if (savedData != null && savedData.PendingRewards != null && savedData.PendingRewards.Count > 0)
            {
                _pendingRewardModel.LoadSaveData(savedData.PendingRewards); 
            }
            _rewardDispatcher = new RewardDispatcherService(_pendingRewardModel, _economyModel, _levelService);
            _rewardSelectionService =
                new RewardSelectionService(_gridModel, _rewardSelectionConfig, _itemDiscoveryModel, _itemDatabase);
            
            _milestoneTracker = new OrderMilestoneTracker(
                _orderDataModel, 
                _rewardDispatcher, 
                _rewardSelectionService,
                seriesGoal: 5
            );
            
            if (savedData != null && savedData.MilestoneData != null)
            {
                _milestoneTracker.LoadSaveData(savedData.MilestoneData); 
            }
            
            _levelRewardService = new LevelRewardService(_levelService, _levelRewardSettings, _rewardDispatcher);

            DynamicWaveRewardCalculator dynamicWaveRewardCalculator = new DynamicWaveRewardCalculator();

            _waveController = new OrderWaveController(
                _orderDataModel, 
                _orderGenerator, 
                dynamicWaveRewardCalculator,
                _levelService,
                minOrdersPerWave: 1, 
                baseMaxOrdersPerWave:3,
                wavesPerIncrement : 4,
                absoluteMaxOrdersLimit : 6
            );
            
            _rewardPlacementController = new RewardPlacementController(
                _pendingRewardModel, 
                _gridModel,
                gridItemDataFactory
            );
            
            _rewardPresentationManager.Initialize(_pendingRewardModel, _objectPoolManager, _itemDatabase, _stateController.StateMachine);
            _rewardQueueView.Initialize(_pendingRewardModel, _rewardPlacementController, _itemDatabase, warningMessageService);

            if (!_tutorialOrchestrator.IsCompleted)
            {
                if (_orderDataModel.ActiveOrderCount == 0)
                {
                    OrderModel tutorialOrder = _startingOrderSetup.CreateTutorialOrder();
                    _orderDataModel.TryCreateOrder(tutorialOrder);
                }
                Vector3 leftItemPos = _mainBoardController.GridToWorldPosition(_currentStartingBoardData.TutorialItem1Position);
                Vector3 rightItemPos = _mainBoardController.GridToWorldPosition(_currentStartingBoardData.TutorialItem2Position);
                
                _tutorialOrchestrator.StartCurrentStep(leftItemPos, rightItemPos);
            }
            else
            {
                if (_orderDataModel.ActiveOrderCount == 0)
                {
                    _waveController.GenerateNewWave();
                }
            }
            
            _idleMonitorService = new IdleMonitorService(idleThresholdSeconds: 2.5f);
            MergeHintService hintService = new MergeHintService(_gridModel, _itemDatabase);

            _idleHintController = new IdleHintController(
                _idleMonitorService, 
                hintService, 
                _hintVisualOrchestrator, 
                _mainBoardController,
                fulfillmentService
            );

            _achievementRewardModel = new AchievementRewardModel();
            if (savedData != null && savedData.ClaimedAchievements != null)
            {
                _achievementRewardModel.LoadSaveData(savedData.ClaimedAchievements);
            }
            _achievementPanelController.Initialize(_itemDatabase, _itemDiscoveryModel, _achievementRewardModel, _economyModel, inputLockService, _currencyFlightService);
            
            _storyService = new StoryService(_globalStoryDatabase);
            if (savedData != null && savedData.StoryData != null)
            {
                _storyService.LoadSaveData(savedData.StoryData);
            }
            
            _storyPanelPresenter.Initialize(_storyService, inputLockService);
            
            _roadmapProgressionService =
                new RoadmapProgressionService(_roadmapDatabase.AllNodes, _economyModel, _levelService);
            
            if (savedData != null && savedData.RoadmapData != null)
            {
                _roadmapProgressionService.LoadSaveData(savedData.RoadmapData);
            }

            _roadmapRewardIntegrator = new RoadmapRewardIntegrator(_roadmapProgressionService, _rewardDispatcher, _economyModel, _rewardSelectionService, _roadmapDatabase);

            _buildingUpgradePresenter.Initialize(_roadmapProgressionService, _economyModel, inputLockService, _objectPoolManager, _globalRewardIconDatabase,_audioService, _hapticService);
            Sprite unBuildSprite = _roadmapDatabase.UnBuildSprite;
            _roadmapUIController.Initialize(_roadmapProgressionService, _economyModel, inputLockService, _objectPoolManager, unBuildSprite,_audioService);
            
            _roadmapUIController.OnNodeUpgradeVisualCompleted += _roadmapRewardIntegrator.OnVisualUpgradeCompleted;
            
            _roadmapUIController.OnNodeClickedRequested += (nodeId) => 
            {
                var clickedNodeDef = _roadmapDatabase.AllNodes.Find(n => n.NodeId == nodeId);
    
                if (clickedNodeDef != null)
                {
                    _buildingUpgradePresenter.OpenPopupForNode(clickedNodeDef);
                    _buildingLevelCheckPresenter.OpenNotEnoughLevelPopup(clickedNodeDef);
                }
                else
                {
                    Debug.LogError($"[Bootstrapper] Node ID bulunamadı: {nodeId}");
                }
            };
            
            _buildingLevelCheckPresenter.Initialize(_roadmapProgressionService, inputLockService);
            
            _storyTriggerController = new StoryTriggerController(_roadmapUIController, _storyPanelPresenter, _storyService);
            
            _mapCameraController.Initialize(inputLockService);
            
            _chunkManager.Initialize(_mapCameraController);
            await InitializeGameAsync();
            
            MilestoneSaveData milestoneLoginSaveData = savedData != null ? savedData.DailyLoginMilestoneData : null;
            _milestoneServiceLogin = new MilestoneService(_milestoneLoginConfig, _rewardDispatcher, milestoneLoginSaveData);
            
            _milestoneLoginProgressBarPresenter.Initialize(_milestoneServiceLogin, _globalRewardIconDatabase);
            
            MilestoneSaveData milestoneQuestSaveData = savedData != null ? savedData.WeeklyQuestMilestoneData : null;
            _milestoneServiceQuest = new MilestoneService(_milestoneQuestConfig, _rewardDispatcher, milestoneQuestSaveData);
            
            _milestoneQuestProgressBarPresenter.Initialize(_milestoneServiceQuest, _globalRewardIconDatabase);
            
            _weeklyQuestService = new WeeklyQuestService(_weeklyQuestDatabase, _rewardDispatcher, _rewardSelectionService,_economyModel, _timeManager, _milestoneServiceQuest);

            if (savedData != null && savedData.WeeklyQuests != null)
            {
                _weeklyQuestService.LoadSaveData(savedData.WeeklyQuests);
            }

            _questEventIntegrator = new QuestEventIntegrator(_weeklyQuestService, _roadmapProgressionService, mergeService, _orderDataModel, _economyModel);

            _weeklyQuestPanelPresenter.Initialize(_weeklyQuestService, inputLockService, _globalRewardIconDatabase, _currencyFlightService, _objectPoolManager);
            
            _questNotificationButtonPresenter.Initialize(_weeklyQuestService);

            _questNotificationButtonPresenter.OnOpenPanelRequested += _weeklyQuestPanelPresenter.OpenPanel;
                
            
            _boardBackdropView.Initialize(boardSelectionService);
            
            DailyLoginSaveData loginSaveData = savedData != null ? savedData.DailyLoginData : null;
            _dailyLoginService = new DailyLoginService(_dailyLoginConfig, _rewardDispatcher, _timeManager, loginSaveData, _milestoneServiceLogin, _rewardSelectionService);
            
            _dailyLoginPanelPresenter.Initialize(_dailyLoginService, _dailyLoginConfig, inputLockService, _currencyFlightService);

            _dailyLoginPopupOrchestrator = new DailyLoginPopupOrchestrator(_dailyLoginService,
                _stateController.StateMachine, _dailyLoginPanelPresenter);
            
            _levelUIPresenter.Initialize(_levelService,
                _levelRewardSettings, 
                _levelProgressionSettings, 
                _globalRewardIconDatabase, 
                _roadmapDatabase, 
                inputLockService,
                _currencyFlightService,
                _objectPoolManager);

            _itemDetailPanelPresenter = new ItemDetailPanelPresenter(_itemDetailEventChannel, _spawnerDetailEventChannel, _itemDetailPanelView, _itemDatabase, _objectPoolManager, _itemDiscoveryModel);
            _spawnerDetailPanelPresenter = new SpawnerDetailPanelPresenter(_spawnerDetailEventChannel, _spawnerDetailPanelView, _itemDatabase, _objectPoolManager, _itemDiscoveryModel);
            
            _buildingUpgradeReadyButtonPresenter.Initialize(_roadmapProgressionService);


            _roadmapProgressionService.OnRewardSave += AutoSave;
            
            _milestoneServiceLogin.OnTierClaimed += (tier) => AutoSave();
            _milestoneServiceLogin.OnMedalCountChanged += (current, max) => AutoSave();

            _milestoneServiceQuest.OnTierClaimed += (tier) => AutoSave();
            _milestoneServiceQuest.OnMedalCountChanged += (current, max) => AutoSave();
            
            _dailyLoginService.OnRewardClaimed += (dayIndex) => AutoSave();
                
            _itemDiscoveryModel.OnItemUnlocked += (id, level) => AutoSave();
            _achievementRewardModel.OnRewardClaimed += () => AutoSave();
            
            _weeklyQuestService.OnQuestProgressChanged += (questDef, current, target) => AutoSave();
            _weeklyQuestService.OnQuestCompleted += (questDef) => AutoSave();
            _weeklyQuestService.OnQuestRewardClaimed += (questDef) => AutoSave();
        }
        
        private void AutoSave()
        {
            if (_gridModel == null || _economyModel == null) return;

            GameSaveData currentSave = new GameSaveData
            {
                Energy = _economyModel.Energy,
                Gem = _economyModel.Gems,
                Gold = _economyModel.Golds,
                GridCells = _gridModel.GetSaveData(),
                BackpackGridCells = _backpackGridModel.GetSaveData(),
                Orders = _orderDataModel.GetSaveData(),
                PendingRewards = _pendingRewardModel.GetSaveData(),
                MilestoneData = _milestoneTracker.GetSaveData(),
                DiscoveredItems = _itemDiscoveryModel.GetSaveData(),
                ClaimedAchievements = _achievementRewardModel.GetSaveData(),
                RoadmapData = _roadmapProgressionService.GetSaveData(),
                WeeklyQuests = _weeklyQuestService.GetSaveData(),
                DailyLoginData = _dailyLoginService.GetSaveData(),
                DailyLoginMilestoneData = _milestoneServiceLogin.GetSaveData(),
                WeeklyQuestMilestoneData = _milestoneServiceQuest.GetSaveData(),
                StoryData = _storyService.GetSaveData(),
                LevelData = _levelService.GetSaveData(),
                SettingsData = _settingsService.GetSaveData(),
                AdShopData = _adShopService.GetSaveData() ,
                TutorialData = _tutorialOrchestrator.GetSaveData(),
                EnergyTargetTimeTicks = _energyRegenerationService.TargetTimeTicks
            };

            SaveManager.SaveGame(currentSave);
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus) AutoSave();
        }
        
        private void OnApplicationQuit()
        {
            AutoSave();
        }

        private void OnDestroy()
        {
            _inventoryTracker?.Dispose();
            _milestoneTracker?.Dispose();
            _idleHintController?.Dispose();
            _idleMonitorService?.Dispose();
            _roadmapRewardIntegrator?.Dispose();
            _questEventIntegrator?.Dispose();
            _dailyLoginPopupOrchestrator?.Dispose();
            _discoveryController?.Dispose();
            _storyTriggerController?.Dispose();
            _adService.Dispose();
            _itemDetailPanelPresenter.Dispose();
            _spawnerDetailPanelPresenter.Dispose();
            _tutorialOrchestrator.Dispose();
            _roadmapProgressionService.Dispose();
            _freeEnergyRefillService.Dispose();
            _energyRegenerationService.Dispose();
            _chunkManager.Dispose();
            _economyAudioController.Dispose();
            _settingsService.OnSettingsChanged -= AutoSave;
            _roadmapProgressionService.OnRewardSave -= AutoSave;
            _roadmapUIController.OnNodeUpgradeVisualCompleted -= _roadmapRewardIntegrator.OnVisualUpgradeCompleted;
        }
    }
}