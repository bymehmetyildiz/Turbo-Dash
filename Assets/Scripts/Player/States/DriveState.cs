using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DriveState : GroundedState
{
    public DriveState(StateMachine stateMachine, string animBoolName, Player player, CharacterController controller) : base(stateMachine, animBoolName, player, controller)
    {
    }

    public override void Enter()
    {
        base.Enter();
        player.car.SetActive(true);
        
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        // Forward movement only
        player.moveDirection = Vector3.forward * player.moveSpeed;

        // Force player to stay aligned with drive position on X/Y
        Vector3 newPos = controller.transform.position;
        newPos.x = player.drivePosition.x;
        newPos.y = player.drivePosition.y;
        newPos.z = player.moveSpeed;
        controller.transform.position = newPos;

    }
}
