using System;
using UnityEngine;
using Uriel.Domain;

namespace Uriel.Behaviours
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(PhotonBuffer))]
    [ExecuteInEditMode]
    public class FractalEcho : MonoBehaviour
    {
        [SerializeField] private Transform source;
        
        private Material mat;
        
        private void Start()
        {
            mat = GetComponent<MeshRenderer>().sharedMaterial;
        }

        private void OnDrawGizmos()
        {
            Vector2 p = source.position;
            float x = 0;
            float y = 0;
            int step = 0;
            Vector3 prev1 = source.position;
            Vector3 prev2 = source.position;
            Vector3 prev3 = source.position;
            Vector3 prev4 = source.position;
            while (step < 20)
            {
                float x_temp = x * x - y * y + p.x;
                y = 2 * x * y + p.y;
                x = x_temp;
                step++;
                Vector3 a = new Vector3(x, y, 0);
                Gizmos.DrawLine(prev1, a);
                prev1 = a;

                Vector3 b = new Vector3(x, -y, 0);
                Gizmos.DrawLine(prev2, b);
                prev2 = b;

                Vector3 c = new Vector3(x, 0, y);
                Gizmos.DrawLine(prev3, c);
                prev3 = c;

                Vector3 d = new Vector3(x, 0, -y);
                Gizmos.DrawLine(prev4, d);
                prev4 = d;
            }

        }

        private void Update()  
        {
            if (source == null)
            {
                return;
            }
            mat.SetVector(ShaderProps.OrbitOrigin, source.position);
        }
    }
}