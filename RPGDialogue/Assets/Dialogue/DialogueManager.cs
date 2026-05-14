using System.Collections;
using UnityEditor.AdaptivePerformance.Editor;
using UnityEditor.EditorTools;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;
    [SerializeField] float defaultCharWaitMult;
    [Tooltip("The default multiplier of the waiting time between characters")]
    [SerializeField] float defaultFastCharWaitMult;
    [Tooltip("The fast multiplier of the waiting time between characters")]

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
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ReadDialogue(Dialogue dialogue)
    {
        bool isEndOfLine = false;
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

    IEnumerator TypewriterReadText(Dialogue dialogue, string dialogueLine, float timeTillNextText)
    {
        float textSpeed = timeTillNextText;
        float t = 0;
        int charIndex = 0;
        string curText = "";

        while(charIndex < dialogueLine.Length)
        {
            t += Time.deltaTime * textSpeed;
            charIndex = Mathf.FloorToInt(t);
            charIndex = Mathf.Clamp(charIndex, 0, dialogueLine.Length);

            
            curText = dialogueLine.Substring(0, charIndex);
            //CHECK FOR MARKUP TEXT

            updateDialogue?.Invoke(dialogue.speaker.speakerName, dialogue.startingExpression, curText);
            yield return null;
        }
    }
}
