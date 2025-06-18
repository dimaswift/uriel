using UnityEngine.UIElements;
using Uriel.Behaviours;

namespace Uriel.UI
{
    public class Inspector
    {
        protected VisualElement Root;
        protected VolumeStudio Studio;

        public bool IsOpen => Root.visible;
        
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
        
        public Inspector(string name, VolumeStudio studio, UIDocument ui)
        {
            Studio = studio;
            Root = ui.rootVisualElement.Q($"{name}Inspector");
        }
    }
}