using System;
using Core.Login;
using Core.StateMachine;
using Core.StateMachine.States;
using UI.Login;

namespace Core.Orchestration
{
    public class DailyLoginPopupOrchestrator : IDisposable
    {
        private readonly DailyLoginService _loginService;
        private readonly GameStateMachine _stateMachine;
        private readonly DailyLoginPanelPresenter _loginPresenter;

        private bool _isPopupPending;

        public DailyLoginPopupOrchestrator(
            DailyLoginService loginService, 
            GameStateMachine stateMachine, 
            DailyLoginPanelPresenter loginPresenter)
        {
            _loginService = loginService;
            _stateMachine = stateMachine;
            _loginPresenter = loginPresenter;

            _loginService.OnNewDayAvailable += HandleNewDayAvailable;
            _stateMachine.OnStateEntered += HandleStateChanged;
            
            CheckInitialState();
        }

        private void CheckInitialState()
        {
            if (_loginService.IsRewardAvailableToday)
            {
                _isPopupPending = true;
            }
        }

        private void HandleNewDayAvailable()
        {
            _isPopupPending = true;
            TryOpenPopup();
        }

        private void HandleStateChanged(Type stateType)
        {
            TryOpenPopup();
        }

        private void TryOpenPopup()
        {
            if (_isPopupPending && _stateMachine.CurrentState is MainMenuState)
            {
                _loginPresenter.OpenPanel();
                
                _isPopupPending = false;
            }
        }

        public void Dispose()
        {
            if (_loginService != null)
            {
                _loginService.OnNewDayAvailable -= HandleNewDayAvailable;
            }
            
            if (_stateMachine != null)
            {
                _stateMachine.OnStateEntered -= HandleStateChanged;
            }
        }
    }
}