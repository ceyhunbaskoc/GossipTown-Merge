using System.Collections.Generic;
using System.Linq;
using Data;
using UnityEngine;

namespace Order
{
    public interface IOrderCharacterSelector
    {
        Sprite GetUniqueCharacterSprite();
        void ReleaseCharacterSprite(Sprite sprite);
    }
    public class OrderRandomCharacterSelector : IOrderCharacterSelector
    {
        private readonly CharacterSpriteDatabaseSO _characterSpriteDatabase;
        private readonly HashSet<Sprite> _inUseSprites = new HashSet<Sprite>();

        public OrderRandomCharacterSelector(CharacterSpriteDatabaseSO characterSpriteDatabase)
        {
            _characterSpriteDatabase = characterSpriteDatabase;
        }
        
        public Sprite GetUniqueCharacterSprite()
        {
            List<Sprite> allSprites = _characterSpriteDatabase.CharacterSprites;
            
            if (allSprites == null || allSprites.Count == 0)
            {
                throw new System.Exception("[OrderCharacterSelector] Character sprites database is empty or null.");
            }

            List<Sprite> availableSprites = allSprites.Where(sprite => !_inUseSprites.Contains(sprite)).ToList();

            if (availableSprites.Count == 0)
            {
                Debug.LogWarning("[OrderCharacterSelector] Not enough unique sprites! Reusing active sprites. Consider adding more characters to the database.");
                _inUseSprites.Clear();
                availableSprites = allSprites.ToList();
            }

            int randomIndex = UnityEngine.Random.Range(0, availableSprites.Count);
            Sprite selectedSprite = availableSprites[randomIndex];
            
            _inUseSprites.Add(selectedSprite);
            
            return selectedSprite;
        }

        public void ReleaseCharacterSprite(Sprite sprite)
        {
            if (sprite != null)
            {
                _inUseSprites.Remove(sprite);
            }
        }
    }
}