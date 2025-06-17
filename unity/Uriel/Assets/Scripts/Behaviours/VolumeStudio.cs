using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Unity.VisualScripting;
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

        // Properties
        public IReadOnlyList<VolumeMesh> Volumes => volumes.AsReadOnly();
        private readonly HashSet<VolumeMesh> selection = new();
        private VolumeMesh lastSelection;
        private bool exportInProgress;
        private ProgressBar progressBar;
        private Button exportButton;
        private Camera cam;
        private bool moving;
        private Vector3 moveStartPoint;
        private readonly Dictionary<VolumeMesh, Vector3> moveClickPoints = new();
        
        private void Awake()
        {
            cam = Camera.main;
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
            Dispatcher.Instance.Enqueue(() => progressBar.value = v * 100);
        }

        public async void ExportSelectedMesh()
        {
            if (exportInProgress || selection.Count == 0)
            {
                return;
            }
            exportButton.SetEnabled(false);
            exportInProgress = true;
            progressBar.visible = true;
            int progress = 0;
            foreach (var vol in selection)
            {
                progress++;
                if (vol?.GeneratedMesh == null)
                {
                    continue;
                }

                progressBar.value = 0;
                progressBar.title = $"Exporting STL {progress}/{selection.Count}";
                try
                {
                    await STLExporter.ExportMeshToSTLAsync(
                        name: Guid.NewGuid().ToString().Substring(0, 5).ToUpper(),
                        mesh: vol.GeneratedMesh,
                        binary: true,
                        optimizeVertices: true
                    );
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Export failed: {ex.Message}");
                   
                }
            }
            exportButton.SetEnabled(true);
            exportInProgress = false;
            progressBar.visible = false;
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
                CreateVolume(config.volumeMeshPrefab);
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

        private Vector3 GetPlanePointer()
        {
            var ray = cam.ScreenPointToRay(Input.mousePosition);
            var plane = new Plane(Vector3.up, Vector3.zero);
            if (plane.Raycast(ray, out var dist))
            {
                return ray.GetPoint(dist);
            }

            return Vector3.zero;
        }

        private void BeginMove()
        {
            moveClickPoints.Clear();
            foreach (var vol in selection)
            {
                moveClickPoints.Add(vol, vol.transform.position);
            }
            moving = selection.Count > 0;
            moveStartPoint = GetPlanePointer();
        }

        private void HandleMoving()
        {
            var mouse = GetPlanePointer();
            var mouseDelta = mouse - moveStartPoint;
            foreach (var volumeMesh in moveClickPoints)
            {
                volumeMesh.Key.transform.position = volumeMesh.Value + mouseDelta;
            }
        }

        private void DeleteSelected()
        {
            foreach (var vol in selection)
            {
                volumes.RemoveAll(v => v == vol);
                Destroy(vol.gameObject);
            }
            selection.Clear();
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
            
            if (!moving && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0)))
            {
                HandleSelection(Input.GetMouseButtonUp(0));
            }

            if (!moving && Input.GetMouseButton(0) && Input.mousePositionDelta.magnitude > 0.1f)
            {
                BeginMove();
            }
            
            if (Input.GetKeyDown(KeyCode.Delete))
            {
                DeleteSelected();
            }
            
            if (Input.GetKey(KeyCode.LeftCommand) && Input.GetKeyDown(KeyCode.D))
            {
                DuplicateSelected();
            }
            
            if (Input.GetMouseButtonUp(0))
            {
                moving = false;
                lastSelection = null;
            }

            if (moving)
            {
                HandleMoving();
            }
            
            HandleInput();
        }

        private void DuplicateSelected()
        {
            foreach (var vol in selection)
            {
                CreateVolume(vol);
            }
        }

        private void ClearSelection()
        {
            lastSelection = null;
            foreach (var vol in selection)
            {
                vol.Selected = false;
            }
            selection.Clear();
        }

        private bool IsMouseOverVolume(out VolumeMesh result)
        {
            var ray = cam.ScreenPointToRay(Input.mousePosition);
            float closestDist = float.MaxValue;
            VolumeMesh hit = null;
            foreach (var volume in volumes)
            {
                if (volume.Bounds.IntersectRay(ray, out var dist) && dist < closestDist)
                {
                    hit = volume;
                    closestDist = dist;
                }
            }
            result = hit;
            return hit != null;
        }

        private void HandleSelection(bool mouseUp)
        {
            if (!IsMouseOverVolume(out var hit))
            {
                ClearSelection();
                return;
            }

            if (hit == lastSelection)
            {
                return;
            }
            if (Input.GetKey(KeyCode.LeftCommand))
            {
                if (!mouseUp)
                {
                    ToggleVolumeSelection(hit);
                }
            }
            else
            {
                if (!hit.Selected)
                {
                    ClearSelection();
                    ToggleVolumeSelection(hit);
                }
                else 
                {
                    if (mouseUp)
                    {
                        ToggleVolumeSelection(hit);
                    }
                }
            }
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
        
        public VolumeMesh CreateVolume(VolumeMesh source)
        {
            if (waveEmitters.Count == 0)
            {
                return null;
            }
            GameObject go = Instantiate(source.gameObject);
            VolumeMesh mesh = go.GetComponent<VolumeMesh>() ?? go.AddComponent<VolumeMesh>();
            
            mesh.DisplayName = $"VOL_{volumes.Count}";;
            mesh.Initialize(config.triangleBudget, config.@default.marchingCubes, waveEmitters[0]);
            mesh.transform.SetParent(transform);
            volumes.Add(mesh);
            
            return mesh;
        }

        public void RemoveVolumeField(VolumeMesh mesh)
        {
            if (volumes.Contains(mesh))
            {
                volumes.Remove(mesh);

                if (mesh != null)
                {
                    DestroyImmediate(mesh.gameObject);
                }
            }
        }

        public void ToggleVolumeSelection(VolumeMesh volume)
        {

            if (volume.Selected)
            {
                volume.Selected = false;
                selection.Remove(volume);
                lastSelection = null;
                return;
            }
            
            if (selection.Add(volume))
            {
                volume.Selected = true;
                lastSelection = volume;
            }
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