using System;
using Core.Economy;
using Core.Map;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Views
{
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] 
        private GameObject _mainMenuUI;
        [SerializeField] 
        private ChunkManager _mainMenuChunkManager;
        [SerializeField] 
        private GameObject _buildingSlotsContainer;
        [SerializeField]
        private Button _playButton;
        
        private IReadOnlyEconomyModel _economyModel;
        public event Action OnPlayButtonClicked;
        
        private void Awake()
        {
            _playButton.onClick.AddListener(_onPlayButtonClicked);
        }
        
        private void _onPlayButtonClicked()
        {
            OnPlayButtonClicked?.Invoke();
        }
        
        public void Show(bool show)
        {
            _mainMenuUI.SetActive(show);
            _mainMenuChunkManager.SetMapActive(show);
            _buildingSlotsContainer.SetActive(show);
        }

        private void OnDestroy()
        {
            _playButton.onClick.RemoveListener(_onPlayButtonClicked);
        }
    }
}