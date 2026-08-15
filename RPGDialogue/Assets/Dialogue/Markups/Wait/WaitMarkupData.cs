using UnityEngine;


[CreateAssetMenu(fileName = "MarkupData", menuName = "ScriptableObjects/DialogueObjects/DialogueMarkups/DialogueMarkup/MarkupData/WaitMarkupData", order = 1)]
public class WaitMarkupData : MarkupData
{
    [SerializeField] float time;

	public override void OpenLogic(DialogueMarkup markup, string rawText)
	{
		base.OpenLogic(markup, rawText);
	}

	public override void CloseLogic(DialogueMarkup markup, string rawText)
	{
		base.CloseLogic(markup, rawText);
	}

}
