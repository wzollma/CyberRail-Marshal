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
    public float RELEASE_EPSILON = 1;
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
        }
        else
        {
            grapplePoint = camTrans.position + camTrans.forward * maxGrappleDistance;

            Invoke(nameof(stopGrapple), grappleDelayTime);
        }
    }

    void executeGrapple()
    {
        Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);

        float grapplePointRelativeYPos = grapplePoint.y - lowestPoint.y;
        float highestPointOnArc = grapplePointRelativeYPos + overshootYAxis;

        if (grapplePointRelativeYPos < 0)
            highestPointOnArc = overshootYAxis;

        movementScript.jumpToPosition(grapplePoint, highestPointOnArc);

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
