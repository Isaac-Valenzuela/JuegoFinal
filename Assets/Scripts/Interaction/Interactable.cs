using UnityEngine;

namespace LaResidencia
{
    public readonly struct InteractContext
    {
        /// <summary>true cuando el jugador interactúa agachado: puertas lentas y silenciosas.</summary>
        public readonly bool Slow;
        public InteractContext(bool slow) { Slow = slow; }
    }

    /// <summary>Base de todo lo que el jugador puede usar con clic izquierdo / F.</summary>
    public abstract class Interactable : MonoBehaviour
    {
        public virtual string GetPrompt(PlayerInteractor who) => "Interactuar";
        public virtual bool CanInteract(PlayerInteractor who) => true;

        /// <summary>0 = instantáneo. &gt; 0 = el jugador debe mantener el botón (forzar cerraduras).</summary>
        public virtual float GetHoldDuration(PlayerInteractor who) => 0f;

        public abstract void Interact(PlayerInteractor who, InteractContext context);

        public virtual void OnHoldTick(PlayerInteractor who, float deltaTime) { }
        public virtual void OnHoldCancelled(PlayerInteractor who) { }
    }
}
