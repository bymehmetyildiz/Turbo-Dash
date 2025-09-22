using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
    public static Player instance;

    public StateMachine stateMachine { get; private set; }
    public Animator anim;   
    public CharacterController controller;

    // States
    public IdleState idleState;
    public MoveState moveState;
    public TurnState turnState;
    public JumpState jumpState;
    public AirState airState;
    public SlideState slideState;
    public FastHitState fastHitState;
    public HitState hitState;
    public JetState jetState;
    public DriveState driveState;
    public PlaneState planeState;

    public GestureState gestureState;

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
    public GameObject plane;

    [Header("Drive")]
    public GameObject[] vehiclePrefab;
    public GameObject activeVehicle;
    public int vehicleIndex;
    public int carIndex;
    public int colorIndex;


    [Header("Collectibles")]
    public int keys;

    [Header("Shield")]
    public GameObject shiledParticle;
    public bool isShielded = false;

    [Header("Coin")]
    public int currentCoinAmount;
    public int totalCoinAmount;

    [Header("Distance")]
    public int distanceTraveled;
    public int highScore;

    [Header("Camera")]
    public CinemachineVirtualCamera virtualCamera;
    public int danceIndex;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        stateMachine = new StateMachine();

        idleState = new IdleState(stateMachine, "Idle", this, controller);
        moveState = new MoveState(stateMachine, "Move", this, controller);
        turnState = new TurnState(stateMachine, "Turn", this, controller);               
        jumpState = new JumpState(stateMachine, "Jump", this, controller);
        airState = new AirState(stateMachine, "Fall", this, controller);
        slideState = new SlideState(stateMachine, "Roll", this, controller);
        fastHitState = new FastHitState(stateMachine, "Death", this, controller);
        hitState = new HitState(stateMachine, "Hit", this, controller);
        jetState = new JetState(stateMachine, "Fly", this, controller);
        driveState = new DriveState(stateMachine, "Drive", this, controller);
        planeState = new PlaneState(stateMachine, "Drive", this, controller);

        gestureState = new GestureState(stateMachine, "Dance", this, controller);
    }

    void Start()
    {
        stateMachine.InitializeState(idleState);
        jetPack.SetActive(false);
        shiledParticle.SetActive(false);
        plane.SetActive(false);
    }

    
    void Update()
    {
        stateMachine.currentstate.Update();

        if (isStarted)
        {
            moveSpeed += moveSpeed * Time.deltaTime * 0.001f;
            anim.speed += Time.deltaTime * 0.001f;
        }

        if(Input.GetKeyDown(KeyCode.V))
        {
            StartCoroutine(ActivateShield());
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
    public IEnumerator ChangeLane(int targetLane, float hopHeight, float angle, float duration)
    {
        isChangingLane = true;

        Vector3 startPosition = transform.position;
        Vector3 endPosition = new Vector3(lanePositions[targetLane], transform.position.y, transform.position.z);
        Quaternion startRotation = transform.rotation;
        Quaternion tiltRotation = Quaternion.Euler(0, 0, angle);        
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
                transform.position.y,
                transform.position.z
            );

            if(stateMachine.currentstate == jetState || stateMachine.currentstate == planeState)
            {
                // Rotation tilt forward, then back
                if (t < 0.5f)
                {
                    // First half: tilt to target angle
                    transform.rotation = Quaternion.Slerp(startRotation, tiltRotation, t * 2f);
                }
                else
                {
                    // Second half: tilt back to upright
                    transform.rotation = Quaternion.Slerp(tiltRotation, Quaternion.identity, (t - 0.5f) * 2f);
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }
        
        transform.position = new Vector3(endPosition.x, transform.position.y, transform.position.z);
        transform.rotation = Quaternion.identity;

        currentLane = targetLane;
        isChangingLane = false;
    }

    // Collider
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Obstacles obstacle = hit.gameObject.GetComponent<Obstacles>();

        if (obstacle != null)
        {
            if (isShielded)
                return;
           
            if (obstacle.obstacleType == ObstacleType.Explosive)
            {
                if (isShielded)
                    return; // ignore collision when shielded

                stateMachine.ChangeState(fastHitState);
                StartCoroutine(DeathBounce());
                isStarted = false;
                jetPack.SetActive(false);
                obstacle.anim.SetBool("Attack", true);
                obstacle.Explode();
            }
            else if (stateMachine.currentstate == jetState)
            {
                stateMachine.ChangeState(fastHitState);
                StartCoroutine(DeathBounce());
                isStarted = false;
                jetPack.SetActive(false);
                obstacle.PushRigidBodies(hit.point, 20f, 10f);
            }
            else
            {
                stateMachine.ChangeState(hitState);
                isStarted = false;
                obstacle.ActivateRigidbodies();
            }
        }

        Projectile projectile = hit.gameObject.GetComponent<Projectile>();
        if (projectile != null)
        {
            if (isShielded)
                return;

            stateMachine.ChangeState(fastHitState);
            StartCoroutine(DeathBounce());
            isStarted = false;
            jetPack.SetActive(false);
        }


    }
    private void OnTriggerEnter(Collider other)
    {
        Coin coin = other.GetComponent<Coin>();

        if (coin != null)
        {
            UIManager.instance.MoveCoinImg(coin.transform.position);
            Destroy(coin.gameObject);
        }
    }

    //Instantiate Car
    public void InstantiateCar()
    {
        activeVehicle = Instantiate(vehiclePrefab[vehicleIndex],
        new Vector3(lanePositions[currentLane], 0, transform.position.z), Quaternion.identity);
        activeVehicle.GetComponent<VehicleController>().SetupCar(carIndex, colorIndex);
    }

    public void DestroyCar() => Destroy(activeVehicle);

    //Shield Mode
    public IEnumerator ActivateShield()
    {
        isShielded = true;
        controller.excludeLayers = LayerMask.GetMask("Obstacle");
        shiledParticle.SetActive(true);
        yield return new WaitForSeconds(3f);
        shiledParticle.SetActive(false);
        controller.excludeLayers = 0;
        isShielded = false;
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

    //Distance Counter
    public void Distance()
    {
        distanceTraveled++;
        UIManager.instance.distanceText.text = NumberFormatter.FormatNumber(distanceTraveled) + " m";
    }
    


}

