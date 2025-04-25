using System.Collections.Generic;
using UnityEngine;
using Uriel.Domain;

namespace Uriel.Domain
{
    [CreateAssetMenu(menuName = "Uriel/Creature")]
    public class Creature : ScriptableObject
    {
        public float scale = 1f;
        public Vector3 offset;
        public List<Gene> genes = new();
    }
}