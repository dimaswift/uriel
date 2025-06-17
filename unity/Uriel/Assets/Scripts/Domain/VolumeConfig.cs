using Uriel.Behaviours;

namespace Uriel.Domain
{
    [System.Serializable]
    public struct VolumeConfig 
    {
        public FieldConfig field;
        public MarchingCubesConfig marchingCubes;
        public static VolumeConfig Default = new()
        {
            field = FieldConfig.Default,
            marchingCubes = MarchingCubesConfig.Default
        };
    }
}