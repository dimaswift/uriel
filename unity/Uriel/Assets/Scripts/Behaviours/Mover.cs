using System;
using System.Collections.Generic;
using UnityEngine;
using Uriel.Commands;

namespace Uriel.Behaviours
{
    public interface IMovable : ISelectable
    {
        Vector3 position { get; set; }
    }

   
    
    public class Mover
    {
        public bool IsMoving => moving;
        public bool Enabled { get; set; } = true;

        private readonly CommandHistory commandHistory;
        private readonly Selector selector;
        private readonly Camera cam;
        private readonly List<IMovable> buffer = new();
        private readonly List<Func<bool>> blockers = new();
        
        public Mover(CommandHistory commandHistory, Selector selector)
        {
            cam = Camera.main;
            this.commandHistory = commandHistory;
            this.selector = selector;
            selector.AddBlocker(() => IsMoving);
        }
         
        private bool moving;
        private Vector3 moveStartPoint;
        private readonly Dictionary<IMovable, Vector3> moveClickPoints = new();

        private MoveCommand command;

        public void AddBlocker(Func<bool> shouldBlock)
        {
            blockers.Add(shouldBlock);
        }

        private bool IsBlocked()
        {
            foreach (var blocker in blockers)
            {
                if (blocker())
                {
                    return true;
                }
            }

            return false;
        }
        
        private void FinishMove()
        {
            command.SaveModified();
            commandHistory.ExecuteCommand(command);
            moving = false;
            moveClickPoints.Clear();
        }
        
        private Vector3 GetPlanePointer()
        {
            var ray = cam.ScreenPointToRay(Input.mousePosition);
            var plane = new Plane(Vector3.up, Vector3.zero);
            if (plane.Raycast(ray, out var dist))
            {
                return ray.GetPoint(dist);
            }

            return Vector3.zero;
        }

        private void BeginMove()
        {
            if (!Enabled || IsBlocked())
            {
                return;
            }
            moveClickPoints.Clear();
            buffer.Clear();
            
            foreach (var movable in selector.GetSelected<IMovable>())
            {
                moveClickPoints.Add(movable, movable.position);
                buffer.Add(movable);
            }
            moving = selector.GetSelectedCount<IMovable>() > 0;
            moveStartPoint = GetPlanePointer();
            command = new MoveCommand(buffer);
        }

        public void Reset()
        {
            moving = false;
            moveClickPoints.Clear();
        }

        public void Update()
        {
            if (!IsMoving && Input.GetMouseButton(0) && Input.mousePositionDelta.magnitude > 0.1f)
            {
                BeginMove();
            }
            
            if (Input.GetMouseButtonUp(0))
            {
                if (moving)
                {
                    FinishMove();
                }
            }

            if (IsMoving)
            {
                var mouse = GetPlanePointer();
                var mouseDelta = mouse - moveStartPoint;
                foreach (var movable in moveClickPoints)
                {
                    movable.Key.position = movable.Value + mouseDelta;
                }
            }
          
        }
        
    }
}