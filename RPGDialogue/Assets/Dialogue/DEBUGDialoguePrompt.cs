using UnityEngine;

public class DialoguePrompt : MonoBehaviour
{
    [SerializeField] Dialogue dialogue;
    
    #region EVENTS
    public delegate void PromptDialogue(Dialogue dialogue); 
    public static event PromptDialogue promptDialogue;
    #endregion
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            DialogueManager.Instance.ReadDialogue(dialogue);
            //Debug.Log("sending");
        }
    }
}
