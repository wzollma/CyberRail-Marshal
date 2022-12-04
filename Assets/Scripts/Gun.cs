using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] Transform shootPos;
    [SerializeField] GameObject bulletHole;
    [SerializeField] Vector3 scopeLocalPos;
    [SerializeField] float scopeMoveSpeed = 3;
    [SerializeField] float scopeFOV = 40;

    float startFOV;
    Vector3 hipLocalPos;
    bool isScoping;
    PlayerCam playerCam;

    private void Start()
    {
        playerCam = Camera.main.GetComponent<PlayerCam>();

        if (shootPos == null)
            Debug.LogError($"{name}'s shootPos is NULL");

        hipLocalPos = transform.localPosition;
        startFOV = playerCam.GetComponent<Camera>().fieldOfView;
    }

    private void Update()
    {
        Vector3 targetPos = isScoping ? scopeLocalPos : hipLocalPos;

        const float EPSILON = .01f;
        if (Vector2.Distance(targetPos, transform.position) > EPSILON)
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPos, scopeMoveSpeed * Time.deltaTime);
    }

    public void ShootGFX()
    {

    }
    public void hitGFX(Vector3 hitPoint)
    {
        Instantiate(bulletHole, hitPoint, Quaternion.identity);
    }

    public void setIsScoping(bool isScoping)
    {
        this.isScoping = isScoping;

        float targetFOV = isScoping ? scopeFOV : startFOV;
        playerCam.setScopeFOVModifier(targetFOV / startFOV);
        playerCam.DoFov(targetFOV);
    }

    //public Vector3 getShootPos()
    //{
    //    return shootPos.position;
    //}
}
