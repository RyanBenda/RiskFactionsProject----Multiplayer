using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Mirror;

public class QuickAttackComponent : NetworkBehaviour
{
    public QuickAttackArrow _Arrow;
    public Transform _Body;
    bool _step1 = false;

    Vector3 _ArrowStart;

    float _timer = 0.25f;
    public float _timerval = 0.25f;

    CountryComponent _AttackingCountry;
    CountryComponent _DefendingCountry;

    bool _ActiveBattle;
    public IEnumerator _BattleCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    [Command(requiresAuthority = false)]
    void CmdArrowPos(Vector3 pos)
    {
        ArrowPos(pos);
    }

    [ClientRpc]
    void ArrowPos(Vector3 pos)
    {
        _Arrow.transform.position = pos;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameCanvasComponent._GameInstance != null && GameCanvasComponent._GameInstance._CurrentState == TurnStates.Battle && !_ActiveBattle && MainCameraComponent._MainCameraInstance._AttackingCountry == null)
        {
            if (Input.GetMouseButton(0) && MainCameraComponent._MainCameraInstance._HoveredCountry != null && _step1 == false && MainCameraComponent._MainCameraInstance._HoveredCountry._TroopsCount > 1)
            {
                if (_AttackingCountry == null && MainCameraComponent._MainCameraInstance._HoveredCountry._CurColour == GameCanvasComponent._GameInstance._CurArmy._Army._ArmyColour)
                {
                    _AttackingCountry = MainCameraComponent._MainCameraInstance._HoveredCountry;
                    _timer = _timerval;
                }
                else if (_AttackingCountry != MainCameraComponent._MainCameraInstance._HoveredCountry && MainCameraComponent._MainCameraInstance._HoveredCountry._CurColour == GameCanvasComponent._GameInstance._CurArmy._Army._ArmyColour)
                {
                    _AttackingCountry = MainCameraComponent._MainCameraInstance._HoveredCountry;
                    _timer = _timerval;
                }

                if (_timer > 0)
                    _timer -= Time.deltaTime;
                else if (_AttackingCountry != null)
                {
                    //Vector3 mouseScreenPos = Input.mousePosition;

                    //mouseScreenPos.z = -Camera.main.transform.position.z;
                    _AttackingCountry._TroopDisplay.transform.parent = BoardComponent._BoardInstance.transform;
                   

                    _Arrow.transform.position = _AttackingCountry.transform.position;
                    _Arrow.gameObject.SetActive(true);
                    _step1 = true;

                    //_Arrow.SetArrowAuthority();

                    _Body.position = _Arrow.transform.position;
                    _Body.gameObject.SetActive(true);
                    _ArrowStart = _Arrow.transform.position;

                    _AttackingCountry._QuickAttacking = true;
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (_AttackingCountry != null && _DefendingCountry != null)
                {
                    QuickAttack();
                }
                else
                {
                    _step1 = false;
                    _timer = _timerval;
                    _Arrow.gameObject.SetActive(false);
                    _Body.gameObject.SetActive(false);                  
                    if (_AttackingCountry != null)
                    {
                        _AttackingCountry._QuickAttacking = false;
                        _AttackingCountry._TroopDisplay.transform.parent = _AttackingCountry.transform;

                        _AttackingCountry = null;
                    }
                    if (_DefendingCountry != null)
                    {
                        _DefendingCountry._TroopDisplay.transform.parent = _DefendingCountry.transform;

                        _DefendingCountry = null;
                    }
                }
            }

            if (_step1)
            {
                if (_DefendingCountry != MainCameraComponent._MainCameraInstance._HoveredCountry)
                {
                    if (MainCameraComponent._MainCameraInstance._HoveredCountry != null && MainCameraComponent._MainCameraInstance._HoveredCountry != _AttackingCountry && MainCameraComponent._MainCameraInstance._HoveredCountry._CurColour != GameCanvasComponent._GameInstance._CurArmy._Army._ArmyColour)
                    {
                        if (_DefendingCountry != null)
                        {
                            _DefendingCountry._TroopDisplay.transform.parent = _DefendingCountry.transform;

                            _DefendingCountry = null;
                        }
                        foreach (CountryComponent c in _AttackingCountry._NeighbouringCountries)
                        {
                            if (c == MainCameraComponent._MainCameraInstance._HoveredCountry)
                            {
                                _DefendingCountry = MainCameraComponent._MainCameraInstance._HoveredCountry;

                                _DefendingCountry._TroopDisplay.transform.parent = BoardComponent._BoardInstance.transform;
                                break;
                            }
                        }
                    }
                    else if (_DefendingCountry != null)
                    {
                        _DefendingCountry._TroopDisplay.transform.parent = _DefendingCountry.transform;

                        _DefendingCountry = null;
                    }
                }

                _Arrow.transform.position = _ArrowStart;
                if (_DefendingCountry == null)
                {
                    Vector3 mouseScreenPos = Input.mousePosition;

                    mouseScreenPos.z = -Camera.main.transform.position.z;

                    _Arrow.transform.LookAt(Camera.main.ScreenToWorldPoint(mouseScreenPos));

                    if (_Arrow.transform.position != Camera.main.ScreenToWorldPoint(mouseScreenPos))
                        _Arrow.transform.right = _Arrow.transform.forward;

                    _Body.up = _Arrow.transform.right;
                    _Arrow.transform.position = Camera.main.ScreenToWorldPoint(mouseScreenPos) - (_Arrow.transform.right * 50);
                    CmdArrowPos(_Arrow.transform.position);
                }
                else
                {
                    _Arrow.transform.LookAt(_DefendingCountry.transform.position);

                    _Arrow.transform.right = _Arrow.transform.forward;
                    _Body.up = _Arrow.transform.right;

                    _Arrow.transform.position = _DefendingCountry.transform.position - (_Arrow.transform.right * 50);
                }

                _Body.position = _ArrowStart + ((_Arrow.transform.position - _ArrowStart) / 2);

                _Body.localScale = new Vector3(1, Vector3.Distance(_Body.position, _Arrow.transform.position) / 50, 1);
            }
        }
    }

   


    IEnumerator DoBattle()
    {
        _ActiveBattle = true;


        yield return new WaitForSecondsRealtime(0.5f);

        List<int> atkdiceRolls = CalculateRoll(_AttackingCountry, true);
        int atkDiceCount = atkdiceRolls.Count;

        if (_AttackingCountry._HasAirfieldEffect)
            atkdiceRolls[0]++;

        List<int> defdiceRolls = CalculateRoll(_DefendingCountry, false);

        if (_DefendingCountry._HasAirfieldEffect)
            defdiceRolls[0]++;

        if (atkDiceCount > 2)
            atkDiceCount = 2;

        if (_DefendingCountry._TroopsCount == 1)
            atkDiceCount = 1;

        int atkTroopsLost = 0;
        int defTroopsLost = 0;

        for (int i = 0; i < atkDiceCount; i++)
        {
            if (defdiceRolls[i] >= atkdiceRolls[i])
            {
                atkTroopsLost++;

                if (defdiceRolls.Count >= 2)
                {
                    if (defdiceRolls[0] >= 6 && defdiceRolls[1] >= 6)
                        atkTroopsLost++;
                }
            }
            else
            {
                defTroopsLost++;

                if (atkdiceRolls.Count >= 2)
                {
                    if (atkdiceRolls[0] >= 6 && atkdiceRolls[1] >= 6)
                        defTroopsLost++;
                }
            }
        }

        _AttackingCountry._TroopsCount -= atkTroopsLost;
        if (_AttackingCountry._TroopsCount <= 0)
        {
            _AttackingCountry._TroopsCount = 1;
        }

        _DefendingCountry._TroopsCount -= defTroopsLost;

        bool battleWon = false;
        bool battleOver = false;
        if (_DefendingCountry._TroopsCount <= 0)
        {
            ArmiesClass2 defArmy = _DefendingCountry._OccupyingArmy;

            defArmy._ControlledCountries.Remove(_DefendingCountry);
            if (defArmy._ControlledCountries.Count == 0)
            {
                defArmy._isDefeated = true;
                //defArmy._Info._Defeated.color = _AttackingCountry._CurColour;
                //defArmy._Info._Defeated.gameObject.SetActive(true);

                GameCanvasComponent._GameInstance._CurArmy._OneStars += defArmy._OneStars;
                GameCanvasComponent._GameInstance._CurArmy._TwoStars += defArmy._TwoStars;

                int stars = GameCanvasComponent._GameInstance._CurArmy._TwoStars * 2;
                stars += GameCanvasComponent._GameInstance._CurArmy._OneStars;

                GameCanvasComponent._GameInstance._StarsDisplay.text = "Stars: " + stars.ToString();
            }

            GameCanvasComponent._GameInstance._CurArmy._ControlledCountries.Add(_DefendingCountry);
            _DefendingCountry._CurColour = _AttackingCountry._CurColour;
            _DefendingCountry._OccupyingArmy = _AttackingCountry._OccupyingArmy;

            if (_DefendingCountry._IsCapital)
            {
                if (_DefendingCountry._CurColour == _DefendingCountry._CapitalColour)
                {
                    _DefendingCountry._CaptialDisplay.color = GameCanvasComponent._GameInstance._CurArmy._TextColour;
                }
                else
                    _DefendingCountry._CaptialDisplay.color = _DefendingCountry._CapitalColour;
            }
            _DefendingCountry._CityDisplay.color = GameCanvasComponent._GameInstance._CurArmy._TextColour;
            _DefendingCountry._TroopDisplay.color = GameCanvasComponent._GameInstance._CurArmy._TextColour;

            for (int j = 0; j < _DefendingCountry._CountryColour.Length; j++)
            {
                _DefendingCountry._CountryColour[j].color = _DefendingCountry._CurColour;
            }

            _DefendingCountry._Continent.CheckCountries(_DefendingCountry._CurColour);

            ObjectiveManager._ObjectiveManagerInstance._TakenOverTerritories++;
            if (_DefendingCountry._HasCity)
                ObjectiveManager._ObjectiveManagerInstance._TakenOverCities++;

            if (_DefendingCountry._HasAirfield)
            {
                _DefendingCountry._AirfieldDisplay.gameObject.SetActive(false);
                _DefendingCountry._HasAirfield = false;
                _DefendingCountry._HasAirfieldEffect = false;

                BoardComponent._BoardInstance._Airfields.Remove(_DefendingCountry);

                foreach (CountryComponent c in _DefendingCountry._NeighbouringCountries)
                    c._HasAirfieldEffect = false;
            }

            BoardComponent._BoardInstance.ResetAirfield();


            _Arrow.gameObject.SetActive(false);
            _Body.gameObject.SetActive(false);

            battleWon = true;
            battleOver = true;
        }

        _AttackingCountry._TroopDisplay.text = _AttackingCountry._TroopsCount.ToString();
        _DefendingCountry._TroopDisplay.text = _DefendingCountry._TroopsCount.ToString();

        if (_DefendingCountry._HasProxy)
            _DefendingCountry._Proxy.UpdateDetails();

        if (_AttackingCountry._HasProxy)
            _AttackingCountry._Proxy.UpdateDetails();

        if (_AttackingCountry._TroopsCount == 1)
        {
            //MainCameraComponent._MainCameraInstance._Tweening = true;

            //MainCameraComponent._MainCameraInstance.transform.DOMove(new Vector3(_AttackingCountry.transform.position.x, _AttackingCountry.transform.position.y, -350), 2).OnComplete(() => MainCameraComponent._MainCameraInstance._Tweening = false);
            //MainCameraComponent._MainCameraInstance.transform.DORotate(MainCameraComponent._MainCameraInstance._StartingRot, 1);

            _DefendingCountry._Selected = false;
            _DefendingCountry._HoverObject.SetActive(_DefendingCountry._MouseHoverTracker);
            _DefendingCountry._MouseHoverTracker = false;

            if (_DefendingCountry._HasProxy)
            {
                _DefendingCountry._Proxy._Selected = false;
                _DefendingCountry._Proxy._HoverObject.SetActive(_DefendingCountry._Proxy._MouseHoverTracker);
                _DefendingCountry._Proxy._MouseHoverTracker = false;
            }

            //MainCameraComponent._MainCameraInstance._DefendingCountry = null;

            if (battleWon)
            {
                if (!GameCanvasComponent._GameInstance._CurArmy._HasStarReward)
                {
                    GameCanvasComponent._GameInstance._CurArmy._HasStarReward = true;
                    GameCanvasComponent._GameInstance._CurArmy._PossibleRewards.Add(BattleSystem._BattleSystemInstance._CardReward);
                    GameCanvasComponent._GameInstance._RewardEffectList.Add(BattleSystem._BattleSystemInstance._CardReward);
                    GameCanvasComponent._GameInstance.PlayRewardEffect();
                }

                ObjectiveManager._ObjectiveManagerInstance.ObjectiveCheck();
            }

            battleOver = true;
        }
        else if (_AttackingCountry._TroopsCount <= 4 && battleWon)
        {
            int temp = _AttackingCountry._TroopsCount - 1;

            _AttackingCountry._TroopsCount = 1;

            _DefendingCountry._TroopsCount = temp;

            _AttackingCountry._TroopDisplay.text = _AttackingCountry._TroopsCount.ToString();
            _DefendingCountry._TroopDisplay.text = _DefendingCountry._TroopsCount.ToString();

            if (_DefendingCountry._HasProxy)
                _DefendingCountry._Proxy.UpdateDetails();

            if (_AttackingCountry._HasProxy)
                _AttackingCountry._Proxy.UpdateDetails();

            //MainCameraComponent._MainCameraInstance._Tweening = true;

            //MainCameraComponent._MainCameraInstance.transform.DOMove(new Vector3(_AttackingCountry.transform.position.x, _AttackingCountry.transform.position.y, -350), 2).OnComplete(() => MainCameraComponent._MainCameraInstance._Tweening = false);
            //MainCameraComponent._MainCameraInstance.transform.DORotate(MainCameraComponent._MainCameraInstance._StartingRot, 1);

            _DefendingCountry._Selected = false;
            _DefendingCountry._HoverObject.SetActive(_DefendingCountry._MouseHoverTracker);
            _DefendingCountry._MouseHoverTracker = false;

            if (_DefendingCountry._HasProxy)
            {
                _DefendingCountry._Proxy._Selected = false;
                _DefendingCountry._Proxy._HoverObject.SetActive(_DefendingCountry._Proxy._MouseHoverTracker);
                _DefendingCountry._Proxy._MouseHoverTracker = false;
            }

            //MainCameraComponent._MainCameraInstance._DefendingCountry = null;

            if (!GameCanvasComponent._GameInstance._CurArmy._HasStarReward)
            {
                GameCanvasComponent._GameInstance._CurArmy._HasStarReward = true;
                GameCanvasComponent._GameInstance._CurArmy._PossibleRewards.Add(BattleSystem._BattleSystemInstance._CardReward);
                GameCanvasComponent._GameInstance._RewardEffectList.Add(BattleSystem._BattleSystemInstance._CardReward);
                GameCanvasComponent._GameInstance.PlayRewardEffect();
            }

            ObjectiveManager._ObjectiveManagerInstance.ObjectiveCheck();
        }
        else if (_AttackingCountry._TroopsCount > 4 && battleWon)
        {
            //MainCameraComponent._MainCameraInstance.transform.DORotate(MainCameraComponent._MainCameraInstance._StartingRot, 1);

            _AttackingCountry._TroopsCount -= 3;

            _DefendingCountry._TroopsCount = 3;

            _AttackingCountry._TroopDisplay.text = _AttackingCountry._TroopsCount.ToString();
            _DefendingCountry._TroopDisplay.text = _DefendingCountry._TroopsCount.ToString();

            if (_DefendingCountry._HasProxy)
                _DefendingCountry._Proxy.UpdateDetails();

            if (_AttackingCountry._HasProxy)
                _AttackingCountry._Proxy.UpdateDetails();

            yield return new WaitForSeconds(1);

            GameCanvasComponent._GameInstance._CurrentState = TurnStates.BattleMove;

            _DefendingCountry.SetUpMoveTroopsButtons(_AttackingCountry);
            //GameCanvasComponent._GameInstance._ProgressButton.gameObject.SetActive(true);

            while (GameCanvasComponent._GameInstance._CurrentState != TurnStates.Battle)
                yield return null;

            //MainCameraComponent._MainCameraInstance._Tweening = true;

            BoardComponent._BoardInstance._IncreaseButtonValue[0].text = "";
            BoardComponent._BoardInstance._DecreaseButtonValue[0].text = "";
            BoardComponent._BoardInstance._IncreaseButton[0].gameObject.SetActive(false);
            BoardComponent._BoardInstance._DecreaseButton[0].gameObject.SetActive(false);

            //MainCameraComponent._MainCameraInstance.transform.DOMove(new Vector3(_AttackingCountry.transform.position.x, _AttackingCountry.transform.position.y, -350), 2).OnComplete(() => MainCameraComponent._MainCameraInstance._Tweening = false);

            if (!GameCanvasComponent._GameInstance._CurArmy._HasStarReward)
            {
                GameCanvasComponent._GameInstance._CurArmy._HasStarReward = true;
                GameCanvasComponent._GameInstance._CurArmy._PossibleRewards.Add(BattleSystem._BattleSystemInstance._CardReward);
                GameCanvasComponent._GameInstance._RewardEffectList.Add(BattleSystem._BattleSystemInstance._CardReward);
                GameCanvasComponent._GameInstance.PlayRewardEffect();
            }

            ObjectiveManager._ObjectiveManagerInstance.ObjectiveCheck();

            _DefendingCountry._Selected = false;
            _DefendingCountry._HoverObject.SetActive(_DefendingCountry._MouseHoverTracker);
            _DefendingCountry._MouseHoverTracker = false;

            if (_DefendingCountry._HasProxy)
            {
                _DefendingCountry._Proxy._Selected = false;
                _DefendingCountry._Proxy._HoverObject.SetActive(_DefendingCountry._Proxy._MouseHoverTracker);
                _DefendingCountry._Proxy._MouseHoverTracker = false;
            }

            GameCanvasComponent._GameInstance._ProgressButton.gameObject.SetActive(true);
            //MainCameraComponent._MainCameraInstance._DefendingCountry = null;
        }

        _ActiveBattle = false;

        if (!battleOver)
            QuickAttack();
        else
        {
            _Arrow.gameObject.SetActive(false);
            _Body.gameObject.SetActive(false);

            MainCameraComponent._MainCameraInstance._AttackingCountry = null;
            MainCameraComponent._MainCameraInstance._DefendingCountry = null;


            _step1 = false;
            _timer = _timerval;           
            if (_AttackingCountry != null)
            {
                _AttackingCountry._QuickAttacking = false;
                _AttackingCountry._TroopDisplay.transform.parent = _AttackingCountry.transform;

                _AttackingCountry = null;
            }
            if (_DefendingCountry != null)
            {
                _DefendingCountry._TroopDisplay.transform.parent = _DefendingCountry.transform;

                _DefendingCountry = null;
            }
        }
    }

    public void QuickAttack()
    {
        if (!_ActiveBattle)
        {
            MainCameraComponent._MainCameraInstance._AttackingCountry = _AttackingCountry;
            MainCameraComponent._MainCameraInstance._DefendingCountry = _DefendingCountry;

            GameCanvasComponent._GameInstance._HasAttacked = true;
            _BattleCoroutine = DoBattle();
            StartCoroutine(_BattleCoroutine);
        }
    }

    List<int> CalculateRoll(CountryComponent country, bool attacker)
    {
        List<int> templist = new List<int>();
        int temp = 0;

        if (attacker)
        {
            temp = country._TroopsCount - 1;

            if (temp > 3)
                temp = 3;
        }
        else
        {
            temp = country._TroopsCount;

            if (temp > 2)
                temp = 2;
        }

        // if AttackingCountry has Attack Die temp += 1;
        if (attacker && country._OccupyingArmy._HasAttackDie)
        {
            temp += 1;
        }
        else if (!attacker && country._OccupyingArmy._HasDefenceDie)
        {
            temp += 1;
        }

        for (int i = 1; i <= temp; i++)
        {
            templist.Add(Random.Range(1, 7));
        }

        if (templist.Count > 1)
        {
            for (int i = 0; i < templist.Count - 1; i++)
            {
                int temp2 = templist[i];
                int temp3 = templist[i + 1];

                if (temp2 < temp3)
                {
                    templist[i] = temp3;
                    templist[i + 1] = temp2;

                    i = -1;
                }
            }
        }

        return templist;
    }
}
