using System;
using UnityEngine;
using UnityEngine.Events;

public class NPC_Base : MonoBehaviour
{
    [NonSerialized]
    public UnityEvent NPC_Interaction;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        NPC_Interaction = new UnityEvent(); // set up event listener for player interaction

        NPC_Interaction.AddListener(OnInteract); // when the NPC_Interaction function is called, it will activate the OnInteract() function
    }

    protected virtual void OnInteract() // we will be inheriting from this in order to use versatile interactions
    {
        Debug.Log("NPC Interaction Successful");
    }
}
