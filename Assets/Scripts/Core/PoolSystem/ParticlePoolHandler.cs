using UnityEngine;

namespace Core.PoolSystem
{
    [RequireComponent(typeof(ParticleSystem), typeof(PooledObject))]
    public class ParticlePoolHandler : MonoBehaviour, IPoolable
    {
        private ParticleSystem _particleSystem;
        private PooledObject _pooledObject;

        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
            _pooledObject = GetComponent<PooledObject>();

            var main = _particleSystem.main;
            main.stopAction = ParticleSystemStopAction.Callback;
        }

        public void OnSpawned()
        {
            _particleSystem.Play(true);
        }

        public void OnDespawned()
        {
            _particleSystem.Clear(true);
        }

        private void OnParticleSystemStopped()
        {
            _pooledObject.ReturnToPool();
        }
    }
}