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
    public override void HandleMarkupLogic(DialogueManager dialogueManager, string text)
    {

        //Track if Tag is at the end of a string????
       if(RecognizeMarkup(text))
        {

            //Debug.Log("Recognized a markup for the first time!");   
            if(hasParameter && GetValidParameterText(text) != null)
            { 
                lastStoredParameter = GetValidParameterText(text);


                //Run logic with parameter based on enum type

                switch(parameterType)
                {
                    case DialogueMarkupParameterType.INT:
                    break;
                    case DialogueMarkupParameterType.FLOAT:
                    dialogueManager.curStartWaitTime = float.Parse(lastStoredParameter);
                    Debug.Log($"Setting wait time to {dialogueManager.curStartWaitTime}");
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

        }

        lastStoredParameter = "";
    }

}
