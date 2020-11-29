// we move as much of the movement code as possible into a separate component,
// so that we can switch it out with character controller movement (etc.) easily

using System;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(PlayerIndicator))]
[RequireComponent(typeof(NetworkNavMeshAgentRubberbanding))]
[DisallowMultipleComponent]
public class PlayerNavMeshMovement : NavMeshMovement
{
    [Header("Components")]
    public Player player;
    public PlayerIndicator indicator;
    public NetworkNavMeshAgentRubberbanding rubberbanding;

    [Header("Camera")]
    public int mouseRotateButton = 1; // right button by default
    public float cameraDistance = 20;
    public float minDistance = 3;
    public float maxDistance = 20;
    public float zoomSpeedMouse = 1;
    public float zoomSpeedTouch = 0.2f;
    public float rotationSpeed = 2;
    public float xMinAngle = -40;
    public float xMaxAngle = 80;

    // the target position can be adjusted by an offset in order to foucs on a
    // target's head for example
    public Vector3 cameraOffset = Vector3.zero;

    // view blocking
    // note: only works against objects with colliders.
    //       uMMORPG has almost none by default for performance reasons
    // note: remember to disable the entity layers so the camera doesn't zoom in
    //       all the way when standing inside another entity
    public LayerMask viewBlockingLayers;

    // store rotation so that unity never modifies it, otherwise unity will put
    // it back to 360 as soon as it's <0, which makes a negative min angle
    // impossible
    Vector3 rotation;
    bool rotationInitialized;

    // Camera.main calls FindObjectWithTag each time. cache it!
    Camera cam;

    public override void Reset()
    {
        // rubberbanding needs a custom reset, along with the base navmesh reset
        if (isServer)
            rubberbanding.ResetMovement();
        agent.ResetMovement();
    }

    // for 4 years since uMMORPG release we tried to detect warps in
    // NetworkNavMeshAgent/Rubberbanding. it never worked 100% of the time:
    // -> checking if dist(pos, lastpos) > speed worked well for far teleports,
    //    but failed for near teleports with dist < speed meters.
    // -> checking if speed since last update is > speed is the perfect idea,
    //    but it turns out that NavMeshAgent sometimes moves faster than
    //    agent.speed, e.g. when moving up or down a corner/stone. in fact, it
    //    sometimes moves up to 5x faster than speed, which makes warp detection
    //    hard.
    // => the ONLY 100% RELIABLE solution is to have our own Warp function that
    //    force warps the client over the network.
    // => this is extremely important for cases where players get warped behind
    //    a small door or wall. this just has to work.
    public override void Warp(Vector3 destination)
    {
        // rubberbanding needs to know about warp. this is the only 100%
        // reliable way to detect it.
        if (isServer)
            rubberbanding.RpcWarp(destination);
        agent.Warp(destination);
    }

    public override void OnStartLocalPlayer()
    {
        // find main camera
        // only for local player. 'Camera.main' is expensive (FindObjectWithTag)
        cam = Camera.main;
    }

    void Update()
    {
        // only for local player
        if (!isLocalPlayer) return;

        // wasd movement allowed?
        if (player.IsMovementAllowed())
            MoveWASD();

        // click movement allowed?
        // (we allowed it in CASTING/STUNNED too by setting nextDestination
        if (player.IsMovementAllowed() || player.state == "CASTING" || player.state == "STUNNED")
            MoveClick();
    }

    [Client]
    void MoveWASD()
    {
        // get horizontal and vertical input
        // note: no != 0 check because it's 0 when we stop moving rapidly
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        if (horizontal != 0 || vertical != 0)
        {
            // create input vector, normalize in case of diagonal movement
            Vector3 input = new Vector3(horizontal, 0, vertical);
            if (input.magnitude > 1) input = input.normalized;

            // get camera rotation without up/down angle, only left/right
            Vector3 angles = cam.transform.rotation.eulerAngles;
            angles.x = 0;
            Quaternion rotation = Quaternion.Euler(angles); // back to quaternion

            // calculate input direction relative to camera rotation
            Vector3 direction = rotation * input;

            // draw direction for debugging
            Debug.DrawLine(transform.position, transform.position + direction, Color.green, 0, false);

            // clear indicator if there is one, and if it's not on a target
            // (simply looks better)
            if (direction != Vector3.zero)
                indicator.ClearIfNoParent();

            // cancel path if we are already doing click movement, otherwise
            // we will slide
            agent.ResetMovement();

            // set velocity
            agent.velocity = direction * player.speed;

            // moving with velocity doesn't look at the direction, do it manually
            LookAtY(transform.position + direction);

            // clear requested skill in any case because if we clicked
            // somewhere else then we don't care about it anymore
            player.useSkillWhenCloser = -1;
        }
    }

    [Client]
    void MoveClick()
    {
        // click raycasting if not over a UI element & not pinching on mobile
        // note: this only works if the UI's CanvasGroup blocks Raycasts
        if (Input.GetMouseButtonDown(0) &&
            !Utils.IsCursorOverUserInterface() &&
            Input.touchCount <= 1)
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            // raycast with local player ignore option
            RaycastHit hit;
            bool cast = player.localPlayerClickThrough
                        ? Utils.RaycastWithout(ray.origin, ray.direction, out hit, Mathf.Infinity, gameObject)
                        : Physics.Raycast(ray, out hit);
            if (cast)
            {
                // clicked a movement target, not an entity?
                if (!hit.transform.GetComponent<Entity>())
                {
                    // set indicator and navigate to the nearest walkable
                    // destination. this prevents twitching when destination is
                    // accidentally in a room without a door etc.
                    Vector3 bestDestination = NearestValidDestination(hit.point);
                    indicator.SetViaPosition(bestDestination);

                    // casting or stunned? then set pending destination
                    if (player.state == "CASTING" || player.state == "STUNNED")
                    {
                        player.pendingDestination = bestDestination;
                        player.pendingDestinationValid = true;
                    }
                    // otherwise navigate there
                    else Navigate(bestDestination, 0);
                }
            }
        }
    }

    // camera //////////////////////////////////////////////////////////////////
    void LateUpdate()
    {
        // only for local player
        if (!isLocalPlayer) return;

        Vector3 targetPos = transform.position + cameraOffset;

        // rotation and zoom should only happen if not in a UI right now
        if (!Utils.IsCursorOverUserInterface())
        {
            // right mouse rotation if we have a mouse
            if (Input.mousePresent)
            {
                if (Input.GetMouseButton(mouseRotateButton))
                {
                    // initialize the base rotation if not initialized yet.
                    // (only after first mouse click and not in Awake because
                    //  we might rotate the camera inbetween, e.g. during
                    //  character selection. this would cause a sudden jump to
                    //  the original rotation from Awake otherwise.)
                    if (!rotationInitialized)
                    {
                        rotation = transform.eulerAngles;
                        rotationInitialized = true;
                    }

                    // note: mouse x is for y rotation and vice versa
                    rotation.y += Input.GetAxis("Mouse X") * rotationSpeed;
                    rotation.x -= Input.GetAxis("Mouse Y") * rotationSpeed;
                    rotation.x = Mathf.Clamp(rotation.x, xMinAngle, xMaxAngle);
                    cam.transform.rotation = Quaternion.Euler(rotation.x, rotation.y, 0);
                }
            }
            else
            {
                // forced 45 degree if there is no mouse to rotate (for mobile)
                cam.transform.rotation = Quaternion.Euler(new Vector3(45, 0, 0));
            }

            // zoom
            float speed = Input.mousePresent ? zoomSpeedMouse : zoomSpeedTouch;
            float step = Utils.GetZoomUniversal() * speed;
            cameraDistance = Mathf.Clamp(cameraDistance - step, minDistance, maxDistance);
        }

        // target follow
        cam.transform.position = targetPos - (cam.transform.rotation * Vector3.forward * cameraDistance);

        // avoid view blocking (disabled, see comment at the top)
        if (Physics.Linecast(targetPos, cam.transform.position, out RaycastHit hit, viewBlockingLayers))
        {
            // calculate a better distance (with some space between it)
            float d = Vector3.Distance(targetPos, hit.point) - 0.1f;

            // set the final cam position
            cam.transform.position = targetPos - (cam.transform.rotation * Vector3.forward * d);
        }
    }

    // validation //////////////////////////////////////////////////////////////
    void OnValidate()
    {
        // make sure that the NetworkNavMeshAgentRubberbanding component is
        // ABOVE this component, so that it gets updated before this one.
        // -> otherwise it overwrites player's WASD velocity for local player
        //    hosts
        // -> there might be away around it, but a warning is good for now
        Component[] components = GetComponents<Component>();
        if (Array.IndexOf(components, GetComponent<NetworkNavMeshAgentRubberbanding>()) >
            Array.IndexOf(components, this))
            Debug.LogWarning(name + "'s NetworkNavMeshAgentRubberbanding component is below the PlayerNavMeshMovement component. Please drag it above the Player component in the Inspector, otherwise there might be WASD movement issues due to the Update order.");
    }
}
