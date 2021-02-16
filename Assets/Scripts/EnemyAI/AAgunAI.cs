using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AAgunAI : MonoBehaviour
{
    public Rigidbody target;
    public Transform turret;
    public Transform cannon;
    public GameObject bulletPrefab;
    public Transform[] cannonPos;
    public float fireRate = 50f;
    public int mode = 1;
    public AudioSource cannonSFX;

    private float turretRotate = 1.5f;
    private float bulletSpeed = 100f;
    private float nextFire = 0f;
    private int cannonNext = 0;
    private float spread = 2f;
    private float aggroDist = 100f;
    private GameObject[] ally;
    private GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        ally = GameObject.FindGameObjectsWithTag("Ally");
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        if (this.GetComponent<EnemyHealth>().health > 0)
        {
            if (mode == 1)
            {
                Aggro();
            }
            else if (mode == 2)
            {
                if (target != null)
                {
                    Vector3 targetDirection = (target.transform.position - transform.position);

                    float targetDist = targetDirection.magnitude;
                    Vector3 targetVelCorrection = target.velocity * targetDist / bulletSpeed;
                    Vector3 gravCorrection = Mathf.Pow(targetDist / bulletSpeed, 2) * Physics.gravity * 0.5f;
                    targetDirection += targetVelCorrection - gravCorrection;
                    Quaternion targetRot = Quaternion.LookRotation(targetDirection);
                    Vector3 targetSlerp = Quaternion.Slerp(cannon.rotation, targetRot, turretRotate * Time.deltaTime).eulerAngles;
                    turret.eulerAngles = new Vector3(-90f, targetSlerp.y, 0f);
                    cannon.eulerAngles = new Vector3(targetSlerp.x, targetSlerp.y, 0f);

                    Vector3 relDir = cannon.transform.InverseTransformDirection(targetDirection);
                    if (targetDist < 80f)
                    {
                        WSO(relDir);
                    }
                    if (targetDist > aggroDist + 20f)
                    {
                        mode = 1;
                    }
                }
                else
                {
                    mode = 1;
                }
            }
            
        }
    }

    void WSO(Vector3 targetRelPos)
    {
        float cannonAngle = Mathf.Acos(targetRelPos.z / targetRelPos.magnitude) * Mathf.Rad2Deg;
        float shotTimer = 1f / fireRate;
        //print(cannonAngle);

        if ((cannonAngle < 20f) && (Time.time > nextFire))
        {
            if (!cannonSFX.isPlaying)
            {
                cannonSFX.Play();
            }
            nextFire = Time.time + shotTimer;
            Vector3 randSpread = Random.insideUnitSphere * spread;
            Quaternion cannonRot = cannonPos[cannonNext].rotation * Quaternion.Euler(randSpread);
            GameObject bull = Instantiate(bulletPrefab, cannonPos[cannonNext].position, cannonRot);
            bull.transform.SetParent(GameObject.Find("/Debris").transform);
            bull.GetComponent<ProjectileMove>().friendly = false;
            cannonNext++;
            if (cannonNext > cannonPos.Length - 1)
            {
                cannonNext = 0;
            }
        }
        else
        {
            cannonSFX.Stop();
        }
    }

    //Aggro ally or player
    void Aggro()
    {
        foreach (GameObject curAlly in ally)
        {
            if (Vector3.Distance(curAlly.transform.position, this.transform.position) < aggroDist)
            {
                target = curAlly.GetComponent<Rigidbody>();
                mode = 2;
            }
        }

        if (Vector3.Distance(player.transform.position, this.transform.position) < aggroDist)
        {
            target = player.GetComponent<Rigidbody>();
            mode = 2;
        }
    }
}
