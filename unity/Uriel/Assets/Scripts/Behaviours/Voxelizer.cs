using UnityEngine;

namespace Uriel.Behaviours
{
    public class Voxelizer : MonoBehaviour
    {
      
        [SerializeField] private Material material;
        [SerializeField] private Mesh mesh;
        [SerializeField] private int resolution = 32;
        private ComputeBuffer meshBuffer;


        private int ResolutionCubed => resolution * resolution * resolution;

        private void Start()
        {
            uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
            args[0] = mesh.GetIndexCount(0);
            args[1] = (uint)ResolutionCubed;
            meshBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
            meshBuffer.SetData(args);
        }

       
        private void Update()
        {
            Graphics.DrawMeshInstancedIndirect(mesh, 0, material,
                new Bounds(transform.position, Vector3.one * (float.MaxValue)), meshBuffer);
        }
    }

}
