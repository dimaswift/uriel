using System.Collections.Generic;
using UnityEngine;

namespace Uriel.Domain
{
    [CreateAssetMenu(menuName = "Uriel/Sculpt State")]
    public class SculptState : ScriptableObject
    {
        public List<SculptSolidState> solids = new();
    }
}