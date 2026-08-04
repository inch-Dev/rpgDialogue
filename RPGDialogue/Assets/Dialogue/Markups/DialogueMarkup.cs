using System;
using NaughtyAttributes;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Localization.PropertyVariants.TrackedProperties;

[CreateAssetMenu(fileName = "Dialogue", menuName = "ScriptableObjects/DialogueObjects/DialogueMarkups/DialogueMarkup", order = 1)]
public class DialogueMarkup : ScriptableObject
{
    [SerializeField] public string keyName;
    [SerializeField] protected char markupCharacter;
    protected string openFormatTagStart = "{";
    protected string openFormatTagEnd = "}";
    protected string closeFormatTagStart = "{/";
    protected string closeFormatTagEnd = "}";
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

    //Iterate multiple times till matching parameter sequence
    virtual public string[] GetParameterStrings(string markupText)
    {
        List<string> parameters = new List<string>();

		if (!hasParameters)
		{
			return null;
		}

		string curParameter = "";

		char tagStart;
		char tagEnd;

		int tagStartLength;
		int tagEndLength;

		//Validate Instance
		if (ValidateOpenMarkup(markupText))
		{
			tagStart = openFormatTagStart.ToCharArray()[0];
			tagEnd = openFormatTagEnd.ToCharArray()[0];

			tagStartLength = openFormatTagStart.Length;
			tagEndLength = openFormatTagEnd.Length;
		}

		else if (ValidateCloseMarkup(markupText))
		{
			tagStart = closeFormatTagStart.ToCharArray()[0];
			tagEnd = closeFormatTagEnd.ToCharArray()[0];

			tagStartLength = closeFormatTagStart.Length;
			tagEndLength = closeFormatTagEnd.Length;
		}

		else
		{
			return null;
		}

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
        

        if(ValidateParameter(parameterText))
            return parameterText;
        else
            return null;
    }
    
    virtual public string GetMarkupString(string text, int index)
    {
        string markupText = "";
        if(!ValidateMarkup(text, index))
            return null;

        if(ValidateOpenMarkup(text, index))
        {
            int indexOfEnd = text.IndexOf(openFormatTagEnd, index + 1);
                if(indexOfEnd != -1)
                {
                    markupText = text.Substring(index, indexOfEnd - index + 2);
                    return markupText;
                }
        }
        else if(ValidateCloseMarkup(text, index))
        {
            int indexOfEnd = text.IndexOf(closeFormatTagEnd, index + 1);
            if(indexOfEnd != -1)
            {
                markupText = text.Substring(index,(indexOfEnd - index) + 2);
                //Debug.Log($"Returning {markupText}");
                return markupText;
            }
        }
        return null;
    }
    virtual public string GetMarkupString(string text)
    {
        string markupText = "";
        char[] textArray = text.ToCharArray();

        if(!ValidateMarkup(text))
            return null;
        
        for(int i = 0; i < text.Length; i++)
        {
            if(GetMarkupString(text, i) != null)
            {
                markupText = GetMarkupString(text, i);
                return markupText;
            }
        }
        //Debug.Log($"Found markup {markupText}");
        return null;
    }
	#endregion



	#region VALIDATE INSTANCES

	#region VALIDATE PARAMETERS
	//Dictionary for types and their functions to call generic method
	static readonly Dictionary<Type, MethodInfo> validParameterCalls = new();
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
        //Compare parameter sequences to parameter text recieved
		foreach (MarkupData markupData in markupDatas)
		{
            FieldInfo[] parameters = markupData.GetParameters();
            if (parameters.Length != parameterStrings.Length)
                continue;

            bool parameterMatch = false;

            //Match parameters in markupData to parameter strings
			for (int i = 0; i < parameters.Length; i++)
			{
                //Get parameter type at index
                Type parameterType = parameters[i].FieldType;

                //Cast string parameter to data type of parameter at index
                if(!CallValidateParameter(parameterStrings[i],parameterType))
                {
                    parameterMatch = false;
                    break;
                }
			}

            if (parameterMatch)
                break;
		}

		return false;
	}
	virtual public bool ValidateParameter(string text)
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
	#endregion

	#region VALIDATE MARKUP
	virtual public bool ValidateMarkup(string text)
    {
        if(text.Length <= 0)
        return false;

        if(ValidateOpenMarkup(text) || ValidateCloseMarkup(text))
            return true;
        // /Debug.Log($"FAILED TO VALIDATE MARKUP");
        return false;
    }
    virtual public bool ValidateMarkup(string text, int index)
    {
        if(text.Length <= 0)
        return false;

        if (index >= text.Length)
            return false;

        if(ValidateOpenMarkup(text, index))
        {
            //Debug.Log("Found valid open markup");
            return true;
        }

        else if(ValidateCloseMarkup(text, index))
        {
            //Debug.Log($"Found valid close markup");
            return true;
        }

        return false;   
    }
    virtual public bool ValidateOpenMarkup(string text)
    {
        char[] textArray = text.ToCharArray();
        for(int i = 0; i < textArray.Length; i++)
        {
            if(ValidateOpenMarkup(text, i))
                return true;
        }
         return false;
    }
    virtual public bool ValidateOpenMarkup(string text, int startIndex)
    {
        char[] textArray = text.ToCharArray();

        if (startIndex >= textArray.Length)
            return false;
        if(startIndex + 1>= textArray.Length)
            return false;

        if(startIndex + 2 >= textArray.Length)
            return false;

        if(textArray[startIndex].ToString() != openFormatTagStart)
            return false;

        if (text.Substring(startIndex, closeFormatTagStart.Length) == closeFormatTagStart)
            return false;

        int indexOfEnd = text.IndexOf(openFormatTagEnd, startIndex + 1);

        if(indexOfEnd == -1)
            return false;
        //Debug.Log($"Open{openFormatTagStart},Close:{openFormatTagEnd}");
		//Debug.Log($"Length:{text.Length} Start index:{startIndex}");
		string tryTag = text.Substring(startIndex, ((indexOfEnd - startIndex) + openFormatTagEnd.Length));
        string tryParam = tryTag.Substring(openFormatTagStart.Length, tryTag.Length - openFormatTagEnd.Length - markupCharacter.ToString().Length); //Start  index cant be bigger than length of string
        char tryChar = tryTag.ToCharArray()[tryTag.Length - (openFormatTagEnd.Length)];
        //Debug.Log($"Try tag:{tryTag},tryParam:{tryParam},tryChar:{tryChar}");

        if (ValidateParameter(tryParam) && tryChar == markupCharacter)
        {
            return true;
        }

        return false;
    }
    virtual public bool ValidateCloseMarkup(string text)
    {
        char[] textArray = text.ToCharArray();
        for(int i = 0; i < textArray.Length; i++)
        {
            if(ValidateCloseMarkup(text, i))
                return true;
        }
         return false;
    }
    virtual public bool ValidateCloseMarkup(string text, int startIndex)
    {
        char[] textArray = text.ToCharArray();

        if (startIndex >= text.Length)
            return false;

        if(startIndex + closeFormatTagStart.Length >= textArray.Length)
            return false;

        if(text.Substring(startIndex, closeFormatTagStart.Length) != closeFormatTagStart)
            return false;

        int indexOfEnd = text.IndexOf(closeFormatTagEnd, startIndex + 1);

        if(indexOfEnd == -1)
            return false;

        string tryTag = text.Substring(startIndex, (indexOfEnd - startIndex) + closeFormatTagStart.Length);
        string tryParam = tryTag.Substring(closeFormatTagStart.Length, tryTag.Length - closeFormatTagEnd.Length - 1 - markupCharacter.ToString().Length);
        char tryChar = tryTag.ToCharArray()[tryTag.Length - (closeFormatTagEnd.Length)];
        //Debug.Log($"Try tag:{tryTag},tryParam:{tryParam},tryChar:{tryChar}");

        if (ValidateParameter(tryParam) && (tryChar == markupCharacter))
        {
            return true;
        }
        return false;
    }
   #endregion
   
   #endregion
   
    
    #region REMOVE TEXT
    virtual public string RemoveMarkupText(string text)
    {   
        string excludedMarkupText = "";
        if(!ValidateMarkup(text))
            return text;
        excludedMarkupText = RemoveOpenMarkup(text);
        excludedMarkupText = RemoveCloseMarkup(excludedMarkupText);
        return excludedMarkupText;
    }

    virtual public string RemoveOpenMarkup(string text)
    {
        string removedText = "";
        char[] textArray = text.ToCharArray();

        for(int i = 0; i < textArray.Length; i++)
        {
            if(ValidateOpenMarkup(text, i))
            {
                int indexOfEnd = text.IndexOf(openFormatTagEnd, i + 1);

                if(indexOfEnd != -1)
                {
                    int lengthOfTag = indexOfEnd - i + 1;
                    i += lengthOfTag;
                    continue;
                }
            }
                removedText += textArray[i];
        }
        return removedText;
    }

    virtual public string RemoveCloseMarkup(string text)
    {
        string removedText = "";
        char[] textArray = text.ToCharArray();

        for(int i = 0; i < textArray.Length; i++)
        {
            if(ValidateCloseMarkup(text, i))
            {
                int indexOfEnd = text.IndexOf(closeFormatTagEnd, i + 1);
                if(indexOfEnd != -1)
                {
                    int lengthOfTag = indexOfEnd - i + 1;
                    i += lengthOfTag;
                    continue;
                }
            }
                removedText += textArray[i];
        }
        Debug.Log($"Removed close makrup text {removedText}");
        return removedText;
    }

    virtual public string RemoveFormatTagText(string text)
    {
        //Need open and close
        //Debug.Log($"Incoming text:{text}");
        string excludedMarkupText = "";
        char[] textArray = text.ToCharArray();
        bool isReadingTag = false;

        for(int i = 0; i < textArray.Length; i++)
        {
            if(i + 1 < textArray.Length && textArray[i].ToString() == openFormatTagStart) //Skip over beginning of tag
            {
                int indexOfEnd = text.IndexOf(openFormatTagEnd, i + 1);
                if(indexOfEnd != -1 )
                {
                    string tryTag = text.Substring(i + 1, indexOfEnd - 2 - i);
                    char tryChar = text.Substring(indexOfEnd - 1, 1).ToCharArray()[0];

                    if(ValidateParameter(tryTag) && tryChar == markupCharacter)
                    {
                        //Debug.Log($"Reading tag as:{parameterType}");
                        isReadingTag = true;
                        continue;
                    }
                    else
                    {
                        Debug.Log($"Failed to recognize {tryTag} as {parameterType} or {tryChar} as {markupCharacter}");
                    }
                }
            }

            if(!isReadingTag)
            {
                excludedMarkupText += textArray[i];
            }


            if(textArray[i] == '>') //Skip over ending of tag
            {
                if(isReadingTag)
                {
                    isReadingTag = false;
                    continue;
                }
            }

            
            //Debug.Log($"Exluded markup text:{excludedMarkupText}");
        }
        return excludedMarkupText;
    }

	#endregion

	#region LOGIC
    //Move this to markupData
	virtual public bool HandleMarkup(DialogueManager dialogueManager, string text)
    {
        if(ValidateOpenMarkup(text))
        {
            //Debug.Log("Recognize markup");
            HandleOpenMarkupLogic(dialogueManager,text);
            return true;
        }
        else if(ValidateCloseMarkup(text))
        {
            HandleCloseMarkupLogic(dialogueManager, text);
            return true;
        }
        return false;
    }
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

    virtual public void HandleOpenMarkupLogic(DialogueManager dialogueManager, string text)
    {
        Debug.Log("Opening logic");
            if(hasParameters && GetParameterText(text) != null)
            { 
                lastStoredParameter = GetParameterText(text);

                //Run logic with parameter based on enum type

                switch(parameterType)
                {
                    case DialogueMarkupParameterType.INT:
                    break;
                    case DialogueMarkupParameterType.FLOAT:
                    break;
                    case DialogueMarkupParameterType.BOOL:
                    break;
                    case DialogueMarkupParameterType.CHAR:
                    break;
                    case DialogueMarkupParameterType.STRING:
                    break;
                    case DialogueMarkupParameterType.DOUBLE:
                    break;
                }
            }

        lastStoredParameter = "";
    }

    virtual public void HandleCloseMarkupLogic(DialogueManager dialogueManager, string text)
    {
        //Debug.Log("Closing markup");
    }
    #endregion
}
