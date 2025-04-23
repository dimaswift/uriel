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
        [SerializeField] private Material sourceMat;
        [SerializeField] private float step = 1;
        [SerializeField] private bool worldSpace;
        [SerializeField] private float height = 100;
        [SerializeField] private int sides = 3;
        [SerializeField] private PolyhedronGenerator.PolyhedronType fieldType;
        [SerializeField] private float radius = 1;
        [SerializeField] private float amplitude = 10;
        [SerializeField] private int frequency;
        [SerializeField] private float scale = 10;
        [Range(0f, 0.01f)][SerializeField] private float scaleFine = 10;
        [Range(0, 5)] [SerializeField] private int operation;
        [SerializeField] private Creature creature;

        private Material mat;

        private ComputeBuffer geneBuffer;
     
        private int geneStride;
        private List<Star> stars = new();
        private Transform starsContainer;

        private void Awake()
        {
            mat = Instantiate(sourceMat);
            GetComponent<Renderer>().sharedMaterial = mat;
            starsContainer = new GameObject("Stars").transform;
            InitializeGeneBuffer();
            fieldType = PolyhedronGenerator.PolyhedronType.Icosahedron;
            InitializeGeneBuffer();
            fieldType = PolyhedronGenerator.PolyhedronType.Dodecahedron;
        }

        private void InitializeGeneBuffer()
        {
            if (geneBuffer != null) geneBuffer.Release();
            geneStride = Marshal.SizeOf(typeof(Gene));
            
            var vertices = new List<Vector3>();
            PolyhedronGenerator.GenerateVertices(vertices, fieldType, sides, radius, height);
            var sigil = new GameObject(fieldType.ToString()).transform;
            sigil.transform.SetParent(starsContainer);
            Star prevStar = null;
            for (int i = 0; i < vertices.Count; i++)
            {
                var t = new GameObject(i.ToString());
                var s = t.AddComponent<Star>();
                s.SetUp(prevStar);
                prevStar = s;
                t.transform.SetParent(sigil.transform);
                t.transform.localPosition = vertices[i];
            }
        }

        private void UpdateGeneBuffer()
        {
            if (creature == null)
            {
                return;
            }

            stars.Clear();
            starsContainer.GetComponentsInChildren(stars);
            if (stars.Count == 0)
            {
                return;
            }
            if (creature.genes.Length != stars.Count || geneBuffer == null)
            {
                creature.genes = new Gene[stars.Count];
                geneBuffer = new ComputeBuffer(stars.Count, geneStride);
                mat.SetBuffer("_GeneBuffer", geneBuffer);
                mat.SetInt("_GeneCount", stars.Count);
            }
            
            for (int i = 0; i < stars.Count; i++)
            {
                var star = stars[i];
                creature.genes[i] = star.GetGene();
            }
            geneBuffer.SetData(creature.genes);
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

            mat.SetFloat("_Scale", scale);
            mat.SetFloat("_Frequency", frequency);
            mat.SetFloat("_Amplitude", amplitude);
            mat.SetInt("_Sides", sides);
            mat.SetFloat("_Radius", radius);
            mat.SetFloat("_Height", height);
            mat.SetFloat("_Step", step);
            mat.SetVector("_Offset", transform.position);
            mat.SetMatrix("_Shape",transform.localToWorldMatrix);
            UpdateGeneBuffer();
            if (Input.GetKeyDown(KeyCode.Space))
            {
                InitializeGeneBuffer();
            }

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