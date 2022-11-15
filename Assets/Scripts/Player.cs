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
    [SerializeField] private float airMult;

    [Header("Crouching")] 
    [SerializeField] private float crouchSpeed;
    [SerializeField] private float crouchYScale;
    
    [Header("Ground Check")] 
    [SerializeField] private float playerHeight;
    [SerializeField] private LayerMask whatIsGround;
    
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

    private bool fireLeft;
    private bool fireRight;
    private bool jump;
    private bool sprint;
    private bool crouch;
    private bool didCrouchForce;
    
    private bool grounded;
    private bool above;

    private float moveSpeed;
    private float startYScale;

    [HideInInspector] public MovementState state;

    /////////////////////////////////////////////////Code/////////////////////////////////////////////////

    public enum MovementState
    {
        walking,
        sprinting,
        air,
        crouching
    }
    
    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        player = ReInput.players.GetPlayer(playerID);

        Cursor.lockState = CursorLockMode.Locked;

        startYScale = transform.localScale.y;
    }

    // Update is called once per frame
    void Update()
    {
        CheckInput();
        MoveCam();
        MovePlayer();
        SpeedControl();
        StateHandler();
        Crouch();
        
        if (jump & grounded)
        {
            Jump();
        }

        GroundCheck();
    }

    void Crouch()
    {
        if (crouch || above)
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            if (!didCrouchForce)
            {
                rb.AddForce(Vector3.down * 5, ForceMode.Impulse);
                didCrouchForce = true;
            }
            above = Physics.Raycast(transform.position, Vector3.up, playerHeight * 0.25f + 0.2f);
        }
        else
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            didCrouchForce = false;
        }

    }

    void CheckInput()
    {
        moveVector.x = player.GetAxis("Move Horizontal");
        moveVector.z = player.GetAxis("Move Vertical");
        
        fireLeft = player.GetButtonDown("Fire Left");
        fireRight = player.GetButtonDown("Fire Right");
        
        lookVector.y += player.GetAxis("Look Horizontal");
        lookVector.x += player.GetAxis("Look Vertical");
        
        lookVector.x = Mathf.Clamp(lookVector.x, minY, maxY);
        
        jump = player.GetButton("Jump");
        sprint = player.GetButton("Sprint");
        crouch = player.GetButton("Crouch");
    }

    void StateHandler()
    {
        switch (grounded)
        {
            case true when crouch:
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

    void MovePlayer()
    {
        var moveDir = transform.forward * moveVector.z + transform.right * moveVector.x;

        switch (grounded)
        {
            case true:
                rb.AddForce(moveDir.normalized * moveSpeed * 10, ForceMode.Force);
                break;
            case false:
                rb.AddForce(moveDir.normalized * moveSpeed * 10 * airMult, ForceMode.Force);
                break;
        }
    }

    void GroundCheck()
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

    void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if (flatVel.magnitude > speedCap)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
    }

    void MoveCam()
    {
        transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.x, lookVector.y * sens, transform.rotation.z));
        camPos.rotation = Quaternion.Euler(-lookVector.x * sens, lookVector.y * sens, 0);
        cam.transform.position = camPos.position;
        cam.transform.rotation = camPos.rotation;
    }
}
