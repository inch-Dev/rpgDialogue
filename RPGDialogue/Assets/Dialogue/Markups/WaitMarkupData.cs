using UnityEngine;


[CreateAssetMenu(fileName = "MarkupData", menuName = "ScriptableObjects/DialogueObjects/DialogueMarkups/DialogueMarkup/MarkupData/WaitMarkupData", order = 1)]
public class WaitMarkupData : MarkupData
{
    [SerializeField] float time;

	public override void OpenLogic()
	{
		Debug.Log("Found wait markupData");
	}

}
