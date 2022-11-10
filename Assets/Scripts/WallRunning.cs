using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRunning : MonoBehaviour
{
    [Header("Wallrunning")]
    public LayerMask whatIsWall;
    public LayerMask whatIsGround;
    public float wallRunForce;
    public float wallJumpUpForce;
    public float wallJumpSideForce;
    public float maxWallRunTime;
    float wallRunTimer;

    [Header("Detection")]
    public float wallCheckDistance;
    public float minJumpHeight;
    RaycastHit leftWallHit;
    RaycastHit rightWallHit;
    bool wallLeft;
    bool wallRight;

    [Header("Exiting")]
    bool exitingWall;
    public float exitWallTime;
    float exitWallTimer;

    [Header("References")]
    PlayerCam cam;
    PlayerRbMovement movementScript;
    PlayerRbInput input;
    Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        movementScript = GetComponent<PlayerRbMovement>();
        input = GetComponent<PlayerRbInput>();
        rb = GetComponent<Rigidbody>();
        cam = Camera.main.GetComponent<PlayerCam>();
    }

    // Update is called once per frame
    void Update()
    {
        CheckForWall();
        StateMachine();
    }

    void FixedUpdate()
    {
        if (movementScript.wallRunning)
            WallRunningMovement();
    }

    void CheckForWall()
    {
        wallRight = Physics.Raycast(transform.position, input.orientation.right, out rightWallHit, wallCheckDistance, whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -input.orientation.right, out leftWallHit, wallCheckDistance, whatIsWall);
    }

    bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
    }

    void StateMachine()
    {
        // State 1 - Wallrunning
        if ((wallLeft || wallRight) && input.moveInput.y > 0 && AboveGround() && !exitingWall)
        {
            if (!movementScript.wallRunning)
                StartWallRun();

            // wallrun timer
            if (wallRunTimer > 0)
                wallRunTimer -= Time.deltaTime;

            if (wallRunTimer <= 0 && movementScript.wallRunning)
            {
                exitingWall = true;
                exitWallTimer = exitWallTime;
            }

            // wall jump
            if (input.jumpDown)
                WallJump();
        }
        else if (exitingWall) // State 2 - Exiting
        {
            if (movementScript.wallRunning)
                StopWallRun();

            if (exitWallTimer > 0)
                exitWallTimer -= Time.deltaTime;

            if (exitWallTimer <= 0)
                exitingWall = false;
        }
        else // State 3 - None
        {
            if (movementScript.wallRunning)
                StopWallRun();
        }
    }

    void StartWallRun()
    {
        movementScript.wallRunning = true;

        wallRunTimer = maxWallRunTime;

        // apply camera effects
        cam.toggleWallRunEffects(true, wallLeft, wallRight);
    }

    void WallRunningMovement()
    {
        rb.useGravity = false;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        // makes the player wallRun in the direction they're facing
        if ((input.orientation.forward - wallForward).magnitude > (input.orientation.forward - -wallForward).magnitude)
            wallForward = -wallForward;

        // forward force
        rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

        // push player toward wall they are trying to run on
        if (!(wallLeft && input.moveInput.x > 0) && !(wallRight && input.moveInput.x < 0))
            rb.AddForce(-wallNormal * 100, ForceMode.Force);
    }

    void StopWallRun()
    {
        movementScript.wallRunning = false;

        // reset camera effects
        cam.toggleWallRunEffects(false, wallLeft, wallRight);
    }

    void WallJump()
    {
        // enter exiting wall state
        exitingWall = true;
        exitWallTimer = exitWallTime;

        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

        Vector3 forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;

        // reset y velocity and add force
        rb.velocity = movementScript.getFlatVelocity();
        rb.AddForce(forceToApply, ForceMode.Impulse);
    }
}