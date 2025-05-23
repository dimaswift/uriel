using UnityEngine;

namespace Uriel.Utils
{
    public static class Extensions
    {
        public static Vector3Int GetGroups(this ComputeShader compute, int size, int kernel)
        {
            compute.GetKernelThreadGroupSizes(kernel, out var x, out var y, out var z);
            return new Vector3Int(
                Mathf.CeilToInt((float) size / x),
                Mathf.CeilToInt((float) size / y),
                Mathf.CeilToInt((float) size / z));
        }
    }
}