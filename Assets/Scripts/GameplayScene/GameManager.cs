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
    private SceneAsset MainMenuScene;



    private bool gamePaused = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gamePaused = false; // ensure game is not paused when main scene is loaded
        Time.timeScale = 1f; // reset time scale
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
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

}
