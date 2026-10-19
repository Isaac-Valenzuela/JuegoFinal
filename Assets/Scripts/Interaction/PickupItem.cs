using UnityEngine;

namespace LaResidencia
{
    /// <summary>Objeto recogible (clave del contrato o de utilidad). Para reintentar: recarga la escena.</summary>
    public class PickupItem : Interactable
    {
        [SerializeField] private ItemData item;
        [SerializeField] private float pickupNoise = 0.8f;

        private string prompt;

        private void Awake() => prompt = item != null ? $"Recoger {item.displayName}" : "Recoger";

        public override string GetPrompt(PlayerInteractor who) => prompt;

        public override void Interact(PlayerInteractor who, InteractContext ctx)
        {
            BackpackInventory inv = who.Inventory;
            if (item == null || inv == null || !inv.TryAdd(item))
            {
                who.Notify(item != null && item.category == ItemCategory.Key
                    ? "No cabe en la mochila."
                    : "Casillas de utilidad llenas.");
                return;
            }

            StealthEvents.EmitNoise(transform.position, pickupNoise, NoiseType.Item, who.gameObject);
            if (item.category == ItemCategory.Key)
                StealthEvents.ReportAlteration(
                    new WorldAlteration(transform.position, AlterationType.ItemMissing, true, gameObject));

            gameObject.SetActive(false);
        }
    }
}
