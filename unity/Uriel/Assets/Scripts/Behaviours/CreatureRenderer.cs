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
        [SerializeField] private float height = 100;
        [SerializeField] private int sides = 3;
        [SerializeField] private PolyhedronGenerator.PolyhedronType fieldType;
        [SerializeField] private float radius = 1;
        [SerializeField] private Transform skew;
        [SerializeField] private float amplitude = 10;
        [SerializeField] private int frequency;
        [SerializeField] private float scale = 10;
        [Range(0f, 0.01f)][SerializeField] private float scaleFine = 10;
        [Range(0, 5)] [SerializeField] private int operation;
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
         
            var vertices = new List<Vector3>();
            PolyhedronGenerator.GenerateVertices(vertices, fieldType, sides, radius, height);
            creature.genes = new Gene[vertices.Count];
            geneCount = creature.genes.Length;
            for (int i = 0; i < vertices.Count; i++)
            {
                var t = new GameObject(i.ToString());
                t.transform.SetParent(transform);
                t.transform.localPosition = vertices[i];
            }

            geneBuffer = new ComputeBuffer(geneCount, geneStride);
            mat.SetBuffer("_GeneBuffer", geneBuffer);
            mat.SetInt("_GeneCount", geneCount);
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

            var skewScale = skew.localScale;
            for (int i = 0; i < transform.childCount; i++)
            {
                var t = transform.GetChild(i);
                var pos = -t.position;
                var skewed = new Vector3(pos.x * skewScale.x, pos.y * skewScale.y, pos.z * skewScale.z);
                creature.genes[i] = new Gene()
                {
                    offset = skewed,
                    scale = scale + scaleFine,
                    iterations = 3,
                    amplitude = amplitude,
                    frequency = frequency,
                    operation = operation
                };
            }
            geneBuffer.SetData(creature.genes);
            mat.SetFloat("_Scale", creature.scale);
            lastGeneCount = geneCount;
        }

        private void OnDrawGizmos()
        {
            if (creature == null)
            {
                return;
            }

            foreach (Gene g1 in creature.genes)
            {
                foreach (Gene g2 in creature.genes)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(g1.offset, g2.offset);
                }
               
            }

            // foreach (Gene gene in creature.genes)
            // {
            //     Vector3? prev = null;
            //     for (int k = 0; k < gene.iterations; k++)
            //     {
            //         float a = (float)k / (gene.iterations) * Mathf.PI;
            //         Vector3 source = new Vector3(Mathf.Sin(a), Mathf.Cos(Mathf.Sin(a)), -Mathf.Sin(a * gene.shift)) + gene.offset + creature.offset;
            //         Gizmos.color = Color.green;
            //         Gizmos.DrawSphere(source, 0.01f);
            //         if (prev.HasValue)
            //         {
            //             Gizmos.DrawLine(source, prev.Value);
            //         }
            //         prev = source;
            //     }
            //
            //     for (int i = 0; i < gene.iterations; i++)
            //     {
            //         float a = (float)i / (gene.iterations) * Mathf.PI;
            //         Vector3 sourceA =
            //             new Vector3(Mathf.Sin(a), Mathf.Cos(Mathf.Sin(a)), -Mathf.Sin(a * gene.shift)) +
            //             gene.offset + creature.offset;
            //         for (int k = 0; k < gene.iterations; k++)
            //         {
            //             float b = (float)k / (gene.iterations) * Mathf.PI;
            //             Vector3 sourceB =
            //                 new Vector3(Mathf.Sin(b), Mathf.Cos(Mathf.Sin(b)), -Mathf.Sin(b * gene.shift)) +
            //                 gene.offset + creature.offset;
            //             Gizmos.DrawLine(sourceA, sourceB);
            //             
            //         }
            //     }
            // }
          
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