using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Uriel.Domain;
using Uriel.Utils;

namespace Uriel.Behaviours
{
    public class VolumeStudio : MonoBehaviour
    {
        [SerializeField] private UIDocument ui;
        [SerializeField] private List<VolumeMesh> volumes = new ();
        [SerializeField] private List<WaveEmitter> waveEmitters = new ();
        
        [SerializeField] private VolumeStudioConfig config;
        
        // Core systems
        public CommandHistory CommandHistory { get; private set; } = new ();
        public StateManager StateManager { get; private set; } = new ();

        // Event
        public event Action<VolumeMesh> OnSelectedVolumeChanged;
        public event Action<VolumeMesh> OnVolumeAdded;
        public event Action<VolumeMesh> OnVolumeRemoved;

        // Properties
        public IReadOnlyList<VolumeMesh> Volumes => volumes.AsReadOnly();

        private bool exportInProgress;
        private ProgressBar progressBar;
        private Button exportButton;

        public VolumeMesh SelectedVolume
        {
            get
            {
                if (volumes.Count == 0) return null;
                if (selectedVolume < 0 || selectedVolume >= volumes.Count) return null;
                return volumes[selectedVolume];
            }
        }
        
        private int selectedVolume;

        private void Awake()
        {
            Dispatcher.Init();
            STLExporter.OnExportProgress += OnExportProgress;
            STLExporter.OnExportCompleted += OnExportCompleted;
            BindUI();
        }

        private void OnDestroy()
        {
            STLExporter.OnExportProgress -= OnExportProgress;
            STLExporter.OnExportCompleted -= OnExportCompleted;
        }

        private void OnExportCompleted(string file, int triangles, int size)
        {
            async void UpdateUI()
            {
                progressBar.title = $"Size: {size}; Trigs: {triangles}; Path: {file}!";
                await Task.Delay(3000);
                exportInProgress = false;
                progressBar.visible = false;
                exportButton.SetEnabled(true);
            }

            Dispatcher.Instance.EnqueueAsync(UpdateUI);
        }

        private  void OnExportProgress(float v)
        {
            Dispatcher.Instance.Enqueue(() =>  progressBar.value = v * 100);
        }

        public async void ExportSelectedMesh()
        {
            if (exportInProgress)
            {
                return;
            }
            
            if (SelectedVolume?.GeneratedMesh == null)
            {
                Debug.LogWarning("No mesh to export");
                return;
            }
            exportButton.SetEnabled(false);
            exportInProgress = true;
            progressBar.visible = true;
            progressBar.title = "Exporting STL...";
            try
            {
                await STLExporter.ExportMeshToSTLAsync(
                    name: Guid.NewGuid().ToString().Substring(0, 5).ToUpper(),
                    mesh: SelectedVolume?.GeneratedMesh,
                    binary: true,
                    optimizeVertices: true
                );
            }
            catch (Exception ex)
            {
                exportButton.SetEnabled(true);
                Debug.LogError($"Export failed: {ex.Message}");
                exportInProgress = false;
                progressBar.visible = false;
            }
        }
        
        private void BindUI()
        {
            var root = ui.rootVisualElement;
            var buttons = root.Q<VisualElement>("Buttons");
            var createEmitterBtn = buttons.Q<Button>("CreateEmitter");
            createEmitterBtn.RegisterCallback<ClickEvent>(c =>
            {
                CreateEmitter();
            });
            
            var createVolumeBtn = buttons.Q<Button>("CreateVolume");
            createVolumeBtn.RegisterCallback<ClickEvent>(c =>
            {
                CreateVolume();
            });

            progressBar = root.Q<ProgressBar>("ProgressBar");
            progressBar.visible = false;
            exportButton = buttons.Q<Button>("Export");
            exportButton.RegisterCallback<ClickEvent>(evt =>
            {
                ExportSelectedMesh();
            });

            var xRes = root.Q<DropdownField>("ResolutionX");
            xRes.value = config.resolution.x.ToString();
            xRes.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                if (int.TryParse(evt.newValue, out var x))
                {
                    config.resolution.x = x;
                    UpdateResolution();
                }
            });
            
            var yRes = root.Q<DropdownField>("ResolutionY");
            yRes.value = config.resolution.y.ToString();
            yRes.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                if (int.TryParse(evt.newValue, out var y))
                {
                    config.resolution.y = y;
                    UpdateResolution();
                }
            });
            
            var zRes = root.Q<DropdownField>("ResolutionZ");
            zRes.value = config.resolution.z.ToString();
            zRes.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                if (int.TryParse(evt.newValue, out var z))
                {
                    config.resolution.z = z;
                    UpdateResolution();
                }
            });

            var triangleBudget = root.Q<SliderInt>("TriangleBudget");
            triangleBudget.value = config.triangleBudget;
            triangleBudget.RegisterCallback<ChangeEvent<int>>(evt =>
            {
                config.triangleBudget = evt.newValue;
                UpdateResolution();
            });
        }

        private void UpdateResolution()
        {
            foreach (var emitter in waveEmitters)
            {
                emitter.Initialize(config.@default.field, config.resolution);
            }

            foreach (var volume in volumes)
            {
                volume.ChangeResolution(config.triangleBudget);
            }
        }
        
        private void Update()
        {
            foreach (var emitter in waveEmitters)
            {
                emitter.Run();
            }
            foreach (var volume in volumes)
            {
                volume.RegenerateVolume();
            }
            HandleInput();
        }

        private void HandleInput()
        {
            // Undo/Redo
            if (Input.GetKeyDown(KeyCode.Z) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    CommandHistory.Redo(); 
                else
                    CommandHistory.Undo();
            }

            // Save/Load
            if (Input.GetKeyDown(KeyCode.S) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            {
                StateManager.SaveState(this);
            }

            if (Input.GetKeyDown(KeyCode.L) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            {
                StateManager.LoadState();
            }
        }

        public WaveEmitter CreateEmitter()
        {
            GameObject go = Instantiate(config.waveEmitterPrefab.gameObject);
            WaveEmitter emitter = go.GetComponent<WaveEmitter>();

            emitter.Initialize(config.@default.field, config.resolution);
            emitter.transform.SetParent(transform);
            emitter.name = $"WAVE_EMITTER_{waveEmitters.Count}";
            waveEmitters.Add(emitter);
            
            return emitter;
        }
        
        public VolumeMesh CreateVolume()
        {
            if (waveEmitters.Count == 0)
            {
                return null;
            }
            GameObject go = Instantiate(config.volumeMeshPrefab.gameObject);
            VolumeMesh mesh = go.GetComponent<VolumeMesh>() ?? go.AddComponent<VolumeMesh>();
            
            mesh.DisplayName = $"VOL_{volumes.Count}";;
            mesh.Initialize(config.triangleBudget, config.@default.marchingCubes, waveEmitters[0]);
            mesh.transform.SetParent(transform);
            volumes.Add(mesh);
            SetActiveVolume(mesh);
            
            OnVolumeAdded?.Invoke(mesh);
            return mesh;
        }

        public void RemoveVolumeField(VolumeMesh mesh)
        {
            if (volumes.Contains(mesh))
            {
                volumes.Remove(mesh);
                
                OnVolumeRemoved?.Invoke(mesh);
                
                if (mesh != null)
                {
                    DestroyImmediate(mesh.gameObject);
                }
            }
        }

        public void SetActiveVolume(VolumeMesh volume)
        {
            selectedVolume = volumes.FindIndex(v => v == volume);
            OnSelectedVolumeChanged?.Invoke(SelectedVolume);
        }

        public void ExecuteCommand(ICommand command)
        {
            CommandHistory.ExecuteCommand(command);
        }

        public IEnumerable<VolumeMesh> GetVolumeFields()
        {
            return volumes;
        }
    }
}