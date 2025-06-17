using UnityEngine;
using Uriel.Domain;

namespace Uriel.Behaviours
{
    public class SculptSolidBehaviour : MonoBehaviour
    {
        [SerializeField] private SculptSolid solid = SculptSolid.Default;
        public SculptSolid GetSolid()
        {
            var m = new Matrix4x4();
            m.SetTRS(transform.localPosition, transform.localRotation, transform.localScale);
            solid.invTransform = m.inverse;
            return solid;
        }

        public SculptSolidState GetState()
        {
            return new SculptSolidState()
            {
                solid = GetSolid(),
                scale = transform.localScale,
                position = transform.localPosition,
                rotation = transform.localEulerAngles
            };
        }
        
        public void RestoreState(SculptSolidState state)
        {
            transform.localPosition = state.position;
            transform.localScale = state.scale;
            transform.localEulerAngles = state.rotation;
            solid = state.solid;
        }
    }
}