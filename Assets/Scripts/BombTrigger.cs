using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombTrigger : MonoBehaviour
{
    public delegate void wepExplode(GameObject bomb);
    public static event wepExplode bombExplode;

    public GameObject hitMark;

    public bool friendly = true;
    float timer;
    float maxTime = 20f;
    Rigidbody rb;
    SphereCollider coll;
    // Start is called before the first frame update
    void Start()
    {
        coll = this.GetComponent<SphereCollider>();
        timer = Time.time + maxTime;
        rb = this.GetComponent<Rigidbody>();
    }

    void Update()
    {
        transform.rotation = Quaternion.LookRotation(rb.velocity);
        if (Time.time > timer)
        {
            Explode();
        }
    }

    void OnTriggerEnter(Collider collision)
    {
        if (friendly)
        {
            if ((collision.gameObject.tag != "Player") && (collision.gameObject.tag != "Missile")
                && (collision.gameObject.tag != "Bullet") && (collision.gameObject.tag != "Bomb"))
            {
                Explode();
            }
        }
        else
        {
            if ((collision.gameObject.tag != "Enemy") && (collision.gameObject.tag != "Missile") 
                && (collision.gameObject.tag != "Bullet") && (collision.gameObject.tag != "Bomb"))
            {
                Explode();
            }
        }
    }

    void Explode()
    {
        coll.radius = 12;
        GameObject hit = Instantiate(hitMark, transform.position + Vector3.up*2f, transform.rotation);
        hit.transform.SetParent(GameObject.Find("/Debris").transform);
        Destroy(hit, 4f);
        Destroy(gameObject,0.5f);
        if (bombExplode != null)
        {
            bombExplode(gameObject);
        }
    }
}
