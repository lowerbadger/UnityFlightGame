using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileMove : MonoBehaviour
{

    public GameObject hitMark;
    public float speed = 100f;
    public bool friendly = true;
    float distance = 100f;
    Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        float time = distance / speed;
        Destroy(gameObject, time);
        rb = this.GetComponent<Rigidbody>();
        rb.velocity = transform.forward * speed;
    }
    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.LookRotation(rb.velocity);
        //rb.velocity = transform.forward * speed;
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
}
