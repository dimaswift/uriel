using UnityEngine;
using Uriel.Domain;

namespace Uriel.Behaviours
{
    [RequireComponent(typeof(MeshRenderer))]
    public class SurfaceSky : MonoBehaviour
    {
        [SerializeField] private Sky sky;
        private Material mat;
        
        private void Awake()
        {
            gameObject.AddComponent<PhotonBuffer>().Init(sky).LinkMaterial(GetComponent<MeshRenderer>().material);
        }
    }
}