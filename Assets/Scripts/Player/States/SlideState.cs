using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideState : GroundedState
{
    public SlideState(StateMachine stateMachine, string animBoolName, Player player, CharacterController controller) : base(stateMachine, animBoolName, player, controller)
    {
    }

    public override void Enter()
    {
        base.Enter();
        controller.center = new Vector3(0, -0.7f, 0);
        controller.height = 0.1f;
        controller.radius = 0.1f;
        player.slideQueued = false;
    }

    public override void Exit()
    {
        base.Exit();

        controller.center = new Vector3(0, 0, 0);
        controller.height = 1.5f;
        controller.radius = 0.25f;        
    }

    public override void Update()
    {
        base.Update();

        if (controller.isGrounded)
        {
            // When grounded, apply a small downward force
            player.verticalVelocity = -1f;
        }
        else
        {
            // Apply gravity when in the air
            player.verticalVelocity += player.gravity * player.gravityScale * Time.deltaTime;
        }

        // Combine forward movement with vertical movement
        player.moveDirection = new Vector3(0, player.verticalVelocity, player.moveSpeed);

        // Pass the complete Vector3 to the Move method
        controller.Move(player.moveDirection * Time.deltaTime);

        // Lane changes are allowed while sliding
        player.TryLaneChange(0, 0, 0.1f, true);

        // Jump cancels the slide
        if (!player.isChangingLane && controller.isGrounded && player.ConsumeInput(BufferedAction.Jump))
        {
            stateMachine.ChangeState(player.jumpState);
            AudioManager.instance.PlaySound(2);
            AudioManager.instance.PlaySound(3);
            return;
        }

        if (triggerCalled)
        {
            stateMachine.ChangeState(player.moveState);
        }
    }
}
