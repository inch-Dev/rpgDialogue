using System;
using System.Security.Cryptography;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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

    string currentDialogue = null;

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

	#region GETTERS

    public GameObject GetActiveTextbox()
    {
        if (textBox.activeSelf)
            return textBox;


        if (altTextBox.activeSelf)
            return altTextBox;

        return null;
    }

    public TMP_Text GetActiveText()
    {
        if(GetActiveTextbox() == textBox)
            return textBox.GetComponent<TMP_Text>();
        if(GetActiveTextbox() == altTextBox)
            return altTextBox.GetComponent<TMP_Text>();

        return null;
    }

    #endregion

	#region SETTERS

    /// <summary>
    /// Set all dialogue UI
    /// </summary>
    /// <param name="speakerName"></param>
    /// <param name="expression"></param>
    /// <param name="curText"></param>
    /// <param name="index"></param>
	public void SetDialogue(string speakerName, DialogueExpression expression, string curText, int index)
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

    public void SetDialogue(string dialogueText)
    {
        currentDialogue = dialogueText;
    }
    public void SetSpeakerName(string name)
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

    /// <summary>
    /// Set speaker's expression sprite
    /// </summary>
    /// <param name="expression"></param>
    public void SetExpression(DialogueExpression expression)
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

    /// <summary>
    /// Sets alternate Textbox GameObject to active and display text
    /// </summary>
    /// <param name="dialogueText"></param>
    /// <param name="index"></param>
    public void SetAltTextbox(String dialogueText, int index)
    {
        textBox.SetActive(false);
        altTextBox.SetActive(true);

        IndexTextVisbility(dialogueText, index, altTextBox.GetComponent<TMP_Text>());

    }

    /// <summary>
    /// Sets standard Textbox GameObject to active and display text
    /// </summary>
    /// <param name="dialogueText"></param>
    /// <param name="index"></param>
    public void SetTextbox(String dialogueText, int index)
    {
        altTextBox.SetActive(false);
        textBox.SetActive(true);

        IndexTextVisbility(dialogueText, index, textBox.GetComponent<TMP_Text>());
    }

    #endregion

    public void IndexTextVisibility(int index, TMP_Text textBox)
    {
        IndexTextVisbility(currentDialogue, index, textBox);
    }

    /// <summary>
    /// Reveals chararacters in display text up to index
    /// </summary>
    /// <param name="rawText"></param>
    /// <param name="index"></param>
    /// <param name="textBox"></param>
    public void IndexTextVisbility(string rawText, int index, TMP_Text textBox)
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

    public void TextOperation(int startIndex)
    {
        TextOperation(startIndex, GetActiveText());
    }

    public void TextOperation(List<Vector2Int> indexRanges)
    {
        TextOperation(indexRanges, GetActiveText());
    }

	public void TextOperation(Vector2Int indexRange)
	{
		TextOperation(indexRange, GetActiveText());
	}

	public void TextOperation(int startIndex, TMP_Text textBox)
    {

    }

    public void TextOperation(List<Vector2Int> indexRanges, TMP_Text textBox)
    {
        foreach(Vector2Int indexRange in indexRanges)
        {
            TextOperation(indexRange, textBox);
        }
    }

    public void TextOperation(Vector2Int indexRange, TMP_Text textBox)
    {
        
    }

    #endregion
}
