using System;
using System.Collections.Generic;
using UnityEngine;
using Controller2k;
using Mirror;
using Random = UnityEngine.Random;

// MoveState as byte for minimal bandwidth (otherwise it's int by default)
// note: distinction between WALKING and RUNNING in case we need to know the
//       difference somewhere (e.g. for endurance recovery)
// note: AIRBORNE means jumping || falling. no need to have two states for that.
public enum MoveState : byte { IDLE, RUNNING, AIRBORNE, SWIMMING, MOUNTED, MOUNTED_AIRBORNE, MOUNTED_SWIMMING, DEAD }

[RequireComponent(typeof(CharacterController2k))]
[RequireComponent(typeof(AudioSource))]
[DisallowMultipleComponent]
public class PlayerCharacterControllerMovement : Movement
{
    // components to be assigned in inspector
    [Header("Components")]
    public PlayerOLD player;
    public Animator animator;
    public Health health;
    public CharacterController2k controller;
    public AudioSource feetAudio;
    public Combat combat;
    public PlayerMountControl mountControl;
    // the collider for the character controller. NOT the hips collider. this
    // one is NOT affected by animations and generally a better choice for state
    // machine logic.
    public CapsuleCollider controllerCollider;
#pragma warning disable CS0109 // member does not hide accessible member
    new Camera camera;
#pragma warning restore CS0109 // member does not hide accessible member

    [Header("State")]
    public MoveState state = MoveState.IDLE;
    MoveState lastState = MoveState.IDLE;
    [HideInInspector] public Vector3 moveDir;

    // it's useful to have both strafe movement (WASD) and rotations (QE)
    // => like in WoW, it more fun to play this way.
    [Header("Rotation")]
    public float rotationSpeed = 190;

    [Header("Running")]
    float runSpeed = 8; // not public, we use Entity's speed via SetSpeed
    [Range(0f, 1f)] public float runStepLength = 0.7f;
    public float runStepInterval = 3;
    public float runCycleLegOffset = 0.2f; //specific to the character in sample assets, will need to be modified to work with others
    float stepCycle;
    float nextStep;
    [Tooltip("Option to automatically run forward while both mouse buttons are pressed. This can be useful for long stretches of walking.")]
    public bool runWhileBothMouseButtonsPressed = true;

    [Header("Swimming")]
    public float swimSpeed = 4;
    public float swimSurfaceOffset = 0.25f;
    Collider waterCollider;
    bool inWater => waterCollider != null; // standing in water / touching it?
    bool underWater; // deep enough in water so we need to swim?
    public LayerMask canStandInWaterCheckLayers = Physics.DefaultRaycastLayers; // set this to everything except water layer
    [Header("Swimming [CAREFUL]")]
    [Tooltip("Percentage of body that needs to be underwater to start swimming. Change this carefully and make sure to test it both on foot and mounted. Higher values might work well on foot, but mounted it would stay underwater for way too long.")]
    [Range(0, 1)] public float underwaterThreshold = 0.7f; // percent of body that need to be underwater to start swimming

    [Header("Jumping")]
    public float jumpSpeed = 7;
    [HideInInspector] public float jumpLeg;
    bool jumpKeyPressed;

    [Header("Airborne")]
    [Tooltip("Allows steering while falling/jumping. Can be cool for some games, or unwanted for others.")]
    public bool airborneSteering = true;
    public float fallMinimumMagnitude = 6; // walking down steps shouldn't count as falling and play no falling sound.
    public float fallDamageMinimumMagnitude = 13;
    public float fallDamageMultiplier = 2;
    [HideInInspector] public Vector3 lastFall;

    [Header("Mounted")]
    public float mountedRotationSpeed = 100;

    [Header("Physics")]
    [Tooltip("Apply a small default downward force while grounded in order to stick on the ground and on rounded surfaces. Otherwise walking on rounded surfaces would be detected as falls, preventing the player from jumping.")]
    public float gravityMultiplier = 2;

    [Header("Synchronization (Best not to modify)")]
    [Tooltip("Buffer at least that many moves before applying them in FixedUpdate. A bigger min buffer offers more lag tolerance with the cost of additional latency.")]
    public int minMoveBuffer = 2;
    [Tooltip("Combine two moves as one after having more than that many pending moves in order to avoid ever growing queues.")]
    public int combineMovesAfter = 5;
    [Tooltip("Buffer at most that many moves before force reseting the client. There is no point in buffering hundreds of moves and having it always lag 3 seconds behind.")]
    public int maxMoveBuffer = 10; // 20ms fixedDelta * 10 = 200 ms. no need to buffer more than that.
    [Tooltip("Rubberband movement: player can move freely as long as the server position matches. If server and client get off further than rubberDistance then a force reset happens.")]
    public float rubberDistance = 1;
    [Tooltip("Tolerance in percent for maximum movement speed. 0% is too small because there is always some latency and imprecision. 100% is too much because it allows for double speed hacks.")]
    [Range(0, 1)] public float validSpeedTolerance = 0.2f;
    byte route = 0; // used to stamp moves sent to server, see FixedUpdate. byte is enough. it will overflow and that's fine!
    int combinedMoves = 0; // debug information only
    int rubbered = 0; // debug information only
    // epsilon for float/vector3 comparison (needed because of imprecision
    // when sending over the network, etc.)
    const float epsilon = 0.1f;

    // helper property to check grounded with some tolerance. technically we
    // aren't grounded when walking down steps, but this way we factor in a
    // minimum fall magnitude. useful for more tolerant jumping etc.
    // (= while grounded or while velocity not smaller than min fall yet)
    public bool isGroundedWithinTolerance =>
        controller.isGrounded || controller.velocity.y > -fallMinimumMagnitude;

    [Header("Sounds")]
    public AudioClip[] footstepSounds;    // an array of footstep sounds that will be randomly selected from.
    public AudioClip jumpSound;           // the sound played when character leaves the ground.
    public AudioClip landSound;           // the sound played when character touches back on ground.

    [Header("Animation")]
    public float directionDampening = 0.05f;
    public float turnDampening = 0.1f;
    Vector3 lastForward;

    [Header("Debug")]
    public bool showDebugGUI;
    [Tooltip("Debug GUI visibility curve. X axis = distance, Y axis = alpha. Nothing will be displayed if Y = 0.")]
    public AnimationCurve debugVisibilityCurve = new AnimationCurve(new Keyframe(0, 0.3f), new Keyframe(15, 0.3f), new Keyframe(20, 0f));

    [Header("Camera")]
    public float XSensitivity = 2;
    public float YSensitivity = 2;
    public float MinimumX = -90;
    public float MaximumX = 90;

    [Tooltip("If free look is enabled, then the camera can be rotated around the character without actually rotating the character. Disabled by default because it can make left clicks awkward.")]
    public float freeLookDragThreshold = 40; // start free look only after a certain distance. not immediately when left clicking.
    bool freeLookDragStarted;
    Vector2 freeLookDragStart;
    public int mouseFreeLookButton = 0; // left button by default
    public int mouseRotateButton = 1; // right button by default

    public bool mouseRotationLocksCursor = true;

    // head position is useful for raycasting etc.
    public Transform firstPersonParent;
    public Vector3 headPosition => firstPersonParent.position;
    public Transform freeLookParent;

    Vector3 originalCameraPosition;

    // the layer mask to use when trying to detect view blocking
    // (this way we dont zoom in all the way when standing in another entity)
    // (-> create a entity layer for them if needed)
    public LayerMask viewBlockingLayers;
    public float zoomSpeed = 0.5f;
    public float distance = 12.5f;
    public float minDistance = 3;
    public float maxDistance = 20;

    [Header("Physical Interaction")]
    [Tooltip("Layers to use for raycasting. Check Default, Walls, Player, Monster, Doors, Interactables, Item, etc. Uncheck IgnoreRaycast, AggroArea, Water, UI, etc.")]
    public LayerMask raycastLayers = Physics.DefaultRaycastLayers;

    // camera offsets. Vector2 because we only want X (left/right) and Y (up/down)
    // to be modified. Z (forward/backward) should NEVER be modified because
    // then we could look through walls when tilting our head forward to look
    // downwards, etc. This can be avoided in the camera positioning logic, but
    // is way to complex and not worth it at all.
    [Header("Offsets - Standing")]
    public Vector2 thirdPersonOffset = Vector2.up;
    public Vector2 thirdPersonOffsetMultiplier = Vector2.zero;

    float lastClientSendTime;

    // we can't point to controller.velocity because that might not be reliable
    // over the network if we apply multiple moves at once. instead save the
    // last valid move's velocity here.
    public Vector3 velocity { get; private set; }

    // abstract Movement overrides /////////////////////////////////////////////
    public override Vector3 GetVelocity()
    {
        return velocity;
    }

    public override bool IsMoving()
    {
        return velocity != Vector3.zero;
    }

    public override void SetSpeed(float speed)
    {
        runSpeed = speed;
    }

    // look at a transform while only rotating on the Y axis (to avoid weird
    // tilts)
    public override void LookAtY(Vector3 position)
    {
        transform.LookAt(new Vector3(position.x, transform.position.y, position.z));
    }

    public override void Reset()
    {
        // we have no navigation, so we don't need to reset any paths
    }

    public override bool CanNavigate()
    {
        return false;
    }

    public override void Navigate(Vector3 destination, float stoppingDistance)
    {
        // character controller movement doesn't allow navigation (yet)
    }

    public override bool IsValidSpawnPoint(Vector3 position)
    {
        // all positions allowed for now.
        // we should probably raycast later.
        return true;
    }

    public override Vector3 NearestValidDestination(Vector3 destination)
    {
        // character controller movement doesn't allow navigation (yet)
        return destination;
    }

    public override bool DoCombatLookAt()
    {
        // player should use keys/mouse to look at. don't overwrite it.
        return false;
    }

    ////////////////////////////////////////////////////////////////////////////\
    void Awake()
    {
        camera = Camera.main;
    }

    void Start()
    {
        // remember lastForward for animations
        lastForward = transform.forward;

        if (isLocalPlayer) // TODO use OnStartLocalPlayer instead?
        {
            // set camera parent to player
            camera.transform.SetParent(transform, false);

            // look into player forward direction, which was loaded from the db
            camera.transform.rotation = transform.rotation;

            // set camera to head position
            camera.transform.position = headPosition;
        }

        // remember original camera position
        originalCameraPosition = camera.transform.localPosition;
    }

    void OnValidate()
    {
        minMoveBuffer = Mathf.Clamp(minMoveBuffer, 1, maxMoveBuffer); // need at least 1 to move
        combineMovesAfter = Mathf.Clamp(combineMovesAfter, minMoveBuffer + 1, maxMoveBuffer);
        maxMoveBuffer = Mathf.Clamp(maxMoveBuffer, combineMovesAfter + 1, 50); // 20ms*50=1s at max should be enough
    }

    // input directions ////////////////////////////////////////////////////////
    Vector2 GetInputDirection()
    {
        // get input direction while alive and while not typing in chat
        // (otherwise 0 so we keep falling even if we die while jumping etc.)
        float horizontal = 0;
        float vertical = 0;

        // keyboard input
        if (!UIUtils.AnyInputActive())
        {
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");
        }

        // pressing both mouse buttons counts as forward input
        if (runWhileBothMouseButtonsPressed &&
            !Utils.IsCursorOverUserInterface() &&
            Input.GetMouseButton(0) &&
            Input.GetMouseButton(1))
        {
            vertical = 1;
        }

        // normalize ONLY IF needs to be normalized (if length>1).
        // we use GetAxis instead of GetAxisRaw, so we may get vectors like
        // (0.1, 0). if we normalized this in all cases, then it would always be
        // (1, 0) even if we slowly push the controller forward.
        Vector2 input = new Vector2(horizontal, vertical);
        if (input.magnitude > 1)
        {
            input = input.normalized;
        }
        return input;
    }

    Vector3 GetDesiredDirection(Vector2 inputDir)
    {
        // always move along the camera forward as it is the direction that is being aimed at
        return transform.forward * inputDir.y + transform.right * inputDir.x;
    }

    // scale charactercontroller collider to pose, otherwise we can fire above a
    // crawling player and still hit him.
    void AdjustControllerCollider()
    {
        // ratio depends on state
        float ratio = 1;
        if (state == MoveState.SWIMMING || state == MoveState.DEAD)
            ratio = 0.25f;

        controller.TrySetHeight(controller.defaultHeight * ratio, true, true, false);
    }

    // movement state machine //////////////////////////////////////////////////
    bool EventDied()
    {
        return health.current == 0;
    }

    bool EventJumpRequested()
    {
        // only while grounded, so jump key while jumping doesn't start a new
        // jump immediately after landing
        // => and not while sliding, otherwise we could climb slides by jumping
        // => not even while SlidingState.Starting, so we aren't able to avoid
        //    sliding by bunny hopping.
        // => grounded check uses min fall tolerance so we can actually still
        //    jump when walking down steps.
        return isGroundedWithinTolerance &&
               controller.slidingState == SlidingState.NONE &&
               jumpKeyPressed;
    }

    bool EventFalling()
    {
        // use minimum fall magnitude so walking down steps isn't detected as
        // falling! otherwise walking down steps would show the fall animation
        // and play the landing sound.
        return !isGroundedWithinTolerance;
    }

    bool EventLanded()
    {
        return controller.isGrounded;
    }

    bool EventMounted()
    {
        return mountControl.IsMounted();
    }

    bool EventDismounted()
    {
        return !mountControl.IsMounted();
    }

    bool EventUnderWater()
    {
        // we can't really make it player position dependent, because he might
        // swim to the surface at which point it might be detected as standing
        // in water but not being under water, etc.
        if (inWater) // in water and valid water collider?
        {
            // raycasting from water to the bottom at the position of the player
            // seems like a very precise solution
            Vector3 origin = new Vector3(transform.position.x,
                                         waterCollider.bounds.max.y,
                                         transform.position.z);
            float distance = controllerCollider.height * underwaterThreshold;
            Debug.DrawLine(origin, origin + Vector3.down * distance, Color.cyan);

            // we are underwater if the raycast doesn't hit anything
            return !Utils.RaycastWithout(origin, Vector3.down, out RaycastHit hit, distance, gameObject, canStandInWaterCheckLayers);
        }
        return false;
    }

    // helper function to apply gravity based on previous Y direction
    float ApplyGravity(float moveDirY)
    {
        // apply full gravity while falling
        if (!controller.isGrounded)
            // gravity needs to be * Time.fixedDeltaTime even though we multiply
            // the final controller.Move * Time.fixedDeltaTime too, because the
            // unit is 9.81m/s²
            return moveDirY + Physics.gravity.y * gravityMultiplier * Time.fixedDeltaTime;
        // if grounded then apply no force. the new OpenCharacterController
        // doesn't need a ground stick force. it would only make the character
        // slide on all uneven surfaces.
        return 0;
    }

    void ApplyFallDamage()
    {
        // measure only the Y direction. we don't want to take fall damage
        // if we jump forward into a wall because xz is high.
        float fallMagnitude = Mathf.Abs(lastFall.y);
        if(fallMagnitude >= fallDamageMinimumMagnitude)
        {
            int damage = Mathf.RoundToInt(fallMagnitude * fallDamageMultiplier);
            health.current -= damage;
            combat.RpcOnReceivedDamaged(damage, DamageTypeOLD.Normal);
        }
    }

    // rotate with QE keys
    void RotateWithKeys()
    {
        float horizontal2 = Input.GetAxis("Horizontal2");
        transform.Rotate(Vector3.up * horizontal2 * rotationSpeed * Time.fixedDeltaTime);
    }

    MoveState UpdateIDLE(Vector2 inputDir, Vector3 desiredDir)
    {
        // QE key rotation
        if (player.IsMovementAllowed())
            RotateWithKeys();

        // move
        // (moveDir.xz can be set to 0 to have an interruption when landing)
        moveDir.x = desiredDir.x * runSpeed;
        moveDir.y = ApplyGravity(moveDir.y);
        moveDir.z = desiredDir.z * runSpeed;

        if (EventDied())
        {
            // rescale capsule
            controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false);
            // dead no matter what, even if rescale failed
            return MoveState.DEAD;
        }
        else if (EventFalling())
        {
            return MoveState.AIRBORNE;
        }
        else if (EventJumpRequested())
        {
            // start the jump movement into Y dir, go to jumping
            // note: no endurance>0 check because it feels odd if we can't jump
            moveDir.y = jumpSpeed;
            PlayJumpSound();
            return MoveState.AIRBORNE;
        }
        else if (EventMounted())
        {
            return MoveState.MOUNTED;
        }
        else if (EventUnderWater())
        {
            // rescale capsule
            if (controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false))
            {
                return MoveState.SWIMMING;
            }
        }
        else if (inputDir != Vector2.zero)
        {
            return MoveState.RUNNING;
        }
        else if (EventDismounted()) {} // don't care

        return MoveState.IDLE;
    }

    MoveState UpdateRUNNING(Vector2 inputDir, Vector3 desiredDir)
    {
        // QE key rotation
        if (player.IsMovementAllowed())
            RotateWithKeys();

        // move
        moveDir.x = desiredDir.x * runSpeed;
        moveDir.y = ApplyGravity(moveDir.y);
        moveDir.z = desiredDir.z * runSpeed;

        if (EventDied())
        {
            // rescale capsule
            controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false);
            // dead no matter what, even if rescale failed
            return MoveState.DEAD;
        }
        else if (EventFalling())
        {
            return MoveState.AIRBORNE;
        }
        else if (EventJumpRequested())
        {
            // start the jump movement into Y dir, go to jumping
            // note: no endurance>0 check because it feels odd if we can't jump
            moveDir.y = jumpSpeed;
            PlayJumpSound();
            return MoveState.AIRBORNE;
        }
        else if (EventMounted())
        {
            return MoveState.MOUNTED;
        }
        else if (EventUnderWater())
        {
            // rescale capsule
            if (controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false))
            {
                return MoveState.SWIMMING;
            }
        }
        // go to idle after fully decelerating (y doesn't matter)
        else if (moveDir.x == 0 && moveDir.z == 0)
        {
            return MoveState.IDLE;
        }
        else if (EventDismounted()) {} // don't care

        ProgressStepCycle(inputDir, runSpeed);
        return MoveState.RUNNING;
    }

    MoveState UpdateAIRBORNE(Vector2 inputDir, Vector3 desiredDir)
    {
        // input allowed while airborne?
        if (airborneSteering)
        {
            // QE key rotation
            if (player.IsMovementAllowed())
                RotateWithKeys();

            // move
            moveDir.x = desiredDir.x * runSpeed;
            moveDir.y = ApplyGravity(moveDir.y);
            moveDir.z = desiredDir.z * runSpeed;
        }
        // otherwise keep moving in same direction, only apply gravity
        else
        {
            moveDir.y = ApplyGravity(moveDir.y);
        }

        if (EventDied())
        {
            // rescale capsule
            controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false);
            // dead no matter what, even if rescale failed
            return MoveState.DEAD;
        }
        else if (EventLanded())
        {
            PlayLandingSound();
            return MoveState.IDLE;
        }
        else if (EventUnderWater())
        {
            // rescale capsule
            if (controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false))
            {
                return MoveState.SWIMMING;
            }
        }

        return MoveState.AIRBORNE;
    }

    MoveState UpdateSWIMMING(Vector2 inputDir, Vector3 desiredDir)
    {
        if (EventDied())
        {
            // rescale capsule
            controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false);
            // dead no matter what, even if rescale failed
            return MoveState.DEAD;
        }
        // not under water anymore?
        else if (!EventUnderWater())
        {
            // rescale capsule if possible
            if (controller.TrySetHeight(controller.defaultHeight * 1f, true, true, false))
            {
                return MoveState.IDLE;
            }
        }

        // QE key rotation
        if (player.IsMovementAllowed())
            RotateWithKeys();

        // move
        moveDir.x = desiredDir.x * swimSpeed;
        moveDir.z = desiredDir.z * swimSpeed;

        // gravitate toward surface
        if (waterCollider != null)
        {
            float surface = waterCollider.bounds.max.y;
            float surfaceDirection = surface - controller.bounds.min.y - swimSurfaceOffset;
            moveDir.y = surfaceDirection * swimSpeed;
        }
        else moveDir.y = 0;

        return MoveState.SWIMMING;
    }

    MoveState UpdateMOUNTED(Vector2 inputDir, Vector3 desiredDir)
    {
        // recalculate desired direction while ignoring inputDir horizontal part
        // (horses can't strafe.)
        desiredDir = GetDesiredDirection(new Vector2(0, inputDir.y));

        // horizontal input axis rotates the character instead of strafing
        if (player.IsMovementAllowed())
            transform.Rotate(Vector3.up * inputDir.x * mountedRotationSpeed * Time.fixedDeltaTime);

        // find mounted speed if mount is still around
        // (it might not be immediately after dismounting, in which case
        //  UpdateMOUNTED gets called one more time until the EventDismounted()
        //  check below)
        float speed = mountControl.activeMount != null
                      ? mountControl.activeMount.speed
                      : runSpeed;

        // move
        moveDir.x = desiredDir.x * speed;
        moveDir.y = ApplyGravity(moveDir.y);
        moveDir.z = desiredDir.z * speed;

        if (EventDied())
        {
            // rescale capsule
            controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false);
            // DEAD in any case, even if rescale failed
            return MoveState.DEAD;
        }
        else if (EventFalling())
        {
            return MoveState.MOUNTED_AIRBORNE;
        }
        else if (EventJumpRequested())
        {
            // start the jump movement into Y dir, go to jumping
            // note: no endurance>0 check because it feels odd if we can't jump
            moveDir.y = jumpSpeed;
            PlayJumpSound();
            return MoveState.MOUNTED_AIRBORNE;
        }
        else if (EventDismounted())
        {
            return MoveState.IDLE;
        }
        else if (EventUnderWater())
        {
            return MoveState.MOUNTED_SWIMMING;
        }
        else if (EventMounted()) {} // don't care

        return MoveState.MOUNTED;
    }

    MoveState UpdateMOUNTED_AIRBORNE(Vector2 inputDir, Vector3 desiredDir)
    {
        // recalculate desired direction while ignoring inputDir horizontal part
        // (horses can't strafe.)
        desiredDir = GetDesiredDirection(new Vector2(0, inputDir.y));

        // horizontal input axis rotates the character instead of strafing
        if (player.IsMovementAllowed())
            transform.Rotate(Vector3.up * inputDir.x * mountedRotationSpeed * Time.fixedDeltaTime);

        // find mounted speed if mount is still around
        // (it might not be immediately after dismounting, in which case
        //  UpdateMOUNTED gets called one more time until the EventDismounted()
        //  check below)
        float speed = mountControl.activeMount != null
            ? mountControl.activeMount.speed
            : runSpeed;

        // move
        moveDir.x = desiredDir.x * speed;
        moveDir.y = ApplyGravity(moveDir.y);
        moveDir.z = desiredDir.z * speed;

        if (EventDied())
        {
            // rescale capsule
            controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false);
            // DEAD in any case, even if rescale failed
            return MoveState.DEAD;
        }
        else if (EventLanded())
        {
            // apply fall damage only in AIRBORNE->Landed.
            // (e.g. not if we run face forward into a wall with high velocity)
            ApplyFallDamage();
            PlayLandingSound();
            return MoveState.MOUNTED;
        }
        else if (EventUnderWater())
        {
            return MoveState.MOUNTED_SWIMMING;
        }

        return MoveState.MOUNTED_AIRBORNE;
    }

    MoveState UpdateMOUNTED_SWIMMING(Vector2 inputDir, Vector3 desiredDir)
    {
        if (EventDied())
        {
            // rescale capsule
            controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false);
            // DEAD in any case, even if rescale failed
            return MoveState.DEAD;
        }
        // not under water anymore?
        else if (!EventUnderWater())
        {
            return MoveState.MOUNTED;
        }
        // dismounted while swimming?
        else if (EventDismounted())
        {
            return MoveState.SWIMMING;
        }

        // recalculate desired direction while ignoring inputDir horizontal part
        // (horses can't strafe.)
        desiredDir = GetDesiredDirection(new Vector2(0, inputDir.y));

        // horizontal input axis rotates the character instead of strafing
        if (player.IsMovementAllowed())
            transform.Rotate(Vector3.up * inputDir.x * mountedRotationSpeed * Time.fixedDeltaTime);

        // move with acceleration (feels better)
        moveDir.x = desiredDir.x * swimSpeed;
        moveDir.z = desiredDir.z * swimSpeed;

        // gravitate toward surface
        if (waterCollider != null)
        {
            float surface = waterCollider.bounds.max.y;
            float surfaceDirection = surface - controller.bounds.min.y + mountControl.seatOffsetY - swimSurfaceOffset;
            moveDir.y = surfaceDirection * swimSpeed;
        }
        else moveDir.y = 0;

        return MoveState.MOUNTED_SWIMMING;
    }

    MoveState UpdateDEAD(Vector2 inputDir, Vector3 desiredDir)
    {
        // keep falling while dead: if we get shot while falling, we shouldn't
        // stop in mid air
        moveDir.x = 0;
        moveDir.y = ApplyGravity(moveDir.y);
        moveDir.z = 0;

        // not dead anymore?
        if (health.current > 0)
        {
            // rescale capsule in any case. we don't check CanSetHeight so that
            // no other entities can block a respawn. SetHeight will depenetrate
            // either way.
            controller.TrySetHeight(controller.defaultHeight * 1f, true, true, false);
            return MoveState.IDLE;
        }
        return MoveState.DEAD;
    }

    void Update()
    {
        // for local player:
        if (isLocalPlayer)
        {
            // only if movement allowed (not typing, trading, etc.)
            if (player.IsMovementAllowed())
            {
                if (!jumpKeyPressed)
                    jumpKeyPressed = Input.GetButtonDown("Jump");
            }

            UpdateCamera();
        }

        // for all players:
        UpdateAnimations();
    }

    // send each FixedUpdate move to the server to apply as well
    struct Move
    {
        public byte route;
        public MoveState state;
        public Vector3 position; // position is 100% accurate. moveDir alone would get out of sync after a while.
        public float yRotation;
        public Move(byte route, MoveState state, Vector3 position, float yRotation)
        {
            this.route = route;
            this.state = state;
            this.position = position;
            this.yRotation = yRotation;
        }
    }
    Queue<Move> pendingMoves = new Queue<Move>();
    [Command]
    void CmdFixedMove(Move move)
    {
        // check route to make sure that the move received was sent AFTER the
        // last force reset. we don't want to apply moves that were in transit
        // during the last force reset (we don't want to apply old moves).
        if (move.route != route)
            return;

        // NOTE: we DO NOT check IsValidMove here. instead we check it when
        //       actually applying the move later. we don't know what state the
        //       player will be in, or which position he will be. it makes no
        //       sense to check IsValid from current position to 'move' because
        //       we add it to pending moves, and there might be 10 other moves
        //       changing transform.position before that.

        // add to pending moves until FixedUpdate happens
        // (not for localplayer, it always moves itself)
        // (not if queue is full. it will be reset in next move anyway, and
        //  there is no need to waste memory with ever growing lists in case
        //  another client or an attacker spams way too frequently)
        if (!isLocalPlayer && pendingMoves.Count < maxMoveBuffer)
            pendingMoves.Enqueue(move);

        // broadcast to all players in proximity
        // (do this for localplayer too. others need to know our moves)
        RpcFixedMove(move);
    }

    [ClientRpc]
    void RpcFixedMove(Move move)
    {
        // note: no need to ignore old routes here because TCP is ordered.
        // all RpcFixedMoves with old routes will arrive BEFORE RpcForceReset.

        // do nothing if host. CmdFixedMove already added to queue.
        if (isServer) return;

        // do nothing if localplayer. it moves itself.
        if (isLocalPlayer) return;

        // add to pending moves until FixedUpdate happens
        // (not if queue is full. it will be reset in next move anyway, and
        //  there is no need to waste memory with ever growing lists in case
        //  another client or an attacker spams way too frequently)
        if (pendingMoves.Count < maxMoveBuffer)
            pendingMoves.Enqueue(move);
    }

    // force reset needs to happen on all clients. they all need to know the
    // player's new route number to disregard old values etc.
    [ClientRpc]
    void RpcWarp(Vector3 position, byte newRoute)
    {
        // force reset position
        transform.position = position;

        // set new route
        route = newRoute;

        // clear all moves with the old route. only apply the new ones now.
        // (we do this for other players, local player has no queue)
        pendingMoves.Clear();
    }

    [Server]
    public override void Warp(Vector3 destination)
    {
        // set new position
        transform.position = destination;

        // are we a spawned object?
        // => if we are a character selection preview or similar, then we would
        //    receive a warning about Rpc being called on unspawned object
        // => and route would get out of sync if we modify it here and the
        //    client never knows because the Rpc doesn't get called.
        if (isServer)
        {
            // ignore all incoming movements with old route
            ++route;

            // clear buffer so we don't apply old moves
            pendingMoves.Clear();

            // let the clients know
            RpcWarp(destination, route);
            //Debug.LogWarning("Force reset to route=" + route);
        }
    }

    // rubberbanding if position got too far off from expected
    // position. e.g. if we ran into a wall even though the
    // client walked around it.
    // -> can be TESTED easily by adding Vector3.forward to
    //    each CmdFixedMove call! it will be reset when missing
    //    walls.
    [Server]
    void RubberbandCheck(Vector3 expectedPosition)
    {
        if (Vector3.Distance(transform.position, expectedPosition) >= rubberDistance)
        {
            Warp(transform.position);
            ++rubbered; // debug information only
        }
    }

    float GetMaximumSpeedForState(MoveState moveState)
    {
        switch (moveState)
        {
            // idle, running, mounted use runSpeed which is set by Entity
            case MoveState.IDLE:
            case MoveState.RUNNING:
            case MoveState.MOUNTED:
                return runSpeed;
            // swimming uses swimSpeed
            case MoveState.SWIMMING:
            case MoveState.MOUNTED_SWIMMING:
                return swimSpeed;
            // airborne accelerates with gravity.
            // maybe check xz and y speed separately.
            case MoveState.AIRBORNE:
            case MoveState.MOUNTED_AIRBORNE:
                return float.MaxValue;
            case MoveState.DEAD:
                return 0;
            default:
                Debug.LogWarning("Don't know how to calculate max speed for state: " + moveState);
                return 0;
        }
    }

    // check if a movement vector is valid (the 'rubber' part)
    // ('move' is a movement vector, NOT a position!)
    bool IsValidMove(Vector3 move)
    {
        // players send Vector3.zero while idle, casting, rotating, changing
        // state, etc.
        // -> those should always be allowed. otherwise we spam invalid move
        //    messages to the console.
        // -> epsilon comparison for network imprecision. otherwise we still get
        //    some "(0, 0, 0)" move warnings in console
        if (move.magnitude <= epsilon)
            return true;

        // are we in a state where movement is allowed (not dead etc.)?
        return player.IsMovementAllowed();
    }

    bool WasValidSpeed(Vector3 moveVelocity, MoveState previousState, MoveState nextState, bool combining)
    {
        // calculate xz magnitude for max speed check.
        // we'll have to check y separately because gravity, sliding,
        // falling, jumping, etc. will cause faster speeds then max speed.
        float speed = new Vector2(moveVelocity.x, moveVelocity.z).magnitude;

        // are we going below maximum allowed speed for the given state?
        // -> we might be going from crouching to running or vice versa, so
        //    let's allow max speed of both states
        float maxSpeed = Mathf.Max(GetMaximumSpeedForState(previousState),
                                   GetMaximumSpeedForState(nextState));

        // calculate max speed with a small tolerance because it's never
        // exact when going over the network
        float maxSpeedWithTolerance = maxSpeed * (1 + validSpeedTolerance);

        // we allow twice the speed when applying a combined move
        if (combining)
            maxSpeedWithTolerance *= 2;

        // we should have a small tolerance for lags, latency, etc.
        //Debug.Log(name + " speed=" + speed + " / " + maxSpeedWithTolerance + " in state=" + targetState);
        if (speed <= maxSpeedWithTolerance)
        {
            return true;
        }
        else Debug.Log(name + " move rejected because too fast: combining=" + combining + " xz speed=" + speed + " / " + maxSpeedWithTolerance + " state=" + previousState + "=>" + nextState);
        return false;
    }

    // CharacterController movement is physics based and requires FixedUpdate.
    // (using Update causes strange movement speeds in builds otherwise)
    void FixedUpdate()
    {
        // only control movement for local player
        if (isLocalPlayer)
        {
            // get input and desired direction based on camera and ground
            Vector2 inputDir = player.IsMovementAllowed() ? GetInputDirection() : Vector2.zero;
            Vector3 desiredDir = GetDesiredDirection(inputDir);
            Debug.DrawLine(transform.position, transform.position + desiredDir, Color.cyan);

            // update state machine
            if      (state == MoveState.IDLE)             state = UpdateIDLE(inputDir, desiredDir);
            else if (state == MoveState.RUNNING)          state = UpdateRUNNING(inputDir, desiredDir);
            else if (state == MoveState.AIRBORNE)         state = UpdateAIRBORNE(inputDir, desiredDir);
            else if (state == MoveState.SWIMMING)         state = UpdateSWIMMING(inputDir, desiredDir);
            else if (state == MoveState.MOUNTED)          state = UpdateMOUNTED(inputDir, desiredDir);
            else if (state == MoveState.MOUNTED_AIRBORNE) state = UpdateMOUNTED_AIRBORNE(inputDir, desiredDir);
            else if (state == MoveState.MOUNTED_SWIMMING) state = UpdateMOUNTED_SWIMMING(inputDir, desiredDir);
            else if (state == MoveState.DEAD)             state = UpdateDEAD(inputDir, desiredDir);
            else Debug.LogError("Unhandled Movement State: " + state);

            // cache this move's state to detect landing etc. next time
            if (!controller.isGrounded) lastFall = controller.velocity;

            // move depending on latest moveDir changes
            //Debug.Log(name + " step speed=" + moveDir.magnitude + " in state=" + state);
            controller.Move(moveDir * Time.fixedDeltaTime); // note: returns CollisionFlags if needed
            velocity = controller.velocity; // for animations and fall damage

            // broadcast to server
            CmdFixedMove(new Move(route, state, transform.position, transform.rotation.eulerAngles.y));

            // calculate which leg is behind, so as to leave that leg trailing in the jump animation
            // (This code is reliant on the specific run cycle offset in our animations,
            // and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)
            float runCycle = Mathf.Repeat(animator.GetCurrentAnimatorStateInfo(0).normalizedTime + runCycleLegOffset, 1);
            jumpLeg = (runCycle < 0.5f ? 1 : -1);// * move.z;

            // reset keys no matter what
            jumpKeyPressed = false;
        }
        // server/other clients need to do some caching and scaling too
        else
        {
            // scale character collider to pose if not local player.
            // -> correct collider is needed both on server and on clients
            //
            // IMPORTANT: only when switching states. if we do it all the time then
            // a build client's movement speed would be significantly reduced.
            // (and performance would be worse too)
            //
            // scale BEFORE moving. we do the same in the localPlayer state
            // machine above! if we scale after moving then server and client
            // might end up with different results.
            if (lastState != state)
                AdjustControllerCollider();

            // assign lastFall to .velocity (works on server), not controller.velocity
            // BEFORE doing the next move, just like we do for the local player
            if (!controller.isGrounded) lastFall = velocity;

            // apply next pending move if we have at least 'minMoveBuffer'
            // moves pending to make sure that we also still have one to apply
            // in the next FixedUpdate call.
            // => it's better to apply 1,1,1,1 move instead of 2,0,2,1,0 moves.
            if (pendingMoves.Count > 0 && pendingMoves.Count >= minMoveBuffer)
            {
                // too many pending moves?
                // (only the server has authority to reset!)
                if (isServer && pendingMoves.Count >= maxMoveBuffer)
                {
                    // force reset
                    Warp(transform.position);
                }
                // more than combine threshold?
                // (both server and client can combine moves if needed)
                else if (pendingMoves.Count >= 2 && pendingMoves.Count >= combineMovesAfter)
                {
                    Vector3 previousPosition = transform.position;
                    MoveState previousState = state;

                    // combine the next two moves to minimize overall delay.
                    // => if we are always behind 5 moves then we might as well
                    //    accelerate a bit to always be behind 4, 3, or 2 moves
                    //    to minimize movement delay.
                    //
                    // note: calling controller.Move() multiple times in one
                    //       FixedUpdate doesn't work well and isn't
                    //       recommended. adding the two vectors together works
                    //       better.
                    //
                    // note: we COULD warp to first.position and then move to
                    //       second.position to avoid the double-move which
                    //       always comes with the risk of getting out of sync
                    //       because A,B went behind a wall while A+B went into
                    //       the wall.
                    //
                    //       BUT if we do warp then we would ALWAYS risk wall
                    //       hack cheats since the server would sometimes set
                    //       the position without checking physics via .Move.
                    //
                    //       INSTEAD we risk move(A+B) running into a wall and
                    //       reset if rubberbanding gets too far off. at least
                    //       this method can be made cheat safe.
                    Move first = pendingMoves.Dequeue();
                    Move second = pendingMoves.Dequeue();
                    Vector3 move = second.position - transform.position; // calculate the delta before each move. using .position is 100% accurate and never gets out of sync.

                    // check if allowed (only check on server)
                    if (!isServer || IsValidMove(move))
                    {
                        state = second.state;
                        // multiple moves. use velocity between the two moves, not between position and second move.
                        // -> and divide by fixedDeltaTime because velocity is
                        //    direction / second
                        velocity = (second.position - first.position) / Time.fixedDeltaTime;
                        transform.rotation = Quaternion.Euler(0, second.yRotation, 0);
                        controller.Move(move);
                        ++combinedMoves; // debug information only

                        // safety checks on server
                        if (isServer)
                        {
                            // check speed AFTER actually moving with the TRUE
                            // velocity. this works better than checking before
                            // moving because now we know the true velocity
                            // after collisions.
                            if (!WasValidSpeed(velocity, previousState, state, true))
                            {
                                Warp(previousPosition);
                            }

                            // rubberbanding check if server
                            RubberbandCheck(second.position);
                        }
                    }
                    // force reset to last allowed position if not allowed
                    else
                    {
                        Warp(transform.position);
                        Debug.LogWarning(name + " combined move: " + move + " rejected and force reset to: " + transform.position + "@" + DateTime.UtcNow);
                    }
                }
                // less than combine threshold?
                else
                {
                    Vector3 previousPosition = transform.position;
                    MoveState previousState = state;

                    // apply one move
                    Move next = pendingMoves.Dequeue();
                    Vector3 move = next.position - transform.position; // calculate the delta before each move. using .position is 100% accurate and never gets out of sync.

                    // check if allowed (only check on server)
                    if (!isServer || IsValidMove(move))
                    {
                        state = next.state;
                        transform.rotation = Quaternion.Euler(0, next.yRotation, 0);
                        controller.Move(move);
                        velocity = controller.velocity; // only one move, so controller velocity is true.

                        // safety checks on server
                        if (isServer)
                        {
                            // check speed AFTER actually moving with the TRUE
                            // velocity. this works better than checking before
                            // moving because now we know the true velocity
                            // after collisions.
                            if (!WasValidSpeed(velocity, previousState, state, false))
                            {
                                Warp(previousPosition);
                            }

                            // rubberbanding check if server
                            RubberbandCheck(next.position);
                        }
                    }
                    // force reset to last allowed position if not allowed
                    else
                    {
                        Warp(transform.position);
                        Debug.LogWarning(name + " single move: " + move + " rejected and force reset to: " + transform.position + "@" + DateTime.UtcNow);
                    }
                }
            }
            //else Debug.Log("none pending. client should send faster...");
        }

        // some server logic
        if (isServer)
        {
            // apply fall damage only in AIRBORNE state. not when running head
            // forward into a wall with high velocity, etc. we don't ever want
            // to get fall damage while running.
            // -> can't rely on EventLanded here because we don't know if we
            //    receive the client's new state exactly when landed.
            if (lastState == MoveState.AIRBORNE && state != MoveState.AIRBORNE)
            {
                ApplyFallDamage();
            }
        }

        // set last state after everything else is done.
        lastState = state;
    }

    void OnGUI()
    {
        // show data next to player for easier debugging. this is very useful!
        // IMPORTANT: this is basically an ESP hack for shooter games.
        //            DO NOT make this available with a hotkey in release builds
        if (Debug.isDebugBuild && showDebugGUI)
        {
            // project player position to screen
            Vector3 center = controllerCollider.bounds.center;
            Vector3 point = camera.WorldToScreenPoint(center);

            // sample visibility curve based on distance. avoid GUI calls if
            // alpha = 0 at this distance.
            float distance = Vector3.Distance(camera.transform.position, transform.position);
            float alpha = debugVisibilityCurve.Evaluate(distance);

            // enough alpha, in front of camera and in screen?
            if (alpha > 0 && point.z >= 0 && Utils.IsPointInScreen(point))
            {
                GUI.color = new Color(0, 0, 0, alpha);
                GUILayout.BeginArea(new Rect(point.x, Screen.height - point.y, 160, 100));
                GUILayout.Label("grounded=" + controller.isGrounded);
                GUILayout.Label("groundedT=" + isGroundedWithinTolerance);
                GUILayout.Label("lastFall=" + lastFall.y);
                GUILayout.Label("sliding=" + controller.slidingState);
                if (!isLocalPlayer)
                {
                    GUILayout.Label("health=" + health.current + "/" + health.max);
                    GUILayout.Label("pending=" + pendingMoves.Count);
                    GUILayout.Label("route=" + route);
                    GUILayout.Label("combined=" + combinedMoves);
                }
                if (isServer) GUILayout.Label("rubbered=" + rubbered);
                GUILayout.EndArea();
                GUI.color = Color.white;
            }
        }
    }

    void PlayLandingSound()
    {
        feetAudio.clip = landSound;
        feetAudio.Play();
        nextStep = stepCycle + .5f;
    }

    void PlayJumpSound()
    {
        feetAudio.clip = jumpSound;
        feetAudio.Play();
    }

    void ProgressStepCycle(Vector3 inputDir, float speed)
    {
        if (controller.velocity.sqrMagnitude > 0 && (inputDir.x != 0 || inputDir.y != 0))
        {
            stepCycle += (controller.velocity.magnitude + (speed * runStepLength)) *  Time.fixedDeltaTime;
        }

        if (stepCycle > nextStep)
        {
            nextStep = stepCycle + runStepInterval;
            PlayFootStepAudio();
        }
    }

    void PlayFootStepAudio()
    {
        if (!controller.isGrounded) return;

        // do we have any footstep sounds?
        if (footstepSounds.Length > 0)
        {
            // pick & play a random footstep sound from the array,
            // excluding sound at index 0
            int n = Random.Range(1, footstepSounds.Length);
            feetAudio.clip = footstepSounds[n];
            feetAudio.PlayOneShot(feetAudio.clip);

            // move picked sound to index 0 so it's not picked next time
            footstepSounds[n] = footstepSounds[0];
            footstepSounds[0] = feetAudio.clip;
        }
    }

    [ClientCallback] // client authoritative movement, don't do this on Server
    //[ServerCallback] <- disabled for now, since movement is client authoritative
    void OnTriggerEnter(Collider co)
    {
        // touching water? then set water collider
        if (co.tag == "Water")
            waterCollider = co;
    }

    [ClientCallback] // client authoritative movement, don't do this on Server
    void OnTriggerExit(Collider co)
    {
        if (co.tag == "Water")
            waterCollider = null;
    }

    // animations //////////////////////////////////////////////////////////////
    float GetJumpLeg()
    {
        // always left leg for others. saves Cmd+SyncVar bandwidth and no one will notice.
        return isLocalPlayer ? jumpLeg : 1;
    }

    // Vector.Angle and Quaternion.FromToRotation and Quaternion.Angle all end
    // up clamping the .eulerAngles.y between 0 and 360, so the first overflow
    // angle from 360->0 would result in a negative value (even though we added
    // something to it), causing a rapid twitch between left and right turn
    // animations.
    //
    // the solution is to use the delta quaternion rotation.
    // when turning by 0.5, it is:
    //   0.5 when turning right (0 + angle)
    //   364.6 when turning left (360 - angle)
    // so if we assume that anything >180 is negative then that works great.
    static float AnimationDeltaUnclamped(Vector3 lastForward, Vector3 currentForward)
    {
        Quaternion rotationDelta = Quaternion.FromToRotation(lastForward, currentForward);
        float turnAngle = rotationDelta.eulerAngles.y;
        return turnAngle >= 180 ? turnAngle - 360 : turnAngle;
    }

    [ClientCallback]
    void UpdateAnimations()
    {
        // local velocity (based on rotation) for animations
        Vector3 localVelocity = transform.InverseTransformDirection(velocity);

        // Turn value so that mouse-rotating the character plays some animation
        // instead of only raw rotating the model.
        float turnAngle = AnimationDeltaUnclamped(lastForward, transform.forward);
        lastForward = transform.forward;

        // apply animation parameters to all animators.
        // there might be multiple if we use skinned mesh equipment.
        foreach (Animator animator in GetComponentsInChildren<Animator>())
        {
            animator.SetFloat("DirX", localVelocity.x, directionDampening, Time.deltaTime); // smooth idle<->run transitions
            animator.SetFloat("DirY", localVelocity.y, directionDampening, Time.deltaTime); // smooth idle<->run transitions
            animator.SetFloat("DirZ", localVelocity.z, directionDampening, Time.deltaTime); // smooth idle<->run transitions
            animator.SetFloat("LastFallY", lastFall.y);
            animator.SetFloat("Turn", turnAngle, turnDampening, Time.deltaTime); // smooth turn
            animator.SetBool("SWIMMING", state == MoveState.SWIMMING);

            // grounded detection for other players works best via .state
            // -> check AIRBORNE state instead of controller.isGrounded to have some
            //    minimum fall tolerance so we don't play the AIRBORNE animation
            //    while walking down steps etc.
            // -> always true while mounted, so we don't play airborne animation
            //    while mount jumps
            animator.SetBool("OnGround", state != MoveState.AIRBORNE || player.mountControl.activeMount != null);
            if (controller.isGrounded) animator.SetFloat("JumpLeg", GetJumpLeg());
        }
    }

    // Camera //////////////////////////////////////////////////////////////////
    // look directions /////////////////////////////////////////////////////////
    // * for first person, all we need is the camera.forward
    //
    // * for third person, we need to raycast where the camera looks and then
    //   calculate the direction from the eyes.
    //   BUT for animations we actually only want camera.forward because it
    //   looks strange if we stand right in front of a wall, camera aiming above
    //   a player's head (because of head offset) and then the players arms
    //   aiming at that point above his head (on the wall) too.
    //     => he should always appear to aim into the far direction
    //     => he should always fire at the raycasted point
    //   in other words, if we want 1st and 3rd person WITH camera offsets, then
    //   we need both the FAR direction and the RAYCASTED direction
    //
    // * we also need to sync it over the network to animate other players.
    //   => we compress it as far as possible to save bandwidth. syncing it via
    //      rotation bytes X and Y uses 2 instead of 12 bytes per observer(!)
    //
    // * and we can't only calculate and store the values in Update because
    //   ShoulderLookAt needs them live in LateUpdate, Update is too far behind
    //   and would cause the arms to be lag behind a bit.
    //
    [SyncVar, HideInInspector] public Vector3 syncedLookDirectionFar;

    public Vector3 lookDirectionFar
    {
        get
        {
            return isLocalPlayer ? camera.transform.forward : syncedLookDirectionFar;
        }
    }

    // the far position, directionFar projected into nirvana
    public Vector3 lookPositionFar
    {
        get
        {
            Vector3 position = isLocalPlayer ? camera.transform.position : headPosition;
            return position + lookDirectionFar * 9999f;
        }
    }

    // the raycasted position is needed for lookDirectionRaycasted calculation
    // and for firing, so we might as well reuse it here
    public Vector3 lookPositionRaycasted
    {
        get
        {
            if (isLocalPlayer)
            {
                // raycast based on position and direction, project into nirvana if nothing hit
                // (not * infinity because might overflow depending on position)
                // -> without self to ignore own body parts etc.
                return Utils.RaycastWithout(camera.transform.position, camera.transform.forward, out RaycastHit hit, Mathf.Infinity, gameObject, raycastLayers)
                    ? hit.point
                    : lookPositionFar;
            }
            else
            {
                // the only person to need the raycast direction is the local player (right now).
                // we use the far direction for other player's animations and we pass a the
                // raycast direction to the server when using items. there should be no reason to
                // sync those 12 extra bytes over the network, so let's just show an error here
                // => can still use the below code if necessary some day
                //return Utils.RaycastWithout(firstPersonParent.position, syncedLookDirectionRaycasted, out hit, Mathf.Infinity, gameObject, raycastLayers)
                //       ? hit.point
                //       : lookPositionFar;
                Debug.LogError("PlayerLook.lookPositionRaycasted isn't synced so you can only call it on the local player right now.\n" + Environment.StackTrace);
                return Vector3.zero;
            }
        }
    }

    [Command]
    void CmdSetLookDirection(Vector3 lookDirectionFar)//, Vector3 directionRaycasted)
    {
        //syncedLookDirectionFar = directionFar;
        //syncedLookDirectionRaycasted = directionRaycasted; <- not needed atm, see syncPositionRaycasted comment
        syncedLookDirectionFar = lookDirectionFar;
    }

    void UpdateCamera()
    {
        if (!isLocalPlayer) return;

        // send only each 'sendinterval', otherwise we send at whatever
        // the player's tick rate is, which is like DDOS
        // (SendInterval doesn't seem to apply to Cmd, so we have to do
        //  it manually)
        if (Time.time - lastClientSendTime >= syncInterval)
        {
            // sync direction if changed
            // NOTE: look direction isn't needed _yet_, but it might be later.
            //       e.g. for free aiming weapons.
            if (Vector3.Distance(lookDirectionFar, syncedLookDirectionFar) > Mathf.Epsilon)
                CmdSetLookDirection(lookDirectionFar); //, lookDirectionRaycasted);

            lastClientSendTime = Time.time;
        }

        // flag to check if cursor locking is necessary
        bool rotatingWithMouse = false;

        // only while alive and while right mouse down
        if (health.current > 0)
        {
            // calculate horizontal and vertical rotation steps
            float xExtra = Input.GetAxis("Mouse X") * XSensitivity;
            float yExtra = Input.GetAxis("Mouse Y") * YSensitivity;

            // calculate drag distance
            // we need this to not start free look immediately, only after a
            // little drag so it doesn't interfere with regular left clicks
            float dragDistance = 0;
            if (Input.GetMouseButton(mouseFreeLookButton) &&
                !Utils.IsCursorOverUserInterface() &&
                UIDragAndDropable.currentlyDragged == null)
            {
                if (!freeLookDragStarted)
                {
                    freeLookDragStarted = true;
                    freeLookDragStart = Input.mousePosition;
                }
                dragDistance = Vector2.Distance(freeLookDragStart, Input.mousePosition);
            }
            else freeLookDragStarted = false;

            // use mouse to rotate character
            // (no free look in first person)
            // (not while drag and dropping items either)
            // (not while dragging UI windows. otherwise freelook would start
            //  when dragging a window so quickly that the cursor leaves the
            //  window for one frame)
            // (only while holding down free look button AND NOT the other one)
            // (only after dragging a minimum distance OR if cursor locked)
            //    => otherwise cursor lock resets cursor to (0,0), which would
            //       set drag distance to 0 if we started near 0. in other words
            //       it would constantly reset dragging if we started dragging
            //       near the center.
            //  holding down both buttons is used to run forward + rotation when
            //  running forward. otherwise we can't rotate the character while
            //  running forward with both mouse buttons held down)
            //Debug.LogWarning("freelookbutton: " + Input.GetMouseButton(mouseFreeLookButton));
            int notFreeLookButton = mouseFreeLookButton == 0 ? 1 : 0;
            if (Input.GetMouseButton(mouseFreeLookButton) &&
                !Input.GetMouseButton(notFreeLookButton) &&
                (dragDistance >= freeLookDragThreshold || Cursor.lockState == CursorLockMode.Locked) &&
                !Utils.IsCursorOverUserInterface() &&
                UIDragAndDropable.currentlyDragged == null &&
                UIWindow.currentlyDragged == null &&
                distance > 0)
            {
                // we are rotating with a mouse
                rotatingWithMouse = true;

                // set to freelook parent already?
                if (camera.transform.parent != freeLookParent)
                    InitializeFreeLook();

                // rotate freelooktarget for horizontal, rotate camera for vertical
                freeLookParent.Rotate(new Vector3(0, xExtra, 0));
                camera.transform.Rotate(new Vector3(-yExtra, 0, 0));
            }
            // not free looking
            else
            {
                // set to player parent no matter what. this is important to do
                // in any case when not free looking, otherwise the free look
                // rotation never resets
                if (camera.transform.parent != transform)
                    InitializeForcedLook();

                // holding right button? then rotate
                if (Input.GetMouseButton(mouseRotateButton) &&
                    !Utils.IsCursorOverUserInterface())
                {
                    // we are rotating with a mouse
                    rotatingWithMouse = true;

                    // rotate character for horizontal, rotate camera for vertical
                    transform.Rotate(new Vector3(0, xExtra, 0));
                    camera.transform.Rotate(new Vector3(-yExtra, 0, 0));
                }
            }
        }

        // lock cursor, or free cursor in any case
        if (mouseRotationLocksCursor)
        {
            // are we rotating with the mouse?
            if (rotatingWithMouse)
            {
                // if the mouse rotation just starts and the mouse moved at
                // least a little bit, then lock it
                // (don't lock if we just clicked on a monster to loot it)
                // (don't check cursorMoved while locked, only when started.
                //  otherwise it would show again if we hold still while dragging)
                if (Cursor.lockState == CursorLockMode.None)
                    Cursor.lockState = CursorLockMode.Locked;
            }
            // otherwise unlock in any case
            else Cursor.lockState = CursorLockMode.None;
        }
    }

    // Update camera position after everything else was updated
    void LateUpdate()
    {
        if (!isLocalPlayer) return;

        // clamp camera rotation automatically. this way we can rotate it to
        // whatever we like in Update, and LateUpdate will correct it.
        camera.transform.localRotation = Utils.ClampRotationAroundXAxis(camera.transform.localRotation, MinimumX, MaximumX);

        // zoom after rotating, otherwise it won't be smooth and would overwrite
        // each other.

        // zoom should only happen if not in a UI right now
        if (!Utils.IsCursorOverUserInterface())
        {
            float step = Utils.GetZoomUniversal() * zoomSpeed;
            distance = Mathf.Clamp(distance - step, minDistance, maxDistance);
        }

        // calculate target and zoomed position
        Vector3 origin = originalCameraPosition;
        Vector3 offsetBase = thirdPersonOffset;
        Vector3 offsetMult = thirdPersonOffsetMultiplier;

        Vector3 target = transform.TransformPoint(origin + offsetBase + offsetMult * distance);
        Vector3 newPosition = target - (camera.transform.rotation * Vector3.forward * distance);

        // avoid view blocking (only third person, pointless in first person)
        // -> always based on original distance and only overwrite if necessary
        //    so that we dont have to zoom out again after view block disappears
        // -> we cast exactly from cam to target, which is the crosshair position.
        //    if anything is inbetween then view blocking changes the distance.
        //    this works perfectly.
        float finalDistance = distance;
        Debug.DrawLine(target, camera.transform.position, Color.white);
        if (Physics.Linecast(target, newPosition, out RaycastHit hit, viewBlockingLayers))
        {
            // calculate a better distance (with some space between it)
            finalDistance = Vector3.Distance(target, hit.point) - 0.1f;
            Debug.DrawLine(target, hit.point, Color.red);
        }
        else Debug.DrawLine(target, newPosition, Color.green);

        // set final position
        camera.transform.position = target - (camera.transform.rotation * Vector3.forward * finalDistance);
    }

    public bool IsFreeLooking()
    {
        if (!isLocalPlayer) return false;

        return camera != null && // camera isn't initialized while loading players in charselection
               camera.transform.parent == freeLookParent;
    }

    public void InitializeFreeLook()
    {
        camera.transform.SetParent(freeLookParent, false);
        freeLookParent.localRotation = Quaternion.identity; // initial rotation := where we look at right now
    }

    public void InitializeForcedLook()
    {
        camera.transform.SetParent(transform, false);
    }
}
