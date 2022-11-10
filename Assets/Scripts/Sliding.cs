using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerRbMovement))]
[RequireComponent(typeof(PlayerRbInput))]
public class Sliding : MonoBehaviour
{
    //[Header("References")]
    Rigidbody rb;
    PlayerRbMovement movementScript;
    PlayerRbInput input;

    [Header("Sliding")]
    [SerializeField] float slideStopVelocity;
    [SerializeField] float minSlideTime;
    public float maxSlideTime;
    public float slideForce;
    float timeSliding;

    public float slideYScale;

    private bool yetToConsumeSlidePress;
   
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        movementScript = GetComponent<PlayerRbMovement>();
        input = GetComponent<PlayerRbInput>();
    }
    
    void Update()
    {
        if (!movementScript.isSliding && input.slideDown)
            yetToConsumeSlidePress = true;
        else if (yetToConsumeSlidePress && !input.isSlide)
            yetToConsumeSlidePress = false;

        if (yetToConsumeSlidePress && input.isMovement)
            StartSlide();
        else if (movementScript.isSliding && (input.slideUp || (movementScript.getFlatVelocity().magnitude < slideStopVelocity) && timeSliding > minSlideTime))
            movementScript.StopSlide();
    }

    private void FixedUpdate()
    {
        if (movementScript.isSliding)
            SlidingMovement();
    }

    void StartSlide()
    {
        yetToConsumeSlidePress = false;
        movementScript.setIsSliding(true);        

        movementScript.setYScale(slideYScale);
        movementScript.pressPlayerToGround();

        timeSliding = 0;
    }

    void SlidingMovement()
    {
        Vector3 inputDirection = input.getInputDirection().normalized;

        // sliding normal
        if (!movementScript.OnSlope() || rb.velocity.y > -PlayerRbMovement.Y_VEL_EPSILON)
        {
            rb.AddForce(inputDirection * slideForce, ForceMode.Force);

            timeSliding += Time.deltaTime;
        }
        else // sliding down a slope
        {
            rb.AddForce(movementScript.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);
        }

        if (timeSliding >= maxSlideTime)
            movementScript.StopSlide();
    }
}
