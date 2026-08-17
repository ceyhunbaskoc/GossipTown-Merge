using Core.Views;

namespace Core.StateMachine.States
{
    public class MainMenuState : IState
    {
        private readonly GameStateMachine _stateMachine;

        public MainMenuState(GameStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }

        public void Enter()
        {
            
        }
        public void Exit()
        {
            
        }
    }
}