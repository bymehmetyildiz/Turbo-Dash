using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnState : GroundedState
{
    public TurnState(StateMachine stateMachine, string animBoolName, Player player, CharacterController controller) : base(stateMachine, animBoolName, player, controller)
    {
    }

    public override void Enter()
    {
        base.Enter();
        player.StartCoroutine(player.Turn());
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

       
        if (triggerCalled)
            stateMachine.ChangeState(player.moveState);
    }
}
