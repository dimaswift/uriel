using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Uriel.Commands;
using Uriel.Domain;
using Uriel.Utils;

namespace Uriel.Behaviours
{
    public class VolumeStudio : MonoBehaviour
    {
        public event Action<int> OnExportProgressChanged = v => { };
        public event Action OnExportStarted = () => { };
        public event Action OnExportFinished = ()  => { };
        public VolumeStudioConfig Config => config;
        public CommandHistory CommandHistory { get; private set; }
        public StateManager StateManager { get; private set; }
        public Selector Selector { get; private set; }
        
        public Mover Mover { get; private set; }
        public bool ExportInProgress { get; private set; }
        
        [SerializeField] private VolumeStudioConfig config;
        
        private Camera cam;
     
        private readonly Dictionary<string, Volume> volumes = new ();
        private readonly Dictionary<string, WaveEmitter> waveEmitters = new ();
        private Volume source;
        private ChangeVolumesCommand changeCommand;
        
        private void Awake()
        {
            CommandHistory = new CommandHistory();
            Selector = new Selector(CommandHistory);
            StateManager = new StateManager(this);
            Mover = new Mover(CommandHistory, Selector);
            
            Selector.Register((string id, out ISelectable selectable) =>
            {
                if (volumes.TryGetValue(id, out var vol))
                {
                    selectable = vol;
                    return true;
                }
                selectable = null;
                return false;
            }, GetVolumes);
            
            Selector.Register((string id, out ISelectable selectable) =>
            {
                if (waveEmitters.TryGetValue(id, out var emitter))
                {
                    selectable = emitter;
                    return true;
                }
                selectable = null;
                return false;
            }, GetWaveEmitters);
            
            source = Instantiate(config.volumePrefab.gameObject).GetComponent<Volume>();
            source.transform.SetParent(transform);
            source.gameObject.SetActive(false);
            cam = Camera.main;
            
            StateManager.LoadFirst();
            
            Dispatcher.Init();
        }
        
        public async void ExportSelectedMesh()
        {
            if (ExportInProgress)
            {
                return;
            }
            var total = Selector.GetSelectedCount<Volume>();

            if (total == 0)
            {
                return;
            }
            
            OnExportStarted();
            OnExportProgressChanged(0);
            ExportInProgress = true;
            int progress = 0;
          
            foreach (var sel in Selector.GetSelected<Volume>())
            {
                var vol = volumes[sel.ID];
                try
                {
                    await STLExporter.ExportMeshToSTLAsync(
                        name: vol.ID,
                        mesh: vol.GeneratedMesh,
                        binary: true,
                        optimizeVertices: true
                    );
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Export failed: {ex.Message}");
                }
                progress++;
                OnExportProgressChanged(Mathf.RoundToInt(((float) progress / total) * 100));
            }
            OnExportFinished();
            ExportInProgress = false;
        }

        private void ClearAll()
        {
            foreach (var volume in volumes)
            {
                Destroy(volume.Value.gameObject);
            }

            foreach (var emitter in waveEmitters)
            {
                Destroy(emitter.Value.gameObject);
            }
            waveEmitters.Clear();
            volumes.Clear();
            Selector.ClearSelection();
            Mover.Reset();
        }
        
        
        public void LoadState(StudioState state)
        {
            ClearAll();
            foreach (var snapshot in state.volumes)
            {
                AddVolume(snapshot);
            }

            foreach (var emitter in state.waveEmitters)
            {
                AddEmitter(emitter);
            }
        }
        
        public IEnumerable<Volume> GetVolumes()
        {
            foreach (var v in volumes)
            {
                yield return v.Value;
            }
        }
        
        
        private void DeleteSelected()
        {
            var list = new List<Volume>(Selector.GetSelected<Volume>());
            ExecuteDeleteVolumesCommand(list.ToArray());
        }
        
        private void Update()
        {
            foreach (var emitter in GetWaveEmitters())
            {
                emitter.Run();
            }

            if (waveEmitters.Count > 0)
            {
                foreach (var volume in GetVolumes())
                {
                    volume.Regenerate(waveEmitters.FirstOrDefault().Value);
                }
            }

            Mover.Update();
            
            HandleInput();
        }

        private void DuplicateSelected()
        {
            var list = new List<VolumeSnapshot>();
            foreach (var vol in Selector.GetSelected<Volume>())
            {
                list.Add(vol.CreateSnapshot());
            }
            CreateVolumes(list.ToArray());
        }

      

        private void HandleInput()
        {
            Selector.Update();
            
            if (Input.GetKeyDown(KeyCode.Delete))
            {
                DeleteSelected();
            }
            
            if (Input.GetKey(KeyCode.LeftCommand) && Input.GetKeyDown(KeyCode.D))
            {
                DuplicateSelected();
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
        
        public WaveEmitter CreateDefaultEmitter()
        {
            var snapshot = new WaveEmitterSnapshot()
            {
                id = Id.Short,
                sources = new List<WaveSource>(Lattices.Tetrahedron(WaveSource.Default)),
                resolution = config.resolution,
                saturate = true
            };
            return AddEmitter(snapshot);
        }

        public WaveEmitter AddEmitter(WaveEmitterSnapshot snapshot)
        {
            if (waveEmitters.ContainsKey(snapshot.id))
            {
                return null;
            }
            GameObject go = Instantiate(config.waveEmitterPrefab.gameObject);
            WaveEmitter emitter = go.GetComponent<WaveEmitter>();
            emitter.Restore(snapshot);
            emitter.transform.SetParent(transform);
            emitter.name = $"WAVE_EMITTER_{snapshot.id}";
            waveEmitters.Add(snapshot.id, emitter);
            return emitter;
        }
        
        public Volume AddVolume(VolumeSnapshot snapshot)
        {
            if (volumes.ContainsKey(snapshot.id))
            {
                return null;
            }
            var volume = Instantiate(config.volumePrefab.gameObject).GetComponent<Volume>();
            volume.transform.SetParent(transform);
            volume.Restore(snapshot);
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
        
        public void CreateVolumes(params VolumeSnapshot[] sources)
        {
            if (sources.Length == 0)
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

        public void CreateDefaultVolume()
        {
            var snapshot = source.CreateSnapshot();
            snapshot.id = Id.Short;
            snapshot.marchingCubes.budget = config.triangleBudget; 
            var cmd = new CreateVolumeCommand(this, snapshot);
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
        
        public IEnumerable<WaveEmitter> GetWaveEmitters()
        {
            return waveEmitters.Values;
        }
    }
}