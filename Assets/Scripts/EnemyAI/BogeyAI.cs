using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BogeyAI : MonoBehaviour
{
    public delegate void radar(int spike);
    public static event radar radarSpike;    //If enemy locks on

    private float thrust = 350.0f;
    private float throttle = 0.7f;
    public float distGoal = 12f;
    //public float wingOff = 2f;
    public Vector3 wingOff;
    //mode 1: patrol
    //mode 2: follow lead
    //mode 3: attack
    public int mode = 1;

    //public float turnRate = 80.0f;
    public Rigidbody rb;
    public Rigidbody player;
    public Rigidbody leader;
    public LineRenderer patrolRoute;
    public GameObject missilePrefab;
    public Transform[] missilePos;
    public GameObject vapor;
    public GameObject shockCone;
    public TrailRenderer[] wingTrail;
    ParticleSystem[] vaporFX;
    public GameObject jetExh;
    private ParticleSystem mainPlume;
    private ParticleSystem shockDiamonds;
    public AudioSource jetEngine;
    //public GameObject co;
    //public GameObject goCanvas;
    public float pitchRate = 12.0f;
    public float rollRate = 12.0f;
    public float yawRate = 5.0f;
    public float maxThrust = 500f;
    public float minThrust = 200f;
    public float agility = 0.9f;        //Should be between 0.0-1.0
    public float stallSpeed = 4.5f;
    public float stallStrength = 5.0f;
    public float psmStrength = 5.0f;
    Transform respawnPosition;
    Rigidbody target;

    private float pitchAngle;
    private float rollAngle;
    private float yawAngle;
    private float gravity = 9.8f;
    private float speed;
    private float speedTarget;
    private float drag;
    private float angularDrag;
    private float aggroDist = 220f;

    private RaycastHit hit;
    private Vector3 targetPos;
    private float collSpread = 0.7f;
    private float collisionDist = 40f;

    private int waypoint = 0;
    private float lockSpeed = 2f;
    private float lockAngle = 45f;
    private float lockTimer = 0;
    private float missileRate = 5f;
    private float missileTimer = 0;
    private int missileNext = 0;
    private float missileDist = 200f;
    private bool locked = false;
    //private bool isAlive = true;
    private Vector3 oldVelocity;
    float vaporStrength = 0;
    private float rotStrength = 0f;
    private GameObject[] ally;
    //private GameObject[] player;

    private void OnEnable()
    {
        EnemyHealth.OnDeath += CheckDeath;
    }

    private void OnDisable()
    {
        EnemyHealth.OnDeath -= CheckDeath;
    }

    // Start is called before the first frame update
    void Start()
    {
        respawnPosition = rb.transform;
        ally = GameObject.FindGameObjectsWithTag("Ally");
        //target = player.GetComponent<Rigidbody>();  //change this later
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

        if (leader != null)
        {
            mode = 2;
        }
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
        //Control plane vapor effect
        float vaporMargin = 2;
        float vaporMax = 10;
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

        float plumeStrength = thrust / maxThrust * 0.3f;

        ChangeStartColor(mainPlume, new Color(plumeStrength, plumeStrength, plumeStrength, 0));
        ChangeStartColor(shockDiamonds, new Color(plumeStrength, plumeStrength, plumeStrength, 0));

        jetEngine.pitch = 1f + (thrust - 100f) * .001f;
    }

    void FixedUpdate()
    {
        Vector3 relAccel = rb.transform.InverseTransformDirection((rb.velocity - oldVelocity) / Time.fixedDeltaTime);
        oldVelocity = rb.velocity;
        vaporStrength = relAccel.magnitude + Mathf.Abs(rb.transform.InverseTransformDirection(rb.angularVelocity).z) * 1f;

        //float health = this.GetComponent<EnemyHealth>().health;
        if (this.GetComponent<EnemyHealth>().health > 0)
        {
            

            collisionDist = thrust * 0.25f;

            if (mode == 1)
            {
                targetPos = patrolRoute.GetPosition(waypoint);
                speedTarget = 0f;
                if (Vector3.Distance(targetPos, rb.transform.position) < 12f)
                {
                    waypoint++;
                    if (waypoint > patrolRoute.positionCount - 1)
                    {
                        waypoint = 0;
                    }
                }
                //print(waypoint);
                Aggro();
            }
            else if (mode == 2)
            {
                if (leader != null)
                {
                    target = leader;
                    targetPos = leader.transform.position + leader.transform.InverseTransformVector(wingOff);
                    speedTarget = target.velocity.magnitude;

                    Aggro();
                }
                else
                {
                    mode = 1;
                }
            }
            else if (mode == 3)
            {
                if (target == null && leader == null)
                {
                    mode = 1;
                }
                else if (target == null && leader != null)
                {
                    mode = 2;
                }
                else
                {
                    if (Vector3.Distance(this.transform.position, target.position) > aggroDist + 20f)
                    {
                        if (leader != null)
                        {
                            mode = 2;
                        }
                        else
                        {
                            mode = 1;
                        }
                    }
                    targetPos = (target.position - target.transform.forward * 2f);
                    speedTarget = player.velocity.magnitude;
                }
            }
            
            speed = rb.velocity.magnitude;
            

            AvoidCollision(Vector3.zero, 2.6f);
            AvoidCollision(rb.transform.right * collSpread, 2.6f);
            AvoidCollision(-rb.transform.right * collSpread, 2.6f);
            AvoidCollision(rb.transform.up * collSpread, 2.6f);
            AvoidCollision(-rb.transform.up * collSpread, 4.2f);

            

            Vector3 direction = targetPos - rb.transform.position;
            Vector3 relDir = rb.transform.InverseTransformDirection(direction);

            if (mode > 2)
            {
                WSO(relDir);
            }
            
            //Quaternion rotation = Quaternion.LookRotation(direction);
            Quaternion quat1 = rb.transform.rotation;
            Quaternion quat2;
            
            Quaternion quat3;
            if (mode == 1)
            {
                quat2 = Quaternion.identity;
                quat3 = Quaternion.LookRotation(direction);
            }
            else
            {
                quat2 = target.transform.rotation;
                if (relDir.z > 0)
                {
                    quat3 = Quaternion.LookRotation(direction, target.transform.up);
                }
                else
                {
                    quat3 = Quaternion.LookRotation(direction, -target.transform.up);
                }
            }
            
            //Quaternion quat3 = Quaternion.LookRotation(direction, target.transform.up);
            Quaternion quat4 = quat3 * Quaternion.Inverse(quat1);
            Quaternion quat5 = quat2 * Quaternion.Inverse(quat1);

            speed = rb.velocity.magnitude;

            rb.AddForce((Vector3.up * agility) * rb.mass * gravity);
            rb.AddForce((transform.up * (1 - agility)) * rb.mass * gravity);
            rb.AddForce(transform.forward * thrust);
            //Vector3 turn = rotation.eulerAngles;
            //turn.x = turn.x - 180f;
            //turn.y = turn.y - 180f;
            float turnX = quat4.x * (0.0f * Mathf.Abs(quat5.x) + 1.2f) * pitchRate;// + quat4.y * yawRate * Mathf.Sin(rollAngle);
            float turnY = quat4.y * (0.0f * Mathf.Abs(quat5.y) + 1.2f) * yawRate;// + quat4.x * pitchRate * Mathf.Sin(rollAngle);
            float turnZ = quat4.z * (0.0f * Mathf.Abs(quat5.z) + 1.2f) * rollRate;

            Vector3 rot = new Vector3(turnX, turnY, turnZ);
            rot = rot * 1.5f * quat4.w;
            rb.AddTorque(rot);

            float distance = (targetPos - rb.transform.position).magnitude;
            //print(collisionDist);

            if (mode != 1)
            {
                if ((distance > distGoal) && (speed < speedTarget))
                {
                    thrust += (distance - distGoal) * Mathf.Abs(speed - speedTarget) * throttle;
                }
                else if ((speed > speedTarget) && (distance < distGoal))
                {
                    thrust -= (distGoal - distance) * Mathf.Abs(speed - speedTarget) * throttle;
                }
            }
            else
            {
                thrust = 200f;
            }
            

            //thrust += (distance - distGoal) * throttle;
            thrust = Mathf.Clamp(thrust, minThrust, maxThrust);
        }
        else
        {
            if ((radarSpike != null) && locked && target == player.GetComponent<Rigidbody>())
            {
                radarSpike(-1);
                locked = false;
            }

            Stall();
        }
        
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
                    targetPos += (-dirOff*2f) * collisionDist * bias;
                }
                
            }
        }
    }

    void Stall()
    {
        float stallRatio = (speed / stallSpeed) / 5.0f;
        
        Vector3 tug = Vector3.right * stallStrength * rotStrength/20f;
        //rb.drag = 1f;
        //weak lift
        rb.AddForce((Vector3.up * agility) * stallRatio * rb.mass * 9.8f);
        rb.AddForce((transform.up * (1 - agility)) * stallRatio * rb.mass * 9.8f);

        //pitch down
        rb.AddForceAtPosition(Vector3.forward, tug);
        rb.AddForceAtPosition(-Vector3.forward, -tug);

        rb.AddTorque(rb.transform.forward * rotStrength);
        //rb.AddTorque(rb.transform.forward * 30f);

        //thrust = thrust + stallStrength / 15.0f;
        rb.AddForce(transform.forward * thrust / stallStrength * 2f);

        jetEngine.pitch = Mathf.Lerp(jetEngine.pitch, 0f, 0.3f * Time.fixedDeltaTime);
    }

    //weapon systems operator
    void WSO(Vector3 targetRelPos)
    {
        //Vector3 targetRelPos = rb.transform.InverseTransformVector(target.transform.position - rb.transform.position);
        Vector2 targetProj = new Vector2(targetRelPos.x, targetRelPos.y);
        float targetAngle = Mathf.Atan2(targetProj.magnitude, targetRelPos.z)*Mathf.Rad2Deg;
        //print(targetRelPos);
        //print(targetAngle);
        //print((targetAngle < lockAngle));
        if ((targetAngle < lockAngle) && (targetRelPos.magnitude < missileDist))
        {
            if (Time.time > lockTimer + lockSpeed)
            {
                if ((radarSpike != null) && !locked && target == player.GetComponent<Rigidbody>())
                {
                    radarSpike(1);
                    locked = true;
                }
                
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
            if ((radarSpike != null) && locked && target == player.GetComponent<Rigidbody>())
            {
                radarSpike(-1);
                locked = false;
            }
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
        missile.GetComponent<MissileTrack>().friendly = false;
    }

    void TrailDeath()
    {
        foreach (TrailRenderer trail in wingTrail)
        {
            trail.autodestruct = true;
            trail.transform.SetParent(GameObject.Find("/Debris").transform, true);
        }
    }

    void CheckDeath(GameObject check)
    {
        if (gameObject == check)
        {
            rotStrength = ((float)Random.Range(-1, 1)+.5f) * 12f * stallStrength;
            //print(rotStrength);
            Invoke("TrailDeath", 3f);
        }
    }

    void ChangeStartColor(ParticleSystem parSys, Color newColor)
    {
        ParticleSystem.MainModule settings = parSys.main;
        settings.startColor = newColor;
    }

    //Aggro ally or player
    void Aggro()
    {
        foreach (GameObject curAlly in ally)
        {
            if (Vector3.Distance(curAlly.transform.position, rb.transform.position) < aggroDist)
            {
                target = curAlly.GetComponent<Rigidbody>();
                mode = 3;
            }
        }

        if (Vector3.Distance(player.transform.position, rb.transform.position) < aggroDist)
        {
            target = player.GetComponent<Rigidbody>();
            mode = 3;
        }
    }
}
