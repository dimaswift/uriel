using UnityEngine;

namespace Uriel.Domain
{
    [System.Serializable]
    public struct FieldConfig
    {
        public bool saturate;
        public static FieldConfig Default => new FieldConfig()
        {
            saturate = true
        };
    }
}