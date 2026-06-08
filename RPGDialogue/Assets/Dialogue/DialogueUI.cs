using System;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    [SerializeField] Image portraitImage;
    [SerializeField] GameObject portrait;
    [SerializeField] TextMeshProUGUI nameTF;
    [SerializeField] GameObject nameTag;
    [SerializeField] TextMeshProUGUI dialogueTF;
    [SerializeField] TextMeshProUGUI altDialogueTF;
    [SerializeField] GameObject textBox;
    [SerializeField] GameObject altTextBox;

    #region EVENTS

    void OnEnable()
    {
        DialogueManager.updateDialogue += UpdateDialogue;

    }

    void OnDisable()
    {
        DialogueManager.updateDialogue -= UpdateDialogue;
    }

    #endregion
    void UpdateDialogue(string speakerName, DialogueExpression expression, string curText)
    {
        UpdateSpeakerName(speakerName);
        UpdateSpeakerExpression(expression);

        if(expression == null)
        {
            UpdateAltTextbox(curText);
        }
        else
        {
            UpdateTextbox(curText);
        }

    }

    void UpdateSpeakerName(string name)
    {
        if(name == null || name == "") 
        {
            Debug.Log("kdjfdkfdkjf");
            nameTag.SetActive(false);
        }
        else
        {
            nameTag.SetActive(true);
            nameTF.text = name;
        }
    }

    void UpdateSpeakerExpression(DialogueExpression expression)
    {
        if(expression == null)
        {
           portrait.SetActive(false);
        }
        else
        {
            portrait.SetActive(true);
            portraitImage.sprite = expression.staticSprite;
        }
    }

    void UpdateAltTextbox(String dialogueText)
    {
        textBox.SetActive(false);
        altTextBox.SetActive(true);

        altDialogueTF.text = dialogueText;
    }

    void UpdateTextbox(String dialogueText)
    {
        altTextBox.SetActive(false);
        textBox.SetActive(true);

        dialogueTF.text = dialogueText;
    }
}
