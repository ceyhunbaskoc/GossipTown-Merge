using Core.PoolSystem;
using UnityEngine;

namespace Core.Views.VFX
{
    [RequireComponent(typeof(ParticleSystem))]
    public class MergeParticleView : MonoBehaviour
    {
        private ParticleSystem _particleSystem;
        private IObjectPool _pool;
        private PoolObjectType _poolType;

        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
        }

        public void Initialize(IObjectPool pool, PoolObjectType poolType)
        {
            _pool = pool;
            _poolType = poolType;
        }

        public void PlayEffect()
        {
            _particleSystem.Play();
        }

        private void OnParticleSystemStopped()
        {
            if (_pool != null)
            {
                _pool.Despawn(_poolType, this.gameObject);
            }
        }
    }
}