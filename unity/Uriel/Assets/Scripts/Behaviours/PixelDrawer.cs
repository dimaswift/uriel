using UnityEngine;

namespace Uriel.Behaviours
{
    public class PixelDrawer : MonoBehaviour
    {
        private Material material;
        private int count;

        public static PixelDrawer Create(ComputeBuffer buffer)
        {
            var go = new GameObject("PixelDrawer");
            go.AddComponent<PixelDrawer>().Init(buffer);
            return go.GetComponent<PixelDrawer>();
        }

        private void Init(ComputeBuffer buffer)
        {
            material = new Material(Shader.Find($"Uriel/Pixel"));
            material.SetBuffer("Particles", buffer);
            count = buffer.count;
        }

        protected void OnRenderObject()
        {
            if (!material)
            {
                return;
            }
            material.SetPass(0);
            Graphics.DrawProceduralNow(MeshTopology.Points, 4, count);
        }

        private void OnDestroy()
        {
            Destroy(material);
            material = null;
        }
    }

}