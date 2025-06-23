using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Uriel.Domain;
using Uriel.Utils;

namespace Uriel.Behaviours
{
    public class StateManager : MonoBehaviour
    {
        public StudioState Current => currentState;
        private Studio studio;
        private StudioState currentState = new ();
        private string SaveDirectory => Path.Combine(Application.dataPath, "Export/Studio/");
        
        public event Action<StudioState> OnStateLoaded = s => {};
        public event Action<StudioState> OnStateSaved = s => {};
        
        private string GetFilePath(string fileName)
        {
            return $"{SaveDirectory}{fileName}.json";
        }

        private void Awake()
        {
            studio = GetComponent<Studio>();
        }

        public void Delete(string fileName)
        {
            try
            {
                File.Delete(GetFilePath(fileName));
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        
        public void SaveState(string fileName)
        {
            currentState.volumes.Clear();
            currentState.waveEmitters.Clear();
            currentState.solids.Clear();
            
            currentState.name = fileName;
            currentState.showGrid = studio.ShowGrid;
            foreach (var volume in studio.Get<Volume>())
            {
                var snapshot = volume.CreateSnapshot();
                currentState.volumes.Add(snapshot);
            }
            foreach (var emitter in studio.Get<WaveEmitter>())
            {
                currentState.waveEmitters.Add(emitter.CreateSnapshot());
            }
            foreach (var sol in studio.Get<SculptSolidBehaviour>())
            {
                currentState.solids.Add(sol.CreateSnapshot());
            }
            SaveToFile(currentState, fileName);
            OnStateSaved?.Invoke(currentState);
        }
        
        public void LoadState(StudioState state)
        {
            studio.ClearAll();
            foreach (var vol in state.volumes)
            {
                studio.Add(vol);
            }
            foreach (var emitter in state.waveEmitters)
            {
                studio.Add(emitter);
            }
            foreach (var solid in state.solids)
            {
                studio.Add(solid);
            }
            studio.ShowGrid = state.showGrid;
            
            OnStateLoaded(currentState);
        }
        
        public IEnumerable<string> ListFiles()
        {
            if (!Directory.Exists(SaveDirectory))
            {
                Directory.CreateDirectory(SaveDirectory);
            }
            var files = Directory.GetFiles(SaveDirectory);
            foreach (var file in files)
            {
                if (Path.GetExtension(file) == ".json")
                {
                    yield return Path.GetFileNameWithoutExtension(file);
                }
            }
        }

        public StudioState CreateNew()
        {
            currentState = new StudioState()
            {
                name = Id.Short,
                volumes = new ()
            };
            LoadState(currentState);
          
            return currentState;
        }
        
        public void LoadState(string fileName)
        {
            var loadedState = LoadFromFile(fileName);
            if (loadedState != null)
            {
                currentState = loadedState;
                LoadState(currentState);
                OnStateLoaded?.Invoke(currentState);
            }
        }
        
        public void LoadFirst()
        {
            var first = ListFiles().FirstOrDefault();
            if (!string.IsNullOrEmpty(first))
            {
                LoadState(first);
            }
        }

        private void SaveToFile(StudioState state, string fileName)
        {
            string json = JsonUtility.ToJson(state, true);
            if (!Directory.Exists(SaveDirectory))
            {
                Directory.CreateDirectory(SaveDirectory);
            }
            File.WriteAllText(GetFilePath(fileName), json);
        }

        private StudioState LoadFromFile(string fileName)
        {
            string filePath = GetFilePath(fileName);
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                return JsonUtility.FromJson<StudioState>(json);
            }
            return null;
        }
    }
}