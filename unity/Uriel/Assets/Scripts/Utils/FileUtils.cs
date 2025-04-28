using System;
using System.IO;
using UnityEngine;

namespace Uriel.Utils
{
    public static class FileUtils
    {
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