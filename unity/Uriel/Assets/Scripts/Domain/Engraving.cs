using UnityEngine;

namespace Uriel.Domain
{
    [CreateAssetMenu(menuName = "Uriel/Engraving")]
    public class Engraving : ScriptableObject
    {
        [Range(0, 1000)] public int minPower = 100;
        [Range(0, 1000)] public int maxPower = 1000;
        [Range(1, 10000)] public int minFeedRate = 1000;
        [Range(1, 10000)] public int maxFeedRate = 1000;
        [Range(0.001f, 1f)] public float precision = 0.06f;
        public float width = 50;
        public float height = 50;
        public Harbor harbor;
        public Vector3Int steps;
        public float scale = 1f;
        public float speedThreshold = 1f;
        public float speedMultiplier = 1f;
        public float powerThreshold = 1f;
        public float powerMultiplier = 1f;
        public float zStart;
        public float zEnd;
        public float zStep;
    }
}