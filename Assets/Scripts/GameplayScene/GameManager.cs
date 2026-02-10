using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private GameObject SettingsMenu;
    [SerializeField]
    private GameObject LoadingScreen;
    [SerializeField]
    private GameObject PauseMenu;

    [Space(20)]

    [SerializeField]
    [Tooltip("Contains a reference to the Main Menu scene.")]
    private SceneAsset MainMenuScene;

    [Space(20)]

    [SerializeField]
    [Tooltip("Contains a reference to the scene that will be loaded after this level is completed. Ensure that the target scene is included in the build settings scene list!")]
    private SceneAsset NextScene;
    private bool loadingNext = false; // whether we're loading the next level yet



    private bool gamePaused = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        loadingNext = false; // make sure the script doesn't think we're loading the next level
        gamePaused = false; // ensure game is not paused when main scene is loaded
        Time.timeScale = 1f; // reset time scale
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !loadingNext)
        {
            TogglePauseGame(); // pause or resume the game when esc key pressed
        }
    }

    public void TogglePauseGame()
    {
        gamePaused = !gamePaused;

        if (gamePaused) {
            Cursor.visible = true;
            Time.timeScale = 0f; // "stop time" so that main gameplay loop is suspended while paused - cheap and easy
            PauseMenu.SetActive(true);
        } else
        {
            Cursor.visible = false;
            Time.timeScale = 1f; // resume time when unpausing
            PauseMenu.SetActive(false);
            SettingsMenu.SetActive(false); // we also turn off the settings menu here, just in case they hit esc instead of the "resume" button
        }
        
    }

    public void SettingsButton()
    {
        PauseMenu.SetActive(false);
        SettingsMenu.SetActive(true); // toggle settings menu ON and pause menu OFF
    }
    
    public void SettingsBackButton()
    {
        PauseMenu.SetActive(true);
        SettingsMenu.SetActive(false); // toggle settings menu OFF and pause menu ON
    }

    public void MainMenuButton()
    {
        PauseMenu.SetActive(false);
        SettingsMenu.SetActive(false);
        LoadingScreen.SetActive(true);
        StartCoroutine(LoadSceneAsync(MainMenuScene.name)); // load main menu
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

    public void LoadNextScene() // can be called from any script with a reference to the GameManager object to load the next scene
    {
        loadingNext = true;
        PauseMenu.SetActive(false);
        SettingsMenu.SetActive(false); // disable all other menus
        LoadingScreen.SetActive(true); // enable loading screen
        Time.timeScale = 0f; // really hacky solution to "disable" player movement and game interactions while loading the next scene. This can be replaced later if need be.
        LoadSceneAsync(NextScene.name);
    }

}
