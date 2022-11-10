using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follow : MonoBehaviour
{
    [SerializeField] Transform followTrans;

    void Update()
    {
        transform.position = followTrans.position;
    }
}
