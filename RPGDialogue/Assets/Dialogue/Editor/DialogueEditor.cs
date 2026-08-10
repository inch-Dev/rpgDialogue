using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Codice.Client.GameUI.Explorer;
using UnityEditor.Search;
using Codice.CM.Common.Update.Partial;
using UnityEditor.Build.Pipeline.Tasks;
using System.Linq;

[CustomEditor(typeof(Dialogue))]
public class DialogueEditor : Editor
{
	//SOURCE VALUES
	Dialogue thisDialogue;
	List<DialogueMarkup> markups = new List<DialogueMarkup>();
	List<MarkupData> markupDatas = new List<MarkupData>();


	//EDITABLE VALUES
	MarkupData selectedMarkupData = null;

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

		//Button createButton = new Button();
  //      createButton.text = "Create Dialogue";
  //      CreateDialogue(createButton);
  //      rightPane.Add(createButton);

		#endregion

		return root;
	}

    void RefreshMarkupDatas(ListView markupList, ListView dataList)
    {
		markupList.itemsChosen += (selectedItems) =>
		{

		};

		markupList.selectionChanged += (selectedItems) =>
		{
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
	}

	void SetMarkupDatas(ListView list)
	{
		list.makeItem = () => new Label();
		list.bindItem = (item, index) => { (item as Label).text = markupDatas[index].name; };
		list.itemsSource = markupDatas;
		list.selectionType = SelectionType.Single;

		list.itemsChosen += (selectedItems) =>
		{
			foreach(MarkupData markupData in selectedItems)
			{
				selectedMarkupData = markupData;
			}
		};
	}

	void SetDialogueLines(ListView list)
	{
		list.makeItem = () =>
		{
			var textField = new TextField();
			textField.RegisterValueChangedCallback(evt =>
			{
				if (textField.userData is int index && index >= 0 && index < thisDialogue.dialogueLines.Count())
				{
					thisDialogue.dialogueLines[index] = evt.newValue;

				}
			});
			return textField;
		};

		list.bindItem = (item,index) =>
		{
			var field = (TextField)item;
			field.userData = index;
			field.SetValueWithoutNotify(thisDialogue.dialogueLines[index]);
		};
	}

    void SetSpeaker(Toggle toggle)
    {
		toggle.RegisterCallback<ClickEvent>(evt =>
		{
			thisDialogue.hasSpeaker = toggle.value;
		});
	}

    void SetSpeaker(ObjectField field)
    {
		field.RegisterCallback<ChangeEvent<Object>>(evt =>
		{
			thisDialogue.speaker = (DialogueSpeaker)evt.newValue;
		});
	}

    void SetSpeaker(Toggle toggle, ObjectField field)
    {
        toggle.RegisterCallback<ClickEvent>( evt =>
        {
            thisDialogue.hasSpeaker = toggle.value;
        });

        field.RegisterCallback<ChangeEvent<Object>>(evt =>
        {
            thisDialogue.speaker = (DialogueSpeaker)evt.newValue; 
        });
    }

    void SetTypewriter(Toggle toggle)
    {
		toggle.RegisterCallback<ClickEvent>(evt =>
		{
			thisDialogue.hasTypeWriter = toggle.value;
		});
	}

    void SetExpression(ObjectField field)
    {
		field.RegisterCallback<ChangeEvent<Object>>(evt =>
		{
			thisDialogue.startExpression = (DialogueExpression)evt.newValue;
		});
	}

 //   void CreateDialogue(Button button)
 //   {
	//    button.RegisterCallback<ClickEvent>(evt =>
	//	{
	//		Dialogue newDialogue = ScriptableObject.CreateInstance<Dialogue>();
	//		newDialogue.hasSpeaker = hasSpeaker;
	//		newDialogue.speaker = speaker;
	//		newDialogue.hasTypeWriter = hasTypeWriter;
	//		newDialogue.startExpression = expression;
	//		newDialogue.dialogueLines = dialougeLines.ToArray();

	//		string path = "Assets/NewDialogue.asset";
	//		path = AssetDatabase.GenerateUniqueAssetPath(path);

	//		AssetDatabase.CreateAsset(newDialogue, path);
	//		AssetDatabase.SaveAssets();

	//		EditorUtility.FocusProjectWindow();
	//		Selection.activeObject = newDialogue;
	//	});
	//}



}
