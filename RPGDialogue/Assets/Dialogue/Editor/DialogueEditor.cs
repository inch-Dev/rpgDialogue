using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Codice.Client.GameUI.Explorer;
using UnityEditor.Search;

public class DialogueEditor : EditorWindow
{
    [MenuItem("Window/DialogueEditor")]
    public static void ShowExample()
    {
        DialogueEditor wnd = GetWindow<DialogueEditor>();
        wnd.titleContent = new GUIContent("DialogueEditor");
    }

    public void CreateGUI()
    {

        

		// Each editor window contains a root VisualElement object
		VisualElement root = rootVisualElement;

		var splitView = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Horizontal);

        root.Add(splitView);


		#region LEFT PANE
		VisualElement leftPane = new VisualElement();
		splitView.Add(leftPane);

        Label markupLabel = new Label("<b>Dialogue Markups");
        markupLabel.style.fontSize = 12;
        leftPane.Add(markupLabel);

		var allMarkupGuids = AssetDatabase.FindAssets("t:DialogueMarkup");
		var allMarkups = new List<DialogueMarkup>();
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

        var allMarkupDataGuids = AssetDatabase.FindAssets("t:MarkupData");
        var markupDatas = new List<MarkupData>();
        foreach(var guid in allMarkupDataGuids)
        {
            markupDatas.Add(AssetDatabase.LoadAssetAtPath<MarkupData>(AssetDatabase.GUIDToAssetPath(guid)));
        }
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

        List<string> dialogueLines  = new List<string>();
        var dialogueLineList = new ListView() { itemsSource = dialogueLines, fixedItemHeight = 24, showAddRemoveFooter = true, reorderable = true };
        dialogueLineList.makeItem = () =>
        {
            var textField = new TextField();
            textField.RegisterValueChangedCallback(evt =>
            {
                if (textField.userData is int index && index >= 0 && index < dialogueLines.Count)
                {
                    dialogueLines[index] = evt.newValue;
                }
            });
            return textField;
        };

        dialogueLineList.bindItem = (item, index) => { (item as TextField).userData = dialogueLineList[index]; };
        rightPane.Add(dialogueLineList);

        Label optionsLabel = new Label("<b>Dialogue Options");
        optionsLabel.style.fontSize = 12;
        rightPane.Add(optionsLabel);

        Toggle typewriterToggle = new Toggle();
        typewriterToggle.text = "Type Writer Effect";
        rightPane.Add(typewriterToggle);

		Toggle speakerToggle = new Toggle();
		speakerToggle.text = "Speaker";
		rightPane.Add(speakerToggle);

        var speakerField = new ObjectField("Speaker");
        speakerField.objectType = typeof(DialogueSpeaker);
        rightPane.Add(speakerField);

        var expressionField = new ObjectField("Starting Expression");
        expressionField.objectType = typeof(DialogueExpression);
        rightPane.Add(expressionField);

		Button createButton = new Button();
        createButton.text = "Create Dialogue";
        rightPane.Add(createButton);

		#endregion

		#region FUNCTIONS

		markupList.itemsChosen += (selectedItems) =>
		{
			foreach(DialogueMarkup dialogueMarkup in selectedItems)
            {
				
			}
		};

		markupList.selectionChanged += (selectedItems) =>
		{
            foreach (DialogueMarkup dialogueMarkup in selectedItems)
            {

                markupDatas.Clear();
				markupDataList.selectedIndex = -1;

                markupDatas.AddRange(dialogueMarkup.GetMarkupDatas());
				//markupDataList = new ListView() { itemsSource = markupDatas };
				//markupDataList.makeItem = () => new Label();
				//markupDataList.bindItem = (item, index) => { (item as Label).text = markupDatas[index].name; };
				markupDataList.RefreshItems();


			}
		};

		#endregion


	}


}
