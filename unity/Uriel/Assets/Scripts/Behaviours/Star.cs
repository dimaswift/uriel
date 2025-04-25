using UnityEngine;
using Uriel.Domain;

namespace Uriel.Behaviours
{
    public class Star : MonoBehaviour
    {
        [SerializeField] private int speed = 0;
        [SerializeField] private int phase;
        [SerializeField] private uint iterations = 1;
        [SerializeField] private Gene gene = new()
        {
            scale = 1,
            frequency = 25,
            amplitude = 0.36f,
        };

        private float timer;
        
        public Gene GetGene()
        {
            gene.source = transform.position;
            gene.phase = phase;
            gene.iterations = iterations;
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
