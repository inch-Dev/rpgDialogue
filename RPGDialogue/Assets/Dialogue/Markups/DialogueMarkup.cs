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

[CreateAssetMenu(fileName = "Dialogue", menuName = "ScriptableObjects/DialogueObjects/DialogueMarkups/DialogueMarkup", order = 1)]
public class DialogueMarkup : ScriptableObject
{
    [SerializeField] Dictionary<string, dynamic> parameterDictionary;
    [SerializeField] protected char markupCharacter;
    protected string openFormatTagStart = "<";
    protected string openFormatTagEnd = ">";
    protected string closeFormatTagStart = "</";
    protected string closeFormatTagEnd = ">";
    [SerializeField] protected bool hasParameter;
    [ShowIf("hasParameter")]
    [EnumFlags][SerializeField] protected DialogueMarkupParameterType parameterType;
    [SerializeField] protected bool hasSpecificParameters;
    [ShowIf("hasSpecificParameters")]
    [SerializeField] List<String> validParameters;
    protected string lastStoredParameter;

    void OnValidate()
    {
        //Reset values
        openFormatTagEnd = ">";
        closeFormatTagEnd = ">";

        openFormatTagEnd = markupCharacter.ToString() + openFormatTagEnd;
        //closeFormatTagEnd = markupCharacter.ToString() + closeFormatTagEnd;
    }
    virtual public bool RecognizeParameterAsParameterType(string text)
    {
        switch(parameterType)
        {
            case DialogueMarkupParameterType.INT:
            if(int.TryParse(text, out int intResult))
                {
                    lastStoredParameter = text;
                    return true;
                }
                break;
            case DialogueMarkupParameterType.FLOAT:
            if(float.TryParse(text, out float floatResult))
                {
                    lastStoredParameter = text;
                    return true;
                }
                break;
            case DialogueMarkupParameterType.BOOL:
            if(bool.TryParse(text, out bool boolResult))
                {
                    lastStoredParameter = text;
                    return true;
                }
                break;
            case DialogueMarkupParameterType.CHAR:
            if(char.TryParse(text, out char charResult))
                {
                    lastStoredParameter = text;
                    return true;
                }
                break;
            case DialogueMarkupParameterType.DOUBLE:
            if(double.TryParse(text, out double doubleResult))
                {
                    lastStoredParameter = text;
                    return true;
                }
                break;
            case DialogueMarkupParameterType.EXPRESSION:
            if(Enum.TryParse(text, out DialogueExpressionID expressionResult))
                {
                  lastStoredParameter = text;
                  return true;  
                }
                break;
            case DialogueMarkupParameterType.SPEED:
                if(Enum.TryParse(text, out DialogueSpeedID speedResult))
                {
                    lastStoredParameter = text;
                    return true;
                }
                else
                {
                    Debug.Log($"Couldn't find speed from {text}");
                }
                break;

            case DialogueMarkupParameterType.STRING:
                lastStoredParameter = text;
                return true;
        }
        return false;
    }
    virtual public string GetValidParameterText(string text)
    {
        string parameterText = "";

        if(!hasParameter)
            return null;

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

        if(RecognizeParameterAsParameterType(parameterText))
            return parameterText;
        else
            return null;
    }
    
    virtual public string RemoveFormatTag(string text)
    {   
        string excludedMarkupText = "";
        if(!RecognizeMarkup(text))
            return text;
        excludedMarkupText = RemoveFormatTagText(text);
        return excludedMarkupText;
    }

    virtual public string RemoveFormatTagText(string text)
    {
        //Debug.Log($"Incoming text:{text}");
        string excludedMarkupText = "";
        char[] textArray = text.ToCharArray();
        bool isReadingTag = false;

        for(int i = 0; i < textArray.Length; i++)
        {
            if(i + 1 < textArray.Length && textArray[i] == '<') //Skip over beginning of tag
            {
                int indexOfEnd = text.IndexOf('>', i + 1);
                if(indexOfEnd != -1 )
                {
                    string tryTag = text.Substring(i + 1, indexOfEnd - 2 - i);
                    char tryChar = text.Substring(indexOfEnd - 1, 1).ToCharArray()[0];

                    if(RecognizeParameterAsParameterType(tryTag) && tryChar == markupCharacter)
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
    virtual public bool HandleMarkup(DialogueManager dialogueManager, string text)
    {
        if(RecognizeOpenFormatTag(text))
        {
            //Debug.Log("Recognize markup");
            HandleOpenMarkupLogic(dialogueManager,text);
            return true;
        }
        else if(RecognizeCloseFormatTag(text))
        {
            HandleCloseMarkupLogic(dialogueManager, text);
            return true;
        }
        return false;
    }

    virtual public bool RecognizeMarkup(string text)
    {
        bool isMarkup = false;
        if(RecognizeOpenFormatTag(text))
        isMarkup = true;
        if(RecognizeCloseFormatTag(text))
        isMarkup = true;

        //Debug.Log($"Is markup:{isMarkup}");
        return isMarkup;
    }

    virtual public bool RecognizeOpenFormatTag(string text)
    {
        bool containsOpenFormatTag = false;
        if(text.Contains(openFormatTagStart) && text.Contains(openFormatTagEnd))
        {
            containsOpenFormatTag = true;
            if(hasParameter && GetValidParameterText(text) != null)
            {
                containsOpenFormatTag = true;
                //Debug.Log("Contains parameter");
            }
        }
        return containsOpenFormatTag;
    }
    
    virtual public bool RecognizeCloseFormatTag(string text)
    {
        bool containsCloseFormatTag = false;
        if(text.Contains(closeFormatTagStart) && text.Contains(closeFormatTagEnd))
        {
            containsCloseFormatTag = true;
            if(hasParameter && GetValidParameterText(text) != null)
            {
                containsCloseFormatTag = true;
            }
        }

        return containsCloseFormatTag;
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
            if(hasParameter && GetValidParameterText(text) != null)
            { 
                lastStoredParameter = GetValidParameterText(text);

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
        Debug.Log("Closing makrup");
    }
}
