using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;

public class ObjectiveManager : NetworkBehaviour
{
    public static ObjectiveManager _ObjectiveManagerInstance;

    public List<ObjectiveScriptableObject> _PossibleObjectives = new List<ObjectiveScriptableObject>();
    //public List<ObjectiveScriptableObject> _ActiveObjectives = new List<ObjectiveScriptableObject>();
    public readonly SyncList<ObjectiveScriptableObject> _ActiveObjectives = new SyncList<ObjectiveScriptableObject>();

    public readonly SyncList<ObjectiveScriptableObject> _InactiveObjectives = new SyncList<ObjectiveScriptableObject>();
    //public List<ObjectiveScriptableObject> _InactiveObjectives = new List<ObjectiveScriptableObject>();

    public RewardScriptableObject[] _Rewards;

    public bool _DisplayActive = false;
    public GameObject _ObjectiveDisplay;
    public ObjectiveDisplay[] _Displayers;
    public TextMeshProUGUI _ObjectiveDescription;

    TurnStates _LastState;

    [SyncVar]
    public bool[] _Can1TurnContinent = new bool[6];
    [SyncVar]
    public int _TakenOverTerritories = 0;
    //public int _TakenOverCapitals = 0;
    [SyncVar]
    public int _TakenOverCities = 0;

    // Start is called before the first frame update
    void Start()
    {
        if (_ObjectiveManagerInstance == null)
            _ObjectiveManagerInstance = this;       
    }

    [Command(requiresAuthority = false)]
    public void SetUpObjectives()
    {
        List<ObjectiveScriptableObject> tempList = new List<ObjectiveScriptableObject>();
        foreach (ObjectiveScriptableObject obj in _PossibleObjectives)
            tempList.Add(obj);

        for (int i = 0; i < _Rewards.Length; i++)
        {
            int temp = Random.Range(0, tempList.Count);

            tempList[temp]._Reward = _Rewards[i];
            tempList[temp]._Inactive = false;
            _ActiveObjectives.Add(tempList[temp]);
            _Displayers[i]._Objective = tempList[temp];

            tempList.RemoveAt(temp);
        }
    }

    public void TurnOnOffDisplay()
    {
        if (!_DisplayActive && !GameCanvasComponent._GameInstance._DisplayActive && !BattleSystem._BattleSystemInstance.gameObject.activeSelf)
        {
            _DisplayActive = true;
            _LastState = GameCanvasComponent._GameInstance._CurrentState;
            GameCanvasComponent._GameInstance._CurrentState = TurnStates.Suspend;
            _ObjectiveDisplay.SetActive(true);
        }
        else if (!BattleSystem._BattleSystemInstance.gameObject.activeSelf)
        {
            foreach (ObjectiveDisplay obj in _Displayers)
            {
                obj._HoverObject.SetActive(false);
            }
            _ObjectiveDescription.text = "";
            _ObjectiveDisplay.SetActive(false);
            GameCanvasComponent._GameInstance._CurrentState = _LastState;
            _DisplayActive = false;
        }
    }

    [Command(requiresAuthority = false)]
    public void ResetManager()
    {
        for (int i = 0; i < _Can1TurnContinent.Length; i++)
        {
            _Can1TurnContinent[i] = true;
        }
        _TakenOverTerritories = 0;
        //_TakenOverCapitals = 0;
        _TakenOverCities = 0;

        foreach (CountryComponent c in GameCanvasComponent._GameInstance._CurArmy._ControlledCountries)
        {
            _Can1TurnContinent[c._Continent._ContinentOrder] = false;
        }
    }

    [Command(requiresAuthority = false)]
    public void UpdateTakenOverVals(CountryComponent c)
    {
        _TakenOverTerritories++;
        if (c._HasCity)
            _TakenOverCities++;
    }

    [Command(requiresAuthority = false)]
    public void ObjectiveCheck()
    {
        foreach (ObjectiveScriptableObject o in _ActiveObjectives)
        {
            if (!o._Inactive)
            {
                if (o._Continent)
                {
                    if (o._OneTurn)
                    {
                        for (int i = 0; i < BoardComponent._BoardInstance._Continents.Length; i++)
                        {
                            if (BoardComponent._BoardInstance._Continents[i]._ControllingArmy._Army == GameCanvasComponent._GameInstance._CurArmy._Army && _Can1TurnContinent[i] == true)
                            {
                                AddReward(o);
                                _InactiveObjectives.Add(o);
                                o._Inactive = true;
                                break;
                            }

                        }
                    }
                    else
                    {
                        if (o._RequiredContinents.Length > 1)
                        {
                            int temp = 0;
                            foreach (ContinentComponent c in BoardComponent._BoardInstance._Continents)
                            {
                                if (c._ControllingArmy._Army == GameCanvasComponent._GameInstance._CurArmy._Army)
                                    temp++;

                                if (temp >= 2)
                                {
                                    AddReward(o);
                                    _InactiveObjectives.Add(o);
                                    o._Inactive = true;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            if (BoardComponent._BoardInstance._Continents[o._RequiredContinents[0]]._ControllingArmy._Army == GameCanvasComponent._GameInstance._CurArmy._Army)
                            {
                                AddReward(o);
                                _InactiveObjectives.Add(o);
                                o._Inactive = true;
                            }
                        }
                    }
                }
                else if (o._Territory)
                {
                    if (o._OneTurn)
                    {
                        if (_TakenOverTerritories == o._RequiredTerritories)
                        {
                            AddReward(o);
                            _InactiveObjectives.Add(o);
                            o._Inactive = true;
                        }
                    }
                    else
                    {
                        if (GameCanvasComponent._GameInstance._CurArmy._ControlledCountries.Count >= o._RequiredTerritories)
                        {
                            AddReward(o);
                            _InactiveObjectives.Add(o);
                            o._Inactive = true;
                        }
                    }
                }
                else if (o._Capital)
                {
                    int temp = 0;

                    for (int i = 0; i < GameCanvasComponent._GameInstance._CurArmy._ControlledCountries.Count; i++)
                    {
                        if (GameCanvasComponent._GameInstance._CurArmy._ControlledCountries[i]._IsCapital && GameCanvasComponent._GameInstance._CurArmy._ControlledCountries[i]._CapitalColour != GameCanvasComponent._GameInstance._CurArmy._Army._ArmyColour)
                            temp++;
                    }

                    if (temp >= o._RequiredCapitals)
                    {
                        AddReward(o);
                        _InactiveObjectives.Add(o);
                        o._Inactive = true;
                    }
                }
                else if (o._City)
                {
                    if (o._OneTurn)
                    {
                        if (_TakenOverCities == o._RequiredCities)
                        {
                            AddReward(o);
                            _InactiveObjectives.Add(o);
                            o._Inactive = true;
                        }
                    }
                    else
                    {
                        int temp = 0;

                        for (int i = 0; i < GameCanvasComponent._GameInstance._CurArmy._ControlledCountries.Count; i++)
                        {
                            if (GameCanvasComponent._GameInstance._CurArmy._ControlledCountries[i]._HasCity)
                            {
                                temp++;
                            }
                        }

                        if (temp >= o._RequiredCities)
                        {
                            AddReward(o);
                            _InactiveObjectives.Add(o);
                            o._Inactive = true;
                        }
                    }
                }
            }
        }
    }

    [ClientRpc]
    void AddReward(ObjectiveScriptableObject o)
    {
        Debug.Log(GameCanvasComponent._GameInstance._CurArmy._Army._ArmyName);
        GameCanvasComponent._GameInstance._CurArmy._PossibleRewards.Add(o._Reward);
        GameCanvasComponent._GameInstance._RewardEffectList.Add(o._Reward);
        GameCanvasComponent._GameInstance.PlayRewardEffect();
        o._Inactive = true;
    }
}
