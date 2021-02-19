using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shoot : MonoBehaviour
{
    public float fireRate = 100f;

    public GameObject bulletPrefab;
    public Transform cannonPos;

    private float nextFire = 0f;
    private float spread = 0.3f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        //Fire cannons
        float shotTimer = 1f / fireRate;

        if (Input.GetMouseButton(0) && Time.time > nextFire)
        {

            //cannonFlash.SetActive(true);
            nextFire = Time.time + shotTimer;

            //Vector3 randSpread = new Vector3(Random.Range(-spread, spread), Random.Range(-spread, spread), 0f);
            Vector3 randSpread = Random.insideUnitSphere * spread;
            //randSpread.z = 0f;
            Quaternion cannonRot = cannonPos.rotation * Quaternion.Euler(randSpread);
            //GameObject bull = Instantiate(bulletPrefab, cannonPos.position + rb.velocity * Time.fixedDeltaTime, cannonRot);
            GameObject bull = Instantiate(bulletPrefab, cannonPos.position, cannonRot);
            bull.transform.SetParent(GameObject.Find("/Debris").transform);


        }
    }
}
