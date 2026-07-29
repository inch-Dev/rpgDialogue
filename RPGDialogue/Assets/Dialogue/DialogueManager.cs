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
using JetBrains.Annotations;

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
    
    string curRawTextSource = "";
    string curLogicText = "";
    string curDisplayText = "";
    string curLogicDeltaText = "";
    string curDisplayDeltaText = "";
    
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
    public delegate void UpdateDialogue(string speakerName, DialogueExpression expression, string curText, int index);
    public static event UpdateDialogue updateDialogue;

    #endregion
    void Start()
    {
        if(Instance == null)
        Instance = this;
        timeTillNextChar = charWaitSeconds;

        ChangeSpeed(DialogueSpeedID.DEFAULT);
    }
    
    public Vector2 getDisplayIndexRange(string target)
    {
        Vector2 indexRange = Vector2.zero;
        char[] displayArray = curDisplayText.ToCharArray();
        char[] targetArray = target.ToCharArray();

        if (target.Length > curDisplayText.Length)
            return indexRange;

        //Iterate through display text until start of target string
        for(int i = 0; i < curDisplayText.Length; i++)
        {
            if (i + target.Length >= curDisplayText.Length)
                break;
            if(displayArray[i] == targetArray[0])
            {
                int indexOfEnd = curDisplayText.IndexOf(targetArray[targetArray.Length - 1]);
                if(indexOfEnd != -1)
                {
                    string targetText = curDisplayText.Substring(i, indexOfEnd - 1 - i);
                    if(targetText == target)
                    {
                        indexRange = new Vector2(i, indexOfEnd);
                    }
                }
            }
        }
        return indexRange;
    }

    public Vector2 getLogicIndexRange(string target)
    {
        Vector2 indexRange = Vector2.zero;
        char[] logicArray = curLogicText.ToCharArray();
        char[] targetArray = target.ToCharArray();

		if (target.Length > curLogicText.Length)
			return indexRange;

		//Iterate through display text until start of target string
		for (int i = 0; i < curLogicText.Length; i++)
	    { 
			if (i + target.Length >= curLogicText.Length)
				break;
			if (logicArray[i] == targetArray[0])
			{
				int indexOfEnd = curLogicText.IndexOf(targetArray[targetArray.Length - 1]);
				if (indexOfEnd != -1)
				{
					string targetText = curLogicText.Substring(i, indexOfEnd - 1 - i);
					if (targetText == target)
					{
						indexRange = new Vector2(i, indexOfEnd);
					}
				}
			}
		}
		return indexRange;
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
            updateDialogue?.Invoke(dialogue.speaker.speakerName, dialogue.startingExpression, RemoveMarkups(totalText), totalText.Length);
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
            updateDialogue?.Invoke(dialogue.speaker.speakerName, dialogue.startingExpression, dialogueLine, dialogueLine.Length);
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
        if(rawText != curRawTextSource)
        {
            curRawTextSource = rawText;
            curLogicText = "";
        }

        //Get text added to display string per call
        string newAddedText;
        if(indexedRawText.Length > curLogicText.Length)
        {
            newAddedText = indexedRawText.Substring(curLogicText.Length);
        }
        else
        {
            newAddedText = "";
        }

        if(newAddedText != null && newAddedText != "")
        {   //Debug.Log($"New text displayed is {newAddedText}");
            HandleMarkups(newAddedText);
        }

        curLogicText = indexedRawText;
        Debug.Log($"Indexed Raw Text:{indexedRawText}, index:{index}");
        return RemoveMarkups(indexedRawText);
        
    }

    string DisplayDraftIndex(int index, string rawText)
    {
        string indexedText = "";
        string deltaText = "";
        char[] textArray = rawText.ToCharArray();
        bool recognizedMarkup = false;
        bool isRichText = false;
        int i = 0;
        int visibleCharCount = 0;

        while(visibleCharCount <= index)
        {
            if (i >= textArray.Length)
            {
                return indexedText;
            }

            recognizedMarkup = false;
            isRichText = false;
            foreach(DialogueMarkup dm in dialogueMarkups)
            {
                if(dm.ValidateMarkup(rawText, i))
                {
                    recognizedMarkup = true;
                    //Debug.Log($"Recognized markup {dm} from {i}");
                    i += dm.GetMarkupText(rawText, i).Length;
                }
                else if(textArray[i] == '<')
                {
					if (i + 1 < textArray.Length)
					{
						int indexOfEnd = rawText.IndexOf('>', i + 1);
						if (indexOfEnd != -1)
						{
							string richText = rawText.Substring(i, (indexOfEnd - i) + 1);
							//Debug.Log($"Found richText:{richText}, Length:{richText.Length}");
							i += richText.Length;
							indexedText += richText;
							isRichText = true;
						}
					}
				}
                if (recognizedMarkup || isRichText)
                    break;
            }
            if (recognizedMarkup || isRichText)
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

        if(curDisplayText  != null && curRawTextSource == rawText)
        {
            int deltaLength = curDisplayText.Length - indexedText.Length;
            deltaText = rawText.Substring(indexedText.Length - deltaLength, deltaLength);
        }
        else if(curRawTextSource != rawText)
        {
            curDisplayDeltaText = indexedText;
        }

        curDisplayText = indexedText;
		curDisplayDeltaText = deltaText;

		return indexedText;
    }

    string LogicDraftIndex(int index, string rawText)
    {
        //Substring of raw text with number of visible characters up to length of index
        string indexedText = "";

        //Text that has been added since last call
        string deltaText = "";


        char[] textArray = rawText.ToCharArray();
        bool recognizedMarkup = false;
        bool isRichText = false;
        int i = 0;
        int visibleCharCount = 0;

        while (visibleCharCount <= index)
        {
            if (i >= textArray.Length)
            {
                break;
            }

            recognizedMarkup = false;
            isRichText = false;

            foreach (DialogueMarkup dm in dialogueMarkups)
            {
                if (dm.ValidateMarkup(rawText, i))
                {
                    recognizedMarkup = true;
                    //Debug.Log($"Recognized markup {dm} from {i}...Moving {dm.GetMarkupText(rawText, i).Length} to {i + dm.GetMarkupText(rawText, i).Length}, Length:{rawText.Length}");

                    indexedText += dm.GetMarkupText(rawText, i);
                    i += dm.GetMarkupText(rawText, i).Length;
                    //Debug.Log($"Added to index...{indexedText}");
                }
                else if (textArray[i] == '<')
                {
                    if (i + 1 < textArray.Length)
                    {
                        int indexOfEnd = rawText.IndexOf('>', i + 1);
                        if (indexOfEnd != -1)
                        {
                            string richText = rawText.Substring(i, (indexOfEnd - i) + 1);
                            //Debug.Log($"Found richText:{richText}, Length:{richText.Length}");
                            i += richText.Length;
                            indexedText += richText;
                            isRichText = true;
                        }
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

        //Debug.Log($"Indexed text:{indexedText}, Length:{indexedText.Length}");
        indexedText = AppendMarkups(rawText, indexedText.Length);
        //Debug.Log($"Text after append:{indexedText}");
        

        //Setting change in deltaLogicText
        if(curLogicText != null && curRawTextSource == rawText)
        {
            //Debug.Log($"Last indexed logic text:{lastIndexedLogicText}, length:{lastIndexedLogicText.Length}");
            int deltaLength = indexedText.Length - curLogicText.Length;
            //Debug.Log($"Indexed test length:{indexedText.Length}, Indexed text:{indexedText}, Delta length:{deltaLength}");
            deltaText = indexedText.Substring(indexedText.Length - deltaLength, deltaLength); //INDEXING ERROR
        }
        else if(curRawTextSource != rawText)
        {
            deltaText = indexedText;
            //Debug.Log($"New dialogue line...delta text:{deltaText}");
        }

        curRawTextSource = rawText;
        curLogicText = indexedText;
        curLogicDeltaText = deltaText;

        //Debug.Log($"Char index for visible chars:{index}, Indexed text:{indexedText}, Delta text:{deltaText}");
        HandleMarkups(deltaText);

		return indexedText;
	}

	void HandleMarkups(string delaText)
    {
        //Debug.Log($"Logic text:{delaText}");
        for(int i = 0; i < dialogueMarkups.Count; i++)
        {
            dialogueMarkups[i].HandleMarkup(this, delaText);
        }
    }

    string RemoveMarkups(string rawText)
    {
        string handledMarkupText = rawText;
       for(int i = 0; i < dialogueMarkups.Count; i++)
        {
            handledMarkupText = dialogueMarkups[i].RemoveMarkupText(handledMarkupText);
        }

        return handledMarkupText;
    }

    string AppendMarkups(string rawText, int startIndex)
    {
        char[] textArray = rawText.ToCharArray();
        string appendText = rawText.Substring(0, startIndex);
        //Debug.Log($"Starting apending text...{appendText}");

        bool isMarkup = false;
        bool isRichText = false;

        for(int i = startIndex; i < textArray.Length; i++)
        {
            isMarkup = false;
            isRichText = false;
            foreach (DialogueMarkup dm in dialogueMarkups)
            {
                if (dm.ValidateMarkup(rawText, i))
                {
                    appendText += dm.GetMarkupText(rawText, i);
                    i += dm.GetMarkupText(rawText, i).Length;
                    isMarkup = true;
                }
                else if (textArray[i] == '<' && i + 1 < rawText.Length)
                {
                    int indexOfEnd = rawText.IndexOf('>', i + 1);
                    if (indexOfEnd != -1)
                    {
                        string richText = rawText.Substring(i, (indexOfEnd - i) + 1);
                        Debug.Log($"Found rich text of Length:{richText.Length} at {i}...moving to index {i + richText.Length}");
                        appendText += richText;
                        i += richText.Length;
                        isRichText = true;
                    }
                }
                if (isMarkup || isRichText)
                    break;
            }
            if (isMarkup || isRichText)
                continue;

            if(textArray[i] == ' ')
            {
                appendText += textArray[i].ToString();
            }
            else
            {
                return appendText;
            }
        }
        //Debug.Log($"Ending append text{appendText}");

        return appendText;
    }
    IEnumerator TypewriterReadDialogue(Dialogue dialogue) 
    {
        int charIndex = 0;
        string curLogicText = "";
        string[] dialogueLines = dialogue.dialogueLines;

        for(int i = 0; i < dialogue.dialogueLines.Length; i++)
        {
            curLogicText = "";
            charIndex = 0;

            string dialogueLine = dialogueLines[i];
            string cleanedDialogue = DisplayDraftIndex(dialogueLine.Length, dialogueLine);
            //Debug.Log($"Cleaned dialogue: {cleanedDialogue}, Length:{cleanedDialogue.Length}");
            this.curLogicText = null; 

            while(charIndex <= cleanedDialogue.Length)
            {   
                float localCharWaitSeconds = charWaitSeconds;
                float localLineWaitSeconds = lineWaitSeconds;

                curLogicText = LogicDraftIndex(charIndex, dialogueLine);
                //Debug.Log($"Curtext:{curText}");

                yield return new WaitForSeconds(curStartWaitTime);
                updateDialogue?.Invoke(dialogue.speaker.speakerName, curExpression, cleanedDialogue, charIndex);

                yield return new WaitForSeconds(curEndWaitTime);

                curStartWaitTime = 0;
                curEndWaitTime = 0;

                yield return new WaitForSeconds(localCharWaitSeconds);
                charIndex++; 

                if(curLogicText == dialogueLine)
                {
                    Debug.Log($"Logic text equals dialogueLine...all logic ran? after {charIndex}");
                    break;
                }
            }
            yield return new WaitForSeconds(lineWaitSeconds);
        }
        
        readingDialogue = false;
        curDialogue = null;
    }
}
