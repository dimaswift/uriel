using System;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

namespace Uriel.Utils
{
    public static class FileUtils
    {
        public static void ExportMeshToASCIISTL(Mesh mesh, string filePath, string name = "Uriel")
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"solid {name}");
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            CultureInfo invariantCulture = CultureInfo.InvariantCulture;

            for (int i = 0; i < triangles.Length; i += 3)
            {
                Vector3 v1 = vertices[triangles[i]];
                Vector3 v2 = vertices[triangles[i + 1]];
                Vector3 v3 = vertices[triangles[i + 2]];
                Vector3 normal = Vector3.Cross(v2 - v1, v3 - v1).normalized;

                sb.AppendLine(string.Format(invariantCulture, "    facet normal {0} {1} {2}", normal.x, normal.y,
                    normal.z));
                sb.AppendLine("         outer loop");
                sb.AppendLine(string.Format(invariantCulture, "            vertex {0} {1} {2}", v1.x, v1.y, v1.z));
                sb.AppendLine(string.Format(invariantCulture, "            vertex {0} {1} {2}", v2.x, v2.y, v2.z));
                sb.AppendLine(string.Format(invariantCulture, "            vertex {0} {1} {2}", v3.x, v3.y, v3.z));
                sb.AppendLine("         endloop");
                sb.AppendLine("     endfacet");
            }
            sb.AppendLine($"endsolid {name}");
        
            File.WriteAllText(filePath + ".stl", sb.ToString());
        }  
        
         public static void SaveTextureAsPNG(RenderTexture texture, string saveDirectory, string filePrefix)  
        {
            if (texture == null)  
                return;  
                
            try  
            {
                RenderTexture tempRT = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.ARGB32);  
                Graphics.Blit(texture, tempRT);
                Texture2D tex2D = new Texture2D(tempRT.width, tempRT.height, TextureFormat.RGBA32, false);  
                RenderTexture.active = tempRT;  
                tex2D.ReadPixels(new Rect(0, 0, tempRT.width, tempRT.height), 0, 0);  
                tex2D.Apply();  
                RenderTexture.active = null;  
                RenderTexture.ReleaseTemporary(tempRT);
                string directory = Path.Combine(Application.dataPath, saveDirectory);  
                if (!Directory.Exists(directory))  
                {  
                    Directory.CreateDirectory(directory);  
                }
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");  
                string filename = $"{filePrefix}_{timestamp}.png";  
                string filePath = Path.Combine(directory, filename);
                byte[] pngBytes = tex2D.EncodeToPNG();  
                File.WriteAllBytes(filePath, pngBytes);
                Debug.Log($"Saved texture to: {filePath}");
            }  
            catch (Exception e)  
            {  
                Debug.LogError($"Error saving texture: {e.Message}");  
            }  
        }  
    }
}