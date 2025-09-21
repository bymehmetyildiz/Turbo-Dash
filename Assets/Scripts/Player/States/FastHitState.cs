using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FastHitState : PlayerState
{
    public FastHitState(StateMachine stateMachine, string animBoolName, Player player, CharacterController controller) : base(stateMachine, animBoolName, player, controller)
    {
    }

    public override void Enter()
    {
        base.Enter();   
        
        player.transform.rotation = Quaternion.Euler(0f, 0f, 0f);    
        UIManager.instance.UpdateScoreBoard(player.distanceTraveled);
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        if (controller.isGrounded)
        {
            player.verticalVelocity = -1f;
        }
        else
        {
            player.verticalVelocity += player.gravity * Time.deltaTime;
        }

        player.moveDirection = Vector3.up * player.verticalVelocity;

        controller.Move(player.moveDirection * Time.deltaTime);
    }
}
