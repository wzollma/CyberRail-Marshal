using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerRbInput))]
public class PlayerRbMovement : MonoBehaviour
{
    public const float Y_VEL_EPSILON = .1f;
    const float SPEED_DIFF_LERP_THRESHOLD = 4f;

    [Header("Movement")]
    [SerializeField] float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float slideSpeed;
    public float wallRunSpeed;

    float desiredMoveSpeed;
    float lastDesiredMoveSpeed;

    [SerializeField] float speedIncreaseMultiplier;
    [SerializeField] float slopeIncreaseMultipler;

    public float groundDrag;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    float startYScale;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    RaycastHit slopeHit;
    private bool exitingSlope;

    Vector3 moveDirection;

    Rigidbody rb;
    PlayerRbInput input;
    PlayerCam cam;

    public MovementState state;
    public enum MovementState { WALKING, SPRINTING, WALLRUNNING, CROUCHING, SLIDING, AIR }

    public bool isSliding { get { return sliding; } } 

    bool sliding;
    public bool wallRunning;
    float timeGroundedSlideStarted;

    private void Start()
    {
        cam = Camera.main.GetComponent<PlayerCam>();
        input = GetComponent<PlayerRbInput>();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        ResetJump();

        startYScale = transform.localScale.y;
    }

    private void Update()
    {
        // ground check
        bool prevGrounded = grounded;
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * .5f + .2f, whatIsGround);
        if (grounded && (!prevGrounded || (sliding && !isState(MovementState.SLIDING))))
            timeGroundedSlideStarted = Time.time;

        HandleInput();
        StateHandler();

        // handle drag
        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    public void resetYScale()
    {
        //setYScale(startYScale);
        transform.DOScaleY(startYScale, .1f);
    }

    public void setYScale(float newYScale)
    {
        Vector3 prevScale = transform.localScale;
        transform.localScale = new Vector3(prevScale.x, newYScale, prevScale.z);
    }

    public void pressPlayerToGround()
    {
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
    }

    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * .5f + .3f, whatIsGround))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    public Vector3 getFlatVelocity()
    {
        return new Vector3(rb.velocity.x, 0, rb.velocity.z);
    }

    public void setDesiredMoveSpeed(float speed)
    {
        desiredMoveSpeed = speed;
    }

    public bool isState(MovementState state)
    {
        return this.state.Equals(state);
    }

    public float getMoveSpeed()
    {
        return moveSpeed;
    }

    public void setIsSliding(bool shouldSlide)
    {
        sliding = shouldSlide;
    }

    public RaycastHit getSlopeHit()
    {
        return slopeHit;
    }

    private void HandleInput()
    {
        // when to jump
        if (input.isJump && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // handle crouching
     
        if (input.crouchDown) // start crouch
        {
            setYScale(crouchYScale);
            pressPlayerToGround();
        }
        else if (input.crouchUp) // stop crouch
        {
            resetYScale();
        }
    }

    void StateHandler()
    {
        // Mode - Wallrunning
        if (wallRunning)
        {
            state = MovementState.WALLRUNNING;
            setDesiredMoveSpeed(wallRunSpeed);
        }        
        else if (sliding) // Mode - Sliding
        {
            // sets up sliding speed
            if (!isState(MovementState.SLIDING))
            {
                state = MovementState.SLIDING;

                moveSpeed = sprintSpeed;
                setDesiredMoveSpeed(sprintSpeed);                
            }
                        
            if (grounded) // increases speed on slopes, and decreases when not
                setDesiredMoveSpeed(isGoingDownSlope() ? slideSpeed : 0);
            else // retains speed if the player is in the air
                setDesiredMoveSpeed(moveSpeed);
        }        
        else if (input.isCrouch) // Mode - Crouching
        {
            state = MovementState.CROUCHING;
            setDesiredMoveSpeed(crouchSpeed);
        }        
        else if (grounded && input.isSprint) // Mode - Sprinting
        {
            state = MovementState.SPRINTING;

            if (moveSpeed < sprintSpeed)
                moveSpeed = sprintSpeed;
            setDesiredMoveSpeed(sprintSpeed);            
        }
        else if (grounded) // Mode - Walking
        {
            state = MovementState.WALKING;

            if (moveSpeed < walkSpeed)
                moveSpeed = walkSpeed;
            else if (moveSpeed == sprintSpeed)
                moveSpeed = walkSpeed;
            setDesiredMoveSpeed(walkSpeed);
        }
        else // Mode - Air
        {
            state = MovementState.AIR;
        }

        if (!isState(MovementState.SLIDING) && moveSpeed < walkSpeed)
        {
            moveSpeed = isState(MovementState.CROUCHING) ? crouchSpeed : walkSpeed;
            lastDesiredMoveSpeed = moveSpeed;
        }

        // check if desiredMoveSpeed has changed drastically
        if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > SPEED_DIFF_LERP_THRESHOLD)
        {            
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed(desiredMoveSpeed == 0 ? 10 : 1));
        }
        else
        {
            moveSpeed = desiredMoveSpeed;
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
    }

    void MovePlayer()
    {
        // calc movement direction
        moveDirection = input.getInputDirection();

        // on slope
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);

            if (isGoingDownSlope())
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }

        // on ground
        if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        else if (!grounded) // in air
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);            

        SpeedControl();

        // turn gravity off while on slope
        rb.useGravity = !OnSlope();
    }

    void SpeedControl()
    {
        // limiting speed on slope
        if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }
        else // limiting speed on ground or in air
        {
            Vector3 flatVel = getFlatVelocity();

            // limit vel if needed
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }        

    }

    public bool isGoingDownSlope()
    {
        return OnSlope() && rb.velocity.y < Y_VEL_EPSILON && input.isMovement;
    }

    void Jump()
    {
        float heightMultiplier = 1;

        if (isState(MovementState.SLIDING))
        {
            StopSlide();

            if (Time.time - timeGroundedSlideStarted >= .2f)
                heightMultiplier *= 1.5f;
        }

        exitingSlope = true;

        // reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce * heightMultiplier, ForceMode.Impulse);
    }

    void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }

    public void StopSlide()
    {
        setIsSliding(false);

        resetYScale();

        if (moveSpeed < walkSpeed)
        {
            moveSpeed = isState(MovementState.CROUCHING) ? crouchSpeed : walkSpeed;
            lastDesiredMoveSpeed = moveSpeed;
        }

        cam.DoIncFov(0);
    }

    IEnumerator SmoothlyLerpMoveSpeed(float changeModifier)
    {
        // smoothly lerp movementSpeed to desired value
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);
            if (moveSpeed < 0)
                moveSpeed = 0;

            float timeIncrease = Time.deltaTime * speedIncreaseMultiplier;
            // gives faster speed increase on slopes, exaggerated the steeper the slope
            if (OnSlope() && rb.velocity.y < -Y_VEL_EPSILON)
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                timeIncrease *= slopeIncreaseMultipler * slopeAngleIncrease;
            }

            time += timeIncrease * changeModifier;
                
            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
    }

    /*private void OnDrawGizmosSelected()
    {
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * (playerHeight * .5f + .2f));
    }*/
}
