using UnityEngine;
using ArenaFall.Interfaces;

namespace ArenaFall.Data
{
    /// <summary>
    /// ScriptableObject for character configuration and cosmetics.
    /// </summary>
    [CreateAssetMenu(fileName = "NewCharacter", menuName = "Arena Fall/Characters/Character Data")]
    public class CharacterData : ScriptableObject
    {
        [Header("General")]
        public string characterId;
        public string characterName;
        [TextArea] public string biography;
        public CharacterGender gender;
        public CharacterFaction faction;
        public ItemRarity rarity;
        public Sprite portrait;
        public Sprite fullBodyPreview;

        [Header("Model")]
        public GameObject characterPrefab;
        public RuntimeAnimatorController animatorController;
        public Avatar avatar;

        [Header("Audio")]
        public AudioClip[] voiceLines;
        public AudioClip footstepSound;
        public AudioClip hurtSound;
        public AudioClip deathSound;

        [Header("Stats (Optional modifiers)")]
        public float movementSpeedModifier = 1.0f;
        public float healthModifier = 1.0f;
        public float shieldModifier = 1.0f;

        [Header("Materials")]
        public Material defaultMaterial;
        public Material[] skinMaterials;

        [Header("Cosmetics")]
        public bool isDefaultCharacter = false;
        public bool isPremium = false;
    }

    public enum CharacterGender
    {
        Male,
        Female,
        Other
    }

    public enum CharacterFaction
    {
        Vanguard,
        Phantom,
        Reaper,
        Sentinel,
        Nomad
    }
}
