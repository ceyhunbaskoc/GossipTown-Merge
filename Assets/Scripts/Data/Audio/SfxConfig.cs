using System;
using UnityEngine;

namespace Data.Audio
{
    public enum SfxId
    {
        None = 0,
        UI_ButtonClick,
        Gameplay_ItemSpawn,
        Gameplay_ItemMerge,
        Gameplay_OrderComplete,
        Reward_CoinCollect,
        Reward_GemCollect,
        Reward_EnergyCollect,
        Building_Upgraded_Confetti,
        Building_Upgraded_Generic,
        Backpack_Drop,
        ChestOpen,
    }
    
    [Serializable]
    public class SfxConfig
    {
        [field: SerializeField] public SfxId Id { get; private set; }
        
        [SerializeField] private AudioClip _clip;
        
        [Range(0f, 1f)]
        [SerializeField] private float _volume = 1f;

        public void Play(AudioSource source)
        {
            if (_clip == null) 
            {
                return;
            }
            source.PlayOneShot(_clip, _volume);
        }
    }
}