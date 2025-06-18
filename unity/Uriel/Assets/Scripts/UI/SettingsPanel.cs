using System.Collections.Generic;
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
        
        public SettingsPanel(UIDocument ui, VolumeStudio studio) 
            : base(studio, ui, "Settings", "ShowSettings")
        {
            var xRes = Root.Q<DropdownField>("ResolutionX");
            xRes.value = studio.Config.resolution.x.ToString();
            xRes.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                if (int.TryParse(evt.newValue, out var x))
                {
                    studio.Config.resolution.x = x;
                    UpdateConfig();
                    studio.UpdateResolution();
                }
            });
            
            var yRes = Root.Q<DropdownField>("ResolutionY");
            yRes.value = studio.Config.resolution.y.ToString();
            yRes.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                if (int.TryParse(evt.newValue, out var y))
                {
                    studio.Config.resolution.y = y;
                    UpdateConfig();
                    studio.UpdateResolution();
                }
            });
            
            var zRes = Root.Q<DropdownField>("ResolutionZ");
            zRes.value = studio.Config.resolution.z.ToString();
            zRes.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                if (int.TryParse(evt.newValue, out var z))
                {
                    studio.Config.resolution.z = z;
                    UpdateConfig();
                    studio.UpdateResolution();
                }
            });

            var triangleBudget = Root.Q<SliderInt>("TriangleBudget");
            triangleBudget.value = studio.Config.triangleBudget;
            triangleBudget.RegisterCallback<ChangeEvent<int>>(evt =>
            {
                studio.Config.triangleBudget = evt.newValue;
                UpdateConfig();
                studio.UpdateResolution();
            });
          
            
        }
    }
}