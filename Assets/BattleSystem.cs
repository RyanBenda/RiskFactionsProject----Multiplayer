using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class BattleSystem : MonoBehaviour
{
    public static BattleSystem _BattleSystemInstance;

    public Image _LeftArmy;
    public TextMeshProUGUI _LeftText;
    public Image _RightArmy;
    public TextMeshProUGUI _RightText;

    public DiceComponent[] _LeftDice;
    public DiceComponent[] _RightDice;

    public GameObject _AttackAirfield;
    public GameObject _DefenceAirfield;

    public Button _RollButton;

    CountryComponent _AttackingCountry;
    CountryComponent _DefendingCountry;
    bool _AttackingCountryOnLeft = true;

    public bool _ActiveBattle = false;
    public IEnumerator _BattleCoroutine;

    public RewardScriptableObject _CardReward;

    public TextMeshProUGUI _DiceButtonText;
    int _DiceIndex = 3;
    // Start is called before the first frame update
    void Start()
    {
        if (_BattleSystemInstance == null)
            _BattleSystemInstance = this;

        this.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (GameCanvasComponent._GameInstance != null)
            GameCanvasComponent._GameInstance._ProgressButton.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetUpFight()
    {
        _AttackingCountry = MainCameraComponent._MainCameraInstance._AttackingCountry;
        _DefendingCountry = MainCameraComponent._MainCameraInstance._DefendingCountry;

        _LeftArmy.color = _AttackingCountry._CurColour;
        _LeftText.color = _AttackingCountry._OccupyingArmy._TextColour;
        _LeftText.text = "A";

        _RightArmy.color = _DefendingCountry._CurColour;
        _RightText.color = _DefendingCountry._OccupyingArmy._TextColour;
        _RightText.text = "D";

        _AttackingCountryOnLeft = true;

        if (_AttackingCountry._TroopsCount >= 4)
        {
            _DiceIndex = 3;
            _DiceButtonText.text = "3";
        }
        else if (_AttackingCountry._TroopsCount == 3)
        {
            _DiceIndex = 2;
            _DiceButtonText.text = "2";
        }
        else
        {
            _DiceIndex = 1;
            _DiceButtonText.text = "1";
        }

        /*Vector3 temp = MainCameraComponent._MainCameraInstance.transform.position - _AttackingCountry.transform.position;

        if (temp.x >= 0)
        {
            _LeftArmy.color = _AttackingCountry._CurColour;
            _LeftText.text = "A";

            _RightArmy.color = _DefendingCountry._CurColour;
            _RightText.text = "D";

            _AttackingCountryOnLeft = true;
        }
        else
        {
            _LeftArmy.color = _DefendingCountry._CurColour;
            _LeftText.text = "D";

            _RightArmy.color = _AttackingCountry._CurColour;
            _RightText.text = "A";

            _AttackingCountryOnLeft = false;
        }*/
    }

    public void ResetFight()
    {
        _AttackingCountry = null;
        _DefendingCountry = null;

        _LeftArmy.color = Color.white;
        _LeftText.text = "";

        _RightArmy.color = Color.white;
        _RightText.text = "";

        for (int i = 0; i < _LeftDice.Length; i++)
        {
            _LeftDice[i]._RollText.text = "";
            _LeftDice[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < _RightDice.Length; i++)
        {
            _RightDice[i]._RollText.text = "";
            _RightDice[i].gameObject.SetActive(false);
        }

        GameCanvasComponent._GameInstance._ProgressButton.gameObject.SetActive(true);
        this.gameObject.SetActive(false);
    }

    private void ResetDice()
    {
        for (int i = 0; i < _LeftDice.Length; i++)
        {
            _LeftDice[i]._RollText.text = "";
            _LeftDice[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < _RightDice.Length; i++)
        {
            _RightDice[i]._RollText.text = "";
            _RightDice[i].gameObject.SetActive(false);
        }

        _AttackAirfield.SetActive(false);
        _DefenceAirfield.SetActive(false);
    }

    public void RollDice()
    {
        if (!_ActiveBattle)
        {
            _BattleCoroutine = DoBattle();
            StartCoroutine(_BattleCoroutine);

            GameCanvasComponent._GameInstance._HasAttacked = true;
        }
    }

    /*DiceComponent[] GetDiceArray(bool isAttacker)
    {
        if (isAttacker)
        {
            if (_AttackingCountryOnLeft)
                return _LeftDice;
            else
                return _RightDice;
        }
        else
        {
            if (_AttackingCountryOnLeft)
                return _RightDice;
            else
                return _LeftDice;
        }    
    }*/

    public void ChangeDice()
    {
        if (_DiceIndex == 1)
        {
            if (_AttackingCountry._TroopsCount > 2)
            {
                _DiceIndex = 2;
                _DiceButtonText.text = "2";
            }
            else
            {
                _DiceIndex = 4;
                _DiceButtonText.text = ">";
            }
        }
        else if (_DiceIndex == 2)
        {
            if (_AttackingCountry._TroopsCount > 3)
            {
                _DiceIndex = 3;
                _DiceButtonText.text = "3";
            }
            else
            {
                _DiceIndex = 4;
                _DiceButtonText.text = ">";
            }
        }
        else if (_DiceIndex == 3)
        {
            _DiceIndex = 4;
            _DiceButtonText.text = ">";
        }
        else if (_DiceIndex == 4)
        {
            _DiceIndex = 1;
            _DiceButtonText.text = "1";
        }
    }

    IEnumerator DoBattle()
    {
        _ActiveBattle = true;

        List<int> diceRolls = CalculateRoll(_AttackingCountry, true);
        int atkDiceRolls = diceRolls.Count;

        //DiceComponent[] attackersDice = GetDiceArray(true);
        //DiceComponent[] defendersDice = GetDiceArray(false);

        for (int i = 0; i < diceRolls.Count; i++)
        {
            if (i == 0 && _AttackingCountry._HasAirfieldEffect)
            {
                _LeftDice[i]._Roll = diceRolls[i] + 1;
                _AttackAirfield.gameObject.SetActive(true);
            }
            else
                _LeftDice[i]._Roll = diceRolls[i];

            _LeftDice[i]._RollText.text = diceRolls[i].ToString();
            _LeftDice[i].gameObject.SetActive(true);

            /*attackersDice[i]._Roll = diceRolls[i];
            attackersDice[i]._RollText.text = diceRolls[i].ToString();
            attackersDice[i].gameObject.SetActive(true);

            if (_AttackingCountryOnLeft)
            {
                _LeftDice[i]._Roll = diceRolls[i];
                _LeftDice[i]._RollText.text = diceRolls[i].ToString();
                _LeftDice[i].gameObject.SetActive(true);
            }
            else
            {
                _RightDice[i]._Roll = diceRolls[i];
                _RightDice[i]._RollText.text = diceRolls[i].ToString();
                _RightDice[i].gameObject.SetActive(true);
            }*/
        }

        diceRolls = CalculateRoll(_DefendingCountry, false);

        for (int i = 0; i < diceRolls.Count; i++)
        {
            if (i == 0 && _DefendingCountry._HasAirfieldEffect)
            {
                _RightDice[i]._Roll = diceRolls[i] + 1;
                _DefenceAirfield.gameObject.SetActive(true);
            }
            else
                _RightDice[i]._Roll = diceRolls[i];

            _RightDice[i]._RollText.text = diceRolls[i].ToString();
            _RightDice[i].gameObject.SetActive(true);

            /*defendersDice[i]._Roll = diceRolls[i];
            defendersDice[i]._RollText.text = diceRolls[i].ToString();
            defendersDice[i].gameObject.SetActive(true);

            if (_AttackingCountryOnLeft)
            {
                _RightDice[i]._Roll = diceRolls[i];
                _RightDice[i]._RollText.text = diceRolls[i].ToString();
                _RightDice[i].gameObject.SetActive(true);
            }
            else
            {
                _LeftDice[i]._Roll = diceRolls[i];
                _LeftDice[i]._RollText.text = diceRolls[i].ToString();
                _LeftDice[i].gameObject.SetActive(true);
            }*/
        }

        yield return new WaitForSecondsRealtime(1f);

        if (atkDiceRolls > diceRolls.Count)
        {
            atkDiceRolls = diceRolls.Count;
        }

        if (atkDiceRolls > 2)
            atkDiceRolls = 2;

        if (_DefendingCountry._TroopsCount == 1)
            atkDiceRolls = 1;

        if (_DiceIndex == 1)
            atkDiceRolls = 1;

        int atkTroopsLost = 0;
        int defTroopsLost = 0;

        for (int i = 0; i < atkDiceRolls; i++)
        {
            if (_RightDice[i]._Roll >= _LeftDice[i]._Roll)
            {
                _RightDice[i]._DiceArrow.gameObject.SetActive(true);
                atkTroopsLost++;

                if (_RightDice.Length >= 2)
                {
                    if (_RightDice[0]._Roll >= 6 && _RightDice[1]._Roll >= 6)
                        atkTroopsLost++;
                }
            }
            else
            {
                _LeftDice[i]._DiceArrow.gameObject.SetActive(true);
                defTroopsLost++;

                if (_LeftDice.Length >= 2)
                {
                    if (_LeftDice[0]._Roll >= 6 && _LeftDice[1]._Roll >= 6)
                        defTroopsLost++;
                }
            }
        }

        yield return new WaitForSecondsRealtime(2f);

        for (int i = 0; i < 4; i++)
        {
            _LeftDice[i]._DiceArrow.gameObject.SetActive(false);
            _RightDice[i]._DiceArrow.gameObject.SetActive(false);
        }

        ResetDice();

        _AttackingCountry._TroopsCount -= atkTroopsLost;
        if (_AttackingCountry._TroopsCount <= 0)
        {
            _AttackingCountry._TroopsCount = 1;
        }

        _DefendingCountry._TroopsCount -= defTroopsLost;

        bool battleWon = false;
        if (_DefendingCountry._TroopsCount <= 0)
        {
            /*for (int i = 0; i < GameCanvasComponent._GameInstance._TurnOrder.Count; i++)
            {
                if (_DefendingCountry._CurColour == GameCanvasComponent._GameInstance._TurnOrder[i]._Army._ArmyColour)
                {
                    GameCanvasComponent._GameInstance._TurnOrder[i]._ControlledCountries.Remove(_DefendingCountry);

                    if (GameCanvasComponent._GameInstance._TurnOrder[i]._ControlledCountries.Count == 0)
                    {
                        GameCanvasComponent._GameInstance._TurnOrder[i]._isDefeated = true;
                    }
                        
                }    
            }*/

            ArmiesClass defArmy = _DefendingCountry._OccupyingArmy;

            defArmy._ControlledCountries.Remove(_DefendingCountry);
            if (defArmy._ControlledCountries.Count == 0)
            {
                defArmy._isDefeated = true;
                defArmy._Info._Defeated.color = _AttackingCountry._CurColour;
                defArmy._Info._Defeated.gameObject.SetActive(true);

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

            //_AttackingCountry._TroopsCount--;

            //_DefendingCountry._TroopsCount = 1;

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

            battleWon = true;
        }

        _AttackingCountry._TroopDisplay.text = _AttackingCountry._TroopsCount.ToString();
        _DefendingCountry._TroopDisplay.text = _DefendingCountry._TroopsCount.ToString();

        if (_DefendingCountry._HasProxy)
            _DefendingCountry._Proxy.UpdateDetails();

        if (_AttackingCountry._HasProxy)
            _AttackingCountry._Proxy.UpdateDetails();

        if (_AttackingCountry._TroopsCount == 1)
        {
            //_AttackingCountry._TroopsCount--;

            //_DefendingCountry._TroopsCount = 1;

            //_AttackingCountry._TroopDisplay.text = _AttackingCountry._TroopsCount.ToString();
            //_DefendingCountry._TroopDisplay.text = _DefendingCountry._TroopsCount.ToString();

            MainCameraComponent._MainCameraInstance._Tweening = true;


            MainCameraComponent._MainCameraInstance.transform.DOMove(new Vector3(_AttackingCountry.transform.position.x, _AttackingCountry.transform.position.y, -350), 2).OnComplete(() => MainCameraComponent._MainCameraInstance._Tweening = false);
            MainCameraComponent._MainCameraInstance.transform.DORotate(MainCameraComponent._MainCameraInstance._StartingRot, 1);

            MainCameraComponent._MainCameraInstance._DefendingCountry._Selected = false;
            MainCameraComponent._MainCameraInstance._DefendingCountry._HoverObject.SetActive(_DefendingCountry._MouseHoverTracker);
            MainCameraComponent._MainCameraInstance._DefendingCountry._MouseHoverTracker = false;

            if (MainCameraComponent._MainCameraInstance._DefendingCountry._HasProxy)
            {
                MainCameraComponent._MainCameraInstance._DefendingCountry._Proxy._Selected = false;
                MainCameraComponent._MainCameraInstance._DefendingCountry._Proxy._HoverObject.SetActive(_DefendingCountry._Proxy._MouseHoverTracker);
                MainCameraComponent._MainCameraInstance._DefendingCountry._Proxy._MouseHoverTracker = false;
            }

            MainCameraComponent._MainCameraInstance._DefendingCountry = null;

            if (battleWon)
            {
                if (!GameCanvasComponent._GameInstance._CurArmy._HasStarReward)
                {
                    GameCanvasComponent._GameInstance._CurArmy._HasStarReward = true;
                    GameCanvasComponent._GameInstance._CurArmy._PossibleRewards.Add(_CardReward);
                    GameCanvasComponent._GameInstance._RewardEffectList.Add(_CardReward);
                    GameCanvasComponent._GameInstance.PlayRewardEffect();
                }

                ObjectiveManager._ObjectiveManagerInstance.ObjectiveCheck();
            }

            ResetFight();
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

            MainCameraComponent._MainCameraInstance._Tweening = true;

            MainCameraComponent._MainCameraInstance.transform.DOMove(new Vector3(_AttackingCountry.transform.position.x, _AttackingCountry.transform.position.y, -350), 2).OnComplete(() => MainCameraComponent._MainCameraInstance._Tweening = false);
            MainCameraComponent._MainCameraInstance.transform.DORotate(MainCameraComponent._MainCameraInstance._StartingRot, 1);

            MainCameraComponent._MainCameraInstance._DefendingCountry._Selected = false;
            MainCameraComponent._MainCameraInstance._DefendingCountry._HoverObject.SetActive(_DefendingCountry._MouseHoverTracker);
            MainCameraComponent._MainCameraInstance._DefendingCountry._MouseHoverTracker = false;

            if (MainCameraComponent._MainCameraInstance._DefendingCountry._HasProxy)
            {
                MainCameraComponent._MainCameraInstance._DefendingCountry._Proxy._Selected = false;
                MainCameraComponent._MainCameraInstance._DefendingCountry._Proxy._HoverObject.SetActive(_DefendingCountry._Proxy._MouseHoverTracker);
                MainCameraComponent._MainCameraInstance._DefendingCountry._Proxy._MouseHoverTracker = false;
            }

            MainCameraComponent._MainCameraInstance._DefendingCountry = null;

            if (!GameCanvasComponent._GameInstance._CurArmy._HasStarReward)
            {
                GameCanvasComponent._GameInstance._CurArmy._HasStarReward = true;
                GameCanvasComponent._GameInstance._CurArmy._PossibleRewards.Add(_CardReward);
                GameCanvasComponent._GameInstance._RewardEffectList.Add(_CardReward);
                GameCanvasComponent._GameInstance.PlayRewardEffect();
            }

            ObjectiveManager._ObjectiveManagerInstance.ObjectiveCheck();

            ResetFight();
        }
        else if (_AttackingCountry._TroopsCount > 4 && battleWon)
        {
            _RollButton.gameObject.SetActive(false);
            MainCameraComponent._MainCameraInstance.transform.DORotate(MainCameraComponent._MainCameraInstance._StartingRot, 1);

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
            GameCanvasComponent._GameInstance._ProgressButton.gameObject.SetActive(true);

            while (GameCanvasComponent._GameInstance._CurrentState != TurnStates.Battle)
                yield return null;

            MainCameraComponent._MainCameraInstance._Tweening = true;

            BoardComponent._BoardInstance._IncreaseButtonValue[0].text = "";
            BoardComponent._BoardInstance._DecreaseButtonValue[0].text = "";
            BoardComponent._BoardInstance._IncreaseButton[0].gameObject.SetActive(false);
            BoardComponent._BoardInstance._DecreaseButton[0].gameObject.SetActive(false);

            MainCameraComponent._MainCameraInstance.transform.DOMove(new Vector3(_AttackingCountry.transform.position.x, _AttackingCountry.transform.position.y, -350), 2).OnComplete(() => MainCameraComponent._MainCameraInstance._Tweening = false);
            
            if (!GameCanvasComponent._GameInstance._CurArmy._HasStarReward)
            {
                GameCanvasComponent._GameInstance._CurArmy._HasStarReward = true;
                GameCanvasComponent._GameInstance._CurArmy._PossibleRewards.Add(_CardReward);
                GameCanvasComponent._GameInstance._RewardEffectList.Add(_CardReward);
                GameCanvasComponent._GameInstance.PlayRewardEffect();
            }

            ObjectiveManager._ObjectiveManagerInstance.ObjectiveCheck();

            MainCameraComponent._MainCameraInstance._DefendingCountry._Selected = false;
            MainCameraComponent._MainCameraInstance._DefendingCountry._HoverObject.SetActive(_DefendingCountry._MouseHoverTracker);
            MainCameraComponent._MainCameraInstance._DefendingCountry._MouseHoverTracker = false;

            if (MainCameraComponent._MainCameraInstance._DefendingCountry._HasProxy)
            {
                MainCameraComponent._MainCameraInstance._DefendingCountry._Proxy._Selected = false;
                MainCameraComponent._MainCameraInstance._DefendingCountry._Proxy._HoverObject.SetActive(_DefendingCountry._Proxy._MouseHoverTracker);
                MainCameraComponent._MainCameraInstance._DefendingCountry._Proxy._MouseHoverTracker = false;
            }

            MainCameraComponent._MainCameraInstance._DefendingCountry = null;

            _RollButton.gameObject.SetActive(true);
            ResetFight();
        }

        _ActiveBattle = false;

        if (_DiceIndex == 4 && !battleWon)
            RollDice();
        else if (!battleWon && _DiceIndex == 3 && _AttackingCountry._TroopsCount < 4)
        {
            if (_AttackingCountry._TroopsCount == 3)
            {
                _DiceIndex = 2;
                _DiceButtonText.text = "2";
            }
            else if (_AttackingCountry._TroopsCount == 2)
            {
                _DiceIndex = 1;
                _DiceButtonText.text = "1";
            }
        }
        else if (!battleWon && _DiceIndex == 2 && _AttackingCountry._TroopsCount == 2)
        {           
            _DiceIndex = 1;
            _DiceButtonText.text = "1";          
        }
    }

    List<int> CalculateRoll(CountryComponent country, bool attacker)
    {
        List<int> templist = new List<int>();
        int temp = 0;

        if (attacker)
        {
            if (_DiceIndex == 1)
            {
                temp = 1;
            }
            else if (_DiceIndex == 2)
            {
                temp = 2;
            }
            else if (_DiceIndex == 3)
            {
                temp = 3;
            }
            else
            {
                temp = country._TroopsCount - 1;

                if (temp > 3)
                    temp = 3;
            }
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
