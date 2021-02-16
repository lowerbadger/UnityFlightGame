using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySound : MonoBehaviour
{
    public AudioSource[] ricochet;
    // Start is called before the first frame update
    void Start()
    {
        int rand = Random.Range(0, ricochet.Length);
        ricochet[rand].Play();
    }

}
