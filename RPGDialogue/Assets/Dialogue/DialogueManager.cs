using System.Collections;
using NUnit.Framework;
using UnityEditor.AdaptivePerformance.Editor;
using UnityEditor.EditorTools;
using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
public class DialogueManager : MonoBehaviour
{
    [HideInInspector] public static DialogueManager Instance;
    [SerializeField] float defaultCharWaitMult;
    [Tooltip("The default multiplier of the waiting time between characters")]
    [SerializeField] float defaultTimeBetweenLines;

    [SerializeField] float defaultFastCharWaitMult;
    [Tooltip("The fast multiplier of the waiting time between characters")]
    [SerializeField] float defaultFastTimeBetweenLines;

    [SerializeField] List<DialogueMarkup> dialogueMarkups = new List<DialogueMarkup>();
    [Tooltip("Markups to trigger custom functions to edit dialogue. Should follow TextMeshPro markup standard format")]
    Dialogue curDialogue; 
    float timeTillNextChar;
    bool readingDialogue = false;
    
    string lastDialogueSource = "";
    string lastDisplayedDialogueText = "";
    
    
    [Header("Editable By Markups")]
    public float curStartWaitTime = 0;
    public float curEndWaitTime = 0;
    public DialogueExpression curExpression;

    public void ChangeCurExpression(DialogueEpressionID id)
    {
        if(curSpeaker == null)
        {
            return;
        }
        if(curSpeaker.getExpressionOf(id) != null)
            curExpression = curSpeaker.getExpressionOf(id);
        Debug.Log("Finished");
    }
    DialogueSpeaker curSpeaker;

    #region EVENTS
    public delegate void UpdateDialogue(string speakerName, DialogueExpression expression, string curText);
    public static event UpdateDialogue updateDialogue;

    void OnEnable()
    {
        DialoguePrompt.promptDialogue += ReadDialogue;
    }

    void OnDisable()
    {
        DialoguePrompt.promptDialogue -= ReadDialogue;
    }
    #endregion
    void Start()
    {
        if(Instance == null)
        Instance = this;

        timeTillNextChar = defaultCharWaitMult;
        
    }
    public void ReadDialogue(Dialogue dialogue)
    {
        if(readingDialogue)
            return;

        curDialogue = dialogue;
        if(dialogue.hasSpeaker)
        {
            curSpeaker = dialogue.speaker;
            curExpression = dialogue.startingExpression;
        }
        else
        {
            curSpeaker = null;
            curExpression = null;
        }
        readingDialogue = true;

        if(dialogue.hasTypeWriterEffect)
        {
            StartCoroutine(TypewriterReadDialogue(dialogue));
        }

        else
        {
            string totalText = "";
            for(int i = 0; i < dialogue.dialogueLines.Length; i++)
            {
                totalText += dialogue.dialogueLines[i] + "\n";
            }
            updateDialogue?.Invoke(dialogue.speaker.speakerName, dialogue.startingExpression, RemoveMarkupText(totalText));
        }
    }

    void ReadDialogueLine(Dialogue dialogue, int dialogueLineIndex, string dialogueLine)
    {
        //NEEDS MARKUP TEXT TO WAIT AND CHANGE EXPRESSIONS
        if(dialogue.hasTypeWriterEffect)
            StartCoroutine(TypewriterReadDialogue(dialogue));
        else
        {
            //Eventually need function that reads for markup text 
            updateDialogue?.Invoke(dialogue.speaker.speakerName, dialogue.startingExpression, dialogueLine);
        }
    }

    string GetDisplayText(int index, string dialogueString) //Clean everything in brackets until the entire tag is included //Need index to cutoff
    {
        string fullText = "";
        string visibleDisplayText = "";

        char[] textArray = dialogueString.ToCharArray();
        bool isReadingTag = false;

        for(int i = 0; i < dialogueString.Length; i++)
        {
            if(textArray[i] == '<')
                isReadingTag = true;

            if(isReadingTag)
                fullText += textArray[i];
            else
            {
                if(visibleDisplayText.Length < index + 1)
                {
                    visibleDisplayText += textArray[i];
                    fullText += textArray[i];
                }
                else
                {
                    //Grab any stray tags attached to visible text
                    fullText = AppendEndingMarkupText(fullText, textArray, i);
                    break;
                }
            }
            if(textArray[i] == '>')
                isReadingTag = false;
        }

        if(dialogueString != lastDialogueSource)
        {
            lastDialogueSource = dialogueString;
            lastDisplayedDialogueText = "";
        }

        //Get text added to display string per call
        string newAddedText;
        if(fullText.Length > lastDisplayedDialogueText.Length)
        {
            newAddedText = fullText.Substring(lastDisplayedDialogueText.Length);
        }
        else
        {
            newAddedText = "";
        }

        if(newAddedText != null && newAddedText != "")
        {   
            HandleMarkupLogic(newAddedText);
        }

        lastDisplayedDialogueText = fullText;
        return RemoveMarkupText(fullText);

    }

    void HandleMarkupLogic(string newText)
    {
        for(int i = 0; i < dialogueMarkups.Count; i++)
        {
            dialogueMarkups[i].HandleMarkup(this, newText);
        }
    }

    string RemoveMarkupText(string newText)
    {
        string handledMarkupText = newText;
       for(int i = 0; i < dialogueMarkups.Count; i++)
        {
            handledMarkupText = dialogueMarkups[i].RemoveFormatTags(handledMarkupText);
        }

        return handledMarkupText;
    }
    string AppendEndingMarkupText(string fullText, char[] textArray, int startIndex)
    {
        bool isReadingTag = false; 
        string appendText = fullText;

        for(int i = startIndex; i < textArray.Length; i++)
        {
            if(textArray[i] == '<')
                isReadingTag = true;
            if(isReadingTag)
                appendText += textArray[i];
            else
                break;
            if(textArray[i] == '>')
                isReadingTag = false;
        }

        return appendText;
    } 

    IEnumerator TypewriterReadDialogue(Dialogue dialogue) 
    {
        float t = 0;
        int charIndex = 0;
        string curText = "";
        string[] dialogueLines = dialogue.dialogueLines;


        for(int i = 0; i < dialogue.dialogueLines.Length; i++)
        {
            curText = "";
            charIndex = 0;
            t = 0;
            string dialogueLine = dialogueLines[i];
            string cleanedDialogue = RemoveMarkupText(dialogueLine);
            float textSpeed = defaultCharWaitMult;
            lastDisplayedDialogueText = null; 

            while(curText.Length < cleanedDialogue.Length)
            {   
                t += Time.deltaTime * textSpeed; 
        
                charIndex = Mathf.FloorToInt(t);
                charIndex = Mathf.Clamp(charIndex, 0, dialogueLine.Length);
                
                curText = dialogueLine.Substring(0, charIndex);
                curText = GetDisplayText(charIndex, dialogueLine);

                yield return new WaitForSeconds(curStartWaitTime); 

                updateDialogue?.Invoke(dialogue.speaker.speakerName, curExpression, curText);

                yield return new WaitForSeconds(curEndWaitTime);


                //Reset all values to 0?
                curStartWaitTime = 0;
                curEndWaitTime = 0;

                yield return null;
            }
            yield return new WaitForSeconds(defaultTimeBetweenLines);
        }
        
        readingDialogue = false;
        curDialogue = null;
    }
}
