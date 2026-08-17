using UnityEngine;



[CreateAssetMenu(fileName = "ColorMarkupData", menuName = "ScriptableObjects/DialogueObjects/DialogueMarkups/MarkupDatas/ColorMarkupData", order = 4)]
public class ColorMarkupData : MarkupData
{

	public override void Open(DialogueMarkup markup, string rawText, DialogueCall callType)
	{
		base.Open(markup, rawText, callType);
	}
	public override void Open(DialogueMarkup markup, string rawText)
	{
	}

	public override void OpenDelta(DialogueMarkup markup, string rawText)
	{
		//Debug.Log($"Markup applied text is: {markup.ParseAppliedText(rawText, DialogueCall.DELTA)}");
		//DialogueUI.Instance.TextOperation(DialogueManager.Instance.GetDisplayIndexRange(markup.ParseAppliedText(rawText, DialogueCall.DELTA)));
	}

	public override void Close(DialogueMarkup markup, string rawText, DialogueCall callType)
	{
		
	}

	public override void Close(DialogueMarkup markup, string rawText)
	{
		
	}

	public override void CloseDelta(DialogueMarkup markup, string rawText)
	{
		//Debug.Log($"Markup applied text is: {markup.ParseAppliedText(rawText, DialogueCall.DELTA)}");
		//DialogueUI.Instance.TextOperation(DialogueManager.Instance.GetDisplayIndexRange(markup.ParseAppliedText(rawText, DialogueCall.DELTA)));
	}
}
