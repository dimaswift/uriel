using System;
using UnityEngine;

namespace Uriel.Utils
{
    public static class MathUtils
    {
        public static float Map(
            float value,
            float inMin, float inMax,
            float outMin, float outMax)
        {
            return (value - inMin)
                   / (inMax - inMin)
                   * (outMax - outMin)
                   + outMin;
        }
    }
}