using System.Collections.Generic;
using UnityEngine;
using Uriel.Behaviours;

namespace Uriel.Commands
{
    public interface ISnapshot
    {
        string ID { get; set; }
        string ParentID { get; set; }
        string TargetType { get; }
        bool ValueEquals(ISnapshot snapshot);
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

        public bool ApplyModifications(IReadOnlyList<IModifiable> modifiables)
        {
            if (modifiables.Count != newStates.Length)
            {
                Debug.LogError($"States amount mismatch");
                return false;
            }

            bool changed = false;
            for (int i = 0; i < modifiables.Count; i++)
            {
                var newState = modifiables[i].CreateSnapshot() as T;
                newStates[i] = newState;
                if (newState != null && !newState.ValueEquals(oldStates[i]))
                {
                    changed = true;
                }
            }

            return changed;
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