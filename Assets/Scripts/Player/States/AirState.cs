using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirState : PlayerState
{
    public AirState(StateMachine stateMachine, string animBoolName, Player player, CharacterController controller) : base(stateMachine, animBoolName, player, controller)
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
        
        player.verticalVelocity += player.gravity * Time.deltaTime * 10;        

        player.moveDirection = Vector3.up * player.verticalVelocity + Vector3.forward * player.moveSpeed;

        controller.Move(player.moveDirection * Time.deltaTime);
        
        if (Input.GetKeyDown(KeyCode.D) && player.currentLane < player.lanePositions.Length - 1 && !player.isChangingLane)
        {
            player.StartCoroutine(player.ChangeLane(player.currentLane + 1));
        }
        else if (Input.GetKeyDown(KeyCode.A) && player.currentLane > 0 && !player.isChangingLane)
        {
            player.StartCoroutine(player.ChangeLane(player.currentLane - 1));
        }

        if (controller.isGrounded)
            stateMachine.ChangeState(player.moveState);
    }
}
