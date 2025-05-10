using UnityEngine;
using Uriel.Domain;

namespace Uriel.Behaviours
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshRenderer))]
    public class LightReceiver : MonoBehaviour
    {
        [SerializeField] private Transform lightSource;

        private Material mat;
        
        private void Awake()
        {
            mat = GetComponent<MeshRenderer>().sharedMaterial;
        }

        private void Update()
        {
            if(!mat || !lightSource) return;
            
            mat.SetVector(ShaderProps.LightSource, lightSource.position);
        }
        
    }
}