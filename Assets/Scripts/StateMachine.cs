using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
    public PlayerState currentstate { get; private set; }

    public void InitializeState(PlayerState startState)
    {
        currentstate = startState;
        currentstate.Enter();
    }

    public void ChangeState(PlayerState newState)
    {
        currentstate.Exit();
        currentstate = newState;
        currentstate.Enter();
    }
}
