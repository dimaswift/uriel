using UnityEngine;
using Uriel.Domain;

namespace Uriel.Behaviours
{
    public class Star : MonoBehaviour
    {
        [SerializeField] private int speed;
        [SerializeField] private int phase;
        [SerializeField] private uint iterations = 0;
        [SerializeField] private Wave wave = new()
        {
            density = 1,
            frequency = 5,
            amplitude = 0.36f
        };

        private float timer;
        
        public Wave GetWave()
        {
            wave.source = transform.position;
            wave.phase = phase;
            wave.ripples = iterations;
            return wave;
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
