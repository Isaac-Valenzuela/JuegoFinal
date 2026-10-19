using UnityEngine;

namespace LaResidencia
{
    /// <summary>Convierte distancia recorrida en pasos; cada paso emite un NoiseEvent según superficie y estado.</summary>
    [RequireComponent(typeof(PlayerMovementTracker), typeof(SurfaceDetector))]
    public class FootstepNoiseEmitter : MonoBehaviour
    {
        [Header("Longitud de paso (m)")]
        [SerializeField] private float crouchStride = 1.1f;
        [SerializeField] private float walkStride = 1.8f;
        [SerializeField] private float runStride = 2.3f;

        [Header("Audio (opcional)")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField, Range(0f, 1f)] private float crouchVolume = 0.25f;
        [SerializeField, Range(0f, 1f)] private float walkVolume = 0.55f;
        [SerializeField, Range(0f, 1f)] private float runVolume = 1f;

        [Header("Opcional")]
        [SerializeField] private BackpackInventory backpack;

        private PlayerMovementTracker tracker;
        private SurfaceDetector detector;
        private Vector3 lastPos;
        private float travelled;
        private int lastClip = -1;

        private void Awake()
        {
            tracker = GetComponent<PlayerMovementTracker>();
            detector = GetComponent<SurfaceDetector>();
            if (backpack == null) backpack = GetComponent<BackpackInventory>();
            lastPos = transform.position;
        }

        private void Update()
        {
            Vector3 delta = transform.position - lastPos;
            delta.y = 0f;
            lastPos = transform.position;

            if (!tracker.IsMoving) return;

            travelled += delta.magnitude;
            if (travelled < StrideFor(tracker.State)) return;

            travelled = 0f;
            EmitStep();
        }

        private float StrideFor(MovementState s)
        {
            switch (s)
            {
                case MovementState.Crouching: return crouchStride;
                case MovementState.Running: return runStride;
                default: return walkStride;
            }
        }

        private float VolumeFor(MovementState s)
        {
            switch (s)
            {
                case MovementState.Crouching: return crouchVolume;
                case MovementState.Running: return runVolume;
                default: return walkVolume;
            }
        }

        private void EmitStep()
        {
            if (!detector.TrySample(out SurfaceNoiseProfile profile, out float mult, out Vector3 point))
                return; // en el aire: no hay paso

            MovementState state = tracker.State;
            float radius = profile.GetRadius(state) * mult;
            if (state == MovementState.Running && backpack != null)
                radius *= backpack.RunNoiseMultiplier;

            StealthEvents.EmitNoise(point, radius, NoiseType.Footstep, gameObject);
            PlayClip(profile, VolumeFor(state));
        }

        private void PlayClip(SurfaceNoiseProfile profile, float volume)
        {
            if (audioSource == null || profile.footstepClips == null || profile.footstepClips.Length == 0) return;

            int i = Random.Range(0, profile.footstepClips.Length);
            if (profile.footstepClips.Length > 1 && i == lastClip) i = (i + 1) % profile.footstepClips.Length;
            lastClip = i;
            audioSource.PlayOneShot(profile.footstepClips[i], volume);
        }
    }
}
