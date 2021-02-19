using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileMove : MonoBehaviour
{

    public GameObject hitMark;
    public float speed = 100f;
    public bool friendly = true;
    float distance = 100f;
    private Rigidbody rb;
    private Vector3 lastPos;
    private float frameDist;
    private RaycastHit hit;
    // Start is called before the first frame update
    void Start()
    {
        float time = distance / speed;
        Destroy(gameObject, time);
        rb = this.GetComponent<Rigidbody>();
        rb.velocity = transform.forward * speed;
        lastPos = rb.position;
    }
    // Update is called once per frame

    void FixedUpdate()
    {
        transform.rotation = Quaternion.LookRotation(rb.velocity);
        //rb.velocity = transform.forward * speed;
        frameDist = Vector3.Distance(rb.position, lastPos);

        //Debug.Log(frameDist);
        //print(frameDist);
        if (Physics.Raycast(lastPos, transform.TransformDirection(Vector3.forward), out hit, frameDist))
        {
            if ((hit.transform != transform) || (hit.transform.gameObject.tag != "Bullet"))
            {

                if ((friendly && (hit.transform.gameObject.tag != "Player")) || (!friendly && (hit.transform.gameObject.tag != "Enemy")))
                {
                    GameObject hitM = Instantiate(hitMark, hit.transform.position, Quaternion.FromToRotation(-Vector3.forward, hit.normal));
                    hitM.transform.SetParent(GameObject.Find("/Debris").transform);
                    Destroy(hitM, 0.5f);
                    Destroy(gameObject);
                    //print(hit.transform.gameObject);
                }
                else
                {

                }
                //Debug.DrawRay(lastPos, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
                //Debug.Log("Did Hit");
                if (friendly && (hit.transform.CompareTag("Enemy")))
                {
                    hit.transform.gameObject.GetComponent<EnemyHealth>().health -= 7f;
                    //hit.transform.EnemyHealth.Health -= 7f;
                }
                if (!friendly && (hit.transform.CompareTag("Player")))
                {
                    hit.transform.gameObject.GetComponent<PlayerHealth>().health -= 7f;
                    //hit.transform.EnemyHealth.Health -= 7f;
                }


            }
        }
        else
        {
            //Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * frameDist, Color.white);
            //Debug.Log("Did not Hit");
        }
        lastPos = rb.position;
    }



    /*
    void OnCollisionEnter(Collision collision)
    {
        
        foreach (ContactPoint contact in collision.contacts)
        {
            Debug.DrawRay(contact.point, contact.normal, Color.red, 0.5f);
            //print(contact.otherCollider);
        }
        

        if ((collision.gameObject.tag != "Player") && (collision.gameObject.tag != "Bullet"))
        {
            //print(collision.gameObject);
            GameObject hit = Instantiate(hitMark, transform.position, transform.rotation);
            hit.transform.SetParent(GameObject.Find("/Debris").transform);
            Destroy(hit, 0.3f);
        }
        
        Destroy(gameObject);
    }
    */

    /*
    void OnTriggerEnter(Collider collision)
    {
        if ((collision.gameObject.tag != "Missile") && (collision.gameObject.tag != "Bullet") && (collision.gameObject.tag != "Bomb"))
        {
            if ((friendly && collision.gameObject.tag != "Player") || (!friendly && collision.gameObject.tag != "Enemy"))
            {
                GameObject hit = Instantiate(hitMark, transform.position, transform.rotation);
                hit.transform.SetParent(GameObject.Find("/Debris").transform);
                Destroy(hit, 0.5f);
                Destroy(gameObject);
            }
            
        }

        //Destroy(gameObject);
    }
    */
}
