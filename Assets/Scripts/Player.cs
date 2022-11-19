using System.Collections;
using UnityEngine;
using Rewired;

public class Player : MonoBehaviour
{
    ////////////////////////////////////////Public Variables/////////////////////////////////////////

    [Header("Movement")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float speedCap;
    [SerializeField] private float groundDrag;

    [Header("Jumping")]
    [SerializeField] private float jumpPower;
    [SerializeField] private float jumpCooldown;
    [SerializeField] private float airMult;

    [Header("Crouching")] 
    [SerializeField] private float crouchSpeed;
    [SerializeField] private float crouchYScale;
    
    [Header("Ground Check")] 
    [SerializeField] private float playerHeight;
    [SerializeField] private LayerMask whatIsGround;
    
    [Header("Slope Handling")]
    [SerializeField] private float maxSlopeAngle;
    
    [Header("Camera")]
    [SerializeField] private float minY = -80;
    [SerializeField] private float maxY = 80;
    [SerializeField] private float sens = 1;
    [SerializeField] private Transform camPos;
    [SerializeField] private Camera cam;
    
    [Header("Rewired")]
    [SerializeField] private int playerID;

    ////////////////////////////////////////Private Variables////////////////////////////////////////

    private Rigidbody rb;

    private Rewired.Player player;

    private Vector3 moveVector;
    private Vector3 lookVector;
    private Vector3 moveDir;

    private bool fireLeft;
    private bool fireRight;
    private bool jump;
    private bool sprint;
    private bool crouch;
    private bool didCrouchForce;
    
    private bool grounded;
    private bool above;
    private bool readyToJump = true;
    private bool exitingSlope;

    private float moveSpeed;
    private float startYScale;

    private RaycastHit slopeHit;

    [HideInInspector] public MovementState state;

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

    private Vector3 GetSlopeDirectionMovement()
    {
        return Vector3.ProjectOnPlane(moveDir, slopeHit.normal).normalized;
    }

    /////////////////////////////////////////////////Code/////////////////////////////////////////////////

    public enum MovementState
    {
        walking,
        sprinting,
        air,
        crouching
    }
    
    // Start is called before the first frame update
    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        player = ReInput.players.GetPlayer(playerID);

        Cursor.lockState = CursorLockMode.Locked;

        startYScale = transform.localScale.y;
    }

    // Update is called once per frame
    private void Update()
    {
        CheckInput();
        MoveCam();
        MovePlayer();
        SpeedControl();
        StateHandler();
        Crouch();
        GroundCheck();
    }

    private void Crouch()
    {
        if (crouch || above)
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            if (!didCrouchForce)
            {
                rb.AddForce(Vector3.down * 5, ForceMode.Impulse);
                didCrouchForce = true;
            }
            above = Physics.Raycast(transform.position, Vector3.up, playerHeight * 0.5f + 0.2f);
        }
        else
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            didCrouchForce = false;
        }

    }

    private void CheckInput()
    {
        moveVector.x = player.GetAxis("Move Horizontal");
        moveVector.z = player.GetAxis("Move Vertical");

        if (!player.GetButton("W") && !player.GetButton("S"))
        {
            moveVector.z = 0f;
        }

        if (!player.GetButton("A") && !player.GetButton("D"))
        {
            moveVector.x = 0f;
        }

        fireLeft = player.GetButtonDown("Fire Left");
        fireRight = player.GetButtonDown("Fire Right");
        
        lookVector.y += player.GetAxis("Look Horizontal");
        lookVector.x += player.GetAxis("Look Vertical");
        
        lookVector.x = Mathf.Clamp(lookVector.x, minY, maxY);
        
        jump = player.GetButton("Jump");
        sprint = player.GetButton("Sprint");
        crouch = player.GetButton("Crouch");

        if (jump & grounded & readyToJump)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void StateHandler()
    {
        switch (grounded)
        {
            case true when crouch || above:
                state = MovementState.crouching;
                moveSpeed = crouchSpeed;
                break;
            case true when sprint:
                state = MovementState.sprinting;
                moveSpeed = runSpeed;
                break;
            case true:
                state = MovementState.walking;
                moveSpeed = walkSpeed;
                break;
            default:
                state = MovementState.air;
                break;
        }
    }

    private void MovePlayer()
    {
        moveDir = transform.forward * moveVector.z + transform.right * moveVector.x;

        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeDirectionMovement() * moveSpeed * 10f, ForceMode.Force);

            if (rb.velocity.y > 0)
            {
                rb.AddForce(Vector3.down * 80, ForceMode.Force);
            }
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

        rb.useGravity = !OnSlope();
    }

    private void GroundCheck()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        if (grounded)
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
    }

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

    private void MoveCam()
    {
        transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.x, lookVector.y * sens, transform.rotation.z));
        camPos.rotation = Quaternion.Euler(-lookVector.x * sens, lookVector.y * sens, 0);
        cam.transform.position = camPos.position;
        cam.transform.rotation = camPos.rotation;
    }
}
