using System.Collections;
using NUnit.Framework;
using UnityEditor.AdaptivePerformance.Editor;
using UnityEditor.EditorTools;
using UnityEngine;
using System.Collections.Generic;
public class DialogueManager : MonoBehaviour
{
    [HideInInspector] public static DialogueManager Instance;
    [SerializeField] float defaultCharWaitMult;
    [Tooltip("The default multiplier of the waiting time between characters")]
    [SerializeField] float defaultFastCharWaitMult;
    [Tooltip("The fast multiplier of the waiting time between characters")]

    [SerializeField] List<DialogueMarkup> customMarkups = new List<DialogueMarkup>();
    [Tooltip("Markups to trigger custom functions to edit dialogue. Should follow TextMeshPro markup standard format")]
    Dialogue curDialogue; 
    float timeTillNextChar;
    bool readingDialogue = false;
    string lastDisplayedDialogue;
    float curWaitTime = 0;
    DialogueExpression curExpression;
    DialogueSpeaker curSpeaker;

    //Need some sort of markup handle event
    #region EVENTS
    public delegate void UpdateDialogue(string speakerName, DialogueExpression expression, string curText);
    public static event UpdateDialogue updateDialogue;

    void OnEnable()
    {
        DialoguePrompt.promptDialogue += ReadDialogue;
    }

    void OnDisable()
    {
        DialoguePrompt.promptDialogue -= ReadDialogue;
    }
    #endregion
    void Start()
    {
        if(Instance == null)
        Instance = this;

        timeTillNextChar = defaultCharWaitMult;
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ReadDialogue(Dialogue dialogue)
    {
        if(readingDialogue)
            return;

        curDialogue = dialogue;
        curSpeaker = dialogue.speaker;
        curExpression = dialogue.startingExpression;
        readingDialogue = true; 
        ReadDialogueLine(dialogue, dialogue.dialogueLines[0]);
    }

    void ReadDialogueLine(Dialogue dialogue, string dialogueLine)
    {
        //Read all words in each dialog line then clear to next line
        //PER WORD text read timer and update ui

        //NEEDS MARKUP TEXT TO WAIT AND CHANGE EXPRESSIONS
        if(dialogue.hasTypeWriterEffect)
            StartCoroutine(TypewriterReadText(dialogue, dialogueLine, defaultCharWaitMult));
        else
        {
            //Eventually need function that reads for markup text 
            updateDialogue?.Invoke(dialogue.speaker.speakerName, dialogue.startingExpression, dialogueLine);
        }
    }

    //Take dialog string
    //Get string of last displayed text
    //Skip over tags until has displayed enough visible characters
    //Add added visible characters with the tags and clean up any incomplete ones
    //If new completed tag run event
    //Add new text to previous dialogue string and display
   


    string CleanMarkupText (int index, string dialogueString) //Clean everything in brackets until the entire tag is included //Need index to cutoff
    {
    //skip till after previously displayed text
        
    //Increment index if in the middle of tag formatting
    //Need to skip format tags
        //NEED TO STORE PREVIOUS STRING TO IGNORE OLD MARKUPS
        string cleanText = "";
        string noTagsCleanText = "";
        char[] textArray  = dialogueString.ToCharArray();

        bool isReadingTag = false;
        string tagText = "";

  
        for(int i = 0; noTagsCleanText.Length < index + 1; i++) //Keep iterating until visible characters is one less than index of typewriter effect
        {
            if (i >= dialogueString.Length)
                break;

            if(textArray[i] == '<') //Grab tags and skip over adding to the string
            {
                isReadingTag = true;
                tagText = cleanText;
            }

            if(!isReadingTag)
            {
                cleanText += textArray[i];
                noTagsCleanText += textArray[i];
            } 

            if(isReadingTag)
            {
                tagText += textArray[i];
            }

			if (textArray[i] == '>') //If complete tag re-add completed tag
			{
                cleanText = tagText;
                Debug.Log($"Ending format tag. Adding... {tagText}|");
				isReadingTag = false;
			}


		}


        lastDisplayedDialogue = cleanText;
        return cleanText;
    }

    IEnumerator TypewriterReadText(Dialogue dialogue, string dialogueLine, float timeTillNextText)  //Find way to recognize if last of dialogue is a format tag
    {
        
        float textSpeed = timeTillNextText;
        float t = 0;
        int charIndex = 0;
        string curText = "";

        while(charIndex < dialogueLine.Length)
        {
            t += Time.deltaTime * textSpeed; //Adjust value based on events
            yield return new WaitForSeconds(curWaitTime); //If wait event called add time between characters;

            charIndex = Mathf.FloorToInt(t);
            charIndex = Mathf.Clamp(charIndex, 0, dialogueLine.Length);

            
            curText = dialogueLine.Substring(0, charIndex);
            curText = CleanMarkupText(charIndex, dialogueLine);
            

            //Need to update expression somehow
            updateDialogue?.Invoke(dialogue.speaker.speakerName, curExpression, curText);
			Debug.Log($"index is {charIndex}, typewrite {curText}|");
			yield return null;
        }
        readingDialogue = false;
        curDialogue = null;
    }
}
