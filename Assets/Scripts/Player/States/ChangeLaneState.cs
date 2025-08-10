using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeLaneState : PlayerState
{   
    public ChangeLaneState(StateMachine stateMachine, string animBoolName, Player player, CharacterController controller) : base(stateMachine, animBoolName, player, controller)
    {
    }

    public override void Enter()
    {
        base.Enter();
        stateTimer = 1.0f;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        // Only apply forward movement — don't touch X or Y because ChangeLane coroutine controls them
        Vector3 forwardMove = Vector3.forward * player.moveSpeed * Time.deltaTime;
        controller.Move(forwardMove);

        if (triggerCalled)
        {
            stateMachine.ChangeState(player.moveState);
        }
    }
}

