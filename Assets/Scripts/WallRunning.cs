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
    public float minWallRunSpeed = .5f;
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

    //[Header("References")]
    PlayerCam cam;
    PlayerRbMovement movementScript;
    PlayerRbInput input;
    Rigidbody rb;

    Vector3 wallForward;
    public float speedAtStartWallRun;
    float lastTimeWallRunning;
    WallRunInfo lastWallRunInfo;

    //[HideInInspector] public 

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
        //RaycastHit hitInfo;

        //Debug.Log("right raycast");
        wallRight = testWallRayCast(input.orientation.right, out rightWallHit);
        //wallRight = wallRight || testWallRayCast((input.orientation.right * 2 + input.orientation.forward).normalized, out rightWallHit);
        wallRight = wallRight || testWallRayCast((input.orientation.right * 2 - input.orientation.forward).normalized, out rightWallHit);

        //Debug.Log("left raycast");
        wallLeft = testWallRayCast(-input.orientation.right, out leftWallHit);
        //wallLeft = wallLeft || testWallRayCast((-input.orientation.right * 2 + input.orientation.forward).normalized, out leftWallHit);
        wallLeft = wallLeft || testWallRayCast((-input.orientation.right * 2 - input.orientation.forward).normalized, out leftWallHit);
    }

    bool testWallRayCast(Vector3 dir, out RaycastHit hitInfo)
    {
        bool ret = Physics.Raycast(transform.position, dir, out hitInfo, wallCheckDistance, whatIsWall);
        //Debug.Log($"ret: {ret}    hit: {hitInfo}");
        return ret;
    }

    bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
    }

    void StateMachine()
    {
        // State 1 - Wallrunning
        if (isWallRunning())
        {
            if (!movementScript.wallRunning)
            {
                Collider wallToJumpOnCollider = wallRight ? rightWallHit.collider : leftWallHit.collider;
                Vector3 wallToJumpOnNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

                WallRunInfo newInfo = new WallRunInfo(wallToJumpOnCollider, wallToJumpOnNormal);
                if (lastWallRunInfo != null && wallToJumpOnCollider != null && wallToJumpOnNormal != null && lastWallRunInfo.Equals(newInfo))
                {
                    //if (lastWallRunInfo != null && lastWallRunInfo.matchesWallCollider(wallToJumpOnCollider))
                    //Debug.Log("same wall");
                    return;
                }
                else
                {
                    lastTimeWallRunning = Time.time;
                    lastWallRunInfo = newInfo;
                    //Debug.Log("not same wall");
                }

                StartWallRun();
            }

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
            // NOTE: if anything more is adding in State 3, make sure those are added to the return; case of State 1
            if (movementScript.wallRunning)
                StopWallRun();
        }
    }

    void StartWallRun()
    {
        //Debug.Log("StartWallRun()");
        movementScript.wallRunning = true;

        wallRunTimer = maxWallRunTime;

        // apply camera effects
        cam.toggleWallRunEffects(true, wallLeft, wallRight);
        RaycastHit hitInfoToUse = wallRight ? rightWallHit : leftWallHit;
        Vector3 wallNormal = hitInfoToUse.normal;        
        speedAtStartWallRun = Vector3.Project(rb.velocity, Vector3.Cross(wallNormal, Vector3.up)).magnitude;//movementScript.getFlatVelocity().magnitude;
        if (movementScript.activeGrapple)
            speedAtStartWallRun *= .5f;
        if (speedAtStartWallRun < movementScript.walkSpeed)
            speedAtStartWallRun = movementScript.walkSpeed;
    }

    void WallRunningMovement()
    {
        rb.useGravity = false;
        //Debug.Log($"{Time.time} - wallrunning, gravity? {rb.useGravity}");
        rb.velocity = Vector3.zero;//new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        RaycastHit hitInfoToUse = wallRight ? rightWallHit : leftWallHit;
        Vector3 wallNormal = hitInfoToUse.normal;
        //Debug.Log($"wallNormal: {wallNormal}");

        Vector3 potentialWallForward = Vector3.Cross(wallNormal, Vector3.up);
        if (potentialWallForward.Equals(Vector3.zero))
        {
            Debug.Log("Cross is (0,0,0)");
            return;
        }

        wallForward = potentialWallForward;

        // makes the player wallRun in the direction they're facing
        if ((forwardDirecForWallRunning() - wallForward).magnitude > (forwardDirecForWallRunning() - -wallForward).magnitude)
            wallForward = -wallForward;

        //Debug.Log($"setting wallForward: {wallForward}");

        // forward force
        //Debug.Log("force - wall running");
        float speedToWallRunAt = movementScript.getMoveSpeed() <= 0 ? movementScript.walkSpeed / 2 : movementScript.getMoveSpeed();
        rb.AddForce(wallForward * wallRunForce * speedToWallRunAt, ForceMode.Force);

        // push player toward wall they are trying to run on        
        if (isWallRunning())
        {
            //Debug.Log("force - pushing toward wall running on");
            rb.AddForce(-wallNormal * 100, ForceMode.Force);
            //Debug.Log("pushing to wall");
            
            lastTimeWallRunning = Time.time;
            lastWallRunInfo = new WallRunInfo(hitInfoToUse.collider, wallNormal);
        }
        else
        {
            //exitingWall = true;
            //exitWallTimer = exitWallTime;
            //Debug.Log($"{Time.deltaTime} falling off");


        }
    }

    bool isFastEnoughToWallRun()
    {        
        float velAbs = Mathf.Abs(movementScript.getFlatVelocity().magnitude);
        //Debug.Log("fastEnoughToWallRun() - " + (velAbs > minWallRunSpeed));
        return velAbs > minWallRunSpeed;
    }

    bool canAngleStayInWallRun(Vector3 wallNormal, Vector3 wallForward)
    {
        return true;
    }

    public void StopWallRun()
    {
        //Debug.Log("StopWallRun()");
        movementScript.wallRunning = false;

        // reset camera effects
        cam.toggleWallRunEffects(false, wallLeft, wallRight);
    }

    void WallJump()
    {
        // enter exiting wall state
        exitingWall = true;
        exitWallTimer = exitWallTime;

        if (movementScript.isState(PlayerRbMovement.MovementState.GRAPPLING))
            movementScript.processGetOutOfGrappleState();

        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

        Vector3 forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;

        // reset y velocity and add force
        rb.velocity = movementScript.getFlatVelocity();
        //Debug.Log("force - wall jumping");
        rb.AddForce(forceToApply, ForceMode.Impulse);

        //Debug.Log("wall jumping");
    }

    Vector3 forwardDirecForWallRunning()
    {
        return input.orientation.forward;
    }

    public bool isWallRunning()
    {
        return (wallLeft || wallRight) && AboveGround() && !exitingWall && input.moveInput.magnitude > 0 && isFastEnoughToWallRun();
    }

    public void resetLastWallRunInfo()
    {
        //Debug.Log("reset wallRunInfo");
        lastWallRunInfo = null;
    }

    private void OnDrawGizmos()
    {
        //if (input == null)
        //    Start();

        //Gizmos.DrawLine(transform.position, transform.position + forwardDirecForWallRunning() * 2);

        Gizmos.DrawLine(transform.position, transform.position + wallForward * 2);
    }
}

public class WallRunInfo
{
    public Collider wallCollider;
    public Vector3 wallNormal;

    public WallRunInfo(Collider wallCollider, Vector3 wallNormal)
    {
        this.wallCollider = wallCollider;
        this.wallNormal = wallNormal;
    }

    public bool matchesWallCollider(Collider otherWallCollider)
    {
        return wallCollider != null && wallCollider.Equals(otherWallCollider);
    }

    public override bool Equals(object obj)
    {
        if (!(obj is WallRunInfo))
            return false;

        WallRunInfo other = obj as WallRunInfo;

        bool colEqual = wallCollider.Equals(other.wallCollider);
        bool normalEqual = wallNormal.Equals(other.wallNormal);

        return colEqual && normalEqual;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}