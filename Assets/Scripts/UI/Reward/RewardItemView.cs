using UnityEngine;
using UnityEngine.UI;
using IPoolable = Core.PoolSystem.IPoolable;

namespace UI.Rewards
{
    public class RewardItemView : MonoBehaviour, IPoolable
    {
        [SerializeField] private Image _iconImage;

        public void Initialize(Sprite icon)
        {
            _iconImage.sprite = icon;
        }

        public void OnSpawned()
        {
            _iconImage.color = Color.white;
            transform.localScale = Vector3.one;
        }

        public void OnDespawned()
        {
            _iconImage.sprite = null;
        }
    }
}