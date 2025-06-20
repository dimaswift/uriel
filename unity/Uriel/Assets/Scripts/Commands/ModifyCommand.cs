using System.Collections.Generic;
using UnityEngine;
using Uriel.Behaviours;

namespace Uriel.Commands
{
    public interface ISnapshot
    {
        string ID { get; set; }
        string TargetType { get; }
    }
    
    public interface IModifiable : ISelectable
    {
        ISnapshot Current { get; }
        ISnapshot CreateSnapshot();
        void Restore(ISnapshot snapshot);
        Transform transform { get; }
    }
    
    public class ModifyCommand<T> : StudioCommand where T : class, ISnapshot
    {
        private readonly T[] oldStates;
        private readonly T[] newStates;
        public ModifyCommand(Studio studio, IReadOnlyList<IModifiable> modifiables) : base(studio)
        {
            oldStates = new T[modifiables.Count];
            newStates = new T[modifiables.Count];
            for (int i = 0; i < modifiables.Count; i++)
            {
                oldStates[i] = modifiables[i].CreateSnapshot() as T;
            }
        }

        public void ApplyModifications(IReadOnlyList<IModifiable> modifiables)
        {
            if (modifiables.Count != newStates.Length)
            {
                Debug.LogError($"States amount mismatch");
                return;
            }
            for (int i = 0; i < modifiables.Count; i++)
            {
                newStates[i] = modifiables[i].CreateSnapshot() as T;
            }
        }
        
        public override void Execute()
        {
            foreach (var snapshot in newStates)
            {
                var vol = Studio.Find(snapshot.ID);
                if (vol == null) continue;
                vol.Restore(snapshot);
            }
        }
        
        public override void Undo()
        {
            foreach (var snapshot in oldStates)
            {
                var vol = Studio.Find(snapshot.ID);
                if (vol == null) continue;
                vol.Restore(snapshot);
            }
        }
    }
}