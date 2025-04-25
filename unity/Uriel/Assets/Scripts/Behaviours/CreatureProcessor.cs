using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Uriel.Domain;

namespace Uriel.Behaviours
{
    public class CreatureProcessor : MonoBehaviour
    {
        public event System.Action<ComputeBuffer> OnBufferCreated = b => { };
        public int GeneCount => creature.genes.Count;
        
        [SerializeField] private GameObject sky;
        [SerializeField] private Creature creature;
        
        private ComputeBuffer geneBuffer;
        private readonly List<Constellation> constellations = new();

        
        public ComputeBuffer GetGeneBuffer()
        {
            if (geneBuffer == null || geneBuffer.count != creature.genes.Count)
            {
                if (geneBuffer != null) geneBuffer.Release();
                geneBuffer = new ComputeBuffer(creature.genes.Count, Marshal.SizeOf(typeof(Gene)));
                OnBufferCreated(geneBuffer);
            }
            return geneBuffer;
        }
        
        public void UpdateGeneBuffer()
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
            
            if (creature.genes.Count == 0)
            {
                return;
            }

            GetGeneBuffer().SetData(creature.genes);
        }
        
        private void OnDestroy()
        {
            if (geneBuffer != null) geneBuffer.Release();
   
        }
    }
    
}