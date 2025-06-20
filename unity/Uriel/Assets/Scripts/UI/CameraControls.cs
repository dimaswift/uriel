using UnityEngine;
using UnityEngine.UIElements;
using Uriel.Behaviours;

namespace Uriel.UI
{
    public class CameraControls
    {
        public CameraControls(UIDocument document)
        {
            var controller = Object.FindFirstObjectByType<CameraController>();
            if (!controller)
            {
                Debug.LogError("Camera Controller not found");
                return;
            }

            var root = document.rootVisualElement.Q("CameraControls");

            if (root == null)
            {
                Debug.LogError($"CameraControls container is missing in {document}");
                return;
            }

            var modeBtn = root.Q<Button>("Mode");
            modeBtn.RegisterCallback<ClickEvent>(_ =>
            {
                controller.ToggleMode();
                modeBtn.text = controller.IsPerspective ? "P" : "0";
            });
            
            root.Q<Button>("Snap").RegisterCallback<ClickEvent>(evt =>
            {
                controller.SnapToClosestDiscreteView();
            });
        }
    }
}
