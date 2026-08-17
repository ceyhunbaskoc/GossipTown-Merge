using Data;
using UnityEngine;

namespace Order
{
    public class OrderRandomCharacterSelector
    {
        private readonly CharacterSpriteDatabaseSO _characterSpriteDatabaseSO;
        public OrderRandomCharacterSelector(CharacterSpriteDatabaseSO characterSpriteDatabaseSO)
        {
            _characterSpriteDatabaseSO = characterSpriteDatabaseSO;
        }
        
        public Sprite GetRandomCharacterSprite()
        {
            var characterSprites = _characterSpriteDatabaseSO.CharacterSprites;
            if (characterSprites == null || characterSprites.Count == 0)
            {
                throw new System.Exception("Character sprites database is empty or null.");
            }

            int randomIndex = UnityEngine.Random.Range(0, characterSprites.Count);
            return characterSprites[randomIndex];
        }
    }
}