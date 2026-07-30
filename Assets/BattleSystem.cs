using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Mirror;

public class BattleSystem : NetworkBehaviour
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

    public CountryComponent _AttackingCountry;
    public CountryComponent _DefendingCountry;
    bool _AttackingCountryOnLeft = true;

    public bool _ActiveBattle = false;
    public IEnumerator _BattleCoroutine;

    public RewardScriptableObject _CardReward;

    public Button _DiceButton;
    public TextMeshProUGUI _DiceButtonText;
    [SyncVar(hook = nameof(OnDiceIndexChanged))]
    int _DiceIndex = 3;
    // Start is called before the first frame update
    void Start()
    {
        //SetUpInstance();

    }

    private void OnEnable()
    {
        /*if (_BattleSystemInstance == null)
            _BattleSystemInstance = this;*/

        if (GameCanvasComponent._GameInstance != null)
            GameCanvasComponent._GameInstance._ProgressButton.gameObject.SetActive(false);
    }

    public void SetUpInstance()
    {
        if (_BattleSystemInstance == null)
            _BattleSystemInstance = this;
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

        //Debug.Log(GameCanvasComponent._GameInstance);
        if (GameCanvasComponent._GameInstance._LocalPlayer._IsTurn)
        {
            _RollButton.gameObject.SetActive(true);
            _DiceButton.interactable = true;
        }
        else
        {
            _RollButton.gameObject.SetActive(false);
            _DiceButton.interactable = false;
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

    [ClientRpc]
    private void ResetDice()
    {
        for (int i = 0; i < _LeftDice.Length; i++)
        {
            _LeftDice[i]._Roll = -1;
            _LeftDice[i]._RollText.text = "";
            _LeftDice[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < _RightDice.Length; i++)
        {
            _RightDice[i]._Roll = -1;
            _RightDice[i]._RollText.text = "";
            _RightDice[i].gameObject.SetActive(false);
        }

        _AttackAirfield.SetActive(false);
        _DefenceAirfield.SetActive(false);
    }

    [Command(requiresAuthority = false)]
    public void CmdRollDice()
    {
        //RpcRollDice(CalculateRoll(_AttackingCountry, true), CalculateRoll(_DefendingCountry, false));
        if (!_ActiveBattle)
        {
            _BattleCoroutine = DoBattle(/*AdiceRolls, DdiceRolls*/);
            StartCoroutine(_BattleCoroutine);

            GameCanvasComponent._GameInstance._HasAttacked = true;
        }
    }

    [ClientRpc]
    void RpcAirfieldDieEffect(bool atk, DiceComponent die)
    {
        //die._Roll -= 1;
        die._Airfield = true;
        if (atk)
            _AttackAirfield.gameObject.SetActive(true);
        else
            _DefenceAirfield.gameObject.SetActive(true);
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

    [Command(requiresAuthority = false)]
    public void CMDChangeDice()
    {
        Debug.Log(_AttackingCountry);
        if (_AttackingCountry != null)
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
    }

    void OnDiceIndexChanged(int old, int _new)
    {
        if (_DiceIndex == 1)
        {
            _DiceButtonText.text = "1";
        }
        else if (_DiceIndex == 2)
        {
            _DiceButtonText.text = "2";
        }
        else if (_DiceIndex == 3)
        {
            _DiceButtonText.text = "3";
        }
        else if (_DiceIndex == 4)
        {
            _DiceButtonText.text = ">";
        }
    }
    
    
    IEnumerator DoBattle(/*List<int> AdiceRolls, List<int> DdiceRolls*/)
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
                RpcAirfieldDieEffect(true, _LeftDice[i]);
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
                RpcAirfieldDieEffect(false, _RightDice[i]);
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
                _RightDice[i].SetArrowActive(true);
                atkTroopsLost++;

                if (_RightDice.Length >= 2)
                {
                    if (_RightDice[0]._Roll >= 6 && _RightDice[1]._Roll >= 6)
                        atkTroopsLost++;
                }
            }
            else
            {
                _LeftDice[i].SetArrowActive(true);
                defTroopsLost++;

                if (_LeftDice.Length >= 2)
                {
                    if (_LeftDice[0]._Roll >= 6 && _LeftDice[1]._Roll >= 6)
                        defTroopsLost++;
                }
            }
        }

        yield return new WaitForSecondsRealtime(2f);

        _AttackingCountry._TroopsCount -= atkTroopsLost;
        if (_AttackingCountry._TroopsCount <= 0)
        {
            _AttackingCountry._TroopsCount = 1;
        }

        _DefendingCountry._TroopsCount -= defTroopsLost;


        for (int i = 0; i < 4; i++)
        {
            _LeftDice[i].SetArrowActive(false);
            _RightDice[i].SetArrowActive(false);
        }

        ResetDice();



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

            //ArmiesClass2 defArmy = _DefendingCountry._OccupyingArmy;

            //defArmy._ControlledCountries.Remove(_DefendingCountry);
            if (_DefendingCountry._OccupyingArmy._ControlledCountries.Count == 1)
            {
                _DefendingCountry._OccupyingArmy._isDefeated = true;
                

                GameCanvasComponent._GameInstance._CurArmy._OneStars += _DefendingCountry._OccupyingArmy._OneStars;
                GameCanvasComponent._GameInstance._CurArmy._TwoStars += _DefendingCountry._OccupyingArmy._TwoStars;


                for (int i = 0; i < GameCanvasComponent._GameInstance._TurnOrder.Count; i++)
                {
                    if (GameCanvasComponent._GameInstance._TurnOrder[i]._Army._ArmyName == GameCanvasComponent._GameInstance._CurArmy._Army._ArmyName)
                    {
                        GameCanvasComponent._GameInstance._TurnOrder[i] = GameCanvasComponent._GameInstance._CurArmy;
                    }
                    else if(GameCanvasComponent._GameInstance._TurnOrder[i]._Army._ArmyName == _DefendingCountry._OccupyingArmy._Army._ArmyName)
                    {
                        ArmiesClass2 a = GameCanvasComponent._GameInstance._TurnOrder[i];
                        a._isDefeated = true;
                        a._Info.SetDefeated();
                        a._Info._Defeated.color = _AttackingCountry._CurColour;
                        a._Info._Defeated.gameObject.SetActive(true);

                        GameCanvasComponent._GameInstance._TurnOrder[i] = a;
                    }  
                }


                UpdateStarsText(GameCanvasComponent._GameInstance._CurArmy._TwoStars, GameCanvasComponent._GameInstance._CurArmy._OneStars);
            }
            _DefendingCountry._OccupyingArmy._ControlledCountries.Remove(_DefendingCountry);
            //EndOfFightRpc("battlewon", true);

            //GameCanvasComponent._GameInstance._CurArmy._ControlledCountries.Add(_DefendingCountry);
            _DefendingCountry._CurColour = _AttackingCountry._CurColour;
            //_DefendingCountry._OccupyingArmy = _AttackingCountry._OccupyingArmy;
            _DefendingCountry.ChangedOwner();


            UpdateTextColours(_DefendingCountry);




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

            //MainCameraComponent._MainCameraInstance._Tweening = true;


            /*MainCameraComponent._MainCameraInstance.transform.DOMove(new Vector3(_AttackingCountry.transform.position.x, _AttackingCountry.transform.position.y, -350), 2).OnComplete(() => MainCameraComponent._MainCameraInstance._Tweening = false);
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

            ResetFight();*/
            EndOfFightRpc("1", battleWon);
        }
        else if (_AttackingCountry._TroopsCount <= 4 && battleWon)
        {
            int temp = _AttackingCountry._TroopsCount - 1;

            _AttackingCountry._TroopsCount = 1;

            _DefendingCountry._TroopsCount = temp;

            _AttackingCountry._TroopDisplay.text = _AttackingCountry._TroopsCount.ToString();
            _DefendingCountry._TroopDisplay.text = _DefendingCountry._TroopsCount.ToString();

            /*if (_DefendingCountry._HasProxy)
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

            ResetFight();*/

            EndOfFightRpc("<4", battleWon);
        }
        else if (_AttackingCountry._TroopsCount > 4 && battleWon)
        {
            _RollButton.gameObject.SetActive(false);
            //MainCameraComponent._MainCameraInstance.transform.DORotate(MainCameraComponent._MainCameraInstance._StartingRot, 1);
            

            _AttackingCountry._TroopsCount -= 3;

            _DefendingCountry._TroopsCount = 3;

            _AttackingCountry._TroopDisplay.text = _AttackingCountry._TroopsCount.ToString();
            _DefendingCountry._TroopDisplay.text = _DefendingCountry._TroopsCount.ToString();

            EndOfFightRpc("rotate", false);

            

            yield return new WaitForSeconds(1);

            GameCanvasComponent._GameInstance._CurrentState = TurnStates.BattleMove;

            /*_DefendingCountry.SetUpMoveTroopsButtons(_AttackingCountry);
            GameCanvasComponent._GameInstance._ProgressButton.gameObject.SetActive(true);*/
            EndOfFightRpc("battlemove", false);

            while (GameCanvasComponent._GameInstance._CurrentState != TurnStates.Battle)
                yield return null;

            /*MainCameraComponent._MainCameraInstance._Tweening = true;

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
            ResetFight();*/

            EndOfFightRpc(">4", battleWon);
        }

        _ActiveBattle = false;

        if (_DiceIndex == 4 && !battleWon)
            CmdRollDice();
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

        bool hasAtkDie = false;
        bool hasDefDie = false;

        for (int i = 0; i < GameCanvasComponent._GameInstance._TurnOrder.Count; i++)
        {
            if (GameCanvasComponent._GameInstance._TurnOrder[i]._Army._ArmyName == country._OccupyingArmy._Army._ArmyName)
            {
                foreach (RewardScriptableObject r in GameCanvasComponent._GameInstance._TurnOrder[i]._Rewards)
                {
                    if (r._AttackDie)   
                        hasAtkDie = true;
                    else if (r._DefenceDie)
                        hasDefDie = true;
                }
                break;
            }
        }


        // if AttackingCountry has Attack Die temp += 1;
        if (attacker && hasAtkDie)
        {
            temp += 1;
        }
        else if (!attacker && hasDefDie)
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

    [ClientRpc]

    void UpdateTextColours(CountryComponent def)
    {
        Debug.Log("called");

        if (def._IsCapital)
        {
            if (def._CurColour == def._CapitalColour)
            {

                def._CaptialDisplay.color = GameCanvasComponent._GameInstance._CurArmy._TextColour;

            }
            else
                def._CaptialDisplay.color = def._CapitalColour;
        }
        def._CityDisplay.color = GameCanvasComponent._GameInstance._CurArmy._TextColour;
        def._TroopDisplay.color = GameCanvasComponent._GameInstance._CurArmy._TextColour;
    }

    [ClientRpc]
    void UpdateStarsText(int two, int one)
    {
        if (GameCanvasComponent._GameInstance._LocalPlayer._Army._ArmyName == GameCanvasComponent._GameInstance._CurArmy._Army._ArmyName)
        {
            int stars = two * 2;
            stars += one;
            GameCanvasComponent._GameInstance._StarsDisplay.text = "Stars: " + stars.ToString();
        }
        else
        {
            int cards = two;
            cards += one;
            GameCanvasComponent._GameInstance._StarsDisplay.text = "Cards: " + cards.ToString();
        }
    }

    [ClientRpc]

    void EndOfFightRpc(string info, bool battleWon)
    {
        /*if (info == "battlewon")
        {
            if (MainCameraComponent._MainCameraInstance._DefendingCountry._OccupyingArmy._ControlledCountries.Count == 1)
            {
                MainCameraComponent._MainCameraInstance._DefendingCountry._OccupyingArmy._isDefeated = true;
                MainCameraComponent._MainCameraInstance._DefendingCountry._OccupyingArmy._Info._Defeated.color = _AttackingCountry._CurColour;
                MainCameraComponent._MainCameraInstance._DefendingCountry._OccupyingArmy._Info._Defeated.gameObject.SetActive(true);

                /*if (isServer) // hopefully this should avoid getting the same stars twice but will need to test this at some point
                {
                    GameCanvasComponent._GameInstance._CurArmy._OneStars += MainCameraComponent._MainCameraInstance._DefendingCountry._OccupyingArmy._OneStars;
                    GameCanvasComponent._GameInstance._CurArmy._TwoStars += MainCameraComponent._MainCameraInstance._DefendingCountry._OccupyingArmy._TwoStars;
                }

                int stars = GameCanvasComponent._GameInstance._CurArmy._TwoStars * 2;
                stars += GameCanvasComponent._GameInstance._CurArmy._OneStars;

                GameCanvasComponent._GameInstance._StarsDisplay.text = "Stars: " + stars.ToString();
            }
            MainCameraComponent._MainCameraInstance._DefendingCountry._OccupyingArmy._ControlledCountries.Remove(MainCameraComponent._MainCameraInstance._DefendingCountry);
        }*/
        if (info == "1")
        {
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
        else if (info == "<4")
        {
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
        else if (info == "rotate")
        {
            MainCameraComponent._MainCameraInstance.transform.DORotate(MainCameraComponent._MainCameraInstance._StartingRot, 1);

            if (_DefendingCountry._HasProxy)
                _DefendingCountry._Proxy.UpdateDetails();

            if (_AttackingCountry._HasProxy)
                _AttackingCountry._Proxy.UpdateDetails();
        }
        else if (info == "battlemove")
        {
            GameCanvasComponent._GameInstance._CurrentState = TurnStates.BattleMove;

            if (GameCanvasComponent._GameInstance._LocalPlayer._IsTurn)
                _DefendingCountry.SetUpMoveTroopsButtons(_AttackingCountry);
            GameCanvasComponent._GameInstance._ProgressButton.gameObject.SetActive(true);
        }
        else if (info == ">4")
        {
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

    }
}
