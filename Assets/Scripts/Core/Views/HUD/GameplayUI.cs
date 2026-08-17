using System;
using Core.Economy;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Views
{
    public class GameplayUI : MonoBehaviour
    {
        [SerializeField] 
        private GameObject _gameplayUI;
        [SerializeField]
        private Button _backToMenuButton;
        
        public event Action OnExitToMenuClicked;

        private void Awake()
        {
            _backToMenuButton.onClick.AddListener(_onBackToMenuButtonClicked);
        }
        
        public void Show(bool show)
        {
            _gameplayUI.SetActive(show);
        }
        
        private void _onBackToMenuButtonClicked()
        {
            OnExitToMenuClicked?.Invoke();
        }
        
        private void OnDestroy()
        {
            _backToMenuButton.onClick.RemoveListener(_onBackToMenuButtonClicked);
        }
    }
}