using System;
using TMPro;
using Unity.Mathematics;
using UnityEditor.U2D.Animation;
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
    void UpdateDialogue(string speakerName, DialogueExpression expression, string curText, int index)
    {
        UpdateSpeakerName(speakerName);
        UpdateSpeakerExpression(expression);

        if(expression == null)
        {
            UpdateAltTextbox(curText, index);
        }
        else
        {
            UpdateTextbox(curText, index);
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

    void UpdateAltTextbox(String dialogueText, int index)
    {
        textBox.SetActive(false);
        altTextBox.SetActive(true);

        IndexTextVisbility(altTextBox.GetComponent<TMP_Text>(), dialogueText, index);

    }

    void UpdateTextbox(String dialogueText, int index)
    {
        altTextBox.SetActive(false);
        textBox.SetActive(true);

        IndexTextVisbility(textBox.GetComponent<TMP_Text>(), dialogueText, index);
    }

    void IndexTextVisbility(TMP_Text textBox, string rawText, int index)
    {
       char[] textArray = rawText.ToCharArray();

        textBox.text = rawText;
        textBox.maxVisibleCharacters = index;
    }
}
