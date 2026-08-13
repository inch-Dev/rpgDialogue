using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    [HideInInspector] public static DialogueManager Instance;

	Dialogue dialogue;
	DialogueSpeed speed;
	float charWaitSecondsInterval = .1f;
	float lineWaitSecondsInterval = .25f;
	float startWaitTime = 0;
	float endWaitTime = 0;
	DialogueSpeaker speaker;
	DialogueExpression expression;


	[SerializeField] List<DialogueSpeed> dialogueSpeeds;

    [SerializeField] List<DialogueMarkup> markups = new List<DialogueMarkup>();

    [Tooltip("Markups to trigger custom functions to edit dialogue. Should follow TextMeshPro markup standard format")]
    
    
    float timeTillNextChar;
    bool readingDialogue = false;
    
    string rawTextSource = "";
    string logicText = "";
    string displayText = "";
    string logicDeltaText = "";
    string displayDeltaText = "";

	#region EVENTS
	public delegate void UpdateDialogue(string speakerName, DialogueExpression expression, string curText, int index);
    public static event UpdateDialogue updateDialogue;

    #endregion


	#region SETTERS
	public void SetSpeed(DialogueSpeedID ID)
	{
		foreach (DialogueSpeed ds in dialogueSpeeds)
		{
			if (ds.id == ID)
			{
				SetSpeed(ds);
			}
		}
	}
	public void SetSpeed(DialogueSpeed newSpeed)
	{
		speed = newSpeed;
		charWaitSecondsInterval = speed.charWaitSeconds;
		lineWaitSecondsInterval = speed.lineWaitSeconds;
	}


	public void SetExpression(DialogueExpressionID id)
	{
		if (speaker == null)
		{
			return;
		}
		if (speaker.getExpressionOf(id) != null)
			expression = speaker.getExpressionOf(id);
	}
	#endregion

	#region GETTERS
	public Vector2 GetDisplayIndexRange(string target)
    {
        Vector2 indexRange = Vector2.zero;
        char[] displayArray = displayText.ToCharArray();
        char[] targetArray = target.ToCharArray();

        if (target.Length > displayText.Length)
            return indexRange;

        //Iterate through display text until start of target string
        for(int i = 0; i < displayText.Length; i++)
        {
            if (i + target.Length >= displayText.Length)
                break;
            if(displayArray[i] == targetArray[0])
            {
                int indexOfEnd = displayText.IndexOf(targetArray[targetArray.Length - 1]);
                if(indexOfEnd != -1)
                {
                    string targetText = displayText.Substring(i, indexOfEnd - 1 - i);
                    if(targetText == target)
                    {
                        indexRange = new Vector2(i, indexOfEnd);
                    }
                }
            }
        }
        return indexRange;
    }

    public Vector2 GetLogicIndexRange(string target)
    {
        Vector2 indexRange = Vector2.zero;
        char[] logicArray = logicText.ToCharArray();
        char[] targetArray = target.ToCharArray();

		if (target.Length > logicText.Length)
			return indexRange;

		//Iterate through display text until start of target string
		for (int i = 0; i < logicText.Length; i++)
	    { 
			if (i + target.Length >= logicText.Length)
				break;
			if (logicArray[i] == targetArray[0])
			{
				int indexOfEnd = logicText.IndexOf(targetArray[targetArray.Length - 1]);
				if (indexOfEnd != -1)
				{
					string targetText = logicText.Substring(i, indexOfEnd - 1 - i);
					if (targetText == target)
					{
						indexRange = new Vector2(i, indexOfEnd);
					}
				}
			}
		}
		return indexRange;
	}
	#endregion

	void Start()
	{
		if (Instance == null)
			Instance = this;
		timeTillNextChar = charWaitSecondsInterval;
	}

	public void ReadDialogue(Dialogue dialogue)
    {
        if(readingDialogue)
            return;

        this.dialogue = dialogue;
        if(dialogue.hasSpeaker)
        {
            speaker = dialogue.speaker;
            expression = dialogue.startExpression;
        }
        else
        {
            speaker = null;
            expression = null;
        }
        readingDialogue = true;

        if(dialogue.hasTypewriter)
        {
            StartCoroutine(TypewriterReadDialogue(dialogue));
        }

        else
        {
            string totalText = "";
            for(int i = 0; i < dialogue.dialogueLines.Count; i++)
            {
                totalText += dialogue.dialogueLines[i] + "\n";
            }
            updateDialogue?.Invoke(dialogue.speaker.speakerName, dialogue.startExpression, RemoveMarkups(totalText), totalText.Length);
        }
    }

    void ReadDialogueLine(Dialogue dialogue, int dialogueLineIndex, string dialogueLine)
    {
        //NEEDS MARKUP TEXT TO WAIT AND CHANGE EXPRESSIONS
        if(dialogue.hasTypewriter)
            StartCoroutine(TypewriterReadDialogue(dialogue));
        else
        {
            //Eventually need function that reads for markup text 
            updateDialogue?.Invoke(dialogue.speaker.speakerName, dialogue.startExpression, dialogueLine, dialogueLine.Length);
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
            foreach(DialogueMarkup dm in markups)
            {
                if(dm.ValidateMarkup(rawText, i))
                {
                    string markupText = dm.ParseMarkup(rawText, i);
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
        if(rawText != rawTextSource)
        {
            rawTextSource = rawText;
            logicText = "";
        }

        //Get text added to display string per call
        string newAddedText;
        if(indexedRawText.Length > logicText.Length)
        {
            newAddedText = indexedRawText.Substring(logicText.Length);
        }
        else
        {
            newAddedText = "";
        }

        if(newAddedText != null && newAddedText != "")
        {   //Debug.Log($"New text displayed is {newAddedText}");
            HandleMarkups(newAddedText);
        }

        logicText = indexedRawText;
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
            foreach(DialogueMarkup dm in markups)
            {
                if (dm.ValidateMarkup(rawText, i))
                {
                    //Debug.Log($"Found markup! at {i}, with starting char:{textArray[i]}");
                    recognizedMarkup = true;
                    //Debug.Log($"Getting string.... at {i} {dm.GetMarkupString(rawText, i)}");
                    i += dm.ParseMarkup(rawText, i).Length;
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

        if(displayText  != null && rawTextSource == rawText)
        {
            int deltaLength = displayText.Length - indexedText.Length;
            deltaText = rawText.Substring(indexedText.Length - deltaLength, deltaLength);
        }
        else if(rawTextSource != rawText)
        {
            displayDeltaText = indexedText;
        }

        displayText = indexedText;
		displayDeltaText = deltaText;

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

            foreach (DialogueMarkup dm in markups)
            {
                if (dm.ValidateMarkup(rawText, i))
                {
                    recognizedMarkup = true;
                    //Debug.Log($"Recognized markup {dm} from {i}...Moving {dm.GetMarkupText(rawText, i).Length} to {i + dm.GetMarkupText(rawText, i).Length}, Length:{rawText.Length}");

                    indexedText += dm.ParseMarkup(rawText, i);
                    i += dm.ParseMarkup(rawText, i).Length;
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
        if(logicText != null && rawTextSource == rawText)
        {
            //Debug.Log($"Last indexed logic text:{lastIndexedLogicText}, length:{lastIndexedLogicText.Length}");
            int deltaLength = indexedText.Length - logicText.Length;
            //Debug.Log($"Indexed test length:{indexedText.Length}, Indexed text:{indexedText}, Delta length:{deltaLength}");
            deltaText = indexedText.Substring(indexedText.Length - deltaLength, deltaLength); //INDEXING ERROR
        }
        else if(rawTextSource != rawText)
        {
            deltaText = indexedText;
            //Debug.Log($"New dialogue line...delta text:{deltaText}");
        }

        rawTextSource = rawText;
        logicText = indexedText;
        logicDeltaText = deltaText;

        //Debug.Log($"Char index for visible chars:{index}, Indexed text:{indexedText}, Delta text:{deltaText}");
        HandleMarkups(deltaText);

		return indexedText;
	}

	void HandleMarkups(string delaText)
    {
        //Debug.Log($"Logic text:{delaText}");
        for(int i = 0; i < markups.Count; i++)
        {
            markups[i].HandleLogic(this, delaText);
        }
    }

    string RemoveMarkups(string rawText)
    {
        string handledMarkupText = rawText;
       for(int i = 0; i < markups.Count; i++)
        {
            handledMarkupText = markups[i].RemoveMarkup(handledMarkupText);
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
            foreach (DialogueMarkup dm in markups)
            {
                if (dm.ValidateMarkup(rawText, i))
                {
                    appendText += dm.ParseMarkup(rawText, i);
                    i += dm.ParseMarkup(rawText, i).Length;
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
        List<string> dialogueLines = dialogue.dialogueLines;

        for(int i = 0; i < dialogue.dialogueLines.Count; i++)
        {
            curLogicText = "";
            charIndex = 0;

            string dialogueLine = dialogueLines[i];
            string cleanedDialogue = DisplayDraftIndex(dialogueLine.Length, dialogueLine);
            //Debug.Log($"Cleaned dialogue: {cleanedDialogue}, Length:{cleanedDialogue.Length}");
            this.logicText = null; 

            while(charIndex <= cleanedDialogue.Length)
            {   
                float localCharWaitSeconds = charWaitSecondsInterval;
                float localLineWaitSeconds = lineWaitSecondsInterval;

                curLogicText = LogicDraftIndex(charIndex, dialogueLine);
                //Debug.Log($"Curtext:{curText}");

                yield return new WaitForSeconds(startWaitTime);
                updateDialogue?.Invoke(dialogue.speaker.speakerName, expression, cleanedDialogue, charIndex);

                yield return new WaitForSeconds(endWaitTime);

                startWaitTime = 0;
                endWaitTime = 0;

                yield return new WaitForSeconds(localCharWaitSeconds);
                charIndex++; 

                if(curLogicText == dialogueLine)
                {
                    Debug.Log($"Logic text equals dialogueLine...all logic ran? after {charIndex}");
                    break;
                }
            }
            yield return new WaitForSeconds(lineWaitSecondsInterval);
        }
        
        readingDialogue = false;
        this.dialogue = null;
    }
}
