using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SAMsiteAI : MonoBehaviour
{
    public delegate void radar(int spike);
    public static event radar radarSpike;    //If enemy locks on

    public Rigidbody target;
    public Transform turret;
    public Transform launcher;

    public GameObject missilePrefab;
    public Transform[] missilePos;
    public int mode = 1;

    private float turretRotate = 0.3f;
    private float lockSpeed = 2f;
    private float lockAngle = 30f;
    private float lockTimer = 0;
    private float missileRate = 3f;
    private float missileTimer = 0;
    private int missileNext = 0;
    private bool locked = false;
    private float aggroDist = 220f;
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
                    Quaternion targetRot = Quaternion.LookRotation(targetDirection);
                    Vector3 targetSlerp = Quaternion.Slerp(launcher.rotation, targetRot, turretRotate * Time.deltaTime).eulerAngles;
                    turret.eulerAngles = new Vector3(-90f, targetSlerp.y, 0f);
                    launcher.eulerAngles = new Vector3(targetSlerp.x, targetSlerp.y, 0f);

                    Vector3 relDir = launcher.transform.InverseTransformDirection(targetDirection);
                    WSO(relDir);

                    if (targetDirection.magnitude > aggroDist + 20f)
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
        else
        {
            if ((radarSpike != null) && locked && target == player.GetComponent<Rigidbody>())
            {
                radarSpike(-1);
                locked = false;
            }
        }
        
            
    }

    //weapon systems operator
    void WSO(Vector3 targetRelPos)
    {
        //Vector3 targetRelPos = rb.transform.InverseTransformVector(target.transform.position - rb.transform.position);
        Vector2 targetProj = new Vector2(targetRelPos.x, targetRelPos.y);
        float targetAngle = Mathf.Atan2(targetProj.magnitude, targetRelPos.z) * Mathf.Rad2Deg;

        if ((targetAngle < lockAngle) && (targetRelPos.magnitude < 200f))
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
        GameObject missile = Instantiate(missilePrefab, missilePos[missileNext].position, missilePos[missileNext].rotation);
        if (missileNext != missilePos.Length - 1)
        {
            missileNext++;
        }
        else
        {
            missileNext = 0;
        }
        missile.transform.SetParent(GameObject.Find("/Debris").transform);
        //missile.GetComponent<Rigidbody>().velocity = rb.velocity * 1.4f - rb.transform.up * 8f;

        //enemies can't dumbfire missiles
        missile.GetComponent<MissileTrack>().target = target;
        missile.GetComponent<MissileTrack>().friendly = false;
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
