using UnityEngine;

namespace Uriel.Domain
{
    public static class ShaderProps
    {
        public static readonly int WaveCount = Shader.PropertyToID("_WaveCount");
        public static readonly int WaveBuffer = Shader.PropertyToID("_WaveBuffer");
        public static readonly int Particles = Shader.PropertyToID("_Particles");
        public static readonly int Resolution = Shader.PropertyToID("_Resolution");
        public static readonly int Source = Shader.PropertyToID("_Source");
        public static readonly int Steps = Shader.PropertyToID("_Steps");
        public static readonly int Gradient = Shader.PropertyToID("_Gradient");
        public static readonly int GradientMultiplier = Shader.PropertyToID("_GradientMultiplier");
        public static readonly int GradientThreshold = Shader.PropertyToID("_GradientThreshold");
        public static readonly int Frequency = Shader.PropertyToID("_Frequency");
        public static readonly int Amplitude = Shader.PropertyToID("_Amplitude");
        public static readonly int Min = Shader.PropertyToID("_Min");
        public static readonly int Depth = Shader.PropertyToID("_Depth");
        public static readonly int Max = Shader.PropertyToID("_Max");
        public static readonly int Normal = Shader.PropertyToID("_Normal");
        public static readonly int Grayscale = Shader.PropertyToID("_Grayscale");
        public static readonly int BaseMap = Shader.PropertyToID("_BaseMap");
        public static readonly int Target = Shader.PropertyToID("_Target");
        public static readonly int MainTex = Shader.PropertyToID("_MainTex");
        public static readonly int Result = Shader.PropertyToID("_Result");
        public static readonly int Size = Shader.PropertyToID("_Size");
        public static readonly int Width = Shader.PropertyToID("_Width");
        public static readonly int Height = Shader.PropertyToID("_Height");
        public static readonly int Focus = Shader.PropertyToID("_Focus");
    }
}

