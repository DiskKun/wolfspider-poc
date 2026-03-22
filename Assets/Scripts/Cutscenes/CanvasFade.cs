using System;
using UnityEngine;
using UnityEngine.UI;

public class CanvasFade : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public Image[] images;
    [NonSerialized]
    public int imgID;
    private float fadeTimer = 0;
    public float fadeDuration = 1f;

    void Start()
    {
        imgID = 0; // reset current image
    }

    // Update is called once per frame
    void Update()
    {
        if (gameObject.activeSelf && imgID < images.Length)
        {
            fadeTimer += Time.deltaTime;
            images[imgID].color = new Color(1, 1, 1, fadeTimer / fadeDuration); // set transparency for fade-in
            if (fadeTimer >= fadeDuration)
            {
                imgID++;
                fadeTimer = 0; // increment current image
            }
        }
    }

    public void skipFade()
    {
        for (int i = 0; i < images.Length; i++)
        {
            images[i].color = new Color(1, 1, 1, 1); // make all images opaque at once
        }
        imgID = images.Length; // set to max
    }

    public void resetFade()
    {
        for (int i = 0; i < images.Length; i++)
        {
            images[i].color = new Color(1, 1, 1, 0); // make all images transparent at once
        }
        imgID = 0; // reset image ID
        fadeTimer = 0; // and fade timer
    }
}
