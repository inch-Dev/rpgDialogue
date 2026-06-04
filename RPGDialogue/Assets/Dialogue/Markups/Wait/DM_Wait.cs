using System;
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEditor.U2D.Animation;
using UnityEngine;

[CreateAssetMenu(fileName = "DM_Wait", menuName = "ScriptableObjects/DialogueObjects/DialogueMarkups/DM_Wait", order = 2)]
public class DM_Wait : DialogueMarkup
{
    public override void HandleOpenMarkupLogic(DialogueManager dialogueManager, string text)
    {
            if(hasParameter && GetValidParameterText(text) != null)
            { 
                lastStoredParameter = GetValidParameterText(text);


                //Run logic with parameter based on enum type

                switch(parameterType)
                {
                    case DialogueMarkupParameterType.INT:
                    break;
                    case DialogueMarkupParameterType.FLOAT:
                    if(RecognizeMarkupAtBeginning(text))
                    {
                        dialogueManager.curStartWaitTime = float.Parse(lastStoredParameter);
                        Debug.Log($"Setting start wait time to {lastStoredParameter}");
                    }
                    else if(RecognizeMarkupAtEnd(text))
                    {
                        dialogueManager.curEndWaitTime = float.Parse(lastStoredParameter);
                        Debug.Log($"Setting end wait time to {lastStoredParameter}");
                    }
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

}
