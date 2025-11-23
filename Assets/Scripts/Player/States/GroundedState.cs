using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundedState : PlayerState
{
    public GroundedState(StateMachine stateMachine, string animBoolName, Player player, CharacterController controller) : base(stateMachine, animBoolName, player, controller)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        if (controller.isGrounded)
        {
            player.verticalVelocity = -1f;
        }
        else
        {
            player.verticalVelocity += player.gravity * Time.deltaTime;
        }
        //if (Input.GetKeyDown(KeyCode.J) && !player.isChangingLane && player.isStarted)
        //{
        //    stateMachine.ChangeState(player.jetState);
        //    AudioManager.instance.PlaySound(20);
        //}
        //else if (Input.GetKeyDown(KeyCode.K) && !player.isChangingLane && player.isStarted)
        //{
        //    stateMachine.ChangeState(player.driveState);
        //}
        //else if (Input.GetKeyDown(KeyCode.H) && !player.isChangingLane && player.isStarted)
        //{
        //    stateMachine.ChangeState(player.planeState);
        //}
        //else if (Input.GetKeyDown(KeyCode.T) && !player.isChangingLane && player.isStarted)
        //{
        //    stateMachine.ChangeState(player.tankState);
        //}

    }
}
