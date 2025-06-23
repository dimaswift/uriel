using System.Collections.Generic;
using UnityEngine;
using Uriel.Commands;

namespace Uriel.Behaviours
{
    public interface IScalable : IModifiable
    {
        public Vector3 Scale { get; set; }
    }
    public class ScaleHandle : Handle<IScalable>
    {
        private readonly Dictionary<IScalable, Vector3> scaleClickPoints = new();

        private ScaleCommand command;

        protected override void OnFinishDragging()
        {
            command.SaveModified();
            CommandHistory.ExecuteCommand(command);
            scaleClickPoints.Clear();
        }

        protected override void OnBeginDrag()
        {
            scaleClickPoints.Clear();
            foreach (var target in Targets)
            {
                scaleClickPoints.Add(target, target.Scale);
            }
            command = new ScaleCommand(Targets);
        }
        
        protected override void OnDrag(Vector3 delta, Axis axis)
        {
            foreach (var scalable in scaleClickPoints)
            {
                if (axis == Axis.XYZ)
                {
                    scalable.Key.Scale = scalable.Value + (delta.z * scalable.Value.normalized * 2);
                }
                else
                {
                    scalable.Key.Scale = scalable.Value + delta * 2;
                }
                
            }
        }

        public void ResetSelected()
        {
            command = new ScaleCommand(Targets);
            foreach (var target in Targets)
            {
                target.Scale = Vector3.one;
            }
            command.SaveModified();
            CommandHistory.ExecuteCommand(command);
        }
    }
}