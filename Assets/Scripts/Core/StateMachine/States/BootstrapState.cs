using Core.Bootstrap;
using Core.SaveSystem;
using UnityEngine;

namespace Core.StateMachine.States
{
    public class BootstrapState : IState
    {
        private readonly GameStateMachine _stateMachine;
        private readonly GameBootstrapper _gameBootstrapper;

        public BootstrapState(GameStateMachine stateMachine, GameBootstrapper gameBootstrapper)
        {
            _stateMachine = stateMachine;
            _gameBootstrapper = gameBootstrapper;
        }

        public void Enter()
        {
            Debug.Log("[Bootstrap] Starting Services...");

            InitializeServices();
            
            GameSaveData savedData = SaveManager.LoadGame();
            bool isTutorialCompleted = savedData != null && 
                                       savedData.TutorialData != null && 
                                       savedData.TutorialData.CurrentStep == TutorialStep.Completed;

            if (!isTutorialCompleted)
            {
                _stateMachine.Enter<GameplayState>();
            }
            else
            {
                _stateMachine.Enter<MainMenuState>();
            }
        }

        private void InitializeServices()
        {
            _gameBootstrapper.InitializeGame();
            Application.targetFrameRate = 60;
        }

        public void Exit() { }
    }
}