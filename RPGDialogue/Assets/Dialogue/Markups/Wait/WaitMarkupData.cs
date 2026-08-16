using UnityEngine;


[CreateAssetMenu(fileName = "MarkupData", menuName = "ScriptableObjects/DialogueObjects/DialogueMarkups/DialogueMarkup/MarkupData/WaitMarkupData", order = 1)]
public class WaitMarkupData : MarkupData
{
    [SerializeField] float time;

	public override void Open(DialogueMarkup markup, string rawText, DialogueCall callType)
	{
		base.Open(markup, rawText);
	}

	public override void Close(DialogueMarkup markup, string rawText, DialogueCall callType)
	{
		base.Close(markup, rawText);
	}

}
