using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LaResidencia
{
    /// <summary>Texto de "Abrir puerta", mensajes breves y barra circular de forzado de cerradura.</summary>
    public class InteractionPromptUI : MonoBehaviour
    {
        [SerializeField] private PlayerInteractor interactor;
        [SerializeField] private TMP_Text promptText;
        [SerializeField] private TMP_Text messageText;
        [Tooltip("Image con Image Type = Filled (Radial 360).")]
        [SerializeField] private Image holdFill;
        [SerializeField] private float messageSeconds = 2f;

        private float messageTimer;

        private void OnEnable()
        {
            interactor.PromptChanged += OnPrompt;
            interactor.HoldProgressChanged += OnProgress;
            interactor.MessageRaised += OnMessage;
            OnPrompt(string.Empty);
            OnProgress(0f);
            if (messageText != null) messageText.text = string.Empty;
        }

        private void OnDisable()
        {
            interactor.PromptChanged -= OnPrompt;
            interactor.HoldProgressChanged -= OnProgress;
            interactor.MessageRaised -= OnMessage;
        }

        private void Update()
        {
            if (messageTimer <= 0f) return;
            messageTimer -= Time.deltaTime;
            if (messageTimer <= 0f && messageText != null) messageText.text = string.Empty;
        }

        private void OnPrompt(string text) { if (promptText != null) promptText.text = text; }

        private void OnProgress(float value)
        {
            if (holdFill == null) return;
            holdFill.fillAmount = value;
            holdFill.enabled = value > 0f;
        }

        private void OnMessage(string text)
        {
            if (messageText == null) return;
            messageText.text = text;
            messageTimer = messageSeconds;
        }
    }
}
