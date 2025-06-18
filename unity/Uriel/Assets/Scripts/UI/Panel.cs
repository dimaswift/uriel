using UnityEngine;
using UnityEngine.UIElements;

namespace Uriel.UI
{
    public class Panel
    {
        protected VisualElement Root;

        public void Show()
        {
            Root.visible = true;
            OnShow();
        }

        public void Hide()
        {
            Root.visible = false;
            OnHide();
        }

        protected virtual void OnShow() {}
        protected virtual void OnHide() {}
        
        public Panel(UIDocument ui, string name, string openButton)
        {
            Root = ui.rootVisualElement.Q<VisualElement>(name);
            if (Root == null)
            {
                Debug.LogWarning($"UI Error: '{name}' panel not found inside {ui.name}");
                return;
            }
            if (openButton != null)
            {
                ui.rootVisualElement.Q("ToolBar").Q<Button>(openButton).RegisterCallback<ClickEvent>(_ => Show());
            }
            Root.Q<Button>("Close")?.RegisterCallback<ClickEvent>(_ => Hide());
        }
    }
}