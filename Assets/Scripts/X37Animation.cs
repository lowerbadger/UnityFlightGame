using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class X37Animation : MonoBehaviour
{
    public Rigidbody rb;
    public Transform brake;
    public Transform leftAileron;
    public Transform rightAileron;
    public Transform leftCanard;
    public Transform rightCanard;
    public Transform leftFlap;
    public Transform rightFlap;

    Transform leftAileronT1;
    Transform leftAileronT2;
    Transform leftAileronB1;
    Transform leftAileronB2;

    Transform rightAileronT1;
    Transform rightAileronT2;
    Transform rightAileronB1;
    Transform rightAileronB2;

    Vector3 oldVelocity;
    Vector3 oldAngVel;
    Vector3 relAccel;
    Vector3 relAngAccel;

    float aileronRest = 2.59f;
    float flapRest = 1.461f;

    //float angAccelMargin = 0.2f;
    //float genMargin = 10f;
    float maxBrake = 50f;
    float maxAileron = 15f;
    float maxCanard = 5f;
    float maxSpoiler = 40f;
    // Start is called before the first frame update
    void Start()
    {
        oldVelocity = rb.velocity;
        oldAngVel = rb.angularVelocity;

        leftAileronT1 = leftAileron.transform.Find("LT_Aileron1");
        leftAileronT2 = leftAileron.transform.Find("LT_Aileron2");
        leftAileronB1 = leftAileron.transform.Find("LB_Aileron1");
        leftAileronB2 = leftAileron.transform.Find("LB_Aileron2");

        rightAileronT1 = rightAileron.transform.Find("RT_Aileron1");
        rightAileronT2 = rightAileron.transform.Find("RT_Aileron2");
        rightAileronB1 = rightAileron.transform.Find("RB_Aileron1");
        rightAileronB2 = rightAileron.transform.Find("RB_Aileron2");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        relAccel = rb.transform.InverseTransformDirection((rb.velocity - oldVelocity) / Time.fixedDeltaTime);
        relAngAccel = rb.transform.InverseTransformDirection((rb.angularVelocity - oldAngVel) / Time.fixedDeltaTime);

        oldVelocity = rb.velocity;
        oldAngVel = rb.angularVelocity;

        //Move air brakes
        float brakeX;
        if ((relAccel.z < -0.2f) || (rb.angularVelocity.magnitude > 4f))
        {
            brakeX = Mathf.Lerp(brake.localEulerAngles.x, maxBrake, 3f * Time.fixedDeltaTime);
        }
        else
        {
            brakeX = Mathf.Lerp(brake.localEulerAngles.x, 0, 3f * Time.fixedDeltaTime);
        }
        brake.localEulerAngles = new Vector3(brakeX, 0f, 0f);

        //Rolling Right
        float targetRoll = rb.transform.InverseTransformDirection(rb.angularVelocity).z*5f;
        float targetPitch = rb.transform.InverseTransformDirection(rb.angularVelocity).x*10f;
        float targetYaw = rb.transform.InverseTransformDirection(rb.angularVelocity).y * 100f;
        float targetBrake = Mathf.Clamp(relAccel.z * 30f, -maxSpoiler, 0f);

        float leftAileronX = Mathf.Clamp(targetRoll - targetPitch, -maxAileron, maxAileron);
        float rightAileronX = Mathf.Clamp(-targetRoll - targetPitch, -maxAileron, maxAileron);
        float canardX = Mathf.Clamp(targetPitch/3f, -maxCanard, maxCanard);
        float leftBrakes = Mathf.Clamp(targetYaw + targetBrake, -maxSpoiler, 0f);
        float rightBrakes = Mathf.Clamp(targetYaw - targetBrake, 0f, maxSpoiler);
        float flapX = Mathf.Clamp(-targetPitch, -maxAileron, maxAileron);
        /*
        if (Mathf.Abs(leftBrakes) < genMargin)
        {
            leftBrakes = 0f;
        }
        else
        {
            leftBrakes = (leftBrakes - genMargin) * 1.4f;
        }
        */
        leftAileron.localEulerAngles = new Vector3(leftAileronX, 0f, 0f);
        rightAileron.localEulerAngles = new Vector3(rightAileronX, 0f, 0f);
        leftCanard.localEulerAngles = new Vector3(canardX, 0f, 0f);
        rightCanard.localEulerAngles = new Vector3(canardX, 0f, 0f);
        leftFlap.localEulerAngles = new Vector3(flapRest + flapX, 0f, 0f);
        rightFlap.localEulerAngles = new Vector3(flapRest + flapX, 0f, 0f);

        leftAileronT1.localEulerAngles = new Vector3(aileronRest - leftBrakes, 0f, 0f);
        leftAileronT2.localEulerAngles = new Vector3(aileronRest - leftBrakes / 2f, 0f, 0f);
        leftAileronB1.localEulerAngles = new Vector3(aileronRest + leftBrakes, 0f, 180f);
        leftAileronB2.localEulerAngles = new Vector3(aileronRest + leftBrakes / 2f, 0f, 180f);

        rightAileronT1.localEulerAngles = new Vector3(aileronRest + rightBrakes, 0f, 0f);
        rightAileronT2.localEulerAngles = new Vector3(aileronRest + rightBrakes / 2f, 0f, 0f);
        rightAileronB1.localEulerAngles = new Vector3(aileronRest - rightBrakes, 0f, 0f);
        rightAileronB2.localEulerAngles = new Vector3(aileronRest - rightBrakes / 2f, 0f, 0f);

        //print(targetBrake);
    }
}
