using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
[CreateAssetMenu(fileName = "DE", menuName = "ScriptableObjects/DialogueObjects/DialogueExpression", order = 3)]
public class DialogueExpression : ScriptableObject
{
    [SerializeField] public DialogueExpressionID id;
    [SerializeField] public Sprite staticSprite;
    [SerializeField] bool isAnimated; //Is expression animated?
    [ShowIf("isAnimated")]
    [SerializeField] List<Sprite> animationFrames;
    [ShowIf("isAnimated")]
    [SerializeField] float animationFPS;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
}
