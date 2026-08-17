using System;
using System.Collections.Generic;

namespace Core.StateMachine
{
    public class GameStateMachine
    {
        private readonly Dictionary<Type, IState> _states = new Dictionary<Type, IState>();
        private IState _currentState;

        public event Action<Type> OnStateEntered;

        public IState CurrentState => _currentState;

        public void Register<TState>(TState state) where TState : IState
        {
            _states[typeof(TState)] = state;
        }

        public void Enter<TState>() where TState : IState
        {
            if (!_states.TryGetValue(typeof(TState), out var next))
                throw new InvalidOperationException($"State not registered: {typeof(TState).Name}");

            _currentState?.Exit();
            _currentState = next;
            OnStateEntered?.Invoke(typeof(TState));
            _currentState.Enter();
        }
    }
}