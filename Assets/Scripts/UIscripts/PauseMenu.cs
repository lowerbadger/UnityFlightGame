using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;
    public GameObject pauseMenuUI;
    public GameObject gameOverUI;
    public GameObject optionsUI;
    public AudioMixer mixer;
    //public AudioListener planeAudio;
    public GameObject playerCamera;
    bool playerIsAlive = true;
    private float currentVolume = 0f;

    private void OnEnable()
    {
        PlayerHealth.ShowGameOver += GameOver;
    }

    private void OnDisable()
    {
        PlayerHealth.ShowGameOver -= GameOver;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerIsAlive)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (GameIsPaused)
                {
                    Resume();
                }
                else
                {
                    Pause();
                }
            }
        }
        
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
        //mixer.SetFloat("MasterVolume", currentVolume);
        optionsUI.SetActive(false);
        playerCamera.GetComponent<AudioListener>().enabled = true;
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        optionsUI.SetActive(false);
        Time.timeScale = 0f;
        GameIsPaused = true;
        //planeAudio.SetActive(false);
        playerCamera.GetComponent<AudioListener>().enabled = false;
        //currentVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        //mixer.SetFloat("MasterVolume", -100f);
    }

    public void Options()
    {
        pauseMenuUI.SetActive(false);
        optionsUI.SetActive(true);
    }

    public void LoadMenu()
    {
        //Debug.Log("Loading Menu...");
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
        GameIsPaused = false;
    }

    public void RestartMission()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        GameIsPaused = false;
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }

    void GameOver()
    {
        gameOverUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;
    }
}
