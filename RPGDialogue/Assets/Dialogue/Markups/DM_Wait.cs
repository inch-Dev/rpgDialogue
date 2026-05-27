using System;
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEditor.U2D.Animation;
using UnityEngine;

[CreateAssetMenu(fileName = "DM_Wait", menuName = "ScriptableObjects/DialogueObjects/DialogueMarkups/DM_Wait", order = 2)]
public class DM_Wait : DialogueMarkup
{
    public override void HandleMarkupLogic(string text)
    {
       base.HandleMarkupLogic(text);
    }

}
