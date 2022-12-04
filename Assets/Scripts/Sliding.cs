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
    PlayerCam cam;

    [Header("Sliding")]
    [SerializeField] float slideStopVelocity;
    [SerializeField] float minSlideTime;
    [SerializeField] float minFastSlideFovInc = 10;
    [SerializeField] float maxSlideFovInc = 30;
    [SerializeField] float slideFovMult = 2;
    [SerializeField] float fovChangeEpsilon = 2;
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
        cam = Camera.main.GetComponent<PlayerCam>();
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
            //Debug.Log("force - sliding");
            rb.AddForce(inputDirection * slideForce, ForceMode.Force);

            timeSliding += Time.deltaTime;
        }
        else // sliding down a slope
        {
            //Debug.Log("force - sliding down slope");
            rb.AddForce(movementScript.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);
        }

        if (timeSliding >= maxSlideTime)
            movementScript.StopSlide();
        else
        {
            float curSpeed = movementScript.getMoveSpeed();
            float minFovInc = minFastSlideFovInc; //(curSpeed > movementScript.walkSpeed) ? minFastSlideFovInc : 0;
            float newTargetFovInc = minFovInc + (curSpeed - movementScript.walkSpeed) * slideFovMult;
            if (newTargetFovInc < 0)
                newTargetFovInc = 0;


            Debug.Log($"prev: {cam.getLastSetFovInc()}     new: {newTargetFovInc}     speed: {curSpeed}");

            if (Mathf.Abs(newTargetFovInc - cam.getLastSetFovInc()) > fovChangeEpsilon)
                cam.DoIncFov(newTargetFovInc);
        }
    }
}
