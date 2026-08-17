using System;
using UnityEngine;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using NaughtyAttributes.Editor;
using UnityEditor;

public enum MarkupType
{
    LOGIC,
    DISPLAY,
    NUM_TYPES
}

public enum FormatType
{
    INVALID = -1,
    OPEN,
    CLOSE,
    NUM_FORMATS
}

[Serializable]
public class DialogueMarkupFormat
{
    [SerializeField] public FormatType type;
    [SerializeField] public string tagStart;
    [SerializeField] public string tagEnd;

	public DialogueMarkupFormat(FormatType theType, string theStart, string theEnd)
	{
		type = theType;
		tagStart = theStart;
		tagEnd = theEnd;
	}
}

[CreateAssetMenu(fileName = "DialogueMarkup", menuName = "ScriptableObjects/DialogueObjects/DialogueMarkups/DialogueMarkup", order = 1)]
public class DialogueMarkup : ScriptableObject
{
    [SerializeField] public MarkupType markupType;
    [SerializeField] public string keyName;
	[SerializeField] List<MarkupData> markupDatas;
    bool isActiveApplying = false;
    MarkupData applyingData = null;
    string dialogueSource = null;
	protected static DialogueMarkupFormat openFormat = new DialogueMarkupFormat(FormatType.OPEN, "{", "}");
    protected static DialogueMarkupFormat closeFormat = new DialogueMarkupFormat(FormatType.CLOSE, "{/", "}");
    protected static DialogueMarkupFormat[] formats = new DialogueMarkupFormat[2] { openFormat, closeFormat };
    protected char equals = '=';

    #region GETTERS

    virtual public MarkupType GetMarkupType(){ return markupType; }
    virtual public string GetMarkup()
    {
        string markup;

        string openMarkup = GetMarkup(openFormat);

        string closeMarkup = GetMarkup(closeFormat);

        markup = openMarkup + closeMarkup;

        return markup;
    }

    virtual public string GetMarkup(MarkupData markupData)
    {
        string markup;

        string openMarkup = GetMarkup(markupData, openFormat);

        string closeMarkup = GetMarkup(markupData, closeFormat);

        markup = openMarkup + closeMarkup;

        return markup;
    }

    virtual public string GetMarkup(DialogueMarkupFormat format)
    {
        string markup;

        markup = format.tagStart + keyName + " " + equals + " " + format.tagEnd;

        return markup;
    }

    virtual public string GetMarkup(MarkupData markupData, DialogueMarkupFormat format)
    {
        string markup;

        markup = format.tagStart + keyName + " " + equals + " " + markupData.keyName + format.tagEnd;

        return markup;
    }

    virtual public List<MarkupData> GetMarkupDatas(){ return markupDatas; }

	/// <summary>
	/// Get markupData in markup
	/// </summary>
	/// <param name="markup"></param>
	/// <returns></returns>

	#endregion

	#region SETTERS

    virtual public void SetDialogueSource(string newSource){  dialogueSource = newSource; }

    virtual public void SetActiveLastDelta(bool newActive){ isActiveApplying = newActive; }

    #endregion

	#region PARSERS

	/// <summary>
	/// Get text this markup applies to
	/// </summary>
	/// 
	virtual public string ParseAppliedText(string rawText, DialogueCall callType)
    {
        switch(callType)
        {
            case DialogueCall.DELTA:
                return ParseAppliedDeltaText(rawText);
            case DialogueCall.FULL:
                return ParseAppliedText(rawText);
        }

        return null;
    }
    virtual public string ParseAppliedText(string rawText)
    {
        string appliedText = "";
        char[] textArray = rawText.ToCharArray();
        bool isAppliedText = false;

        if (!ValidateMarkup(ParseMarkup(rawText)))
            return null;

        for (int i = 0; i < rawText.Length; i++)
        {
            if (ParseMarkup(rawText, i, openFormat) != null)
            {
                i += ParseMarkup(rawText, i, openFormat).Length;
                isAppliedText = true;
            }

            if (ParseMarkup(rawText, i, closeFormat) != null)
            {
                isAppliedText = false;
            }

            if (i < rawText.Length && isAppliedText)
            {
                appliedText += textArray[i];
            }
        }

        Debug.Log($"Applied text in string:{rawText} is:{appliedText}");

        return appliedText;
    }

    virtual public string ParseAppliedDeltaText(string deltaText)
    {
        Debug.Log($"Parsing in {deltaText}");
        string appliedText = "";
        char[] textArray = deltaText.ToCharArray();
        bool isAppliedText = false;

        if (isActiveApplying)
        {
            isAppliedText = true;
        }

        for (int i = 0; i < deltaText.Length; i++)
        {
            if (ParseMarkup(deltaText, i, openFormat) != null)
            {
                i += ParseMarkup(deltaText, i, openFormat).Length;
                isAppliedText = true;
            }

            if(ParseMarkup(deltaText, i, closeFormat) != null)
            {
                isAppliedText = false;
            }

            if( i < deltaText.Length && isAppliedText)
            {
                appliedText += textArray[i];
            }
        }

        return appliedText;
    }

    virtual public List<string> ParseAppliedText(string rawText, DialogueCall callType)
    {
        switch (callType)
        {
            case DialogueCall.DELTA:
                return ParseDeltaAppliedText(rawText);
            case DialogueCall.FULL:
                return ParseFullAppliedText(rawText);
            default:
                return null;
        }

    }

    virtual public List<string> ParseFullAppliedText(string rawText)
    {
        List<string> appliedText = new List<string>();
        List<Vector2Int> indexRanges = ParseAppliedIndexRanges(rawText, DialogueCall.FULL);

        foreach(Vector2Int range in indexRanges)
        {
            string textRange = rawText.Substring(range.x, range.y - range.x);
            appliedText.Add(textRange);
        }

        return appliedText;
    }

    virtual public List<string> ParseDeltaAppliedText(string deltaText)
    {
        List<string> appliedText = new List<string>;
        List<Vector2Int> indexRanges = ParseAppliedIndexRanges(deltaText, DialogueCall.DELTA);

        foreach (Vector2Int range in indexRanges)
        {
            string textRange = deltaText.Substring(range.x, range.y - range.x);
            appliedText.Add(textRange);
        }

        return appliedText;
    }


    #region INDEX RANGES

    virtual public List<Vector2Int> ParseIndexRanges(string rawText, DialogueCall callType)
    {
        switch (callType)
        {
            case DialogueCall.FULL:
                return ParseIndexRanges(rawText);
            case DialogueCall.DELTA:
                return ParseDeltaIndexRanges(rawText);
            default:
                return null;
        }
            
    }

    virtual public List<Vector2Int> ParseFullIndexRanges(string rawText)
    {
        List<Vector2Int> indexRanges = new List<Vector2Int>();
        char[] textArray = rawText.ToCharArray();

        Vector2Int currentRange = new Vector2Int(-1, -1);

        for(int i = 0; i < rawText.Length; i++)
        {
            if(ValidateMarkup(ParseMarkup(rawText, i)))
            {
                switch(ParseFormatType(ParseMarkup(rawText, i)))
                {
                    case FormatType.OPEN:
                        currentRange.x = i;
                        break;
                    case FormatType.CLOSE:
                        currentRange.y = i;
                        indexRanges.Add(currentRange);
                        break;
                }
            }
        }
        return indexRanges;
    }

    virtual public List<Vector2Int> ParseDeltaIndexRanges(string deltaText)
    {
        List<Vector2Int> indexRanges = new List<Vector2Int>();
        char[] textArray = deltaText.ToCharArray();

        Vector2Int currentRange = new Vector2Int(-1, -1);

        if(isActiveApplying)
        {
            currentRange.x = 0;
        }

        for(int i = 0; i < deltaText.Length;i++)
        {
            if (ValidateMarkup(ParseMarkup(deltaText, i)))
            {
                switch(ParseFormatType(ParseMarkup(deltaText, i)))
                {
                    case FormatType.OPEN:
                        currentRange.x = i;
                        break;
                    case FormatType.CLOSE:
                        currentRange.y = i;
                        indexRanges.Add(currentRange);
                        break;

                }
            }

        }

        return indexRanges;
    }

    /// <summary>
    /// Get index range of this markup in rawText
    /// </summary>
    /// <param name="rawText"></param>
    /// <returns></returns>
    /// 


    //REPLACE WITH INDEX RANGES
	virtual public Vector2Int ParseIndexRange(string rawText)
	{
		Vector2Int indexRange = new Vector2Int(-1, -1);
		char[] textArray = rawText.ToCharArray();

		if (!ValidateMarkup(ParseMarkup(rawText)))
		{
			return indexRange;
		}

		for (int i = 0; i < rawText.Length; i++)
		{
			if (ParseMarkup(rawText, i) != null)
			{
				indexRange.x = i;
				indexRange.y = ParseMarkup(rawText, i).Length + i;
			}
		}

		return indexRange;
	}

    /// <summary>
    /// Get index range of the text this markup applies to in rawText
    /// </summary>
    /// <param name="rawText"></param>
    /// <returns></returns>
    virtual public Vector2Int ParseAppliedIndexRange(string rawText)
    {
        //EDIT THIS
        Vector2Int indexRange = new Vector2Int(-1, -1);
        char[] textArray = rawText.ToCharArray();

        for(int i = 0; i < rawText.Length; i++)
        {
            if(ParseMarkup(rawText, i, openFormat) != null)
            {
                indexRange.x = i + ParseMarkup(rawText, i, openFormat).Length;
            }

            if(ParseMarkup(rawText, i, closeFormat) != null)
            {
                indexRange.y = i - 1;
            }
        }
        return indexRange;
    }

    virtual public List<Vector2Int> ParseAppliedIndexRanges(string rawText, DialogueCall callType)
    {
        switch (callType)
        {
            case DialogueCall.FULL:
                return ParseAppliedIndexRanges(rawText);
            case DialogueCall.DELTA:
                return ParseDeltaAppliedIndexRanges(rawText);
            default:
                return null;
        }

    }

    virtual public List<Vector2Int> ParseFullAppliedIndexRanges(string rawText)
    {
        List<Vector2Int> indexRanges = new List<Vector2Int>();
        char[] textArray = rawText.ToCharArray();

        Vector2Int currentRange = new Vector2Int(-1, -1);

        for(int i = 0; i < rawText.Length; i++)
        {
            if(ValidateMarkup(ParseMarkup(rawText, i)))
            {
                switch(ParseFormatType(ParseMarkup(rawText, i)))
                {
                    case FormatType.OPEN:
                        currentRange.x = i += ParseMarkup(rawText, i).Length;
                        break;
                    case FormatType.CLOSE:
                        currentRange.y = i - 1;
                        indexRanges.Add(currentRange);
                        break;
                }
            }
        }

        return indexRanges;
    }

    virtual public List<Vector2Int> ParseDeltaAppliedIndexRanges(string rawText)
    {
        List<Vector2Int> indexRanges = new List<Vector2Int>();
        char[] textArray = rawText.ToCharArray();

        Vector2Int currentRange = new Vector2Int(-1, -1);

        if(isActiveApplying)
        {
            currentRange.x = 0;
        }

        for(int i = 0; i < rawText.Length; i++)
        {
            if( ValidateMarkup(ParseMarkup(rawText,i)))
            {
                switch (ParseFormatType(ParseMarkup(rawText, i)))
                {
                    case FormatType.OPEN:
                        currentRange.x = i += ParseMarkup(rawText, i).Length;
                        break;
                    case FormatType.CLOSE:
                        currentRange.y = i - 1;
                        indexRanges.Add(currentRange);
                        break;
                }
                    
            }
        }

        return indexRanges;
    }
    #endregion

    /// <summary>
    /// Get format type of markup
    /// </summary>
    /// <param name="markup"></param>
    /// <returns></returns>
    virtual public FormatType ParseFormatType(string markup)
	{
		//Debug.Log($"Getting markup text:{markup}");
		FormatType theType = FormatType.INVALID;

		foreach (DialogueMarkupFormat format in formats)
		{
			if (ValidateFormat(markup, format))
				return format.type;
		}

		return theType;
	}

	/// <summary>
	/// Get format of markup
	/// </summary>
	/// <param name="markup"></param>
	/// <returns></returns>
	virtual public DialogueMarkupFormat ParseFormat(string markup)
	{
		DialogueMarkupFormat theFormat = null;

		char[] textArray = markup.ToCharArray();

		foreach (DialogueMarkupFormat format in formats)
		{
			if (markup.Length < format.tagStart.Length + format.tagEnd.Length)
				continue;

			if (markup.Substring(0, format.tagStart.Length) == format.tagStart
			&& markup.Substring(markup.Length - format.tagEnd.Length, format.tagEnd.Length) == format.tagEnd)
				return format;
		}

		return theFormat;
	}

	/// <summary>
	/// Get markupData in markup
	/// </summary>
	/// <param name="markup"></param>
	/// <returns></returns>
	virtual public MarkupData ParseMarkupData(string markup)
	{
		foreach (DialogueMarkupFormat format in formats)
		{
			MarkupData theMarkupData = ParseMarkupData(markup, format);
			if (theMarkupData)
				return theMarkupData;

		}
		return null;
	}

	/// <summary>
	/// Get markupData in markup of this format
	/// </summary>
	/// <param name="markup"></param>
	/// <param name="format"></param>
	/// <returns></returns>
	virtual public MarkupData ParseMarkupData(string markup, DialogueMarkupFormat format)
	{
		foreach (MarkupData markupData in markupDatas)
		{
			if (ValidateMarkupData(markup, markupData, format))
				return markupData;
		}

		return null;
	}

	/// <summary>
	/// Gets first valid markup in rawText
	/// </summary>
	virtual public string ParseMarkup(string rawText)
	{
		for (int i = 0; i < rawText.Length; i++)
		{
			if (ParseMarkup(rawText, i) != null)
				return ParseMarkup(rawText, i);
		}
		return null;
	}

	virtual public string ParseMarkup(string rawText, FormatType type)
	{
		switch (type)
		{
			case FormatType.OPEN:
                return ParseMarkup(rawText, openFormat);
			case FormatType.CLOSE:
                return ParseMarkup(rawText, closeFormat);
		}
        return null;
	}

	virtual public string ParseMarkup(string rawText, DialogueMarkupFormat format)
    {
		for (int i = 0; i < rawText.Length; i++)
		{
			if (ParseMarkup(rawText, i) != null)
				return ParseMarkup(rawText, i, format);
		}
		return null;
	}

	/// <summary>
	/// Gets valid markup in rawText at starting index
	/// </summary>
	virtual public string ParseMarkup(string rawText, int startIndex)
	{
		//Check all formats for first valid instance

		foreach (DialogueMarkupFormat format in formats)
		{
			string markupString = ParseMarkup(rawText, startIndex, format);
			if (markupString != null)
			{
				return markupString;
			}
		}

		return null;
	}

    /// <summary>
    /// Gets valid markup of this format type in rawText at starting index 
    /// </summary>
    /// <param name="rawText"></param>
    /// <param name="startIndex"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    virtual public string ParseMarkup(string rawText, int startIndex, FormatType type)
    {
        switch(type)
        {
            case FormatType.OPEN:
                return ParseMarkup(rawText, startIndex, openFormat);
            case FormatType.CLOSE:
                return ParseMarkup(rawText, startIndex, closeFormat);

        }

        return null;
    }

    /// <summary>
    /// Gets valid markup in rawText at starting index in this format
    /// </summary>
    virtual public string ParseMarkup(string rawText, int startIndex, DialogueMarkupFormat format)
	{

		if (startIndex + format.tagStart.Length < rawText.Length)
		{

			if (rawText.Substring(startIndex, format.tagStart.Length) == format.tagStart)
			{
				//Debug.Log($"Looking at {rawText.Substring(i, format.tagStart.Length)}, Format start:{format.tagStart}");
				int endIndex = rawText.IndexOf(format.tagEnd.ToCharArray()[format.tagEnd.Length - 1], startIndex + 1);
				//Debug.Log($"Looking for...{format.tagEnd.ToCharArray()[format.tagEnd.Length - 1]}");
				if (endIndex != -1)
				{
					string tryMarkup = rawText.Substring(startIndex, endIndex + 1 - startIndex);
					//Debug.Log($"Found {tryMarkup}");
					if (ValidateFormat(tryMarkup, format) && ValidateKeyName(tryMarkup, format) && ValidateMarkupData(tryMarkup, format))
					{
						//Debug.Log($"FOUND {tryMarkup} AT Raw text:{rawText}, startIndex:{startIndex}, format:{format.type}");
						return tryMarkup;

					}
				}
			}
		}
		return null;
	}
	#endregion

	#region VALIDATE INSTANCES

	#region VALIDATE KEYNAME
	/// <summary>
	/// Validates key name in markup
	/// </summary>
	/// <param name="markup"></param>
	/// <returns></returns>
	virtual public bool ValidateKeyName(string markup)
    {
        foreach (DialogueMarkupFormat format in formats)
        {
            if (ValidateKeyName(markup, format))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Validates key name in markup of this format
    /// </summary>
    /// <param name="markup"></param>
    /// <param name="format"></param>
    /// <returns></returns>
    virtual public bool ValidateKeyName(string markup, DialogueMarkupFormat format)
    {
        string markupKeyName = "";
        char[] textArray = markup.ToCharArray();

        for (int i = format.tagStart.Length; i < markup.Length; i++)
        {
            if (textArray[i] != ' ')
                markupKeyName += textArray[i];
            if (markupKeyName == keyName)
            {
                return true;
            }
        }

        return false;
    }
    #endregion

    #region VALIDATE FORMAT
    /// <summary>
    /// Validates markup is in this format
    /// </summary>
    /// <param name="markup"></param>
    /// <param name="format"></param>
    /// <returns></returns>
    virtual public bool ValidateFormat(string markup, DialogueMarkupFormat format)
    {
        if (markup.Length < format.tagStart.Length + format.tagEnd.Length)
            return false;


        return (markup.Substring(0, format.tagStart.Length) == format.tagStart
        && markup.Substring(markup.Length - format.tagEnd.Length, format.tagEnd.Length) == format.tagEnd);
    }
    #endregion

    #region VALIDATE MARKUPDATA

    /// <summary>
    /// Validates there is a valid markupData instance in markup
    /// </summary>
    /// <param name="markup"></param>
    /// <returns></returns>
    virtual public bool ValidateMarkupData(string markup)
    {
        foreach (MarkupData markupData in markupDatas)
        {
            if (ValidateMarkupData(markup, markupData))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Validates there is a valid instance of this markupData in markup
    /// </summary>
    /// <param name="markup"></param>
    /// <param name="markupData"></param>
    /// <returns></returns>
    virtual public bool ValidateMarkupData(string markup, MarkupData markupData)
    {
        foreach(DialogueMarkupFormat format in formats)
        {
            if (ValidateMarkupData(markup, markupData, format))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Validates there is a valid markupData instance in markup of this format
    /// </summary>
    /// <param name="markup"></param>
    /// <param name="format"></param>
    /// <returns></returns>
    virtual public bool ValidateMarkupData(string markup, DialogueMarkupFormat format)
    {
        foreach (MarkupData markupData in markupDatas)
        {
            //Debug.Log("Getting markup data from markupDatas");
            if (ValidateMarkupData(markup, markupData, format))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Validates there is a valid instance of this markupData in markup of this format
    /// </summary>
    /// <param name="markup"></param>
    /// <param name="markupData"></param>
    /// <param name="format"></param>
    /// <returns></returns>
    virtual public bool ValidateMarkupData(string markup, MarkupData markupData, DialogueMarkupFormat format)
    {   
        string markupKeyName = "";
        bool postEquals = false;

        char[] textArray = markup.ToCharArray();
        for (int i = format.tagStart.Length; i < markup.Length; i++)
        {

            if (postEquals && textArray[i] != ' ')
            {
                markupKeyName += textArray[i];
                //Debug.Log("MarkupKey:{markupKeyName}");
            }

            if (markupKeyName == markupData.keyName)
            {
                //Debug.Log("MarkupData validated");
                return true;
            }

            if (textArray[i] == equals)
                postEquals = true;


        }

        return false;

    }
    #endregion



    #region VALIDATE MARKUP

    /// <summary>
    /// Validates markup instance
    /// </summary>
    /// <param name="markup"></param>
    /// <returns></returns>
    virtual public bool ValidateMarkup(string markup)
    {


        //Debug.Log($"Trying to get markup text {markupText}");
        if (markup == null || markup.Length <= 0)
            return false;


        bool validFormat = false;

        //Debug.Log("Trying format..");
        DialogueMarkupFormat theFormat = null;

        //Match format
        foreach (DialogueMarkupFormat format in formats)
        {
            if (ValidateFormat(markup, format))
            {
                validFormat = true;
                theFormat = format;
            }
        }

        if (!validFormat)
            return false;

        if (ValidateKeyName(markup, theFormat))
        {
            foreach (MarkupData markupData in markupDatas)
            {
                if (ValidateMarkupData(markup, markupData, theFormat))
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Validates if there is a valid markup instance in rawText at starting index
    /// </summary>
    /// <param name="rawText"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    virtual public bool ValidateMarkup(string rawText, int index)
    {
        if (rawText == null || rawText.Length <= 0)
            return false;

        if (ValidateMarkup(ParseMarkup(rawText, index)))
            return true;

        return false;
    }

	#endregion

	#endregion

	#region APPLY TEXT
    virtual public string ApplyMarkup(string rawText, int startIndex)
    {
        string applyText = "";
        char[] textArray = rawText.ToCharArray();

        for(int i = 0; i < rawText.Length; i++)
        {
            if(i == startIndex)
            {
                applyText += GetMarkup();
			}
            
            applyText += textArray[i];
        }

		if (startIndex >= rawText.Length)
			applyText += GetMarkup();

		return applyText;
    }

    virtual public string ApplyMarkup(string rawText, int startIndex, MarkupData markupData)
    {
		string applyText = "";
		char[] textArray = rawText.ToCharArray();

		for (int i = 0; i < rawText.Length; i++)
		{
			if (i == startIndex)
			{
				applyText += GetMarkup(markupData);
			}
            
            applyText += textArray[i];
		}

        if(startIndex >= rawText.Length)
            applyText += GetMarkup(markupData);

		return applyText;
	}

    virtual public string ApplyMarkup(string rawText, int startIndex, FormatType type)
    {
        switch (type)
        {
            case FormatType.OPEN:
                return ApplyMarkup(rawText, startIndex, openFormat);
            case FormatType.CLOSE:
                return ApplyMarkup(rawText, startIndex, closeFormat);
        }
        return null;
    }

    virtual public string ApplyMarkup(string rawText, int startIndex, MarkupData markupData, FormatType type)
    {
        switch (type)
        {
            case FormatType.OPEN:
                return ApplyMarkup(rawText, startIndex, markupData, openFormat);
            case FormatType.CLOSE:
                return ApplyMarkup(rawText, startIndex, markupData, closeFormat);
        }

        return null;
}

	virtual public string ApplyMarkup(string rawText, int startIndex, DialogueMarkupFormat format)
	{
		string applyText = "";
		char[] textArray = rawText.ToCharArray();

		for (int i = 0; i < rawText.Length; i++)
		{
			if (i == startIndex)
			{
                applyText += GetMarkup(format);
			}
            
            applyText += textArray[i];
		}

        if(startIndex >= rawText.Length)
            applyText += GetMarkup(format);

		return applyText;
	}

    virtual public string ApplyMarkup(string rawText, int startIndex, MarkupData markupData, DialogueMarkupFormat format)
    {
		string applyText = "";
		char[] textArray = rawText.ToCharArray();

		for (int i = 0; i < rawText.Length; i++)
		{
			if (i == startIndex)
			{
				applyText += GetMarkup(markupData, format);
			}

		    applyText += textArray[i];
		}

        if (startIndex >= rawText.Length)
            applyText += GetMarkup(markupData, format);

		return applyText;

        
	}

    virtual public string ApplyMarkup(string rawText, Vector2Int indexRange)
    {
		string applyText = "";
		char[] textArray = rawText.ToCharArray();

		for (int i = 0; i < rawText.Length; i++)
		{
			if (i == indexRange.x)
			{
				applyText += GetMarkup(openFormat);
			}

				applyText += textArray[i];

			if (i == indexRange.y)
			{
				applyText += GetMarkup(closeFormat);
			}
		}

        if(indexRange.y >= rawText.Length)
            applyText += GetMarkup(closeFormat);

		return applyText;
	}

	virtual public string ApplyMarkup(string rawText, Vector2Int indexRange, MarkupData markupData)
	{
		string applyText = "";
		char[] textArray = rawText.ToCharArray();

		for (int i = 0; i < rawText.Length; i++)
		{
			if (i == indexRange.x)
			{
				applyText += GetMarkup(markupData, openFormat);
			}

			applyText += textArray[i];

			if (i == indexRange.y)
			{
				applyText += GetMarkup(markupData, closeFormat);
			}
		}

        if(indexRange.y >= rawText.Length)
            applyText += GetMarkup(markupData, closeFormat);

		return applyText;
	}

	#endregion

	#region REMOVE TEXT

	/// <summary>
	/// Removes all valid markup strings
	/// </summary>
	/// <param name="rawText"></param>
	/// <returns></returns>
	virtual public string RemoveMarkup(string rawText)
    {
        string removedMarkupText = "";

        char[] textArray = rawText.ToCharArray();

        for (int i = 0; i < rawText.Length; i++)
        {
            if (ValidateMarkup(rawText, i))
            {
                //Debug.Log($"Got markup:{ParseMarkup(rawText, i)}....moved to{textArray[i + ParseMarkup(rawText, i).Length]}");
                i += ParseMarkup(rawText, i).Length;
            }

            if (i < rawText.Length)
            {
                removedMarkupText += textArray[i];
            }
        }

        return removedMarkupText;
    }
    #endregion

    #region LOGIC
    virtual public bool Handle(string rawText, DialogueCall callType)
    {
        switch (callType)
        {
            case DialogueCall.DELTA:
                return HandleDelta(rawText);
            case DialogueCall.FULL:
                return HandleFull(rawText);
        }

        return false;
    }

    /// <summary>
    /// Handles logic for this markup in fullText
    /// </summary>
    /// <param name="dialogueManager"></param>
    /// <param name="fullText"></param>
    /// <returns></returns>
    virtual public bool HandleFull(string fullText)
    {
        if (!ValidateMarkup(ParseMarkup(fullText)))
        {
            return false;
        }

        switch(this.markupType)
        {
            case MarkupType.LOGIC:
                return false;
            case MarkupType.DISPLAY:
                
                char[] textArray = fullText.ToCharArray();

                for(int i = 0; i <  fullText.Length; i++)
                {
                    if(ValidateMarkup(ParseMarkup(fullText, i)))
                    {
                        string theMarkup = ParseMarkup(fullText, i);
                        MarkupData theMarkupData = ParseMarkupData(theMarkup);

                        switch(ParseFormatType(theMarkup))
                        {
                            case FormatType.OPEN:
                                theMarkupData.Open(this, fullText, DialogueCall.FULL);
                                isActiveApplying = true;
                                applyingData = theMarkupData;
                                break;
                            case FormatType.CLOSE:
                                theMarkupData.Close(this, fullText, DialogueCall.FULL);
                                isActiveApplying = false;
                                applyingData = null;
                                break;
                            default:
                                break;
                        }
                        i += ParseMarkup(fullText, i).Length - 1;
                    }

                    else if(isActiveApplying)
                    {
                        applyingData.Continue(this, textArray[i].ToString());
                    }
                }

                break;
        }

        return true;
    }


    /// <summary>
    /// Handles logic for this markup in deltaText
    /// </summary>
    /// <param name="dialogueManager"></param>
    /// <param name="deltaText"></param>
    /// <returns></returns>
    virtual public bool HandleDelta(string deltaText)
    {
		MarkupData theMarkupData = ParseMarkupData(deltaText);

		switch (this.markupType)
		{
			case MarkupType.LOGIC:
			{
                if(!ValidateMarkup(ParseMarkup(deltaText)))
                {
                    return false;
                }

				switch (ParseFormatType(ParseMarkup(deltaText)))
				{
					case FormatType.OPEN:
						theMarkupData.Open(this, deltaText, DialogueCall.DELTA);
						isActiveApplying = true;
						break;
					case FormatType.CLOSE:
						theMarkupData.Close(this, deltaText, DialogueCall.DELTA);
						isActiveApplying = false;
						break;
					default:
						break;
					}
				}
				break;

            case MarkupType.DISPLAY:
            {
                switch(ParseFormatType(ParseMarkup(deltaText)))
                {
                        case FormatType.OPEN:
                            theMarkupData.Open(this, deltaText, DialogueCall.DELTA);
                            isActiveApplying = true;
                            applyingData = theMarkupData;
                            break;
                        case FormatType.CLOSE:
                            theMarkupData.Close(this, deltaText, DialogueCall.DELTA);
                            isActiveApplying = false;
                            applyingData = null;
                            break;

                        default:
                            if(isActiveApplying && applyingData)
                            {
                                applyingData.Continue(this, deltaText);
                            }
                            break;
                }
            }
                break;
		}

		return true;
    }

    virtual public void OpenLogic(DialogueManager dialogueManager, string text)
    {
        //Debug.Log("Opening logic");
        //    if(hasParameters && GetParameterText(text) != null)
        //    { 
        //        lastStoredParameter = GetParameterText(text);

        //        //Run logic with parameter based on enum type

        //        switch(parameterType)
        //        {
        //            case DialogueMarkupParameterType.INT:
        //            break;
        //            case DialogueMarkupParameterType.FLOAT:
        //            break;
        //            case DialogueMarkupParameterType.BOOL:
        //            break;
        //            case DialogueMarkupParameterType.CHAR:
        //            break;
        //            case DialogueMarkupParameterType.STRING:
        //            break;
        //            case DialogueMarkupParameterType.DOUBLE:
        //            break;
        //        }
        //    }

        //lastStoredParameter = "";
    }

    virtual public void CloseLogic(DialogueManager dialogueManager, string text)
    {
        //Debug.Log("Closing markup");
    }
    #endregion
}