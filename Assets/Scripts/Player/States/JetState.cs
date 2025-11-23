using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JetState : PlayerState
{
    private float jetHeight = 10f;
    

    public JetState(StateMachine stateMachine, string animBoolName, Player player, CharacterController controller)
        : base(stateMachine, animBoolName, player, controller)
    {
    }

    public override void Enter()
    {
        base.Enter();
        stateTimer = player.jetDriveDur;
        player.jetPack.SetActive(true);
        player.lanePositions = new float[] { -3.5f, 0f, 3.5f };
        UIManager.instance.StartDriveStateCounter(stateTimer);
        AudioManager.instance.PlaySound(21);
    }

    public override void Exit()
    {
        base.Exit();
        player.StartCoroutine(player.ActivateShield());
        player.jetPack.SetActive(false);
        player.lanePositions = new float[] { -1.2f, 0f, 1.2f };
        UIManager.instance.StopDriveStateCounter();
        AudioManager.instance.StopSound(21);
        AudioManager.instance.PlaySound(22);
    }

    public override void Update()
    {
        base.Update();

        

        Quaternion targetRotation;

        if (player.transform.position.y < jetHeight)
        {
            player.moveDirection = Vector3.forward * player.moveSpeed * 3 + Vector3.up * jetHeight;
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
        if ((Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) && player.currentLane < player.lanePositions.Length - 1 && !player.isChangingLane)
        {
            player.StartCoroutine(player.ChangeLane(player.currentLane + 1, 0, -30, 0.3f));
        }
        else if ((Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) && player.currentLane > 0 && !player.isChangingLane)
        {
            player.StartCoroutine(player.ChangeLane(player.currentLane - 1, 0, 30, 0.3f));
        }

        // Timer
        if (stateTimer <= 0f)
        {            
            stateMachine.ChangeState(player.airState);
        }

        if (distanceTimer <= 0f)
        {
            distanceTimer = 0.25f;
            player.Distance();

        }

    }
}
