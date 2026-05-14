using NaughtyAttributes;
using UnityEngine;

public class DialogueMarkup : MonoBehaviour
{
    [SerializeField] protected string formatTagStart;
    [SerializeField] protected string formatTagEnd;
    [SerializeField] protected bool hasParameter;
    [ShowIf("hasParameter")]
    [SerializeField] protected DialogueMarkupParameterType parameterType;

    virtual public bool MarkupRecognition(string text)
    {
        bool containsFullTag = false;

        if(text.Contains(formatTagStart) && text.Contains(formatTagEnd))
        {
            containsFullTag = true;
            //Send some sort of event
        }

        return containsFullTag;
    }

    /*EXAMPLE EVENT?
    pass self as reference and get parameter string and parameter type to cast as?
     */
}
