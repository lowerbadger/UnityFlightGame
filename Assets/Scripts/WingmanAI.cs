using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WingmanAI : MonoBehaviour
{
    private float thrust = 350.0f;
    private float throttle = 0.7f;
    public float distGoal = 16f;
    public Vector3 wingOff;

    //public float turnRate = 80.0f;
    public Rigidbody rb;
    public Rigidbody player;
    public Rigidbody target;
    public int mode = 1;
    //public GameObject co;
    //public GameObject goCanvas;
    public float pitchRate = 12.0f;
    public float rollRate = 12.0f;
    public float yawRate = 5.0f;
    public float maxThrust = 750f;
    public float minThrust = 100f;
    public float agility = 0.9f;        //Should be between 0.0-1.0
    public float stallSpeed = 4.5f;
    public float stallStrength = 5.0f;
    public float psmStrength = 5.0f;
    public AudioSource jetEngine;
    public AudioSource sonicBoom;
    public GameObject missilePrefab;
    public Transform[] missilePos;
    public GameObject vapor;
    public GameObject shockCone;
    public TrailRenderer[] wingTrail;
    ParticleSystem[] vaporFX;
    public GameObject jetExh;
    private ParticleSystem mainPlume;
    private ParticleSystem shockDiamonds;
    Transform respawnPosition;
    Transform playerTrans;

    private float pitchAngle;
    private float rollAngle;
    private float yawAngle;
    private float gravity = 9.8f;
    private float speed;
    private float speedTarget;
    private float drag;
    private float angularDrag;

    private RaycastHit hit;
    private Vector3 targetPos;
    private float collSpread = 0.5f;
    private float collisionDist = 40f;

    private float lockSpeed = 0.5f;
    private float lockAngle = 45f;
    private float lockTimer = 0;
    private float missileRate = 3.5f;
    private float missileTimer = 0;
    private int missileNext = 0;
    private Vector3 oldVelocity;
    float vaporStrength = 0;

    // Start is called before the first frame update
    void Start()
    {
        respawnPosition = rb.transform;
        playerTrans = player.transform;
        rb.velocity = transform.forward * (stallSpeed + 1);
        drag = rb.drag;
        angularDrag = rb.angularDrag;

        vaporFX = new ParticleSystem[vapor.GetComponent<Transform>().childCount];

        for (int i = 0; i < vapor.GetComponent<Transform>().childCount; i++)
        {
            vaporFX[i] = vapor.GetComponent<Transform>().GetChild(i).GetComponent<ParticleSystem>();
        }

        mainPlume = jetExh.GetComponent<ParticleSystem>();
        shockDiamonds = jetExh.transform.GetChild(0).GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        rollAngle = rb.transform.rotation.eulerAngles.z;
        /*
        Vector3 direction = target.position - transform.position;
        Quaternion rotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, yawRate * Time.deltaTime);
        */
        float vaporMargin = 2;
        float vaporMax = 14;
        //print(vaporStrength);

        if (vaporStrength > vaporMax)
        {
            vaporStrength = vaporMax;
        }

        if (vaporStrength > vaporMargin)
        {
            vapor.SetActive(true);
            foreach (ParticleSystem vape in vaporFX)
            {
                ParticleSystem.MainModule settings = vape.main;
                settings.startColor = new Color(1, 1, 1, 0.1f * (vaporStrength - vaporMargin) / (vaporMax - vaporMargin));
            }

            foreach (TrailRenderer trail in wingTrail)
            {
                trail.emitting = true;
            }
        }
        else
        {
            vapor.SetActive(false);
            foreach (TrailRenderer trail in wingTrail)
            {
                trail.emitting = false;
            }
        }

        //Display shock cone if close to speed of sound
        if (Mathf.Abs(speed - 12.4f) < 0.5f)
        {
            if (!shockCone.activeSelf)
            {
                sonicBoom.Play();
            }
            shockCone.SetActive(true);
        }
        else
        {
            shockCone.SetActive(false);
        }

        float plumeStrength = thrust / maxThrust * 0.4f;

        ChangeStartColor(mainPlume, new Color(plumeStrength, plumeStrength, plumeStrength, 0));
        ChangeStartColor(shockDiamonds, new Color(plumeStrength, plumeStrength, plumeStrength, 0));
    }

    void FixedUpdate()
    {
        Vector3 relAccel = rb.transform.InverseTransformDirection((rb.velocity - oldVelocity) / Time.fixedDeltaTime);
        oldVelocity = rb.velocity;
        vaporStrength = relAccel.magnitude + Mathf.Abs(rb.transform.InverseTransformDirection(rb.angularVelocity).z) * 1f;

        collisionDist = thrust * 0.1f;
        if (mode == 1)
        {
            targetPos = (playerTrans.position + playerTrans.InverseTransformDirection(wingOff));
            speedTarget = player.velocity.magnitude;
            Vector3 rubberband = targetPos - rb.transform.position;
            Vector3 dampener = player.velocity - rb.velocity;
            rubberband = Vector3.ClampMagnitude(rubberband + dampener*5f, 30f);
            rb.AddForce(rubberband);

            
        }
        else if (mode == 2 && target != null)
        {
            targetPos = target.transform.position;
            speedTarget = target.velocity.magnitude;
            WSO(targetPos);
        }
        else if (mode == 2 && target == null)
        {
            //targetPos = (playerTrans.position + rb.transform.InverseTransformDirection(wingOff));
            //speedTarget = player.velocity.magnitude;
            mode = 1;
        }

        AvoidCollision(Vector3.zero, 1.5f);
        AvoidCollision(rb.transform.right * collSpread, 1.5f);
        AvoidCollision(-rb.transform.right * collSpread, 1.5f);
        AvoidCollision(rb.transform.up * collSpread, 1.5f);
        AvoidCollision(-rb.transform.up * collSpread, 3f);

        speed = rb.velocity.magnitude;
        

        Vector3 direction = targetPos - rb.transform.position;
        Vector3 relDir = rb.transform.InverseTransformDirection(direction);
        //Quaternion rotation = Quaternion.LookRotation(direction);
        Quaternion quat1 = rb.transform.rotation;
        Quaternion quat2 = playerTrans.transform.rotation;
        Quaternion quat3;

        if (relDir.z > 0)
        {
            quat3 = Quaternion.LookRotation(direction, playerTrans.transform.up);
        }
        else
        {
            quat3 = Quaternion.LookRotation(direction, -playerTrans.transform.up);
        }

        Quaternion quat4 = quat3*Quaternion.Inverse(quat1);
        Quaternion quat5 = quat2 * Quaternion.Inverse(quat1);
        //quat4 = quat5 * Quaternion.Inverse(quat4);
        //quat4 *= Quaternion.AngleAxis(rollAngle, rb.transform.forward);
        speed = rb.velocity.magnitude;

        rb.AddForce((Vector3.up * agility) * rb.mass * gravity);
        rb.AddForce((transform.up * (1 - agility)) * rb.mass * gravity);
        rb.AddForce(transform.forward * thrust);
        //Vector3 turn = rotation.eulerAngles;
        //turn.x = turn.x - 180f;
        //turn.y = turn.y - 180f;
        if (speedTarget < 1f)
        {
            quat5 = Quaternion.identity;
        }

        float turnX = quat4.x * (quat5.x + 1.5f) * pitchRate ;// + quat4.y * yawRate * Mathf.Sin(rollAngle);
        float turnY = quat4.y * (quat5.y + 1.5f) * yawRate;// + quat4.x * pitchRate * Mathf.Sin(rollAngle);
        float turnZ = quat4.z * (quat5.z + 1.5f) * rollRate;

        //Vector3 rot = new Vector3(turnX, turnY, quat4.z*rollRate);
        //Vector3 rot = new Vector3(quat5.x * pitchRate, quat5.y * yawRate, quat5.z * rollRate);
        //Vector3 rot = new Vector3(quat4.x * pitchRate, quat4.y * yawRate, quat4.z * rollRate);
        Vector3 rot = new Vector3(turnX, turnY, turnZ);
        rot = rot * 1.5f *quat4.w;
        rb.AddTorque(rot);

        float distance = (targetPos - rb.transform.position).magnitude;
        //print(speed);

        if ((distance > distGoal)&&(speed < speedTarget))
        {
            thrust += (distance - distGoal)*Mathf.Abs(speed-speedTarget) * throttle;
        }
        else if ((speed > speedTarget)&&(distance < distGoal))
        {
            thrust -= (distGoal - distance)*Mathf.Abs(speed-speedTarget) * throttle;
        }

        if (speedTarget < 1f && distance > 20f)
        {
            thrust = Mathf.Lerp(thrust, maxThrust * 0.8f, throttle);
        }
        else if (speedTarget < 1f && distance < 40f)
        {
            thrust = Mathf.Lerp(thrust, maxThrust * 0.5f, throttle);
        }
        //thrust += (distance - distGoal) * throttle;
        thrust = Mathf.Clamp(thrust, minThrust, maxThrust);

        //print(thrust);
    }

    void AvoidCollision(Vector3 dirOff, float bias)
    {
        if (Physics.Raycast(rb.transform.position, rb.transform.forward + dirOff, out hit, collisionDist))
        {
            if ((hit.transform != transform) || (hit.transform.CompareTag("Player") == false))
            {
                Debug.DrawLine(transform.position, hit.point, Color.white);

                if (dirOff == Vector3.zero)
                {
                    targetPos += (hit.normal) * collisionDist * bias;
                }
                else
                {
                    targetPos += (-dirOff * 2f) * collisionDist * bias;
                }

            }
        }
    }

    //weapon systems operator
    void WSO(Vector3 targetPos)
    {
        Vector3 targetRelPos = rb.transform.InverseTransformVector(targetPos - rb.transform.position);
        Vector2 targetProj = new Vector2(targetRelPos.x, targetRelPos.y);
        float targetAngle = Mathf.Atan2(targetProj.magnitude, targetRelPos.z) * Mathf.Rad2Deg;
        //print(targetRelPos);
        //print(targetAngle);
        //print((targetAngle < lockAngle));
        if ((targetAngle < lockAngle) && (targetRelPos.magnitude < 200f))
        {
            if (Time.time > lockTimer + lockSpeed)
            {
                //print("enemy has locked!");
                if (Time.time > missileTimer + missileRate)
                {
                    //print("enemy has fired!");
                    FireMissile();
                    missileTimer = Time.time;
                }
            }
        }
        else
        {
            missileTimer = Time.time;
            lockTimer = Time.time;
        }
    }

    void FireMissile()
    {
        GameObject missile = Instantiate(missilePrefab, missilePos[missileNext].position, transform.rotation);
        if (missileNext != missilePos.Length - 1)
        {
            missileNext++;
        }
        else
        {
            missileNext = 0;
        }
        missile.transform.SetParent(GameObject.Find("/Debris").transform);
        missile.GetComponent<Rigidbody>().velocity = rb.velocity * 1.4f - rb.transform.up * 8f;

        //enemies can't dumbfire missiles
        missile.GetComponent<MissileTrack>().target = target;
        //missile.GetComponent<MissileTrack>().friendly = true;
    }

    void ChangeStartColor(ParticleSystem parSys, Color newColor)
    {
        ParticleSystem.MainModule settings = parSys.main;
        settings.startColor = newColor;
    }
}
