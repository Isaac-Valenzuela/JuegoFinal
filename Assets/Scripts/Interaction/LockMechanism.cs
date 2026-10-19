using System;
using UnityEngine;

namespace LaResidencia
{
    /// <summary>Estado de cerradura reutilizable (puertas, cajones). El ruido del forzado es continuo y leve.</summary>
    public class LockMechanism : MonoBehaviour
    {
        [SerializeField] private bool isLocked = true;
        [SerializeField, Min(0.5f)] private float pickDuration = 4f;
        [SerializeField] private float pickNoiseRadius = 2.5f;
        [SerializeField, Min(0.1f)] private float pickNoiseInterval = 0.7f;
        [SerializeField] private float unlockNoiseRadius = 3f;

        public bool IsLocked => isLocked;
        public float PickDuration => pickDuration;
        public event Action Unlocked;

        private float noiseTimer;

        public void TickPick(float deltaTime)
        {
            noiseTimer -= deltaTime;
            if (noiseTimer > 0f) return;
            noiseTimer = pickNoiseInterval;
            StealthEvents.EmitNoise(transform.position, pickNoiseRadius, NoiseType.Lock, gameObject);
        }

        public void CancelPick() => noiseTimer = 0f;

        public void Unlock()
        {
            if (!isLocked) return;
            isLocked = false;
            noiseTimer = 0f;
            StealthEvents.EmitNoise(transform.position, unlockNoiseRadius, NoiseType.Lock, gameObject);
            Unlocked?.Invoke();
        }
    }
}
