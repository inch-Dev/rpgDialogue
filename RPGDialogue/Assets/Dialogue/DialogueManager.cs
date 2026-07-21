using System.Collections;
using NUnit.Framework;
using UnityEditor.AdaptivePerformance.Editor;
using UnityEditor.EditorTools;
using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using Unity.VisualScripting;
using UnityEditor.U2D.Animation;
using TMPro;

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
        Debug.Log($"Raw text:{rawText}");
        //Get all text needed to process till certain index
                                                                                                                      
         string indexedRawText = "";
        int visibleCharCount = 0;
        char[] textArray = rawText.ToCharArray();

        for(int i = 0; visibleCharCount < index; i++)
        {
            //If markup recognized 
            foreach(DialogueMarkup dm in dialogueMarkups)
            {
                if(dm.ValidateMarkup(rawText, i))
                {
                    string markupText = dm.GetMarkupText(rawText, i);
                    indexedRawText += markupText;
                    i += markupText.Length;
                    Debug.Log($"Found markup of {dm}. Moving to index {i}");
                    continue;
                }
            }

            if(textArray[i] == ' ')
            {
                indexedRawText += textArray[i].ToString();
            }

            else if(visibleCharCount < index)
            {
                indexedRawText += textArray[i].ToString();
                visibleCharCount++;
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
        if(indexedRawText.Length > lastDisplayedDialogueText.Length)
        {
            newAddedText = indexedRawText.Substring(lastDisplayedDialogueText.Length);
        }
        else
        {
            newAddedText = "";
        }

        if(newAddedText != null && newAddedText != "")
        {   //Debug.Log($"New text displayed is {newAddedText}");
            HandleMarkupLogic(newAddedText);
        }

        lastDisplayedDialogueText = indexedRawText;
        Debug.Log($"Indexed Raw Text:{indexedRawText}, index:{index}");
        return RemoveMarkupText(indexedRawText);
        
    }

    string IndexRawText(int index, string rawText)
    {
        //Get raw text and index to run logic
    
        string indexedRawText = "";
        int visibleCharacterCount = 0;
        char[] textArray = rawText.ToCharArray();

        for(int i = 0; visibleCharacterCount < index; i++)
        {
            //If markup recognized 
            foreach(DialogueMarkup dm in dialogueMarkups)
            {
                if(dm.ValidateMarkup(rawText, i))
                {
                    string markupText = dm.GetMarkupText(rawText, i);
                    indexedRawText += markupText;
                    i += markupText.Length;
                    continue;
                }

                else if(textArray[i] == ' ')
                {
                    indexedRawText += textArray[i].ToString();
                }

                else if(visibleCharacterCount < index)
                {
                    indexedRawText += textArray[i].ToString();
                    visibleCharacterCount++;
                }


            }
        }
        //Append any remaining markups if possible

        //Pass on to run logic for markups
        return indexedRawText;
    }

    string IndexDisplayText(int index, string rawText) //Display text values to index
    {
        int charIndex = index;
        if(index >= rawText.Length)
            charIndex = rawText.Length;

        string indexedDisplayText = "";
        char[] textArray = rawText.ToCharArray();
        int visibleCharCount = 0;
        
        for(int i = 0; i < rawText.Length; i++)
        {
            foreach(DialogueMarkup dm in dialogueMarkups)
            {
                if(dm.ValidateMarkup(rawText, i))
                {
                    Debug.Log($"Recognized {dm}");
                    string markupText = dm.GetMarkupText(rawText, i);
                    int debugI = i;
                    i += markupText.Length;
                    Debug.Log($"Moving {markupText.Length} spaces from {debugI}...Index now at {i}");
                    continue;
                }
            }

            if(textArray[i] == ' ')
            {
                indexedDisplayText += textArray[i].ToString();
            }

            else
            {
                indexedDisplayText += textArray[i].ToString();
                visibleCharCount++;
            }
            
            if(visibleCharCount >= index)
            {
                break;
            }
        }

        return indexedDisplayText;
    }

    string DisplayDraftIndex(int index, string rawText)
    {
        string indexedText = "";
        char[] textArray = rawText.ToCharArray();
        bool recognizedMarkup = false;
        bool isRichText = false;
        int i = 0;
        int visibleCharCount = 0;

        while(visibleCharCount < index + 1)
        {
            if (i >= textArray.Length)
            {
                return indexedText;
            }
            recognizedMarkup = false;
            foreach(DialogueMarkup dm in dialogueMarkups)
            {
                if(dm.ValidateMarkup(rawText, i))
                {
                    recognizedMarkup = true;
                    Debug.Log($"Recognized markup {dm} from {i}");
                    i += dm.GetMarkupText(rawText, i).Length;
                }
                if (recognizedMarkup)
                    break;
            }
            if (recognizedMarkup)
                continue;

            if(textArray[i] == ' ')
            {
                indexedText += textArray[i].ToString();
            }

            else
            {
                indexedText += textArray[i].ToString();
                visibleCharCount++;
            }

            i++;
        }
        return indexedText;
    }

	string LogicDraftIndex(int index, string rawText)
	{
		string indexedText = "";
		char[] textArray = rawText.ToCharArray();
		bool recognizedMarkup = false;
		bool isRichText = false;
		int i = 0;
		int visibleCharCount = 0;

		while (visibleCharCount < index + 1)
		{
			if (i >= textArray.Length)
			{
				return indexedText;
			}

			recognizedMarkup = false;
            isRichText = false;
            
            foreach (DialogueMarkup dm in dialogueMarkups)
            {
                if (dm.ValidateMarkup(rawText, i))
                {
                    recognizedMarkup = true;
                    Debug.Log($"Recognized markup {dm} from {i}");
                    i += dm.GetMarkupText(rawText, i).Length;
                    indexedText += dm.GetMarkupText(rawText, i);
                }
                else if (textArray[i] == '<')
                {
                    if (i + 1 < textArray.Length)
                    {
                        int indexOfEnd = rawText.IndexOf('>', i + 1);
                        string richText = rawText.Substring(i, (indexOfEnd - i) + 1);
                        
						Debug.Log($"Found richText:{richText}, Length:{richText.Length}");
                        i += richText.Length;
                        isRichText = true;
					}

                }
                if (recognizedMarkup || isRichText)
					break;
			}
			if (recognizedMarkup || isRichText)
				continue;

			if (textArray[i] == ' ')
			{
				indexedText += textArray[i].ToString();
			}

			else
			{
				indexedText += textArray[i].ToString();
				visibleCharCount++;
			}

			i++;
		}

        //Append remaining markups
        Debug.Log($"Indexed:{indexedText}");
		return indexedText;
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
            string cleanedDialogue = DisplayDraftIndex(dialogueLine.Length, dialogueLine);
            Debug.Log($"Cleaned dialogue: {cleanedDialogue}");
            lastDisplayedDialogueText = null; 

            while(charIndex <= cleanedDialogue.Length)
            {   
                float localCharWaitSeconds = charWaitSeconds;
                float localLineWaitSeconds = lineWaitSeconds;

                curText = dialogueLine.Substring(0, charIndex);
                curText = LogicDraftIndex(charIndex, dialogueLine); //TEST IF THIS WORKS!!!!!!!!!!!!!!!!!!!!!!!!

                yield return new WaitForSeconds(curStartWaitTime); 

                updateDialogue?.Invoke(dialogue.speaker.speakerName, curExpression, curText);

                yield return new WaitForSeconds(curEndWaitTime);

                curStartWaitTime = 0;
                curEndWaitTime = 0;

                yield return new WaitForSeconds(localCharWaitSeconds);
                charIndex++; 
            }
            yield return new WaitForSeconds(lineWaitSeconds);
        }
        
        readingDialogue = false;
        curDialogue = null;
    }
}
