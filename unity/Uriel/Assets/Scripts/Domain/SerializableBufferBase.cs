using UnityEngine;

namespace Uriel.Domain
{
    public abstract class SerializableBufferBase : ScriptableObject
    {
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

        public abstract int GetBufferHash();

        public abstract bool CreateBuffer();

        protected abstract string GetName();

        public abstract void Update();
        
        
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
            
            material.SetBuffer(BufferId, buffer);
            material.SetInt(CountId, buffer.count);
        }

        public void LinkComputeKernel(ComputeShader computeShader, int kernelIndex = 0)
        {
            if (buffer == null)
            {
                if (!CreateBuffer()) return;
            }
            
            computeShader.SetBuffer(kernelIndex, BufferId, buffer);
            computeShader.SetInt(CountId, buffer.count);

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