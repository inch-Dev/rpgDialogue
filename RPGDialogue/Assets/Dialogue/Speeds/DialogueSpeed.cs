using UnityEngine;

[CreateAssetMenu(fileName = "DS", menuName = "ScriptableObjects/DialogueObjects/DialogueSpeed", order = 5)]


public class DialogueSpeed : ScriptableObject
{
    public DialogueSpeedID id;
    public int charWaitFrames;
    public int lineWaitFrames;
}
