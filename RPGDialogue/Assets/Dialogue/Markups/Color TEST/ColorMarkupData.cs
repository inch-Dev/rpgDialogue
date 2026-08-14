using UnityEngine;



[CreateAssetMenu(fileName = "ColorMarkupData", menuName = "ScriptableObjects/DialogueObjects/DialogueMarkups/MarkupDatas/ColorMarkupData", order = 4)]
public class ColorMarkupData : MarkupData
{
	public override void OpenLogic(DialogueMarkup markup, string rawText)
	{
		DialogueUI.Instance.TextOperation(DialogueManager.Instance.GetDisplayIndexRange(markup.ParseAppliedText(rawText)));
	}

	public override void CloseLogic(DialogueMarkup markup, string rawText)
	{
		
	}
}
