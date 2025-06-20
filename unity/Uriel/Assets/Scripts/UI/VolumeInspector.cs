using UnityEngine.UIElements;
using Uriel.Behaviours;
using Uriel.Commands;
using Uriel.Domain;

namespace Uriel.UI
{
    public class VolumeInspector : Inspector<VolumeSnapshot>
    {
        private IntegerField budgetField;
        private Slider shellField;
        private Toggle flipNormalsToggle;
        private Toggle invertTrianglesToggle;
        private FloatField shrinkField;

        private VisualElement configContainer;
        private ModifyCommand<VolumeSnapshot> command;
        
        public VolumeInspector(UIDocument ui, Studio studio) : base("Volume", studio, ui)
        {
            SetupUI();
           
        }

        protected override void UpdateUI(ISnapshot snapshot)
        {
            var volCfg = snapshot as VolumeSnapshot;
            if (volCfg == null) return;
            budgetField.SetValueWithoutNotify(volCfg.marchingCubes.budget);
            shellField.SetValueWithoutNotify(volCfg.marchingCubes.shell);
            flipNormalsToggle.SetValueWithoutNotify(volCfg.marchingCubes.flipNormals);
            invertTrianglesToggle.SetValueWithoutNotify(volCfg.marchingCubes.invertTriangles);
            shrinkField.SetValueWithoutNotify(volCfg.marchingCubes.shrink);
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
            
            AddField(budgetField, true);
            AddField(shellField, true);
            AddField(flipNormalsToggle, true);
            AddField(invertTrianglesToggle, true);
            AddField(shrinkField, true);
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
            configContainer.Add(buttonContainer);
        }
        
 
        protected override void OnApplyChanges()
        {
            var newConfig = GetConfigFromUI();
            foreach (var volume in GetInspected<Volume>())
            {
                var snapshot = volume.Current as VolumeSnapshot;
                snapshot.marchingCubes = newConfig;
            }
        }
    }
}
