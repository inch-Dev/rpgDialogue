using System;
using NaughtyAttributes;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Localization.PropertyVariants.TrackedProperties;
using JetBrains.Annotations;
using System.Diagnostics.CodeAnalysis;

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
    [SerializeField] public string keyName;
    [SerializeField] protected char markupCharacter;
    protected static DialogueMarkupFormat openFormat = new DialogueMarkupFormat(FormatType.OPEN, "{", "}");
    protected static DialogueMarkupFormat closeFormat = new DialogueMarkupFormat(FormatType.CLOSE, "{/", "}");
    protected static DialogueMarkupFormat[] formats = new DialogueMarkupFormat[2] { openFormat, closeFormat };
    protected char equals = '=';
    [SerializeField] protected bool hasParameters;
    [ShowIf("hasParameters")]
    [SerializeField] List<MarkupData> markupDatas;
    protected string lastStoredParameter;

    virtual public void ApplyMarkup(string rawText)
    {

    }

    #region GETTERS

    /// <summary>
    /// Get format type of markup
    /// </summary>
    /// <param name="markup"></param>
    /// <returns></returns>
    virtual public FormatType GetFormatType(string markup)
    {
        Debug.Log($"Getting markup text:{markup}");
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
    virtual public DialogueMarkupFormat GetFormat(string markup)
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


    virtual public List<MarkupData> GetMarkupDatas(){ return markupDatas; }

    /// <summary>
    /// Get markupData in markup
    /// </summary>
    /// <param name="markup"></param>
    /// <returns></returns>
    virtual public MarkupData GetMarkupData(string markup)
    {
        foreach (DialogueMarkupFormat format in formats)
        {
            MarkupData theMarkupData = GetMarkupData(markup, format);
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
    virtual public MarkupData GetMarkupData(string markup, DialogueMarkupFormat format)
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
    virtual public string GetMarkup(string rawText)
    {
        for (int i = 0; i < rawText.Length; i++)
        {
            if (GetMarkup(rawText, i) != null)
                return GetMarkup(rawText, i);
        }
        return null;
    }

    /// <summary>
    /// Gets valid markup in rawText at starting index
    /// </summary>
    virtual public string GetMarkup(string rawText, int startIndex)
    {
        //Check all formats for first valid instance

        foreach (DialogueMarkupFormat format in formats)
        {
            string markupString = GetMarkup(rawText, startIndex, format);
            if (markupString != null)
            {
                return markupString;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets valid markup in rawText at starting index in this format
    /// </summary>
    virtual public string GetMarkup(string rawText, int startIndex, DialogueMarkupFormat format)
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
        foreach (DialogueMarkupFormat format in formats)
        {
            if (ValidateMarkupData(markup, format))
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

        if (ValidateMarkup(GetMarkup(rawText, index)))
            return true;

        return false;
    }

    #endregion

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
                i += GetMarkup(rawText, i).Length;
            }

            else
            {
                removedMarkupText += textArray[i];
            }
        }

        return removedMarkupText;
    }

    //virtual public string RemoveOpenMarkup(string text)
    //{
    //    string removedText = "";
    //    char[] textArray = text.ToCharArray();

    //    for(int i = 0; i < textArray.Length; i++)
    //    {
    //        if(ValidateOpenMarkup(text, i))
    //        {
    //            int indexOfEnd = text.IndexOf(openFormatTagEnd, i + 1);

    //            if(indexOfEnd != -1)
    //            {
    //                int lengthOfTag = indexOfEnd - i + 1;
    //                i += lengthOfTag;
    //                continue;
    //            }
    //        }
    //            removedText += textArray[i];
    //    }
    //    return removedText;
    //}

    //virtual public string RemoveCloseMarkup(string text)
    //{
    //    string removedText = "";
    //    char[] textArray = text.ToCharArray();

    //    for(int i = 0; i < textArray.Length; i++)
    //    {
    //        if(ValidateCloseMarkup(text, i))
    //        {
    //            int indexOfEnd = text.IndexOf(closeFormatTagEnd, i + 1);
    //            if(indexOfEnd != -1)
    //            {
    //                int lengthOfTag = indexOfEnd - i + 1;
    //                i += lengthOfTag;
    //                continue;
    //            }
    //        }
    //            removedText += textArray[i];
    //    }
    //    Debug.Log($"Removed close makrup text {removedText}");
    //    return removedText;
    //}

    //virtual public string RemoveFormatTagText(string text)
    //{
    //    //Need open and close
    //    //Debug.Log($"Incoming text:{text}");
    //    string excludedMarkupText = "";
    //    char[] textArray = text.ToCharArray();
    //    bool isReadingTag = false;

    //    for(int i = 0; i < textArray.Length; i++)
    //    {
    //        if(i + 1 < textArray.Length && textArray[i].ToString() == openFormatTagStart) //Skip over beginning of tag
    //        {
    //            int indexOfEnd = text.IndexOf(openFormatTagEnd, i + 1);
    //            if(indexOfEnd != -1 )
    //            {
    //                string tryTag = text.Substring(i + 1, indexOfEnd - 2 - i);
    //                char tryChar = text.Substring(indexOfEnd - 1, 1).ToCharArray()[0];

    //                if(OLDValidateParameter(tryTag) && tryChar == markupCharacter)
    //                {
    //                    //Debug.Log($"Reading tag as:{parameterType}");
    //                    isReadingTag = true;
    //                    continue;
    //                }
    //                else
    //                {
    //                    Debug.Log($"Failed to recognize {tryTag} as {parameterType} or {tryChar} as {markupCharacter}");
    //                }
    //            }
    //        }

    //        if(!isReadingTag)
    //        {
    //            excludedMarkupText += textArray[i];
    //        }


    //        if(textArray[i] == '>') //Skip over ending of tag
    //        {
    //            if(isReadingTag)
    //            {
    //                isReadingTag = false;
    //                continue;
    //            }
    //        }


    //        //Debug.Log($"Exluded markup text:{excludedMarkupText}");
    //    }
    //    return excludedMarkupText;
    //}

    #endregion

    #region LOGIC
    //Move this to markupData
    virtual public bool HandleLogic(DialogueManager dialogueManager, string deltaLogicText)
    {
        //Error handling
        if (!ValidateMarkup(GetMarkup(deltaLogicText)))
        {
            //Debug.Log($"Could not validate from:{deltaLogicText}");
            return false;
        }
        else
        {
            MarkupData theMarkupData = GetMarkupData(deltaLogicText);

            switch (GetFormatType(GetMarkup(deltaLogicText)))
            {
                case FormatType.OPEN:
                    theMarkupData.OpenLogic();
                    break;
                case FormatType.CLOSE:
                    theMarkupData.CloseLogic();
                    break;
                default:
                    Debug.Log($"Could not get format type from {GetFormatType(GetMarkup(deltaLogicText))}");
                    break;
            }
        }

        //Get markupData
        //Run markupData function



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