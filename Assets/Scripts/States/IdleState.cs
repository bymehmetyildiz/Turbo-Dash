using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : GroundedState
{
    public IdleState(StateMachine stateMachine, string animBoolName, Player player, CharacterController controller) : base(stateMachine, animBoolName, player, controller)
    {
    }

    public override void Enter()
    {
        base.Enter();
        player.transform.rotation = Quaternion.Euler(0, 180, 0); // Reset rotation to face forward
        player.transform.position = new Vector3(0, player.transform.position.y, player.transform.position.z); // Reset position to origin
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        if(player.isStarted && controller.isGrounded)
        {
            stateMachine.ChangeState(player.turnState);
        }

        if (controller.isGrounded)
        {
            player.verticalVelocity = -1f; // small push down to keep grounded            
        }
        else
        {
            player.verticalVelocity += player.gravity * Time.deltaTime; // falling
        }

        player.moveDirection = Vector3.up * player.verticalVelocity; // apply vertical velocity to move direction

        controller.Move(player.moveDirection * Time.deltaTime);
    }
}
