using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Codice.Client.GameUI.Explorer;
using UnityEditor.Search;

public class DialogueEditor : EditorWindow
{
    [MenuItem("Window/UI Toolkit/DialogueEditor")]
    public static void ShowExample()
    {
        DialogueEditor wnd = GetWindow<DialogueEditor>();
        wnd.titleContent = new GUIContent("DialogueEditor");
    }

    public void CreateGUI()
    {

        var allMarkupGuids = AssetDatabase.FindAssets("t:DialogueMarkup");
        var allMarkups = new List<DialogueMarkup>();
        foreach(var guid in allMarkupGuids)
        {
            allMarkups.Add(AssetDatabase.LoadAssetAtPath<DialogueMarkup>(AssetDatabase.GUIDToAssetPath(guid)));
        }

		// Each editor window contains a root VisualElement object
		VisualElement root = rootVisualElement;

		var splitView = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Horizontal);

        root.Add(splitView);


		#region LEFT PANE
		VisualElement leftPane = new VisualElement();
		splitView.Add(leftPane);

		Label markupLabel = new Label("Dialogue Markups");
        leftPane.Add(markupLabel);

        var markupList = new ListView();
        markupList.makeItem = () => new Label();
        markupList.bindItem = (item, index) => { (item as Label).text = allMarkups[index].name; };
        markupList.itemsSource = allMarkups;
        leftPane.Add(markupList);

        Label formatTypeLabel = new Label("Format Type");
        leftPane.Add(formatTypeLabel);

        DropdownField formatTypeDropdown = new DropdownField(new List<string> { "Open", "Close" }, 0);
        leftPane.Add(formatTypeDropdown);

        Button markupButton = new Button();
        markupButton.text = "Add Markup";
        leftPane.Add(markupButton);
		#endregion

		#region RIGHT PANE
		VisualElement rightPane = new VisualElement();
        splitView.Add(rightPane);

  

        Label dialogueLabel = new Label("Dialogue Lines");
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

        Toggle typewriterToggle = new Toggle();
        typewriterToggle.text = "Type Writer Effect";
        rightPane.Add(typewriterToggle);

		Toggle speakerToggle = new Toggle();
		speakerToggle.text = "Speaker";
		rightPane.Add(speakerToggle);

        var expressionField = new ObjectField("Starting Expression");
        expressionField.objectType = typeof(DialogueExpression);
        rightPane.Add(expressionField);

		Button createButton = new Button();
        createButton.text = "Create Dialogue";
        rightPane.Add(createButton);

		#endregion
		

    }

    
}
