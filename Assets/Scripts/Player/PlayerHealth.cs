using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public delegate void playerDeath();
    //public UnityEvent death;
    public static event playerDeath OnDeath;
    public static event playerDeath ShowGameOver;

    Rigidbody rb;
    public float health = 100f;
    public float deathDelay = 3f;
    //public GameObject goCanvas;
    public GameObject explosion;
    public GameObject damage;
    public GameObject fire;
    public GameObject mesh;

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
        if (health < maxHealth / 2f)
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Missile")
        {
            if (!other.gameObject.GetComponent<MissileTrack>().friendly)
            {
                health -= missileDamage/2f;
            }

        }

        if (other.gameObject.tag == "Bullet")
        {
            if (!other.gameObject.GetComponent<ProjectileMove>().friendly)
            {
                health -= bulletDamage/2f;
            }

        }

        if (other.gameObject.tag == "Bomb")
        {
            if (!other.gameObject.GetComponent<BombTrigger>().friendly)
            {
                health -= bombDamage/2f;
            }
            
        }
    }

    void Death()
    {
        isAlive = false;

        if (OnDeath != null)
        {
            OnDeath();
        }
        Invoke("DeathAnimation", deathDelay);
        Invoke("AnimationEnd", deathDelay + 1f);
        //Destroy(gameObject, deathDelay + 0.03f);
        
        
        //gameObject = null;
    }

    void DeathAnimation()
    {
        GameObject splosion = Instantiate(explosion, transform.position, transform.rotation);
        splosion.transform.SetParent(GameObject.Find("/Debris").transform);
        Camera.main.transform.parent = null;
        mesh.SetActive(false);
        //Destroy(splosion, 2f);
    }

    void AnimationEnd()
    {
        if (ShowGameOver != null)
        {
            ShowGameOver();
        }
    }
}
