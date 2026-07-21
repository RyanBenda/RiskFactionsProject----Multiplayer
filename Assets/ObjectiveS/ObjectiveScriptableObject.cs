using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Type", menuName = "ScriptableObjects/Objective", order = 2)]
public class ObjectiveScriptableObject : ScriptableObject
{
    public string _Name;

    public bool _Continent = false;
    public bool _Territory = false;
    public bool _Capital = false;
    public bool _City = false;

    public int[] _RequiredContinents;

    public int _RequiredTerritories = 0;

    public int _RequiredCapitals = 0;

    public int _RequiredCities = 0;

    public bool _OneTurn = false;

    public RewardScriptableObject _Reward;

    public bool _Inactive = true;
}
