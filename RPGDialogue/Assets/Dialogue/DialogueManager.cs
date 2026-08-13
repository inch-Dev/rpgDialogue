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
    string markupIndexText = "";
    string displayIndexText = "";
    string markupDeltaText = "";
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
        char[] displayArray = displayIndexText.ToCharArray();
        char[] targetArray = target.ToCharArray();

        if (target.Length > displayIndexText.Length)
            return indexRange;

        //Iterate through display text until start of target string
        for(int i = 0; i < displayIndexText.Length; i++)
        {
            if (i + target.Length >= displayIndexText.Length)
                break;
            if(displayArray[i] == targetArray[0])
            {
                int indexOfEnd = displayIndexText.IndexOf(targetArray[targetArray.Length - 1]);
                if(indexOfEnd != -1)
                {
                    string targetText = displayIndexText.Substring(i, indexOfEnd - 1 - i);
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
        char[] logicArray = markupIndexText.ToCharArray();
        char[] targetArray = target.ToCharArray();

		if (target.Length > markupIndexText.Length)
			return indexRange;

		//Iterate through display text until start of target string
		for (int i = 0; i < markupIndexText.Length; i++)
	    { 
			if (i + target.Length >= markupIndexText.Length)
				break;
			if (logicArray[i] == targetArray[0])
			{
				int indexOfEnd = markupIndexText.IndexOf(targetArray[targetArray.Length - 1]);
				if (indexOfEnd != -1)
				{
					string targetText = markupIndexText.Substring(i, indexOfEnd - 1 - i);
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
            Handle(totalText, MarkupType.DISPLAY);
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

    /// <summary>
    /// Gets and stores text for display up to index in rawText
    /// </summary>
    /// <param name="index"></param>
    /// <param name="rawText"></param>
    /// <returns></returns>
    string DisplayIndex(int index, string rawText)
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
                    recognizedMarkup = true;
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

        if(displayIndexText  != null && rawTextSource == rawText)
        {
            int deltaLength = displayIndexText.Length - indexedText.Length;
            deltaText = rawText.Substring(indexedText.Length - deltaLength, deltaLength);
        }
        else if(rawTextSource != rawText)
        {
            displayDeltaText = indexedText;
        }

        displayIndexText = indexedText;
		displayDeltaText = deltaText;

		return indexedText;
    }


    /// <summary>
    /// Gets and stores text for running markup functions up to index in rawText
    /// </summary>
    /// <param name="index"></param>
    /// <param name="rawText"></param>
    /// <returns></returns>
    string MarkupIndex(int index, string rawText)
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

                    indexedText += dm.ParseMarkup(rawText, i);
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

        indexedText = AppendMarkups(rawText, indexedText.Length);
        

        //Setting change in deltaLogicText
        if(markupIndexText != null && rawTextSource == rawText)
        {
            int deltaLength = indexedText.Length - markupIndexText.Length;
            deltaText = indexedText.Substring(indexedText.Length - deltaLength, deltaLength);
        }
        else if(rawTextSource != rawText)
        {
            deltaText = indexedText;
        }

        rawTextSource = rawText;
        markupIndexText = indexedText;
        markupDeltaText = deltaText;

		return indexedText;
	}

	
    void Handle(string fullText, MarkupType type)
    {
        for(int i = 0; i < markups.Count; i++)
        {
            if (markups[i].GetMarkupType().Equals(type))
                markups[i].Handle(this, fullText);
        }
    }

    void HandleDelta(string deltaText, MarkupType type)
    {
        for(int i = 0; i < markups.Count; i++)
        {
            if (markups[i].GetMarkupType().Equals(type))
                markups[i].HandleDelta(this, deltaText);
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
            string displayDialogue = DisplayIndex(dialogueLine.Length, dialogueLine);

            this.markupIndexText = null; 

            while(charIndex <= displayDialogue.Length)
            {   
                float localCharWaitSeconds = charWaitSecondsInterval;
                float localLineWaitSeconds = lineWaitSecondsInterval;

                curLogicText = MarkupIndex(charIndex, dialogueLine);

                HandleDelta(markupDeltaText, MarkupType.LOGIC);
                HandleDelta(markupDeltaText, MarkupType.DISPLAY);

                yield return new WaitForSeconds(startWaitTime);
                updateDialogue?.Invoke(dialogue.speaker.speakerName, expression, displayDialogue, charIndex);

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
