using System;
using UnityEngine;
using Uriel.Domain;

namespace Uriel.Behaviours
{
    public class Star : MonoBehaviour
    {
        [SerializeField] private int speed = 0;
        [SerializeField] private int operation;
        [SerializeField] private int phase;
        [SerializeField] private int shift;
        [SerializeField] private Gene gene = new()
        {
            scale = 1,
            frequency = 25,
            amplitude = 0.36f,
            iterations = 1
        };

        private float timer;
        
        public Gene GetGene()
        {
            gene.offset = transform.position;
            gene.operation = operation;
            gene.phase = phase;
            gene.shift = shift;
            return gene;
        }

        private void Update()
        {
            timer += speed * Time.deltaTime;
            if (timer >= 1)
            {
                timer = 0;
                phase++;
            }
        }
    }
}
