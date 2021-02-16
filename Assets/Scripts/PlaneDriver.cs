//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlaneDriver : MonoBehaviour
{

    public delegate void targetSwitch(Rigidbody mainTarget);
    public static event targetSwitch targSwitch;
    public static event targetSwitch targNext;
    public delegate void modeSwitch(int mode);
    public static event modeSwitch spSwitch;
    public delegate void deployWep(GameObject bomb);
    public static event deployWep dropBomb;

    public float thrust = 450.0f;
    public float throttle = 0.7f;
    public float fireRate = 100f;
    public int cannonMax = 250;
    public float missileCool = 2f;
    public float bombCool = 2f;
    public float flareCool = 5f;
    private float flareDuration = 2.5f;

    //public float turnRate = 80.0f;
    public Rigidbody rb;
    public GameObject co;
    public GameObject goCanvas;
    public GameObject bulletPrefab;
    public GameObject missilePrefab;
    public GameObject spPrefab;
    public GameObject cannonFlash;
    public GameObject flares;
    public GameObject vapor;
    public GameObject shockCone;
    public TrailRenderer[] wingTrail;

    public AudioSource jetEngine;
    public AudioSource sonicBoom;
    public AudioSource cannonSFX;
    public AudioSource windSFX;
    public AudioSource flareSFX;
    public AudioSource blip1;
    public AudioSource blip2;
    public Transform cannonDoor;
    public Transform[] missilePos;
    public Transform[] spPos;
    public Transform cannonPos;
    public GameObject jetExh;
    public GameObject[] wingman;
    public Rigidbody target;
    public bool locked = false;
    public bool ecm = false;            //Electronic Counter Measures
    GameObject[] ally;
    //GameObject[] enemy;
    List<GameObject> enemy = new List<GameObject>();
    ParticleSystem[] vaporFX;
    //GameObject[] enemyMissile;


    //public GameObject goCanvas;
    public float pitchRate = 30.0f;
    public float rollRate = 20.0f;
    public float yawRate = 5.0f;
    public float maxThrust = 700f;
    public float minThrust = 0.0f;
    public float agility = 0.9f;        //Should be between 0.0-1.0
    public float stallSpeed = 4.5f;
    public float stallStrength = 5.0f;  //How quick this plane falls out of the sky
    public float psmStrength = 5.0f;    //Strength of post-stall maneuvers
    public float lockSpeed = 0.5f;      //number of seconds to lock-on
    public float lockAngle = 45f;       //Angle that radar will start to lock-on
    public Transform respawnPosition;

    private Vector3 oldVelocity;
    //private Vector3 oldVelocityWing;
    private float pitchAngle;
    private float rollAngle;
    private float yawAngle;
    private float gravity = 9.8f;
    private float speed = 0f;
    private float drag;
    private float angularDrag;
    private float nextFire = 0f;
    public int cannonCurr;          //dont change this to private
    private int missileNext = 0;
    private int spNext = 0;
    public float[] missileTime;     //dont change this either
    public float[] spTime;
    public float flareTime;
    private float vaporStrength = 0;
    private ParticleSystem mainPlume;
    private ParticleSystem shockDiamonds;
    private float spread = 0.3f;
    private Rigidbody nextTarget;
    private float rotStrength = 0f;
    // Start is called before the first frame update
    int spMode = 1;

    private void OnEnable()
    {
        EnemyHealth.OnDeath += Recount;
        PlayerHealth.OnDeath += CheckDeath;
    }

    private void OnDisable()
    {
        EnemyHealth.OnDeath -= Recount;
        PlayerHealth.OnDeath -= CheckDeath;
    }

    void Start()
    {
        flareTime = -flareCool;
        missileTime = new float[missilePos.Length];
        for (int i = 0; i < missileTime.Length; i++)
        {
            missileTime[i] = -missileCool;
        }
        spTime = new float[spPos.Length];
        for (int i = 0; i < missileTime.Length; i++)
        {
            spTime[i] = -bombCool;
        }
        cannonCurr = cannonMax;
        ally = GameObject.FindGameObjectsWithTag("Ally");
        //enemy = GameObject.FindGameObjectsWithTag("Enemy");
        enemy.AddRange(GameObject.FindGameObjectsWithTag("Enemy"));

        rb.velocity = transform.forward * (stallSpeed+15f);
        drag = rb.drag;
        angularDrag = rb.angularDrag;
        oldVelocity = rb.velocity;

        vaporFX = new ParticleSystem[vapor.GetComponent<Transform>().childCount];

        for (int i = 0; i < vapor.GetComponent<Transform>().childCount; i++)
        {
            vaporFX[i] = vapor.GetComponent<Transform>().GetChild(i).GetComponent<ParticleSystem>();
        }

        mainPlume = jetExh.GetComponent<ParticleSystem>();
        shockDiamonds = jetExh.transform.GetChild(0).GetComponent<ParticleSystem>();
    }

    public void Respawn()
    {
        transform.position = respawnPosition.position;
        transform.rotation = respawnPosition.rotation;
        rb.velocity = transform.forward * (stallSpeed + 1);
        //rigidbody.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        rollAngle = -RelAngle(rb.transform.rotation.eulerAngles.z);
        pitchAngle = -RelAngle(rb.transform.rotation.eulerAngles.x);
        yawAngle = -RelAngle(rb.transform.rotation.eulerAngles.y);

        //Control plane vapor effect
        float vaporMargin = 3;
        float vaporMax = 40;
        //print(vaporStrength);

        vaporStrength = Mathf.Clamp(vaporStrength, 0f, vaporMax);

        if (vaporStrength > vaporMargin)
        {
            vapor.SetActive(true);
            foreach (ParticleSystem vape in vaporFX)
            {
                //ParticleSystem.MainModule settings = vape.main;
                //settings.startColor = new Color(1, 1, 1, 0.1f * (vaporStrength - vaporMargin) / (vaporMax - vaporMargin));
                ChangeStartColor(vape, new Color(1, 1, 1, 0.2f * (vaporStrength - vaporMargin) / (vaporMax - vaporMargin)));
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

        //Set engine exhaust
        float plumeStrength = thrust / maxThrust * 0.5f;

        ChangeStartColor(mainPlume, new Color(plumeStrength, plumeStrength, plumeStrength, 0));
        ChangeStartColor(shockDiamonds, new Color(plumeStrength, plumeStrength, plumeStrength, 0));

        jetEngine.pitch = 1f + (thrust - 100f) * .001f;
        jetEngine.volume = 0.5f + (thrust - 200f) / 2000f;
        windSFX.pitch = 0.8f + speed / 100f;
        windSFX.volume = 0.3f + speed / 100f;

        //enemy.sort
        //enemy.OrderBy
        //Array.Sort(enemy);

        enemy.Sort(delegate (GameObject a, GameObject b)
            {
                Vector3 aPosition = rb.transform.InverseTransformVector(a.transform.position - rb.transform.position);
                Vector3 bPosition = rb.transform.InverseTransformVector(b.transform.position - rb.transform.position);
                Vector2 aProj = new Vector2(aPosition.x, aPosition.y);
                Vector2 bProj = new Vector2(bPosition.x, bPosition.y);
                float aDist = aProj.sqrMagnitude;
                float bDist = bProj.sqrMagnitude;
                if (aPosition.z < 0)
                {
                    aDist += 100000f;
                }
                if (bPosition.z < 0)
                {
                    bDist += 100000f;
                }

                return aDist.CompareTo(bDist);
                /*
                return Vector3.Distance(this.transform.position, a.transform.position)
                .CompareTo(
                Vector3.Distance(this.transform.position, b.transform.position));
                */
            }
        );

        for (int i = 0; i < enemy.Count; i++)
        {
            Rigidbody currentBody = enemy[i].GetComponent<Rigidbody>();
            if (currentBody != target)
            {
                if (currentBody == nextTarget)
                {
                    //do nothing
                    break;
                }
                else
                {
                    nextTarget = currentBody;
                    if (targNext != null)
                    {
                        targNext(nextTarget);
                    }
                    break;
                }
            }
            
        }

        //Command wingman follow
        if (Input.GetKeyDown("1"))
        {
            blip2.Play();
            for (int i = 0; i < wingman.Length; i++)
            {
                wingman[i].GetComponent<WingmanAI>().mode = 1;
            }
        }

        //Command wingman attack
        if (Input.GetKeyDown("2"))
        {
            blip1.Play();
            for (int i = 0; i < wingman.Length && i < enemy.Count; i++)
            {
                wingman[i].GetComponent<WingmanAI>().target = enemy[i].GetComponent<Rigidbody>();
                wingman[i].GetComponent<WingmanAI>().mode = 2;
            }
            //print("wingman attack");
        }
        //print(enemy[0]);

        //Fire missile or special
        if (Input.GetMouseButtonDown(1))
        {
            if (spMode == 1)
            {
                if (Time.time > missileTime[missileNext] + missileCool)
                {
                    GameObject missile = Instantiate(missilePrefab, missilePos[missileNext].position, transform.rotation);
                    missileTime[missileNext] = Time.time;
                    if (missileNext != missilePos.Length - 1)
                    {
                        missileNext++;
                    }
                    else
                    {
                        missileNext = 0;
                    }
                    missile.transform.SetParent(GameObject.Find("/Debris").transform);
                    missile.GetComponent<Rigidbody>().velocity = rb.velocity * 3f - rb.transform.up * 10f;
                    if ((locked) && (target != null))
                    {
                        missile.GetComponent<MissileTrack>().target = target;
                    }
                }
            }

            if (spMode == 2)
            {
                if (Time.time > spTime[spNext] + bombCool)
                {
                    //print("bombs away!");
                    GameObject bomb = Instantiate(spPrefab, spPos[spNext].position, transform.rotation);
                    spTime[spNext] = Time.time;
                    if (spNext != spPos.Length - 1)
                    {
                        spNext++;
                    }
                    else
                    {
                        spNext = 0;
                    }
                    bomb.transform.SetParent(GameObject.Find("/Debris").transform);
                    bomb.GetComponent<Rigidbody>().velocity = rb.velocity;
                    if (dropBomb != null)
                    {
                        dropBomb(bomb);
                    }
                    //Destroy(bomb, 5f);
                }

            }
            //bearing.transform.SetParent(goCanvas.transform);
        }

        //Switch fire mode
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (spMode < 2)
            {
                spMode++;
            }
            else
            {
                spMode = 1;
            }

            if (spSwitch != null)
            {
                spSwitch(spMode);
            }
        }

        //Flares
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (Time.time > flareTime + flareCool)
            {
                flareTime = Time.time;
                flares.GetComponent<ParticleSystem>().Play();
                ecm = true;
                flareSFX.Play();
            }
        }

        if (Time.time > flareTime + flareDuration)
        {
            ecm = false;
        }

        //Switch Targets
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            
            Rigidbody first = null;
            if (enemy.Count > 0)
            {
                first = enemy[0].GetComponent<Rigidbody>();
                if (first == target && enemy.Count > 1)
                {
                    target = enemy[1].GetComponent<Rigidbody>();
                }
                else
                {
                    target = first;
                }
            }
            if (targSwitch != null)
            {
                targSwitch(target);
            }
        }

        //Reset plane
        /*
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Respawn();
        }
        */
    }

    void Stall()
    {
        float stallRatio = (speed/stallSpeed)/5.0f;
        Vector3 tug = Vector3.up * stallStrength;
        //weak lift
        rb.AddForce((Vector3.up * agility) * stallRatio * rb.mass * 9.8f);
        rb.AddForce((transform.up * (1 - agility)) * stallRatio * rb.mass * 9.8f);

        //pitch down
        rb.AddForceAtPosition(transform.forward, tug);
        rb.AddForceAtPosition(-transform.forward, -tug);
        thrust = thrust + stallStrength/15.0f;
    }

    public float RelAngle(float inAngle)
    {
        float outAngle;
        if (inAngle > 180)
        {
            outAngle = inAngle - 360;
        }
        else
        {
            outAngle = inAngle;
        }
        return outAngle;
    }

    void FixedUpdate()
    {
        Vector3 relAccel = rb.transform.InverseTransformDirection((rb.velocity - oldVelocity) / Time.fixedDeltaTime);
        oldVelocity = rb.velocity;
        //print(rb.transform.InverseTransformDirection(rb.angularVelocity));
        vaporStrength = relAccel.magnitude + Mathf.Abs(rb.transform.InverseTransformDirection(rb.angularVelocity).z) * 1f;
        //update speed and acceleration
        speed = rb.velocity.magnitude;
        if (speed > stallSpeed)
        {
            rb.AddForce((Vector3.up * agility) * rb.mass * gravity);
            rb.AddForce((transform.up * (1 - agility)) * rb.mass * gravity);
        }
        else
        {
            Stall();
        }

        if (this.GetComponent<PlayerHealth>().health > 0)
        {

            //Fire cannons
            float shotTimer = 1f / fireRate;
            float cannonX;
            if (Input.GetMouseButton(0) && (cannonCurr > 0))
            {
                cannonX = Mathf.Lerp(cannonDoor.localEulerAngles.x, 90f, 8f * Time.fixedDeltaTime);
                if (!cannonSFX.isPlaying)
                {
                    cannonSFX.Play();
                }
                cannonFlash.SetActive(true);
            }
            else
            {
                cannonX = Mathf.Lerp(cannonDoor.localEulerAngles.x, 0f, 8f * Time.fixedDeltaTime);
                cannonSFX.Stop();
                cannonFlash.SetActive(false);
            }
            cannonDoor.localEulerAngles = new Vector3(cannonX, 0f, 0f);

            if (Input.GetMouseButton(0) && Time.time > nextFire)
            {

                if (cannonCurr > 0)
                {
                    //cannonFlash.SetActive(true);
                    nextFire = Time.time + shotTimer;

                    //Vector3 randSpread = new Vector3(Random.Range(-spread, spread), Random.Range(-spread, spread), 0f);
                    Vector3 randSpread = Random.insideUnitSphere * spread;
                    //randSpread.z = 0f;
                    Quaternion cannonRot = transform.rotation * Quaternion.Euler(randSpread);
                    GameObject bull = Instantiate(bulletPrefab, cannonPos.position + rb.velocity * Time.fixedDeltaTime, cannonRot);
                    bull.transform.SetParent(GameObject.Find("/Debris").transform);

                    cannonCurr--;
                }
            }
            else
            {
                if (cannonCurr < cannonMax)
                {
                    cannonCurr = cannonCurr + 2;
                }
                else
                {
                    cannonCurr = cannonMax;
                }
            }


            //Pitch Up
            if (Input.GetKey(KeyCode.W))
            {
                rb.AddTorque(transform.right * pitchRate);
            }

            //Pitch Down
            if (Input.GetKey(KeyCode.S))
            {
                rb.AddTorque(transform.right * -pitchRate);
            }

            //Roll Left
            if (Input.GetKey(KeyCode.A))
            {
                rb.AddTorque(transform.forward * rollRate);
            }

            //Roll Right
            if (Input.GetKey(KeyCode.D))
            {
                rb.AddTorque(transform.forward * -rollRate);
            }

            //Auto
            if (Input.GetKey(KeyCode.Q) && Input.GetKey(KeyCode.E))
            {
                rb.AddTorque(transform.right * pitchAngle / 90f * pitchRate);
                rb.AddTorque(transform.forward * rollAngle / 90f * rollRate);
            }

            //Yaw Left
            else if (Input.GetKey(KeyCode.Q))
            {
                rb.AddTorque(transform.up * -yawRate);
            }

            //Yaw Right
            else if (Input.GetKey(KeyCode.E))
            {
                rb.AddTorque(transform.up * yawRate);
            }

            //Increase Throttle
            if (Input.GetKey(KeyCode.LeftShift))
            {
                thrust = thrust + throttle;
            }

            //Decrease Throttle
            if (Input.GetKey(KeyCode.LeftControl))
            {
                thrust = thrust - throttle;
            }

            thrust = Mathf.Clamp(thrust, minThrust, maxThrust);

            //Post-Stall Maneuver
            if (Input.GetKey(KeyCode.Space))
            {
                rb.drag = drag / (psmStrength / 1.0f);
                rb.angularDrag = angularDrag / psmStrength;
                thrust = thrust - psmStrength / 1.2f;
                rb.AddForce(transform.forward * thrust / psmStrength);
                vaporStrength *= psmStrength * 2f;
            }
            else
            {
                rb.drag = drag;
                rb.angularDrag = angularDrag;
                rb.AddForce(transform.forward * thrust);
            }
        }
        else
        {
            DeathSpiral();
        }
    }

    void Recount(GameObject testEnemy)
    {
        //enemy = goCanvas.GetComponent<PlaneHUD>().enemy;
        /*
        GameObject[] temp1 = new GameObject[enemy.Length - 1];

        for (int i = 0; i + 1 < enemy.Length; i++)
        {
            if (enemy[i] == testEnemy)
            {
                enemy[i] = enemy[enemy.Length - 1];
                //break;
            }
            temp1[i] = enemy[i];
        }
        enemy = temp1;
        */

        for (int i = 0; i < enemy.Count; i++)
        {
            if (enemy[i] == testEnemy)
            {
                enemy.RemoveAt(i);
                break;
            }
        }
    }

    void ChangeStartColor(ParticleSystem parSys, Color newColor)
    {
        ParticleSystem.MainModule settings = parSys.main;
        settings.startColor = newColor;
    }

    void DeathSpiral()
    {
        float stallRatio = (speed / stallSpeed) / 5.0f;

        Vector3 tug = Vector3.right * stallStrength * rotStrength / 20f;
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

    void TrailDeath()
    {
        foreach (TrailRenderer trail in wingTrail)
        {
            trail.autodestruct = true;
            trail.transform.SetParent(GameObject.Find("/Debris").transform, true);
        }
    }

    void CheckDeath()
    {
        rotStrength = ((float)Random.Range(-1, 1) + .5f) * 12f * stallStrength;
        //print(rotStrength);
        Invoke("TrailDeath", 3f);
    }
}
