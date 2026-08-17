using System.Collections.Generic;
using Core.Milestones;
using Data.Milestones;
using Data.Reward;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI.Milestones
{
    public class MilestoneProgressBarPresenter : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Slider _progressBar;
        [SerializeField] private TextMeshProUGUI _progressText;
        
        [Header("Node Generation")]
        [SerializeField] private RectTransform _nodesContainer;
        [SerializeField] private MilestoneTierNodeView _nodePrefab;

        private MilestoneService _milestoneService;
        private GlobalRewardIconDatabaseSO _iconDatabase;
        private readonly List<MilestoneTierNodeView> _activeNodes = new List<MilestoneTierNodeView>();

        public void Initialize(MilestoneService milestoneService, GlobalRewardIconDatabaseSO iconDatabase)
        {
            _milestoneService = milestoneService;
            _iconDatabase = iconDatabase;
            
            GenerateNodes();
            EvaluateAllNodeStates();
            
            _milestoneService.OnMedalCountChanged += HandleMedalCountChanged;
            _milestoneService.OnTierClaimed += HandleTierClaimed;

            HandleMedalCountChanged(_milestoneService.CurrentMedals, _milestoneService.MaxMedals);
        }

        private void GenerateNodes()
        {
            ClearNodes();

            float maxMedals = _milestoneService.MaxMedals;
            if (maxMedals <= 0) return;
            
            foreach (var tier in _milestoneService.GetTiers())
            {
                MilestoneTierNodeView nodeInstance = Instantiate(_nodePrefab, _nodesContainer);
                nodeInstance.gameObject.SetActive(true);
                
                float normalizedPosition = tier.RequiredMedals / maxMedals;
                
                RectTransform rectTransform = nodeInstance.GetComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(normalizedPosition, 0.5f);
                rectTransform.anchorMax = new Vector2(normalizedPosition, 0.5f);
                rectTransform.anchoredPosition = Vector2.zero;
                
                Sprite icon = tier.Reward != null ? tier.Reward.GetRewardIcon(_iconDatabase) : null;
                nodeInstance.Initialize(tier.TierId, tier.RequiredMedals, icon);
                
                nodeInstance.OnNodeClicked += HandleNodeClicked;
                
                _activeNodes.Add(nodeInstance);
            }
        }

        private void EvaluateAllNodeStates()
        {
            if (_milestoneService == null) return;
            
            int currentMedals = _milestoneService.CurrentMedals;

            int index = 0;
            foreach (var tier in _milestoneService.GetTiers())
            {
                var node = _activeNodes[index];

                if (_milestoneService.IsTierClaimed(tier.TierId))
                {
                    node.SetStateClaimed();
                }
                else if (currentMedals >= tier.RequiredMedals)
                {
                    node.SetStateReady();
                }
                else
                {
                    node.SetStateLocked();
                }
                index++;
            }
        }

        private void HandleMedalCountChanged(int current, int max)
        {
            if (max > 0)
            {
                _progressBar.value = (float)current / max;
            }
            
            if (_progressText != null)
            {
                _progressText.text = $"{current} / {max}";
            }

            EvaluateAllNodeStates(); 
        }

        private void HandleNodeClicked(string tierId)
        {
            _milestoneService.ClaimMilestone(tierId);
        }

        private void HandleTierClaimed(MilestoneTier tier)
        {
            EvaluateAllNodeStates();
        }

        private void ClearNodes()
        {
            foreach (var node in _activeNodes)
            {
                if (node != null)
                {
                    node.OnNodeClicked -= HandleNodeClicked;
                    Destroy(node.gameObject);
                }
            }
            _activeNodes.Clear();
        }

        private void OnDestroy()
        {
            if (_milestoneService != null)
            {
                _milestoneService.OnMedalCountChanged -= HandleMedalCountChanged;
                _milestoneService.OnTierClaimed -= HandleTierClaimed;
            }
            ClearNodes();
        }
    }
}