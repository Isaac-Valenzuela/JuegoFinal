using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace LaResidencia
{
    /// <summary>
    /// Mochila abierta con Tab. NO toca Time.timeScale ni el cursor: el juego sigue corriendo.
    /// Cada Image de keySlots / utilitySlots es la casilla COMPLETA: siempre visible (vacía = tenue,
    /// llena = con el icono del objeto). Nunca se desactiva.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class InventoryUI : MonoBehaviour
    {
        [SerializeField] private BackpackInventory inventory;
        [SerializeField] private KeyCode toggleKey = KeyCode.Tab;
        [SerializeField] private bool holdToView = false;
        [SerializeField] private float fadeSpeed = 12f;

        [Header("Casillas (cada Image es la casilla completa)")]
        [FormerlySerializedAs("keySlotIcons")] [SerializeField] private Image[] keySlots;
        [FormerlySerializedAs("utilitySlotIcons")] [SerializeField] private Image[] utilitySlots;
        [SerializeField] private Color emptySlotColor = new Color(1f, 1f, 1f, 0.18f);
        [SerializeField] private Color filledSlotColor = Color.white;

        [Header("Contrato")]
        [SerializeField] private TMP_Text contractText;

        public bool IsOpen { get; private set; }

        private CanvasGroup group;
        private readonly StringBuilder sb = new StringBuilder(256);

        private void Awake()
        {
            group = GetComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;
        }

        private void OnEnable()
        {
            if (inventory == null) { Debug.LogError("[InventoryUI] Falta asignar BackpackInventory.", this); return; }
            if (keySlots == null || keySlots.Length == 0)
                Debug.LogWarning("[InventoryUI] keySlots vacío: no se dibujará ninguna casilla de objetos clave.", this);
            if (contractText == null)
                Debug.LogWarning("[InventoryUI] contractText sin asignar: no se verá la lista de contrato.", this);

            inventory.Changed += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            if (inventory != null) inventory.Changed -= Refresh;
        }

        private void Update()
        {
            if (holdToView) IsOpen = Input.GetKey(toggleKey);
            else if (Input.GetKeyDown(toggleKey)) IsOpen = !IsOpen;

            group.alpha = Mathf.MoveTowards(group.alpha, IsOpen ? 1f : 0f, fadeSpeed * Time.deltaTime);
        }

        private void Refresh()
        {
            FillSlots(keySlots, inventory.KeyItems);
            FillSlots(utilitySlots, inventory.UtilityItems);
            BuildContractText();
        }

        private void FillSlots(Image[] slots, IReadOnlyList<ItemData> items)
        {
            if (slots == null) return;
            for (int i = 0; i < slots.Length; i++)
            {
                Image slot = slots[i];
                if (slot == null) continue;

                ItemData item = i < items.Count ? items[i] : null;
                slot.enabled = true;                                   // jamás se oculta
                slot.sprite = item != null ? item.icon : null;
                slot.color = item != null ? filledSlotColor : emptySlotColor;
            }
        }

        private void BuildContractText()
        {
            if (contractText == null || inventory.Contract == null) return;

            sb.Clear();
            sb.AppendLine("CONTRATO");
            foreach (ItemData req in inventory.Contract.requiredItems)
            {
                if (req == null) continue;
                bool got = inventory.Has(req);
                if (got) sb.Append("[x] <s><color=#777777>").Append(req.displayName).AppendLine("</color></s>");
                else sb.Append("[  ] ").AppendLine(req.displayName);
                if (!got && !string.IsNullOrEmpty(req.contractHint))
                    sb.Append("      <size=70%><color=#AAAAAA>").Append(req.contractHint).AppendLine("</color></size>");
            }
            contractText.text = sb.ToString();
        }
    }
}
