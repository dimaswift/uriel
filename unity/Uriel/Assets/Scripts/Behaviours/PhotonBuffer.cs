using UnityEngine;
using Uriel.Domain;

namespace Uriel.Behaviours
{
    public class PhotonBuffer : SerializableBufferHandler<Lumen>
    {
        [SerializeField] private Transform source;
        protected override void OnBeforeUpdate()
        {
            if (source)
            {
                Buffer.UpdateTransform(source.localToWorldMatrix);
            }
        }
  
    }
}