using UnityEngine;

namespace Uriel.Domain
{
    [CreateAssetMenu(menuName = "Uriel/Resonance Cascade Config")]
    public class ResonanceCascadeConfig : ScriptableObject
    {
        public Texture gradient;
        public float gradientThreshold;
        public float gradientMultiplier;
        public uint fieldResolution = 128;
        public uint particleResolution = 128;
        public Vector3 threshold;
        public Vector3 multiplier;
        public int dimensions = 5;
        [Range(1, 32)] public uint scanRadius = 1;
        public float scale = 1f;
        public int spawnRate = 1;
        public int particleRadius = 1;
        public float canvasFadeSpeed = 1f;
        public float phaseSpeed;
        public float lifetime;
        public float gravity;
        public float acceleration;
        public float speed;
        public float attraction = 1;
        public float repulsion = 1;
        public float radius = 1;
    }
}