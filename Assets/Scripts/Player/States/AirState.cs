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

        if ((Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) && player.currentLane < player.lanePositions.Length - 1 && !player.isChangingLane)
        {
            player.StartCoroutine(player.ChangeLane(player.currentLane + 1, 0, 0, 0.1f));
            AudioManager.instance.PlaySound(1);
            AudioManager.instance.PlaySound(6);
        }
        else if ((Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) && player.currentLane > 0 && !player.isChangingLane)
        {
            player.StartCoroutine(player.ChangeLane(player.currentLane - 1, 0, 0, 0.1f));
            AudioManager.instance.PlaySound(1);
            AudioManager.instance.PlaySound(6);
        }

        if (controller.isGrounded)
        {
            stateMachine.ChangeState(player.moveState);
            AudioManager.instance.PlaySound(4);
        }

    }
}
