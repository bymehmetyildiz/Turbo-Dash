using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DriveState : PlayerState
{
    public DriveState(StateMachine stateMachine, string animBoolName, Player player, CharacterController controller) : base(stateMachine, animBoolName, player, controller)
    {
    }

    public override void Enter()
    {
        base.Enter();
        player.InstantiateCar();

        // Attach player to the seat
        player.transform.SetParent(player.car.GetComponent<CarController>().playerPosition);
        player.transform.localPosition = Vector3.zero; // snap exactly to seat

        stateTimer = 10f;
       
        controller.height = 0.1f;
        controller.radius = 0.1f;
    }

    public override void Exit()
    {
        base.Exit();

        // Detach player from car
        player.transform.SetParent(null);
        player.DestroyCar();

        player.currentLane = player.car.GetComponent<CarController>().currentLane;
        player.transform.position = new Vector3(player.lanePositions[player.currentLane], player.transform.position.y, player.transform.position.z);

        player.StartCoroutine(player.ActivateShield());
    
        controller.height = 1.5f;
        controller.radius = 0.15f;
    }

    public override void Update()
    {
        base.Update();

        if (player.car != null)
        {
            player.transform.position = player.car
                .GetComponent<CarController>()
                .playerPosition.position;
        }

        if (stateTimer <= 0f)
            stateMachine.ChangeState(player.airState);
    }
}
