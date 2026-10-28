using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ColorMarkupData", menuName = "ScriptableObjects/DialogueObjects/DialogueMarkups/MarkupDatas/ExpressionMarkupData", order = 3)]
public class ExpressionMarkupData : MarkupData
{
	[SerializeField] DialogueExpressionID id;
	public override void Open(DialogueMarkup markup, string rawText, DialogueCall callType)
	{
		base.Open(markup, rawText, callType);
	}

	public override void OpenDelta(DialogueMarkup markup, string deltaText)
	{
		DialogueManager.Instance.SetExpression(id);
		Debug.Log($"Setting expression to {id}");
		//Debug.Log("Opened");
		//Debug.Log($"Markup applied text is: {markup.ParseAppliedText(rawText, DialogueCall.DELTA)}");
		//DialogueUI.Instance.TextOperation(DialogueManager.Instance.GetDisplayIndexRange(markup.ParseAppliedText(rawText, DialogueCall.DELTA)));
	}

	public override void Close(DialogueMarkup markup, string rawText, DialogueCall callType)
	{
		base.Close(markup, rawText, callType);
	}

	public override void Close(DialogueMarkup markup, string rawText)
	{
		DialogueManager.Instance.SetExpression();
	}

	public override void CloseDelta(DialogueMarkup markup, string deltaText)
	{
		DialogueManager.Instance.SetExpression();

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



		List<Vector2Int> displayRanges = DialogueManager.Instance.GetDisplayIndexRanges(appliedText);
		foreach (Vector2Int range in displayRanges)
		{
			Debug.Log(range);
		}
		//Debug.Log("Continue!");
		//Debug.Log($"Markup applied text is: {markup.ParseAppliedText(rawText, DialogueCall.DELTA)}");
	}
}
