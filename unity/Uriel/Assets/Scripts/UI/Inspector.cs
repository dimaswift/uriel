using System.Collections.Generic;
using UnityEngine.UIElements;
using Uriel.Behaviours;
using Uriel.Commands;

namespace Uriel.UI
{
    public class Inspector<TSnapshot> where TSnapshot : class, ISnapshot
    {
        private readonly List<IModifiable> modifiables = new();

        private Label idLabel;
        private ModifyCommand<TSnapshot> command;
        protected VisualElement Root;
        protected Studio Studio;
        private Button applyButton;

        public bool IsOpen => Root.visible;

        private bool isModified;
        
        private void OnValueChanged<T>(ChangeEvent<T> _, bool immediate)
        {
            if (applyButton != null && !immediate)
            {
                isModified = true;
                applyButton?.SetEnabled(true);
            }
            else
            {
                ApplyChanges();
            }
        }

        public void AddField<T>(BaseField<T> field, bool immediate)
        {
            field.RegisterValueChangedCallback(evt =>
            {
                OnValueChanged(evt, immediate);
            });
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
        
        protected void ApplyChanges()
        {
            command = new ModifyCommand<TSnapshot>(Studio, modifiables);
            OnApplyChanges();
            command.ApplyModifications(modifiables);
            Studio.CommandHistory.ExecuteCommand(command);
        }
        
        protected virtual void OnClearUI() {}

        private void ClearUI()
        {
            
        }
        
        public void Set(IEnumerable<IModifiable> list)
        {
            modifiables.Clear();
            modifiables.AddRange(list);
            
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
                if (isModified)
                {
                    ApplyChanges();
                    isModified = false;
                    applyButton.SetEnabled(false);
                }
            });
            applyButton?.SetEnabled(false);
        }
    }
}