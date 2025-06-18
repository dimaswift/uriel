using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Uriel.Behaviours;
using Uriel.Commands;

namespace Uriel.UI
{
    public class VolumeInspector : Inspector
    {
        private Volume[] volumes;
        private Label idLabel;
        
        // UI Elements for MarchingCubesConfig
        private IntegerField budgetField;
        private Slider shellField;
        private Toggle flipNormalsToggle;
        private Toggle invertTrianglesToggle;
        private FloatField shrinkField;
        
        // Control elements
        private Button resetButton;
        private VisualElement configContainer;
        private ChangeVolumesCommand command;
        
        public VolumeInspector(UIDocument ui, VolumeStudio studio) : base("Volume", studio, ui)
        {
            idLabel = Root.Q<Label>("Id");
            SetupUI();
            BindUIEvents();
            Studio.CommandHistory.OnUndoOrRedo += OnUndo;
        }

        private void OnUndo()
        {
            if (!IsOpen)
            {
                return;
            }

            if (volumes == null || volumes.Length == 0)
            {
                return;
            }
            
            var config = volumes[0].Snapshot.marchingCubes;
            UpdateUIFromConfig(config);
        }

        private void SetupUI()
        {
            configContainer = Root.Q<VisualElement>("ConfigContainer");
            if (configContainer == null)
            {
                configContainer = new VisualElement();
                configContainer.name = "ConfigContainer";
                Root.Add(configContainer);
            }
            
            CreateConfigUI();
            CreateControlButtons();
        }
        
        private void CreateConfigUI()
        {
            // Budget field
            var budgetContainer = CreateFieldContainer("Budget");
            budgetField = new IntegerField();
            budgetField.name = "BudgetField";
            budgetField.value = MarchingCubesConfig.Default.budget;
            budgetField.tooltip = "Maximum number of triangles to generate";
            budgetContainer.Add(budgetField);
            
            // Shell field
            var shellContainer = CreateFieldContainer("Shell");
            shellField = new Slider(0, 0.1f);
            shellField.name = "ShellField";
            
            shellField.tooltip = "Shell thickness for surface generation";
            shellContainer.Add(shellField);
            
            // Flip Normals toggle
            var flipNormalsContainer = CreateFieldContainer("Flip Normals");
            flipNormalsToggle = new Toggle();
            flipNormalsToggle.name = "FlipNormalsToggle";
            flipNormalsToggle.value = MarchingCubesConfig.Default.flipNormals;
            flipNormalsToggle.tooltip = "Flip the normal direction of generated triangles";
            flipNormalsContainer.Add(flipNormalsToggle);
            
            // Invert Triangles toggle
            var invertTrianglesContainer = CreateFieldContainer("Invert Triangles");
            invertTrianglesToggle = new Toggle();
            invertTrianglesToggle.name = "InvertTrianglesToggle";
            invertTrianglesToggle.value = MarchingCubesConfig.Default.invertTriangles;
            invertTrianglesToggle.tooltip = "Invert triangle winding order";
            invertTrianglesContainer.Add(invertTrianglesToggle);
            
            // Shrink field
            var shrinkContainer = CreateFieldContainer("Shrink");
            shrinkField = new FloatField();
            shrinkField.name = "ShrinkField";
            shrinkField.value = MarchingCubesConfig.Default.shrink;
            shrinkField.tooltip = "Shrink factor for the generated mesh";
            shrinkContainer.Add(shrinkField);
        }
        
        private VisualElement CreateFieldContainer(string labelText)
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.alignItems = Align.Center;
            container.style.marginBottom = 5;
            container.style.paddingLeft = 5;
            container.style.paddingRight = 5;
            container.AddToClassList("inspector");
            var label = new Label(labelText);
            label.style.width = 120;
            label.style.minWidth = 120;
            
            container.Add(label);
            configContainer.Add(container);
            
            return container;
        }
        
        private void CreateControlButtons()
        {
            var buttonContainer = new VisualElement();
            buttonContainer.style.flexDirection = FlexDirection.Row;
            buttonContainer.style.marginTop = 10;
            buttonContainer.style.justifyContent = Justify.SpaceBetween;

            resetButton = new Button(ResetToDefaults);
            resetButton.text = "Reset to Defaults";
            resetButton.style.flexGrow = 1;
            resetButton.style.marginRight = 5;
            
            buttonContainer.Add(resetButton);
            configContainer.Add(buttonContainer);
        }
        
        private void BindUIEvents()
        {
            budgetField.RegisterValueChangedCallback(OnConfigChanged);
            shellField.RegisterValueChangedCallback(OnConfigChanged);
            flipNormalsToggle.RegisterValueChangedCallback(OnConfigChanged);
            invertTrianglesToggle.RegisterValueChangedCallback(OnConfigChanged);
            shrinkField.RegisterValueChangedCallback(OnConfigChanged);
        }

        private void OnConfigChanged<T>(ChangeEvent<T> evt)
        {
            ApplyChanges();
        }
        
        public void SetVolumes(Volume[] volumesList)
        {
            volumes = volumesList;
            
            if (volumes.Length == 0)
            {
                ClearUI();
                return;
            }
            var labelText = "";
            for (int i = 0; i < volumes.Length; i++)
            {
                labelText += volumes[i].ID;
                if (i < volumes.Length - 1)
                {
                    labelText += ", ";
                }
            }

            idLabel.text = labelText;
            var config = volumes[0].Snapshot.marchingCubes;
            UpdateUIFromConfig(config);
            SetEnabled(true);
        }
        
        private void UpdateUIFromConfig(MarchingCubesConfig config)
        {
            budgetField.SetValueWithoutNotify(config.budget);
            shellField.SetValueWithoutNotify(config.shell);
            flipNormalsToggle.SetValueWithoutNotify(config.flipNormals);
            invertTrianglesToggle.SetValueWithoutNotify(config.invertTriangles);
            shrinkField.SetValueWithoutNotify(config.shrink);
        }
        
        private void SetEnabled(bool enabled)
        {
            budgetField.SetEnabled(enabled);
            shellField.SetEnabled(enabled);
            flipNormalsToggle.SetEnabled(enabled);
            invertTrianglesToggle.SetEnabled(enabled);
            shrinkField.SetEnabled(enabled);
            resetButton.SetEnabled(enabled);
        }
        
        private MarchingCubesConfig GetConfigFromUI()
        {
            return new MarchingCubesConfig
            {
                budget = budgetField.value,
                shell = shellField.value,
                flipNormals = flipNormalsToggle.value,
                invertTriangles = invertTrianglesToggle.value,
                shrink = shrinkField.value,
            };
        }
        
        private void ResetToDefaults()
        {
            var defaultConfig = MarchingCubesConfig.Default;
            UpdateUIFromConfig(defaultConfig);
            ApplyChanges();
        }
        
        private void ApplyChanges()
        {
            command = new ChangeVolumesCommand(Studio, volumes);
            var newConfig = GetConfigFromUI();
            foreach (var volume in volumes)
            {
                volume.Snapshot.marchingCubes = newConfig;
            }
            command.SaveNewStates(volumes);
            Studio.CommandHistory.ExecuteCommand(command);
        }
        
        private void ClearUI()
        {
            UpdateUIFromConfig(MarchingCubesConfig.Default);
        }
    }
}
