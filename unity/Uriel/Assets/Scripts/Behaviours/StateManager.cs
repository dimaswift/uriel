using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Uriel.Domain;
using Uriel.Utils;

namespace Uriel.Behaviours
{
    public class StateManager
    {
        public StudioState Current => currentState;
        private readonly Studio studio;
        private StudioState currentState = new ();
        private string SaveDirectory => Path.Combine(Application.dataPath, "Export/Studio/");
        
        public event Action<StudioState> OnStateLoaded = s => {};
        public event Action<StudioState> OnStateSaved = s => {};
        
        public StateManager(Studio studio)
        {
            this.studio = studio;
        }

        private string GetFilePath(string name)
        {
            return $"{SaveDirectory}{name}.json";
        }
        
        public void Delete(string name)
        {
            try
            {
                File.Delete(GetFilePath(name));
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        
        public void SaveState(string name)
        {
            currentState.volumes.Clear();
            currentState.waveEmitters.Clear();
            currentState.name = name;
            foreach (var volume in studio.Get<Volume>())
            {
                var snapshot = volume.CreateSnapshot();
                currentState.volumes.Add(snapshot);
            }
            foreach (var emitter in studio.Get<WaveEmitter>())
            {
                currentState.waveEmitters.Add(emitter.CreateSnapshot());
            }
            
            SaveToFile(currentState, name);
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
            OnStateLoaded(currentState);
            return currentState;
        }
        
        public void LoadState(string name)
        {
            var loadedState = LoadFromFile(name);
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

        private void SaveToFile(StudioState state, string name)
        {
            string json = JsonUtility.ToJson(state, true);
            if (!Directory.Exists(SaveDirectory))
            {
                Directory.CreateDirectory(SaveDirectory);
            }
            File.WriteAllText(GetFilePath(name), json);
        }

        private StudioState LoadFromFile(string name)
        {
            string filePath = GetFilePath(name);
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                return JsonUtility.FromJson<StudioState>(json);
            }
            return null;
        }
    }
}