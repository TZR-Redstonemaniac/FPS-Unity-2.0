using System;
using System.Collections;
using UnityEngine;
using Rewired;
using DG.Tweening;
#pragma warning disable CS0414

public class Player : MonoBehaviour
{
    ////////////////////////////////////////Variables/////////////////////////////////////////

    #region Movement

    [Header("Movement")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float slideSpeed;
    [SerializeField] private float wallrunSpeed;
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashSpeedChangeFactor;
    [SerializeField] private float speedCap;
    [SerializeField] private float ySpeedCap;
    [SerializeField] private float groundDrag;
    [SerializeField] private float airSpeed;
    [SerializeField] private float swingSpeed;
    
    private Rigidbody rb;
    
    [HideInInspector] public Vector3 moveVector;
    private Vector3 moveDir;
    
    private float moveSpeed;
    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;
    private float lastYSpeedCap;

    #endregion

    #region Jumping

    [Header("Jumping")]
    [SerializeField] private float jumpPower;
    [SerializeField] private float jumpCooldown;
    [SerializeField] private float airMult;

    #endregion
    
    #region Crouching

    [Header("Crouching")] 
    [SerializeField] private float crouchSpeed;
    [SerializeField] private float crouchYScale;
    
    private bool didCrouchForce;
    private bool isCrouch;
    private bool above;

    #endregion

    #region Ground Check

    [Header("Ground Check")] 
    [SerializeField] private LayerMask whatIsGround;
    
    public float playerHeight;
    
    private bool grounded;

    #endregion

    #region Slope

    [Header("Slope Handling")]
    [SerializeField] private float maxSlopeAngle;
    [SerializeField] private float keepOnSlopeForce;

    #endregion

    #region Sliding

    [Header("Sliding")]
    [SerializeField] private float maxSlideTime;
    [SerializeField] private float slideForce;
    [SerializeField] private float slideYScale;
    
    private bool exitingSlope;
    private bool sliding;
    private bool didSlideForce;
    private bool didSlide;
    
    private float startYScale;
    private float slideTimer;
    
    private RaycastHit slopeHit;
    
    private bool OnSlope()
    {
        if (crouch)
        {
            if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.25f + 0.3f))
            {
                var angle = Vector3.Angle(Vector3.up, slopeHit.normal);
                return angle <= maxSlopeAngle && angle != 0;
            }
        }
        else
        {
            if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
            {
                var angle = Vector3.Angle(Vector3.up, slopeHit.normal);
                return angle <= maxSlopeAngle && angle != 0;
            }
        }

        return false;
    }

    private Vector3 GetSlopeDirectionMovement(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    #endregion

    #region Wallrunning

    [Header("WallRunning")]
    [SerializeField] private LayerMask whatIsWall;
    [SerializeField] private float wallRunForce;
    [SerializeField] private float maxWallRunTime;
    [SerializeField] private float wallCheckDistance;
    [SerializeField] private float minJumpHeight;
    [SerializeField] private float wallClimbSpeed;
    [SerializeField] private float wallJumpUpForce;
    [SerializeField] private float wallJumpSideForce;
    [SerializeField] private float exitWallTime;
    [SerializeField] private float gravityCounterForce;
    [SerializeField] private float camTilt;
    [SerializeField] private float camFovChange;
    
    [SerializeField] private bool useGravity;
    
    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;

    private float wallRunTimer;
    private float exitWallTimer;

    private bool wallLeft;
    private bool wallRight;
    private bool wallrunning;
    private bool wallrunUp;
    private bool wallrunDown;
    private bool exitingWall;

    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
    }

    #endregion

    #region Climbing

    [Header("Climbing")]
    [SerializeField] private float climbSpeed;
    [SerializeField] private float maxClimbTime;
    [SerializeField] private float detectionLength;
    [SerializeField] private float sphereCastRadius;
    [SerializeField] private float maxWallLookAngle;
    [SerializeField] private float climbJumpUpForce;
    [SerializeField] private float climbJumpBackForce;
    [SerializeField] private float minWallAngleChange;
    [SerializeField] private float exitClimbWallTime;
    [SerializeField] private int climbJumps;

    private float climbTimer;
    private float wallLookAngle;
    private float exitWallClimbTimer;

    private int climbJumpsLeft;

    private RaycastHit frontWallHit;

    private bool wallFront;
    private bool climbing;
    private bool exitingClimbWall;

    private Transform lastWall;
    private Vector3 lastWallNormal;

    #endregion

    #region Camera

    [Header("Camera")]
    [SerializeField] private float minY = -80;
    [SerializeField] private float maxY = 80;
    [SerializeField] private float sens = 1;
    [SerializeField] private Transform camPos;
    [SerializeField] private Transform camHolder;
    [SerializeField] private Camera cam;
    
    private Vector3 lookVector;

    private float fov;

    #endregion

    #region Ledge Grabbing

    [Header("Ledge Grabbing")]
    [SerializeField] private LayerMask whatIsLedge;
    [SerializeField] private float ledgeDetectionLength;
    [SerializeField] private float ledgeSphereCastRadius;
    [SerializeField] private float moveToLedgeSpeed;
    [SerializeField] private float ledgeGrabDistance;
    [SerializeField] private float minTimeOnLedge;
    [SerializeField] private float ledgeJumpForwardForce;
    [SerializeField] private float ledgeJumpUpForce;
    [SerializeField] private float exitLedgeTime;

    private float timeOnLedge;
    private float exitLedgeTimer;

    private Transform lastLedge;
    private Transform currentLedge;

    private bool holding;
    private bool exitingLedge;

    private RaycastHit ledgeHit;

    #endregion

    #region Dash
    
    [Header("Dashing")]
    [SerializeField] private float dashForce;
    [SerializeField] private float dashUpForce;
    [SerializeField] private float dashDuration;
    [SerializeField] private float dashCooldown;
    [SerializeField] private float maxDashYSpeed;
    [SerializeField] private float dashFov;

    [SerializeField] private bool useCamForward = true;
    [SerializeField] private bool allowAllDir = true;
    [SerializeField] private bool disableGrav;
    [SerializeField] private bool resetVel = true;
    
    private float dashCooldownTimer;
    private Vector3 delayedForceToApply;

    #endregion

    #region Hooking

    [Header("Hooking")]
    [SerializeField] private float hookFovChange;

    [HideInInspector] public bool activeHook;
    
    private Vector3 velocityToSet;

    private bool enableMovementOnNextTouch;

    #endregion
    
    #region Rewired

    [Header("Rewired")]
    [SerializeField] private int playerID;

    private Rewired.Player player;
    
    [HideInInspector] public bool fireLeft;
    [HideInInspector] public bool fireRight;
    [HideInInspector] public bool jump;
    [HideInInspector] public bool sprint;
    [HideInInspector] public bool crouch;
    [HideInInspector] public bool dash;
    [HideInInspector] public bool scrollUp;
    [HideInInspector] public bool scrollDown;
    
    private bool readyToJump = true;

    #endregion

    #region Etc

    [HideInInspector] public MovementState state;
    private MovementState lastState;

    public enum MovementState
    {
        unlimited,
        freeze,
        walking,
        sprinting,
        wallrunning,
        air,
        dashing,
        sliding,
        crouching,
        climbing,
        ledge,
        swinging
    }

    [HideInInspector] public bool freeze;
    [HideInInspector] public bool swinging;
    private bool restricted;
    private bool unlimited;
    private bool dashing;
    private bool keepMomentum;

    private float speedChangeFactor;

    #endregion
    
    ////////////////////////////////////////Code/////////////////////////////////////////

    #region General

    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        player = ReInput.players.GetPlayer(playerID);
        fov = cam.fieldOfView;

        Cursor.lockState = CursorLockMode.Locked;

        startYScale = transform.localScale.y;
    }
    
    private void Update()
    {
        CheckInput();
        MoveCam();
        MovePlayer();
        SpeedControl();
        Crouch();
        GroundCheck();
        WallCheck();
        CheckForWall();
        LedgeDetection();
        StateMachine();
        StateHandler();
    }
    
    private void CheckInput()
    {
        moveVector.x = player.GetAxis("Move Horizontal");
        moveVector.z = player.GetAxis("Move Vertical");

        if (!player.GetButton("W") && !player.GetButton("S")) moveVector.z = 0f;
        if (!player.GetButton("A") && !player.GetButton("D")) moveVector.x = 0f;
        
        fireLeft = player.GetButton("Fire Left");
        fireRight = player.GetButton("Fire Right");
        
        lookVector.y += player.GetAxis("Look Horizontal");
        lookVector.x += player.GetAxis("Look Vertical");
        
        lookVector.x = Mathf.Clamp(lookVector.x, minY, maxY);
        
        jump = player.GetButton("Jump");
        sprint = player.GetButton("Sprint");
        crouch = player.GetButton("Crouch");
        dash = player.GetButton("Dash");
        
        if (player.GetAxis("Scroll") > 0)
        {
            scrollUp = true;
            scrollDown = false;
        } else if (player.GetAxis("Scroll") < 0)
        {
            scrollUp = false;
            scrollDown = true;
        }
        else
        {
            scrollUp = false;
            scrollDown = false;
        }

        wallrunUp = player.GetButton("Wallrun Up");
        wallrunDown = player.GetButton("Wallrun Down");

        if (dash) Dash();

        if (jump & grounded & readyToJump)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        switch (crouch)
        {
            case true when (moveVector.x != 0 || moveVector.z != 0) && !didSlide && !isCrouch && !wallrunning:
                StartSlide();
                break;
            case true when !sliding && !wallrunning:
                isCrouch = true;
                break;
            case false when sliding:
                EndSlide();
                didSlide = false;
                break;
            case false:
                didSlide = false;
                isCrouch = false;
                break;
        }
    }

    private void FixedUpdate()
    {
        if (sliding) SlidingMovement();

        if (wallrunning) WallRunningMovement();

        if (climbing && !exitingClimbWall) ClimbingMovement();
    }
    
    private void StateMachine()
    {
        #region Wallrun

        if ((wallLeft || wallRight) && moveVector.z > 0 && AboveGround() && !exitingWall)
        {
            if (!wallrunning) StartWallRun();

            if (wallRunTimer > 0) wallRunTimer -= Time.deltaTime;

            if (wallRunTimer <= 0 && wallrunning)
            {
                exitingWall = true;
                exitWallTimer = exitWallTime;
            }

            if (jump) WallJump();
        }
        else if (exitingWall)
        {
            if (wallrunning) EndWallRun();

            if (exitWallTimer > 0) exitWallTimer -= Time.deltaTime;

            if (exitWallTimer <= 0) exitingWall = false;
        }
        else
        {
            if (wallrunning) EndWallRun();
        }

        #endregion

        #region Climbing

        if (holding)
        {
            if (climbing) EndClimb();
        }
        else if (wallFront && moveVector.z > 0 && wallLookAngle < maxWallLookAngle && !exitingClimbWall)
        {
            if (!climbing && climbTimer > 0) StartClimbing();

            if (climbTimer > 0) climbTimer -= Time.deltaTime;
            if (climbTimer <= 0) EndClimb();
        }
        else if (exitingClimbWall)
        {
            if (climbing) EndClimb();

            switch (exitWallClimbTimer)
            {
                case > 0:
                    exitWallClimbTimer -= Time.deltaTime;
                    break;
                case <= 0:
                    exitingClimbWall = false;
                    break;
            }
        }
        
        else
        {
            if (climbing) EndClimb();
        }
        
        if (wallFront && jump && climbJumpsLeft > 0) ClimbJump();

        #endregion

        #region Ledge Grabbing

        var anyInputPressed = moveVector.x != 0 || moveVector.z != 0;

        if (holding)
        {
            FreezeOnLedge();

            timeOnLedge += Time.deltaTime;

            if (timeOnLedge > minTimeOnLedge && anyInputPressed) ExitLedgeHold();
            
            if (jump) LedgeJump();
        }
        else if (exitingLedge)
        {
            if (exitLedgeTimer > 0) exitLedgeTimer -= Time.deltaTime;
            else exitingLedge = false;
        }

        #endregion

        #region Dashing

        if (dashCooldownTimer > 0) dashCooldownTimer -= Time.deltaTime;

        #endregion
    }
    
    private void StateHandler()
    {
        if (freeze)
        {
            state = MovementState.freeze;
            rb.velocity = Vector3.zero;
            desiredMoveSpeed = 0f;
        } 
        else if (dashing)
        {
            state = MovementState.dashing;
            desiredMoveSpeed = dashSpeed;
            speedChangeFactor = dashSpeedChangeFactor;
        }
        else if (holding || exitingLedge)
        {
            state = MovementState.ledge;
            desiredMoveSpeed = ledgeJumpForwardForce + ledgeJumpUpForce;
        }
        else if (unlimited)
        {
            state = MovementState.unlimited;
            desiredMoveSpeed = 999f;
        }
        else if (swinging)
        {
            state = MovementState.swinging;
            desiredMoveSpeed = swingSpeed;
        }
        else if (climbing)
        {
            state = MovementState.climbing;
            desiredMoveSpeed = climbSpeed;
        }
        else if (wallrunning)
        {
            state = MovementState.wallrunning;
            desiredMoveSpeed = wallrunSpeed;
        }
        else if (sliding)
        {
            state = MovementState.sliding;

            if (OnSlope() && rb.velocity.y < 0.1f)
            {
                desiredMoveSpeed = slideSpeed;
                keepMomentum = true;
            }
            else
            {
                desiredMoveSpeed = runSpeed;
            }
        }
        else if (grounded)
        {
            switch (grounded)
            {
                case true when crouch || above:
                    state = MovementState.crouching;
                    desiredMoveSpeed = crouchSpeed;
                    break;
                case true when sprint:
                    state = MovementState.sprinting;
                    desiredMoveSpeed = runSpeed;
                    break;
                case true:
                    state = MovementState.walking;
                    desiredMoveSpeed = walkSpeed;
                    break;
            }
        } 
        else
        {
            state = MovementState.air;
            keepMomentum = true;
            desiredMoveSpeed = airSpeed;
        }

        
        var desiredMoveSpeedHasChanged = !desiredMoveSpeed.Equals(lastDesiredMoveSpeed);
        
        if (Mathf.Abs(desiredMoveSpeed - moveSpeed) < 0.1f) keepMomentum = false;
        if (lastState == MovementState.dashing) keepMomentum = true;

        if (desiredMoveSpeedHasChanged)
        {
            if (keepMomentum)
            {
                StopAllCoroutines();
                StartCoroutine(SmoothlyLerpMoveSpeed());
            }
            else
            {
                StopAllCoroutines();
                moveSpeed = desiredMoveSpeed;
            }
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
        lastState = state;
    }
    
    private void MoveCam()
    {
        transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.x, lookVector.y * sens, transform.rotation.z));
        camPos.rotation = Quaternion.Euler(-lookVector.x * sens, lookVector.y * sens, 0);
        camHolder.position = camPos.position;
        camHolder.rotation = camPos.rotation;
    }

    private void OnCollisionEnter()
    {
        if (enableMovementOnNextTouch)
        {
            enableMovementOnNextTouch = false;
            ResetRestrictions();

            camHolder.GetComponentInChildren<HookGun>().StopHook();
        }
    }

    private void ResetRestrictions()
    {
        activeHook = false;
        DoFov(fov);
    }

    private void DoFov(float endVal)
    {
        cam.DOFieldOfView(endVal, 0.25f);
    }

    private void DoTilt(float zTilt)
    {
        cam.transform.DOLocalRotate(new Vector3(0, 0, zTilt), 0.25f);
    }

    #endregion

    #region Movement
    private void MovePlayer()
        {
            if (exitingClimbWall) return;
            if (activeHook) return;
            if (state is MovementState.swinging or MovementState.dashing or MovementState.swinging) return;

            moveDir = transform.forward * moveVector.z + transform.right * moveVector.x;

            if (OnSlope() && !exitingSlope)
            {
                rb.AddForce(GetSlopeDirectionMovement(moveDir) * moveSpeed * 2.5f, ForceMode.Force);

                if (moveVector.magnitude != 0) rb.AddForce(Vector3.down * keepOnSlopeForce, ForceMode.Force);
            }

            switch (grounded)
            {
                case true:
                    rb.AddForce(moveDir.normalized * moveSpeed * 10, ForceMode.Force);
                    break;
                case false:
                    rb.AddForce(moveDir.normalized * moveSpeed * 10 * airMult, ForceMode.Force);
                    break;
            }

            if (!wallrunning) rb.useGravity = !OnSlope();
        }
    
    private void Crouch()
    {
        if ((crouch || above) && isCrouch && !wallrunning)
        {
            var wasGrounded = grounded;
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            if (!didCrouchForce && wasGrounded)
            {
                rb.AddForce(Vector3.down * 5, ForceMode.Impulse);
                didCrouchForce = true;
            }
            else didCrouchForce = true;

            above = Physics.Raycast(transform.position, Vector3.up, playerHeight * 0.5f + 0.2f);
        }
        else if (!sliding)
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            didCrouchForce = false;
        }
    }

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        float time = 0;
        var difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        var startVal = moveSpeed;

        var boostFactor = speedChangeFactor;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startVal, desiredMoveSpeed, time / difference);
            time += Time.deltaTime * boostFactor;
            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
        speedChangeFactor = 1f;
        keepMomentum = false;
    }

    private void GroundCheck()
    {
        if (OnSlope())
        {
            grounded = OnSlope();
        }
        else
        {
            grounded = crouch ? Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.25f + 0.3f, whatIsGround)
                : Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);
        }

        if (state is MovementState.walking or MovementState.crouching or MovementState.sprinting && !activeHook)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = 0;
        }
    }

    private void SpeedControl()
    {
        if (activeHook) return;
        
        if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
            {
                rb.velocity = rb.velocity.normalized * moveSpeed;
            }
        }
        else
        {
            var flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            // limit velocity if needed
            if (!(flatVel.magnitude > moveSpeed)) return;
            var limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }

        if (ySpeedCap != 0 && rb.velocity.y > ySpeedCap) rb.velocity = new Vector3(rb.velocity.x, ySpeedCap, rb.velocity.z);
    }

    #endregion

    #region Jumping

    private void Jump()
    {
        exitingSlope = true;
        
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }

    #endregion

    #region Sliding

    private void StartSlide()
    {
        if (!grounded) return;
        
        sliding = true;
        
        transform.localScale = new Vector3(transform.localScale.x, slideYScale, transform.localScale.z);

        if (!didSlideForce)
        {
            rb.AddForce(Vector3.down * 5, ForceMode.Impulse);
            didSlideForce = true;
        }

        slideTimer = maxSlideTime;
        didSlide = true;
    }

    private void SlidingMovement()
    {
        if (!OnSlope() || rb.velocity.y > -0.1f)
        {
            rb.AddForce(moveDir.normalized * slideForce, ForceMode.Force);

            slideTimer -= Time.deltaTime;
        }
        else
        {
            rb.AddForce(GetSlopeDirectionMovement(moveDir) * slideForce, ForceMode.Force);
            rb.AddForce(Vector3.down * 10, ForceMode.Force);
        }
        

        if (slideTimer <= 0)
        {
            EndSlide();
        }
    }

    private void EndSlide()
    {
        sliding = false;
        didSlideForce = false;


        if (!crouch)
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }

    #endregion
    
    #region Wallrun Movement

    private void CheckForWall()
    {
        wallRight = Physics.Raycast(transform.position, transform.right, out rightWallHit, wallCheckDistance, whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -transform.right, out leftWallHit, wallCheckDistance, whatIsWall);
    }
    private void StartWallRun()
    {
        wallrunning = true;

        wallRunTimer = maxWallRunTime;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        
        DoFov(fov + camFovChange);
        if(wallLeft) DoTilt(-camTilt);
        if(wallRight) DoTilt(camTilt);
    }

    private void WallRunningMovement()
    {
        rb.useGravity = useGravity;
        
        var wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        var wallForward = Vector3.Cross(wallNormal, Vector3.up);

        if ((transform.forward - wallForward).magnitude > (transform.forward - -wallForward).magnitude)
        {
            wallForward = -wallForward;
        }
        
        rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

        if (wallrunUp) rb.velocity = new Vector3(rb.velocity.x, wallClimbSpeed, rb.velocity.z);
        
        if (wallrunDown) rb.velocity = new Vector3(rb.velocity.x, -wallClimbSpeed, rb.velocity.z);
        

        if (!(wallLeft && moveVector.x > 0) && !(wallRight && moveVector.x < 0)) rb.AddForce(-wallNormal * 100, ForceMode.Force);
        
        if(useGravity) rb.AddForce(transform.up * gravityCounterForce, ForceMode.Force);
    }

    private void EndWallRun()
    {
        wallrunning = false;
        
        DoFov(fov - camFovChange);
        DoTilt(0f);
    }

    private void WallJump()
    {
        if (grounded) return;
        if (holding || exitingLedge) return;
        
        exitingWall = true;
        exitWallTimer = exitWallTime;
        
        var wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

        var forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;

        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);
    }

    #endregion

    #region Climbing

    private void WallCheck()
    {
        wallFront = Physics.SphereCast(transform.position, sphereCastRadius, transform.forward, out frontWallHit, detectionLength, whatIsWall);
        wallLookAngle = Vector3.Angle(transform.forward, -frontWallHit.normal);

        var newWall = frontWallHit.transform != lastWall || Mathf.Abs(Vector3.Angle(lastWallNormal, frontWallHit.normal)) > minWallAngleChange;

        if ((wallFront && newWall) || grounded)
        {
            climbTimer = maxClimbTime;
            climbJumpsLeft = climbJumps;
        }
    }

    private void StartClimbing()
    {
        climbing = true;

        lastWall = frontWallHit.transform;
        lastWallNormal = frontWallHit.normal;
    }

    private void ClimbingMovement()
    {
        rb.velocity = new Vector3(rb.velocity.x, climbSpeed, rb.velocity.z);
    }

    private void EndClimb()
    {
        climbing = false;
    }

    private void ClimbJump()
    {
        if (grounded) return;
        if (holding || exitingLedge) return;
        
        exitingClimbWall = true;
        exitWallClimbTimer = exitClimbWallTime;
        
        var forceToApply = transform.up * climbJumpUpForce + frontWallHit.normal * climbJumpBackForce;

        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);

        climbJumpsLeft--;
    }

    #endregion

    #region Ledge Grabbing

    private void LedgeDetection()
    {
        var ledgeDetected = Physics.SphereCast(transform.position, ledgeSphereCastRadius, cam.transform.forward, out ledgeHit,
            ledgeDetectionLength, whatIsLedge);
        
        if (!ledgeDetected) return;

        var distanceToLedge = Vector3.Distance(transform.position, ledgeHit.transform.position);
        
        if (ledgeHit.transform == lastLedge) return;
        
        if (distanceToLedge < ledgeGrabDistance && !holding) EnterLedgeHold();
    }

    private void EnterLedgeHold()
    {
        holding = true;

        unlimited = true;
        restricted = true;

        currentLedge = ledgeHit.transform;
        lastLedge = ledgeHit.transform;

        rb.useGravity = false;
        rb.velocity = Vector3.zero;
    }

    private void FreezeOnLedge()
    {
        rb.useGravity = false;

        var directionToLedge = currentLedge.position - transform.position;
        var distanceToLedge = Vector3.Distance(transform.position, currentLedge.position);

        if (distanceToLedge > 1f)
        {
            if (rb.velocity.magnitude < moveToLedgeSpeed) rb.AddForce(directionToLedge.normalized * moveToLedgeSpeed * 1000 * Time.deltaTime);
        }
        else
        {
            if (!freeze) freeze = true;
            if (unlimited) unlimited = false;
        }

        if (distanceToLedge > ledgeGrabDistance) ExitLedgeHold();
    }

    private void ExitLedgeHold()
    {
        exitingLedge = true;
        exitLedgeTimer = exitLedgeTime;
        holding = false;
        timeOnLedge = 0;
        
        restricted = false;
        freeze = false;

        rb.useGravity = true;
        
        //StopAllCoroutines();
        Invoke(nameof(ResetLastLedge), 1);
    }

    private void ResetLastLedge()
    {
        lastLedge = null;
    }

    private void LedgeJump()
    {
        ExitLedgeHold();

        Invoke(nameof(DelayedJumpForce), 0.05f);
    }

    private void DelayedJumpForce()
    {
        var forceToAdd = camHolder.forward * ledgeJumpForwardForce + transform.up * ledgeJumpUpForce;
        rb.velocity = Vector3.zero;
        rb.AddForce(forceToAdd, ForceMode.Impulse);
    }

    #endregion

    #region Dashing

    private void Dash()
    {
        if (dashCooldownTimer > 0) return;
        
        dashCooldownTimer = dashCooldown;

        dashing = true;
        lastYSpeedCap = ySpeedCap;
        ySpeedCap = maxDashYSpeed;
        
        DoFov(fov + dashFov);

        var forwardT = useCamForward ? camHolder : transform;

        var direction = GetDirection(forwardT);
        var forceToApply = direction * dashForce + transform.up * dashUpForce;

        if (disableGrav) rb.useGravity = false;

        delayedForceToApply = forceToApply;
        Invoke(nameof(DelayedDashForce), 0.025f);
        Invoke(nameof(ResetDash), dashDuration);
    }

    private void DelayedDashForce()
    {
        if (resetVel) rb.velocity = Vector3.zero;
        
        rb.AddForce(delayedForceToApply, ForceMode.Impulse);
    }

    private void ResetDash()
    {
        dashing = false;
        ySpeedCap = lastYSpeedCap;
        
        DoFov(fov - dashFov);

        if (disableGrav) rb.useGravity = true;
    }

    private Vector3 GetDirection(Transform forwardT)
    {
        var direction = allowAllDir ? moveDir + forwardT.forward : forwardT.forward;

        if (moveVector.x == 0 && moveVector.z == 0) direction = forwardT.forward;

        return direction.normalized;
    }

    #endregion

    #region Hooking

    public void JumpToPosition(Vector3 targetPos, float trajectoryHeight)
    {
        activeHook = true;
        
        velocityToSet = CalculateJumpVelocity(transform.position, targetPos, trajectoryHeight);
        Invoke(nameof(SetVelocity), 0.1f);
        
        Invoke(nameof(ResetRestrictions), 3f);
    }

    private void SetVelocity()
    {
        enableMovementOnNextTouch = true;
        rb.velocity = velocityToSet;
        
        DoFov(fov + hookFovChange);
    }

    private static Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    {
        var gravity = Physics.gravity.y;
        var displacementY = endPoint.y - startPoint.y;
        var displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

        var velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
        var velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity) + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));

        return velocityXZ + velocityY;
    }

    #endregion
}
