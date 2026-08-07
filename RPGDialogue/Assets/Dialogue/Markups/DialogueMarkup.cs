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




//Make DialogueMarkupFormat struct???


[CreateAssetMenu(fileName = "Dialogue", menuName = "ScriptableObjects/DialogueObjects/DialogueMarkups/DialogueMarkup", order = 1)]
public class DialogueMarkup : ScriptableObject
{
    [SerializeField] public string keyName;
    [SerializeField] protected char markupCharacter;
    protected static DialogueMarkupFormat openFormat = new DialogueMarkupFormat(FormatType.OPEN, "{", "}");
    protected static DialogueMarkupFormat closeFormat = new DialogueMarkupFormat(FormatType.CLOSE, "{/", "}");
    protected static DialogueMarkupFormat[] formats = new DialogueMarkupFormat[2]{ openFormat, closeFormat };
    protected string openFormatTagStart = "{";
    protected string openFormatTagEnd = "}";
    protected string closeFormatTagStart = "{/";
    protected string closeFormatTagEnd = "}";
    protected char equals = '=';
    [SerializeField] protected bool hasParameters;
    [ShowIf("hasParameters")]
    [SerializeField] protected DialogueMarkupParameterType parameterType;
    [ShowIf("hasParameters")]
    [SerializeField] List<MarkupData> markupDatas;
    protected string lastStoredParameter;


    void OnValidate()
    {
        //Reset values

        openFormatTagStart = "{";
        closeFormatTagStart = "{/";
        openFormatTagEnd = "}";
        closeFormatTagEnd = "}";

        openFormatTagEnd = markupCharacter.ToString() + openFormatTagEnd;
        closeFormatTagEnd = markupCharacter.ToString() + closeFormatTagEnd;
    }
    
    #region GETTERS

    virtual public FormatType GetFormatType(string markupText)
    {
        FormatType formatType = FormatType.INVALID;

        char[] textArray = markupText.ToCharArray();

        if(textArray.Length > closeFormatTagEnd.Length + closeFormatTagStart.Length)
        {
            if (markupText.Substring(0, closeFormatTagStart.Length) == closeFormatTagStart && markupText.Substring(markupText.Length - closeFormatTagEnd.Length, closeFormatTagEnd.Length) == closeFormatTagEnd)
                formatType = FormatType.CLOSE;
        }

        else if(textArray.Length > openFormatTagStart.Length + openFormatTagEnd.Length)
        {
			if (markupText.Substring(0, openFormatTagStart.Length) == openFormatTagStart && markupText.Substring(markupText.Length - openFormatTagEnd.Length, openFormatTagEnd.Length) == openFormatTagEnd)
				formatType = FormatType.OPEN;
		}

        return formatType;
    }

    virtual public DialogueMarkupFormat GetFormat(string markupText)
    {
        DialogueMarkupFormat theFormat = null;

        char[] textArray = markupText.ToCharArray();

        foreach(DialogueMarkupFormat format in formats)
        {
            if (markupText.Length < format.tagStart.Length + format.tagEnd.Length)
                continue;

            if (markupText.Substring(0, format.tagStart.Length) == format.tagStart
            && markupText.Substring(markupText.Length - format.tagEnd.Length, format.tagEnd.Length) == format.tagEnd)
                return format;
        }

        return theFormat;
    }


    //Iterate multiple times till matching parameter sequence
    /*virtual public string[] GetParameterStrings(string markupText, DialogueMarkupFormat format)
    {

        if (!ValidateFormat(markupText, format))
            return null;

		if (!hasParameters)
		{
			return null;
		}

		List<string> parameters = new List<string>();

		

		string curParameter = "";

		char tagStart = '{';
		char tagEnd = '}';

		int tagStartLength = format.tagStart.Length;
		int tagEndLength = format.tagEnd.Length;

		//Validate Instance
		

		char[] textArray = markupText.ToCharArray();

		//Find all parameters
		for (int i = tagStartLength; i < markupText.Length - tagStartLength - tagEndLength - markupCharacter.ToString().Length; i++)
		{
			if (textArray[i] != ',')
            {
                curParameter += textArray[i].ToString();
            }
            else
            {
                parameters.Add(curParameter);
                curParameter = "";
            }
			
		}

        if (ValidateParameters(parameters.ToArray()))
            return parameters.ToArray();
        else
            return null;
	}
    virtual public string GetParameterText(string text)
    {
        foreach(MarkupData data in markupDatas)
        {
           Debug.Log($"Markups on {this}...{data.GetParameters().Length}");
        }






        string parameterText = "";

        //Check for markup character

        if(!hasParameters)
        {   
            Debug.Log("This markup has no parameter!");
            return null;
        }

        //Isolate string format tags and parameter
        bool shoudlAddToMarkupText = false;

        //Identify first char of format tag start and format tag end
        char formatTagStartFirstChar = openFormatTagStart.ToCharArray()[0];
        char formatTagEndFirstChar = openFormatTagEnd.ToCharArray()[0];

        char[] textArray = text.ToCharArray();

        //Get parameter text
        for(int i = 0; i < textArray.Length; i++)
        {
            if(textArray[i] == formatTagEndFirstChar)
            {
                break;
            }
            if(textArray[i] == formatTagStartFirstChar)
            {
                shoudlAddToMarkupText = true;

                //Skip over format tag start char
                i += openFormatTagStart.Length - 1;
                continue;
            }
            if(shoudlAddToMarkupText)
            {
                parameterText += textArray[i];
            }
                    
        }
        

        if(OLDValidateParameter(parameterText))
            return parameterText;
        else
            return null;
    }

    virtual public MarkupData GetMatchingParametersMarkupData(string[] parameterStrings)
    {
        MarkupData matchMarkupData =  null;

        foreach(MarkupData markupData in markupDatas)
        {
            if(ValidateParameters(parameterStrings, markupData))
                return markupData;
        }


        return matchMarkupData;
    }*/
    
    //virtual public string GetMarkupString(string text, int index)
    //{
    //    string markupText = "";
    //    if(!ValidateMarkup(text, index))
    //        return null;

    //    if(ValidateOpenMarkup(text, index))
    //    {
    //        int indexOfEnd = text.IndexOf(openFormatTagEnd, index + 1);
    //            if(indexOfEnd != -1)
    //            {
    //                markupText = text.Substring(index, indexOfEnd - index + 2);
    //                return markupText;
    //            }
    //    }
    //    else if(ValidateCloseMarkup(text, index))
    //    {
    //        int indexOfEnd = text.IndexOf(closeFormatTagEnd, index + 1);
    //        if(indexOfEnd != -1)
    //        {
    //            markupText = text.Substring(index,(indexOfEnd - index) + 2);
    //            //Debug.Log($"Returning {markupText}");
    //            return markupText;
    //        }
    //    }
    //    return null;
    //}

    //virtual public string GetMarkupString(string text)
    //{
    //    string markupText = "";
    //    char[] textArray = text.ToCharArray();

    //    if(!ValidateMarkup(text))
    //        return null;
        
    //    for(int i = 0; i < text.Length; i++)
    //    {
    //        if(GetMarkupString(text, i) != null)
    //        {
    //            markupText = GetMarkupString(text, i);
    //            return markupText;
    //        }
    //    }
    //    //Debug.Log($"Found markup {markupText}");
    //    return null;
    //}

    
    /// <summary>
    /// Gets first valid markup string from rawText
    /// </summary>
    virtual public string GetMarkupString(string rawText)
    {
       for(int i = 0; i < rawText.Length; i++)
       {
            if (GetMarkupString(rawText, i) != null)
                return GetMarkupString(rawText, i);
       }
        return null;
    }

    /// <summary>
    /// Gets valid markup string from rawText at starting index
    /// </summary>
    virtual public string GetMarkupString(string rawText, int startIndex)
    {
        //Check all formats for first valid instance

        foreach(DialogueMarkupFormat format in formats)
        {
            string markupString = GetMarkupString(rawText, startIndex, format);
            if (markupString != null)
            {
                return markupString;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets valid markup string of MarkupFormat from rawText at starting index
    /// </summary>
    virtual public string GetMarkupString(string rawText, int startIndex, DialogueMarkupFormat format)
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
							Debug.Log($"FOUND {tryMarkup} AT Raw text:{rawText}, startIndex:{startIndex}, format:{format.type}");
							return tryMarkup;

                        }
                    }
                }
			}
        return null;
	}
	#endregion




	#region VALIDATE INSTANCES

    virtual public bool ValidateKeyName(string markupText, DialogueMarkupFormat format)
    {
        string markupKeyName = "";
        char[] textArray = markupText.ToCharArray();

        for(int i = format.tagStart.Length; i < markupText.Length; i++)
        {
            if(textArray[i] != ' ')
                markupKeyName += textArray[i];
            if (markupKeyName == keyName)
            {
                return true;
            }
        }

        return false;
    }

    virtual public bool ValidateFormat(string markupText, DialogueMarkupFormat format)
    {
        if (markupText.Length < format.tagStart.Length + format.tagEnd.Length)
            return false;

        
        return (markupText.Substring(0, format.tagStart.Length) == format.tagStart
        && markupText.Substring(markupText.Length - format.tagEnd.Length, format.tagEnd.Length) == format.tagEnd);
	}

	virtual public bool ValidateMarkupData(string markupText, DialogueMarkupFormat format)
    {
        foreach(MarkupData markupData in markupDatas)
        {
            //Debug.Log("Getting markup data from markupDatas");
            if (ValidateMarkupData(markupText, markupData, format))
                return true;
        }
        return false;
    }
    
    virtual public bool ValidateMarkupData(string markupText, MarkupData markupData, DialogueMarkupFormat format)
    {
        string markupKeyName = "";
        bool postEquals = false;

        char[] textArray = markupText.ToCharArray();
        for(int i = format.tagStart.Length; i < markupText.Length; i++)
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
            
            if(textArray[i] == equals)
                postEquals = true;


        }

        return false;
        
    }
    
    #region VALIDATE PARAMETERS
	//Dictionary for types and their functions to call generic method
	/*static readonly Dictionary<Type, MethodInfo> validParameterCalls = new();
    virtual public bool CallValidateParameter(string parameterString, Type parameterType)
    {
        if (!validParameterCalls.TryGetValue(parameterType, out MethodInfo callMethod))
        {
            MethodInfo newMethod = GetType().GetMethod(nameof(ValidateParameter), BindingFlags.Public | BindingFlags.Instance);


            callMethod = newMethod.MakeGenericMethod(parameterType);
            validParameterCalls[parameterType] = callMethod;
        }
        return (bool)callMethod.Invoke(this, new object[] { parameterString });
    }
    virtual public bool ValidateParameter<T>(string parameterString)
    {
        Type t = typeof(T);


        //Error handling for different types

        //Enums
        if (t.IsEnum)
        {
            return Enum.TryParse(t, parameterString, true, out _);
        }

        if (t == typeof(string))
        {
            return true;
        }

        MethodInfo tryParse = t.GetMethod("TryParse", new[] { typeof(string), t.MakeByRefType() });

        //Types with try parse
        if(tryParse != null)
        {
            object[] args = { parameterString, null };
            bool success = (bool)tryParse.Invoke(null, args);
        }

        //Others without
		try
        {
            Convert.ChangeType(parameterString, t);
            return true;
        }

        catch
        {
            return false;
        }

	}
	virtual public bool ValidateParameters(string[] parameterStrings)
	{
        Debug.Log("Trying to validate...");
        return false;
        bool parameterMatch = false;
        //Compare parameters to parameter text recieved
		foreach (MarkupData markupData in markupDatas)
		{
            if (ValidateParameters(parameterStrings, markupData))
                parameterMatch = true;
		}

        return parameterMatch;
	}
    virtual public bool ValidateParameters(string[] parameterStrings, MarkupData markupData)
    {
        bool parameterMatch = true;

        FieldInfo[] parameters = markupData.GetParameters();
        if (parameters.Length != parameterStrings.Length)
            return false;


        //Cast parameter strings to parameter types and validate
        for(int i = 0; i < parameters.Length;i++)
        {
            Type parameterType = parameters[i].FieldType;

            if(!CallValidateParameter(parameterStrings[i], parameterType))
            {
                parameterMatch = false;
            }
        }

        return parameterMatch;
    }
	virtual public bool OLDValidateParameter(string text)
    {
        bool isValid = false;
        switch(parameterType)
        {
            case DialogueMarkupParameterType.INT:
            if(int.TryParse(text, out int intResult))
                {
                    lastStoredParameter = text;
                    isValid = true;
                }
                break;
            case DialogueMarkupParameterType.FLOAT:
            if(float.TryParse(text, out float floatResult))
                {
                    lastStoredParameter = text;
                    isValid = true;
                }
                break;
            case DialogueMarkupParameterType.BOOL:
            if(bool.TryParse(text, out bool boolResult))
                {
                    lastStoredParameter = text;
                    isValid = true;
                }
                break;
            case DialogueMarkupParameterType.CHAR:
            if(char.TryParse(text, out char charResult))
                {
                    lastStoredParameter = text;
                    isValid = true;
                }
                break;
            case DialogueMarkupParameterType.DOUBLE:
            if(double.TryParse(text, out double doubleResult))
                {
                    lastStoredParameter = text;
                    isValid = true;
                }
                break;
            case DialogueMarkupParameterType.EXPRESSION:
            if(Enum.TryParse(text, out DialogueExpressionID expressionResult))
                {
                  lastStoredParameter = text;
                  isValid = true;  
                }
                break;
            case DialogueMarkupParameterType.SPEED:
                if(Enum.TryParse(text, out DialogueSpeedID speedResult))
                {
                    lastStoredParameter = text;
                    isValid = true;
                }
                break;

            case DialogueMarkupParameterType.STRING:
                lastStoredParameter = text;
                isValid = true;
                break;
        }
        //Debug.Log($"FAILED TO VALIDATE PARAMETER {text} as {parameterType}");
        return isValid;
    }
    */
	#endregion

	#region VALIDATE MARKUP
	virtual public bool ValidateMarkup(string markupText)
    {


        //Debug.Log($"Trying to get markup text {markupText}");
        if(markupText == null || markupText.Length <= 0)
            return false;


        bool validFormat = false;

        //Debug.Log("Trying format..");
		DialogueMarkupFormat theFormat = null;

        //Match format
        foreach (DialogueMarkupFormat format in formats)
        {
            if (ValidateFormat(markupText, format))
            {
                validFormat = true;
                theFormat = format;
            }
        }

            if (!validFormat)
            return false;

        if(ValidateKeyName(markupText, theFormat))
        {
            foreach(MarkupData markupData in markupDatas)
            {
                    if (ValidateMarkupData(markupText, markupData, theFormat))
                        return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Validates if there is a valid markup string in rawText up to index value
    /// </summary>
    /// <param name="rawText"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    virtual public bool ValidateMarkup(string rawText, int index)
    {
        if (rawText == null || rawText.Length <= 0)
            return false;

        if (ValidateMarkup(GetMarkupString(rawText, index)))
                return true;

        return false;
    }

    //virtual public bool ValidateMarkup(string text, int startIndex)
    //{
    //    if(text.Length <= 0)
    //    return false;

    //    if (startIndex >= text.Length)
    //        return false;
        
    //    if(ValidateOpenMarkup(text, startIndex))
    //    {
    //        return true;
    //    }

    //    else if(ValidateCloseMarkup(text, startIndex))
    //    {
    //        return true;
    //    }

    //    return false;   
    //}
  //  virtual public bool ValidateOpenMarkup(string text)
  //  {
  //      char[] textArray = text.ToCharArray();
  //      for(int i = 0; i < textArray.Length; i++)
  //      {
  //          if(ValidateOpenMarkup(text, i))
  //              return true;
  //      }
  //       return false;
  //  }
  //  virtual public bool ValidateOpenMarkup(string text, int startIndex)
  //  {
  //      char[] textArray = text.ToCharArray();

  //      if (startIndex >= textArray.Length)
  //          return false;
  //      if(startIndex + 1>= textArray.Length)
  //          return false;

  //      if(startIndex + 2 >= textArray.Length)
  //          return false;

  //      if(textArray[startIndex].ToString() != openFormatTagStart)
  //          return false;

  //      if (text.Substring(startIndex, closeFormatTagStart.Length) == closeFormatTagStart)
  //          return false;

  //      int indexOfEnd = text.IndexOf(openFormatTagEnd, startIndex + 1);

  //      if(indexOfEnd == -1)
  //          return false;
  //      //Debug.Log($"Open{openFormatTagStart},Close:{openFormatTagEnd}");
		////Debug.Log($"Length:{text.Length} Start index:{startIndex}");
		//string tryTag = text.Substring(startIndex, ((indexOfEnd - startIndex) + openFormatTagEnd.Length));
  //      string tryParam = tryTag.Substring(openFormatTagStart.Length, tryTag.Length - openFormatTagEnd.Length - markupCharacter.ToString().Length); //Start  index cant be bigger than length of string
  //      char tryChar = tryTag.ToCharArray()[tryTag.Length - (openFormatTagEnd.Length)];
  //      //Debug.Log($"Try tag:{tryTag},tryParam:{tryParam},tryChar:{tryChar}");

  //      if (ValidateParameters(GetParameterStrings(tryTag)) && tryChar == markupCharacter)
  //      {
  //          return true;
  //      }

  //      return false;
  //  }
  //  virtual public bool ValidateCloseMarkup(string text)
  //  {
  //      char[] textArray = text.ToCharArray();
  //      for(int i = 0; i < textArray.Length; i++)
  //      {
  //          if(ValidateCloseMarkup(text, i))
  //              return true;
  //      }
  //       return false;
  //  }
  //  virtual public bool ValidateCloseMarkup(string text, int startIndex)
  //  {
  //      char[] textArray = text.ToCharArray();

  //      if (startIndex >= text.Length)
  //          return false;

  //      if(startIndex + closeFormatTagStart.Length >= textArray.Length)
  //          return false;

  //      if(text.Substring(startIndex, closeFormatTagStart.Length) != closeFormatTagStart)
  //          return false;

  //      int indexOfEnd = text.IndexOf(closeFormatTagEnd, startIndex + 1);

  //      if(indexOfEnd == -1)
  //          return false;

  //      string tryTag = text.Substring(startIndex, (indexOfEnd - startIndex) + closeFormatTagStart.Length);
  //      string tryParam = tryTag.Substring(closeFormatTagStart.Length, tryTag.Length - closeFormatTagEnd.Length - 1 - markupCharacter.ToString().Length);
  //      char tryChar = tryTag.ToCharArray()[tryTag.Length - (closeFormatTagEnd.Length)];
  //      //Debug.Log($"Try tag:{tryTag},tryParam:{tryParam},tryChar:{tryChar}");

  //      if (ValidateParameters(GetParameterStrings(tryTag)) && (tryChar == markupCharacter))
  //      {
  //          return true;
  //      }
  //      return false;
  //  }
   #endregion
   
   #endregion
   
    
    #region REMOVE TEXT
    //virtual public string RemoveMarkupText(string text)
    //{   
    //    string excludedMarkupText = "";
    //    if(!ValidateMarkup(text))
    //        return text;
    //    excludedMarkupText = RemoveOpenMarkup(text);
    //    excludedMarkupText = RemoveCloseMarkup(excludedMarkupText);
    //    return excludedMarkupText;
    //}

  

    virtual public string RemoveMarkupText(string rawText)
    {
        string removedMarkupText = "";

        char[] textArray = rawText.ToCharArray();

        for(int i = 0; i < rawText.Length; i++)
        {
            if(ValidateMarkup(rawText, i))
            {
                i += GetMarkupString(rawText, i).Length;
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
	virtual public bool HandleLogic(DialogueManager dialogueManager, string markupText)
    {
        //Error handling
        if (!ValidateMarkup(markupText)) //Validate parameters in validate markup
            return false;

        Debug.Log("Logic handling!!!!!!!!!!!");

        //Get markup from keynames

        
        
        return true;
    }

    //Move these??????
    virtual public bool RecognizeMarkupAtBeginning(string text)
    {
        bool isAtBeginning = false;
        char[] textArray = text.ToCharArray();

        if(text.Length < openFormatTagStart.Length)
        return false;

        for(int i = 0; i < textArray.Length; i++)
        {
            if(i == 0)
            {
                if(openFormatTagStart.Length <= textArray.Length)
                {
                    string tryFormatTag = text.Substring(i, openFormatTagStart.Length);
                    if(tryFormatTag == openFormatTagStart)
                        return true;
                }
            }
        }

        return isAtBeginning;
    }

    virtual public bool RecognizeMarkupAtEnd(string text)
    {
        bool isAtEnd = false;
        char[] textArray = text.ToCharArray();
        
        if(text.Length < openFormatTagEnd.Length)
            return false;

        for(int i = 0; i < textArray.Length; i++)
        {
            if(i + openFormatTagEnd.Length == textArray.Length)
            {
                string tryFormatTag = text.Substring(i, openFormatTagEnd.Length);
                if(tryFormatTag == openFormatTagEnd)
                    return true;
            }
        }

        return isAtEnd;
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
