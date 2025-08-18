using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveState : GroundedState
{
    public MoveState(StateMachine stateMachine, string animBoolName, Player player, CharacterController controller) : base(stateMachine, animBoolName, player, controller)
    {
    }

    public override void Enter()
    {
        base.Enter();
        player.transform.rotation = Quaternion.Euler(0, 0, 0); // Reset rotation to face forward
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

       

        player.moveDirection = Vector3.up * player.verticalVelocity + Vector3.forward * player.moveSpeed;

        controller.Move(player.moveDirection * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.D) && player.currentLane < player.lanePositions.Length - 1 && !player.isChangingLane)
        {
            player.StartCoroutine(player.ChangeLane(player.currentLane + 1));
            stateMachine.ChangeState(player.changeLaneState);
        }
        else if (Input.GetKeyDown(KeyCode.A) && player.currentLane > 0 && !player.isChangingLane)
        {
            player.StartCoroutine(player.ChangeLane(player.currentLane - 1));
            stateMachine.ChangeState(player.changeLaneState);
        }
        else if(Input.GetKeyDown(KeyCode.Space) && controller.isGrounded && !player.isChangingLane)
        {
            stateMachine.ChangeState(player.jumpState);
        }
        else if (Input.GetKeyDown(KeyCode.S) && controller.isGrounded && !player.isChangingLane)
        {
            stateMachine.ChangeState(player.rollState);
        }

      
    }
}
