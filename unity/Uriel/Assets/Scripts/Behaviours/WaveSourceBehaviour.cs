using UnityEngine;
using Uriel.Domain;

namespace Uriel.Behaviours
{
    public class WaveSourceBehaviour : MonoBehaviour
    {
        [SerializeField] private WaveSource config = WaveSource.Default;
        public WaveSource GetWaveSource()
        {
            config.position = transform.localPosition;
            return config;
        }
        
        public void SetWaveSource(WaveSource source)
        {
            transform.localPosition = source.position;
            config = source;
        }
    }
}