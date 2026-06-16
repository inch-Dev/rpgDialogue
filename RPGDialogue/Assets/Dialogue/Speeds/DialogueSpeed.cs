using UnityEngine;

[CreateAssetMenu(fileName = "DM_Expression", menuName = "ScriptableObjects/DialogueObjects/DialogueSpeed", order = 5)]


public class DialogueSpeed : ScriptableObject
{
    DialogueSpeedID id;
    public int charWaitFrames;
    public int lineWaitFrames;
}
