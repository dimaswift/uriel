using System.Collections.Generic;
using UnityEngine;
using Uriel.Domain;
using Uriel.Utils;
using Random = UnityEngine.Random;

namespace Uriel.Behaviours
{
    public class Constellation : MonoBehaviour
    {
        public int depth;
        [SerializeField] private uint harmonic = 2;
        [SerializeField] private bool updateEachFrame;
        [SerializeField] private uint ripples = 0;
        [Range(0f, 2f)] [SerializeField] private float density = 1.0f;
        [SerializeField] private float frequency = 1.0f;
        [SerializeField] private float amplitude = 1.0f;
        [SerializeField] private float densityFine = 1.0f;
        [SerializeField] private float frequencyFine = 1.0f;
        [SerializeField] private Vector2 uv;
        [SerializeField] private PlatonicSolids.Type type;
        [SerializeField] private PlatonicSolids.Mode mode;
        private readonly List<Star> stars = new();

      
        private void Awake()
        {
            Generate();
        }

        private void FixedUpdate()
        {
            if (updateEachFrame)
            {
                Generate();
            }
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

        public void FillWaveBuffer(List<Wave> waves)
        {
            GetComponentsInChildren(stars);
            foreach (Star star in stars)
            {
                var wave = star.GetWave();
                wave.frequency *= frequency + frequencyFine;
                wave.density *= density + densityFine;
                wave.ripples += ripples;
                wave.harmonic += harmonic;
                wave.amplitude *= amplitude;
                waves.Add(wave);
            }
        }
    }
}