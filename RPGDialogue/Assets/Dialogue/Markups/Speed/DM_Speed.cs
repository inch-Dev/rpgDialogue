using UnityEngine;
using System;
[CreateAssetMenu(fileName = "DM_Speed", menuName = "ScriptableObjects/DialogueObjects/DialogueMarkups/DM_Speed", order = 4)]
public class DM_Speed : DialogueMarkup
{
    public override void OpenLogic(DialogueManager dialogueManager, string text)
    {
        //if(hasParameters && GetParameterText(text) != null)
        //    { 
        //        lastStoredParameter = GetParameterText(text);

        //        //Run logic with parameter based on enum type

        //        if(Enum.TryParse(lastStoredParameter, out DialogueSpeedID speedResult))
        //        {
        //            dialogueManager.ChangeSpeed(speedResult);
        //        }
        //    }
        //lastStoredParameter = "";
    }

    public override void CloseLogic(DialogueManager dialogueManager, string text)
    {
        base.CloseLogic(dialogueManager, text);
    }
}
