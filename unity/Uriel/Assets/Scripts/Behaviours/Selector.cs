using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Uriel.Commands;

namespace Uriel.Behaviours
{
    public delegate bool SelectorDelegate(string id, out ISelectable selectable);
    public class Selector
    {
        public ISelectable LastSelection => lastSelection;
        public int Size => selection.Count;
        private readonly CommandHistory history;
        
        private readonly Dictionary<string, ISelectable> selection = new();
        private ISelectable lastSelection;
        private readonly List<ISelectable> buffer = new();
        private readonly List<SelectorDelegate> sources = new();

        public Selector(CommandHistory history)
        {
            this.history = history;
        }

        public void AddSource(SelectorDelegate source)
        {
            sources.Add(source);
        }

        public IEnumerable<T> GetSelected<T>() where T : class, ISelectable
        {
            foreach (var id in selection)
            {
                if (id.Value is T)
                {
                    yield return id.Value as T;
                }
            }
        }
        
        public IEnumerable<string> GetSelectedIds() 
        {
            foreach (var id in selection)
            {
                yield return id.Key;
            }
        }

        public IEnumerable<string> GetSelectedIds<T>() where T : class, ISelectable
        {
            foreach (var id in selection)
            {
                if (id.Value is T)
                {
                    yield return id.Key;
                }
            }
        }

        public bool Select(string id)
        {
            foreach (var source in sources)
            {
                if (source.Invoke(id, out var selectable))
                {
                    if (!selection.TryAdd(id, selectable))
                    {
                        return false;
                    }
                    selectable.Selected = true;
                    lastSelection = selectable;
                    return true;
                }
            }
            return false;
        }
        
        public bool Deselect(string id)
        {
            if (!selection.TryGetValue(id, out var selectable))
            {
                return false;
            }
            selectable.Selected = false;
            selection.Remove(id);
            if (lastSelection == selectable)
            {
                lastSelection = null;
            }
            return false;
        }

        
        public int GetSelectedCount<T>() where T : ISelectable
        {
            int count = 0;
            foreach (var id in selection)
            {
                if (id.Value is T)
                {
                    count++;
                }
            }
            return count;
        }
        
        public void AppendSelection<T>(T selectable) where T : class, ISelectable
        {
            var old = GetSelectedIds<T>().ToArray();
            var newSelection = new string[old.Length + 1];
            for (int i = 0; i < old.Length; i++)
            {
                newSelection[i] = old[i];
            }
            newSelection[^1] = selectable.ID; 
            history.ExecuteCommand(new ChangeSelectionCommand(this, old, newSelection));
        }
        
           
        public void ClearSelection<T>() where T : class, ISelectable
        {
            var ids = GetSelectedIds<T>().ToArray();
            if (ids.Length == 0)
            {
                return;
            }
            history.ExecuteCommand(new ChangeSelectionCommand(this, ids, 
                Array.Empty<string>()));
        }
        
        public void ClearSelection()
        {
            if (selection.Count == 0)
            {
                return;
            }
            history.ExecuteCommand(new ChangeSelectionCommand(this, GetSelectedIds().ToArray(), 
                Array.Empty<string>()));
        }
        
        public void SelectSingle<T>(T selectable) where T : class, ISelectable
        {
            var newSelection = new [] {selectable.ID};
            history.ExecuteCommand(new ChangeSelectionCommand(this, 
                GetSelectedIds<T>().ToArray(), newSelection));
        }
        
        public void RemoveSelection<T>(T selectable) where T : class, ISelectable
        {
            var old = GetSelectedIds<T>().ToArray();
            var newSelection = new List<string>(old);
            newSelection.Remove(selectable.ID);
            history.ExecuteCommand(new ChangeSelectionCommand(this, old, newSelection.ToArray()));
        }

        public void HandleSelection<T>(T hit) where T : class, ISelectable
        {
            if (hit.ID == lastSelection?.ID)
            {
                return;
            }

            if (selection.ContainsKey(hit.ID))
            {
                if (Input.GetMouseButtonUp(0))
                {
                    RemoveSelection(hit);
                }
            
                return;
            }
            
            if (Input.GetKey(KeyCode.LeftCommand))
            {
                AppendSelection(hit);
                return;
            }
            
            SelectSingle(hit);
        }
    }
}