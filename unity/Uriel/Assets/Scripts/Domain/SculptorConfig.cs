using UnityEngine;

namespace Uriel.Domain
{
    [CreateAssetMenu(menuName = "Uriel/Sculptor")]
    public class SculptorConfig : ScriptableObject
    {
        public int fieldResolution = 64;
        public int shells = 1;
        public int capacity = 64;
        public int particleRadius;
        public float bounds;
        public float gravity;
        public float acceleration;
        public float speed;
        public float sampleRadius = 0.1f;
        public float particleSize = 0.1f;
        public int frequency;
    }
}