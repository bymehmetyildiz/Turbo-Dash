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
        player.verticalVelocity = Mathf.Sqrt(player.jumpHeight * player.gravity * player.gravityScale);
        stateTimer = 0.5f; // Duration of the jump state
        player.slideQueued = false;
      
    }

    public override void Exit()
    {
        base.Exit();
        
    }

    public override void Update()
    {
        base.Update();

        // Apply gravity each frame
        player.verticalVelocity += player.gravity * player.gravityScale * Time.deltaTime;

        // Move player (forward + vertical)
        player.moveDirection = Vector3.forward * player.moveSpeed + Vector3.up * player.verticalVelocity;
        controller.Move(player.moveDirection * Time.deltaTime);

        if (!player.isChangingLane && player.transform.position.x != player.lanePositions[player.currentLane])
        {
            controller.transform.position = Vector3.Lerp(player.transform.position, new Vector3(player.lanePositions[player.currentLane],
            player.transform.position.y,
            player.transform.position.z), 0.1f);
        }

        // When grounded, go back to MoveState (or straight into a queued slide)
        if (controller.isGrounded)
        {
            player.verticalVelocity = -5f; // Keeps player grounded
            if (player.slideQueued)
            {
                stateMachine.ChangeState(player.slideState);
                AudioManager.instance.PlaySound(5);
            }
            else
                stateMachine.ChangeState(player.moveState);
            return;
        }

        player.TryLaneChange(0, 0, 0.1f, true);

        // Swipe down in the air: drop fast and slide on landing
        if (player.ConsumeInput(BufferedAction.Slide))
        {
            player.verticalVelocity = Mathf.Min(player.verticalVelocity, -player.fastFallSpeed);
            player.slideQueued = true;
        }

        if (stateTimer <= 0)
            stateMachine.ChangeState(player.airState);
    }
}

