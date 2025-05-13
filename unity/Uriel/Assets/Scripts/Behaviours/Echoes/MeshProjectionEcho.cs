using System;
using System.Runtime.InteropServices;
using UnityEngine;
using Uriel.Domain;

namespace Uriel.Behaviours
{
    public class MeshProjectionEcho : MonoBehaviour
    {
        [SerializeField] private MeshEcho meshEcho;
        [SerializeField] private Material material;
        
        private void Start()
        {
            var buff = meshEcho.GetOutputVertexBuffer();
            material.SetBuffer("_VertexBuffer", buff);
            material.SetBuffer("_NormalBuffer", meshEcho.GetNormalBuffer());
            material.SetInt("_VertexCount", buff.count);
            gameObject.AddComponent<PhotonBuffer>().LinkMaterial(material);
        }
    }
}
