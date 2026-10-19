using UnityEngine;

namespace LaResidencia
{
    [CreateAssetMenu(menuName = "La Residencia/Contract", fileName = "Contract_")]
    public class ContractData : ScriptableObject
    {
        public ItemData[] requiredItems = new ItemData[3];
    }
}
