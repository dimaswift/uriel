using System;
using UnityEngine;

namespace Uriel.Behaviours
{
    public enum SelectableState
    {
        None,
        Hover,
        Selected
    }

    public class SelectableGizmo : MonoBehaviour
    {
        [SerializeField] private Material hoverMaterial, regularMaterial, selectedMaterial;
     
        private Renderer rend;
   
      
        private void Awake()
        {
            rend = GetComponent<Renderer>();
        }
        
        public void SetState(SelectableState state)
        {
            if (!rend)
            {
                rend = GetComponent<Renderer>();
            }
            switch (state)
            {
                case SelectableState.Hover:
                    rend.sharedMaterial = hoverMaterial;
                    break;
                case SelectableState.Selected:
                    rend.sharedMaterial = selectedMaterial;
                    break;
                default:
                    rend.sharedMaterial = regularMaterial;
                    break;
            }
        }
    }
}