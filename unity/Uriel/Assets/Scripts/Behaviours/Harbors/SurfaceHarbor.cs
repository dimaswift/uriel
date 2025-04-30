using UnityEngine;
using Uriel.Domain;

namespace Uriel.Behaviours
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(HarborBehaviour))]
    public class SurfaceHarbor : MonoBehaviour
    {
        private HarborBehaviour harbor;
        private Material mat;
        
        private void Awake()
        {
            mat = GetComponent<MeshRenderer>().material;
            harbor = GetComponent<HarborBehaviour>();
            harbor.OnBufferCreated += b =>
            {
                mat.SetInt(ShaderProps.WaveCount, harbor.WaveCount);
                mat.SetBuffer(ShaderProps.WaveBuffer, harbor.GetWaveBuffer());
            };
        }

        private void Update()
        {
            harbor.UpdateWaveBuffer();
        }
    }
}