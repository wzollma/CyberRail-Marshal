using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    [SerializeField] Transform orientation;
    [SerializeField] Transform followTrans;
    [SerializeField] Vector2 sensitivity = new Vector2(500, 500);    

    Vector2 curRot;
    
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    void Update()
    {
        // move cam to followTrans
        transform.position = followTrans.position;

        // get mouse input
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensitivity.x;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensitivity.y;

        curRot.y += mouseX;

        curRot.x -= mouseY;
        curRot.x = Mathf.Clamp(curRot.x, -90f, 90f);

        // rotate cam and orientation
        transform.rotation = Quaternion.Euler(curRot.x, curRot.y, 0);
        orientation.rotation = Quaternion.Euler(0, curRot.y, 0);
    }
}
