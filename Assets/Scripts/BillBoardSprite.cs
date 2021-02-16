using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillBoardSprite : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        //transform.LookAt(Camera.main.transform.position, -Camera.main.transform.up);
        transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward,
            Camera.main.transform.rotation * Vector3.up);
    }
}
