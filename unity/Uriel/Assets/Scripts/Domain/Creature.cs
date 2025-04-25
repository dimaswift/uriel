using System.Collections.Generic;
using UnityEngine;


namespace Uriel.Domain
{
    [CreateAssetMenu(menuName = "Uriel/Creature")]
    public class Creature : ScriptableObject
    {
        public List<Gene> genes = new();
    }
}