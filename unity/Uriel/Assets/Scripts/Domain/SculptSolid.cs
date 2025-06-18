using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Uriel.Domain
{
    public enum SculptSolidType
    {
        Box = 0,
        Sphere = 1,
        Cylinder = 2,
        Capsule = 3
    }
    
    public enum SculptOperation
    {
        Add = 0,
        Multiply = 1,
        Intersect = 2,
        Interpolate = 3,
        Cut = 4
    }
    
    [StructLayout(LayoutKind.Sequential)]
    [Serializable]
    public struct SculptSolid
    {
        [HideInInspector] public Matrix4x4 invTransform; 
        public float scale;
        public SculptSolidType type;  
        public SculptOperation op;
        [Range(0f, 0.1f)] public float feather;
        public float exp;
        public float lerp;
        public int priority;

        public static SculptSolid Default => new()
        {
            scale = 1f,
            feather = 0.1f
        };

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public bool Equals(SculptSolid other)
        {
            return invTransform.Equals(other.invTransform) && scale.Equals(other.scale) 
                                                           && type == other.type 
                   && op == other.op && feather.Equals(other.feather) && exp.Equals(other.exp) 
                   && lerp.Equals(other.lerp) && priority == other.priority;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(invTransform, scale, (int) type, (int) op, feather, exp, lerp, priority);
        }
    }
    
    [Serializable]
    public struct SculptSolidState
    {
        public SculptSolid solid;
        public Vector3 position;
        public Vector3 scale;
        public Vector3 rotation;
    }
}