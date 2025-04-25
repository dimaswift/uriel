using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Uriel.Domain;
using Uriel.Utils;

namespace Uriel.Behaviours
{
    public class CreatureRenderer : MonoBehaviour
    {
        [SerializeField] private GameObject sky;
        [SerializeField] private Material sourceMat;
        [SerializeField] private float step = 1;
        [SerializeField] private bool worldSpace;
        [SerializeField] private float height = 100;
        [SerializeField] private int sides = 3;
      
        [SerializeField] private float radius = 1;
        [SerializeField] private float amplitude = 10;
        [SerializeField] private int frequency;
        [SerializeField] private float scale = 10;
        [Range(0f, 0.01f)][SerializeField] private float scaleFine = 10;
        [Range(0, 5)] [SerializeField] private int operation;
        [SerializeField] private Creature creature;

        private Material mat;

        private ComputeBuffer geneBuffer;
        
        private readonly List<Constellation> constellations = new();
        
        private void Awake()
        {
            mat = Instantiate(sourceMat);
            GetComponent<Renderer>().sharedMaterial = mat;
        }
        
        private void UpdateGeneBuffer()
        {
            if (creature == null || sky == null)
            {
                return;
            }

            sky.GetComponentsInChildren(constellations);
            
            if (constellations.Count == 0)
            {
                return;
            }

            creature.genes.Clear();

            foreach (Constellation constellation in constellations)
            {
                constellation.FillGeneBuffer(creature.genes);
            }

            mat.SetInt("_GeneCount", creature.genes.Count);
            
            if (creature.genes.Count == 0)
            {
                return;
            }
            
            if (geneBuffer == null || geneBuffer.count != creature.genes.Count)
            {
                if(geneBuffer != null) geneBuffer.Release();
                geneBuffer = new ComputeBuffer(creature.genes.Count, Marshal.SizeOf(typeof(Gene)));
                mat.SetBuffer("_GeneBuffer", geneBuffer);
            }
            
            geneBuffer.SetData(creature.genes);
        }
        
        private void Update()
        {
            if (!creature)
            {
                return;
            }

            mat.SetFloat("_Scale", scale);
            mat.SetFloat("_Frequency", frequency);
            mat.SetFloat("_Amplitude", amplitude);
            mat.SetInt("_Sides", sides);
            mat.SetFloat("_Radius", radius);
            mat.SetFloat("_Height", height);
            mat.SetFloat("_Step", step);
            mat.SetVector("_Offset", transform.position);

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