using UnityEngine;
using Uriel.Behaviours;

namespace Uriel.Domain
{
    [CreateAssetMenu(menuName = "Uriel/Studio")]
    public class VolumeStudioConfig : ScriptableObject
    {
        public int triangleBudget;
        public Vector3Int resolution = new (64, 64, 64);
        public VolumeConfig @default = VolumeConfig.Default;
        public Volume volumePrefab;
        public WaveEmitter waveEmitterPrefab;
    }
}