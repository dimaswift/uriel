using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using Uriel.Commands;
using Uriel.Domain;
using Uriel.Utils;

namespace Uriel.Behaviours
{
    public class VolumeStudio : MonoBehaviour
    {
        [SerializeField] private UIDocument ui;
       
      
        
        [SerializeField] private VolumeStudioConfig config;
        
        // Core systems
        public CommandHistory CommandHistory { get; private set; }
        public StateManager StateManager { get; private set; }
        public Selector Selector { get; private set; }
        // Properties

        private bool exportInProgress;
        private ProgressBar progressBar;
        private Button exportButton;
        private Camera cam;
        private bool moving;
        private Vector3 moveStartPoint;
        private readonly Dictionary<Volume, Vector3> moveClickPoints = new();
        private readonly Dictionary<string, Volume> volumes = new ();
        private readonly List<WaveEmitter> waveEmitters = new ();
        private Volume source;
        private ChangeVolumesCommand changeCommand;
        
        private void Awake()
        {
            CommandHistory = new CommandHistory();
            StateManager = new StateManager(this);
            Selector = new Selector(CommandHistory);
            
            Selector.AddSource((string id, out ISelectable selectable) =>
            {
                if (volumes.TryGetValue(id, out var vol))
                {
                    selectable = vol;
                    return true;
                }
                selectable = null;
                return false;
            });
            
            source = Instantiate(config.volumePrefab.gameObject).GetComponent<Volume>();
            source.transform.SetParent(transform);
            source.gameObject.SetActive(false);
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
            if (exportInProgress || Selector.Size == 0)
            {
                return;
            }
            exportButton.SetEnabled(false);
            exportInProgress = true;
            progressBar.visible = true;
            int progress = 0;
            foreach (var sel in Selector.GetSelected<Volume>())
            {
                var vol = volumes[sel.ID];
                progress++;
                if (vol?.GeneratedMesh == null)
                {
                    continue;
                }

                progressBar.value = 0;
                progressBar.title = $"Exporting STL {progress}/{Selector.GetSelectedCount<Volume>()}";
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

        private void UpdateConfig()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(config);
            #endif
        }

        private void ClearAll()
        {
            foreach (var volume in volumes)
            {
                Destroy(volume.Value.gameObject);
            }

            foreach (var emitter in waveEmitters)
            {
                Destroy(emitter.gameObject);
            }
            waveEmitters.Clear();
            volumes.Clear();
            Selector.ClearSelection();
            moveClickPoints.Clear();
            moving = false;
        }
        
        public void CreateNewState()
        {
            ClearAll();
        }
        
        public void LoadState(StudioState state)
        {
            ClearAll();
        }
        
        private void BindUI()
        {
            StateManager.BindUI(ui);
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
                ExecuteCreateVolumesCommand(source.CreateSnapshot());
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
                    UpdateConfig();
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
                    UpdateConfig();
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
                    UpdateConfig();
                    UpdateResolution();
                }
            });

            var triangleBudget = root.Q<SliderInt>("TriangleBudget");
            triangleBudget.value = config.triangleBudget;
            triangleBudget.RegisterCallback<ChangeEvent<int>>(evt =>
            {
                config.triangleBudget = evt.newValue;
                UpdateConfig();
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
                volume.Value.ChangeResolution(config.triangleBudget);
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
        
        public IEnumerable<Volume> GetVolumes()
        {
            foreach (var v in volumes)
            {
                yield return v.Value;
            }
        }
        
        private void BeginMove()
        {
            moveClickPoints.Clear();
            var targets = new List<Volume>();
            foreach (var vol in Selector.GetSelected<Volume>())
            {
                moveClickPoints.Add(vol, vol.transform.position);
                targets.Add(vol);
            }
            moving = Selector.GetSelectedCount<Volume>() > 0;
            moveStartPoint = GetPlanePointer();
            changeCommand = new ChangeVolumesCommand(this, targets.ToArray());
        }

        private void FinishMove()
        {
            changeCommand.SaveNewStates(Selector.GetSelected<Volume>().ToArray());
            CommandHistory.ExecuteCommand(changeCommand);
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
            var list = new List<Volume>(Selector.GetSelected<Volume>());
            ExecuteDeleteVolumesCommand(list.ToArray());
        }
        
        private void Update()
        {
            foreach (var emitter in waveEmitters)
            {
                emitter.Run();
            }
            foreach (var volume in GetVolumes())
            {
                volume.RegenerateVolume();
            }
            
            if (moving)
            {
                HandleMoving();
            }
            
            HandleInput();
        }

        private void DuplicateSelected()
        {
            var list = new List<VolumeSnapshot>();
            foreach (var vol in Selector.GetSelected<Volume>())
            {
                list.Add(vol.CreateSnapshot());
            }
            ExecuteCreateVolumesCommand(list.ToArray());
        }



        private bool IsMouseOverVolume(out Volume result)
        {
            var ray = cam.ScreenPointToRay(Input.mousePosition);
            float closestDist = float.MaxValue;
            Volume hit = null;
            foreach (var volume in GetVolumes())
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

        private void HandleInput()
        {
            if (!moving && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0)))
            {
                if (IsMouseOverVolume(out var vol))
                {
                    Selector.HandleSelection(vol);
                }
                else
                {
                    Selector.ClearSelection<Volume>();
                }
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
                if (moving && Selector.GetSelectedCount<Volume>() > 0)
                {
                    FinishMove();
                }
                moving = false;
            }
            
            if (Input.GetKeyDown(KeyCode.Z) && Input.GetKey(KeyCode.LeftControl))
            {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    CommandHistory.Redo(); 
                else
                    CommandHistory.Undo();
            }

            if (Input.GetKeyDown(KeyCode.S) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            {
                StateManager.SaveState(Id.Short);
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
        
        public Volume AddVolume(VolumeSnapshot snapshot)
        {
            if (volumes.ContainsKey(snapshot.id))
            {
                return null;
            }
            var volume = Instantiate(config.volumePrefab.gameObject).GetComponent<Volume>();
            volume.Initialize(snapshot.id, config.triangleBudget, config.@default.marchingCubes, waveEmitters[0]);
            volume.transform.SetParent(transform);
            volume.RestoreFromSnapshot(snapshot);
            volumes.Add(snapshot.id, volume);
            Selector.Select(volume.ID);
            return volume;
        }
        
        public bool RemoveVolume(string id)
        {
            if (!volumes.TryGetValue(id, out var volume))
            {
                return false;
            }
            Selector.Deselect(volume.ID);
            volumes.Remove(id);
            Destroy(volume.gameObject);
            return true;
        }
        
        public void ExecuteCreateVolumesCommand(params VolumeSnapshot[] sources)
        {
            if (waveEmitters.Count == 0)
            {
                return;
            }

            foreach (var snapshot in sources)
            {
                snapshot.id = Id.Short;
            }
            var cmd = new CreateVolumeCommand(this, sources);
            CommandHistory.ExecuteCommand(cmd);
        }
        
        public void ExecuteDeleteVolumesCommand(params Volume[] volumesToDelete)
        {
            var cmd = new DeleteVolumesCommand(this, volumesToDelete);
            CommandHistory.ExecuteCommand(cmd);
        }
        
        public Volume GetVolume(string id)
        {
            return volumes.GetValueOrDefault(id);
        }
    }
}