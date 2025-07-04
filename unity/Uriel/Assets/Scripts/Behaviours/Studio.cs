using System.Collections.Generic;
using UnityEngine;
using Uriel.Commands;
using Uriel.Domain;
using Uriel.Utils;

namespace Uriel.Behaviours
{
    public class Studio : MonoBehaviour
    {
        public bool ShowGrid
        {
            get => grid.activeSelf;
            set => grid.SetActive(value);
        }
        public StudioConfig Config => config;
        public CommandHistory CommandHistory { get; private set; }
        public StateManager StateManager { get; private set; }
        public Selector Selector { get; private set; }
        public MoveHandle MoveHandle { get; private set; }
        public ScaleHandle ScaleHandle { get; private set; }

        public CameraController Camera { get; private set; }
        
        [SerializeField] private StudioConfig config;
        
        [SerializeField] private GameObject grid;
        
        private readonly Dictionary<string, IModifiable> modifiables = new ();

        private readonly Dictionary<string, IModifiable> prefabs = new();

        private Vector3 lastCameraAnchor;
        
        
        private void Awake()
        {
            CommandHistory = GetComponent<CommandHistory>();
            Selector = GetComponent<Selector>();
            StateManager = GetComponent<StateManager>();
            MoveHandle = GetComponent<MoveHandle>();
            ScaleHandle = GetComponent<ScaleHandle>();
            Camera = FindFirstObjectByType<CameraController>();
            Selector.Register((string id, out ISelectable selectable) =>
            {
                if (modifiables.TryGetValue(id, out var vol))
                {
                    selectable = vol;
                    return true;
                }
                selectable = null;
                return false;
            }, GetAll);

            foreach (var prefab in config.prefabs)
            {
                var source = Instantiate(prefab).GetComponent<IModifiable>();
                source.transform.SetParent(transform);
                source.transform.gameObject.SetActive(false);
                prefabs.TryAdd(source.GetType().Name, source);
            }
        
            StateManager.LoadFirst();

            Dispatcher.Init();
        }
        

        public void ClearAll()
        {
            foreach (var volume in modifiables)
            {
                Destroy(volume.Value.transform.gameObject);
            }
            modifiables.Clear();
            Selector.ClearSelection();
            MoveHandle.Reset();
        }
        
        private void DeleteSelected()
        {
            var list = new List<IModifiable>(Selector.GetSelected<IModifiable>());
            Delete(list);
        }
        
        public int Count<T>() where T : class, IModifiable
        {
            int c = 0;
            foreach (var _ in Get<T>())
            {
                c++;
            }
            return c;
        }
        
        private void Update()
        {
            foreach (var emitter in Get<WaveEmitter>())
            {
                emitter.Run();
            }

            foreach (var emitter in Get<WaveEmitter>())
            {
                foreach (var volume in Get<Volume>())
                {
                    volume.Regenerate(emitter);
                }
                break;
            }
            
            MoveHandle.Update();

            if (Input.GetKeyDown(KeyCode.F))
            {
                var anchor = Vector3.zero;
                int amount = 0;
                foreach (var m in Selector.GetSelected<IModifiable>())
                {
                    amount++;
                    anchor += m.transform.position;
                }
                if (amount > 0)
                {
                    anchor /= amount;
                    lastCameraAnchor = anchor;
                    Camera.SetOrbitPoint(lastCameraAnchor);
                    Camera.FocusOnTarget();
                }
            }
            
            HandleInput();
        }

        private void DuplicateSelected()
        {
            var list = new List<ISnapshot>();
            foreach (var vol in Selector.GetSelected<IModifiable>())
            {
                list.Add(vol.CreateSnapshot());
            }
            Create(list);
        }

        private void HandleInput()
        {
            Selector.Update();
            
            if (Input.GetKeyDown(KeyCode.Delete))
            {
                DeleteSelected();
            }
            
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.D))
            {
                DuplicateSelected();
            }
            
            if (Input.GetKeyDown(KeyCode.Z) && Input.GetKey(KeyCode.LeftControl))
            {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    CommandHistory.Redo(); 
                }
                else
                {
                    CommandHistory.Undo();
                }
            }
        }
        
        public IModifiable Add(ISnapshot snapshot)
        {
            if (modifiables.ContainsKey(snapshot.ID))
            {
                Debug.LogWarning($"Snapshot with id {snapshot.ID} already exists");
                return null;
            }
            if (!prefabs.TryGetValue(snapshot.TargetType, out var prefab))
            {
                return null;
            }
            var mod = Instantiate(prefab.transform.gameObject).GetComponent<IModifiable>();
            mod.transform.gameObject.SetActive(true);
            mod.transform.name = $"{snapshot.TargetType}_{snapshot.ID}";

            if (snapshot.ParentID != null)
            {
                var parent = Find(snapshot.ParentID);
                if (parent != null)
                {
                    mod.transform.SetParent(parent.transform);
                }
                else
                {
                    mod.transform.SetParent(transform);
                }
            }
            else
            {
                mod.transform.SetParent(transform);
            }
          
            mod.Restore(snapshot);
            modifiables.Add(snapshot.ID, mod);
            return mod;
        }
        
        public bool Remove(string id)
        {
            if (!modifiables.TryGetValue(id, out var volume))
            {
                return false;
            }
            Selector.Deselect(id);
            modifiables.Remove(id);
            Destroy(volume.transform.gameObject);
            return true;
        }

        public void AssignParents()
        {
            foreach (var modifiable in modifiables)
            {
                var mod = modifiable.Value;
                if (mod.Current.ParentID == null)
                {
                    continue;
                }

                var parent = Find(mod.Current.ParentID);

                if (parent == null)
                {
                    continue;
                }
                
                mod.transform.SetParent(parent.transform);
            }
        }
        
        public void Create(IReadOnlyList<ISnapshot> snapshots)
        {
            if (snapshots.Count == 0)
            {
                return;
            }
            foreach (var snapshot in snapshots)
            {
                snapshot.ID = Id.Short;
            }
            var cmd = new CreateCommand(this, snapshots);
            CommandHistory.ExecuteCommand(cmd);
        }

        public void Create(ISnapshot source, IModifiable parent)
        {
            source.ID = Id.Short;
            source.ParentID = parent?.ID;
            var cmd = new CreateCommand(this, source);
            CommandHistory.ExecuteCommand(cmd);
        }
        
        public ISnapshot CreateDefault<T>(IModifiable parent) where T : class, IModifiable
        {
            if (!prefabs.TryGetValue(typeof(T).Name, out var prefab))
            {
                return null;
            }
            var snapshot = prefab.CreateSnapshot();
            snapshot.ID = Id.Short;
            snapshot.ParentID = parent?.ID;
            var cmd = new CreateCommand(this, snapshot);
            CommandHistory.ExecuteCommand(cmd);
            return snapshot;
        }
        
        public void Delete<T>(IEnumerable<T> targets) where T : class, IModifiable
        {
            var list = new List<T>(targets);
            var cmd = new DeleteCommand(this, list);
            CommandHistory.ExecuteCommand(cmd);
        }
        
        public T Find<T>(string id)  where T : class, IModifiable
        {
            return modifiables.GetValueOrDefault(id) as T;
        }
        
        public IModifiable Find(string id)
        {
            return modifiables.GetValueOrDefault(id);
        }
        
        public IEnumerable<IModifiable> GetAll()
        {
            foreach (var modifiable in modifiables)
            {
                yield return modifiable.Value;
            }
        }
        
        public IEnumerable<T> Get<T>() where T : class, IModifiable
        {
            foreach (var modifiable in modifiables)
            {
                if (modifiable.Value is T)
                {
                    yield return modifiable.Value as T;
                }
            }
        }
    }
}