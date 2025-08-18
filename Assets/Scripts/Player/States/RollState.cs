using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollState : GroundedState
{
    public RollState(StateMachine stateMachine, string animBoolName, Player player, CharacterController controller) : base(stateMachine, animBoolName, player, controller)
    {
    }

    public override void Enter()
    {
        base.Enter();
        controller.center = new Vector3(0, 0.1f, 0);
        controller.height = 0.1f;
        controller.radius = 0.1f;
    }

    public override void Exit()
    {
        base.Exit();

        controller.center = new Vector3(0, 0.9f, 0);
        controller.height = 1.4f;
        controller.radius = 0.3f;  
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

        player.moveDirection = Vector3.forward * player.moveSpeed;

        controller.Move(player.moveDirection * Time.deltaTime);

        if (triggerCalled)
            stateMachine.ChangeState(player.moveState);
    }
}
