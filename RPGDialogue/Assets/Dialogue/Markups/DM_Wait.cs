using System;
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEditor.U2D.Animation;
using UnityEngine;

public class DM_Wait : DialogueMarkup
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override bool MarkupRecognition(string text) //Convert the isolating parameter function to the base?
    {
        //Return markup to check and null if not found?
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

                Debug.Log($"Starting char:{formatTagStartFirstChar}, Ending char:{formatTagEndFirstChar}");

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

            //Send some sort of event
        }

        return containsMarkup;
    }
}
