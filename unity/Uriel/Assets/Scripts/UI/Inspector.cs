using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Uriel.Behaviours;
using Uriel.Commands;

namespace Uriel.UI
{
    [Flags]
    public enum PointerEventType
    {
        BeginEdit = 1,
        Editing = 2,
        EndEdit = 4
    }
    public class Inspector<TSnapshot> where TSnapshot : class, ISnapshot
    {
        private readonly List<IModifiable> modifiables = new();

        private Label idLabel;
        private ModifyCommand<TSnapshot> command;
        protected VisualElement Root;
        protected Studio Studio;
        private Button applyButton;

        public bool IsOpen => Root.visible;
        private readonly Dictionary<BindableElement, bool> valueTracker = new();

        protected bool IsModified
        {
            get => modified;
            set
            {
                if (applyButton != null)
                {
                    applyButton.SetEnabled(value);
                }
                modified = value;
            }
        }

        protected bool modified;
        
        private void OnValueChanged<T>(ChangeEvent<T> _, bool immediate, PointerEventType type)
        {
            if (applyButton != null && !immediate)
            {
                IsModified = true;
                applyButton?.SetEnabled(true);
            }
            else
            {
                ApplyChanges(type);
            }
        }
        
        public T RegisterField<T, T1>(string name, bool continuous, bool immediate) where T :  BaseField<T1>
        {
            var field = Root.Q<T>(name);
            ListenToField(field,  immediate, continuous);
            return field;
        }

        public void ListenToField<T>(BaseField<T> field, bool immediate, bool continuous)
        {
            if (continuous)
            {
                field?.RegisterCallback<PointerCaptureEvent>(_ =>
                {
                    OnValueChanged(new ChangeEvent<T>(), immediate, PointerEventType.BeginEdit);
                });
                field?.RegisterCallback<PointerCaptureOutEvent>(_ =>
                {
                    OnValueChanged(new ChangeEvent<T>(), immediate, PointerEventType.EndEdit);
                });
                field?.RegisterCallback<FocusOutEvent>(_ =>
                {
                    OnValueChanged(new ChangeEvent<T>(), immediate, PointerEventType.EndEdit);
                });
                field?.RegisterValueChangedCallback(evt =>
                {
                    OnValueChanged(evt, immediate, PointerEventType.Editing);
                });
            }
            else
            {
                field?.RegisterValueChangedCallback(evt =>
                {
                    OnValueChanged(evt, immediate, PointerEventType.BeginEdit | PointerEventType.EndEdit | PointerEventType.Editing);
                });
            }
        }
        
        public void HandleSelection<T>() where T : class, IModifiable
        {
            var count = Studio.Selector.GetSelectedCount<T>();
            if (count == 0)
            {
                Close();
                return;
            }
            Open();
            Set(Studio.Selector.GetSelected<T>());
        }
        
        public void Open()
        {
            Root.visible = true;
            OnShow();
        }

        public void Close()
        {
            Root.visible = false;
            OnHide();
        }

        protected virtual void OnShow() {}
        protected virtual void OnHide() {}
        
        protected void ApplyChanges(PointerEventType eventType)
        {
            if (eventType.HasFlag(PointerEventType.BeginEdit) && command == null)
            {
                command = new ModifyCommand<TSnapshot>(Studio, modifiables);
            }

            if (eventType.HasFlag(PointerEventType.Editing) && command == null)
            {
                command = new ModifyCommand<TSnapshot>(Studio, modifiables);
            }
            
            if (command != null)
            {
                OnApplyChanges();
            }
           
            if (eventType.HasFlag(PointerEventType.EndEdit) && command != null)
            {
                if (command.ApplyModifications(modifiables))
                {
                    Studio.CommandHistory.ExecuteCommand(command);
                }
                command = null;
            }
        }
        
        protected virtual void OnClearUI() {}

        private void ClearUI()
        {
            IsModified = false;
            OnClearUI();
        }
        
        public void Set(IEnumerable<IModifiable> list)
        {
            modifiables.Clear();
            modifiables.AddRange(list);
            IsModified = false;
            if (modifiables.Count == 0)
            {
                ClearUI();
                return;
            }
            var labelText = "";
            for (int i = 0; i < modifiables.Count; i++)
            {
                labelText += modifiables[i].ID;
                if (i < modifiables.Count - 1)
                {
                    labelText += ", ";
                }
            }

            idLabel.text = labelText;
            var config = modifiables[0].Current;
            UpdateUI(config);
        }
        
        protected virtual void OnApplyChanges() {}
        
        private void OnUndo()
        {
            if (!IsOpen)
            {
                return;
            }

            if (modifiables == null || modifiables.Count == 0)
            {
                return;
            }
            
            var snapshot = modifiables[0].Current;
            UpdateUI(snapshot);
        }

        protected virtual void UpdateUI(ISnapshot snapshot) {}

        protected IEnumerable<T1> GetInspected<T1>() where T1 : class, IModifiable
        {
            foreach (var modifiable in modifiables)
            {
                yield return modifiable as T1;
            }
        }
        
        public Inspector(string name, Studio studio, UIDocument ui)
        {
            Studio = studio;
            Root = ui.rootVisualElement.Q($"{name}Inspector");
            Studio.CommandHistory.OnUndoOrRedo += OnUndo;
            idLabel = Root.Q<Label>("Id");
            applyButton = Root.Q<Button>("Apply");
            applyButton?.RegisterCallback<ClickEvent>(_ =>
            {
                if (IsModified)
                {
                    ApplyChanges(PointerEventType.BeginEdit | PointerEventType.EndEdit);
                    IsModified = false;
                }
            });
            IsModified = false;
        }
    }
}