using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grappling : MonoBehaviour
{
    [Header("References")]
    PlayerRbMovement movementScript;
    PlayerRbInput inputScript;
    Transform camTrans;
    public Transform shootPoint;
    public LayerMask grappleLayers;

    [Header("Grappling")]
    [SerializeField] float maxGrappleDistance;
    [SerializeField] float grappleDelayTime;
    [SerializeField] float overshootYAxis;
    [SerializeField] float releaseDistance = 1;
    [SerializeField] float grappleSpeed = 10;
    Vector3 grappleDirec;
    public bool canGrappleJump = true;

    Vector3 grapplePoint;

    [Header("Cooldown")]
    [SerializeField] float grapplingCd;
    private float grapplingCdTimer;

    bool grappling;

    void Start()
    {
        movementScript = GetComponent<PlayerRbMovement>();
        inputScript = GetComponent<PlayerRbInput>();
        camTrans = Camera.main.transform;
    }

    void Update()
    {
        if (inputScript.grappleDown)
            startGrapple();

        if (grapplingCdTimer > 0)
            grapplingCdTimer -= Time.deltaTime; 
    }

    void FixedUpdate()
    {
        if (movementScript.enableMovementOnNextTouch || grappling)
        {
            Debug.Log("setting force");
            movementScript.rb.AddForce(grappleDirec * grappleSpeed * 20, ForceMode.VelocityChange);
        }
        else
        {
            movementScript.enableMovementOnNextTouch = false;
            if (grappling)
            {
                if (movementScript.isState(PlayerRbMovement.MovementState.GRAPPLING))
                    movementScript.processGetOutOfGrappleState();
                else
                    stopGrapple();
            }
        }
    }

    void startGrapple()
    {
        if (grapplingCdTimer > 0)
            return;

        grappling = true;

        RaycastHit hit;
        if (Physics.Raycast(camTrans.position, camTrans.forward, out hit, maxGrappleDistance, grappleLayers))
        {
            grapplePoint = hit.point;

            Invoke(nameof(executeGrapple), grappleDelayTime);
            //StartCoroutine(executeGrapple());
        }
        else
        {
            grapplePoint = camTrans.position + camTrans.forward * maxGrappleDistance;

            Invoke(nameof(stopGrapple), grappleDelayTime);
        }
    }

    void executeGrapple()
    {
        //Debug.Log("executing grapple");
        //// waits for animation
        //yield return new WaitForSeconds(grappleDelayTime);

        Vector3 grappleDirec = (grapplePoint - getGrappleShootPoint()).normalized;
        movementScript.enableMovementOnNextTouch = true;
        movementScript.rb.AddForce(grappleDirec * grappleSpeed * 30, ForceMode.VelocityChange);
        //bool doneOnce = false;
        //while (grappling && Vector3.Distance(getGrappleShootPoint(), grapplePoint) > releaseDistance)
        //{
        //    //movementScript.rb.useGravity = true;            
        //    if (!doneOnce)
        //    {
        //        movementScript.enableMovementOnNextTouch = true;
        //    }
        //        movementScript.rb.AddForce(grappleDirec * grappleSpeed * 30, ForceMode.Acceleration);
        //    if (!movementScript.rb.useGravity)
        //        Debug.Log("no gravity");
        //    doneOnce = true;
        //    yield return null;
        //}

        //Debug.Log("stopped grapple");
        //movementScript.enableMovementOnNextTouch = false;
        //if (grappling)
        //{
        //    if (movementScript.isState(PlayerRbMovement.MovementState.GRAPPLING))
        //        movementScript.processGetOutOfGrappleState();
        //    else
        //        stopGrapple();
        //}
        //movementScript.rb.useGravity = true;

        //Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);

        //float grapplePointRelativeYPos = grapplePoint.y - lowestPoint.y;
        //float highestPointOnArc = grapplePointRelativeYPos + overshootYAxis;

        //if (grapplePointRelativeYPos < 0)
        //    highestPointOnArc = overshootYAxis;

        //movementScript.jumpToPosition(grapplePoint, highestPointOnArc);

        Invoke(nameof(stopGrapple), 1f);
    }

    public void stopGrapple()
    {
        grappling = false;

        grapplingCdTimer = grapplingCd;
    }

    public bool getGrappling()
    {
        return grappling;
    }

    public Vector3 getGrapplePoint()
    {
        return grapplePoint;
    }

    public Vector3 getGrappleShootPoint()
    {
        return shootPoint.position;
    }
}
