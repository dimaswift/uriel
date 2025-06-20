using UnityEngine.UIElements;
using Uriel.Behaviours;

namespace Uriel.UI
{
    public class SettingsPanel : StudioPanel
    {
        private void UpdateConfig()
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(Studio.Config);
#endif
        }
        
        public SettingsPanel(UIDocument ui, Studio studio) 
            : base(studio, ui, "Settings", "ShowSettings")
        {
            
            var triangleBudget = Root.Q<SliderInt>("TriangleBudget");
            triangleBudget.value = studio.Config.triangleBudget;
            triangleBudget.RegisterCallback<ChangeEvent<int>>(evt =>
            {
                studio.Config.triangleBudget = evt.newValue;
                UpdateConfig();
            });
        }
    }
}