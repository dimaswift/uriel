using System.Collections.Generic;
using UnityEngine;

namespace Uriel.Domain
{
    [CreateAssetMenu(menuName = "Uriel/Harbor")]
    public class Harbor : ScriptableObject
    {
        public List<Wave> waves = new();
    }
}