using UnityEngine;

namespace Uriel.Domain
{
    [CreateAssetMenu(menuName = "Uriel/Studio")]
    public class StudioConfig : ScriptableObject
    {
        public GameObject[] prefabs;
    }
}