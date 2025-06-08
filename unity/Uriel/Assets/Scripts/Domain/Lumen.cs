using System.Collections.Generic;
using UnityEngine;

namespace Uriel.Domain
{
    [CreateAssetMenu(menuName = "Uriel/Lumen")]
    public class Lumen : SerializableBuffer<Photon>
    {
        [SerializeField] private bool updateTransform;
        public List<Photon> photons = new();
        
        public void UpdateTransform(Matrix4x4 m)
        {
            if (!updateTransform) return;
            for (int i = 0; i < photons.Count; i++)
            {
                Photon p = photons[i];
                p.transform = m;
                photons[i] = p;
            }
        }
        
        protected override List<Photon> GetData()
        {
            return photons;
        }
        
        protected override void OnBeforeUpdate()
        {
            if (!updateTransform) return;
            
            for (int i = 0; i < photons.Count; i++)
            {
                Photon p = photons[i];
                if (p.transform == Matrix4x4.zero)
                {
                    p.transform = Matrix4x4.identity;
                    photons[i] = p;
                }
            }
        }

        public override int GetBufferHash()
        {
            var h = 0;
            foreach (var photon in photons)
            {
                h += photon.GetHashCode();
            }
            return h;
        }
    }
}