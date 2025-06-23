using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Uriel.Behaviours
{
    public class AxisGizmo : MonoBehaviour
    {
        public Axis? SelectedAxis;
        [SerializeField] private Collider xAxis, yAxis, zAxis;
        [SerializeField] private Collider xyz;

        private readonly RaycastHit[] hitBuffer = new RaycastHit[8];
        private Camera cam;

        private void Awake()
        {
            cam = Camera.main;
        }

        private void FixedUpdate()
        {
            var ray = cam.ScreenPointToRay(Input.mousePosition);
            var hits = Physics.RaycastNonAlloc(ray, hitBuffer);
            for (int i = 0; i < hits; i++)
            {
                var hit = hitBuffer[i];
                if (hit.collider == xyz)
                {
                    SelectedAxis = Axis.XYZ;
                    return;
                }
                if (hit.collider == xAxis)
                {
                    SelectedAxis = Axis.X;
                    return;
                }
                if (hit.collider == yAxis)
                {
                    SelectedAxis = Axis.Y;
                    return;
                }
                if (hit.collider == zAxis)
                {
                    SelectedAxis = Axis.Z;
                    return;
                }
            }
            SelectedAxis = null;
        }
    }

    public enum Axis
    {
        X,Y,Z,XYZ
    }
}