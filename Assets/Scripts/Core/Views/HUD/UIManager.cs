using System;
using Core.Economy;
using Core.StateMachine;
using UnityEngine;
using Core.StateMachine.States;

namespace Core.Views
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private MainMenuUI _mainMenuUI;
        [SerializeField] private GameplayUI _gameplayUI;
        
        private GameStateMachine _stateMachine;
        
        public event Action OnExitToMenuRequested;
        public event Action OnPlayRequested;

        public void Initialize(GameStateMachine stateMachine)
        {
            _gameplayUI.OnExitToMenuClicked += () => OnExitToMenuRequested?.Invoke();
            _mainMenuUI.OnPlayButtonClicked += () => OnPlayRequested?.Invoke();
            
            _stateMachine = stateMachine;
            _stateMachine.OnStateEntered += HandleStateChanged;
        }

        private void HandleStateChanged(Type stateType)
        {
            _mainMenuUI.Show(stateType == typeof(MainMenuState));
            _gameplayUI.Show(stateType == typeof(GameplayState));
        }

        private void OnDestroy()
        {
            _stateMachine.OnStateEntered -= HandleStateChanged;
        }
    }
}