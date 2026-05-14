using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "DS", menuName = "ScriptableObjects/DialogueObjects/DialogueSpeaker", order = 2)]
public class DialogueSpeaker : ScriptableObject
{
    //Has expressions
    [SerializeField] public List<DialogueExpression> dialogueExpressions;
    [SerializeField] public string speakerName;
    [SerializeField] public bool shouldAnimate;
}
