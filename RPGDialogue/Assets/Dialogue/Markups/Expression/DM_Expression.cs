
using System;
using UnityEngine;
[CreateAssetMenu(fileName = "DM_Expression", menuName = "ScriptableObjects/DialogueObjects/DialogueMarkups/DM_Expression", order = 3)]
public class DM_Expression : DialogueMarkup
{
    public override void HandleOpenMarkupLogic(DialogueManager dialogueManager, string text)
    {
            if(hasParameter && GetValidParameterText(text) != null)
            { 
                lastStoredParameter = GetValidParameterText(text);

                //Run logic with parameter based on enum type

                Debug.Log($"Last stored expression parameter:{lastStoredParameter}");
                if(Enum.TryParse(lastStoredParameter, out DialogueExpressionID expressionResult))
                {
                    dialogueManager.ChangeCurExpression(expressionResult);
                    Debug.Log("Changing expression....");
                }
            }
        lastStoredParameter = "";
    }
}
