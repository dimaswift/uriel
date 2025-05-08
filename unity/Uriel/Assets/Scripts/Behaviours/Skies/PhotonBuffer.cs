using UnityEditor;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

using UnityEngine;
using Uriel.Domain;

namespace Uriel.Behaviours
{
    [ExecuteInEditMode]
    public class PhotonBuffer : MonoBehaviour
    {
        [SerializeField] private bool useTransform;
        [SerializeField] private Sky sky;

        public bool UseTransform
        {
            get => useTransform;
            set => useTransform = value;
        }
        
#if UNITY_EDITOR
        // Register for callbacks when the script is enabled
        private void OnEnable()
        {
            // Subscribe to domain reload events
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;

            // Subscribe to scene reload/change events
            EditorApplication.playModeStateChanged += PlayModeStateChanged;
            EditorSceneManager.sceneOpened += SceneOpened;
            EditorSceneManager.sceneSaved += SceneSaved;
            EditorSceneManager.sceneClosed += SceneClosed;
        }

        private void OnDisable()
        {
            // Unsubscribe to prevent memory leaks
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;

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
            if (sky != null) sky.DisposeBuffer();
        }
        
        public PhotonBuffer Init(Sky sky)
        {
            if (sky == null)
                return this;
            this.sky = sky;
            sky.EnsureBufferExists();
            return this;
        }
        
        public PhotonBuffer LinkComputeKernel(ComputeShader shader, int id = 0)
        {
            if (sky == null) return this;
            sky.LinkComputeKernel(shader, id);
            return this;
        }

        public PhotonBuffer LinkMaterial(Material mat)
        {
            if (sky == null) return this;
            sky.LinkMaterial(mat);
            return this;
        }

        private void Update()
        {
            if (sky == null)
            {
                return;
            }

            if (useTransform)
            {
                sky.UpdateTransform(transform.localToWorldMatrix);
            }
            sky.Update();
        }
    }
}