using UnityEngine;

namespace LaResidencia
{
    /// <summary>Lanza un raycast hacia abajo solo cuando se le pide (al dar un paso), con caché por collider.</summary>
    public class SurfaceDetector : MonoBehaviour
    {
        [SerializeField] private SurfaceNoiseProfile fallbackProfile;
        [SerializeField] private LayerMask groundMask = ~0;
        [Tooltip("Desde el centro de la cápsula (≈1 m sobre el suelo).")]
        [SerializeField] private float rayDistance = 1.4f;

        private Collider lastCollider;
        private SurfaceZone lastZone;

        public bool TrySample(out SurfaceNoiseProfile profile, out float multiplier, out Vector3 point)
        {
            profile = fallbackProfile;
            multiplier = 1f;
            point = transform.position;

            if (!Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, rayDistance,
                                 groundMask, QueryTriggerInteraction.Ignore))
                return false;

            point = hit.point;
            if (hit.collider != lastCollider)
            {
                lastCollider = hit.collider;
                lastZone = hit.collider.GetComponentInParent<SurfaceZone>();
            }

            if (lastZone != null && lastZone.Profile != null)
            {
                profile = lastZone.Profile;
                multiplier = lastZone.RadiusMultiplier;
            }
            return profile != null;
        }
    }
}
