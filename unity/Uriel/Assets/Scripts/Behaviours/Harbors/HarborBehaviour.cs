using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Uriel.Domain;

namespace Uriel.Behaviours
{
    public class HarborBehaviour : MonoBehaviour
    {
        public event Action<ComputeBuffer> OnBufferCreated = b => { };
        public int WaveCount => harbor.waves.Count;
        
        [SerializeField] private Spiral spiral;
        [SerializeField] private Harbor harbor;
        [SerializeField] private bool useSpiral;
        
        private ComputeBuffer waveBuffer;
        private readonly List<Constellation> constellations = new();

        private void OnValidate()
        {
            if (spiral == null)
            {
                spiral = FindFirstObjectByType<Spiral>();
            }
        }

        public ComputeBuffer GetWaveBuffer()
        {
            if (waveBuffer == null || waveBuffer.count != harbor.waves.Count)
            {
                if (waveBuffer != null) waveBuffer.Release();
                waveBuffer = new ComputeBuffer(harbor.waves.Count, Marshal.SizeOf(typeof(Wave)));
                OnBufferCreated(waveBuffer);
            }
            return waveBuffer;
        }
        
        private void FillWavesFromSpiral()
        {
            if (spiral == null)
            {
                return;
            }

            spiral.GetComponentsInChildren(constellations);
            
            if (constellations.Count == 0)
            {
                return;
            }

            harbor.waves.Clear();

            foreach (Constellation constellation in constellations)
            {
                constellation.FillWaveBuffer(harbor.waves);
            }

        }
        
        public void UpdateWaveBuffer()
        {
            if (harbor == null)
            {
                return;
            }
            
            if (useSpiral)
            {
                FillWavesFromSpiral();
            }
            
            if (harbor.waves.Count == 0)
            {
                return;
            }

            GetWaveBuffer().SetData(harbor.waves);
        }
        
        private void OnDestroy()
        {
            if (waveBuffer != null) waveBuffer.Release();
   
        }
    }
    
}