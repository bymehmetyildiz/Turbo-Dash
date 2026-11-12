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

        player.verticalVelocity += player.gravity * player.gravityScale * Time.deltaTime;

        player.moveDirection = Vector3.up * player.verticalVelocity + Vector3.forward * player.moveSpeed;

        controller.Move(player.moveDirection * Time.deltaTime);

        if (!player.isChangingLane && player.transform.position.x != player.lanePositions[player.currentLane])
        {
            controller.transform.position = Vector3.Lerp(player.transform.position, new Vector3(player.lanePositions[player.currentLane],
            player.transform.position.y,
            player.transform.position.z), 0.1f);
        }

        player.TryLaneChange(0, 0, 0.1f, true);

        // Swipe down in the air: drop fast and slide on landing
        if (player.ConsumeInput(BufferedAction.Slide))
        {
            player.verticalVelocity = Mathf.Min(player.verticalVelocity, -player.fastFallSpeed);
            player.slideQueued = true;
        }

        if (controller.isGrounded)
        {
            AudioManager.instance.PlaySound(4);
            if (player.slideQueued)
            {
                stateMachine.ChangeState(player.slideState);
                AudioManager.instance.PlaySound(5);
            }
            else
                stateMachine.ChangeState(player.moveState);
        }
    }
}
