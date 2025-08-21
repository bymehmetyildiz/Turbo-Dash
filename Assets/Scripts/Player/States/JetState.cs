using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JetState : PlayerState
{
    private float jetHeight = 5;

    public JetState(StateMachine stateMachine, string animBoolName, Player player, CharacterController controller)
        : base(stateMachine, animBoolName, player, controller)
    {
    }

    public override void Enter()
    {
        base.Enter();
        stateTimer = 10f;
        player.jetPack.SetActive(true);
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        Quaternion targetRotation;

        if (player.transform.position.y < jetHeight)
        {
            player.moveDirection = Vector3.forward * player.moveSpeed * 2 + Vector3.up * jetHeight;
            targetRotation = Quaternion.Euler(-45, 0, 0); // Tilt down
        }
        else
        {
            player.moveDirection = Vector3.forward * player.moveSpeed * 2;
            targetRotation = Quaternion.Euler(0, 0, 0); // Straight forward
        }

        // Smoothly rotate toward target
        player.transform.rotation = Quaternion.Slerp(
            player.transform.rotation,
            targetRotation,
            10f * Time.deltaTime // <-- tweak speed until smooth
        );

        controller.Move(player.moveDirection * Time.deltaTime);

        // Lane change
        if (Input.GetKeyDown(KeyCode.D) && player.currentLane < player.lanePositions.Length - 1 && !player.isChangingLane)
        {
            player.StartCoroutine(player.ChangeLane(player.currentLane + 1, 0));
        }
        else if (Input.GetKeyDown(KeyCode.A) && player.currentLane > 0 && !player.isChangingLane)
        {
            player.StartCoroutine(player.ChangeLane(player.currentLane - 1, 0));
        }

        // Timer
        if (stateTimer <= 0f)
        {
            player.jetPack.SetActive(false);
            stateMachine.ChangeState(player.airState);
        }
    }
}
