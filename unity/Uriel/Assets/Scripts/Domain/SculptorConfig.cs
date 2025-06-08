using UnityEngine;

namespace Uriel.Domain
{
    [CreateAssetMenu(menuName = "Uriel/Sculptor")]
    public class SculptorConfig : ScriptableObject
    {
        public Sculpt sculpt;
        public int budget = 64;
        public int resolution = 64;
        public Vector4[] shells;
    }

    [System.Serializable]
    public struct Sculpt
    {
        public float shell;
        public float radius;
        public float innerRadius;
        public float transitionWidth;
        public bool flipNormals;
        public bool invertTriangles;
        public Vector3 ellipsoidScale;
        public int radialSymmetryCount;
        public Vector3 core;
        [Range(-0.1f, 0.1f)] public float coreStrength;
        public float coreRadius;
        public float scale;
        public float shrink;
        
    }
}