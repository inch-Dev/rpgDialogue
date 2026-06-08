using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
[CreateAssetMenu(fileName = "Dialogue", menuName = "ScriptableObjects/DialogueObjects/Dialogue", order = 1)]
public class Dialogue : ScriptableObject
{
    [SerializeField] public bool hasSpeaker;
    [ShowIf("hasSpeaker")] public DialogueSpeaker speaker;
    [ShowIf("hasSpeaker")] public DialogueExpression startingExpression;
    [SerializeField] public string[] dialogueLines;
    [SerializeField] public bool hasTypeWriterEffect = true;
}
