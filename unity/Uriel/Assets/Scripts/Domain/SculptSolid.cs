using System.Runtime.InteropServices;
using UnityEngine;

namespace Uriel.Domain
{
    public enum SculptSolidType
    {
        Box = 0,
        Sphere = 1,
        Tetrahedron = 2
    }
    
    public enum SculptOperation
    {
        Add = 0,
        Subtract = 1,
        Intersect = 2
    }
    
    [StructLayout(LayoutKind.Sequential)]
    [System.Serializable]
    public struct SculptSolid
    {
        public Matrix4x4 invTransform;   // inverse transform to local space
        public float    scale;          // used for signed distance control
        public SculptSolidType      type;           // shape ID: 0 = box, 1 = sphere, 2 = tetra, etc.
        public SculptOperation      op;             // blend op: 0 = add, 1 = subtract, 2 = intersect
        public float    feather;        // for smooth blending
    }
}