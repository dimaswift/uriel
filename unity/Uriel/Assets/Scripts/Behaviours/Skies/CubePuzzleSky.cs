using System;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Uriel.Domain;
using Uriel.Utils;

namespace Uriel.Behaviours
{
    public class CubePuzzleSky : MonoBehaviour
    {
        [SerializeField] private float scale = 1.0f;
        [SerializeField] private float amplitude = 1.0f;
        [SerializeField] private Mesh mesh;
        [SerializeField] private ComputeShader compute;
        [SerializeField] private Lumen lumen;
        [SerializeField] private int resolution = 128;
        [SerializeField] private MeshRenderer side;

        private ComputeBuffer vertexBuffer, normalBuffer, photonBuffer;

        private RenderTexture texture;
        
        private void Start()
        {
            Vector3[] vertices = mesh.vertices;
            Vector3[] normals = mesh.normals;

            vertexBuffer = new ComputeBuffer(vertices.Length, Marshal.SizeOf(typeof(Vector3)));
            normalBuffer = new ComputeBuffer(normals.Length, Marshal.SizeOf(typeof(Vector3)));
            photonBuffer = new ComputeBuffer(lumen.photons.Count, Marshal.SizeOf(typeof(Photon)));
            vertexBuffer.SetData(vertices);
            normalBuffer.SetData(normals);
            photonBuffer.SetData(lumen.photons);

            compute.SetInt(ShaderProps.PhotonCount, lumen.photons.Count);
            compute.SetBuffer(0, ShaderProps.PhotonBuffer, photonBuffer);
            compute.SetBuffer(0, "_VertexBuffer", vertexBuffer);
            compute.SetBuffer(0, "_NormalBuffer", normalBuffer);
            compute.SetInt(ShaderProps.Resolution, resolution);
            compute.SetInt("_VertexCount", vertices.Length);
            
            Transform s = side.transform;
         
            texture = new RenderTexture(resolution, resolution, 0, GraphicsFormat.R32G32B32A32_SFloat)
            {
                enableRandomWrite = true,
                useMipMap = false,
                autoGenerateMips = false,
                filterMode = FilterMode.Point
            };
          
            texture.Create();
            side.GetComponent<MeshRenderer>().material.SetTexture(ShaderProps.BaseMap, texture);

            compute.SetTexture(0, "_Texture", texture);
        }

        private void Update()
        {
            photonBuffer.SetData(lumen.photons);
            compute.SetVector("_Offset", side.transform.position);
            compute.SetFloat(ShaderProps.Scale, scale);
            compute.SetFloat(ShaderProps.Amplitude, amplitude);
            compute.SetMatrix("_Matrix", transform.localToWorldMatrix);
            compute.Dispatch(0, Mathf.CeilToInt(resolution / 8f), Mathf.CeilToInt(resolution / 8f), 1);

            if (Input.GetKeyDown(KeyCode.Space))
            {
                FileUtils.SaveTextureAsPNG(texture, "C:/Unity/uriel/unity/Uriel/Generated/Projections/", Guid.NewGuid().ToString().Substring(0, 5).ToUpper());
            }
        }

        private void OnDestroy()
        {
            if (texture) texture.Release();
            vertexBuffer?.Release();
            normalBuffer?.Release();
            photonBuffer?.Release();
        }
    }
}