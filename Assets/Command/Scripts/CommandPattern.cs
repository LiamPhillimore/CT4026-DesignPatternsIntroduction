using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class CommandPattern : MonoBehaviour
{
    [SerializeField]
    private GameObject target = null;
    [SerializeField]
    private int undoRecordLimit; // how many undo actions can be recorded

    // History stack for handing undo
    private Stack<ICommand> undoStack;
    // History stack for handling redos
    private Stack<ICommand> redoStack;

    private void Start()
    {
        undoStack = new Stack<ICommand>();
        redoStack = new Stack<ICommand>();
    }

    private void Update()
    {
        // Add commands if keys are pressed
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Debug.Log("Up key pressed");
            Execute(new ForwardCommand());
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Execute(new BackwardCommand());
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Execute(new LeftCommand());
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Execute(new RightCommand());
        }

        // Undo the action if undo is pressed
        if (Input.GetKeyDown(KeyCode.U))
        {
            Undo();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Redo();
        }
    }

    /// <summary>
    /// Executes a command, adds it to history and includes a target
    /// </summary>
    /// <param name="command"></param>
    private void Execute(ICommand command)
    {
        // Inject target gameobject into the command (if it requires one)
        if (command is TargetCommand)
        {
            (command as TargetCommand).target = target;
        }

        // Exectute command
        command.Execute();
        // Add it to the stack, so we can undo
        undoStack.Push(command);
        /*if (undoStack.Count > undoRecordLimit) // if the redo stack exceeds the maximum size
        {
            undoStack = RemoveStackElement(undoStack, undoStack.Count); // remove the last element of the stack
        } */
    }

    private void Undo()
    {
        // Only if there are items in the stack
        if (undoStack.Count > 0)
        {
            redoStack.Push(undoStack.Peek()); // stores the undone command in the redo stack
            /*if (redoStack.Count > undoRecordLimit) // if the redo stack exceeds the maximum size
            {
                redoStack = RemoveStackElement(redoStack, redoStack.Count); // remove the last element of the stack
            } */
            // Get and remove the last item from the stack
            ICommand command = undoStack.Pop();
            // Undo the command
            command.Undo();
        }
    }

    private void Redo()
    {
        if (redoStack.Count > 0) // if there are items in stack
        {
            undoStack.Push(redoStack.Peek()); // stores the redone command back into the undo stack
            /*if (undoStack.Count > undoRecordLimit) // if the redo stack exceeds the maximum size
            {
                undoStack = RemoveStackElement(undoStack, undoStack.Count); // remove the last element of the stack
            } */
            ICommand command = redoStack.Pop(); // get and remove the last undone command from the stack
            command.Execute(); // executes the command, redoing it
        }
    }

    // method to remove a particular element of a stack using a for loop to go through each element of the stack, placing it into a new stack or removing it if it is the element to be removed.
    /*private Stack<ICommand> tempStack;
    private Stack<ICommand> RemoveStackElement(Stack<ICommand> stack, int element)
    { 

        tempStack = new Stack<ICommand>();
        for(int i = 0; i < stack.Count + 1; i++)
        {
            if (stack.Count == stack.Count - element + 1) // if element is the one to be removed, removes it
            {
                stack.Pop();
            }
            else
            {
                tempStack.Push(stack.Pop()); // else, move element to tempoary stack
            }
        }
        return tempStack;
    } */
}

// ICommand is the basic layout of our commands
// It defines methods that need to be in the class if implemented
// Unlike inheritence, you can implement many interfaces
//
// Eg. One class, extending base class, implementing 2 interfaces:
//
// public class MyClass : BaseClass, IInterface1, IInterface2


public interface ICommand
{

    void Execute();
    void Undo();
}

// Target Command stores the target gameboject that we will wish to 
// perform execute and undos upon
// defines two abstract functions for Execute and undo, which child
// classes must implement or pass on if they themselves are abstract
public abstract class TargetCommand : ICommand
{
    // Target for the command to be effected onto
    public GameObject target;

    public abstract void Execute();
    public abstract void Undo();
}


/// <summary>
/// By adding this middle layer to the inheritance heirarchy, I avoid having
/// to retype all this movement code that's mostly the same between directions.
/// 
/// All that I have to add to each individual direction is the correct vector.
/// </summary>
public abstract class MoveCommand : TargetCommand
{
    //We will need to know where we were prior to moving, in order to undo
    Vector3 oldPosition;

    /// <summary>
    /// Store the object we've acted on, and the oldposition, then
    /// move the object by the supplied movement vector
    /// </summary>
    /// <param name="Mobile">GameObject to act upon.</param>
    /// <param name="Movement">Movement vector.</param>
    protected virtual void Move(Vector3 Movement)
    {
        oldPosition = target.transform.position;
        target.transform.position = oldPosition + Movement;
    }

    /// <summary>
    /// This is just passed  on through for the child classes to implement
    /// as this class is also abstract.
    /// </summary>
    public override abstract void Execute();

    /// <summary>
    /// Reverse the action taken
    /// </summary>
    public override void Undo()
    {
        target.transform.position = oldPosition;
    }
}

// For each of these, because undo is already overridden in MoveCommand,
// they do not need to override that. They have access to "Move" because it
// is in MoveCommand and is protected, so they can call that function in
// their implementation of Execute.

public class ForwardCommand : MoveCommand
{
    public override void Execute()
    {
        Move(new Vector3(0.0f, 0.0f, 1.0f));
    }
}

public class BackwardCommand : MoveCommand
{
    public override void Execute()
    {
        Move(new Vector3(0.0f, 0.0f, -1.0f));
    }
}

public class LeftCommand : MoveCommand
{
    public override void Execute()
    {
        Move(new Vector3(-1.0f, 0.0f, 0.0f));
    }
}

public class RightCommand : MoveCommand
{
    public override void Execute()
    {
        Move(new Vector3(1.0f, 0.0f, 0.0f));
    }
}
