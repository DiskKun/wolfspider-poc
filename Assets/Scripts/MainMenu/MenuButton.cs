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
    private float splashScreenTimer;

    [Space(20)]

    [SerializeField]
    private SceneAsset GameplayScene;



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
    }

    public void StartButtonClick()
    {
        MainMenu.SetActive(false);
        LoadingScreen.SetActive(true);
        StartCoroutine(LoadSceneAsync(GameplayScene.name));
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
}
