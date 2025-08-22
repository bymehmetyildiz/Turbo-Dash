using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
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
    public SlideState slideState;
    public FastHitState fastHitState;
    public HitState hitState;
    public JetState jetState;
    public DriveState driveState;

    [Header("Idle")]
    public bool isStarted;
    public float gravity = -9.81f;
    public float gravityScale = 2f;
    public float verticalVelocity;
    public bool triggerCalled;
    public Vector3 moveDirection;

    [Header("Move")]
    public float moveSpeed = 5f;    
    public bool isChangingLane = false;
    public float[] lanePositions = { -1.2f, 0f, 1.2f };
    public int currentLane = 1;
    public float jumpHeight = -10f;

    [Header("Fly")]
    public GameObject jetPack;

    [Header("Drive")]
    public GameObject car;
    public Vector3 drivePosition;  

    [Header("Collectibles")]
    public int keys;

    private void Awake()
    {
        stateMachine = new StateMachine();

        idleState = new IdleState(stateMachine, "Idle", this, controller);
        moveState = new MoveState(stateMachine, "Move", this, controller);
        turnState = new TurnState(stateMachine, "Turn", this, controller);
        changeLaneState = new ChangeLaneState(stateMachine, "ChangeLane", this, controller);        
        jumpState = new JumpState(stateMachine, "Jump", this, controller);
        airState = new AirState(stateMachine, "Fall", this, controller);
        slideState = new SlideState(stateMachine, "Roll", this, controller);
        fastHitState = new FastHitState(stateMachine, "Death", this, controller);
        hitState = new HitState(stateMachine, "Hit", this, controller);
        jetState = new JetState(stateMachine, "Fly", this, controller);
        driveState = new DriveState(stateMachine, "Drive", this, controller);
    }

    void Start()
    {
        stateMachine.InitializeState(idleState);
        jetPack.SetActive(false);
        car.SetActive(false);
    }

    
    void Update()
    {
        stateMachine.currentstate.Update();

        if(Input.GetKeyDown(KeyCode.Q) && !isStarted)
        {
            isStarted = true;
        }

        if (isStarted)
        {
            moveSpeed += moveSpeed * Time.deltaTime * 0.001f;
            anim.speed += Time.deltaTime * 0.001f;
        }

    }

    //Check if Turn Animation Ended
    public void TriggerCalled() => stateMachine.currentstate.AnimationTrigger();

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

    // Change Lane with a hop
    public IEnumerator ChangeLane(int targetLane, float hopHeight)
    {
        isChangingLane = true;

        Vector3 startPosition = transform.position;
        Vector3 endPosition = new Vector3(lanePositions[targetLane], transform.position.y, transform.position.z);

        float duration = 0.2f; // slightly longer for the hop
        float elapsed = 0f;
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

    // Collider
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Vehicle vehicle = hit.gameObject.GetComponent<Vehicle>();

        if (vehicle != null)
        {            
            stateMachine.ChangeState(fastHitState);
            isStarted = false;
            Vehicle[] vehicles = FindObjectsOfType<Vehicle>();
            foreach (Vehicle v in vehicles)
            {
                v.speed = 0;
            }
            StartCoroutine(DeathBounce());
        }

        Obstacles obstacle = hit.gameObject.GetComponent<Obstacles>();

        if (obstacle != null)
        {
            if (stateMachine.currentstate == jetState)
            {
                stateMachine.ChangeState(fastHitState);
                StartCoroutine(DeathBounce());
                isStarted = false;
                jetPack.SetActive(false);
                obstacle.PushRigidBodies();
            }
            else
            {
                stateMachine.ChangeState(hitState);
                isStarted = false;
                obstacle.ActivateRigidbodies();
            }
                
        }

        
    }
    private void OnTriggerEnter(Collider other)
    {
        Doors door = other.GetComponent<Doors>();
        if (door != null)
        {
            if (keys > 0 && !door.isOpen)  // make a bool in Doors to track open state
            {
                keys--;
                StartCoroutine(door.OpenDoor());
                door.isOpen = true;
            }
            else if (!door.isOpen) // block player if no key and not open
            {
                stateMachine.ChangeState(hitState);
                isStarted = false;
            }
        }

        Collectible collectible = other.GetComponent<Collectible>();
        if(collectible != null)
        {
           collectible.Collect(this);
        }
    }

    //Death 
    public IEnumerator DeathBounce()
    {
        Vector3 startPosition = transform.position;
        Vector3 endPosition = transform.position + Vector3.back * 5;

        float duration = 1f; // slightly longer for the hop
        float elapsed = 0f;

        float hopHeight = 2.5f; // how high the hop goes

        while (elapsed < duration)
        {
            float t = Mathf.Clamp01(elapsed / duration);

            // Horizontal movement
            float zPos = Mathf.Lerp(startPosition.z, endPosition.z, t);

            // Vertical hop arc (parabola)
            float yOffset = hopHeight * Mathf.Sin(t * Mathf.PI);

            transform.position = new Vector3(
                transform.position.x,
                startPosition.y + yOffset,
                zPos
            );
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = endPosition;
    }

}
