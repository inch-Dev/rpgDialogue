using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    [HideInInspector]  public static DialogueUI Instance;
    [SerializeField] Image portraitImage;
    [SerializeField] GameObject portrait;
    [SerializeField] TextMeshProUGUI nameTF;
    [SerializeField] GameObject nameTag;
    [SerializeField] TextMeshProUGUI dialogueTF;
    [SerializeField] TextMeshProUGUI altDialogueTF;
    [SerializeField] GameObject textBox;
    [SerializeField] GameObject altTextBox;

    #region EVENTS

    void OnEnable()
    {
        DialogueManager.updateDialogue += SetDialogue;

    }

    void OnDisable()
    {
        DialogueManager.updateDialogue -= SetDialogue;
    }

	#endregion

	private void Start()
	{
        if(Instance == null)
		    Instance = this;
	}

	#region SETTERS
	void SetDialogue(string speakerName, DialogueExpression expression, string curText, int index)
    {
        SetSpeakerName(speakerName);
        SetExpression(expression);

        if(expression == null)
        {
            SetAltTextbox(curText, index);
        }
        else
        {
            SetTextbox(curText, index);
        }

    }

    void SetSpeakerName(string name)
    {
        if(name == null || name == "") 
        {
            Debug.Log("kdjfdkfdkjf");
            nameTag.SetActive(false);
        }
        else
        {
            nameTag.SetActive(true);
            nameTF.text = name;
        }
    }

    void SetExpression(DialogueExpression expression)
    {
        if(expression == null)
        {
           portrait.SetActive(false);
        }
        else
        {
            portrait.SetActive(true);
            portraitImage.sprite = expression.staticSprite;
        }
    }

    void SetAltTextbox(String dialogueText, int index)
    {
        textBox.SetActive(false);
        altTextBox.SetActive(true);

        IndexTextVisbility(dialogueText, index, altTextBox.GetComponent<TMP_Text>());

    }

    void SetTextbox(String dialogueText, int index)
    {
        altTextBox.SetActive(false);
        textBox.SetActive(true);

        IndexTextVisbility(dialogueText, index, textBox.GetComponent<TMP_Text>());
    }

    #endregion

    void IndexTextVisbility(string rawText, int index, TMP_Text textBox)
    {
        char[] textArray = rawText.ToCharArray();
        textBox.text = rawText;
        textBox.maxVisibleCharacters = 0;
        int visibleChars = 0;

        for(int i = 0; i < rawText.Length; i++)
        {
            if (visibleChars >= index + 1)
                break;
            
            if(textArray[i] == ' ')
            {
                textBox.maxVisibleCharacters++;
            }

            else
            {
                textBox.maxVisibleCharacters++;
                visibleChars++;
            }
        }

        visibleChars = textBox.maxVisibleCharacters;

	}

	#region TEXT EFFECTS

    void TextOperation(string rawText, int startIndex)
    {

    }

    void TextOperation(string rawText, int startIndex, int endIndex)
    {

    }

    void TextOperation(string rawText, int startIndex, TMP_Text textBox)
    {

    }

    void TextOperation(string rawText, int startIndex, int endIndex, TMP_Text textBox)
    {

    }

    #endregion
}
