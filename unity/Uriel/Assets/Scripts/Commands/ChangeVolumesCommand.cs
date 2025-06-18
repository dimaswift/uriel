using System.Collections.Generic;
using UnityEngine;
using Uriel.Behaviours;
using Uriel.Domain;

namespace Uriel.Commands
{
    public class ChangeVolumesCommand : StudioCommand
    {
        private readonly VolumeSnapshot[] oldStates;
        private readonly VolumeSnapshot[] newStates;
        public ChangeVolumesCommand(VolumeStudio studio, params Volume[] volumes) : base(studio)
        {
            oldStates = new VolumeSnapshot[volumes.Length];
            newStates = new VolumeSnapshot[volumes.Length];
            for (int i = 0; i < volumes.Length; i++)
            {
                oldStates[i] = volumes[i].CreateSnapshot();
            }
        }

        public void SaveNewStates(params Volume[] volumes)
        {
            if (volumes.Length != newStates.Length)
            {
                Debug.LogError($"States amount mismatch");
                return;
            }
            for (int i = 0; i < volumes.Length; i++)
            {
                newStates[i] = volumes[i].CreateSnapshot();
            }
        }
        
        public override void Execute()
        {
            foreach (var snapshot in newStates)
            {
                var vol = Studio.GetVolume(snapshot.id);
                if (!vol) continue;
                vol.RestoreFromSnapshot(snapshot);
            }
        }
        
        
        public override void Undo()
        {
            foreach (var snapshot in oldStates)
            {
                var vol = Studio.GetVolume(snapshot.id);
                if (!vol) continue;
                vol.RestoreFromSnapshot(snapshot);
            }
        }
    }
}