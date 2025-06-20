using System;
using System.Collections.Generic;
using UnityEngine;

namespace Uriel.Behaviours
{
    public interface ICommand
    {
        void Execute();
        void Undo();
    }

    public class CommandHistory
    {
        private readonly Stack<ICommand> undoStack = new ();
        private readonly Stack<ICommand> redoStack = new ();
        
        private const int MaxHistorySize = 100;

        public event Action OnHistoryChanged = () => {};
        public event Action OnUndo = () => {};
        public event Action OnRedo = () => {};
        public event Action OnUndoOrRedo = () => {};

        
        public void ExecuteCommand(ICommand command)
        {
            command.Execute();

            undoStack.Push(command);
            redoStack.Clear();
            
            if (undoStack.Count > MaxHistorySize)
            {
                var tempStack = new Stack<ICommand>();
                for (int i = 0; i < MaxHistorySize - 1; i++)
                {
                    tempStack.Push(undoStack.Pop());
                }
                undoStack.Clear();
                while (tempStack.Count > 0)
                {
                    undoStack.Push(tempStack.Pop());
                }
            }
            
            OnHistoryChanged();
        }

        public void Undo()
        {
            if (CanUndo)
            {
                var command = undoStack.Pop();
                command.Undo();
                redoStack.Push(command);
                OnHistoryChanged();
                OnUndo();
                OnUndoOrRedo();
            }
        }

        public void Redo()
        {
            if (CanRedo)
            {
                var command = redoStack.Pop();
                command.Execute();
                undoStack.Push(command);
                OnHistoryChanged();
                OnRedo();
                OnUndoOrRedo();
            }
        }

        public bool CanUndo => undoStack.Count > 0;
        public bool CanRedo => redoStack.Count > 0;
        
        public void Clear()
        {
            undoStack.Clear();
            redoStack.Clear();
            OnHistoryChanged();
        }
    }

}