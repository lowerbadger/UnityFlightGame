using UnityEngine.Audio;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public AudioMixer mixer;
    public Slider slider;
    //public string parameterName = "MasterVolume";
    // Start is called before the first frame update
    void Start()
    {
        //mixer.SetFloat("MasterVolume", 0f);
        slider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
    }


    // Update is called once per frame
    

    public void SetLevel(float sliderValue)
    {
        //float sliderValue = slider.value;
        mixer.SetFloat("MasterVolume", Mathf.Log10(sliderValue) * 20);
        PlayerPrefs.SetFloat("MasterVolume", sliderValue);
    }
}
