using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class BoardComponent : NetworkBehaviour
{
    public static BoardComponent _BoardInstance;

    public ArmyScriptableObject[] _Armies;
    public CountryComponent[] _Countries;
    public ContinentComponent[] _Continents;

    public GameCanvasComponent _GameCanvas;
    public BattleSystem _BattleSystem;
    public TextMeshProUGUI _NewTroopsCount;
    [SyncVar(hook = nameof(OnNewTroopsChanged))]
    public int _NewTroops = 0;

    public Image[] _IncreaseButton;
    public TextMeshProUGUI[] _IncreaseButtonValue;
    public Image[] _DecreaseButton;
    public TextMeshProUGUI[] _DecreaseButtonValue;

    public List<CountryComponent> _TroopsAdded = new List<CountryComponent>();

    public List<CountryComponent> _Airfields = new List<CountryComponent>();

    //public readonly SyncList<CountryComponent> tempCountries = new SyncList<CountryComponent>();

    private void Start()
    {
        if (_BoardInstance == null)
            _BoardInstance = this;
    }
    public void BeginMatch(ArmyScriptableObject[] armies)
    {
        int armyIndex = Random.Range(0, armies.Length);

        int temp = armyIndex;
        for (int i = 0; i < armies.Length; i++)
        {
            ArmiesClass2 army = new ArmiesClass2();
            army._Army = armies[armyIndex];
            /*army._Stars = 0;
            army._HasEarlyMove = false;
            army._HasAdditionalMove = false;
            army._HasReward = false;
            army._isDefeated = false;

            army._ControlledCountries = new List<CountryComponent>();*/
            float cardcount = -0.1f + (0.5f * (i + 1));
            int cardcountint = Mathf.FloorToInt(cardcount);

            for (int j = 0; j < cardcountint; j++)
            {
                int val = Random.Range(0, 3);

                if (val != 2)
                {
                    army._OneStars++;
                }
                else
                    army._TwoStars++;
            }

            army._PossibleRewards = new List<RewardScriptableObject>();
            army._Rewards = new List<RewardScriptableObject>();
            army._ControlledCountries = new List<CountryComponent>();

            /*GameObject a = Instantiate(_GameCanvas._ArmyTabPrefab, _GameCanvas._ArmyOrder.content);
            ArmyInfoComponent aic = a.GetComponent<ArmyInfoComponent>();
            aic._Army = army;
            aic._Image.color = army._Army._ArmyColour;
            aic._Name.text = army._Army._ArmyName;*/

            float sum = army._Army._ArmyColour.r + army._Army._ArmyColour.g + army._Army._ArmyColour.b;

            if (sum < 1)
                army._TextColour = Color.white;
            else
                army._TextColour = Color.black;

            

            CmdCreateArmyOrderObject(army);


            //Debug.Log(_GameCanvas._ArmyOrder.content.transform.childCount);

            //army._Info = _GameCanvas._ArmyOrder.transform.GetChild(armyIndex).GetComponent<ArmyInfoComponent>();
            _GameCanvas._TurnOrder.Add(army);

            armyIndex++;
            if (armyIndex == armies.Length)
                armyIndex = 0;
        }
        armyIndex = temp;

        for (int i = 0; i < 15; i++)
        {
            int index = Random.Range(0, _Countries.Length);

            while (_Countries[index]._HasCity)
            {
                index = Random.Range(0, _Countries.Length);
            }

            _Countries[index]._HasCity = true;
            _Countries[index]._CityDisplay.gameObject.SetActive(true);
        }

        List<CountryComponent> tempCountries = new List<CountryComponent>();

        for (int i = 0; i < _Countries.Length; i++)
        {
            tempCountries.Add(_Countries[i]);
        }

        for (int i = 0; i < tempCountries.Count;)
        {
            int index = Random.Range(0, tempCountries.Count);

            tempCountries[index]._CurColour = armies[armyIndex]._ArmyColour;
            for (int j = 0; j < tempCountries[index]._CountryColour.Length; j++)
            {
                tempCountries[index]._CountryColour[j].color = armies[armyIndex]._ArmyColour;
            }

            for (int j = 0; j < _GameCanvas._TurnOrder.Count; j++)
            {
                if (_GameCanvas._TurnOrder[j]._Army._ArmyColour == tempCountries[index]._CurColour)
                {
                    _GameCanvas._TurnOrder[j]._ControlledCountries.Add(tempCountries[index]);
                    tempCountries[index]._OccupyingArmy = _GameCanvas._TurnOrder[j];
                    //tempCountries[index].SetOccupier(j);

                    tempCountries[index]._TroopDisplay.color = _GameCanvas._TurnOrder[j]._TextColour;
                    tempCountries[index]._CityDisplay.color = _GameCanvas._TurnOrder[j]._TextColour;
                    tempCountries[index]._CaptialDisplay.color = _GameCanvas._TurnOrder[j]._TextColour;

                    if (tempCountries[index]._HasProxy)
                        tempCountries[index]._Proxy._TroopDisplay.color = _GameCanvas._TurnOrder[j]._TextColour;

                }
            }
            
            if (tempCountries[index]._HasProxy)
                tempCountries[index]._Proxy.UpdateDetails();

            tempCountries.RemoveAt(index);
            armyIndex++;
            if (armyIndex == armies.Length)
                armyIndex = 0;
        }

        int troopCap = 3;
        if (_GameCanvas._TurnOrder.Count == 4)
            troopCap = 4;

        for (int i = 0; i < _GameCanvas._TurnOrder.Count; i++)
        {
            int troops = 30 - (5 * (_GameCanvas._TurnOrder.Count - 3));

            if (troops < 10)
            {
                troops = 4; //10
                troopCap = 20;
            }  

            foreach (CountryComponent c in _GameCanvas._TurnOrder[i]._ControlledCountries)
            {
                c._TroopsCount = 1;
                troops--;
                c.UpdateTroops();
            }

            int val = Random.Range(0, _GameCanvas._TurnOrder[i]._ControlledCountries.Count);
            while (troops > 0)
            {
                while (_GameCanvas._TurnOrder[i]._ControlledCountries[val]._TroopsCount == troopCap)
                    val = Random.Range(0, _GameCanvas._TurnOrder[i]._ControlledCountries.Count);

                _GameCanvas._TurnOrder[i]._ControlledCountries[val]._TroopsCount++;
                troops--;
                _GameCanvas._TurnOrder[i]._ControlledCountries[val].UpdateTroops();
                val = Random.Range(0, _GameCanvas._TurnOrder[i]._ControlledCountries.Count);
            }
        }

        foreach (ContinentComponent c in _Continents)
        {
            c._ControllingArmy = default;
            foreach (ArmiesClass2 a in _GameCanvas._TurnOrder)
            {
                if (c._ControllingArmy._Army == default)
                {
                    _GameCanvas._CurArmy = a;
                    c.CheckCountries(_GameCanvas._CurArmy._Army._ArmyColour);
                }
            }
        }

        

        _GameCanvas._CurrentArmyBanner.color = _GameCanvas._TurnOrder[0]._Army._ArmyColour;
        _GameCanvas._CurArmy = _GameCanvas._TurnOrder[0];


        HideMinMaXButtons();
        HideBattle();
        //_GameCanvas.SetInfo();
        /*int stars = _GameCanvas._CurArmy._TwoStars * 2;
        stars += _GameCanvas._CurArmy._OneStars;

        _GameCanvas._StarsDisplay.color = _GameCanvas._CurArmy._TextColour;
        _GameCanvas._StarsDisplay.text = "Stars: " + stars.ToString();*/

        /*CalculateNewTroops(0);

        if (_GameCanvas._CurrentState == TurnStates.CalculateTroops)
        {
            if (_GameCanvas._CurArmy._OneStars >= 2 || _GameCanvas._CurArmy._TwoStars >= 1)
                _GameCanvas._StarTrade.gameObject.SetActive(true);
            else
                _GameCanvas.ProgressTurn();
        }*/
    }

    [ClientRpc]
    void HideMinMaXButtons()
    {
        _IncreaseButton[1].gameObject.SetActive(false);
        _DecreaseButton[1].gameObject.SetActive(false);
    }

    [ClientRpc]
    void HideBattle()
    {
        _BattleSystem.SetUpInstance();

        _BattleSystem.gameObject.SetActive(false);
    }

    [Command(requiresAuthority = false)]

    void CmdCreateArmyOrderObject(ArmiesClass2 army)
    {
        GameObject a = Instantiate(_GameCanvas._ArmyTabPrefab); //_GameCanvas._ArmyOrder.content
        

        NetworkServer.Spawn(a);

        ArmyInfoComponent aic = a.GetComponent<ArmyInfoComponent>();
        aic.RpcCreateArmyOrderObject(army);
    }
    
    /*[ClientRpc]
    void RpcCreateArmyOrderObject(GameObject a, ArmiesClass2 army)
    {
        ArmyInfoComponent aic = a.GetComponent<ArmyInfoComponent>();
        aic._Army = army;
        aic._Image.color = army._Army._ArmyColour;
        aic._Name.text = army._Army._ArmyName;
        aic._Name.color = army._TextColour;
    }*/

    void OnNewTroopsChanged(int old, int _new)
    {
        _NewTroopsCount.text = _NewTroops.ToString();
    }

    private void Update()
    {

    }

    [ClientRpc]
    public void ResetAirfield()
    {
        foreach (CountryComponent c in _Airfields)
        {
            foreach (CountryComponent c1 in c._NeighbouringCountries)
                c1._HasAirfieldEffect = false;
        }

        foreach (CountryComponent c in _Airfields)
        {
            foreach (CountryComponent c1 in c._NeighbouringCountries)
            { 
                if (c1._CurColour == c._CurColour)
                    c1._HasAirfieldEffect = true;
            }
        }
    }

    public void CalculateNewTroops(int index)
    {
        float troopPoints = 0;
        float capitals = 0;

        foreach (CountryComponent c in _GameCanvas._TurnOrder[index]._ControlledCountries)
        {
            if (c._HasCity)
            {
                troopPoints += 2;
            }
            else
                troopPoints++;

            if (c._IsCapital)
            {
                capitals++;
            }
        }

        troopPoints = troopPoints / 3;

        float troopPointsFloor = Mathf.FloorToInt(troopPoints);

        if (troopPointsFloor < 3)
            troopPointsFloor = 3;

        troopPointsFloor += capitals;

        for (int i = 0; i < _Continents.Length; i++)
        {
            if (_GameCanvas._TurnOrder[index]._Army == _Continents[i]._ControllingArmy._Army)
                troopPointsFloor += _Continents[i]._TroopValue;
        }

        bool hasExtraTroops = false;

        foreach (RewardScriptableObject r in _GameCanvas._TurnOrder[index]._Rewards)
        {
            if (r._ExtraTroops)
            {
                hasExtraTroops = true;
                break;
            }
        }

        if (hasExtraTroops)
        {
            Debug.Log("EXTRA TROOPS");
            troopPointsFloor += 2;
        }

        _NewTroops = (int)troopPointsFloor;

        _NewTroopsCount.text = _NewTroops.ToString();
    }

    public void ScrollCheck(CountryComponent scrolledCountry)
    {
        if (GameCanvasComponent._GameInstance._CurrentState == TurnStates.PlaceTroops && MainCameraComponent._MainCameraInstance._AttackingCountry != null && MainCameraComponent._MainCameraInstance._AttackingCountry == scrolledCountry)
        {
            if (Input.mouseScrollDelta.y > 0)
                AddTroops();
            else if (Input.mouseScrollDelta.y < 0)
                SubtractTroops();
        }
        else if (MainCameraComponent._MainCameraInstance._AttackingCountry != null && MainCameraComponent._MainCameraInstance._DefendingCountry != null && MainCameraComponent._MainCameraInstance._DefendingCountry == scrolledCountry)
        {
            if (GameCanvasComponent._GameInstance._CurrentState == TurnStates.BattleMove || GameCanvasComponent._GameInstance._CurrentState == TurnStates.Move || GameCanvasComponent._GameInstance._CurrentState == TurnStates.AdditionalMove || GameCanvasComponent._GameInstance._CurrentState == TurnStates.EarlyMove)
            {
                if (Input.mouseScrollDelta.y > 0)
                    MoveTroopsTo();
                else if (Input.mouseScrollDelta.y < 0)
                    MoveTroopsBack();
            }
        }
    }

    [Command(requiresAuthority = false)]
    public void MaxTroops()
    {
        if (GameCanvasComponent._GameInstance._CurrentState == TurnStates.PlaceTroops)
        {
            MainCameraComponent._MainCameraInstance._AttackingCountry._AddedTroops += _NewTroops;
            MainCameraComponent._MainCameraInstance._AttackingCountry._TroopsCount += _NewTroops;
            MainCameraComponent._MainCameraInstance._AttackingCountry._TroopDisplay.text = MainCameraComponent._MainCameraInstance._AttackingCountry._TroopsCount.ToString();

            /*if (MainCameraComponent._MainCameraInstance._AttackingCountry._TroopDisplay.color != Color.magenta)
            {
                MainCameraComponent._MainCameraInstance._AttackingCountry._TroopDisplay.color = Color.magenta;
                _TroopsAdded.Add(MainCameraComponent._MainCameraInstance._AttackingCountry);
            }

            if (MainCameraComponent._MainCameraInstance._AttackingCountry._HasProxy)
                MainCameraComponent._MainCameraInstance._AttackingCountry._Proxy.UpdateDetails();*/

            _NewTroops = 0;
            _NewTroopsCount.text = "0";

            /*BoardComponent._BoardInstance._IncreaseButtonValue[0].text = "";

            int temp2 = MainCameraComponent._MainCameraInstance._AttackingCountry._TroopsCount - 1;
            BoardComponent._BoardInstance._DecreaseButtonValue[0].text = temp2.ToString();*/

            MaxTroopsButtonsValueUpdate(MainCameraComponent._MainCameraInstance._AttackingCountry._TroopsCount, false);
        }
        else //GameCanvasComponent._GameInstance._CurrentState == TurnStates.BattleMove || GameCanvasComponent._GameInstance._CurrentState == TurnStates.Move || GameCanvasComponent._GameInstance._CurrentState == TurnStates.AdditionalMove || GameCanvasComponent._GameInstance._CurrentState == TurnStates.EarlyMove
        {
            if (MainCameraComponent._MainCameraInstance._AttackingCountry._TroopsCount > 1)
            {
                MainCameraComponent._MainCameraInstance._DefendingCountry._AddedTroops += MainCameraComponent._MainCameraInstance._AttackingCountry._TroopsCount - 1;
                MainCameraComponent._MainCameraInstance._DefendingCountry._TroopsCount += MainCameraComponent._MainCameraInstance._AttackingCountry._TroopsCount - 1;
                MainCameraComponent._MainCameraInstance._DefendingCountry._TroopDisplay.text = MainCameraComponent._MainCameraInstance._DefendingCountry._TroopsCount.ToString();

                /*if (MainCameraComponent._MainCameraInstance._DefendingCountry._TroopDisplay.color != Color.magenta)
                {
                    MainCameraComponent._MainCameraInstance._DefendingCountry._TroopDisplay.color = Color.magenta;
                    if (MainCameraComponent._MainCameraInstance._DefendingCountry._HasProxy)
                        MainCameraComponent._MainCameraInstance._DefendingCountry._Proxy._TroopDisplay.color = Color.magenta;
                    _TroopsAdded.Add(MainCameraComponent._MainCameraInstance._DefendingCountry);
                }

                if (MainCameraComponent._MainCameraInstance._DefendingCountry._HasProxy)
                    MainCameraComponent._MainCameraInstance._DefendingCountry._Proxy.UpdateDetails();*/

                MainCameraComponent._MainCameraInstance._AttackingCountry._TroopsCount = 1;
                MainCameraComponent._MainCameraInstance._AttackingCountry._TroopDisplay.text = "1";

                /*if (MainCameraComponent._MainCameraInstance._AttackingCountry._HasProxy)
                    MainCameraComponent._MainCameraInstance._AttackingCountry._Proxy.UpdateDetails();

                BoardComponent._BoardInstance._IncreaseButtonValue[0].text = "";

                int temp2 = MainCameraComponent._MainCameraInstance._DefendingCountry._TroopsCount - 1;
                BoardComponent._BoardInstance._DecreaseButtonValue[0].text = temp2.ToString();*/

                MaxTroopsButtonsValueUpdate(MainCameraComponent._MainCameraInstance._DefendingCountry._TroopsCount, true);
            }
        }
    }

    [ClientRpc]
    void MaxTroopsButtonsValueUpdate(int troopCount, bool isMove)
    {
        if (!isMove)
        {
            if (MainCameraComponent._MainCameraInstance._AttackingCountry._TroopDisplay.color != Color.magenta)
            {
                MainCameraComponent._MainCameraInstance._AttackingCountry._TroopDisplay.color = Color.magenta;
                _TroopsAdded.Add(MainCameraComponent._MainCameraInstance._AttackingCountry);
            }

            if (MainCameraComponent._MainCameraInstance._AttackingCountry._HasProxy)
                MainCameraComponent._MainCameraInstance._AttackingCountry._Proxy.UpdateDetails();

            BoardComponent._BoardInstance._IncreaseButtonValue[0].text = "";

            int temp2 = troopCount - 1;
            BoardComponent._BoardInstance._DecreaseButtonValue[0].text = temp2.ToString();
        }
        else
        {
            if (MainCameraComponent._MainCameraInstance._DefendingCountry._TroopDisplay.color != Color.magenta)
            {
                MainCameraComponent._MainCameraInstance._DefendingCountry._TroopDisplay.color = Color.magenta;
                if (MainCameraComponent._MainCameraInstance._DefendingCountry._HasProxy)
                    MainCameraComponent._MainCameraInstance._DefendingCountry._Proxy._TroopDisplay.color = Color.magenta;
                _TroopsAdded.Add(MainCameraComponent._MainCameraInstance._DefendingCountry);
            }

            if (MainCameraComponent._MainCameraInstance._DefendingCountry._HasProxy)
                MainCameraComponent._MainCameraInstance._DefendingCountry._Proxy.UpdateDetails();

            if (MainCameraComponent._MainCameraInstance._AttackingCountry._HasProxy)
                MainCameraComponent._MainCameraInstance._AttackingCountry._Proxy.UpdateDetails();

            BoardComponent._BoardInstance._IncreaseButtonValue[0].text = "";

            int temp2 = troopCount - 1;
            BoardComponent._BoardInstance._DecreaseButtonValue[0].text = temp2.ToString();
        }
    }

    [Command(requiresAuthority = false)]
    public void AddTroops()
    {
        if (_NewTroops > 0 && GameCanvasComponent._GameInstance._CurrentState == TurnStates.PlaceTroops)
        {
            MainCameraComponent._MainCameraInstance._AttackingCountry._AddedTroops++;
            MainCameraComponent._MainCameraInstance._AttackingCountry._TroopsCount++;
            MainCameraComponent._MainCameraInstance._AttackingCountry._TroopDisplay.text = MainCameraComponent._MainCameraInstance._AttackingCountry._TroopsCount.ToString();

            /*if (MainCameraComponent._MainCameraInstance._AttackingCountry._TroopDisplay.color != Color.magenta)
            {
                MainCameraComponent._MainCameraInstance._AttackingCountry._TroopDisplay.color = Color.magenta;
                _TroopsAdded.Add(MainCameraComponent._MainCameraInstance._AttackingCountry);
            }

            if (MainCameraComponent._MainCameraInstance._AttackingCountry._HasProxy)
                MainCameraComponent._MainCameraInstance._AttackingCountry._Proxy.UpdateDetails();*/

            _NewTroops--;
            _NewTroopsCount.text = _NewTroops.ToString();

            /*if (_NewTroops == 0)
                BoardComponent._BoardInstance._IncreaseButtonValue[0].text = "";
            else
            {
                int temp = MainCameraComponent._MainCameraInstance._AttackingCountry._TroopsCount + 1;
                BoardComponent._BoardInstance._IncreaseButtonValue[0].text = temp.ToString();
            }

            int temp2 = MainCameraComponent._MainCameraInstance._AttackingCountry._TroopsCount - 1;
            BoardComponent._BoardInstance._DecreaseButtonValue[0].text = temp2.ToString();*/

            AddTroopsButtonsValueUpdate(_NewTroops, MainCameraComponent._MainCameraInstance._AttackingCountry._TroopsCount);
        }
        else if (GameCanvasComponent._GameInstance._CurrentState == TurnStates.PlaceCapital)
        {
            MainCameraComponent._MainCameraInstance._AttackingCountry.CmdSetCapital(MainCameraComponent._MainCameraInstance._AttackingCountry._CurColour);

            //MainCameraComponent._MainCameraInstance.ResetSelected();
            GameCanvasComponent._GameInstance.CmdProgressTurn();
            MainCameraComponent._MainCameraInstance.CmdResetCamera();
        }
        else if (GameCanvasComponent._GameInstance._CurrentState == TurnStates.PlaceAirfield)
        {
            MainCameraComponent._MainCameraInstance._AttackingCountry._HasAirfield = true;
            MainCameraComponent._MainCameraInstance._AttackingCountry._HasAirfieldEffect = true;
            MainCameraComponent._MainCameraInstance._AttackingCountry._AirfieldDisplay.gameObject.SetActive(true);

            _Airfields.Add(MainCameraComponent._MainCameraInstance._AttackingCountry);

            ResetAirfield();

            GameCanvasComponent._GameInstance.CmdProgressTurn();
            MainCameraComponent._MainCameraInstance.CmdResetCamera();
        }
    }

    [ClientRpc]
    void AddTroopsButtonsValueUpdate(int newtroops, int troopCount)
    {
        if (MainCameraComponent._MainCameraInstance._AttackingCountry._TroopDisplay.color != Color.magenta)
        {
            MainCameraComponent._MainCameraInstance._AttackingCountry._TroopDisplay.color = Color.magenta;
            _TroopsAdded.Add(MainCameraComponent._MainCameraInstance._AttackingCountry);
        }

        if (MainCameraComponent._MainCameraInstance._AttackingCountry._HasProxy)
            MainCameraComponent._MainCameraInstance._AttackingCountry._Proxy.UpdateDetails();

        if (newtroops == 0)
        {
            BoardComponent._BoardInstance._IncreaseButtonValue[0].text = "";
        }
        else
        {
            int temp = troopCount + 1;
            BoardComponent._BoardInstance._IncreaseButtonValue[0].text = temp.ToString();
        }

        int temp2 = troopCount - 1;
        BoardComponent._BoardInstance._DecreaseButtonValue[0].text = temp2.ToString();
    }

    [Command(requiresAuthority = false)]
    public void MinTroops()
    {
        if (GameCanvasComponent._GameInstance._CurrentState == TurnStates.PlaceTroops && MainCameraComponent._MainCameraInstance._AttackingCountry._AddedTroops > 0)
        {
            int temp = MainCameraComponent._MainCameraInstance._AttackingCountry._AddedTroops;

            MainCameraComponent._MainCameraInstance._AttackingCountry._AddedTroops -= temp;
            MainCameraComponent._MainCameraInstance._AttackingCountry._TroopsCount -= temp;
            MainCameraComponent._MainCameraInstance._AttackingCountry._TroopDisplay.text = MainCameraComponent._MainCameraInstance._AttackingCountry._TroopsCount.ToString();

            /*_TroopsAdded.Remove(MainCameraComponent._MainCameraInstance._AttackingCountry);
            MainCameraComponent._MainCameraInstance._AttackingCountry._TroopDisplay.color = MainCameraComponent._MainCameraInstance._AttackingCountry._OccupyingArmy._TextColour;

            if (MainCameraComponent._MainCameraInstance._AttackingCountry._HasProxy)
                MainCameraComponent._MainCameraInstance._AttackingCountry._Proxy.UpdateDetails();*/

            _NewTroops += temp;
            _NewTroopsCount.text = _NewTroops.ToString();

            /*BoardComponent._BoardInstance._DecreaseButtonValue[0].text = "";

            int temp2 = MainCameraComponent._MainCameraInstance._AttackingCountry._TroopsCount + 1;
            BoardComponent._BoardInstance._IncreaseButtonValue[0].text = temp2.ToString();*/

            MinTroopsButtonsValueUpdate(MainCameraComponent._MainCameraInstance._AttackingCountry._TroopsCount, false);
        }
        else if (GameCanvasComponent._GameInstance._CurrentState == TurnStates.BattleMove || GameCanvasComponent._GameInstance._CurrentState == TurnStates.Move || GameCanvasComponent._GameInstance._CurrentState == TurnStates.AdditionalMove || GameCanvasComponent._GameInstance._CurrentState == TurnStates.EarlyMove)
        {
            if (MainCameraComponent._MainCameraInstance._DefendingCountry._AddedTroops > 0)
            {
                int temp = MainCameraComponent._MainCameraInstance._DefendingCountry._AddedTroops;

                MainCameraComponent._MainCameraInstance._DefendingCountry._AddedTroops -= temp;
                MainCameraComponent._MainCameraInstance._DefendingCountry._TroopsCount -= temp;
                MainCameraComponent._MainCameraInstance._DefendingCountry._TroopDisplay.text = MainCameraComponent._MainCameraInstance._DefendingCountry._TroopsCount.ToString();

                _TroopsAdded.Remove(MainCameraComponent._MainCameraInstance._DefendingCountry);
                /*if (MainCameraComponent._MainCameraInstance._DefendingCountry._HasProxy)
                {
                    MainCameraComponent._MainCameraInstance._DefendingCountry._Proxy._TroopDisplay.color = MainCameraComponent._MainCameraInstance._DefendingCountry._OccupyingArmy._TextColour;
                    MainCameraComponent._MainCameraInstance._DefendingCountry._Proxy.UpdateDetails();
                }
                MainCameraComponent._MainCameraInstance._DefendingCountry._TroopDisplay.color = MainCameraComponent._MainCameraInstance._DefendingCountry._OccupyingArmy._TextColour;*/

                
                    

                MainCameraComponent._MainCameraInstance._AttackingCountry._TroopsCount += temp;
                MainCameraComponent._MainCameraInstance._AttackingCountry._TroopDisplay.text = MainCameraComponent._MainCameraInstance._AttackingCountry._TroopsCount.ToString();

                /*if (MainCameraComponent._MainCameraInstance._AttackingCountry._HasProxy)
                    MainCameraComponent._MainCameraInstance._AttackingCountry._Proxy.UpdateDetails();

                BoardComponent._BoardInstance._DecreaseButtonValue[0].text = "";

                int temp2 = MainCameraComponent._MainCameraInstance._DefendingCountry._TroopsCount + 1;
                BoardComponent._BoardInstance._IncreaseButtonValue[0].text = temp2.ToString();*/
                MinTroopsButtonsValueUpdate(MainCameraComponent._MainCameraInstance._DefendingCountry._TroopsCount, true);
            }
        }
        /*else if (GameCanvasComponent._GameInstance._CurrentState == TurnStates.BattleMove)
        {
            if (MainCameraComponent._MainCameraInstance._DefendingCountry._AddedTroops > 0 && MainCameraComponent._MainCameraInstance._DefendingCountry._TroopsCount > 3)
            {
                int temp = MainCameraComponent._MainCameraInstance._DefendingCountry._AddedTroops;

                MainCameraComponent._MainCameraInstance._DefendingCountry._AddedTroops -= temp;
                MainCameraComponent._MainCameraInstance._DefendingCountry._TroopsCount -= temp;
                MainCameraComponent._MainCameraInstance._DefendingCountry._TroopDisplay.text = MainCameraComponent._MainCameraInstance._DefendingCountry._TroopsCount.ToString();

                _TroopsAdded.Remove(MainCameraComponent._MainCameraInstance._DefendingCountry);
                if (MainCameraComponent._MainCameraInstance._DefendingCountry._HasProxy)
                    MainCameraComponent._MainCameraInstance._DefendingCountry._Proxy._TroopDisplay.color = Color.black;
                MainCameraComponent._MainCameraInstance._DefendingCountry._TroopDisplay.color = Color.black;

                MainCameraComponent._MainCameraInstance._AttackingCountry._TroopsCount += temp;
                MainCameraComponent._MainCameraInstance._AttackingCountry._TroopDisplay.text = MainCameraComponent._MainCameraInstance._AttackingCountry._TroopsCount.ToString();

                if (MainCameraComponent._MainCameraInstance._AttackingCountry._HasProxy)
                    MainCameraComponent._MainCameraInstance._AttackingCountry._Proxy.UpdateDetails();

                BoardComponent._BoardInstance._DecreaseButtonValue.text = "";

                int temp2 = MainCameraComponent._MainCameraInstance._DefendingCountry._TroopsCount + 1;
                BoardComponent._BoardInstance._IncreaseButtonValue.text = temp2.ToString();
            }
        }*/
    }

    [ClientRpc]
    void MinTroopsButtonsValueUpdate(int troopCount, bool isMove)
    {
        if (!isMove)
        {


            _TroopsAdded.Remove(MainCameraComponent._MainCameraInstance._AttackingCountry);
            MainCameraComponent._MainCameraInstance._AttackingCountry._TroopDisplay.color = MainCameraComponent._MainCameraInstance._AttackingCountry._OccupyingArmy._TextColour;

            if (MainCameraComponent._MainCameraInstance._AttackingCountry._HasProxy)
                MainCameraComponent._MainCameraInstance._AttackingCountry._Proxy.UpdateDetails();

            BoardComponent._BoardInstance._DecreaseButtonValue[0].text = "";

            int temp2 = troopCount + 1;
            BoardComponent._BoardInstance._IncreaseButtonValue[0].text = temp2.ToString();
        }
        else
        {
            if (MainCameraComponent._MainCameraInstance._DefendingCountry._HasProxy)
            {
                MainCameraComponent._MainCameraInstance._DefendingCountry._Proxy._TroopDisplay.color = MainCameraComponent._MainCameraInstance._DefendingCountry._OccupyingArmy._TextColour;
                MainCameraComponent._MainCameraInstance._DefendingCountry._Proxy.UpdateDetails();
            }
            MainCameraComponent._MainCameraInstance._DefendingCountry._TroopDisplay.color = MainCameraComponent._MainCameraInstance._DefendingCountry._OccupyingArmy._TextColour;

            if (MainCameraComponent._MainCameraInstance._AttackingCountry._HasProxy)
                MainCameraComponent._MainCameraInstance._AttackingCountry._Proxy.UpdateDetails();

            BoardComponent._BoardInstance._DecreaseButtonValue[0].text = "";

            int temp2 = troopCount + 1;
            BoardComponent._BoardInstance._IncreaseButtonValue[0].text = temp2.ToString();
        }
    }

    [Command(requiresAuthority = false)]
    public void SubtractTroops()
    {
        if (MainCameraComponent._MainCameraInstance._AttackingCountry._AddedTroops > 0 && GameCanvasComponent._GameInstance._CurrentState == TurnStates.PlaceTroops)
        {
            MainCameraComponent._MainCameraInstance._AttackingCountry._AddedTroops--;
            MainCameraComponent._MainCameraInstance._AttackingCountry._TroopsCount--;
            MainCameraComponent._MainCameraInstance._AttackingCountry._TroopDisplay.text = MainCameraComponent._MainCameraInstance._AttackingCountry._TroopsCount.ToString();
            /*if (MainCameraComponent._MainCameraInstance._AttackingCountry._AddedTroops == 0)
            {
                _TroopsAdded.Remove(MainCameraComponent._MainCameraInstance._AttackingCountry);
                MainCameraComponent._MainCameraInstance._AttackingCountry._TroopDisplay.color = MainCameraComponent._MainCameraInstance._AttackingCountry._OccupyingArmy._TextColour;
            }

            if (MainCameraComponent._MainCameraInstance._AttackingCountry._HasProxy)
                MainCameraComponent._MainCameraInstance._AttackingCountry._Proxy.UpdateDetails();*/

            _NewTroops++;
            _NewTroopsCount.text = _NewTroops.ToString();

            /*if (MainCameraComponent._MainCameraInstance._AttackingCountry._AddedTroops == 0)
                BoardComponent._BoardInstance._DecreaseButtonValue[0].text = "";
            else
            {
                int temp = MainCameraComponent._MainCameraInstance._AttackingCountry._TroopsCount - 1;
                BoardComponent._BoardInstance._DecreaseButtonValue[0].text = temp.ToString();
            }

            int temp2 = MainCameraComponent._MainCameraInstance._AttackingCountry._TroopsCount + 1;
            BoardComponent._BoardInstance._IncreaseButtonValue[0].text = temp2.ToString();*/

            SubtractTroopsButtonsValueUpdate(MainCameraComponent._MainCameraInstance._AttackingCountry._AddedTroops, MainCameraComponent._MainCameraInstance._AttackingCountry._TroopsCount);
        }
        else if (GameCanvasComponent._GameInstance._CurrentState == TurnStates.PlaceCapital)
        {
            //MainCameraComponent._MainCameraInstance.ResetSelected();
            MainCameraComponent._MainCameraInstance.CmdResetCamera();
        }
        else if (GameCanvasComponent._GameInstance._CurrentState == TurnStates.PlaceAirfield)
        {
            //MainCameraComponent._MainCameraInstance.ResetSelected();
            MainCameraComponent._MainCameraInstance.CmdResetCamera();
        }
    }

    [ClientRpc]
    void SubtractTroopsButtonsValueUpdate(int addedTroops, int troopCount)
    {

        if (addedTroops == 0)
        {
            BoardComponent._BoardInstance._DecreaseButtonValue[0].text = "";

            _TroopsAdded.Remove(MainCameraComponent._MainCameraInstance._AttackingCountry);
            MainCameraComponent._MainCameraInstance._AttackingCountry._TroopDisplay.color = MainCameraComponent._MainCameraInstance._AttackingCountry._OccupyingArmy._TextColour;
        }
        else
        {
            int temp = troopCount - 1;
            BoardComponent._BoardInstance._DecreaseButtonValue[0].text = temp.ToString();
        }

        int temp2 = troopCount + 1;
        BoardComponent._BoardInstance._IncreaseButtonValue[0].text = temp2.ToString();

        if (MainCameraComponent._MainCameraInstance._AttackingCountry._HasProxy)
            MainCameraComponent._MainCameraInstance._AttackingCountry._Proxy.UpdateDetails();
    }

    [Command(requiresAuthority = false)]
    public void MoveTroopsTo()
    {
        if (GameCanvasComponent._GameInstance._CurrentState == TurnStates.BattleMove || GameCanvasComponent._GameInstance._CurrentState == TurnStates.Move || GameCanvasComponent._GameInstance._CurrentState == TurnStates.AdditionalMove || GameCanvasComponent._GameInstance._CurrentState == TurnStates.EarlyMove)
        {
            if (MainCameraComponent._MainCameraInstance._AttackingCountry._TroopsCount > 1)
            {
                MainCameraComponent._MainCameraInstance._DefendingCountry._AddedTroops++;
                MainCameraComponent._MainCameraInstance._DefendingCountry._TroopsCount++;
                MainCameraComponent._MainCameraInstance._DefendingCountry._TroopDisplay.text = MainCameraComponent._MainCameraInstance._DefendingCountry._TroopsCount.ToString();

                /*if (MainCameraComponent._MainCameraInstance._DefendingCountry._TroopDisplay.color != Color.magenta)
                {
                    MainCameraComponent._MainCameraInstance._DefendingCountry._TroopDisplay.color = Color.magenta;
                    if (MainCameraComponent._MainCameraInstance._DefendingCountry._HasProxy)
                        MainCameraComponent._MainCameraInstance._DefendingCountry._Proxy._TroopDisplay.color = Color.magenta;
                    _TroopsAdded.Add(MainCameraComponent._MainCameraInstance._DefendingCountry);
                }

                if (MainCameraComponent._MainCameraInstance._DefendingCountry._HasProxy)
                    MainCameraComponent._MainCameraInstance._DefendingCountry._Proxy.UpdateDetails();*/

                MainCameraComponent._MainCameraInstance._AttackingCountry._TroopsCount--;
                MainCameraComponent._MainCameraInstance._AttackingCountry._TroopDisplay.text = MainCameraComponent._MainCameraInstance._AttackingCountry._TroopsCount.ToString();

                /*if (MainCameraComponent._MainCameraInstance._AttackingCountry._HasProxy)
                    MainCameraComponent._MainCameraInstance._AttackingCountry._Proxy.UpdateDetails();

                if (MainCameraComponent._MainCameraInstance._AttackingCountry._TroopsCount == 1)
                    BoardComponent._BoardInstance._IncreaseButtonValue[0].text = "";
                else
                {
                    int temp = MainCameraComponent._MainCameraInstance._DefendingCountry._TroopsCount + 1;
                    BoardComponent._BoardInstance._IncreaseButtonValue[0].text = temp.ToString();
                }

                int temp2 = MainCameraComponent._MainCameraInstance._DefendingCountry._TroopsCount - 1;
                BoardComponent._BoardInstance._DecreaseButtonValue[0].text = temp2.ToString();*/

                MoveTroopsToButtonsValueUpdate(MainCameraComponent._MainCameraInstance._AttackingCountry._TroopsCount, MainCameraComponent._MainCameraInstance._DefendingCountry._TroopsCount);
            }
        }
    }

    [ClientRpc]
    void MoveTroopsToButtonsValueUpdate(int troopCountA, int troopCountD)
    {
        if (MainCameraComponent._MainCameraInstance._DefendingCountry._TroopDisplay.color != Color.magenta)
        {
            MainCameraComponent._MainCameraInstance._DefendingCountry._TroopDisplay.color = Color.magenta;
            if (MainCameraComponent._MainCameraInstance._DefendingCountry._HasProxy)
                MainCameraComponent._MainCameraInstance._DefendingCountry._Proxy._TroopDisplay.color = Color.magenta;
            _TroopsAdded.Add(MainCameraComponent._MainCameraInstance._DefendingCountry);
        }

        if (MainCameraComponent._MainCameraInstance._DefendingCountry._HasProxy)
            MainCameraComponent._MainCameraInstance._DefendingCountry._Proxy.UpdateDetails();

        if (MainCameraComponent._MainCameraInstance._AttackingCountry._HasProxy)
            MainCameraComponent._MainCameraInstance._AttackingCountry._Proxy.UpdateDetails();

        if (troopCountA == 1)
            BoardComponent._BoardInstance._IncreaseButtonValue[0].text = "";
        else
        {
            int temp = troopCountD + 1;
            BoardComponent._BoardInstance._IncreaseButtonValue[0].text = temp.ToString();
        }

        int temp2 = troopCountD - 1;
        BoardComponent._BoardInstance._DecreaseButtonValue[0].text = temp2.ToString();
    }

    [Command(requiresAuthority = false)]
    public void MoveTroopsBack()
    {
        if (GameCanvasComponent._GameInstance._CurrentState == TurnStates.BattleMove || GameCanvasComponent._GameInstance._CurrentState == TurnStates.Move || GameCanvasComponent._GameInstance._CurrentState == TurnStates.AdditionalMove || GameCanvasComponent._GameInstance._CurrentState == TurnStates.EarlyMove)
        {
            if (MainCameraComponent._MainCameraInstance._DefendingCountry._AddedTroops > 0)
            {
                MainCameraComponent._MainCameraInstance._DefendingCountry._AddedTroops--;
                MainCameraComponent._MainCameraInstance._DefendingCountry._TroopsCount--;
                MainCameraComponent._MainCameraInstance._DefendingCountry._TroopDisplay.text = MainCameraComponent._MainCameraInstance._DefendingCountry._TroopsCount.ToString();
                /*if (MainCameraComponent._MainCameraInstance._DefendingCountry._AddedTroops == 0)
                {
                    _TroopsAdded.Remove(MainCameraComponent._MainCameraInstance._DefendingCountry);
                    if (MainCameraComponent._MainCameraInstance._DefendingCountry._HasProxy)
                        MainCameraComponent._MainCameraInstance._DefendingCountry._Proxy._TroopDisplay.color = MainCameraComponent._MainCameraInstance._DefendingCountry._OccupyingArmy._TextColour;
                    MainCameraComponent._MainCameraInstance._DefendingCountry._TroopDisplay.color = MainCameraComponent._MainCameraInstance._DefendingCountry._OccupyingArmy._TextColour;
                }

                if (MainCameraComponent._MainCameraInstance._DefendingCountry._HasProxy)
                    MainCameraComponent._MainCameraInstance._DefendingCountry._Proxy.UpdateDetails();*/

                MainCameraComponent._MainCameraInstance._AttackingCountry._TroopsCount++;
                MainCameraComponent._MainCameraInstance._AttackingCountry._TroopDisplay.text = MainCameraComponent._MainCameraInstance._AttackingCountry._TroopsCount.ToString();

                /*if (MainCameraComponent._MainCameraInstance._AttackingCountry._HasProxy)
                    MainCameraComponent._MainCameraInstance._AttackingCountry._Proxy.UpdateDetails();

                if (MainCameraComponent._MainCameraInstance._DefendingCountry._AddedTroops == 0)
                    BoardComponent._BoardInstance._DecreaseButtonValue[0].text = "";
                else
                {
                    int temp = MainCameraComponent._MainCameraInstance._DefendingCountry._TroopsCount - 1;
                    BoardComponent._BoardInstance._DecreaseButtonValue[0].text = temp.ToString();
                }

                int temp2 = MainCameraComponent._MainCameraInstance._DefendingCountry._TroopsCount + 1;
                BoardComponent._BoardInstance._IncreaseButtonValue[0].text = temp2.ToString();*/

                MoveTroopsBackButtonsValueUpdate(MainCameraComponent._MainCameraInstance._DefendingCountry._AddedTroops, MainCameraComponent._MainCameraInstance._DefendingCountry._TroopsCount);
            }
        }
        /*else if (GameCanvasComponent._GameInstance._CurrentState == TurnStates.BattleMove)
        {
            if (MainCameraComponent._MainCameraInstance._DefendingCountry._AddedTroops > 0 && MainCameraComponent._MainCameraInstance._DefendingCountry._TroopsCount > 3)
            {
                MainCameraComponent._MainCameraInstance._DefendingCountry._AddedTroops--;
                MainCameraComponent._MainCameraInstance._DefendingCountry._TroopsCount--;
                MainCameraComponent._MainCameraInstance._DefendingCountry._TroopDisplay.text = MainCameraComponent._MainCameraInstance._DefendingCountry._TroopsCount.ToString();
                if (MainCameraComponent._MainCameraInstance._DefendingCountry._AddedTroops == 0)
                {
                    _TroopsAdded.Remove(MainCameraComponent._MainCameraInstance._DefendingCountry);
                    if (MainCameraComponent._MainCameraInstance._DefendingCountry._HasProxy)
                        MainCameraComponent._MainCameraInstance._DefendingCountry._Proxy._TroopDisplay.color = Color.black;
                    MainCameraComponent._MainCameraInstance._DefendingCountry._TroopDisplay.color = Color.black;
                }

                if (MainCameraComponent._MainCameraInstance._DefendingCountry._HasProxy)
                    MainCameraComponent._MainCameraInstance._DefendingCountry._Proxy.UpdateDetails();

                MainCameraComponent._MainCameraInstance._AttackingCountry._TroopsCount++;
                MainCameraComponent._MainCameraInstance._AttackingCountry._TroopDisplay.text = MainCameraComponent._MainCameraInstance._AttackingCountry._TroopsCount.ToString();

                if (MainCameraComponent._MainCameraInstance._AttackingCountry._HasProxy)
                    MainCameraComponent._MainCameraInstance._AttackingCountry._Proxy.UpdateDetails();

                if (MainCameraComponent._MainCameraInstance._DefendingCountry._AddedTroops == 0)
                    BoardComponent._BoardInstance._DecreaseButtonValue.text = "";
                else
                {
                    int temp = MainCameraComponent._MainCameraInstance._DefendingCountry._TroopsCount - 1;
                    BoardComponent._BoardInstance._DecreaseButtonValue.text = temp.ToString();
                }

                int temp2 = MainCameraComponent._MainCameraInstance._DefendingCountry._TroopsCount + 1;
                BoardComponent._BoardInstance._IncreaseButtonValue.text = temp2.ToString();
            }
        }*/
    }

    [ClientRpc]
    void MoveTroopsBackButtonsValueUpdate(int addedTroops, int troopCount)
    {
        if (addedTroops == 0)
        {
            _TroopsAdded.Remove(MainCameraComponent._MainCameraInstance._DefendingCountry);
            if (MainCameraComponent._MainCameraInstance._DefendingCountry._HasProxy)
                MainCameraComponent._MainCameraInstance._DefendingCountry._Proxy._TroopDisplay.color = MainCameraComponent._MainCameraInstance._DefendingCountry._OccupyingArmy._TextColour;
            MainCameraComponent._MainCameraInstance._DefendingCountry._TroopDisplay.color = MainCameraComponent._MainCameraInstance._DefendingCountry._OccupyingArmy._TextColour;
        }

        if (MainCameraComponent._MainCameraInstance._DefendingCountry._HasProxy)
            MainCameraComponent._MainCameraInstance._DefendingCountry._Proxy.UpdateDetails();

        if (MainCameraComponent._MainCameraInstance._AttackingCountry._HasProxy)
            MainCameraComponent._MainCameraInstance._AttackingCountry._Proxy.UpdateDetails();

        if (addedTroops == 0)
            BoardComponent._BoardInstance._DecreaseButtonValue[0].text = "";
        else
        {
            int temp = troopCount - 1;
            BoardComponent._BoardInstance._DecreaseButtonValue[0].text = temp.ToString();
        }

        int temp2 = troopCount + 1;
        BoardComponent._BoardInstance._IncreaseButtonValue[0].text = temp2.ToString();
    }

    public bool CheckCountriesConnection(CountryComponent Provider, CountryComponent Reciever)
    {
        Provider._ConnectedToProvider = true;
        Reciever._ConnectedToReciever = true;

        bool foundConnection = false;
        List<CountryComponent> countriesToCheck = new List<CountryComponent>();
        List<CountryComponent> countriesChecked = new List<CountryComponent>();
        countriesToCheck.Add(Provider);
        countriesToCheck.Add(Reciever);
        Provider._BeenAdded = true;
        Reciever._BeenAdded = true;

        while (countriesToCheck.Count > 0)
        {
            int countryCount = countriesToCheck.Count;
            for (int i = 0; i < countryCount; i++)
            {
                foreach (CountryComponent c in countriesToCheck[i]._NeighbouringCountries)
                {
                    if (c._CurColour == countriesToCheck[i]._CurColour)
                    {
                        if (countriesToCheck[i]._ConnectedToProvider)
                            c._ConnectedToProvider = true;
                        if (countriesToCheck[i]._ConnectedToReciever)
                            c._ConnectedToReciever = true;

                        if (!c._BeenAdded)
                        {
                            c._BeenAdded = true;
                            countriesToCheck.Add(c);
                        }
                    }
                }

                countriesToCheck[i]._BeenChecked = true;
            }

            for (int i = 0; i < countriesToCheck.Count; i++)
            {
                if (countriesToCheck[i]._ConnectedToProvider && countriesToCheck[i]._ConnectedToReciever)
                {
                    foundConnection = true;
                    foreach (CountryComponent c in countriesToCheck)
                    {
                        countriesChecked.Add(c);
                    }
                    countriesToCheck.Clear();
                }
                else if (countriesToCheck[i]._BeenChecked)
                {
                    countriesChecked.Add(countriesToCheck[i]);
                    countriesToCheck.Remove(countriesToCheck[i]);
                    i--;
                }
            }
        }

        foreach (CountryComponent c in countriesChecked)
        {
            c._ConnectedToProvider = false;
            c._ConnectedToReciever = false;
            c._BeenChecked = false;
            c._BeenAdded = false;
        }

        return foundConnection;
        
    }
}
