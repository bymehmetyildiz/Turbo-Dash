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

        player.moveDirection = new Vector3(0, player.verticalVelocity, player.moveSpeed);
        controller.Move(player.moveDirection * Time.deltaTime);

        if (!player.isChangingLane && player.transform.position.x != player.lanePositions[player.currentLane])
        {
            controller.transform.position = Vector3.Lerp(
                player.transform.position,
                new Vector3(player.lanePositions[player.currentLane], player.transform.position.y, player.transform.position.z),
                0.1f);
        }

        if (player.TryLaneChange(0.5f, 0, 0.1f, true))
            return;

        if (player.isChangingLane || !controller.isGrounded)
            return;

        // ---- JUMP ----
        if (player.ConsumeInput(BufferedAction.Jump))
        {
            stateMachine.ChangeState(player.jumpState);
            AudioManager.instance.PlaySound(2);
            AudioManager.instance.PlaySound(3);
        }
        // ---- SLIDE ----
        else if (player.ConsumeInput(BufferedAction.Slide))
        {
            stateMachine.ChangeState(player.slideState);
            AudioManager.instance.PlaySound(5);
        }
    }
}
