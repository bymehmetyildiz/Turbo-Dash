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
    }

    public override void Exit()
    {
        base.Exit();

        // Detach player from car
        player.transform.SetParent(null);
        player.DestroyCar();

        player.transform.position = new Vector3(0, player.transform.position.y, player.transform.position.z);
        player.currentLane = 1;
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
