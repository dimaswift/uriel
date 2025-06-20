using UnityEngine.UIElements;
using Uriel.Behaviours;

namespace Uriel.UI
{
    public class SettingsPanel : StudioPanel
    {
        public SettingsPanel(UIDocument ui, Studio studio) 
            : base(studio, ui, "Settings", "ShowSettings")
        {
            var gridToggle = Root.Q<Toggle>("ShowGrid");
            gridToggle.SetValueWithoutNotify(studio.ShowGrid);
            studio.StateManager.OnStateLoaded += s =>
            {
                gridToggle.SetValueWithoutNotify(studio.ShowGrid);
            };
            gridToggle.RegisterValueChangedCallback(v =>
            {
                studio.ShowGrid = v.newValue;
            });
        }
    }
}