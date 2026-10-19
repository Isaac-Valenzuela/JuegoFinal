using UnityEngine;

namespace LaResidencia
{
    public enum ItemCategory { Key, Utility }

    [CreateAssetMenu(menuName = "La Residencia/Item Data", fileName = "Item_")]
    public class ItemData : ScriptableObject
    {
        public string id;
        public string displayName;
        public Sprite icon;
        public ItemCategory category = ItemCategory.Key;

        [TextArea] public string contractHint;

        [Header("Efectos")]
        [Tooltip("Se suma al multiplicador de ruido al correr (caja de música: 0.15).")]
        public float runNoiseBonus = 0f;

        [Header("Utilidad")]
        public bool throwable;
        public float throwNoiseRadius = 8f;
    }
}
