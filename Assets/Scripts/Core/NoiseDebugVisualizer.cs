using System.Collections.Generic;
using UnityEngine;

namespace LaResidencia
{
    /// <summary>Dibuja gizmos de cada ruido emitido. Colócalo en cualquier objeto de la escena para depurar.</summary>
    public class NoiseDebugVisualizer : MonoBehaviour
    {
        [SerializeField] private float lifetime = 1.25f;

        private struct Ping { public Vector3 Pos; public float Radius; public float Time; }
        private readonly List<Ping> pings = new List<Ping>();

        private void OnEnable() => StealthEvents.NoiseEmitted += OnNoise;
        private void OnDisable() => StealthEvents.NoiseEmitted -= OnNoise;

        private void OnNoise(NoiseEvent e)
            => pings.Add(new Ping { Pos = e.Position, Radius = e.Radius, Time = UnityEngine.Time.time });

        private void OnDrawGizmos()
        {
            for (int i = pings.Count - 1; i >= 0; i--)
            {
                float age = UnityEngine.Time.time - pings[i].Time;
                if (age > lifetime) { pings.RemoveAt(i); continue; }
                Gizmos.color = new Color(1f, 0.6f, 0.1f, 1f - age / lifetime);
                Gizmos.DrawWireSphere(pings[i].Pos, pings[i].Radius);
            }
        }
    }
}
