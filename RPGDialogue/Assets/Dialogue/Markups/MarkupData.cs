using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

[CreateAssetMenu(fileName = "MarkupData", menuName = "ScriptableObjects/DialogueObjects/DialogueMarkups/DialogueMarkup/MarkupData", order = 1)]
public class MarkupData : ScriptableObject
{
    [SerializeField] string keyName;


    //Get all variables (parameters) in this markupData that is not its keyName
    public FieldInfo[] GetParameters()
    {
        Type type = typeof(MarkupData);
        FieldInfo[] fields = null;

        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        fields = type.GetFields(flags);

        return fields;
    }
    public virtual void HandleOpenLogic(){ }

    public virtual void HandleCloseLogic(){ }
}
