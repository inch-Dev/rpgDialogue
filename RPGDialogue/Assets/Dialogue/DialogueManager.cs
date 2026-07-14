using System.Collections;
using NUnit.Framework;
using UnityEditor.AdaptivePerformance.Editor;
using UnityEditor.EditorTools;
using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using Unity.VisualScripting;
public class DialogueManager : MonoBehaviour
{
    [HideInInspector] public static DialogueManager Instance;

    [SerializeField] DialogueSpeed curSpeed;

    public void ChangeSpeed(DialogueSpeedID ID)
    {
        foreach(DialogueSpeed ds in dialogueSpeeds)
        {
            if(ds.id == ID)
            {
                ChangeSpeed(ds);
            }
        }
    }
    public void ChangeSpeed(DialogueSpeed newSpeed)
    {
        curSpeed = newSpeed;
        charWaitSeconds = curSpeed.charWaitSeconds;
        lineWaitSeconds = curSpeed.lineWaitSeconds;
    }

    [SerializeField] List<DialogueSpeed> dialogueSpeeds;
    [SerializeField] float charWaitSeconds = 10;
    [SerializeField] float lineWaitSeconds = 5;


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

    public void ChangeExpression(DialogueExpressionID id)
    {
        if(curSpeaker == null)
        {
            return;
        }
        if(curSpeaker.getExpressionOf(id) != null)
            curExpression = curSpeaker.getExpressionOf(id);
    }
    DialogueSpeaker curSpeaker;

    #region EVENTS
    public delegate void UpdateDialogue(string speakerName, DialogueExpression expression, string curText);
    public static event UpdateDialogue updateDialogue;

    #endregion
    void Start()
    {
        if(Instance == null)
        Instance = this;
        timeTillNextChar = charWaitSeconds;

        ChangeSpeed(DialogueSpeedID.DEFAULT);
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

    string IndexText(int index, string rawText) 
    {
        //Get all text needed to process till certain index
                                                                                                                      
        string indexedText = "";
        string visibleText = "";

        char[] textArray = rawText.ToCharArray();
        bool isReadingTag = false;

        for(int i = 0; i < rawText.Length; i++)
        {
            if(textArray[i] == ' ')
            {
                
            }
            foreach(DialogueMarkup dm in dialogueMarkups)
            {
                if(dm.ValidateMarkup(rawText, i))
                {
                    //Recognized markup
                    //Get length of markup
                    dm.GetMarkupText(rawText, i);
                    Debug.Log($"Found markup text {dm.GetMarkupText(rawText, i)}");
                }
            }
            if(textArray[i] == '<')
            {
                //Run recognition for each tag and add to index accordingly
                isReadingTag = true;
            }

            if(isReadingTag)
                indexedText += textArray[i];
            else
            {
                if(visibleText.Length <= index)
                {
                    visibleText += textArray[i];
                    indexedText += textArray[i];
                }
                else
                {
                    indexedText = AppendMarkups(indexedText, textArray, i);
                    break;
                }
            }
            if(textArray[i] == '>')
            {
                //Check for valid tag
                isReadingTag = false;
            }
        }

        //If new line of dialogue clear comparison text
        if(rawText != lastDialogueSource)
        {
            lastDialogueSource = rawText;
            lastDisplayedDialogueText = "";
        }

        //Get text added to display string per call
        string newAddedText;
        if(indexedText.Length > lastDisplayedDialogueText.Length)
        {
            newAddedText = indexedText.Substring(lastDisplayedDialogueText.Length);
        }
        else
        {
            newAddedText = "";
        }

        if(newAddedText != null && newAddedText != "")
        {   //Debug.Log($"New text displayed is {newAddedText}");
            HandleMarkupLogic(newAddedText);
        }

        lastDisplayedDialogueText = indexedText;
        return RemoveMarkupText(indexedText);      
    }

//MOVE TO UI
    void DisplayText(int curIndex, string dialogueText) //Display text values to index
    {

        int visibleTextCount = 0;
        for(int i = 0; i < dialogueText.Length; i++)
        {
            if(visibleTextCount < curIndex)
            {

                //Check to recognize 
                //Toggle on visibility


            }

            else
            {
                //Toggle off visibility
            }
        }
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
            handledMarkupText = dialogueMarkups[i].RemoveMarkupText(handledMarkupText);
        }

        return handledMarkupText;
    }
    string AppendMarkups(string fullText, char[] textArray, int startIndex)
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
        int charIndex = 0;
        string curText = "";
        string[] dialogueLines = dialogue.dialogueLines;

        for(int i = 0; i < dialogue.dialogueLines.Length; i++)
        {
            curText = "";
            charIndex = 0;

            string dialogueLine = dialogueLines[i];
            string cleanedDialogue = RemoveMarkupText(dialogueLine);
            Debug.Log($"Cleaned dialogue: {cleanedDialogue}");
            lastDisplayedDialogueText = null; 

            while(charIndex <= cleanedDialogue.Length)
            {   
                float localCharWaitSeconds = charWaitSeconds;
                float localLineWaitSeconds = lineWaitSeconds;

                curText = dialogueLine.Substring(0, charIndex);
                curText = IndexText(charIndex, dialogueLine);
                //Debug.Log("Calling to change text");

                yield return new WaitForSeconds(curStartWaitTime); 

                updateDialogue?.Invoke(dialogue.speaker.speakerName, curExpression, curText);

                yield return new WaitForSeconds(curEndWaitTime);

                curStartWaitTime = 0;
                curEndWaitTime = 0;

                yield return new WaitForSeconds(localCharWaitSeconds);
                charIndex++; 
                //Debug.Log($"Char index is {charIndex}");
            }
            //Debug.Log($"Char index was {charIndex}, and cleaned dialogue is {cleanedDialogue.Length}");
            yield return new WaitForSeconds(lineWaitSeconds);
        }
        
        readingDialogue = false;
        curDialogue = null;
    }
}
