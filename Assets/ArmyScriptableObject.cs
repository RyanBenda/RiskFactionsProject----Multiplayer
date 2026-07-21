using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Type", menuName = "ScriptableObjects/Army", order = 1)]
public class ArmyScriptableObject : ScriptableObject
{
    public string _ArmyName;
    public Color _ArmyColour;
}
