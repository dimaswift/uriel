using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Uriel.Domain
{
    public abstract class SerializableBuffer<T> : SerializableBufferBase where T : struct
    {
        protected override string GetName() => typeof(T).Name;
        protected abstract List<T> GetData();
        
        public override void Update()
        {
            if (buffer == null)
            {
                if (!CreateBuffer()) return;
            }

            OnBeforeUpdate();
            
            var data = GetData();
            
            if (buffer.count != data.Count)
            {
                if (!CreateBuffer()) return;
            }
            
            buffer.SetData(data);
        }

        public override void EnsureBufferExists()
        {
            if (buffer != null) return;
            var data = GetData();
            buffer = new ComputeBuffer(data.Count, Marshal.SizeOf(typeof(T)));
            buffer.SetData(data);
        }
        
        public override bool CreateBuffer()
        {
            DisposeBuffer();
            var data = GetData();
            if (data.Count == 0)
            {
                return false;
            }
            
            buffer = new ComputeBuffer(data.Count, Marshal.SizeOf(typeof(T)));
            buffer.SetData(data);
            
            return true;
        }
    }
}