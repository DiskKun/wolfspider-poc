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

    [Space(20)]

    [SerializeField]
    private SceneAsset GameplayScene;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
