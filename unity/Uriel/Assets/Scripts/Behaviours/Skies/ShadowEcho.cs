using UnityEngine;
using Uriel.Domain;

namespace Uriel.Behaviours
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(PhotonBuffer))]
    public class ShadowEcho : MonoBehaviour
    {
        [SerializeField] private Lumen lumen;
        [SerializeField] private Transform source;
        
        private Material mat;
        
        private void Start()
        {
            mat = GetComponent<MeshRenderer>().sharedMaterial;
        }
        
        private void Update()  
        {
            if (source == null)
            {
                return;
            }
            mat.SetVector(ShaderProps.Source, source.position);
        }
    }
}