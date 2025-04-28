using UnityEngine;
using Uriel.Domain;

namespace Uriel.Behaviours
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(CreatureProcessor))]
    public class CreatureShadowRenderer : MonoBehaviour
    {
        [SerializeField] private Transform source;
        
        private CreatureProcessor processor;

        private Material mat;
        
        private void Start()
        {
            mat = GetComponent<MeshRenderer>().material;
            processor = GetComponent<CreatureProcessor>();
            processor.OnBufferCreated += OnBufferCreated;
        }

        private void OnBufferCreated(ComputeBuffer buffer)
        {
            mat.SetInt(ShaderProps.GeneCount, processor.GeneCount);
            mat.SetBuffer(ShaderProps.GeneBuffer, buffer);
        }
        
        
        private void Update()  
        {  
            if (processor == null || mat == null)  
                return;
            processor.UpdateGeneBuffer();
            mat.SetVector(ShaderProps.Source, source.position);
        }
    }
}