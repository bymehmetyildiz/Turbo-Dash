using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    public StateMachine stateMachine { get; private set; }
    public Animator anim;
    public CharacterController controller;

    // States
    public IdleState idleState;
    public MoveState moveState;
    public TurnState turnState;
    public ChangeLaneState changeLaneState;   
    public JumpState jumpState;
    public AirState airState;

    // Idle properties
    public bool isStarted;
    public float gravity = -9.81f;
    public float verticalVelocity;
    public bool triggerCalled;
    public Vector3 moveDirection;

    //Movement properties
    public float moveSpeed = 5f;    
    public bool isChangingLane = false;
    public float[] lanePositions = { -2.15f, 0f, 2.15f };
    public int currentLane = 1;

    private void Awake()
    {
        stateMachine = new StateMachine();

        idleState = new IdleState(stateMachine, "Idle", this, controller);
        moveState = new MoveState(stateMachine, "Move", this, controller);
        turnState = new TurnState(stateMachine, "Turn", this, controller);
        changeLaneState = new ChangeLaneState(stateMachine, "ChangeLane", this, controller);        
        jumpState = new JumpState(stateMachine, "Jump", this, controller);
        airState = new AirState(stateMachine, "Fall", this, controller);
    }

    void Start()
    {
        stateMachine.InitializeState(idleState);
    }

    
    void Update()
    {
        stateMachine.currentstate.Update();

        if(Input.GetKeyDown(KeyCode.Q) && !isStarted)
        {
            isStarted = true;
        }
    }

    //Check if Turn Animation Ended
    public void TriggerCalled()
    {
        if(stateMachine.currentstate == turnState)
            turnState.AnimationTrigger();
        else if(stateMachine.currentstate == changeLaneState)
            changeLaneState.AnimationTrigger();

    }
    

    // Turn
    public IEnumerator Turn()
    {        
            Quaternion startRotation = transform.rotation;
            Quaternion endRotation = startRotation * Quaternion.Euler(0, -180, 0);
            float duration = 0.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                transform.rotation = Quaternion.Slerp(startRotation, endRotation, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.rotation = endRotation;        
    }

    public IEnumerator ChangeLane(int targetLane)
    {
        isChangingLane = true;

        Vector3 startPosition = transform.position;
        Vector3 endPosition = new Vector3(lanePositions[targetLane], transform.position.y, transform.position.z);

        float duration = 0.25f; // slightly longer for the hop
        float elapsed = 0f;

        float hopHeight = 0.6f; // how high the hop goes

        while (elapsed < duration)
        {
            float t = Mathf.Clamp01(elapsed / duration);

            // Horizontal movement
            float xPos = Mathf.Lerp(startPosition.x, endPosition.x, t);

            // Vertical hop arc (parabola)
            float yOffset = hopHeight * Mathf.Sin(t * Mathf.PI);

            transform.position = new Vector3(
                xPos,
                startPosition.y + yOffset,
                transform.position.z
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Snap to final lane position
        transform.position = new Vector3(endPosition.x, endPosition.y, transform.position.z);

        currentLane = targetLane;
        isChangingLane = false;
    }

    

}
