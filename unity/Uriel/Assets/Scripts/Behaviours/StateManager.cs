using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;
using Uriel.Behaviours;
using Uriel.Domain;

namespace Uriel.Behaviours
{
    public class StateManager
    {
        private readonly VolumeStudio studio;
        private StudioState currentState = new StudioState();
        private string saveDirectory = "Assets/Export/VolumeStudio/";
        
        public event Action<StudioState> OnStateLoaded;
        public event Action<StudioState> OnStateSaved;

        public StateManager(VolumeStudio studio)
        {
            this.studio = studio;
        }
        

        public void SaveState(string name)
        {
            currentState.volumes.Clear();
            currentState.name = name;
            currentState.lastSaved = DateTime.Now;
            
            foreach (var volume in studio.GetVolumes())
            {
                var snapshot = volume.CreateSnapshot();
                currentState.volumes.Add(snapshot);
            }
            
            SaveToFile(currentState, name);
            OnStateSaved?.Invoke(currentState);
        }

        public void BindUI(UIDocument ui)
        {
            var root = ui.rootVisualElement.Q("SavePanel");
            var createNewButton = root.Q<Button>("New");
            createNewButton.RegisterCallback<ClickEvent>(evt =>
            {
                studio.CreateNewState();
            });
        }

        public IEnumerable<string> ListFiles()
        {
            var files = Directory.GetFiles(saveDirectory);
            foreach (var file in files)
            {
                if (Path.GetExtension(file) == ".json")
                {
                    yield return Path.GetFileNameWithoutExtension(file);
                }
            }
        }
        
        public void LoadState(string name)
        {
            var loadedState = LoadFromFile(name);
            if (loadedState != null)
            {
                currentState = loadedState;
                OnStateLoaded?.Invoke(currentState);
            }
        }

        private void SaveToFile(StudioState state, string name)
        {
            string json = JsonUtility.ToJson(state, true);
            if (!Directory.Exists(saveDirectory))
            {
                Directory.CreateDirectory(saveDirectory);
            }
            File.WriteAllText($"{saveDirectory}{name}.json", json);
        }

        private StudioState LoadFromFile(string name)
        {
            string filePath = $"{saveDirectory}{name}.json";
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                return JsonUtility.FromJson<StudioState>(json);
            }
            return null;
        }
    }
}