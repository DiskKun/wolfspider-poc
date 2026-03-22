using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CutsceneManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public GameObject[] imageCanvases;
    public GameObject clickNotice;
    private int canvasID;
    [SerializeField]
    [Tooltip("Contains a reference to the scene that will be loaded after this level is completed. Ensure that the target scene is included in the build settings scene list!")]
    private string NextScene;
    private bool loadingNext = false; // whether we're loading the next level yet


    void Start()
    {
        canvasID = 0; // reset canvas to display images for
        imageCanvases[canvasID].SetActive(true);
        imageCanvases[canvasID].GetComponent<CanvasFade>().resetFade();
    }

    // Update is called once per frame
    void Update()
    {
        if (!loadingNext)
        {
            if (canvasID < imageCanvases.Length)
            {
                CanvasFade cf = imageCanvases[canvasID].GetComponent<CanvasFade>();
                if (cf.imgID >= cf.images.Length)
                {
                    clickNotice.SetActive(true);
                }
                else
                {
                    clickNotice.SetActive(false); // enable and disable continue notice depending on whether all images are visible
                }
            }


            if (Input.GetMouseButtonDown(0)) // if we've clicked
            {
                CanvasFade cf = imageCanvases[canvasID].GetComponent<CanvasFade>();
                if (cf.imgID < cf.images.Length) // if we click before all images are visible, we can skip the fade-in
                {
                    cf.skipFade();
                }
                else if (canvasID < imageCanvases.Length)
                {
                    cf.gameObject.SetActive(false); // disable current canvas
                    canvasID++;
                    if (canvasID < imageCanvases.Length)
                    {
                        cf = imageCanvases[canvasID].GetComponent<CanvasFade>();
                        cf.gameObject.SetActive(true); // enable new canvas
                        cf.resetFade();
                    }
                    else
                    {
                        cf.gameObject.SetActive(false); // disable current canvas
                        clickNotice.SetActive(false);
                        SceneManager.LoadScene(NextScene);
                        //LoadNextScene(); // load next if we're done with cutscene canvases
                    }

                }
            }
        }
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
        //Time.timeScale = 0f; // really hacky solution to "disable" player movement and game interactions while loading the next scene. This can be replaced later if need be.
        LoadSceneAsync(NextScene);
    }
}
