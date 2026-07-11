using UnityEngine;
using ArenaFall.Interfaces;

namespace ArenaFall.Data
{
    /// <summary>
    /// ScriptableObject for weapon attachment configuration.
    /// </summary>
    [CreateAssetMenu(fileName = "NewAttachment", menuName = "Arena Fall/Weapons/Attachment Data")]
    public class AttachmentData : ScriptableObject
    {
        [Header("General")]
        public string attachmentId;
        public string attachmentName;
        [TextArea] public string description;
        public AttachmentSlotType slotType;
        public ItemRarity rarity;
        public Sprite icon;
        public GameObject attachmentPrefab;

        [Header("Stat Modifiers")]
        public float damageModifier = 1.0f;
        public float fireRateModifier = 1.0f;
        public float accuracyModifier = 1.0f;
        public float recoilModifier = 1.0f;
        public float rangeModifier = 1.0f;
        public float reloadSpeedModifier = 1.0f;
        public float aimSpeedModifier = 1.0f;
        public float movementSpeedModifier = 1.0f;

        [Header("Visuals")]
        public Vector3 positionOffset;
        public Vector3 rotationOffset;
        public Material attachmentMaterial;

        [Header("Audio")]
        public AudioClip attachSound;
        public AudioClip detachSound;

        [Header("Sight Specific")]
        public float sightZoomMultiplier = 1.0f;
        public GameObject sightReticlePrefab;
        public Vector2 sightReticleSize = Vector2.one;

        [Header("Muzzle Specific")]
        public bool isSuppressor;
        public bool isCompensator;
        public bool isFlashHider;

        [Header("Magazine Specific")]
        public int ammoCapacityModifier = 0;
        public float reloadSpeedModifierSpecific = 1.0f;
    }
}
