using System;
using UnityEngine;
using Uriel.Domain;
using Random = UnityEngine.Random;

namespace Uriel.Behaviours
{
    public class Star : MonoBehaviour
    {
        public float noise;
        public float speed = 0.01f;
        public int frequencyScale = 5;

        private Star parent;
        private Vector3 offset;
        
        public Gene gene = new Gene()
        {
            scale = 1,
            frequency = 25,
            amplitude = 0.36f,
            iterations = 1
        };

        private void Update()
        {
            if (!parent)
            {
                return;
            }

            

        }

        private void OnEnable()
        {
         
        }

        public void SetUp(Star parent)
        {
            this.parent = parent;
        }
        public Gene GetGene()
        {
          
            gene.offset = transform.position + Random.insideUnitSphere * noise;
            return gene;
        }
    }

}
