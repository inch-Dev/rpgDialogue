using System.Buffers.Text;
using UnityEngine;

[CreateAssetMenu(fileName = "DM_Expression", menuName = "ScriptableObjects/DialogueObjects/DialogueMarkups/DM_Expression", order = 3)]
public class DM_Expression : DialogueMarkup
{
    public override void HandleOpenMarkupLogic(DialogueManager dialogueManager, string text)
    {
        Debug.Log("Running logic for expression change");
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
                            dialogueManager.ChangeCurEpxression(DialogueEpressionID.ANGRY);
                            break;
                            case "H":
                            case "h":
                            dialogueManager.ChangeCurEpxression(DialogueEpressionID.HAPPY);
                            break;
                            case "N":
                            case "n":
                            dialogueManager.ChangeCurEpxression(DialogueEpressionID.NEUTRAL);
                            break;
                            case "S":
                            case "s":
                            dialogueManager.ChangeCurEpxression(DialogueEpressionID.SAD);
                            break;

                        }
                        break;
                }
            }
        lastStoredParameter = "";
    }
}
