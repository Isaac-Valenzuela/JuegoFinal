using System;
using UnityEngine;

namespace LaResidencia
{
    public enum NoiseType { Footstep, Door, Lock, Switch, Hiding, Item, Throw, Alarm }

    /// <summary>Un ruido en el mundo: la IA decide si lo oye comparando distancia y radio.</summary>
    public readonly struct NoiseEvent
    {
        public readonly Vector3 Position;
        public readonly float Radius;
        public readonly NoiseType Type;
        public readonly GameObject Source;
        public readonly float Time;

        public NoiseEvent(Vector3 position, float radius, NoiseType type, GameObject source)
        {
            Position = position;
            Radius = radius;
            Type = type;
            Source = source;
            Time = UnityEngine.Time.time;
        }

        public bool IsAudibleFrom(Vector3 listenerPosition)
            => (Position - listenerPosition).sqrMagnitude <= Radius * Radius;
    }

    public enum AlterationType { DoorOpen, LightChanged, ItemMissing }

    /// <summary>Cambio en el entorno que Wilson puede notar (puerta abierta, luz alterada, objeto faltante).</summary>
    public readonly struct WorldAlteration
    {
        public readonly Vector3 Position;
        public readonly AlterationType Type;
        public readonly bool Active;      // true = alterado respecto al estado original
        public readonly GameObject Source;

        public WorldAlteration(Vector3 position, AlterationType type, bool active, GameObject source)
        {
            Position = position;
            Type = type;
            Active = active;
            Source = source;
        }
    }

    /// <summary>Bus estático de eventos de sigilo. La IA de Wilson (Fase 2) solo se suscribe aquí.</summary>
    public static class StealthEvents
    {
        public static event Action<NoiseEvent> NoiseEmitted;
        public static event Action<WorldAlteration> WorldAltered;

        public static void EmitNoise(Vector3 position, float radius, NoiseType type, GameObject source = null)
        {
            if (radius <= 0f) return;
            NoiseEmitted?.Invoke(new NoiseEvent(position, radius, type, source));
        }

        public static void ReportAlteration(WorldAlteration alteration) => WorldAltered?.Invoke(alteration);

        // Evita suscripciones fantasma si "Enter Play Mode Options" desactiva el domain reload.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            NoiseEmitted = null;
            WorldAltered = null;
        }
    }

    /// <summary>Compatibilidad Unity 2022 / Unity 6 (velocity -> linearVelocity).</summary>
    public static class PhysicsCompat
    {
        public static void Stop(Rigidbody rb)
        {
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = Vector3.zero;
#else
            rb.velocity = Vector3.zero;
#endif
            rb.angularVelocity = Vector3.zero;
        }
    }
}
