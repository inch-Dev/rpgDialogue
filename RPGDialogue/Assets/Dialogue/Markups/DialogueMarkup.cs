using NaughtyAttributes;
using Unity.VisualScripting;
using UnityEngine;

public class DialogueMarkup : MonoBehaviour
{
    [SerializeField] protected string formatTagStart;
    [SerializeField] protected string formatTagEnd;
    [SerializeField] protected bool hasParameter;
    [ShowIf("hasParameter")]
    [SerializeField] protected DialogueMarkupParameterType parameterType;

    virtual public bool MarkupRecognition(string text) //If it has the full tag then send event
    {
        bool containsFullTag = false;

        if(text.Contains(formatTagStart) && text.Contains(formatTagEnd))
        {
            containsFullTag = true;
            //Send some sort of event
        }

        return containsFullTag;
    }

    virtual public bool  IncompleteMarkupRecognition(string text) //If has incomplete tag delete from displayed string until ready
        //Is this needed?
    {
        bool containsStartTag = false;

        //Check for start of the tag and check that there is no end of tag
        //Iterate from start of tag and check for ending tag format
        return containsStartTag;
    }

    /*EXAMPLE EVENT?
    pass self as reference and get parameter string and parameter type to cast as?
     */
}
