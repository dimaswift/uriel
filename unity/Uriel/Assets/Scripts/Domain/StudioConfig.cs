using UnityEngine;

namespace Uriel.Domain
{
    [CreateAssetMenu(menuName = "Uriel/Studio")]
    public class StudioConfig : ScriptableObject
    {
        public int triangleBudget;
        public Vector3Int resolution = new (64, 64, 64);
        public GameObject[] prefabs;
    }
}