using Core.Bootstrap;
using Core.CameraSystem;
using Core.Views;
using UI.Map;
using UnityEngine;

namespace Core.StateMachine.States
{
    public class StateController : MonoBehaviour
    {
        private GameStateMachine _stateMachine;
        public GameStateMachine StateMachine => _stateMachine;
        [Header("UI References")]
        [SerializeField] private UIManager _uiManager;
        [SerializeField] private BuildingUpgradeReadyButtonPresenter _buildingUpgradeReadyButtonPresenter;
        
        [Header("Dependencies")]
        [SerializeField] private GameBootstrapper _gameBootstrapper;
        
        [Header("Camera System")]
        [SerializeField] private CameraStateOrchestrator _cameraOrchestrator;
        
        private void Awake()
        {
            _stateMachine = new GameStateMachine();
            _uiManager.Initialize(_stateMachine);
            _cameraOrchestrator.Initialize(_stateMachine);

            _stateMachine.Register(new BootstrapState(_stateMachine, _gameBootstrapper));
            _stateMachine.Register(new MainMenuState(_stateMachine));
            _stateMachine.Register(new GameplayState(_stateMachine));
            
            
            _uiManager.OnPlayRequested += () => _stateMachine.Enter<GameplayState>();
            _uiManager.OnExitToMenuRequested += () => _stateMachine.Enter<MainMenuState>();
            _buildingUpgradeReadyButtonPresenter.OnBackToMenuRequested += () => _stateMachine.Enter<MainMenuState>();
        }

        private void Start()
        {
            _stateMachine.Enter<BootstrapState>();
        } 
        
        
    }
}