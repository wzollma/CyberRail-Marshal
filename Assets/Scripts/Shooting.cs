using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerRbInput))]
public class Shooting : MonoBehaviour
{
    [SerializeField] LayerMask shootLayers;
    [SerializeField] Gun curGun;
    [SerializeField] float maxDist = 20;

    // component cache
    PlayerRbInput input;
    Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        input = GetComponent<PlayerRbInput>();
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        Cursor.visible = true;

        if (input.shootDown)
            Shoot(maxDist, shootLayers);

        if (input.scopeDown)
            curGun.setIsScoping(true);
        else if (input.scopeUp)
            curGun.setIsScoping(false);
    }

    public void Shoot(float maxDist, LayerMask shootLayers)
    {
        //Debug.Log("shoot");
        curGun.ShootGFX();
        RaycastHit hit;
        if (Physics.Raycast(shootPos(), shootDirec(), out hit, maxDist, shootLayers))
        {
            //Debug.Log(hit.transform.gameObject.name);
            curGun.hitGFX(hit.point);
        }
    }

    Vector3 shootPos()
    {
        return cam.transform.position;
    }

    Vector3 shootDirec()
    {
        return cam.ScreenPointToRay(Input.mousePosition).direction;
    }

    private void OnDrawGizmos()
    {
        if (cam == null)
            cam = Camera.main;

        Gizmos.DrawWireSphere(shootPos(), maxDist);
    }
}
