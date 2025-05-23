#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using Uriel.Domain;

namespace Uriel.Behaviours
{
    [ExecuteInEditMode]
    public abstract class SerializableBufferHandler<T> : MonoBehaviour where T : SerializableBufferBase
    {
        public T Buffer => buffer;
        
        [SerializeField] private T buffer;

        protected virtual void Init()
        {
            if (buffer == null) return;
            var rend = GetComponent<MeshRenderer>();
            if (rend && rend.sharedMaterial)
            {
                buffer.EnsureBufferExists();
                LinkMaterial(rend.sharedMaterial);
            }
        }

        public SerializableBufferHandler<T> LinkComputeKernel(ComputeShader shader, int id = 0)
        {
            if (buffer == null) return this;
            buffer.LinkComputeKernel(shader, id);
            return this;
        }

        public SerializableBufferHandler<T> LinkMaterial(Material mat)
        {
            if (buffer == null) return this;
            buffer.LinkMaterial(mat);
            return this;
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
            if (buffer) buffer.EnsureBufferExists();
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
            if (buffer != null) buffer.DisposeBuffer();
        }


        protected virtual void OnBeforeUpdate() {}


        private void Update()
        {
            if (buffer == null)
            {
                return;
            }

            OnBeforeUpdate();

#if UNITY_EDITOR
            
            OnValidate();
            
#endif
            
            buffer.Update();
        }
    }
}