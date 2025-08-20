using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class JumpState : PlayerState
{
  

    public JumpState(StateMachine stateMachine, string animBoolName, Player player, CharacterController controller)
        : base(stateMachine, animBoolName, player, controller)
    {
    }

    public override void Enter()
    {
        base.Enter();       
        player.verticalVelocity = Mathf.Sqrt(player.jumpHeight * player.gravity);
        stateTimer = 0.5f; // Duration of the jump state
      
    }

    public override void Exit()
    {
        base.Exit();
        
    }

    public override void Update()
    {
        base.Update();

        // Apply gravity each frame
        player.verticalVelocity += player.gravity * Time.deltaTime;

        // Move player (forward + vertical)
        player.moveDirection = Vector3.forward * player.moveSpeed + Vector3.up * player.verticalVelocity;
        controller.Move(player.moveDirection * Time.deltaTime);

        // When grounded, go back to MoveState
        if (controller.isGrounded)
        {
            player.verticalVelocity = -5f; // Keeps player grounded
            stateMachine.ChangeState(player.moveState);
        }
        else if (Input.GetKeyDown(KeyCode.D) && player.currentLane < player.lanePositions.Length - 1 && !player.isChangingLane)
        {
            player.StartCoroutine(player.ChangeLane(player.currentLane + 1));            
        }
        else if (Input.GetKeyDown(KeyCode.A) && player.currentLane > 0 && !player.isChangingLane)
        {
            player.StartCoroutine(player.ChangeLane(player.currentLane - 1));            
        }
        else if (stateTimer <= 0)
        {
            stateMachine.ChangeState(player.airState);
        }
        
    }
}

