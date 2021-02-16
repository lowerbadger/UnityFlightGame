using UnityEngine.Audio;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioMixer mixer;
    //public string parameterName = "MasterVolume";
    // Start is called before the first frame update
    void Start()
    {
        mixer.SetFloat("MasterVolume", 0f);
    }


    // Update is called once per frame
    void Update()
    {
        if (PauseMenu.GameIsPaused)
        {
            mixer.SetFloat("MasterVolume", -100f);
        }
        else
        {
            mixer.SetFloat("MasterVolume", 0f);
        }
    }
}
