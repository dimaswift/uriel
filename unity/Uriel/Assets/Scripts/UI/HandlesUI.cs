using UnityEngine.UIElements;
using Uriel.Behaviours;

namespace Uriel.UI
{
    public class HandlesUI
    {
        public HandlesUI(UIDocument document, Studio studio)
        {
            var root = document.rootVisualElement.Q("Handles");
            var moveBtn = root.Q<Button>("Move");
            var scaleBtn = root.Q<Button>("Scale");
            
            moveBtn.RegisterCallback<ClickEvent>(evt =>
            {
                studio.MoveHandle.Selected = true;
                studio.ScaleHandle.Selected = false;
                scaleBtn.RemoveFromClassList("selected");
                moveBtn.AddToClassList("selected");
            });
            
            scaleBtn.RegisterCallback<ClickEvent>(evt =>
            {
                studio.MoveHandle.Selected = false;
                studio.ScaleHandle.Selected = true;
                moveBtn.RemoveFromClassList("selected");
                scaleBtn.AddToClassList("selected");
            });
            
            root.Q<Button>("ResetScale").RegisterCallback<ClickEvent>(evt =>
            {
                studio.ScaleHandle.ResetSelected();
            });
            root.Q<Button>("ResetPosition").RegisterCallback<ClickEvent>(evt =>
            {
                studio.MoveHandle.ResetSelected();
            });
        }
    }
}