using System;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Uriel.Utils
{
    public static class FileUtils
    {
        public static void MeshToObjFile(Mesh mesh, string filePath, bool includeUVs = true, bool includeNormals = true)
        {
            StringBuilder sb = new StringBuilder();
            
            sb.AppendLine("# Unity Mesh to OBJ Exporter");
            sb.AppendLine("# Mesh: " + mesh.name);
            
            // Write vertices
            Vector3[] vertices = mesh.vertices;
            foreach (Vector3 v in vertices)
            {
                sb.AppendLine("v " + v.x + " " + v.y + " " + v.z);
            }
            
            // Write UVs
            if (includeUVs && mesh.uv.Length > 0)
            {
                Vector2[] uvs = mesh.uv;
                foreach (Vector2 uv in uvs)
                {
                    sb.AppendLine("vt " + uv.x + " " + uv.y);
                }
            }
            
            // Write normals
            if (includeNormals && mesh.normals.Length > 0)
            {
                Vector3[] normals = mesh.normals;
                foreach (Vector3 n in normals)
                {
                    sb.AppendLine("vn " + n.x + " " + n.y + " " + n.z);
                }
            }
            
            // Write triangles
            int[] triangles = mesh.triangles;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int idx1 = triangles[i] + 1;    // OBJ format uses 1-based indices
                int idx2 = triangles[i + 1] + 1;
                int idx3 = triangles[i + 2] + 1;
                
                // Format depends on what data is available
                if (includeUVs && includeNormals && mesh.uv.Length > 0 && mesh.normals.Length > 0)
                {
                    sb.AppendLine("f " + idx1 + "/" + idx1 + "/" + idx1 + " " + 
                                        idx2 + "/" + idx2 + "/" + idx2 + " " + 
                                        idx3 + "/" + idx3 + "/" + idx3);
                }
                else if (includeUVs && mesh.uv.Length > 0)
                {
                    sb.AppendLine("f " + idx1 + "/" + idx1 + " " + 
                                        idx2 + "/" + idx2 + " " + 
                                        idx3 + "/" + idx3);
                }
                else if (includeNormals && mesh.normals.Length > 0)
                {
                    sb.AppendLine("f " + idx1 + "//" + idx1 + " " + 
                                        idx2 + "//" + idx2 + " " + 
                                        idx3 + "//" + idx3);
                }
                else
                {
                    sb.AppendLine("f " + idx1 + " " + idx2 + " " + idx3);
                }
            }
            
            // Write to file
            File.WriteAllText(filePath, sb.ToString());
            Debug.Log("Mesh exported to: " + filePath);
        }
    
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
                RenderTexture tempRT = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.ARGB64);  
                Graphics.Blit(texture, tempRT);
                Texture2D tex2D = new Texture2D(tempRT.width, tempRT.height, TextureFormat.RGBA64_SIGNED, false);  
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
                Object.Destroy(tex2D);
            }  
            catch (Exception e)  
            {  
                Debug.LogError($"Error saving texture: {e.Message}");  
            }  
        }  
    }
}