using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRbInput : MonoBehaviour
{
    public bool isJump { get { return Input.GetKey(jumpKey); } }
    public bool isSprint { get { return Input.GetKey(sprintKey); } }
    public bool isCrouch { get { return Input.GetKey(crouchKey); } }
    public bool isSlide { get { return Input.GetKey(slideKey); } }

    public bool jumpDown { get { return Input.GetKeyDown(jumpKey); } }
    public bool crouchDown { get { return Input.GetKeyDown(crouchKey); } }
    public bool crouchUp { get { return Input.GetKeyUp(crouchKey); } }
    public bool slideDown { get { return Input.GetKeyDown(slideKey); } }
    public bool slideUp { get { return Input.GetKeyUp(slideKey); } }

    // shooting
    public bool shootDown { get { return Input.GetKeyDown(shootKey); } }
    public bool shootUp { get { return Input.GetKeyUp(shootKey); } }
    public bool scopeDown { get { return Input.GetKeyDown(scopeKey); } }
    public bool scopeUp { get { return Input.GetKeyUp(scopeKey); } }

    // grappling
    public bool grappleDown { get { return Input.GetKeyDown(grappleKey); } }

    public bool isMovement { get { return moveInput.magnitude != 0;  } }
    public Vector2 moveInput { get { return curMoveInput; } }

    [Header("References")]
    public Transform orientation;

    [Header("Keybinds")]
    [SerializeField] KeyCode jumpKey = KeyCode.Space;
    [SerializeField] KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] KeyCode crouchKey = KeyCode.C;
    [SerializeField] KeyCode slideKey = KeyCode.LeftControl;
    [SerializeField] KeyCode shootKey = KeyCode.Mouse0;
    [SerializeField] KeyCode scopeKey = KeyCode.Mouse1;
    [SerializeField] KeyCode grappleKey = KeyCode.Q;

    Vector2 curMoveInput;
    
    void Update()
    {
        curMoveInput.x = Input.GetAxisRaw("Horizontal");
        curMoveInput.y = Input.GetAxisRaw("Vertical");
    }

    public Vector3 getInputDirection()
    {
        // calc movement direction
        return orientation.forward * curMoveInput.y + orientation.right * curMoveInput.x;
    }
}
