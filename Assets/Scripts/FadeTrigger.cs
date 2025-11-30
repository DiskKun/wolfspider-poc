using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

public class FadeTrigger : MonoBehaviour
{
    public Material mat;
    public CinemachineCamera cam;
    public GameObject player;
    public float zoomFOV;
    float normalFOV;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        normalFOV = cam.Lens.FieldOfView;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            StopAllCoroutines();
            StartCoroutine(FadeMaterial(0, 0.2f));
            cam.Target.TrackingTarget = gameObject.transform;
            cam.Lens.FieldOfView = 17.3f;
        }
        
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            StopAllCoroutines();
            StartCoroutine(FadeMaterial(1, 0.2f));
            cam.Target.TrackingTarget = player.transform;
            cam.Lens.FieldOfView = 34;
        }
        
    }

    IEnumerator FadeMaterial(float targetAlpha, float time)
    {

        float startingAlpha = mat.color.a;
        float t = 0;
        t = 0;
        while (mat.color.a != targetAlpha)
        {
            t += Time.deltaTime;
            Color c = mat.color;
            c.a = Mathf.Lerp(startingAlpha, targetAlpha, t / time);

            mat.SetColor(Shader.PropertyToID("_BaseColor"), c);
            yield return null;
        }
        Debug.Log("finished");
    }

    
}
