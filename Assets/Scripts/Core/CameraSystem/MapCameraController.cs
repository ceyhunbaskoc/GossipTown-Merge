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
        [SerializeField] private float minZoom = 3f;
        [SerializeField] private float maxZoom = 10f;
        [SerializeField] private float zoomLerpSpeed = 10f;
        [SerializeField] private float zoomMultiplier = 2f;

        [Header("Pan Settings")]
        [SerializeField] private float panLerpSpeed = 15f;
        [SerializeField] private Collider2D mapBounds;

        private Camera _mainCamera;
        
        private Vector3 _targetPosition;
        private float _targetZoom;
        
        private Vector3 _lastPointerPosition;
        private bool _isDragging;
        private IInputLockService _inputLockService;
        
        public void Initialize(IInputLockService inputLockService)
        {
            _inputLockService = inputLockService;
        }

        private void Awake()
        {
            _mainCamera = GetComponent<Camera>();
            _targetPosition = transform.position;
            _targetZoom = _mainCamera.orthographicSize;
        }

        private void LateUpdate()
        {
            HandleInput();
            ApplyTransformations();
        }
        
        private void HandleInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                bool isLocked = _inputLockService != null && _inputLockService.IsLocked;
                Debug.Log($"[MapCameraController] Fare/Dokunma algılandı. InputLockService IsLocked: {isLocked}");
            }
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
                _targetZoom -= scrollData * zoomMultiplier;
            }

            if (Input.touchCount == 2)
            {
                Touch touchZero = Input.GetTouch(0);
                Touch touchOne = Input.GetTouch(1);

                Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

                float difference = currentMagnitude - prevMagnitude;
                _targetZoom -= difference * 0.01f;
            }

            float absoluteMaxZoom = CalculateAbsoluteMaxZoom();
            float clampedMaxZoom = Mathf.Min(maxZoom, absoluteMaxZoom);
            
            _targetZoom = Mathf.Clamp(_targetZoom, minZoom, clampedMaxZoom);
        }

        private void HandlePanInput()
        {
            if (Input.GetMouseButtonDown(0) && Input.touchCount < 2)
            {
                bool isOverUI = IsPointerOverUI();
                Debug.Log($"[MapCameraController] Harita kaydırma denemesi. IsPointerOverUI: {isOverUI}");

                if (isOverUI) return;

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

        private void ApplyTransformations()
        {
            _targetPosition = ClampPosition(_targetPosition, _targetZoom);

            _mainCamera.orthographicSize = Mathf.Lerp(_mainCamera.orthographicSize, _targetZoom, Time.deltaTime * zoomLerpSpeed);
            Vector3 newPosition = Vector3.Lerp(transform.position, _targetPosition, Time.deltaTime * panLerpSpeed);

            transform.position = ClampPosition(newPosition, _mainCamera.orthographicSize);
        }

        private Vector3 ClampPosition(Vector3 targetPos, float orthoSize)
        {
            if (mapBounds == null) return targetPos;

            Bounds bounds = mapBounds.bounds;
            
            float cameraHalfHeight = orthoSize;
            float cameraHalfWidth = cameraHalfHeight * _mainCamera.aspect;

            float minX = bounds.min.x + cameraHalfWidth;
            float maxX = bounds.max.x - cameraHalfWidth;
            float minY = bounds.min.y + cameraHalfHeight;
            float maxY = bounds.max.y - cameraHalfHeight;

            if (minX > maxX) minX = maxX = bounds.center.x;
            if (minY > maxY) minY = maxY = bounds.center.y;

            targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
            targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);
            
            return targetPos;
        }

        private float CalculateAbsoluteMaxZoom()
        {
            if (mapBounds == null) return maxZoom;

            Bounds bounds = mapBounds.bounds;
            
            float maxOrthoY = bounds.size.y / 2f;
            
            float maxOrthoX = (bounds.size.x / 2f) / _mainCamera.aspect;

            return Mathf.Min(maxOrthoX, maxOrthoY);
        }
        
        private bool IsPointerOverUI()
        {
            if (EventSystem.current == null) return false;

            PointerEventData eventData = new PointerEventData(EventSystem.current);
            if (Input.touchCount > 0)
            {
                eventData.position = Input.GetTouch(0).position;
            }
            else
            {
                eventData.position = Input.mousePosition;
            }
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            int uiLayer = LayerMask.NameToLayer("UI");

            foreach (RaycastResult result in results)
            {
                if (result.gameObject.layer == uiLayer)
                {
                    return true;
                }
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
            if (_mainCamera == null)
            {
                _mainCamera = GetComponent<Camera>();
            }

            _targetPosition = newPos;
            _targetZoom = newZoom;
            transform.position = newPos;
            if (_mainCamera != null)
            {
                _mainCamera.orthographicSize = newZoom;
            }
        }
    }
}