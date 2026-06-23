using System;
using UnityEngine;

public enum DialogueMarkupParameterType
{
    //Each of these stores a value of a type so we can get that type and cast a parameter to that type
    NULL = -1,
    INT = 1 << 0,
    FLOAT = 1 << 1,
    BOOL = 1 << 2,
    CHAR = 1 << 3,
    STRING = 1<< 4,
    DOUBLE = 1 << 5,
    EXPRESSION = 1 << 6,
    SPEED = 1 << 7,
    NUM_PARAMETER_TYPES
}

