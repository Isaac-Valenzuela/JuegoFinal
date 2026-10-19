using System;
using UnityEngine;

namespace LaResidencia
{
    /// <summary>
    /// Raycast desde el centro de la cámara. Clic izquierdo (o F) interactúa; si el objetivo exige
    /// mantener (cerraduras), gestiona la barra de progreso. Va en el GameObject raíz del jugador.
    /// </summary>
    [RequireComponent(typeof(PlayerMovementTracker))]
    public class PlayerInteractor : MonoBehaviour
    {
        [Header("Raycast")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float interactDistance = 2.2f;
        [Tooltip("Excluye la capa del jugador. NO limites a una sola capa: las paredes deben bloquear el rayo.")]
        [SerializeField] private LayerMask raycastMask = ~0;

        [Header("Entrada")]
        [SerializeField] private KeyCode primaryKey = KeyCode.Mouse0;
        [SerializeField] private KeyCode secondaryKey = KeyCode.F;
        [SerializeField] private bool slowWhenCrouched = true;

        [Header("Equipo de Leo")]
        [SerializeField] private bool hasLockpick = true;

        [Header("Referencias")]
        [Tooltip("El componente FirstPersonController del asset (se desactiva al esconderse).")]
        [SerializeField] private Behaviour movementController;
        [SerializeField] private BackpackInventory inventory;

        public Camera PlayerCamera => playerCamera;
        public PlayerMovementTracker Tracker { get; private set; }
        public BackpackInventory Inventory => inventory;
        public Behaviour MovementController => movementController;
        public bool HasLockpick => hasLockpick;

        public bool InteractDown => Input.GetKeyDown(primaryKey) || Input.GetKeyDown(secondaryKey);
        public bool InteractHeld => Input.GetKey(primaryKey) || Input.GetKey(secondaryKey);

        public event Action<string> PromptChanged;
        public event Action<float> HoldProgressChanged;   // 0..1
        public event Action<string> MessageRaised;

        private Interactable target, holdTarget, lastInteractable;
        private Collider lastCollider;
        private bool holding, waitForRelease;
        private float holdTime;
        private string lastPrompt = string.Empty;
        private int suppressUntilFrame = -1;

        private void Awake()
        {
            Tracker = GetComponent<PlayerMovementTracker>();
            if (playerCamera == null) playerCamera = GetComponentInChildren<Camera>();
            if (inventory == null) inventory = GetComponent<BackpackInventory>();
        }

        private void OnDisable() => CancelHold();

        public void Notify(string message) => MessageRaised?.Invoke(message);

        /// <summary>Evita que el mismo clic que salió de un armario vuelva a activar otra interacción.</summary>
        public void SuppressInput() => suppressUntilFrame = Time.frameCount;

        private void Update()
        {
            if (Tracker.IsHidden || Time.frameCount <= suppressUntilFrame)
            {
                CancelHold();
                SetPrompt(string.Empty);
                return;
            }

            Interactable found = RaycastInteractable();
            if (found != target)
            {
                CancelHold();
                target = found;
            }

            SetPrompt(target != null && target.CanInteract(this) ? target.GetPrompt(this) : string.Empty);
            HandleInput();
        }

        private Interactable RaycastInteractable()
        {
            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (!Physics.Raycast(ray, out RaycastHit hit, interactDistance, raycastMask, QueryTriggerInteraction.Ignore))
                return null;

            if (hit.collider != lastCollider)
            {
                lastCollider = hit.collider;
                lastInteractable = hit.collider.GetComponentInParent<Interactable>();
            }
            return lastInteractable;
        }

        private void HandleInput()
        {
            if (waitForRelease)
            {
                if (!InteractHeld) waitForRelease = false;
                return;
            }

            if (target == null || !target.CanInteract(this))
            {
                CancelHold();
                return;
            }

            float duration = target.GetHoldDuration(this);

            if (!holding)
            {
                if (!InteractDown) return;
                if (duration <= 0f) { Execute(target); return; }

                holding = true;
                holdTime = 0f;
                holdTarget = target;
            }

            if (!InteractHeld || holdTarget != target || duration <= 0f)
            {
                CancelHold();
                return;
            }

            holdTime += Time.deltaTime;
            target.OnHoldTick(this, Time.deltaTime);
            HoldProgressChanged?.Invoke(Mathf.Clamp01(holdTime / duration));

            if (holdTime >= duration)
            {
                Interactable done = target;
                holding = false;
                holdTarget = null;
                HoldProgressChanged?.Invoke(0f);
                Execute(done);
                waitForRelease = true;   // evita que el mismo clic abra la puerta recién desbloqueada
            }
        }

        private void Execute(Interactable t)
        {
            bool slow = slowWhenCrouched && Tracker.State == MovementState.Crouching;
            t.Interact(this, new InteractContext(slow));
        }

        private void CancelHold()
        {
            if (!holding) return;
            holding = false;
            if (holdTarget != null) holdTarget.OnHoldCancelled(this);
            holdTarget = null;
            HoldProgressChanged?.Invoke(0f);
        }

        private void SetPrompt(string prompt)
        {
            if (prompt == lastPrompt) return;
            lastPrompt = prompt;
            PromptChanged?.Invoke(prompt);
        }
    }
}
