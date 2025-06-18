using System.Collections.Generic;
using UnityEngine;

namespace Uriel.Behaviours
{
    public interface ISelectable
    {
        string ID { get; }
        bool Selected { get; set; }
        Bounds Bounds { get; }
    }
    
    public interface IModifiable
    {
        string ID { get; }
    }
    
    public interface ICommand
    {
        void Execute();
        void Undo();
    }

    public class CommandHistory
    {
        private readonly Stack<ICommand> undoStack = new Stack<ICommand>();
        private readonly Stack<ICommand> redoStack = new Stack<ICommand>();
        private int maxHistorySize = 50;

        public event System.Action<bool, bool> OnHistoryChanged; // canUndo, canRedo
        
        public void ExecuteCommand(ICommand command)
        {
            command.Execute();
            undoStack.Push(command);
            redoStack.Clear();
            
            // Limit history size
            if (undoStack.Count > maxHistorySize)
            {
                var tempStack = new Stack<ICommand>();
                for (int i = 0; i < maxHistorySize - 1; i++)
                {
                    tempStack.Push(undoStack.Pop());
                }
                undoStack.Clear();
                while (tempStack.Count > 0)
                {
                    undoStack.Push(tempStack.Pop());
                }
            }
            
            OnHistoryChanged?.Invoke(CanUndo, CanRedo);
        }

        public void Undo()
        {
            if (CanUndo)
            {
                var command = undoStack.Pop();
                command.Undo();
                redoStack.Push(command);
                OnHistoryChanged?.Invoke(CanUndo, CanRedo);
            }
        }

        public void Redo()
        {
            if (CanRedo)
            {
                var command = redoStack.Pop();
                command.Execute();
                undoStack.Push(command);
                OnHistoryChanged?.Invoke(CanUndo, CanRedo);
            }
        }

        public bool CanUndo => undoStack.Count > 0;
        public bool CanRedo => redoStack.Count > 0;
        
        public void Clear()
        {
            undoStack.Clear();
            redoStack.Clear();
            OnHistoryChanged?.Invoke(false, false);
        }
    }

}