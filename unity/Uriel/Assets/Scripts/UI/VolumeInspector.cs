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
        private Slider shrinkField;

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
            budgetField = RegisterField<IntegerField, int>("Budget");
            shellField = RegisterField<Slider, float>("Shell");
            flipNormalsToggle = RegisterField<Toggle, bool>("FlipNormals");
            invertTrianglesToggle = RegisterField<Toggle, bool>("InvertTriangles");
            shrinkField = RegisterField<Slider, float>("Shrink");
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
