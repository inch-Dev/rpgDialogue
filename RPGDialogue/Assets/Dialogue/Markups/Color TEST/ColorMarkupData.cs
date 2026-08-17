using UnityEngine;
using System.Collections.Generic;


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

	public override void OpenDelta(DialogueMarkup markup, string deltaText)
	{
		//Debug.Log("Opened");
		//Debug.Log($"Markup applied text is: {markup.ParseAppliedText(rawText, DialogueCall.DELTA)}");
		//DialogueUI.Instance.TextOperation(DialogueManager.Instance.GetDisplayIndexRange(markup.ParseAppliedText(rawText, DialogueCall.DELTA)));
	}

	public override void Close(DialogueMarkup markup, string rawText, DialogueCall callType)
	{
		
	}

	public override void Close(DialogueMarkup markup, string rawText)
	{
		
	}

	public override void CloseDelta(DialogueMarkup markup, string deltaText)
	{
		Debug.Log("Closing!S");
		List<string> appliedText = markup.ParseAppliedText(deltaText, DialogueCall.DELTA);
		foreach(string text in appliedText)
		{
			Debug.Log($"Applied text range:{text}");
		}


		//Debug.Log($"Markup applied text is: {markup.ParseAppliedText(rawText, DialogueCall.DELTA)}");
		//DialogueUI.Instance.TextOperation(DialogueManager.Instance.GetDisplayIndexRange(markup.ParseAppliedText(rawText, DialogueCall.DELTA)));
	}

	public override void Continue(DialogueMarkup markup, string deltaText)
	{
		List<string> appliedText = markup.ParseAppliedText(deltaText, DialogueCall.DELTA);
		foreach (string text in appliedText)
		{
			Debug.Log($"Applied text range:{text}");
		}

		//Debug.Log("Continue!");
		//Debug.Log($"Markup applied text is: {markup.ParseAppliedText(rawText, DialogueCall.DELTA)}");
	}
}
