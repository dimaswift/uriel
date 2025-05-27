using System;
using UnityEditor;
using UnityEngine;

namespace Uriel.Utils
{
    public static class MeshSaveUtility
    {
        /// <summary>
        /// Saves a mesh asset with automatic GUID preservation if overwriting
        /// </summary>
        public static Mesh SaveMeshAsset(Mesh mesh, string defaultName = "ProceduralMesh")
        {
            // Opens a file save dialog window
            string path = EditorUtility.SaveFilePanel("Save Mesh Asset", "Assets/Meshes", defaultName, "asset");
            
            // Path is empty if the user exits out of the window
            if(string.IsNullOrEmpty(path)) 
            {
                Debug.Log("Save cancelled");
                return null;
            }

            // Transforms the path to a project relative path
            path = FileUtil.GetProjectRelativePath(path);
            
            if(string.IsNullOrEmpty(path))
            {
                Debug.LogError("Path must be within the project Assets folder");
                return null;
            }

            // Check if this path already contains a mesh
            var oldMesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(oldMesh != null) 
            {
                // Clear all mesh data on the old mesh, readying it to receive new data
                oldMesh.Clear();
                // Copy mesh data from the new mesh to the old mesh
                EditorUtility.CopySerialized(mesh, oldMesh);
                Debug.Log($"Updated existing mesh at: {path}");
            } 
            else 
            {
                // Nothing is at this path (or it wasn't a mesh), so create a new asset
                AssetDatabase.CreateAsset(mesh, path);
                Debug.Log($"Created new mesh at: {path}");
            }

            // Tell Unity to save all assets
            AssetDatabase.SaveAssets();
            
            // Ping the asset in the project window
            var result = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            EditorGUIUtility.PingObject(result);
            return result;
        }
        
        /// <summary>
        /// Saves mesh with metadata about generation parameters
        /// </summary>
        public static void SaveMeshWithMetadata(Mesh mesh, string parameters, string defaultName = "ProceduralMesh")
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string meshName = $"{defaultName}_{timestamp}";
            
            // Save the mesh
            SaveMeshAsset(mesh, meshName);
            
            // Save metadata alongside
            string metadataPath = $"Assets/Meshes/{meshName}_metadata.txt";
            System.IO.File.WriteAllText(metadataPath, parameters);
            AssetDatabase.Refresh();
        }
    }
}