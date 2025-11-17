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

        // ---- RIGHT ----
        if (UnifiedInput.MoveRight && player.currentLane < player.lanePositions.Length - 1 && !player.isChangingLane)
        {
            player.StartCoroutine(player.ChangeLane(player.currentLane + 1, 0.5f, 0, 0.1f));
            AudioManager.instance.PlaySound(1);
            AudioManager.instance.PlaySound(6);
        }
        // ---- LEFT ----
        else if (UnifiedInput.MoveLeft && player.currentLane > 0 && !player.isChangingLane)
        {
            player.StartCoroutine(player.ChangeLane(player.currentLane - 1, 0.5f, 0, 0.1f));
            AudioManager.instance.PlaySound(1);
            AudioManager.instance.PlaySound(6);
        }
        // ---- JUMP ----
        else if (UnifiedInput.Jump && controller.isGrounded && !player.isChangingLane)
        {
            stateMachine.ChangeState(player.jumpState);
            AudioManager.instance.PlaySound(2);
            AudioManager.instance.PlaySound(3);
        }
        // ---- SLIDE ----
        else if (UnifiedInput.Slide && controller.isGrounded && !player.isChangingLane)
        {
            stateMachine.ChangeState(player.slideState);
            AudioManager.instance.PlaySound(5);
        }
    }


}
