using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLook : MonoBehaviour
{
    public Rigidbody rb;
    private Rigidbody target;
    public GameObject goCanvas;
    private float sensitivity = 5.0f;
    private float limitMargin = 10.0f;
    private bool limitDown;
    //bool limitDown2;
    private bool limitUp;
    private bool lookingUp;
    private float rotSpeed = 12f;
    private int mode;
    //Object CameraOrigin;
    // Start is called before the first frame update

    void Start()
    {
        mode = 1;
        //Camera main;    
    }

    void Update()
    {
        //Change camera modes
        if (Input.GetKeyDown(KeyCode.F))
        {
            if ((mode != 2) && (target != null))
            {
                mode++;
            }
            else
            {
                mode = 1;
            }
        }

        //Mode 1 just points the camera straight
        if (mode == 1)
        {
            //Free look option in mode 1
            if (Input.GetKey(KeyCode.X))
            {
                float x = sensitivity * Input.GetAxis("Mouse X");
                float y = sensitivity * -Input.GetAxis("Mouse Y");

                //(limitDown || limitUp)
                limitDown = (transform.localEulerAngles.x < (90.0f - limitMargin));    //within lower limit
                limitUp = (transform.localEulerAngles.x > (270.0f + limitMargin));     //within upper limit
                lookingUp = (Input.GetAxis("Mouse Y") > 0.0f);        //looking up
                                                                      //print(transform.localEulerAngles.x);
                                                                      //print(Input.GetAxis("Mouse Y"));
                if (limitDown || limitUp)
                {
                    transform.Rotate(y, x, 0.0f);
                    //print(limitDown);
                }
                else
                {
                    limitDown = (transform.localEulerAngles.x < 90.0f);
                    //print(limitDown);
                    if (limitDown && lookingUp)
                    {
                        transform.Rotate(y, x, 0.0f);
                    }
                    else if (!limitDown && !lookingUp)
                    {
                        transform.Rotate(y, x, 0.0f);
                    }
                    else
                    {
                        transform.Rotate(0.0f, x, 0.0f);
                        //print("camera lock");
                    }

                }
                //Vector3 asdf = new Vector3(1.0f, 1.0f, 0.0f);

                cameraStraight();
            }
            else
            {
                if (transform.localEulerAngles != Vector3.zero)
                {
                    Vector3 direction = rb.transform.forward;
                    //Quaternion rotation = Quaternion.LookRotation(direction);
                    transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.identity, rotSpeed * Time.deltaTime);
                    //transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 5f * Time.deltaTime);
                    //transform.localEulerAngles = Vector3.Slerp(transform.localEulerAngles, Vector3.zero, 0.5f * Time.deltaTime);
                }
                else
                {
                    transform.localEulerAngles = new Vector3(0, 0, 0);
                }
            }

        }
    }

    //FixedUpdate fixes jittery camera for some reason
    void FixedUpdate()
    {
        target = rb.GetComponent<PlaneDriver>().target;
        if (target == null)
        {
            mode = 1;
        }

        
        //Mode 2 points camera towards target if target exists
        if (mode == 2)
        {
            //Vector3 direction = target.position - transform.position;

            
            Vector3 direction = (target.transform.position - transform.position);
            Quaternion lookRot = Quaternion.LookRotation(direction, rb.transform.up);

            //print(Quaternion.Angle(transform.rotation, lookRot));
            //print(Quaternion.Angle(transform.rotation, lookRot) * Time.deltaTime * rotSpeed);
            //if (Time.deltaTime * rotSpeed < 0.15f)
            transform.rotation = Quaternion.Lerp(transform.rotation, lookRot,
                    rotSpeed * Time.fixedDeltaTime);
            /*
            if (slerp)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRot,
                    (Quaternion.Angle(transform.rotation, lookRot)/ rotSpeed + rotSpeed) * Time.deltaTime);
            }
            else
            {
                transform.rotation = lookRot;
            }
            */
            //transform.localEulerAngles = Vector3.Slerp(transform.localEulerAngles, eulDir, 5f * Time.deltaTime);
        }
    }

    void cameraStraight()
    {
        transform.localEulerAngles = new Vector3(
                    transform.localEulerAngles.x,
                    transform.localEulerAngles.y,
                    0.0f);
    }

}
