using System;
using System.Data.Common;
using NaughtyAttributes;
using NUnit.Compatibility;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;

[CreateAssetMenu(fileName = "Dialogue", menuName = "ScriptableObjects/DialogueObjects/DialogueMarkups/DialogueMarkup", order = 1)]
public class DialogueMarkup : ScriptableObject
{
    [SerializeField] protected string formatTagStart;
    [SerializeField] protected string formatTagEnd;
    [SerializeField] protected bool hasParameter;
    [ShowIf("hasParameter")]
    [SerializeField] protected DialogueMarkupParameterType parameterType;

    string lastStoredParameter;
    DialogueManager dialogueManager;

    void Awake()
    {
        dialogueManager = DialogueManager.Instance;
    }

    //Call this when recognizing markup
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
            case DialogueMarkupParameterType.STRING:
                lastStoredParameter = text;
            return true;
        }
        return false;
    }
    
    virtual public string GetParameterText(string text)
    {
        string parameterText = "";

        if(!hasParameter)
            return null;

        //Isolate string format tags and parameter
        bool shoudlAddToMarkupText = false;

        //Identify first char of format tag start and format tag end
        char formatTagStartFirstChar = formatTagStart.ToCharArray()[0];
        char formatTagEndFirstChar = formatTagEnd.ToCharArray()[0];

        //Debug.Log($"Starting char:{formatTagStartFirstChar}, Ending char:{formatTagEndFirstChar}");

        char[] textArray = text.ToCharArray();

        //Get parameter
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
                i += formatTagStart.Length - 1;
                continue;
            }
            if(shoudlAddToMarkupText)
            {
                parameterText += textArray[i];
            }
                    
        }
        Debug.Log($"Parameter text:{parameterText}");
        if(RecognizeParameterAsParameterType(parameterText))
            return parameterText;
        else
            return null;
    }
    virtual public string GetRemovedMarkupText(string text) //If it has the full tag then send event
    {
        //Return markup to check and null if not found?
        string handledMarkupText = "";

        if(RecognizeMarkup(text))
        {
            handledMarkupText = RemoveMarkupText(text);

            return handledMarkupText;
        }

        return text;
    }
    
    virtual public string RemoveMarkupText(string text)
    {
        string excludedMarkupText = "";
        char[] textArray = text.ToCharArray();
        bool isReadingTag = false;

        for(int i = 0; i < textArray.Length; i++)
        {
            if(i + formatTagStart.Length - 1 <= textArray.Length) //Skip over beginning tag
            {
                string tryTag = text.Substring(i, formatTagStart.Length);

                if(tryTag == formatTagStart)
                {
                    isReadingTag = true;
                    i += formatTagStart.Length - 1;
                    continue;
                }
            }


            if(i + formatTagEnd.Length <= textArray.Length) //Skip over closing tag
            {
                string tryTag = text.Substring(i, formatTagEnd.Length);

                if(tryTag == formatTagEnd)
                {
                    isReadingTag = false;
                    i += formatTagEnd.Length - 1;
                    continue;
                }
            }

            if(!isReadingTag)
            {
                excludedMarkupText += textArray[i];
            }
        }
        return excludedMarkupText;
    }

    virtual public bool RecognizeMarkup(string text)
    {
        //Debug.Log($"Recognizing from {text}");
        bool containsMarkup = false;
        if(text.Contains(formatTagStart) && text.Contains(formatTagEnd))
        {
            containsMarkup = true;
            if(hasParameter && GetParameterText(text) != null)
            {
                containsMarkup = true;
            }
        }

        return containsMarkup;
    }

    virtual public void HandleMarkupLogic(string text)
    {
        if(RecognizeMarkup(text))
        {

            Debug.Log("Recognized a markup for the first time!");   
            if(hasParameter && GetParameterText(text) != null)
            { 
                lastStoredParameter = GetParameterText(text);

                Debug.Log($"Last stored parameter:{lastStoredParameter}");
            }

        }

        lastStoredParameter = "";
    }
}
