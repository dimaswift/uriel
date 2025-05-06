using UnityEngine;
using Uriel.Domain;

namespace Uriel.Behaviours
{
    [RequireComponent(typeof(MeshRenderer))]
    public class ShadowSky : MonoBehaviour
    {
        [SerializeField] private Sky sky;
        [SerializeField] private Transform source;

        
        private Material mat;
        
        private void Start()
        {
            mat = GetComponent<MeshRenderer>().material;
            gameObject.AddComponent<PhotonBuffer>().Init(sky).LinkMaterial(mat);
        }
        
        private void Update()  
        {
            mat.SetVector(ShaderProps.Source, source.position);
        }
    }
}