using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileTrack : MonoBehaviour
{
    public delegate void radarWarn(GameObject missile);
    //public delegate void radarWarn2(GameObject missile);
    public static event radarWarn MissilePlayer;    //If enemy fires a missile
    public static event radarWarn MissileLost;      //If enemy missile is lost

    private Rigidbody rb;
    public GameObject hitMark;
    public Rigidbody target;
    public GameObject missileMesh;
    public GameObject missileJet;
    public AudioSource rocketMotor;

    SphereCollider coll;
    float trackSpeed = 9f;
    float trackAngle = 60f;
    float fuse = 6.2f;
    float delay = 0.2f;
    float relTime;
    private bool canExplode = true;
    private bool toPlayer = false;
    public bool friendly = true;

    // Start is called before the first frame update
    void Start()
    {
        coll = this.GetComponent<SphereCollider>();
        rb = this.GetComponent<Rigidbody>();
        relTime = Time.time;

        if ((MissilePlayer != null) && (target != null) && (target.tag == "Player"))
        {
            toPlayer = true;
            MissilePlayer(gameObject);
        }
        //rb.velocity = rb.transform.forward * 10f;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if ((Time.time > relTime + delay) && (Time.time < relTime + fuse) && (canExplode)) 
        {
            coll.enabled = true;
            rb.AddForce(rb.transform.forward * 200f);

            if (target != null)
            {
                Vector3 relDir = rb.transform.InverseTransformDirection(target.transform.position - rb.transform.position);
                float relProj = new Vector2(relDir.x, relDir.y).magnitude;
                float angle = Mathf.Atan2(relProj, relDir.z) * Mathf.Rad2Deg;
                //print(angle);
                
                //Check if target is within scope
                if ((angle < trackAngle) && (relDir.z > 0))
                {
                    float targetDistance = (target.transform.position - transform.position).magnitude;
                    float leadTime;
                    if (rb.velocity.magnitude > 15f)
                    {
                        leadTime = (targetDistance) / (rb.velocity.magnitude);
                        //leadTime = 0f;
                    }
                    else
                    {
                        leadTime = 0f;
                    }
                    Vector3 leadPos = target.transform.position + target.velocity * leadTime * 0.5f;
                    //print(rb.velocity.magnitude);

                    Vector3 direction = (target.transform.position - transform.position);
                    //Vector3 direction = (leadPos - transform.position);

                    Debug.DrawRay(transform.position, direction, Color.red);
                    Quaternion lookRot = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, trackSpeed * Time.fixedDeltaTime);
                }
                else
                {
                    Debug.DrawRay(transform.position, rb.transform.forward * 80f, Color.green, 3f);
                    //print(angle);
                    //print("no lock");
                    if ((MissileLost != null) && toPlayer)
                    {
                        MissileLost(gameObject);
                        toPlayer = false;
                    }
                    target = null;
                }

                //check if target deploys flares
                if ((target != null) && (target.CompareTag("Player")))
                {
                    bool flare = target.GetComponent<PlaneDriver>().ecm;
                    if (flare)
                    {
                        Debug.DrawRay(transform.position, rb.transform.forward * 80f, Color.green, 3f);
                        if ((MissileLost != null) && toPlayer)
                        {
                            MissileLost(gameObject);
                            toPlayer = false;
                        }
                        target = null;
                    }
                }
                //print(angle);
            }
        }
        else if ((Time.time > relTime + fuse) && canExplode)
        {
            SlowDestroy();
            //print("MISS");
        }


    }

    void OnTriggerEnter(Collider collision)
    {
        if ((collision.gameObject.tag != "Missile") && (collision.gameObject.tag != "Bullet"))
        {
            if (friendly && (collision.gameObject.tag != "Player") && (collision.gameObject.tag != "Ally"))
            {
                SlowDestroy();
                //print("HIT");
            }
            else if (!friendly && (collision.gameObject.tag != "Enemy"))
            {
                SlowDestroy();
                //print("HIT");
            }
        }
    }

    void SlowDestroy()
    {
        if ((MissileLost != null) && toPlayer)
        {
            MissileLost(gameObject);
            toPlayer = false;
        }

        canExplode = false;
        coll.radius = 2;
        
        rb.isKinematic = true;
        rocketMotor.Stop();

        //this.transform.Find("")
        missileMesh.GetComponent<Renderer>().enabled = false;
        missileJet.SetActive(false);
        GameObject hit = Instantiate(hitMark, transform.position, transform.rotation);
        hit.transform.SetParent(GameObject.Find("/Debris").transform);
        //hit.transform.localScale = Vector3.one * 5f;
        Invoke("DisableCollider", 0.1f);
        Destroy(hit, 2f);
        Destroy(gameObject, 5f);
    }

    void DisableCollider()
    {
        coll.enabled = false;
    }
}
