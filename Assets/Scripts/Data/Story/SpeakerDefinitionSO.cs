using UnityEngine;

namespace Data.Story
{
    [CreateAssetMenu(fileName = "NewSpeaker", menuName = "GameData/Story/Speaker")]
    public class SpeakerDefinitionSO : ScriptableObject
    {
        [field: SerializeField] public string SpeakerId { get; private set; }
        [field: SerializeField] public string DisplayName { get; private set; }
        [field: SerializeField] public Sprite Portrait { get; private set; }
    }
}