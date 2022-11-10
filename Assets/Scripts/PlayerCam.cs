using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerCam : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform orientation;
    [SerializeField] Transform camHolder;

    [Header("Game Feel")]
    [SerializeField] Vector2 sensitivity = new Vector2(500, 500);
    [SerializeField] float wallRunFovShift = 15;
    [SerializeField] float wallRunTilt = 10;

    Vector2 curRot;
    float startFov;
    float lastSetFovInc;
    
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        startFov = GetComponent<Camera>().fieldOfView;
        lastSetFovInc = 0;
    }
    
    void Update()
    {
        // get mouse input
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensitivity.x;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensitivity.y;

        curRot.y += mouseX;

        curRot.x -= mouseY;
        curRot.x = Mathf.Clamp(curRot.x, -90f, 90f);

        // rotate cam and orientation
        camHolder.rotation = Quaternion.Euler(curRot.x, curRot.y, 0);
        orientation.rotation = Quaternion.Euler(0, curRot.y, 0);
    }

    public void toggleWallRunEffects(bool on, bool wallLeft, bool wallRight)
    {
        float newFovInc = 0;
        float newTilt;

        if (on)
        {
            newFovInc += wallRunFovShift;
            newTilt = wallRunTilt * (wallLeft ? -1 : (wallRight ? 1 : 0));
        }
        else
        {            
            newTilt = 0;
        }

        DoIncFov(newFovInc);
        DoTilt(newTilt);
    }

    public void DoIncFov(float incVal)
    {
        DoFov(startFov + incVal);
    }

    public float getLastSetFovInc()
    {
        return lastSetFovInc;
    }

    void DoFov(float endValue)
    {
        GetComponent<Camera>().DOFieldOfView(endValue, 0.25f);
        lastSetFovInc = endValue - startFov;
    }

    void DoTilt(float zTilt)
    {
        transform.DOLocalRotate(new Vector3(0, 0, zTilt), 0.25f);
    }    
}
