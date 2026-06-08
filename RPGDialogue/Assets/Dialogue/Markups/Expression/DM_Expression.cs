using System.Buffers.Text;
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

                switch(parameterType)
                {
                    case DialogueMarkupParameterType.CHAR:
                    switch(lastStoredParameter)
                        {
                            case "A":
                            case "a":
                            dialogueManager.ChangeCurExpression(DialogueEpressionID.ANGRY);
                            break;
                            case "H":
                            case "h":
                            dialogueManager.ChangeCurExpression(DialogueEpressionID.HAPPY);
                            break;
                            case "N":
                            case "n":
                            dialogueManager.ChangeCurExpression(DialogueEpressionID.NEUTRAL);
                            break;
                            case "S":
                            case "s":
                            dialogueManager.ChangeCurExpression(DialogueEpressionID.SAD);
                            break;

                        }
                        break;
                }
            }
        lastStoredParameter = "";
    }
}
