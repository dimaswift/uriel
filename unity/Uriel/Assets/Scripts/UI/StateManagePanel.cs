using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Uriel.Behaviours;

namespace Uriel.UI
{
    public class StateManagePanel : StudioPanel
    {
        private readonly ListView list;
        private readonly List<string> files = new();

        private void RefreshFileList()
        {
            files.Clear();
            files.AddRange(Studio.StateManager.ListFiles());
            list.Rebuild();
        }
        
        protected override void OnShow()
        {
            RefreshFileList();
            base.OnShow();
        }
        
        public StateManagePanel(UIDocument ui, VolumeStudio studio) 
            : base(studio, ui, "Projects", "ShowProjects")
        {
            Studio.StateManager.OnStateLoaded += s =>
            {
                ui.rootVisualElement.Q<Label>("ProjectName").text = Studio.StateManager.Current.name;
            };
            list = Root.Q<ListView>("Files");
            list.selectionType = SelectionType.Single;
            list.makeItem = MakeItem;
            list.bindItem = (element, i) =>
            {
                element.Q<Label>().text = files[i];
            };
            var nameField = Root.Q<TextField>("NameField");
            var loadBtn = Root.Q<Button>("Load");
            var saveBtn = Root.Q<Button>("Save");
            var deleteBtn = Root.Q<Button>("Delete");
            
            deleteBtn.RegisterCallback<ClickEvent>(evt =>
            {
                if (list.selectedItem != null)
                {
                    Studio.StateManager.Delete(list.selectedItem as string);
                    RefreshFileList();
                }
            });
            
            Root.Q<Button>("New").RegisterCallback<ClickEvent>(_ =>
            {
                var state = Studio.StateManager.CreateNew();
                nameField.value = state.name;
                Hide();
            });
            
            loadBtn.RegisterCallback<ClickEvent>(_ =>
            {
                if (list.selectedItem != null)
                {
                    Studio.StateManager.LoadState(list.selectedItem as string);
                    Hide();
                }
               
            });
            
            saveBtn.RegisterCallback<ClickEvent>(_ =>
            {
                Studio.StateManager.SaveState(nameField.value);
                Hide();
            });

            loadBtn.SetEnabled(false);
            deleteBtn.SetEnabled(false);
            list.itemsSource = files;
            list.selectionChanged += s =>
            {
                loadBtn.SetEnabled(list.selectedItem != null);
                deleteBtn.SetEnabled(list.selectedItem != null);
                nameField.value = list.selectedItem as string;
            };
            list.itemsChosen += s =>
            {
                loadBtn.SetEnabled(list.selectedItem != null);
                deleteBtn.SetEnabled(list.selectedItem != null);
            };
            saveBtn.SetEnabled(false);
            nameField.RegisterCallback<ChangeEvent<string>>(s =>
            {
                saveBtn.SetEnabled(!string.IsNullOrWhiteSpace(s.newValue));
            });
        }

        private VisualElement MakeItem()
        {
            var itemContainer = new VisualElement();
            itemContainer.style.flexDirection = FlexDirection.Row;
            itemContainer.style.alignItems = Align.Center;
            itemContainer.style.paddingLeft = 10;
            itemContainer.style.paddingRight = 10;
            itemContainer.style.paddingTop = 5;
            itemContainer.style.paddingBottom = 5;

            var label = new Label();
            label.name = "Label";
            label.style.flexGrow = 1;
            label.style.unityTextAlign = TextAnchor.MiddleLeft;

            itemContainer.Add(label);
            
            return itemContainer;
        }
    }
}