using System;
using System.Runtime.InteropServices;
using UnityEngine;
using Uriel.Domain;

namespace Uriel.Behaviours
{
    public class CreatureRenderer : MonoBehaviour
    {
        [SerializeField] private Creature creature;

        private Material mat;

        private ComputeBuffer geneBuffer;
        private int geneCount;
        private int geneStride;

        private int lastGeneCount;
        
        private void Awake()
        {
            mat = new Material(Shader.Find("Uriel/Creature"));
            GetComponent<Renderer>().material = mat;
        }

        private void InitializeGeneBuffer()
        {
            if (geneBuffer != null) geneBuffer.Release();
            geneStride = Marshal.SizeOf(typeof(Gene));
            geneCount = creature.genes.Length;
            geneBuffer = new ComputeBuffer(geneCount, geneStride);
            mat.SetBuffer("_GeneBuffer", geneBuffer);
        }

        private void UpdateGeneBuffer()
        {
            if (creature == null || creature.genes == null || creature.genes.Length == 0)
            {
                return;
            }
            if (lastGeneCount != creature.genes.Length)
            {
                InitializeGeneBuffer();
            }
            geneBuffer.SetData(creature.genes);
            mat.SetInt("_GeneCount", geneCount);
            mat.SetVector("_Offset", creature.offset);
            mat.SetFloat("_Scale", creature.scale);
            lastGeneCount = geneCount;
        }  
        
        private void Update()
        {
            if (!creature)
            {
                return;
            }

            UpdateGeneBuffer();

        }

        private void OnDestroy()
        {
            if(geneBuffer != null) geneBuffer.Release();
            if (mat)
            {
                Destroy(mat);
            }
        }
    }
}