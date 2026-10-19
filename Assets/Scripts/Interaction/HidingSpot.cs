using UnityEngine;

namespace LaResidencia
{
    /// <summary>
    /// Armario. Al entrar: se desactiva el controller del asset, el jugador pasa a hidePoint y la cámara
    /// queda limitada a un arco estrecho (las rejillas). Interactuar de nuevo para salir.
    /// hidePoint debe estar a la altura del centro de la cápsula del jugador; su forward mira a las rejillas.
    /// </summary>
    public class HidingSpot : Interactable
    {
        [SerializeField] private Transform hidePoint;
        [SerializeField] private Transform exitPoint;

        [Header("Vista desde las rejillas")]
        [SerializeField] private float maxYaw = 40f;
        [SerializeField] private float maxPitch = 22f;
        [SerializeField] private float lookSensitivity = 2f;

        [Header("Ruido (radio en m)")]
        [SerializeField] private float enterNoise = 2f;
        [SerializeField] private float exitNoise = 2.5f;

        private PlayerInteractor occupant;
        private Rigidbody body;
        private Collider bodyCollider;
        private Transform cam;
        private float yaw, pitch;
        private int enterFrame;

        public bool IsOccupied => occupant != null;

        public override string GetPrompt(PlayerInteractor who) => "Esconderse";
        public override bool CanInteract(PlayerInteractor who) => occupant == null && !who.Tracker.IsHidden;
        public override void Interact(PlayerInteractor who, InteractContext ctx) => Enter(who);

        private void Enter(PlayerInteractor who)
        {
            occupant = who;
            body = who.GetComponent<Rigidbody>();
            bodyCollider = who.GetComponent<Collider>();
            cam = who.PlayerCamera.transform;

            if (who.MovementController != null) who.MovementController.enabled = false;
            if (body != null) { PhysicsCompat.Stop(body); body.isKinematic = true; }
            if (bodyCollider != null) bodyCollider.enabled = false;

            who.transform.SetPositionAndRotation(hidePoint.position, Quaternion.Euler(0f, hidePoint.eulerAngles.y, 0f));
            Physics.SyncTransforms();

            yaw = 0f;
            pitch = 0f;
            cam.localRotation = Quaternion.identity;

            who.Tracker.SetHidden(true);
            enterFrame = Time.frameCount;
            StealthEvents.EmitNoise(hidePoint.position, enterNoise, NoiseType.Hiding, who.gameObject);
        }

        private void Update()
        {
            if (occupant == null) return;

            yaw = Mathf.Clamp(yaw + Input.GetAxis("Mouse X") * lookSensitivity, -maxYaw, maxYaw);
            pitch = Mathf.Clamp(pitch - Input.GetAxis("Mouse Y") * lookSensitivity, -maxPitch, maxPitch);
            cam.localRotation = Quaternion.Euler(pitch, yaw, 0f);

            if (Time.frameCount > enterFrame && occupant.InteractDown) Exit();
        }

        private void Exit()
        {
            PlayerInteractor who = occupant;
            occupant = null;

            who.transform.SetPositionAndRotation(exitPoint.position, Quaternion.Euler(0f, exitPoint.eulerAngles.y, 0f));
            cam.localRotation = Quaternion.identity;

            if (bodyCollider != null) bodyCollider.enabled = true;
            if (body != null) body.isKinematic = false;
            Physics.SyncTransforms();

            if (who.MovementController != null) who.MovementController.enabled = true;
            who.Tracker.SetHidden(false);
            who.SuppressInput();

            StealthEvents.EmitNoise(exitPoint.position, exitNoise, NoiseType.Hiding, who.gameObject);
        }
    }
}
