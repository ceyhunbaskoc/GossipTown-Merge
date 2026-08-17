using System;
using Core.StateMachine;
using Core.StateMachine.States;
using UnityEngine;

namespace Core.CameraSystem
{
    public class CameraStateOrchestrator : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private MapCameraController _mapCameraController;
        [SerializeField] private BoardCameraFitter _boardCameraFitter;
        [SerializeField] private Camera _mainCamera;

        private GameStateMachine _stateMachine;

        private Vector3 _savedMapPosition;
        private float _savedMapOrthoSize;

        public void Initialize(GameStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
            _stateMachine.OnStateEntered += HandleStateChanged;
            SaveMapState();
        }

        private void HandleStateChanged(Type stateType)
        {
            if (stateType == typeof(MainMenuState))
            {
                TransitionToMap();
            }
            else if (stateType == typeof(GameplayState))
            {
                TransitionToGameplay();
            }
        }

        private void TransitionToGameplay()
        {
            _mapCameraController.SaveCurrentTargetState(out _savedMapPosition, out _savedMapOrthoSize);
            _mapCameraController.enabled = false;

            if (_boardCameraFitter != null)
            {
                _boardCameraFitter.FitBoardToScreen();
            }
            else
            {
                Debug.LogError("[CameraStateOrchestrator] BoardCameraFitter referansı eksik!");
            }
        }

        private void TransitionToMap()
        {
            _mapCameraController.OverrideCameraState(_savedMapPosition, _savedMapOrthoSize);
            _mapCameraController.enabled = true;
        }

        private void SaveMapState()
        {
            if (_mainCamera != null)
            {
                _savedMapPosition = _mainCamera.transform.position;
                _savedMapOrthoSize = _mainCamera.orthographicSize;
            }
        }

        private void OnDestroy()
        {
            if (_stateMachine != null)
            {
                _stateMachine.OnStateEntered -= HandleStateChanged;
            }
        }
    }
}