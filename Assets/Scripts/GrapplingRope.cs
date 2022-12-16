using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingRope : MonoBehaviour
{
    [SerializeField] Grappling grappleScript;    
    [SerializeField] int quality;
    [SerializeField] float damper;
    [SerializeField] float strength;
    [SerializeField] float velocity;
    [SerializeField] float waveCount;
    [SerializeField] float waveHeight;
    [SerializeField] AnimationCurve affectCurve;
    LineRenderer lr;
    Spring spring;
    Vector3 currentGrapplePosition;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        spring = new Spring();
        spring.SetTarget(0);
    }

    void LateUpdate()
    {
        DrawRope();
    }

    void DrawRope()
    {
        if (!grappleScript.getGrappling())
        {
            currentGrapplePosition = grappleScript.getGrappleShootPoint();
            spring.Reset();
            if (lr.positionCount > 0)
                lr.positionCount = 0;
            return;
        }

        if (lr.positionCount != quality + 1)
        {
            spring.SetVelocity(velocity);
            lr.positionCount = quality + 1;
        }

        spring.SetDamper(damper);
        spring.SetStrength(strength);
        spring.Update(Time.deltaTime);

        Vector3 grapplePoint = grappleScript.getGrapplePoint();
        Vector3 shootPoint = grappleScript.getGrappleShootPoint();
        Vector3 up = Quaternion.LookRotation((grapplePoint - shootPoint).normalized) * Vector3.up;

        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, grapplePoint, Time.deltaTime * 12);

        for (int i = 0; i < quality + 1; i++)
        {
            float delta = i / (float)quality;
            Vector3 right = Quaternion.LookRotation((grapplePoint - shootPoint).normalized) * Vector3.right;

            Vector3 offset = up * waveHeight * Mathf.Sin(delta * waveCount * Mathf.PI) * spring.Value *
                                     affectCurve.Evaluate(delta) +
                                     right * waveHeight * Mathf.Cos(delta * waveCount * Mathf.PI) * spring.Value *
                                     affectCurve.Evaluate(delta);

            lr.SetPosition(i, Vector3.Lerp(shootPoint, currentGrapplePosition, delta) + offset);
        }
    }
}
