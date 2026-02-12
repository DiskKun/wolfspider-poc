using UnityEngine;
using Unity;
using System.Collections;
using System;
using TMPro;
using UnityEngine.UI;
public class DialogueManager : MonoBehaviour
{
    public TextAsset dialogueTextFile;
    public TextMeshProUGUI dialogueBoxTextMesh;
    public Button dialogueBoxButton;

    string[][] DialogueText = new string[][] {};

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // text array setup
        int index = 0;
        string[] dLines = dialogueTextFile.text.Split("\n");
        Array.Resize(ref DialogueText, dLines.Length);
        foreach (string s in dLines)
        {
            DialogueText[index] = s.Split("\t");
            index++;
        }

        ShowDialogue("F01");
    }

    string GetDialogueLineByID(string id) // returns the line of dialogue associated with the given dialogue ID (column 1 of the spreadsheet)
    {
        Debug.Log("Finding line ID !" + id + "!");
        foreach (string[] s in DialogueText)
        {
            if (s[0] == id)
            {
                return s[1];

            }
        }
        return id + "ERR: DIALOGUE LINE NOT FOUND";
    }

    string GetNextDialogueLineID(string id)
    {
        Debug.Log("Finding next line for ID !" + id + "!");
        foreach (string[] s in DialogueText)
        {
            if (s[0] == id)
            {
                return s[2].Remove(3); // for some INEXPLICABLE reason, a CARRIAGE RETURN is inserted into the string by default, so it must be removed. It took me like 2 hours to figure out what was going on, not an easy problem to see because it's an invisible character fucking things up!!!
            }
        }
        return id + "ERR: DIALOGUE ID NOT FOUND";
    }

    public void ShowDialogue(string id)
    {
        if (id == "END")
        {
            dialogueBoxTextMesh.transform.parent.gameObject.SetActive(false);
            return;
        }
        dialogueBoxTextMesh.transform.parent.gameObject.SetActive(true);
        dialogueBoxTextMesh.text = GetDialogueLineByID(id);
        dialogueBoxButton.onClick.RemoveAllListeners();
        string nextID = GetNextDialogueLineID(id);
        dialogueBoxButton.onClick.AddListener(delegate { ShowDialogue(nextID); });
    }

   
}
