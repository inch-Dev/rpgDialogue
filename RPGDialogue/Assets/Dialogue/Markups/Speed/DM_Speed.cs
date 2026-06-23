using UnityEngine;

[CreateAssetMenu(fileName = "DM_Speed", menuName = "ScriptableObjects/DialogueObjects/DialogueMarkups/DM_Speed", order = 4)]
public class DM_Speed : DialogueMarkup
{
    public override void HandleOpenMarkupLogic(DialogueManager dialogueManager, string text)
    {
        base.HandleOpenMarkupLogic(dialogueManager, text);
    }

    public override void HandleCloseMarkupLogic(DialogueManager dialogueManager, string text)
    {
        base.HandleCloseMarkupLogic(dialogueManager, text);
    }
}
