
using UnityEngine;
using UnityEngine.UIElements;
using Uriel.Behaviours;

namespace Uriel.UI
{
    public class CameraControls
    {
        private CameraController controller;
        private const float ROTATION_STEP = 45f;
        
        public CameraControls(UIDocument document)
        {
            controller = Object.FindFirstObjectByType<CameraController>();
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

            // Existing mode button
            var modeBtn = root.Q<Button>("Mode");
            modeBtn.RegisterCallback<ClickEvent>(_ =>
            {
                controller.ToggleMode();
                modeBtn.text = controller.IsPerspective ? "P" : "0";
            });

            // X-axis rotation buttons
            var rotateXUpBtn = root.Q<Button>("RotateXUp");
            var rotateXDownBtn = root.Q<Button>("RotateXDown");
            
            // Y-axis rotation buttons
            var rotateYLeftBtn = root.Q<Button>("RotateYLeft");
            var rotateYRightBtn = root.Q<Button>("RotateYRight");

            // Register X-axis rotation events
            if (rotateXUpBtn != null)
            {
                rotateXUpBtn.RegisterCallback<ClickEvent>(_ =>
                {
                    controller.RotateAroundX(ROTATION_STEP);
                });
            }
            else
            {
                Debug.LogWarning("RotateXUp button not found in UI");
            }

            if (rotateXDownBtn != null)
            {
                rotateXDownBtn.RegisterCallback<ClickEvent>(_ =>
                {
                    controller.RotateAroundX(-ROTATION_STEP);
                });
            }
            else
            {
                Debug.LogWarning("RotateXDown button not found in UI");
            }

            // Register Y-axis rotation events
            if (rotateYLeftBtn != null)
            {
                rotateYLeftBtn.RegisterCallback<ClickEvent>(_ =>
                {
                    controller.RotateAroundY(-ROTATION_STEP);
                });
            }
            else
            {
                Debug.LogWarning("RotateYLeft button not found in UI");
            }

            if (rotateYRightBtn != null)
            {
                rotateYRightBtn.RegisterCallback<ClickEvent>(_ =>
                {
                    controller.RotateAroundY(ROTATION_STEP);
                });
            }
            else
            {
                Debug.LogWarning("RotateYRight button not found in UI");
            }
        }
    }
}
