using UnityEngine;

namespace Uriel.Domain
{
    [CreateAssetMenu(menuName = "Uriel/Sky Config")]
    public class SkyConfig : ScriptableObject
    {
        public int capacity = 128;
        public float simulationSpeed = 1f;
        public float radius = 1;
        public Vector4 offset;
        public Star[] protoStars;
    }
}