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


	public Dialogue(bool hasSpeaker, DialogueSpeaker speaker, DialogueExpression startingExpression, string[] dialogueLines, bool hasTypeWriterEffect)
	{
		this.hasSpeaker = hasSpeaker;
		this.speaker = speaker;
		this.startingExpression = startingExpression;
		this.dialogueLines = dialogueLines;
		this.hasTypeWriterEffect = hasTypeWriterEffect;
	}


	public Dialogue(DialogueSpeaker speaker, DialogueExpression startingExpression, string[] dialogueLines, bool hasTypeWriterEffect)
	{
		this.hasSpeaker = true;
		this.speaker = speaker;
		this.startingExpression = startingExpression;
		this.dialogueLines = dialogueLines;
		this.hasTypeWriterEffect |= hasTypeWriterEffect;
	}

	public Dialogue(string[] dialogueLines, bool hasTypeWriterEffect)
	{
		this.hasSpeaker = false;
		this.speaker = null;
		this.startingExpression = null;
		this.dialogueLines = dialogueLines;
		this.hasTypeWriterEffect = hasTypeWriterEffect;
	}
}
