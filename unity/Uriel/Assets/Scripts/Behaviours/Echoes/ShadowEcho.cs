using UnityEngine;
using Uriel.Domain;

namespace Uriel.Behaviours
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(PhotonBuffer))]
    public class ShadowEcho : MonoBehaviour
    {
        [SerializeField] private Transform source;
        
        private Material mat;
        
        private void Start()
        {
            mat = GetComponent<MeshRenderer>().sharedMaterial;
        }
        
        private void Update()  
        {
            if (mat == null)
            {
                mat = GetComponent<MeshRenderer>().sharedMaterial;
            }
            if (source == null)
            {
                return;
            }
            mat.SetVector(ShaderProps.Source, source.position);
        }
    }
}