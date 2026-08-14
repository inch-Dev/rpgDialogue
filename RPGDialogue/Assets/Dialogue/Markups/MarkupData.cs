using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

[CreateAssetMenu(fileName = "MarkupData", menuName = "ScriptableObjects/DialogueObjects/DialogueMarkups/DialogueMarkup/MarkupData", order = 1)]
public class MarkupData : ScriptableObject
{
    [SerializeField] public string keyName;
    public virtual void OpenLogic(DialogueMarkup markup, string rawText){ }

    public virtual void CloseLogic(DialogueMarkup markup, string rawText){ }
}
