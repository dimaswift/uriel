using System.Collections.Generic;
using UnityEngine;

namespace Uriel.Domain
{
    [CreateAssetMenu(menuName = "Uriel/Sky")]
    public class Sky : ScriptableObject
    {
        public List<Photon> photons = new();
    }
}