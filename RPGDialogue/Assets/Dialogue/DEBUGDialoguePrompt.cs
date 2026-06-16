using UnityEngine;

public class DialoguePrompt : MonoBehaviour
{
    [SerializeField] Dialogue dialogue;
    
    #region EVENTS

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
