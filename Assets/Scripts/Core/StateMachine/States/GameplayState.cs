using Core.Views;

namespace Core.StateMachine.States
{
    public class GameplayState : IState
    {
        private readonly GameStateMachine _stateMachine;

        public GameplayState(GameStateMachine stateMachine)
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