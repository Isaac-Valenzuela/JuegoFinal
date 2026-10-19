using System;
using UnityEngine;

namespace LaResidencia
{
    public enum MovementState { Walking, Running, Crouching }

    /// <summary>
    /// Deduce el estado de movimiento SIN modificar el asset: usa el desplazamiento real y la escala
    /// vertical del jugador (ModularFirstPersonController escala el transform al agacharse).
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public class PlayerMovementTracker : MonoBehaviour
    {
        [SerializeField] private float movingThreshold = 0.35f;
        [Tooltip("Debe quedar entre walkSpeed y sprintSpeed del controller (5 y 7 por defecto).")]
        [SerializeField] private float runSpeedThreshold = 6f;
        [SerializeField, Range(0.3f, 0.99f)] private float crouchScaleRatio = 0.9f;

        public MovementState State { get; private set; } = MovementState.Walking;
        public bool IsMoving { get; private set; }
        public float Speed { get; private set; }
        public bool IsHidden { get; private set; }

        public event Action<MovementState> StateChanged;
        public event Action<bool> HiddenChanged;

        private float standingScaleY;
        private Vector3 lastPos;

        private void Awake()
        {
            standingScaleY = transform.localScale.y;
            lastPos = transform.position;
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f) return;

            Vector3 delta = transform.position - lastPos;
            delta.y = 0f;
            lastPos = transform.position;

            float instant = delta.magnitude / dt;
            Speed = Mathf.Lerp(Speed, instant, 1f - Mathf.Exp(-15f * dt));
            IsMoving = !IsHidden && Speed > movingThreshold;

            MovementState next;
            if (transform.localScale.y < standingScaleY * crouchScaleRatio) next = MovementState.Crouching;
            else if (Speed > runSpeedThreshold) next = MovementState.Running;
            else next = MovementState.Walking;

            if (next != State)
            {
                State = next;
                StateChanged?.Invoke(State);
            }
        }

        /// <summary>Llamar DESPUÉS de reposicionar al jugador (armarios) para no contar el teletransporte como velocidad.</summary>
        public void SetHidden(bool hidden)
        {
            lastPos = transform.position;
            Speed = 0f;
            if (IsHidden == hidden) return;
            IsHidden = hidden;
            HiddenChanged?.Invoke(hidden);
        }
    }
}
