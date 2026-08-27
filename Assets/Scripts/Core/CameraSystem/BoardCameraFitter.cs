using UnityEngine;

namespace Core.CameraSystem
{
    [RequireComponent(typeof(Camera))]
    public class BoardCameraFitter : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private int _columns = 7;
        [SerializeField] private int _rows = 9;
        [SerializeField] private float _cellSize = 1f;
        [SerializeField] private Transform _boardContainer; 
        [SerializeField] private float _horizontalPadding = 0.5f;

        [Header("UI Safe Area (Viewport Percentages)")]
        [Range(0f, 0.5f)] [SerializeField] private float _topUIPercentage = 0.2f;    
        [Range(0f, 0.5f)] [SerializeField] private float _bottomUIPercentage = 0.15f; 

        private Camera _mainCamera;

        private void Awake()
        {
            _mainCamera = GetComponent<Camera>();
        }

        private void Start()
        {
            FitBoardToScreen();
        }

        public void FitBoardToScreen()
        {
            if (_mainCamera == null || !_mainCamera.orthographic) return;

            CalculateCameraTarget(out float targetOrthoSize, out Vector3 targetPosition);

            _mainCamera.orthographicSize = targetOrthoSize;
            transform.position = targetPosition;
        }

        private void CalculateCameraTarget(out float targetOrthoSize, out Vector3 targetPosition)
        {
            Vector3 boardCenter = CalculateBoardWorldCenter();

            float gridTotalWidth = (_columns * _cellSize) + _horizontalPadding;
            float gridTotalHeight = (_rows * _cellSize);

            float screenAspect = (float)Screen.width / Screen.height;

            float availableHeightPercentage = 1.0f - _topUIPercentage - _bottomUIPercentage;

            float orthoSizeForWidth = (gridTotalWidth / 2f) / screenAspect;
            
            float orthoSizeForHeight = (gridTotalHeight / 2f) / availableHeightPercentage;
            targetOrthoSize = Mathf.Max(orthoSizeForWidth, orthoSizeForHeight);

            float availableCenterViewportY = _bottomUIPercentage + (availableHeightPercentage / 2f);
            
            float offsetViewportY = availableCenterViewportY - 0.5f;
            
            float offsetWorldY = offsetViewportY * (targetOrthoSize * 2f);

            targetPosition = new Vector3(boardCenter.x, boardCenter.y - offsetWorldY, transform.position.z);
        }

        private Vector3 CalculateBoardWorldCenter()
        {
            if (_boardContainer == null) return Vector3.zero;
            return new Vector3(_boardContainer.position.x, _boardContainer.position.y, 0f);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            CalculateCameraTarget(out float targetOrthoSize, out Vector3 targetPosition);
            Vector3 boardCenter = CalculateBoardWorldCenter();

            float gridTotalWidth = (_columns * _cellSize) + _horizontalPadding;
            float gridTotalHeight = (_rows * _cellSize);
            
            float lineLeftX = boardCenter.x - (gridTotalWidth / 2f);
            float lineRightX = boardCenter.x + (gridTotalWidth / 2f);

            float cameraTopY = targetPosition.y + targetOrthoSize;
            float cameraBottomY = targetPosition.y - targetOrthoSize;
            float totalCameraHeight = targetOrthoSize * 2f;

            float safeAreaTopY = cameraTopY - (totalCameraHeight * _topUIPercentage);
            float safeAreaBottomY = cameraBottomY + (totalCameraHeight * _bottomUIPercentage);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(new Vector3(lineLeftX, safeAreaTopY, 0f), new Vector3(lineRightX, safeAreaTopY, 0f));

            Gizmos.DrawLine(new Vector3(lineLeftX, safeAreaBottomY, 0f), new Vector3(lineRightX, safeAreaBottomY, 0f));

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(new Vector3(lineLeftX, targetPosition.y, 0f), new Vector3(lineRightX, targetPosition.y, 0f));
            
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f); 
            Gizmos.DrawWireCube(boardCenter, new Vector3(gridTotalWidth, gridTotalHeight, 0f));
        }
#endif
    }
}