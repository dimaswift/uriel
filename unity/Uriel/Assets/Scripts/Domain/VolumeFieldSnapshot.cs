using System;
using System.Collections.Generic;
using UnityEngine;
using Uriel.Behaviours;

namespace Uriel.Domain
{
    [System.Serializable]
    public class VolumeFieldSnapshot
    {
        public string id;
        public string name;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
        public SculptState sculptState;
        public DateTime timestamp;
        
        public VolumeFieldSnapshot()
        {
            timestamp = DateTime.Now;
        }
    }
    
    [System.Serializable]
    public class StudioState
    {
        public List<VolumeFieldSnapshot> volumeFields = new ();
        public DateTime lastSaved;
    }
    
    public class StateManager
    {
        private StudioState currentState = new StudioState();
        private string saveDirectory = "Assets/VolumeStudio/Saves/";
        
        public event System.Action<StudioState> OnStateLoaded;
        public event System.Action<StudioState> OnStateSaved;

        public void SaveState(VolumeStudio studio)
        {
            currentState.volumeFields.Clear();
            currentState.lastSaved = DateTime.Now;
            
            foreach (var field in studio.GetVolumeFields())
            {
                var snapshot = field.CreateSnapshot();
                currentState.volumeFields.Add(snapshot);
            }
            
            // Save to file (implement your preferred serialization)
            SaveToFile(currentState);
            OnStateSaved?.Invoke(currentState);
        }

        public void LoadState()
        {
            var loadedState = LoadFromFile();
            if (loadedState != null)
            {
                currentState = loadedState;
                OnStateLoaded?.Invoke(currentState);
            }
        }

        private void SaveToFile(StudioState state)
        {
            // Implement JSON/Binary serialization
            string json = JsonUtility.ToJson(state, true);
            System.IO.File.WriteAllText($"{saveDirectory}studio_state.json", json);
        }

        private StudioState LoadFromFile()
        {
            string filePath = $"{saveDirectory}studio_state.json";
            if (System.IO.File.Exists(filePath))
            {
                string json = System.IO.File.ReadAllText(filePath);
                return JsonUtility.FromJson<StudioState>(json);
            }
            return null;
        }
    }
}