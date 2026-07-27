using System;
using System.ComponentModel;
using System.Data.Common;
using NaughtyAttributes;
using NUnit.Compatibility;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using UnityEngine.UI;
using UnityEngine.Rendering;
using JetBrains.Annotations;

[CreateAssetMenu(fileName = "Dialogue", menuName = "ScriptableObjects/DialogueObjects/DialogueMarkups/DialogueMarkup", order = 1)]
public class DialogueMarkup : ScriptableObject
{
    [SerializeField] Dictionary<string, dynamic> parameterDictionary;
    [SerializeField] protected char markupCharacter;
    protected string openFormatTagStart = "{";
    protected string openFormatTagEnd = "}";
    protected string closeFormatTagStart = "{/";
    protected string closeFormatTagEnd = "}";
    [SerializeField] protected bool hasParameter;
    [ShowIf("hasParameter")]
    [SerializeField] protected DialogueMarkupParameterType parameterType;
    [SerializeField] protected bool hasSpecificParameters;
    [ShowIf("hasSpecificParameters")]
    [SerializeField] List<String> validParameters;
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
    virtual public string GetParameterText(string text)
    {
        string parameterText = "";

        //Check for markup character

        if(!hasParameter)
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
        
        //If it is a valid parameter
        if(hasSpecificParameters)
        {
            bool matchesValidParameter = false;
            for(int i = 0; i < validParameters.Count; i++)
            {
                if(validParameters[i] == parameterText)
                {
                    matchesValidParameter = true;
                }
            }

            if(!matchesValidParameter)
                return null;
        }

        if(ValidateParameter(parameterText))
            return parameterText;
        else
            return null;
    }
    
    virtual public string GetMarkupText(string text, int index)
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

    virtual public string GetMarkupText(string text)
    {
        string markupText = "";
        char[] textArray = text.ToCharArray();

        if(!ValidateMarkup(text))
            return null;
        
        for(int i = 0; i < text.Length; i++)
        {
            if(GetMarkupText(text, i) != null)
            {
                markupText = GetMarkupText(text, i);
                return markupText;
            }
        }
        //Debug.Log($"Found markup {markupText}");
        return null;
    }
    #endregion
    
    
    #region VALIDATE INSTANCE
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
            if(hasParameter && GetParameterText(text) != null)
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
        Debug.Log("Closing markup");
    }
}
