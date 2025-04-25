using UnityEngine;

namespace Uriel.Domain
{
    public static class ShaderProps
    {
        public static readonly int GeneCount = Shader.PropertyToID("_GeneCount");
        public static readonly int GeneBuffer = Shader.PropertyToID("_GeneBuffer");
        public static readonly int Particles = Shader.PropertyToID("_Particles");
        public static readonly int Resolution = Shader.PropertyToID("_Resolution");
        
    }
}