using Uriel.Behaviours;

namespace Uriel.Domain
{
    [System.Serializable]
    public struct VolumeConfig 
    {
        public MarchingCubesConfig marchingCubes;
        public static VolumeConfig Default = new()
        {
            marchingCubes = MarchingCubesConfig.Default
        };
    }
}