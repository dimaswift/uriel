using System;
using System.Collections.Generic;
using UnityEngine;
using Uriel.Domain;
using Uriel.Utils;

namespace Uriel.Behaviours
{
    public class Constellation : MonoBehaviour
    {
        [SerializeField] private float scale = 1.0f;
        [SerializeField] private float frequency = 1.0f;
        [Range(0f, 0.1f)][SerializeField] private float scaleFine = 1.0f;
        [Range(0f, 0.1f)] [SerializeField] private float frequencyFine = 1.0f;
        [SerializeField] private Vector2 uv;
        [SerializeField] private PlatonicSolids.Type type;
        [SerializeField] private PlatonicSolids.Mode mode;
        private readonly List<Star> stars = new();

        
        private void Awake()
        {
            Generate();
        }

        private void Generate()
        {
            var vertices = new List<Vector3>();
            PlatonicSolids.GenerateVertices(vertices, type, mode, uv);
            foreach (var star in transform.GetComponentsInChildren<Star>())
            {
                Destroy(star.gameObject);
            }
            for (int i = 0; i < vertices.Count; i++)
            {
                var t = new GameObject(i.ToString());
                t.AddComponent<Star>();
                t.transform.SetParent(transform);
                t.transform.localPosition = vertices[i];
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            foreach (Star a in stars)
            {
                foreach (Star b in stars)
                {
                    Gizmos.DrawLine(a.transform.position, b.transform.position);
                }
            }
        }

        public void FillGeneBuffer(List<Gene> genes)
        {
            GetComponentsInChildren(stars);
            foreach (Star star in stars)
            {
                var gene = star.GetGene();
                gene.frequency *= frequency + frequencyFine;
                gene.scale *= scale + scaleFine;
                genes.Add(gene);
            }
        }
    }
}