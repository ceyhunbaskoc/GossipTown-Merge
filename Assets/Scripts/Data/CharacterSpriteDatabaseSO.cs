using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "CharacterSpriteDatabase", menuName = "GameData/SpriteDatabase/CharacterSpriteDatabase")]
    public class CharacterSpriteDatabaseSO : ScriptableObject
    {
        [field: SerializeField] public List<Sprite> CharacterSprites { get; private set; } = new List<Sprite>();
    }
}