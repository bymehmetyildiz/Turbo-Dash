using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneState : PlayerState
{
    private float planeHeight = 10;

    public PlaneState(StateMachine stateMachine, string animBoolName, Player player, CharacterController controller) : base(stateMachine, animBoolName, player, controller)
    {
    }

    public override void Enter()
    {
        base.Enter();
        stateTimer = 10f;
        player.plane.SetActive(true);
    }

    public override void Exit()
    {
        base.Exit();
        player.StartCoroutine(player.ActivateShield());
        player.plane.SetActive(false);
    }

    public override void Update()
    {
        base.Update();

        Quaternion targetRotation;

        if (player.transform.position.y < planeHeight)
        {
            player.moveDirection = Vector3.forward * player.moveSpeed * 3 + Vector3.up * planeHeight;
            targetRotation = Quaternion.Euler(-45, 0, 0); // Tilt down
        }
        else
        {
            player.moveDirection = Vector3.forward * player.moveSpeed * 3;
            targetRotation = Quaternion.Euler(0, 0, 0); // Straight forward
        }

        // Smoothly rotate toward target
        player.transform.rotation = Quaternion.Slerp(
            player.transform.rotation,
            targetRotation,
            10f * Time.deltaTime // <-- tweak speed until smooth
        );

        controller.Move(player.moveDirection * Time.deltaTime);

        if (!player.isChangingLane && player.transform.position.x != player.lanePositions[player.currentLane])
        {
            controller.transform.position = Vector3.Lerp(player.transform.position, new Vector3(player.lanePositions[player.currentLane],
            player.transform.position.y,
            player.transform.position.z), 0.1f);
        }

        // Lane change
        if (Input.GetKeyDown(KeyCode.D) && player.currentLane < player.lanePositions.Length - 1 && !player.isChangingLane)
        {
            player.StartCoroutine(player.ChangeLane(player.currentLane + 1, 0, -15, 0.3f));
        }
        else if (Input.GetKeyDown(KeyCode.A) && player.currentLane > 0 && !player.isChangingLane)
        {
            player.StartCoroutine(player.ChangeLane(player.currentLane - 1, 0, 15, 0.3f));
        }

        // Timer
        if (stateTimer <= 0f)
        {            
            stateMachine.ChangeState(player.airState);
        }
    }
}
