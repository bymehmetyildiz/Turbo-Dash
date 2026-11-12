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
    public TankState tankState;

    public GestureState gestureState;

    [Header("Idle")]
    public bool isStarted;
    public float gravity = -9.81f;
    public float gravityScale = 2f;
    public float verticalVelocity;
    public bool triggerCalled;
    public Vector3 moveDirection;

    [Header("Move")]
    public float moveSpeed = 15f;    
    public bool isChangingLane = false;
    public float[] lanePositions = { -1.2f, 0f, 1.2f };
    public int currentLane = 1;
    public float jumpHeight = -10f;


    [Header("Tank")]
    public float tankReloadDur = 3f; 
    public GameObject tankPrefab;
    public GameObject tank;

    [Header("Fly")]
    public GameObject jetPack;
    public GameObject plane;
    public float planeReloadDur = 3f;

    [Header("Drive")]
    public GameObject[] vehiclePrefab;
    public GameObject activeVehicle;
    public int vehicleIndex;
    public int carIndex;
    public int colorIndex;
    public float carDriveDur = 10f;
    public float jetDriveDur = 10f;
    public float tankDriveDur = 10f;
    public float planeFlyDur = 10f;

    [Header("Shield")]
    public GameObject shiledParticle;
    public bool isShielded = false;
    public float shieldDuration = 1.5f;

    [Header("Coin")]
    public int currentCoinAmount;
    public int totalCoinAmount;
    public ParticleSystem collectParticle;

    [Header("Fun Feel")]
    public int combo;
    public int bestCombo;
    public float comboTimeout = 1.6f;
    public float nearMissWindow = 0.35f;       // seconds before impact that still count as a late dodge
    public float nearMissCooldown = 0.35f;
    public int nearMissComboBonus = 5;
    public int comboPerBoost = 25;
    public float rewardSpeedBoost = 2f;
    public float rewardBoostDuration = 1.25f;
    private float comboTimer;
    private float nearMissTimer;
    private float boostTimer;

    [Header("Difficulty")]
    public float maxMoveSpeed = 42f;
    public float speedGainPerSecond = 0.085f;  // 25 -> 42 in ~200 seconds
    public float maxAnimSpeed = 1.25f;
    private float startMoveSpeed;

    [Header("Input Buffer")]
    public float inputBufferTime = 0.18f;
    public float fastFallSpeed = 25f;
    public bool slideQueued;
    private BufferedAction bufferedAction;
    private float bufferTimer;

    private float runStartZ;
    private bool runStartSet;

    [Header("Distance")]
    public int distanceTraveled;
    public int highScore;

    [Header("Camera")]
    public CinemachineVirtualCamera virtualCamera;
    public int danceIndex;

    // --- Optimization fields ---
    private int coinLayerMask;
    private Collider[] coinOverlapBuffer = new Collider[32];
    private ParticleSystem pooledCollectParticle;

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
        tankState = new TankState(stateMachine, "Drive", this, controller);

        gestureState = new GestureState(stateMachine, "Dance", this, controller);

        // cache layer mask to avoid calling GetMask every frame
        coinLayerMask = LayerMask.GetMask("Coin");
    }

    void Start()
    {
        stateMachine.InitializeState(idleState);
        jetPack.SetActive(false);
        shiledParticle.SetActive(false);
        plane.SetActive(false);
        startMoveSpeed = moveSpeed;

        SaveManager.instance.LoadGame();
    }

    
    void Update()
    {
        CaptureInput();

        stateMachine.currentstate.Update();

        if (isStarted)
        {
            // Speed ramps linearly up to a cap; the combo boost is tracked separately so it can never leak in.
            if (BaseMoveSpeed < maxMoveSpeed)
                moveSpeed += speedGainPerSecond * Time.deltaTime;

            anim.speed = Mathf.Lerp(1f, maxAnimSpeed, Difficulty01) * (boostTimer > 0f ? 1.08f : 1f);

            UpdateDistance();
        }

        TickFunFeel();
        CheckCoinOverlap();
    }

    // Speed without the temporary combo boost
    public float BaseMoveSpeed => moveSpeed - (boostTimer > 0f ? rewardSpeedBoost : 0f);

    // 0 at the start of a run, 1 once the speed cap is reached
    public float Difficulty01 => Mathf.InverseLerp(startMoveSpeed, maxMoveSpeed, BaseMoveSpeed);

    public bool IsOnFoot =>
        stateMachine.currentstate == moveState || stateMachine.currentstate == jumpState ||
        stateMachine.currentstate == airState || stateMachine.currentstate == slideState;

    // ---------------- Input buffer ----------------
    private void CaptureInput()
    {
        if (bufferTimer > 0f)
        {
            bufferTimer -= Time.unscaledDeltaTime;
            if (bufferTimer <= 0f)
                bufferedAction = BufferedAction.None;
        }

        if (MobileInputController.instance == null)
            return;

        if (UnifiedInput.MoveRight) Buffer(BufferedAction.Right);
        else if (UnifiedInput.MoveLeft) Buffer(BufferedAction.Left);
        else if (UnifiedInput.Jump) Buffer(BufferedAction.Jump);
        else if (UnifiedInput.Slide) Buffer(BufferedAction.Slide);
    }

    private void Buffer(BufferedAction action)
    {
        bufferedAction = action;
        bufferTimer = inputBufferTime;
    }

    public bool ConsumeInput(BufferedAction action)
    {
        if (bufferedAction != action)
            return false;

        bufferedAction = BufferedAction.None;
        return true;
    }

    // Shared by every state so keyboard and swipe behave the same on foot, in the air and while flying.
    public bool TryLaneChange(float hopHeight, float tiltAngle, float duration, bool playSound)
    {
        if (isChangingLane)
            return false;

        int dir;
        if (bufferedAction == BufferedAction.Right) dir = 1;
        else if (bufferedAction == BufferedAction.Left) dir = -1;
        else return false;

        bufferedAction = BufferedAction.None;

        int target = currentLane + dir;
        if (target < 0 || target >= lanePositions.Length)
            return false;

        StartCoroutine(ChangeLane(target, hopHeight, -dir * tiltAngle, duration));

        if (playSound)
        {
            AudioManager.instance.PlaySound(1);
            AudioManager.instance.PlaySound(6);
        }
        return true;
    }

    // Score is real distance along the track, not footsteps.
    private void UpdateDistance()
    {
        if (!runStartSet)
        {
            runStartZ = transform.position.z;
            runStartSet = true;
        }

        int meters = Mathf.FloorToInt(transform.position.z - runStartZ);
        if (meters > distanceTraveled)
        {
            distanceTraveled = meters;
            UIManager.instance.distanceText.text = NumberFormatter.FormatNumber(distanceTraveled) + " m";
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
        //0) Fast layer/tag bailouts
        int obstacleMask = LayerMask.GetMask("Obstacle");
        if (((1 << hit.collider.gameObject.layer) & obstacleMask) ==0)
            return;

        //1) Ignore ground: for ground the surface normal points mostly up
        // (normal.y close to1). Walls have small normal.y.
        if (hit.normal.y >0.5f)
            return;

        //2) From here on we know it's a wall-like hit (i.e., obstacle)
        if (isShielded) return;

        // Optional: also ensure player was moving forward relative to facing
        Vector3 horizontalMove = new Vector3(hit.moveDirection.x,0f, hit.moveDirection.z);
        if (Vector3.Dot(transform.forward, horizontalMove.normalized) <0.25f)
            return;

        var obstacle = hit.collider.GetComponent<Obstacles>();
        if (obstacle == null) return;

        //3) Handle by type / current state
        if (obstacle.obstacleType == ObstacleType.Explosive)
        {
            stateMachine.ChangeState(fastHitState);
            StartCoroutine(DeathBounce());
            AudioManager.instance.PlaySound(9);
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
            obstacle.PushRigidBodies(hit.point,20f,10f);
        }
        else
        {
            stateMachine.ChangeState(hitState);
            isStarted = false;
            obstacle.ActivateRigidbodies();
        }
        AudioManager.instance.PlaySound(7);
    }


    private void CheckCoinOverlap()
    {
        // Use non-alloc overlap to reduce GC pressure
        // A little more reach while flying, where the player hovers slightly above / below the coin line
        bool flying = stateMachine.currentstate == planeState || stateMachine.currentstate == jetState;
        int count = Physics.OverlapSphereNonAlloc(transform.position, flying ? 1f : 0.5f, coinOverlapBuffer, coinLayerMask);

        for (int i = 0; i < count; i++)
        {
            var coinCollider = coinOverlapBuffer[i];
            if (coinCollider == null) continue;

            Coin c = coinCollider.GetComponent<Coin>();
            if (c != null)
            {
                CollectCoin(c, coinCollider.transform.position);
            }
        }
    }

    public void CollectCoin(Coin coin, Vector3 worldPos)
    {
        if (coin == null)
            return;

        combo++;
        comboTimer = comboTimeout;
        bestCombo = Mathf.Max(bestCombo, combo);

        AudioManager.instance.PlayCoin();
        UIManager.instance.MoveCoinImg(worldPos);
        PlayCollectParticleAt(transform.position);

        if (combo % comboPerBoost == 0)
            StartRewardBoost();

        Destroy(coin.gameObject);
    }

    public void RegisterNearMiss(Vector3 obstaclePosition)
    {
        if (!isStarted || isShielded || !IsOnFoot || nearMissTimer > 0f)
            return;

        nearMissTimer = nearMissCooldown;
        int before = combo;
        combo += nearMissComboBonus;
        comboTimer = comboTimeout;
        bestCombo = Mathf.Max(bestCombo, combo);
        PlayCollectParticleAt(transform.position);
        AudioManager.instance.PlayCoin();

        if (combo / comboPerBoost > before / comboPerBoost)
            StartRewardBoost();
    }

    public int GetCoinRewardAmount()
    {
        if (combo >= 100)
            return 4;

        if (combo >= 50)
            return 3;

        if (combo >= 20)
            return 2;

        return 1;
    }

    private void TickFunFeel()
    {
        if (nearMissTimer > 0f)
            nearMissTimer -= Time.deltaTime;

        if (boostTimer > 0f)
        {
            boostTimer -= Time.deltaTime;
            if (boostTimer <= 0f)
            {
                boostTimer = 0f;
                moveSpeed -= rewardSpeedBoost;
            }
        }

        if (!isStarted)
            return;

        if (comboTimer > 0f)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0f)
                combo = 0;
        }
    }

    private void StartRewardBoost()
    {
        // A boost that is already running is only extended; the speed bonus is applied exactly once.
        if (boostTimer <= 0f)
            moveSpeed += rewardSpeedBoost;

        boostTimer = rewardBoostDuration;
    }

    // Play or lazily create a pooled collect particle at given position
    public void PlayCollectParticleAt(Vector3 pos)
    {
        if (collectParticle == null) return;

        if (pooledCollectParticle == null)
        {
            pooledCollectParticle = Instantiate(collectParticle, pos, Quaternion.identity);
            pooledCollectParticle.transform.parent = transform;
        }
        else
        {
            pooledCollectParticle.transform.position = pos;
        }

        pooledCollectParticle.Play();
    }

    //Instantiate Car
    public void InstantiateCar()
    {
        activeVehicle = Instantiate(vehiclePrefab[vehicleIndex],
        new Vector3(lanePositions[currentLane],0.065f, transform.position.z), Quaternion.identity);
        activeVehicle.GetComponent<VehicleController>().SetupCar(carIndex, colorIndex);
    }

    //InstantiateTank
    public void InstantiateTank()
    {
        tank = Instantiate(tankPrefab,
        new Vector3(lanePositions[currentLane],0.065f, transform.position.z), Quaternion.identity);
    }

    public void DestroyCar() => Destroy(activeVehicle);
    public void DestroyTank() => Destroy(tank);

    //Shield Mode
    public IEnumerator ActivateShield()
    {
        isShielded = true;
        controller.excludeLayers = LayerMask.GetMask("Obstacle");
        shiledParticle.SetActive(true);
        yield return new WaitForSeconds(shieldDuration);
        shiledParticle.SetActive(false);
        controller.excludeLayers =0;
        isShielded = false;
    }

    //Death 
    public IEnumerator DeathBounce()
    {
        Vector3 startPosition = transform.position;
        Vector3 endPosition = transform.position + Vector3.back *5;

        float duration =1f; // slightly longer for the hop
        float elapsed =0f;
 

        float hopHeight =2.5f; // how high the hop goes

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

        yield return new WaitForSeconds(0.5f);
        AudioManager.instance.PlaySound(8);
    }

    // Called by the run animation events and the vehicle states. Distance itself is measured in UpdateDistance().
    public void Distance()
    {
        if (stateMachine.currentstate == moveState && controller.isGrounded)
            AudioManager.instance.PlayFootstep();
    }
}

public enum BufferedAction
{
    None,
    Left,
    Right,
    Jump,
    Slide,
}

