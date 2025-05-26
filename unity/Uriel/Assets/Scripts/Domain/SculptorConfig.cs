using UnityEngine;

namespace Uriel.Domain
{
    [CreateAssetMenu(menuName = "Uriel/Sculptor")]
    public class SculptorConfig : ScriptableObject
    {
        public bool invertTriangles;
        public bool flipNormals;
        public float target, range;
        public int budget = 64;
        public int resolution = 64;
    }
}