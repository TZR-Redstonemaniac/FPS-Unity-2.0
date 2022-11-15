using System.Collections;
using UnityEngine;
using Rewired;

public class Player : MonoBehaviour
{
    ////////////////////////////////////////Public Variables/////////////////////////////////////////

    [Header("Movement")]
    [SerializeField] private float groundDrag;
    [SerializeField] private float jumpPower;
    [SerializeField] private float airMult;

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
    private bool grounded;

    private float moveSpeed;

    /////////////////////////////////////////////////Code/////////////////////////////////////////////////
    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        player = ReInput.players.GetPlayer(playerID);

        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        CheckInput();
        MoveCam();
        MovePlayer();
        SpeedControl();
        
        if (jump & grounded)
        {
            Jump();
        }

        GroundCheck();
    }

    void CheckInput()
    {
        moveVector.x = player.GetAxis("Move Horizontal");
        moveVector.z = player.GetAxis("Move Vertical");
        
        fireLeft = player.GetButtonDown("Fire Left");
        fireRight = player.GetButtonDown("Fire Right");
        jump = player.GetButton("Jump");
        
        lookVector.y += player.GetAxis("Look Horizontal");
        lookVector.x += player.GetAxis("Look Vertical");
        lookVector.x = Mathf.Clamp(lookVector.x, minY, maxY);
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

        if (flatVel.magnitude > moveSpeed)
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
