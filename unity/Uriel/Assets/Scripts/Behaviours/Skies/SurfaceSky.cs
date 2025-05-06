using UnityEngine;
using Uriel.Domain;

namespace Uriel.Behaviours
{
    [RequireComponent(typeof(MeshRenderer))]
    public class SurfaceSky : MonoBehaviour
    {
        private Material mat;
        
        private void Awake()
        {
            mat = GetComponent<MeshRenderer>().material;
            gameObject.AddComponent<PhotonBuffer>().LinkMaterial(mat);
        }
    }
}