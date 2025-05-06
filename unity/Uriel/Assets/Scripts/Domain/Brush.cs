using System.Runtime.InteropServices;
using UnityEngine;

namespace Uriel.Domain
{
    [StructLayout(LayoutKind.Sequential)]
    [System.Serializable]
    public struct Brush
    {
        public uint amount;
        public Vector3 position;
        public uint particleIndex;
    }
}