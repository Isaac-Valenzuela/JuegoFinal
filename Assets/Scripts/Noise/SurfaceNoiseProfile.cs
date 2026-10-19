using UnityEngine;

namespace LaResidencia
{
    public enum SurfaceType { Carpet, Wood, Tile, Gravel }

    /// <summary>Radios de ruido (metros) por estado de movimiento para una superficie. Un asset por superficie.</summary>
    [CreateAssetMenu(menuName = "La Residencia/Surface Noise Profile", fileName = "Surface_")]
    public class SurfaceNoiseProfile : ScriptableObject
    {
        public SurfaceType surface = SurfaceType.Wood;

        [Min(0f)] public float crouchRadius = 2f;
        [Min(0f)] public float walkRadius = 5f;
        [Min(0f)] public float runRadius = 11f;

        public AudioClip[] footstepClips;

        public float GetRadius(MovementState state)
        {
            switch (state)
            {
                case MovementState.Crouching: return crouchRadius;
                case MovementState.Running: return runRadius;
                default: return walkRadius;
            }
        }
    }
}
