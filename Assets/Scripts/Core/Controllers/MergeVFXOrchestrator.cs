using Core.PoolSystem;
using Core.Views.VFX;
using UnityEngine;

namespace Core.Controllers
{
    public class MergeVFXOrchestrator
    {
        private IObjectPool _objectPool;

        public MergeVFXOrchestrator(IObjectPool objectPool)
        {
            _objectPool = objectPool;
        }

        public void PlayMergeEffect(Vector3 worldPosition)
        {
            GameObject particleObj = _objectPool.Spawn(PoolObjectType.MergeParticle, worldPosition, Quaternion.identity);

            if (particleObj.TryGetComponent(out MergeParticleView mergeParticle))
            {
                mergeParticle.Initialize(_objectPool, PoolObjectType.MergeParticle);
                mergeParticle.PlayEffect();
            }
            else
            {
                _objectPool.Despawn(PoolObjectType.MergeParticle, particleObj);
            }
        }
    }
}