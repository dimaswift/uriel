using System;
using System.Runtime.InteropServices;
using UnityEngine;
using Uriel.Domain;

namespace Uriel.Behaviours
{
    public class MeshProjectionSky : MonoBehaviour
    {
        [SerializeField] private MeshSky meshSky;
        [SerializeField] private Sky sky;
        [SerializeField] private Material material;
        
    
        private void Start()
        {
            var buff = meshSky.GetOutputVertexBuffer();
            material.SetBuffer("_VertexBuffer", buff);
            material.SetBuffer("_NormalBuffer", meshSky.GetNormalBuffer());
            material.SetInt("_VertexCount", buff.count);
            gameObject.AddComponent<PhotonBuffer>().Init(sky).LinkMaterial(material);
        }

    }
}
