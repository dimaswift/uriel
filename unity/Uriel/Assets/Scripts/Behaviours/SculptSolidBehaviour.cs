using UnityEngine;
using Uriel.Domain;

namespace Uriel.Behaviours
{
    public class SculptSolidBehaviour : MonoBehaviour
    {
        [SerializeField] private float scale = 1f;
        [Range(0f, 0.5f)] [SerializeField] private float feather = 0.1f;
        [SerializeField] private SculptOperation operation;
        [SerializeField] private SculptSolidType type;
        
        public SculptSolid GetSolid()
        {
    
            return new SculptSolid()
            {
                invTransform = transform.localToWorldMatrix.inverse,
                scale = scale,
                type = type,
                op = operation,
                feather = feather
            };  
        }
    }
}