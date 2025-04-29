using UnityEngine;
using Uriel.Domain;

namespace Uriel.Behaviours
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(HarborBehaviour))]
    public class ShadowHarbor : MonoBehaviour
    {
        [SerializeField] private Transform source;
        
        private HarborBehaviour harbor;

        private Material mat;
        
        private void Start()
        {
            mat = GetComponent<MeshRenderer>().material;
            harbor = GetComponent<HarborBehaviour>();
            harbor.OnBufferCreated += OnBufferCreated;
        }

        private void OnBufferCreated(ComputeBuffer buffer)
        {
            mat.SetInt(ShaderProps.WaveCount, harbor.WaveCount);
            mat.SetBuffer(ShaderProps.WaveBuffer, buffer);
        }
        
        
        private void Update()  
        {  
            if (harbor == null || mat == null)  
                return;
            harbor.UpdateWaveBuffer();
            mat.SetVector(ShaderProps.Source, source.position);
        }
    }
}