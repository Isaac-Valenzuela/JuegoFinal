using System.Collections;
using UnityEngine;

namespace LaResidencia
{
    /// <summary>
    /// Puerta con bisagra. Requisitos del modelo: el pivote del objeto "hinge" está en la bisagra, el panel
    /// se extiende hacia +X local y +Z es la normal de la puerta. Si abre al revés, invierte openAngle.
    /// Interactuar agachado = apertura lenta (poco ruido). Si tiene LockMechanism cerrado: mantener para forzar.
    /// </summary>
    public class Door : Interactable
    {
        [SerializeField] private Transform hinge;
        [SerializeField] private float openAngle = 95f;
        [SerializeField] private bool swingAwayFromPlayer = true;
        [SerializeField] private LockMechanism lockMechanism;

        [Header("Duración (s)")]
        [SerializeField] private float fastDuration = 0.55f;
        [SerializeField] private float slowDuration = 2.4f;

        [Header("Ruido (radio en m)")]
        [SerializeField] private float openNoiseFast = 6f;
        [SerializeField] private float openNoiseSlow = 1.2f;
        [SerializeField] private float closeNoiseFast = 8f;
        [SerializeField] private float closeNoiseSlow = 1.2f;

        [Header("Audio (opcional)")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip openClip;
        [SerializeField] private AudioClip closeClip;

        public bool IsOpen { get; private set; }

        private Quaternion closedRotation;
        private float currentAngle;
        private Coroutine swing;

        private bool LockedNow => lockMechanism != null && lockMechanism.IsLocked;

        private void Awake()
        {
            if (hinge == null) hinge = transform;
            closedRotation = hinge.localRotation;
        }

        public override string GetPrompt(PlayerInteractor who)
        {
            if (LockedNow) return who.HasLockpick ? "Forzar cerradura (mantener)" : "Cerrada con llave";
            return IsOpen ? "Cerrar puerta" : "Abrir puerta";
        }

        public override float GetHoldDuration(PlayerInteractor who)
            => LockedNow && who.HasLockpick ? lockMechanism.PickDuration : 0f;

        public override void OnHoldTick(PlayerInteractor who, float dt) => lockMechanism.TickPick(dt);
        public override void OnHoldCancelled(PlayerInteractor who) { if (lockMechanism != null) lockMechanism.CancelPick(); }

        public override void Interact(PlayerInteractor who, InteractContext ctx)
        {
            if (LockedNow)
            {
                if (!who.HasLockpick) { who.Notify("Está cerrada con llave."); return; }
                lockMechanism.Unlock();
                who.Notify("Cerradura forzada.");
                return;
            }
            Toggle(ctx.Slow, who.transform.position);
        }

        /// <summary>API pública: Wilson también podrá abrir/cerrar puertas con esto.</summary>
        public void Toggle(bool slow, Vector3 actorPosition)
        {
            if (swing != null) StopCoroutine(swing);

            IsOpen = !IsOpen;
            float target = 0f;
            if (IsOpen)
            {
                float side = 1f;
                if (swingAwayFromPlayer)
                    side = Vector3.Dot(actorPosition - hinge.position, hinge.forward) >= 0f ? 1f : -1f;
                target = side * openAngle;
                StealthEvents.EmitNoise(hinge.position, slow ? openNoiseSlow : openNoiseFast, NoiseType.Door, gameObject);
                Play(openClip, slow ? 0.25f : 1f);
            }

            StealthEvents.ReportAlteration(new WorldAlteration(hinge.position, AlterationType.DoorOpen, IsOpen, gameObject));

            float baseDuration = slow ? slowDuration : fastDuration;
            float fraction = Mathf.Abs(target - currentAngle) / Mathf.Max(1f, Mathf.Abs(openAngle));
            swing = StartCoroutine(Swing(target, Mathf.Max(0.2f, baseDuration * fraction), slow));
        }

        private IEnumerator Swing(float to, float duration, bool slow)
        {
            float from = currentAngle;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                currentAngle = Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t));
                hinge.localRotation = closedRotation * Quaternion.Euler(0f, currentAngle, 0f);
                yield return null;
            }

            currentAngle = to;
            hinge.localRotation = closedRotation * Quaternion.Euler(0f, to, 0f);
            swing = null;

            if (!IsOpen)  // el "golpe" del pestillo ocurre al terminar de cerrar
            {
                StealthEvents.EmitNoise(hinge.position, slow ? closeNoiseSlow : closeNoiseFast, NoiseType.Door, gameObject);
                Play(closeClip, slow ? 0.25f : 1f);
            }
        }

        // Gizmos (con la puerta seleccionada): esfera verde = bisagra, rojo = hacia donde se extiende el panel (+X), azul = normal (+Z).
        private void OnDrawGizmosSelected()
        {
            Transform h = hinge != null ? hinge : transform;
            Gizmos.color = Color.green; Gizmos.DrawSphere(h.position, 0.06f);
            Gizmos.color = Color.red;   Gizmos.DrawLine(h.position, h.position + h.right * 0.9f);
            Gizmos.color = Color.blue;  Gizmos.DrawLine(h.position, h.position + h.forward * 0.5f);
        }

        private void Play(AudioClip clip, float volume)
        {
            if (audioSource != null && clip != null) audioSource.PlayOneShot(clip, volume);
        }
    }
}
