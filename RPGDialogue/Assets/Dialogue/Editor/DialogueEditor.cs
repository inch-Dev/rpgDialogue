using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEditor.Search;
using System.Linq;
using UnityEngine.Rendering;
using System.Runtime.Serialization;

[CustomEditor(typeof(Dialogue))]
public class DialogueEditor : Editor
{
	SerializedObject serializedObject;
	Dialogue thisDialogue;

	SerializedProperty dialogueLines;
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
		serializedObject = new SerializedObject(target);
		SetObject();
		
		VisualElement root = new VisualElement();

		var splitView = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Horizontal);
        root.Add(splitView);

		#region LEFT PANE
		VisualElement leftPane = new VisualElement();
		SetFocusable(leftPane, false);
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
		List<string> dialogueLinesSource = new List<string>();
		SetDialogueLines(dialogueLinesSource);
        var dialogueLineList = new ListView() { itemsSource = dialogueLinesSource, fixedItemHeight = 24, showAddRemoveFooter = true, reorderable = true };
		SetDialogueLines(dialogueLineList);
		AddMarkup(markupButton, dialogueLineList);
		rightPane.Add(dialogueLineList);

        Label optionsLabel = new Label("<b>Dialogue Options");
        optionsLabel.style.fontSize = 12;
        rightPane.Add(optionsLabel);

        Toggle typewriterToggle = new Toggle();
        typewriterToggle.text = "Type Writer Effect";
		typewriterToggle.value = serializedObject.FindProperty("hasTypewriter").boolValue;
		SetTypewriter(typewriterToggle);
        rightPane.Add(typewriterToggle);

		Toggle speakerToggle = new Toggle();
		speakerToggle.text = "Speaker";
		speakerToggle.value = serializedObject.FindProperty("hasSpeaker").boolValue;
		SetSpeaker(speakerToggle);
		rightPane.Add(speakerToggle);

        ObjectField speakerField = new ObjectField("Speaker");
        speakerField.objectType = typeof(DialogueSpeaker);
		speakerField.value = serializedObject.FindProperty("speaker").objectReferenceValue;
		SetSpeaker(speakerField);
        rightPane.Add(speakerField);

        ObjectField expressionField = new ObjectField("Starting Expression");
        expressionField.objectType = typeof(DialogueExpression);
		expressionField.value = serializedObject.FindProperty("startExpression").objectReferenceValue;
		SetExpression(expressionField);
        rightPane.Add(expressionField);

		serializedObject.Update();
		serializedObject.ApplyModifiedProperties();
		//Debug.Log($"Applied: {serializedObject.ApplyModifiedProperties()}");
		EditorUtility.SetDirty(target);

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

	void SetFocusable(VisualElement element, bool focus)
	{
		element.RegisterCallback<PointerDownEvent>(evt =>
		{
			if ((evt.currentTarget as VisualElement) != null)
				(evt.currentTarget as VisualElement).focusable = focus;

			if ((evt.currentTarget as VisualElement)?.focusable == false)
			{
				(evt.currentTarget as VisualElement).focusController.IgnoreEvent(evt);
			}
		}, TrickleDown.TrickleDown);
	}

	void SetButtonFocus(TextField field)
	{

		//Debug.Log("Setting button focus");

		if (field == null || field.panel == null)
		{
			buttonFocusField = null;
			buttonFocusIndex = -1;
			buttonFocusCursorIndex = -1;
			buttonFocusSelectIndex = -1;
			Debug.Log("Resetting BUTTON focus");
			return;
		}

		field.RegisterCallback<KeyUpEvent>(SetButtonMouseFocus);
		field.RegisterCallback<PointerUpEvent>(SetButtonMouseFocus);
		field.RegisterCallback<PointerDownEvent>(SetButtonMouseFocus);
		field.RegisterCallback<PointerMoveEvent>(SetButtonMouseFocus);

		//Debug.Log("Registered callbacks");

		buttonFocusField = field;
		buttonFocusIndex = (int)field.userData;
		buttonFocusCursorIndex = field.cursorIndex;
		buttonFocusSelectIndex = field.selectIndex;

	}

	void SetButtonMouseFocus(EventBase evt)
	{
		//Debug.Log("Calling mouse focus");

		TextField field = (TextField)evt.currentTarget;

		if(field == buttonFocusField && field.selectIndex != buttonFocusSelectIndex)
		{
			buttonFocusSelectIndex = (int)field.selectIndex;
		}


		if(field == buttonFocusField && field.cursorIndex != buttonFocusCursorIndex)
		{
			buttonFocusCursorIndex = (int)field.cursorIndex;
		}

		//Debug.Log($"Setting selection to {buttonFocusSelectIndex} and cursor to {buttonFocusCursorIndex}");
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
		}

	}

	void SetObject()
	{
		if (!(Dialogue)target)
			return;
		serializedObject.Update();

	}
	void SetMarkups(ListView list)
	{
		list.makeItem = () => new Label();
		list.bindItem = (item, index) => { (item as Label).text = markups[index].name; };
		list.itemsSource = markups;
		list.selectionType = SelectionType.Single;

		list.itemsChosen += (selectedItems) =>
		{
			//SetButtonFocus(null);
			foreach (DialogueMarkup markup in selectedItems)
			{
				selectedMarkup = markup;
				Debug.Log($"Selected markup:{selectedMarkup}");
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
			//SetButtonFocus(null);
			
		};

		list.selectionChanged += (selectedItems) =>
		{
			
			//SetButtonFocus(null);
			foreach (MarkupData markupData in selectedItems)
			{
				selectedMarkupData = markupData;
				Debug.Log($"Set markupData! {selectedMarkupData}");
			}
		};
	}


	void SetDialogueLines(List<string> sourceList)
	{
		sourceList.Clear();

		serializedObject.Update();
		SerializedProperty dialogueLines = serializedObject.FindProperty("dialogueLines");

		for(int i = 0; i < dialogueLines.arraySize; i++)
		{
			sourceList.Add(dialogueLines.GetArrayElementAtIndex(i).stringValue);
		}
	}

	void SetDialogueLines(ListView list)
	{

		list.reorderable = true;
		list.reorderMode = ListViewReorderMode.Animated;

		list.itemIndexChanged += (oldIndex, newIndex) =>
		{
			serializedObject.Update();
			SerializedProperty dialogueLines = serializedObject.FindProperty("dialogueLines");

			string newCopyString = dialogueLines.GetArrayElementAtIndex(newIndex).stringValue;
			string oldCopyString = dialogueLines.GetArrayElementAtIndex(oldIndex).stringValue;

			dialogueLines.GetArrayElementAtIndex(newIndex).stringValue = oldCopyString;
			dialogueLines.GetArrayElementAtIndex(oldIndex).stringValue = newCopyString;

			serializedObject.ApplyModifiedProperties();

			list.RefreshItems();
		};


		list.onRemove = (baseListView) =>
		{
			serializedObject.Update();
			SerializedProperty dialogueLines = serializedObject.FindProperty("dialogueLines");
			
			if(focusIndex >= 0)
			{
				serializedObject.Update();
				dialogueLines.DeleteArrayElementAtIndex(focusIndex);
				serializedObject.ApplyModifiedProperties();
				//If removed don't store for markup button
				if(focusIndex == buttonFocusIndex)
				{
					SetButtonFocus(null);
				}
				focusIndex--;
			}

			else
			{
				serializedObject.Update();
				dialogueLines.DeleteArrayElementAtIndex(dialogueLines.arraySize - 1);
				serializedObject.ApplyModifiedProperties();
				//If removed don't store for markup button
				if (dialogueLines.arraySize - 1 == buttonFocusIndex)
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
				serializedObject.Update();
				SerializedProperty dialogueLines = serializedObject.FindProperty("dialogueLines");

				if (field.userData is int index && index >= 0 && index < dialogueLines.arraySize)
				{
					SerializedProperty indexElement = dialogueLines.GetArrayElementAtIndex(index);
					indexElement.stringValue = evt.newValue;
					serializedObject.ApplyModifiedProperties();
				}
			});
			return field;
		};

		list.bindItem = (item,index) =>
		{
			TextField field = (TextField)item;
			field.userData = index;

			serializedObject.Update();
			SerializedProperty dialogueLines = serializedObject.FindProperty("dialogueLines");

			field.SetValueWithoutNotify(dialogueLines.GetArrayElementAtIndex(index).stringValue);
			serializedObject.ApplyModifiedProperties();

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
			serializedObject.Update();
			serializedObject.FindProperty("hasSpeaker").boolValue = toggle.value;
			serializedObject.ApplyModifiedProperties();
			
		});
	}

    void SetSpeaker(ObjectField field)
    {
		field.RegisterCallback<ChangeEvent<Object>>(evt =>
		{
			SetButtonFocus(null);
			serializedObject.Update();
			serializedObject.FindProperty("speaker").objectReferenceValue = (DialogueSpeaker)evt.newValue;
			serializedObject.ApplyModifiedProperties();
			
		});
	}

	#endregion

    void SetTypewriter(Toggle toggle)
    {
		toggle.RegisterCallback<ClickEvent>(evt =>
		{
			SetButtonFocus(null);
			serializedObject.Update();
			serializedObject.FindProperty("hasTypewriter").boolValue = toggle.value;
			serializedObject.ApplyModifiedProperties();
		
		});
	}

    void SetExpression(ObjectField field)
    {
		field.RegisterCallback<ChangeEvent<Object>>(evt =>
		{
			SetButtonFocus(null);
			serializedObject.Update();
			serializedObject.FindProperty("startExpression").objectReferenceValue = (DialogueExpression)evt.newValue;
			serializedObject.ApplyModifiedProperties();
		});
	}

	#endregion


	void AddMarkup(Button button, ListView dialogueLinesList)
	{
		button.RegisterCallback<ClickEvent>(evt =>
		{
			//Debug.Log($"Button clicked...indexes are select:{buttonFocusSelectIndex} and cursor:{buttonFocusCursorIndex}");

			if (buttonFocusField == null || buttonFocusField.panel == null)
			{
				SetButtonFocus(null);
				return;
			}

			if(selectedMarkup == null)
			{
				Debug.Log("No markup!");
			}

			else if(selectedMarkupData == null)
			{
				Debug.Log("No markupData!");
			}

			//If range of highlight selection
			if (buttonFocusSelectIndex != -1 && buttonFocusCursorIndex != -1 && buttonFocusSelectIndex != buttonFocusCursorIndex)
			{
				if (selectedMarkup != null && selectedMarkupData != null)
				{
					int min = Mathf.Min(buttonFocusSelectIndex, buttonFocusCursorIndex);
					int max = Mathf.Max(buttonFocusSelectIndex, buttonFocusCursorIndex);
					Vector2Int selectRange = new Vector2Int(min, max);

					serializedObject.Update();
					SerializedProperty dialogueLines = serializedObject.FindProperty("dialogueLines");
					dialogueLines.GetArrayElementAtIndex(buttonFocusIndex).stringValue = selectedMarkup.ApplyMarkup(dialogueLines.GetArrayElementAtIndex(buttonFocusIndex).stringValue, selectRange, selectedMarkupData);
					serializedObject.ApplyModifiedProperties();
				}

				else if (selectedMarkup != null)
				{
					int min = Mathf.Min(buttonFocusSelectIndex, buttonFocusCursorIndex);
					int max = Mathf.Max(buttonFocusSelectIndex, buttonFocusCursorIndex);
					Vector2Int selectRange = new Vector2Int(min, max);

					serializedObject.Update();
					SerializedProperty dialogueLines = serializedObject.FindProperty("dialogueLines");
					dialogueLines.GetArrayElementAtIndex(buttonFocusIndex).stringValue = selectedMarkup.ApplyMarkup(dialogueLines.GetArrayElementAtIndex(buttonFocusIndex).stringValue, selectRange);
					serializedObject.ApplyModifiedProperties();
				}
			}

			//If just cursor
			else if (buttonFocusCursorIndex != -1)
			{
				
				if (selectedMarkup != null && selectedMarkupData != null)
				{

					Debug.Log($"Running with markupData:{selectedMarkupData}");

					serializedObject.Update();
					SerializedProperty dialogueLines = serializedObject.FindProperty("dialogueLines");
					dialogueLines.GetArrayElementAtIndex(buttonFocusIndex).stringValue = selectedMarkup.ApplyMarkup(dialogueLines.GetArrayElementAtIndex(buttonFocusIndex).stringValue, buttonFocusCursorIndex, selectedMarkupData);
					serializedObject.ApplyModifiedProperties();
				}

				else if(selectedMarkup != null)
				{
					Debug.Log("No selected markupData");
					serializedObject.Update();
					SerializedProperty dialogueLines = serializedObject.FindProperty("dialogueLines");
					dialogueLines.GetArrayElementAtIndex(buttonFocusIndex).stringValue = selectedMarkup.ApplyMarkup(dialogueLines.GetArrayElementAtIndex(buttonFocusIndex).stringValue, buttonFocusCursorIndex);
					serializedObject.ApplyModifiedProperties();
				}

			}

			//Debug.Log($"New line is {thisDialogue.dialogueLines[buttonFocusIndex]}");

			dialogueLinesList.Rebuild();

		});
	}
	void RefreshMarkupDatas(ListView markupList, ListView dataList)
	{

		markupList.selectionChanged += (selectedItems) =>
		{
			//SetButtonFocus(null);
			foreach (DialogueMarkup dialogueMarkup in selectedItems)
			{

				selectedMarkup = dialogueMarkup;
				Debug.Log("Setting selectedMarkupData to null");
				selectedMarkupData = null;
				markupDatas.Clear();
				dataList.selectedIndex = -1;

				markupDatas.AddRange(dialogueMarkup.GetMarkupDatas());
				dataList.RefreshItems();


			}
		};

	}

}
