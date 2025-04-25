using UnityEngine;
using Uriel.Domain;

namespace Uriel.Behaviours
{
    [RequireComponent(typeof(CreatureProcessor))]
    public class TextureCreatureRenderer : MonoBehaviour
    {
        [SerializeField] private Material sourceMat;
        private CreatureProcessor processor;
        private Material mat;
        
        private void Awake()
        {
            mat = Instantiate(sourceMat);
            GetComponent<Renderer>().sharedMaterial = mat;
            processor = GetComponent<CreatureProcessor>();
            processor.OnBufferCreated += b =>
            {
                mat.SetBuffer(ShaderProps.GeneBuffer, processor.GetGeneBuffer());
            };
        }

        private void Update()
        {
            processor.UpdateGeneBuffer();
            mat.SetInt(ShaderProps.GeneCount, processor.GeneCount);
        }

        private void OnDestroy()
        {
            if (mat)
            {
                Destroy(mat);
            }
        }
    }
}