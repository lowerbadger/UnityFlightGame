
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Events;

public class EnemyHealth : MonoBehaviour
{
    public delegate void npcChange(GameObject testEnemy);
    //public UnityEvent death;
    public static event npcChange OnDeath;

    Rigidbody rb;
    public float health = 100f;
    public float deathDelay = 3f;
    //public GameObject goCanvas;
    public GameObject explosion;
    public GameObject damage;
    public GameObject fire;

    private float maxHealth;
    private float bulletDamage = 7f;
    private float missileDamage = 60f;
    private float bombDamage = 120f;
    private bool isAlive = true;
    // Start is called before the first frame update
    void Start()
    {
        maxHealth = health;
    }

    // Update is called once per frame
    void Update()
    {
        if (health < maxHealth/2f)
        {
            damage.SetActive(true);
        }
        else
        {
            damage.SetActive(false);
            fire.SetActive(false);
        }
        if ((health < 0) && isAlive)
        {
            Death();
            fire.SetActive(true);
        }
    }

    /*
    Maybe use this for terrain damage latter
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Bullet")
        {
            health -= bulletDamage;
        }
    }
    */

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Missile")
        {
            if (other.gameObject.GetComponent<MissileTrack>().friendly)
            {
                health -= missileDamage;
            }
            
        }

        if (other.gameObject.tag == "Bullet")
        {
            if (other.gameObject.GetComponent<ProjectileMove>().friendly)
            {
                health -= bulletDamage;
            }
            
        }

        if (other.gameObject.tag == "Bomb")
        {
            if (other.gameObject.GetComponent<BombTrigger>().friendly)
            {
                health -= bombDamage;
            };
        }
    }

    /*
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Bomb")
        {
            health -= bombDamage;
        }
    }
    */
    void Death()
    {
        isAlive = false;

        if (OnDeath != null)
        {
            OnDeath(gameObject);
        }
        Invoke("DeathAnimation", deathDelay);
        Destroy(gameObject, deathDelay + 0.03f);
        //gameObject = null;
    }

    void DeathAnimation()
    {
        GameObject splosion = Instantiate(explosion, transform.position, transform.rotation);
        splosion.transform.SetParent(GameObject.Find("/Debris").transform);
        //Destroy(splosion, 2f);
    }
}
