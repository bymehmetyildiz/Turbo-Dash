using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState
{
    protected StateMachine stateMachine;
    protected Player player;
    protected string animBoolName;
    protected CharacterController controller;

    protected float xInput;
    

    protected float stateTimer;
    protected float distanceTimer;
    protected bool triggerCalled;

    public PlayerState(StateMachine stateMachine, string animBoolName, Player player, CharacterController controller)
    {
        this.stateMachine = stateMachine;
        this.animBoolName = animBoolName;
        this.player = player;
        this.controller = controller;
    }

    public virtual void Enter()
    {
        player.anim.SetBool(animBoolName, true);
        triggerCalled = false;
    }

    public virtual void Update()
    {
        stateTimer -= Time.deltaTime;
        distanceTimer -= Time.deltaTime;
    }

    public virtual void Exit()
    {
        player.anim.SetBool(animBoolName, false);
    }

    public virtual void AnimationTrigger()
    {
        triggerCalled = true;
    }
}
