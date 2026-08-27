using System;
using System.Collections.Generic;
using Core.Services;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Core.CameraSystem
{
    [RequireComponent(typeof(Camera))]
    public class MapCameraController : MonoBehaviour
    {
        [Header("Zoom Settings")]
        [SerializeField] private float _minZoom = 3f;
        [SerializeField] private float _maxZoom = 10f;
        [SerializeField] private float _zoomLerpSpeed = 10f;
        [SerializeField] private float _zoomMultiplier = 2f;

        [Header("Pan Settings")]
        [SerializeField] private float _panLerpSpeed = 15f;
        
        [Header("Virtual Map Bounds")]
        [SerializeField] private Vector2 _mapSize = new Vector2(50f, 50f); 

        private Camera _mainCamera;
        
        private Vector3 _targetPosition;
        private float _targetZoom;
        
        private Vector3 _lastPointerPosition;
        private bool _isDragging;
        private IInputLockService _inputLockService;
        
        private Vector3 _lastReportedPosition;
        private const float REPORT_THRESHOLD = 2f;
        
        public event Action<Vector3, float, float> OnCameraMoved;

        private float _lastReportedZoom;
        private const float ZOOM_REPORT_THRESHOLD = 0.5f;

        public void Initialize(IInputLockService inputLockService)
        {
            _inputLockService = inputLockService;
        }

        private void Awake()
        {
            _mainCamera = GetComponent<Camera>();
            _targetPosition = transform.position;
            _targetZoom = _mainCamera.orthographicSize;
            _lastReportedZoom = _targetZoom;
            _lastReportedPosition = _targetPosition;
        }

        private void LateUpdate()
        {
            HandleInput();
            ApplyTransformations();
            ReportPositionIfChanged();
        }
        
        private void HandleInput()
        {
            if (_inputLockService != null && _inputLockService.IsLocked)
            {
                _isDragging = false;
                return; 
            }

            HandleZoomInput();
            HandlePanInput();
        }

        private void HandleZoomInput()
        {
            float scrollData = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scrollData) > 0.01f)
            {
                _targetZoom -= scrollData * _zoomMultiplier;
            }
            if (Input.touchCount == 2)
            {
                Touch touchZero = Input.GetTouch(0);
                Touch touchOne = Input.GetTouch(1);

                Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                float difference = (touchZero.position - touchOne.position).magnitude - (touchZeroPrevPos - touchOnePrevPos).magnitude;
                _targetZoom -= difference * 0.01f;
            }
            float absoluteMaxZoom = CalculateAbsoluteMaxZoom();
            float clampedMaxZoom = Mathf.Min(_maxZoom, absoluteMaxZoom);
            float safeMinZoom = Mathf.Min(_minZoom, clampedMaxZoom);
            _targetZoom = Mathf.Clamp(_targetZoom, safeMinZoom, clampedMaxZoom);
        }

        private void HandlePanInput()
        {
            if (Input.GetMouseButtonDown(0) && Input.touchCount < 2)
            {
                if (IsPointerOverUI()) return;

                _lastPointerPosition = Input.mousePosition;
                _isDragging = true;
            }
    
            if (Input.GetMouseButtonUp(0))
            {
                _isDragging = false;
            }

            if (Input.GetMouseButton(0) && _isDragging && Input.touchCount < 2)
            {
                Vector3 pointerDelta = _lastPointerPosition - Input.mousePosition;
        
                float orthoHeight = _mainCamera.orthographicSize * 2f;
                float orthoWidth = orthoHeight * _mainCamera.aspect;

                Vector3 worldDelta = new Vector3(
                    (pointerDelta.x / Screen.width) * orthoWidth,
                    (pointerDelta.y / Screen.height) * orthoHeight,
                    0f
                );

                _targetPosition += worldDelta;
                _lastPointerPosition = Input.mousePosition;
            }
        }
        /*
        public void ForceReportPosition()
        {
            if (_mainCamera == null) return;
            
            _lastReportedPosition = transform.position;
            _lastReportedZoom = _mainCamera.orthographicSize;
            
            OnCameraMoved?.Invoke(transform.position, _mainCamera.orthographicSize, _mainCamera.aspect);
        }
*/
        private void ApplyTransformations()
        {
            _targetPosition = ClampPosition(_targetPosition, _targetZoom);

            _mainCamera.orthographicSize = Mathf.Lerp(_mainCamera.orthographicSize, _targetZoom, Time.deltaTime * _zoomLerpSpeed);
            transform.position = Vector3.Lerp(transform.position, _targetPosition, Time.deltaTime * _panLerpSpeed);
        }
        private Vector3 ClampPosition(Vector3 targetPos, float orthoSize)
        {
            float cameraHalfHeight = orthoSize;
            float cameraHalfWidth = cameraHalfHeight * _mainCamera.aspect;
            float minX = -(_mapSize.x / 2f) + cameraHalfWidth;
            float maxX = (_mapSize.x / 2f) - cameraHalfWidth;
            float minY = -(_mapSize.y / 2f) + cameraHalfHeight;
            float maxY = (_mapSize.y / 2f) - cameraHalfHeight;
            if (minX > maxX) minX = maxX = 0f;
            if (minY > maxY) minY = maxY = 0f;

            targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
            targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);
            
            return targetPos;
        }

        private float CalculateAbsoluteMaxZoom()
        {
            float maxOrthoY = _mapSize.y / 2f;
            float maxOrthoX = (_mapSize.x / 2f) / _mainCamera.aspect;

            return Mathf.Min(maxOrthoX, maxOrthoY);
        }
        private void ReportPositionIfChanged()
        {
            bool positionChanged = Vector3.Distance(transform.position, _lastReportedPosition) > REPORT_THRESHOLD;
            bool zoomChanged = Mathf.Abs(_mainCamera.orthographicSize - _lastReportedZoom) > ZOOM_REPORT_THRESHOLD;

            if (positionChanged || zoomChanged)
            {
                _lastReportedPosition = transform.position;
                _lastReportedZoom = _mainCamera.orthographicSize;
                OnCameraMoved?.Invoke(transform.position, _mainCamera.orthographicSize, _mainCamera.aspect);
            }
        }
        
        private bool IsPointerOverUI()
        {
            if (EventSystem.current == null) return false;

            PointerEventData eventData = new PointerEventData(EventSystem.current)
            {
                position = Input.touchCount > 0 ? Input.GetTouch(0).position : Input.mousePosition
            };
            
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            int uiLayer = LayerMask.NameToLayer("UI");
            foreach (RaycastResult result in results)
            {
                if (result.gameObject.layer == uiLayer) return true;
            }
            return false;
        }
        
        private void OnEnable()
        {
            if (_mainCamera == null) return;
            _targetPosition = transform.position;
            _targetZoom = _mainCamera.orthographicSize;
            _isDragging = false;
        }
        
        public void SaveCurrentTargetState(out Vector3 savedPos, out float savedZoom)
        {
            savedPos = _targetPosition; 
            savedZoom = _targetZoom;
        }

        public void OverrideCameraState(Vector3 newPos, float newZoom)
        {
            if (_mainCamera == null) _mainCamera = GetComponent<Camera>();

            _targetPosition = newPos;
            _targetZoom = newZoom;
            transform.position = newPos;
            _mainCamera.orthographicSize = newZoom;
            
            ReportPositionIfChanged();
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(_mapSize.x, _mapSize.y, 0f));
        }
#endif
    }
}