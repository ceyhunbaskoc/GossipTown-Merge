using System.Collections.Generic;
using UnityEngine;

namespace Data.Audio
{
    [CreateAssetMenu(fileName = "SFXDatabase", menuName = "GameData/Audio/SFX Database")]
    public class SfxDatabaseSO : ScriptableObject
    {
        [SerializeField] private List<SfxConfig> _sfxList = new List<SfxConfig>();

        private Dictionary<SfxId, SfxConfig> _sfxDictionary;

        public void Initialize()
        {
            _sfxDictionary = new Dictionary<SfxId, SfxConfig>();
            foreach (var sfx in _sfxList)
            {
                if (!_sfxDictionary.ContainsKey(sfx.Id))
                {
                    _sfxDictionary.Add(sfx.Id, sfx);
                }
            }
        }

        public SfxConfig GetSfxConfig(SfxId id)
        {
            if (_sfxDictionary == null) Initialize();

            if (_sfxDictionary.TryGetValue(id, out SfxConfig config))
            {
                return config;
            }
            
            return null;
        }
    }
}