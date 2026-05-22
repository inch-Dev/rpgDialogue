using System.Data.Common;
using NaughtyAttributes;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

public class DialogueMarkup : MonoBehaviour
{
    [SerializeField] protected string formatTagStart;
    [SerializeField] protected string formatTagEnd;
    [SerializeField] protected bool hasParameter;
    [ShowIf("hasParameter")]
    [SerializeField] protected DialogueMarkupParameterType parameterType;

    virtual public string HandleMarkup(string text) //If it has the full tag then send event
    {
        //Return markup to check and null if not found?
        string handledMarkupText = "";

        if(RecognizeMarkup(text))
        {
            Debug.Log($"Recognized markup in {text}");
            handledMarkupText = GetRemovedText(text);

            return handledMarkupText;
        }

        return text;
    }

    virtual public string GetRemovedText(string text)
    {
        string excludedMarkupText = "";
        char[] textArray = text.ToCharArray();
        bool isReadingTag = false;


        Debug.Log($"Size of new text:{text.Length}");
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
        Debug.Log($"Getting {excludedMarkupText}");
        return excludedMarkupText;
    }

    virtual public bool RecognizeMarkup(string text)
    {
        bool containsMarkup = false;
        if(text.Contains(formatTagStart) && text.Contains(formatTagEnd))
        {
            containsMarkup = true;
            if(hasParameter)
            {
                //Isolate string format tags and parameter
                string parameterText = "";
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

                //Validate parameter is of right primitive parameter type;
                switch(parameterType)
                {
                    case DialogueMarkupParameterType.INT:
                        if(int.TryParse(parameterText, out int intResult))
                        {
                            if(intResult == null)
                            {
                                containsMarkup = false;
                            }
                            else
                                containsMarkup = true;
                        }
                        break;

                    case DialogueMarkupParameterType.FLOAT:
                        if(float.TryParse(parameterText, out float floatResult))
                        {
                            if(floatResult == null)
                            {
                                containsMarkup = false;
                            }
                            else
                                containsMarkup = true;
                        }
                        break;          
                    case DialogueMarkupParameterType.BOOL:
                        if(bool.TryParse(parameterText, out bool boolResult))
                        {
                            if(boolResult == null)
                            {
                                containsMarkup = false;
                            }
                            else
                                containsMarkup = true;
                        }
                        break;
                    case DialogueMarkupParameterType.CHAR:
                        if(char.TryParse(parameterText, out  char charResult))
                        {
                            if(charResult == null)
                            {
                                containsMarkup = false;
                            }
                            else
                            {
                                containsMarkup = true;
                            }
                        }
                        break;      
                    case DialogueMarkupParameterType.STRING:
                        containsMarkup = true;
                        break;
                    case DialogueMarkupParameterType.DOUBLE:
                        if(double.TryParse(parameterText, out double doubleResult))
                        {
                            if(doubleResult == null)
                            {
                                containsMarkup = false;
                            }

                            else
                            {
                                containsMarkup = true;
                            }
                        }
                        break;         
                }

            }
        }

        return containsMarkup;
    }
}
