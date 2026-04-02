using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButton : MonoBehaviour
{
    [SerializeField]
    private GameObject MainMenu;
    [SerializeField]
    private GameObject SettingsMenu;
    [SerializeField]
    private GameObject LoadingScreen;
    [SerializeField]
    private GameObject SplashScreen;
    [SerializeField]
    [Tooltip("Time in seconds for the splash screen to stay on-screen when the project loads.")]
    private float splashScreenTime;
    private float splashScreenTimer; // the actual timer

    [SerializeField]
    private SFX_Menu SFX_Menu;
    private AudioSource audioSource;
    private AudioClip menuBlip;
    private AudioClip menuSelect;
    private AudioClip menuBack;
    private AudioClip menuContinue;
    private bool queueLoadGame;

    [Space(20)]

    [SerializeField]
    private string GameplayScene;

    private void Start()
    {
        splashScreenTimer = splashScreenTime;
        Time.timeScale = 1f; // reset timescale

        audioSource = GetComponent<AudioSource>();

        menuBlip = SFX_Menu.blipSFX; // load audio hooks
        menuSelect = SFX_Menu.selectSFX;
        menuBack = SFX_Menu.backSFX;
        menuContinue = SFX_Menu.continueSFX;
    }

    void Update()
    {
        if (splashScreenTimer > 0)
        {
            splashScreenTimer -= Time.deltaTime; // only keep the splash screen on-screen for a certain amount of time
            if (splashScreenTimer <= 0)
            {
                SplashScreen.SetActive(false); // then activate the main menu
                MainMenu.SetActive(true);
            }
        }
        if (queueLoadGame && !audioSource.isPlaying)
        {
            StartCoroutine(LoadSceneAsync(GameplayScene));
        }
    }

    public void StartButtonClick()
    {
        MainMenu.SetActive(false);
        LoadingScreen.SetActive(true);
        queueLoadGame = true;
    }

    public void SettingsButtonClick()
    {
        MainMenu.SetActive(false);
        SettingsMenu.SetActive(true); // toggle settings menu ON and main menu OFF
    }

    public void QuitButtonClick()
    {
        Application.Quit(); // quit the game; no effect in editor
    }

    public void SettingsBackButtonClick()
    {
        MainMenu.SetActive(true);
        SettingsMenu.SetActive(false); // toggle settings menu OFF and main menu ON
    }

    IEnumerator LoadSceneAsync(string scenePath)
    {
        // load the scene in the background while the current scene runs
        // this allows us to add loading screens if need be

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scenePath);

        // do nothing until scene is loaded
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    public void playMenuAudio(string clip)
    {
        audioSource.pitch = Random.Range(0.925f, 1.075f);
        if (clip == "blip")
        {
            audioSource.PlayOneShot(menuBlip, 0.6f);
        }
        if (clip == "select")
        {
            audioSource.PlayOneShot(menuSelect, 0.6f);
        }
        if (clip == "back")
        {
            audioSource.PlayOneShot(menuBack, 0.6f);
        }
        if (clip == "continue")
        {
            audioSource.PlayOneShot(menuContinue, 0.6f);
        }
    }
}
