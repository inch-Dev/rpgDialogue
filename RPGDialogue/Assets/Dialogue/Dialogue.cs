using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Dialogue", menuName = "ScriptableObjects/DialogueObjects/Dialogue", order = 1)]
public class Dialogue : ScriptableObject
{
    [SerializeField] public DialogueSpeaker speaker;
    [SerializeField] public DialogueExpression startingExpression;
    [SerializeField] public List<string> dialogueLines;
    [SerializeField] public bool hasTypeWriterEffect = true;
}
