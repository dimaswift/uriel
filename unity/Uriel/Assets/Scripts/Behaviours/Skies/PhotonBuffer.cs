#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

using System;
using UnityEngine;
using Uriel.Domain;

namespace Uriel.Behaviours
{
    [ExecuteInEditMode]
    public class PhotonBuffer : MonoBehaviour
    {
        public Lumen Lumen => lumen;
        
        [SerializeField] private Transform source;
        [SerializeField] private Lumen lumen;

        private void Init()
        {
            if (lumen == null) return;
            var rend = GetComponent<MeshRenderer>();
            if (rend)
            {
                lumen.EnsureBufferExists();
                LinkMaterial(rend.sharedMaterial);
            }
        }
        
        private void Start()
        {
            Init();
        }

#if UNITY_EDITOR
        // Register for callbacks when the script is enabled
        private void OnEnable()
        {
            // Subscribe to domain reload events
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
            // Subscribe to scene reload/change events
            EditorApplication.playModeStateChanged += PlayModeStateChanged;
            EditorSceneManager.sceneOpened += SceneOpened;
            EditorSceneManager.sceneSaved += SceneSaved;
            EditorSceneManager.sceneClosed += SceneClosed;
        }

        private void OnValidate()
        {
            Init();
        }

        private void OnDisable()
        {
            // Unsubscribe to prevent memory leaks
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;
            
            EditorApplication.playModeStateChanged -= PlayModeStateChanged;
            EditorSceneManager.sceneOpened -= SceneOpened;
            EditorSceneManager.sceneSaved -= SceneSaved;
            EditorSceneManager.sceneClosed -= SceneClosed;

            DisposeBuffer(); 
        }

        private void OnBeforeAssemblyReload()
        {
            DisposeBuffer(); 
        }

        private void OnAfterAssemblyReload()
        {
            if(lumen) lumen.EnsureBufferExists();
        }

        private void PlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode ||
                state == PlayModeStateChange.ExitingPlayMode)
            {
                DisposeBuffer();
            }
        }

        private void SceneOpened(UnityEngine.SceneManagement.Scene scene, OpenSceneMode mode)
        {
            DisposeBuffer();
        }

        private void SceneSaved(UnityEngine.SceneManagement.Scene scene)
        {
            DisposeBuffer();
        }

        private void SceneClosed(UnityEngine.SceneManagement.Scene scene)
        {
            DisposeBuffer();
        }
#endif

        private void DisposeBuffer()
        {
            if (lumen != null) lumen.DisposeBuffer();
        }

        public PhotonBuffer LinkComputeKernel(ComputeShader shader, int id = 0)
        {
            if (lumen == null) return this;
            lumen.LinkComputeKernel(shader, id);
            return this;
        }

        public PhotonBuffer LinkMaterial(Material mat)
        {
            if (lumen == null) return this;
            lumen.LinkMaterial(mat);
            return this;
        }

        private void Update()
        {
            if (lumen == null)
            {
                return;
            }

            if (source)
            {
                lumen.UpdateTransform(source.localToWorldMatrix);
            }
            
            #if UNITY_EDITOR
            
            OnValidate();
            
            #endif
            
            lumen.Update();
        }
    }
}