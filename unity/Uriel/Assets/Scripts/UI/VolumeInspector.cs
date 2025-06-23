using System.Collections.Generic;
using UnityEngine;
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
        private ListView solidList;

        private readonly List<string> solidBuffer = new();
        
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
            
            solidBuffer.Clear();
            var vol = Studio.Find<Volume>(snapshot.ID);
            foreach (var solid in vol.GetComponentsInChildren<SculptSolidBehaviour>())
            {
                solidBuffer.Add(solid.ID);
            }
            solidList.SetSelection(-1);
            solidList.Rebuild();
        }

        private VisualElement MakeSolid()
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
        
        private void SetupUI()
        {
            budgetField = RegisterField<IntegerField, int>("Budget");
            shellField = RegisterField<Slider, float>("Shell");
            flipNormalsToggle = RegisterField<Toggle, bool>("FlipNormals");
            invertTrianglesToggle = RegisterField<Toggle, bool>("InvertTriangles");
            shrinkField = RegisterField<Slider, float>("Shrink");
            solidList = Root.Q<ListView>("Solids");
            solidList.itemsSource = solidBuffer;
            solidList.selectionType = SelectionType.Single;
            solidList.makeItem = MakeSolid;
            solidList.bindItem = (element, i) =>
            {
                element.Q<Label>().text = solidBuffer[i];
            };
            solidList.itemsChosen += c =>
            {
                Studio.Selector.SelectSingle(solidList.selectedItem.ToString());
            };
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
