using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Codice.Client.GameUI.Explorer;
using UnityEditor.Search;
using Codice.CM.Common.Update.Partial;
using UnityEditor.Build.Pipeline.Tasks;
using System.Linq;
using System.Security.Permissions;

[CustomEditor(typeof(Dialogue))]
public class DialogueEditor : Editor
{
	Dialogue thisDialogue;
	List<DialogueMarkup> markups = new List<DialogueMarkup>();
	List<MarkupData> markupDatas = new List<MarkupData>();

	DialogueMarkup selectedMarkup = null;
	MarkupData selectedMarkupData = null;

	TextField buttonFocusField;
	int buttonFocusIndex = -1;
	int buttonFocusCursorIndex = -1;
	int buttonFocusSelectIndex = -1;


	TextField focusField;
	int focusIndex = -1;

	public override VisualElement CreateInspectorGUI()
    {
		SetDialogue();
		
		VisualElement root = new VisualElement();

		var splitView = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Horizontal);
        root.Add(splitView);

		#region LEFT PANE
		VisualElement leftPane = new VisualElement();
		splitView.Add(leftPane);

        Label markupLabel = new Label("<b>Dialogue Markups");
        markupLabel.style.fontSize = 12;
        leftPane.Add(markupLabel);


		string[] allMarkupGuids = AssetDatabase.FindAssets("t:DialogueMarkup");
		markups.Clear();
		foreach (var guid in allMarkupGuids)
		{
			markups.Add(AssetDatabase.LoadAssetAtPath<DialogueMarkup>(AssetDatabase.GUIDToAssetPath(guid)));
		}
		var markupList = new ListView();
		SetMarkups(markupList);
        leftPane.Add(markupList);
		

        Label dataLabel = new Label("<b>Markup Datas");
        dataLabel.style.fontSize = 12;
        leftPane.Add(dataLabel);

        var markupDataList = new ListView();
		SetMarkupDatas(markupDataList);
		RefreshMarkupDatas(markupList, markupDataList);
        leftPane.Add(markupDataList);

		Label formatLabel = new Label("<b>Format Type");
        formatLabel.style.fontSize = 12;
		leftPane.Add(formatLabel);

		DropdownField formatTypeDropdown = new DropdownField(new List<string> { "Open", "Close" }, 0);
		leftPane.Add(formatTypeDropdown);
       
		Button markupButton = new Button();
        markupButton.text = "Add Markup";
        leftPane.Add(markupButton);
		#endregion

		#region RIGHT PANE
		VisualElement rightPane = new VisualElement();
        splitView.Add(rightPane);

        Label dialogueLabel = new Label("<b>Dialogue Lines");
        dialogueLabel.style.fontSize = 12;
        rightPane.Add(dialogueLabel);

        var dialogueLineList = new ListView() { itemsSource = thisDialogue.dialogueLines, fixedItemHeight = 24, showAddRemoveFooter = true, reorderable = true };
		SetDialogueLines(dialogueLineList);
		AddMarkup(markupButton, dialogueLineList);
		rightPane.Add(dialogueLineList);

        Label optionsLabel = new Label("<b>Dialogue Options");
        optionsLabel.style.fontSize = 12;
        rightPane.Add(optionsLabel);

        Toggle typewriterToggle = new Toggle();
        typewriterToggle.text = "Type Writer Effect";
		typewriterToggle.value = thisDialogue.hasTypeWriter;
		SetTypewriter(typewriterToggle);
        rightPane.Add(typewriterToggle);

		Toggle speakerToggle = new Toggle();
		speakerToggle.text = "Speaker";
		speakerToggle.value = thisDialogue.hasSpeaker;
		SetSpeaker(speakerToggle);
		rightPane.Add(speakerToggle);

        ObjectField speakerField = new ObjectField("Speaker");
        speakerField.objectType = typeof(DialogueSpeaker);
		speakerField.value = thisDialogue.speaker;
		SetSpeaker(speakerField);
        rightPane.Add(speakerField);

        ObjectField expressionField = new ObjectField("Starting Expression");
        expressionField.objectType = typeof(DialogueExpression);
		expressionField.value = thisDialogue.startExpression;
		SetExpression(expressionField);
        rightPane.Add(expressionField);

		#endregion

		return root;
	}

	#region GETTERS

	public (TextField field, int index) GetFocus()
	{
		if (focusField == null || focusField.panel == null)
			return (null, -1);
		return(focusField, focusIndex);
	}

	#endregion

	#region SETTERS

	void SetButtonFocus(TextField field)
	{
		if (field == null || field.panel == null)
		{
			buttonFocusField = null;
			buttonFocusIndex = -1;
			buttonFocusCursorIndex = -1;
			buttonFocusSelectIndex = -1;
			Debug.Log("Resetting BUTTON focus");
			return;
		}

		buttonFocusField = field;
		buttonFocusIndex = (int)field.userData;
		buttonFocusCursorIndex = field.cursorIndex;
		buttonFocusSelectIndex = field.selectIndex;

	}
	void SetFocus(TextField field)
	{
		field.UnregisterCallback<FocusInEvent>(SetFocus);
		field.RegisterCallback<FocusInEvent>(SetFocus);

		field.UnregisterCallback<FocusOutEvent>(SetFocus);
		field.RegisterCallback<FocusOutEvent>(SetFocus);
	}
	
	void SetFocus(FocusInEvent evt)
	{
		TextField field = (TextField)evt.currentTarget;
		focusField = field;
		focusIndex = (int)field.userData;

		SetButtonFocus(field);
	}

	void SetFocus(FocusOutEvent evt)
	{
		TextField field = (TextField)evt.currentTarget;
		if(field == focusField)
		{
			focusField = null;
			focusIndex = -1;
			Debug.Log("Resetting focus");
		}
	}

	void SetDialogue()
	{
		if (!(Dialogue)target)
			return;
		thisDialogue = (Dialogue)target;

	}
	void SetMarkups(ListView list)
	{
		list.makeItem = () => new Label();
		list.bindItem = (item, index) => { (item as Label).text = markups[index].name; };
		list.itemsSource = markups;
		list.selectionType = SelectionType.Single;

		list.itemsChosen += (selectedItems) =>
		{
			SetButtonFocus(null);
			foreach (DialogueMarkup markup in selectedItems)
			{
				selectedMarkup = markup;
			}
		};
	}

	void SetMarkupDatas(ListView list)
	{
		list.makeItem = () => new Label();
		list.bindItem = (item, index) => { (item as Label).text = markupDatas[index].name; };
		list.itemsSource = markupDatas;
		list.selectionType = SelectionType.Single;

		list.itemsChosen += (selectedItems) =>
		{
			SetButtonFocus(null);
			foreach(MarkupData markupData in selectedItems)
			{
				selectedMarkupData = markupData;
			}
		};

		list.selectionChanged += (selectedItems) =>
		{
			SetButtonFocus(null);
		};
	}

	void SetDialogueLines(ListView list)
	{
		list.onRemove = (baseListView) =>
		{
			if(focusIndex >= 0)
			{
				thisDialogue.dialogueLines.RemoveAt(focusIndex);

				//If removed don't store for markup button
				if(focusIndex == buttonFocusIndex)
				{
					SetButtonFocus(null);
				}
				focusIndex--;
			}

			else
			{
				thisDialogue.dialogueLines.RemoveAt(thisDialogue.dialogueLines.Count - 1);

				//If removed don't store for markup button
				if (thisDialogue.dialogueLines.Count - 1 == buttonFocusIndex)
				{
					SetButtonFocus(null);
				}
			}

			list.RefreshItems();
		};

		list.makeItem = () =>
		{
			TextField field = new TextField();
			field.RegisterValueChangedCallback(evt =>
			{
				if (field.userData is int index && index >= 0 && index < thisDialogue.dialogueLines.Count())
				{
					thisDialogue.dialogueLines[index] = evt.newValue;

				}
			});
			return field;
		};

		list.bindItem = (item,index) =>
		{
			TextField field = (TextField)item;
			field.userData = index;

			field.SetValueWithoutNotify(thisDialogue.dialogueLines[index]);

			field.selectAllOnFocus = false;
			field.selectAllOnMouseUp = false;

			SetFocus(field);

			if(index == focusIndex)
			{
				field.Focus();
				field.SelectRange(field.text.Length, field.text.Length);
			}
	
		};
	}

	#region SPEAKER
	void SetSpeaker(Toggle toggle)
    {
		toggle.RegisterCallback<ClickEvent>(evt =>
		{
			SetButtonFocus(null);
			thisDialogue.hasSpeaker = toggle.value;
		});
	}

    void SetSpeaker(ObjectField field)
    {
		field.RegisterCallback<ChangeEvent<Object>>(evt =>
		{
			SetButtonFocus(null);
			thisDialogue.speaker = (DialogueSpeaker)evt.newValue;
		});
	}

	#endregion

    void SetTypewriter(Toggle toggle)
    {
		toggle.RegisterCallback<ClickEvent>(evt =>
		{
			SetButtonFocus(null);
			thisDialogue.hasTypeWriter = toggle.value;
		});
	}

    void SetExpression(ObjectField field)
    {
		field.RegisterCallback<ChangeEvent<Object>>(evt =>
		{
			SetButtonFocus(null);
			thisDialogue.startExpression = (DialogueExpression)evt.newValue;
		});
	}

	#endregion


	void AddMarkup(Button button, ListView dialogueLinesList)
	{
		button.RegisterCallback<ClickEvent>(evt =>
		{
			Debug.Log("Button clicked");
			if (buttonFocusField == null || buttonFocusField.panel == null)
			{
				SetButtonFocus(null);
				return;
			}

			//If range of highlight selection
			if (buttonFocusSelectIndex != -1)
			{
				if (selectedMarkup && selectedMarkupData)
				{
					Vector2Int selectRange = new Vector2Int(buttonFocusSelectIndex, buttonFocusCursorIndex);
					thisDialogue.dialogueLines[buttonFocusIndex] = selectedMarkup.ApplyMarkup(thisDialogue.dialogueLines[buttonFocusIndex], selectRange, selectedMarkupData);
				}

				else if (selectedMarkup)
				{
					Vector2Int selectRange = new Vector2Int(buttonFocusSelectIndex, buttonFocusCursorIndex);
					thisDialogue.dialogueLines[buttonFocusIndex] = selectedMarkup.ApplyMarkup(thisDialogue.dialogueLines[buttonFocusIndex], selectRange);
				}
			}

			//If just cursor
			else if (buttonFocusCursorIndex != -1)
			{
				if (selectedMarkup && selectedMarkupData)
				{
					thisDialogue.dialogueLines[buttonFocusIndex] = selectedMarkup.ApplyMarkup(thisDialogue.dialogueLines[buttonFocusIndex], buttonFocusCursorIndex, selectedMarkupData);
				}

				else if(selectedMarkup)
				{
					thisDialogue.dialogueLines[buttonFocusIndex] = selectedMarkup.ApplyMarkup(thisDialogue.dialogueLines[buttonFocusIndex], buttonFocusCursorIndex);
				}

				Debug.Log("Working??");
			}

			dialogueLinesList.RefreshItems();

		});
	}
	void RefreshMarkupDatas(ListView markupList, ListView dataList)
	{
		markupList.itemsChosen += (selectedItems) =>
		{

		};

		markupList.selectionChanged += (selectedItems) =>
		{
			SetButtonFocus(null);
			foreach (DialogueMarkup dialogueMarkup in selectedItems)
			{

				selectedMarkupData = null;
				markupDatas.Clear();
				dataList.selectedIndex = -1;

				markupDatas.AddRange(dialogueMarkup.GetMarkupDatas());
				dataList.RefreshItems();


			}
		};
	}

}
