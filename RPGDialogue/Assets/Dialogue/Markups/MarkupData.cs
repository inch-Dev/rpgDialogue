using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

[CreateAssetMenu(fileName = "MarkupData", menuName = "ScriptableObjects/DialogueObjects/DialogueMarkups/DialogueMarkup/MarkupData", order = 1)]
public class MarkupData : ScriptableObject
{
    [SerializeField] public string keyName;
    public virtual void Open(DialogueMarkup markup, string rawText, DialogueCall callType)
    {
        switch(callType)
        {
            case DialogueCall.DELTA:
                OpenDelta(markup, rawText);
                break;
            case DialogueCall.FULL:
                Open(markup, rawText);
                break;
        }
    }

    public virtual void Open(DialogueMarkup markup, string rawText) { }

    public virtual void OpenDelta(DialogueMarkup markup, string deltaText) { }

    public virtual void Close(DialogueMarkup markup, string rawText, DialogueCall callType)
    {
        switch(callType)
        {
            case DialogueCall.DELTA:
                CloseDelta(markup, rawText); 
                break;
            case DialogueCall.FULL:
                Close(markup, rawText);
                break;
        }
    }
    public virtual void Close(DialogueMarkup markup, string rawText) { }
    public virtual void CloseDelta(DialogueMarkup markup, string deltaText) { }

    public virtual void Continue(DialogueMarkup markup, string deltaText) { }
}
