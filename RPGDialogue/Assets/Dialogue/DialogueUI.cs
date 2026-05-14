using System;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    [SerializeField] Image portraitImage;
    [SerializeField] TextMeshProUGUI nameTF;
    [SerializeField] TextMeshProUGUI dialogueTF;

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
        UpdateTextbox(curText);

    }

    void UpdateSpeakerName(string name)
    {
        nameTF.text = name;
    }

    void UpdateSpeakerExpression(DialogueExpression expression)
    {
        portraitImage.sprite = expression.staticSprite;
    }

    void UpdateTextbox(String dialogueText)
    {
        dialogueTF.text = dialogueText;
    }
}
