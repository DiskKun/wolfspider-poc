using UnityEngine;

[CreateAssetMenu(fileName = "SFX_Menu", menuName = "Scriptable Objects/SFX_Menu")]
public class SFX_Menu : ScriptableObject
{
    public AudioClip blipSFX;
    public AudioClip selectSFX;
    public AudioClip backSFX;
    public AudioClip continueSFX;
}
