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
            controller.transform.position = Vector3.Lerp(player.transform.position, new Vector3(player.lanePositions[player.currentLane],
            player.transform.position.y,
            player.transform.position.z), 0.1f);
        }

        if ((Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) && player.currentLane < player.lanePositions.Length - 1 && !player.isChangingLane)
        {
            player.StartCoroutine(player.ChangeLane(player.currentLane + 1, 0.5f, 0, 0.1f));                    
        }
        else if ((Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) && player.currentLane > 0 && !player.isChangingLane)
        {
            player.StartCoroutine(player.ChangeLane(player.currentLane - 1, 0.5f, 0, 0.1f));                      
        }
        else if((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow)) && controller.isGrounded && !player.isChangingLane)
        {
            stateMachine.ChangeState(player.jumpState);
        }
        else if ((Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) && controller.isGrounded && !player.isChangingLane)
        {
            stateMachine.ChangeState(player.slideState);
        }

      
    }
}
