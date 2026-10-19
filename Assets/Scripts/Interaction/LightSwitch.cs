using UnityEngine;

namespace LaResidencia
{
    /// <summary>Interruptor: apagar favorece el sigilo, pero queda registrado como alteración que Wilson puede notar.</summary>
    public class LightSwitch : Interactable
    {
        [SerializeField] private Light[] lights;
        [SerializeField] private bool isOn = true;
        [SerializeField] private float noiseRadius = 1.5f;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip clickClip;

        private bool initialState;

        private void Awake()
        {
            initialState = isOn;
            Apply();
        }

        public override string GetPrompt(PlayerInteractor who) => isOn ? "Apagar luz" : "Encender luz";

        public override void Interact(PlayerInteractor who, InteractContext ctx)
        {
            isOn = !isOn;
            Apply();

            if (audioSource != null && clickClip != null) audioSource.PlayOneShot(clickClip);
            StealthEvents.EmitNoise(transform.position, noiseRadius, NoiseType.Switch, who.gameObject);
            StealthEvents.ReportAlteration(
                new WorldAlteration(transform.position, AlterationType.LightChanged, isOn != initialState, gameObject));
        }

        private void Apply()
        {
            if (lights == null) return;
            foreach (Light l in lights)
                if (l != null) l.enabled = isOn;
        }
    }
}
