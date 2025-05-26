using System;
using System.Collections.Generic;
using UnityEngine;

namespace Uriel.Domain
{
    public abstract class SerializableBufferBase : ScriptableObject
    {
        private readonly HashSet<int> linked = new();

        public event Action<int> OnBufferCreated = (s) => { };
        
        private int? countId;

        private int CountId
        {
            get
            {
                countId ??= Shader.PropertyToID($"_{GetName()}Count");
                return countId.Value;
            }
        }

        private int? bufferId;

        private int BufferId
        {
            get
            {
                bufferId ??= Shader.PropertyToID($"_{GetName()}Buffer");
                return bufferId.Value;
            }
        }
        
        protected ComputeBuffer buffer;
        protected virtual void OnBeforeUpdate()
        {
        }

        public abstract bool CreateBuffer();

        protected abstract string GetName();

        public abstract void Update();

        protected void CreateBuffer(int size)
        {
            OnBufferCreated(size);
        }
        
        public void DisposeBuffer()
        {
            if (buffer != null)
            {
                buffer.Release();
                buffer = null;
            }
        }
        
        public void LinkMaterial(Material material)
        {
            if (buffer == null)
            {
                if (!CreateBuffer()) return;
            }

            if (linked.Contains(material.GetInstanceID()))
            {
                return;
            }

            linked.Add(material.GetInstanceID());
            
            material.SetBuffer(BufferId, buffer);
            material.SetInt(CountId, buffer.count);

            OnBufferCreated += s =>
            {
                material.SetBuffer(BufferId, buffer);
                material.SetInt(CountId, buffer.count);
            };
        }

        public void LinkComputeKernel(ComputeShader computeShader, int kernelIndex = 0)
        {
            if (buffer == null)
            {
                if (!CreateBuffer()) return;
            }

            var id = computeShader.GetInstanceID() + kernelIndex;
            if (linked.Contains(id))
            {
                return;
            }

            linked.Add(id);
            
            computeShader.SetBuffer(kernelIndex, BufferId, buffer);
            computeShader.SetInt(CountId, buffer.count);

            OnBufferCreated += s =>
            {
                computeShader.SetBuffer(kernelIndex, BufferId, buffer);
                computeShader.SetInt(CountId, buffer.count);
            };
        }


        public abstract void EnsureBufferExists();

        private void OnDisable()
        {
            DisposeBuffer();
        }

        private void OnDestroy()
        {
            DisposeBuffer();
        }
    }
}