using UnityEngine;

namespace Uriel.Behaviours
{
    [RequireComponent(typeof(Renderer))]
    public class SelectionGizmo : MonoBehaviour
    {
        [SerializeField] private Material selectedMaterial;
        [SerializeField] private Material regularMaterial;
        
        private Renderer rend;
        
        public void SetSelected(bool selected)
        {
            if (!rend) rend = GetComponent<Renderer>();
            rend.sharedMaterial = selected ? selectedMaterial : regularMaterial;
        }
    }
}