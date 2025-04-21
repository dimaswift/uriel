using UnityEngine;
using Uriel.Domain;

namespace Uriel.Domain
{
    [CreateAssetMenu(menuName = "Uriel/Creature")]
    public class Creature : ScriptableObject
    {
        public float scale = 1f;
        public Vector3 offset;
        public Gene[] genes;
    }
}