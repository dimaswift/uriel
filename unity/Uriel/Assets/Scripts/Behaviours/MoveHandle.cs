using System.Collections.Generic;
using UnityEngine;
using Uriel.Commands;

namespace Uriel.Behaviours
{
    public interface IMovable : IModifiable
    {
        Vector3 Position { get; set; }
    }
    
    public class MoveHandle : Handle<IMovable>
    {
        private readonly Dictionary<IMovable, Vector3> moveClickPoints = new();

        private MoveCommand command;

        protected override void OnFinishDragging()
        {
            command.SaveModified();
            CommandHistory.ExecuteCommand(command);
            moveClickPoints.Clear();
        }

        protected override void OnBeginDrag()
        {
            moveClickPoints.Clear();
            foreach (var target in Targets)
            {
                moveClickPoints.Add(target, target.Position);
            }
            command = new MoveCommand(Targets);
        }
        
        protected override void OnDrag(Vector3 delta, Axis axis)
        {
            foreach (var movable in moveClickPoints)
            {
                movable.Key.Position = movable.Value + delta;
            }
            MoveGizmo(delta);
        }

        public void ResetSelected()
        {
            command = new MoveCommand(Targets);
            foreach (var movable in Targets)
            {
                movable.Position = Vector3.zero;
            }
            command.SaveModified();
            CommandHistory.ExecuteCommand(command);
            MoveGizmoToCenter();
        }
    }
}