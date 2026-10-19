using System;
using System.Collections.Generic;
using UnityEngine;

namespace LaResidencia
{
    /// <summary>
    /// Mochila: 3 casillas de objetos clave + 2 de utilidad. Los objetos clave no se pueden descartar.
    /// Emite ContractCompleted al recoger el último objeto de la lista (gancho para alarma/persecución, Fase 3).
    /// </summary>
    public class BackpackInventory : MonoBehaviour
    {
        [SerializeField] private ContractData contract;
        [SerializeField, Min(1)] private int keyCapacity = 3;
        [SerializeField, Min(0)] private int utilityCapacity = 2;

        private readonly List<ItemData> keyItems = new List<ItemData>();
        private readonly List<ItemData> utilityItems = new List<ItemData>();

        public event Action Changed;
        public event Action<ItemData> ItemAdded;
        public event Action ContractCompleted;

        public ContractData Contract => contract;
        public int KeyCapacity => keyCapacity;
        public int UtilityCapacity => utilityCapacity;
        public IReadOnlyList<ItemData> KeyItems => keyItems;
        public IReadOnlyList<ItemData> UtilityItems => utilityItems;
        public bool IsContractComplete { get; private set; }
        public float RunNoiseMultiplier { get; private set; } = 1f;

        public bool Has(ItemData item) => keyItems.Contains(item) || utilityItems.Contains(item);

        public int CollectedContractCount
        {
            get
            {
                if (contract == null) return 0;
                int n = 0;
                foreach (ItemData req in contract.requiredItems)
                    if (req != null && keyItems.Contains(req)) n++;
                return n;
            }
        }

        public bool TryAdd(ItemData item)
        {
            if (item == null) return false;

            bool isKey = item.category == ItemCategory.Key;
            List<ItemData> list = isKey ? keyItems : utilityItems;
            int capacity = isKey ? keyCapacity : utilityCapacity;

            if (list.Count >= capacity) return false;
            if (isKey && list.Contains(item)) return false;

            list.Add(item);
            Recalculate();
            ItemAdded?.Invoke(item);
            Changed?.Invoke();
            CheckContract();
            return true;
        }

        /// <summary>Solo objetos de utilidad (al arrojarlos o usarlos). Los objetos clave no se pueden descartar.</summary>
        public bool TryRemoveUtility(ItemData item)
        {
            if (item == null || item.category != ItemCategory.Utility || !utilityItems.Remove(item)) return false;
            Recalculate();
            Changed?.Invoke();
            return true;
        }

        public void ClearAll()
        {
            keyItems.Clear();
            utilityItems.Clear();
            IsContractComplete = false;
            Recalculate();
            Changed?.Invoke();
        }

        private void Recalculate()
        {
            float bonus = 0f;
            foreach (ItemData i in keyItems) bonus += i.runNoiseBonus;
            foreach (ItemData i in utilityItems) bonus += i.runNoiseBonus;
            RunNoiseMultiplier = 1f + bonus;
        }

        private void CheckContract()
        {
            if (IsContractComplete || contract == null || contract.requiredItems.Length == 0) return;
            if (CollectedContractCount < contract.requiredItems.Length) return;

            IsContractComplete = true;
            ContractCompleted?.Invoke();
        }
    }
}
