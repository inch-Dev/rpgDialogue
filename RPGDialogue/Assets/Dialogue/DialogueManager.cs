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

    [SerializeField] List<DialogueMarkup> customMarkups = new List<DialogueMarkup>();
    [Tooltip("Markups to trigger custom functions to edit dialogue. Should follow TextMeshPro markup standard format")]
    Dialogue curDialogue; 
    float timeTillNextChar;
    bool readingDialogue = false;
    string lastDisplayedDialogue = "";
    float curWaitTime = 0;
    DialogueExpression curExpression;
    DialogueSpeaker curSpeaker;

    //Need some sort of markup handle event
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

    // Update is called once per frame
    void Update()
    {

    }

    public void ReadDialogue(Dialogue dialogue)
    {
        if(readingDialogue)
            return;

        curDialogue = dialogue;
        curSpeaker = dialogue.speaker;
        curExpression = dialogue.startingExpression;
        readingDialogue = true; 
        ReadDialogueLine(dialogue, 0, dialogue.dialogueLines[0]);
    }

    void ReadDialogueLine(Dialogue dialogue, int dialogueLineIndex, string dialogueLine)
    {
        //Read all words in each dialog line then clear to next line
        //PER WORD text read timer and update ui

        //NEEDS MARKUP TEXT TO WAIT AND CHANGE EXPRESSIONS
        if(dialogue.hasTypeWriterEffect)
            StartCoroutine(TypewriterReadDialogueLine(dialogue));
        else
        {
            //Eventually need function that reads for markup text 
            updateDialogue?.Invoke(dialogue.speaker.speakerName, dialogue.startingExpression, dialogueLine);
        }
    }

    string CleanMarkupText (int index, string dialogueString) //Clean everything in brackets until the entire tag is included //Need index to cutoff
    {
        Debug.Log("Reading from {dialogueLine}");
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
                    fullText = AppendEndingMarkupText(fullText, textArray, i);
                    break;
                }
            }
            if(textArray[i] == '>')
                isReadingTag = false;
        }

        string newAddedText;
        if(fullText.Length > lastDisplayedDialogue.Length)
        newAddedText = fullText.Substring(lastDisplayedDialogue.Length, fullText.Length - lastDisplayedDialogue.Length);
        else
        newAddedText = fullText.Substring(0, fullText.Length); //Starting new line of dialogue

        lastDisplayedDialogue = fullText;
        return fullText;

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

    //Change to read ALL dialogue
    IEnumerator TypewriterReadDialogueLine(Dialogue dialogue) 
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
            float textSpeed = defaultCharWaitMult;

            while(curText.Length < dialogueLine.Length) 
            {
                
                t += Time.deltaTime * textSpeed; //Adjust value based on events
                yield return new WaitForSeconds(curWaitTime); //If wait event called add time between characters;

                charIndex = Mathf.FloorToInt(t);
                charIndex = Mathf.Clamp(charIndex, 0, dialogueLine.Length);
                
                
                curText = dialogueLine.Substring(0, charIndex);
                

                //Need to update expression somehow
                updateDialogue?.Invoke(dialogue.speaker.speakerName, curExpression, CleanMarkupText(charIndex, dialogueLine));
                curText = CleanMarkupText(charIndex, dialogueLine);

                //Debug.Log($"CurText:{curText.Length} DialogueLine:{dialogueLine.Length}");
                Debug.Log($"index is {charIndex}, typewrite |{curText}| dialogueLine is :{dialogueLine}");
                yield return null;
            }
            yield return new WaitForSeconds(defaultTimeBetweenLines);
        }
        
        readingDialogue = false;
        curDialogue = null;
    }
}
