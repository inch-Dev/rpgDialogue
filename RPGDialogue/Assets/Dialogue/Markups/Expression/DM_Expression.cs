
using System;
using UnityEngine;
[CreateAssetMenu(fileName = "DM_Expression", menuName = "ScriptableObjects/DialogueObjects/DialogueMarkups/DM_Expression", order = 3)]
public class DM_Expression : DialogueMarkup
{
    public override void HandleOpenMarkupLogic(DialogueManager dialogueManager, string text)
    {
            if(hasParameter && GetParameterText(text) != null)
            { 
                lastStoredParameter = GetParameterText(text);

                //Run logic with parameter based on enum type

                if(Enum.TryParse(lastStoredParameter, out DialogueExpressionID expressionResult))
                {
                    dialogueManager.ChangeExpression(expressionResult);
                }
            }
        lastStoredParameter = "";
    }
}
