using Core.GridSystem;
using UnityEngine;
using UnityEngine.UI;
using Core.Reward;
using Core.PoolSystem;
using Core.StateMachine;
using Core.StateMachine.States;
using Data;
using DG.Tweening;

namespace UI.Rewards
{
    public class RewardPresentationManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private CanvasGroup _blockerPanel;
        [SerializeField] private RectTransform _spawnOrigin;
        
        [Header("Targets (State Dependant)")]
        [SerializeField] private RectTransform _mainMenuTarget;
        [SerializeField] private RectTransform _gameplayTarget;

        private PendingRewardModel _rewardModel;
        private ObjectPoolManager _poolManager;
        private ItemDatabaseSO _itemDatabase;
        
        private GameStateMachine _stateMachine;

        public void Initialize(PendingRewardModel rewardModel, ObjectPoolManager poolManager, ItemDatabaseSO itemDatabase, GameStateMachine stateMachine)
        {
            _rewardModel = rewardModel;
            _poolManager = poolManager;
            _itemDatabase = itemDatabase;
            _stateMachine = stateMachine;

            _rewardModel.OnRewardAdded += HandleRewardAdded;
            
            _blockerPanel.alpha = 0;
            _blockerPanel.blocksRaycasts = false;
        }

        private void HandleRewardAdded(ItemIdentifier reward)
        {
            BaseItemDefinitionSO itemDef = _itemDatabase.GetItemDef(reward.Id);
            Sprite itemSprite = itemDef.GetIcon(reward.Level);

            GameObject rewardObj = _poolManager.Spawn(PoolObjectType.RewardItem, _spawnOrigin.position, Quaternion.identity);
            rewardObj.transform.SetParent(_spawnOrigin, false);
            
            RewardItemView rewardItemView = rewardObj.GetComponent<RewardItemView>();
            rewardItemView.Initialize(itemSprite);

            RectTransform activeTarget = _getTargetByCurrentState();

            _playPresentationSequence(rewardItemView, activeTarget);
        }
        
        private RectTransform _getTargetByCurrentState()
        {
            if (_stateMachine.CurrentState is GameplayState)
            {
                return _gameplayTarget;
            }
            return _mainMenuTarget;
        }

        private void _playPresentationSequence(RewardItemView targetView, RectTransform targetDestination)
        {
            Sequence seq = DOTween.Sequence();
            RectTransform viewRect = targetView.GetComponent<RectTransform>();

            _blockerPanel.blocksRaycasts = true;
            viewRect.localScale = Vector3.zero;
            viewRect.position = _spawnOrigin.position;

            seq.Join(_blockerPanel.DOFade(0.7f, 0.3f));
            seq.Join(viewRect.DOScale(1.5f, 0.5f).SetEase(Ease.OutBack));

            seq.AppendInterval(1.5f);

            seq.Append(_blockerPanel.DOFade(0f, 0.3f));
            seq.Join(viewRect.DOMove(targetDestination.position, 0.6f).SetEase(Ease.InBack));
            seq.Join(viewRect.DOScale(0.5f, 0.6f));

            seq.OnComplete(() =>
            {
                _blockerPanel.blocksRaycasts = false;
                _poolManager.Despawn(PoolObjectType.RewardItem, targetView.gameObject);
                
                targetDestination.DOPunchScale(Vector3.one * 0.2f, 0.3f);
            });
        }

        private void OnDestroy()
        {
            if (_rewardModel != null)
                _rewardModel.OnRewardAdded -= HandleRewardAdded;
        }
    }
}