using UnityEngine;

[CreateAssetMenu(fileName = "DS", menuName = "ScriptableObjects/DialogueObjects/DialogueSpeed", order = 5)]


public class DialogueSpeed : ScriptableObject
{
    public DialogueSpeedID id;
    public string keyName;
    public float charWaitSeconds;
    public float lineWaitSeconds;
}
