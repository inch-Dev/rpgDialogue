using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Codice.Client.GameUI.Explorer;
using UnityEditor.Search;

public class DialogueEditor : EditorWindow
{
    [MenuItem("Tools/DialogueEditor")]
    public static void ShowExample()
    {
        DialogueEditor wnd = GetWindow<DialogueEditor>();
        wnd.titleContent = new GUIContent("DialogueEditor");
    }

    TextField focusField;

	//SOURCE VALUES
	List<DialogueMarkup> allMarkups = new List<DialogueMarkup>();
	List<MarkupData> markupDatas = new List<MarkupData>();


	//EDITABLE VALUES
	List<string> allDialogueLines = new List<string>();
	bool hasSpeaker = false;
	bool hasTypeWriter = false;
    DialogueSpeaker speaker = null;
    DialogueExpression expression = null;

	public void CreateGUI()
    {
		VisualElement root = rootVisualElement;

		var splitView = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Horizontal);
        root.Add(splitView);

		#region LEFT PANE
		VisualElement leftPane = new VisualElement();
		splitView.Add(leftPane);

        Label markupLabel = new Label("<b>Dialogue Markups");
        markupLabel.style.fontSize = 12;
        leftPane.Add(markupLabel);


		string[] allMarkupGuids = AssetDatabase.FindAssets("t:DialogueMarkup");
		foreach (var guid in allMarkupGuids)
		{
			allMarkups.Add(AssetDatabase.LoadAssetAtPath<DialogueMarkup>(AssetDatabase.GUIDToAssetPath(guid)));
		}
		var markupList = new ListView();
        markupList.makeItem = () => new Label();
        markupList.bindItem = (item, index) => { (item as Label).text = allMarkups[index].name; };
        markupList.itemsSource = allMarkups;
        markupList.selectionType = SelectionType.Single;
        leftPane.Add(markupList);


        Label dataLabel = new Label("<b>Markup Datas");
        dataLabel.style.fontSize = 12;
        leftPane.Add(dataLabel);

        var markupDataList = new ListView();
        markupDataList.makeItem = () => new Label();
        markupDataList.bindItem = (item, index) => { (item as Label).text = markupDatas[index].name; };
        markupDataList.itemsSource = markupDatas;
        markupDataList.selectionType = SelectionType.Single;
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

        var dialogueLineList = new ListView() { itemsSource = allDialogueLines, fixedItemHeight = 24, showAddRemoveFooter = true, reorderable = true };
        
        rightPane.Add(dialogueLineList);

        Label optionsLabel = new Label("<b>Dialogue Options");
        optionsLabel.style.fontSize = 12;
        rightPane.Add(optionsLabel);

        Toggle typewriterToggle = new Toggle();
        typewriterToggle.text = "Type Writer Effect";
		SetTypewriter(typewriterToggle);
        rightPane.Add(typewriterToggle);

        
		Toggle speakerToggle = new Toggle();
		speakerToggle.text = "Speaker";
		SetSpeaker(speakerToggle);
		rightPane.Add(speakerToggle);

        var speakerField = new ObjectField("Speaker");
        speakerField.objectType = typeof(DialogueSpeaker);
		SetSpeaker(speakerField);
        rightPane.Add(speakerField);

        var expressionField = new ObjectField("Starting Expression");
        expressionField.objectType = typeof(DialogueExpression);
		SetExpression(expressionField);
        rightPane.Add(expressionField);

		Button createButton = new Button();
        createButton.text = "Create Dialogue";
        CreateDialogue(createButton);
        rightPane.Add(createButton);

        #endregion

        #region FUNCTIONS

		markupList.itemsChosen += (selectedItems) =>
		{

		};

		markupList.selectionChanged += (selectedItems) =>
		{
            foreach (DialogueMarkup dialogueMarkup in selectedItems)
            {

                markupDatas.Clear();
				markupDataList.selectedIndex = -1;

                markupDatas.AddRange(dialogueMarkup.GetMarkupDatas());
				markupDataList.RefreshItems();


			}
		};


		dialogueLineList.makeItem = () =>
		{
			var textField = new TextField();
			textField.RegisterValueChangedCallback(evt =>
			{
				if (textField.userData is int index && index >= 0 && index < allDialogueLines.Count)
				{
					allDialogueLines[index] = evt.newValue;
                    
				}
			});
			return textField;
		};

		dialogueLineList.bindItem = (item, index) =>
		{
			var field = (TextField)item;
			field.userData = index;
			field.SetValueWithoutNotify(allDialogueLines[index]);
		};

		//speakerToggle.RegisterCallback<ClickEvent>(evt =>
		//{
		//	hasSpeaker = speakerToggle.value;
		//	//Debug.Log($"Has speaker is {hasSpeaker}");
		//});

        //typewriterToggle.RegisterCallback<ClickEvent>(evt =>
        //{
        //    hasTypeWriter = typewriterToggle.value;
        //});

        
        //speakerField.RegisterCallback<ChangeEvent<Object>>(evt =>
        //{
        //    speaker = (DialogueSpeaker)evt.newValue;
        //});

        //expressionField.RegisterCallback<ChangeEvent<Object>>(evt =>
        //{
        //    expression = (DialogueExpression)evt.newValue;
        //});

  //      createButton.RegisterCallback<ClickEvent>(evt =>
  //      {
  //          Debug.Log("Clicked create!");

  //          Dialogue newDialogue = ScriptableObject.CreateInstance<Dialogue>();
  //          newDialogue.hasSpeaker = hasSpeaker;
  //          newDialogue.speaker = speaker;
  //          newDialogue.hasTypeWriterEffect = hasTypeWriter;
  //          newDialogue.startingExpression = expression;
  //          newDialogue.dialogueLines = allDialogueLines.ToArray();

  //          string path = "Assets/NewDialogue.asset";
  //          path = AssetDatabase.GenerateUniqueAssetPath(path);

  //          AssetDatabase.CreateAsset(newDialogue, path);
  //          AssetDatabase.SaveAssets();

  //          EditorUtility.FocusProjectWindow();
  //          Selection.activeObject = newDialogue;

  //          Debug.Log("Created new dialogue!");
            

		//});

		#endregion


	}

    void RefreshMarkupDatas()
    {
        
    }

    void SetSpeaker(Toggle toggle)
    {
		toggle.RegisterCallback<ClickEvent>(evt =>
		{
			hasSpeaker = toggle.value;
		});
	}

    void SetSpeaker(ObjectField field)
    {
		field.RegisterCallback<ChangeEvent<Object>>(evt =>
		{
			speaker = (DialogueSpeaker)evt.newValue;
		});
	}

    void SetSpeaker(Toggle toggle, ObjectField field)
    {
        toggle.RegisterCallback<ClickEvent>( evt =>
        {
            hasSpeaker = toggle.value;
        });

        field.RegisterCallback<ChangeEvent<Object>>(evt =>
        {
            speaker = (DialogueSpeaker)evt.newValue; 
        });
    }

    void SetTypewriter(Toggle toggle)
    {
		toggle.RegisterCallback<ClickEvent>(evt =>
		{
			hasTypeWriter = toggle.value;
		});
	}

    void SetExpression(ObjectField field)
    {
		field.RegisterCallback<ChangeEvent<Object>>(evt =>
		{
			expression = (DialogueExpression)evt.newValue;
		});
	}

    void CreateDialogue(Button button)
    {
	    button.RegisterCallback<ClickEvent>(evt =>
		{
			Dialogue newDialogue = ScriptableObject.CreateInstance<Dialogue>();
			newDialogue.hasSpeaker = hasSpeaker;
			newDialogue.speaker = speaker;
			newDialogue.hasTypeWriterEffect = hasTypeWriter;
			newDialogue.startingExpression = expression;
			newDialogue.dialogueLines = allDialogueLines.ToArray();

			string path = "Assets/NewDialogue.asset";
			path = AssetDatabase.GenerateUniqueAssetPath(path);

			AssetDatabase.CreateAsset(newDialogue, path);
			AssetDatabase.SaveAssets();

			EditorUtility.FocusProjectWindow();
			Selection.activeObject = newDialogue;
		});
	}



}
