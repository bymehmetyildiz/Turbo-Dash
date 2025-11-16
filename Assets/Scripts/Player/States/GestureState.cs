using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GestureState : PlayerState
{   
    public GestureState(StateMachine stateMachine, string animBoolName, Player player, CharacterController controller) : base(stateMachine, animBoolName, player, controller)
    {
    }

    public override void Enter()
    {
        base.Enter();
        player.anim.applyRootMotion = true;
        player.virtualCamera.Follow = null;
        controller.center = new Vector3(0, -0.7f, 0);
        controller.height = 0.1f;
        controller.radius = 0.1f;
        player.anim.SetInteger("DanceIndex", player.danceIndex);
       

    }

    public override void Exit()
    {
        base.Exit();
        player.anim.applyRootMotion = false;   
        player.virtualCamera.Follow = player.transform;
        player.transform.position = new Vector3(0, player.transform.position.y, player.transform.position.z);
        controller.center = new Vector3(0, 0, 0);
        controller.height = 1.5f;
        controller.radius = 0.25f;        

        GestureManager.instance.currentGesture.isPlaying = false;
        GestureManager.instance.currentGesture.audioSource.Stop();
        GestureManager.instance.currentGesture.gesturePriceText.text = "Play";
        GestureManager.instance.currentGesture = null;
    }

    public override void Update()
    {
        base.Update();

        if (triggerCalled)
            stateMachine.ChangeState(player.idleState);



    }
}
