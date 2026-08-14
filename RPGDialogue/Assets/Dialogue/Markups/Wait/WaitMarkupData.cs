using UnityEngine;


[CreateAssetMenu(fileName = "MarkupData", menuName = "ScriptableObjects/DialogueObjects/DialogueMarkups/DialogueMarkup/MarkupData/WaitMarkupData", order = 1)]
public class WaitMarkupData : MarkupData
{
    [SerializeField] float time;

	public override void OpenLogic(DialogueMarkup markup)
	{
		base.OpenLogic(markup);
	}

	public override void CloseLogic(DialogueMarkup markup)
	{
		base.CloseLogic(markup);
	}

}
