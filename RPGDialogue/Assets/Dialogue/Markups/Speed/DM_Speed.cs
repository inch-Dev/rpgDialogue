using UnityEngine;
using System;
[CreateAssetMenu(fileName = "DM_Speed", menuName = "ScriptableObjects/DialogueObjects/DialogueMarkups/DM_Speed", order = 4)]
public class DM_Speed : DialogueMarkup
{
    public override void HandleOpenMarkupLogic(DialogueManager dialogueManager, string text)
    {
        if(hasParameter && GetParameterText(text) != null)
            { 
                lastStoredParameter = GetParameterText(text);

                //Run logic with parameter based on enum type

                if(Enum.TryParse(lastStoredParameter, out DialogueSpeedID speedResult))
                {
                    dialogueManager.ChangeSpeed(speedResult);
                }
            }
        lastStoredParameter = "";
    }

    public override void HandleCloseMarkupLogic(DialogueManager dialogueManager, string text)
    {
        base.HandleCloseMarkupLogic(dialogueManager, text);
    }
}
