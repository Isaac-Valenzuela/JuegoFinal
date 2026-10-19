using UnityEngine;

namespace LaResidencia
{
    /// <summary>
    /// Se coloca en el collider del suelo (o en su padre: una habitación entera).
    /// El multiplicador permite que la escalera "resuene" más que el resto de la madera.
    /// </summary>
    public class SurfaceZone : MonoBehaviour
    {
        [SerializeField] private SurfaceNoiseProfile profile;
        [SerializeField, Min(0.1f)] private float radiusMultiplier = 1f;

        public SurfaceNoiseProfile Profile => profile;
        public float RadiusMultiplier => radiusMultiplier;
    }
}
