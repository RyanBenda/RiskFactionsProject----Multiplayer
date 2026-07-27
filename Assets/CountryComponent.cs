using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;
using Mirror;

public class CountryComponent : NetworkBehaviour
{
    [SyncVar]
    public ArmiesClass2 _OccupyingArmy;
    public bool _NewOccupier = false;

    public GameObject _HoverObject;
    [SyncVar]
    public bool _Selected = false;
    [SyncVar]
    public bool _MouseHoverTracker = false;

    [SyncVar(hook = nameof(OnTroopsChanged))]
    public int _TroopsCount = 1;
    [SyncVar]
    public int _AddedTroops = 0;
    public TextMeshProUGUI _TroopDisplay;

    [SyncVar(hook = nameof(OnColourChanged))]
    public Color _CurColour;
    public Image[] _CountryColour;
    [SyncVar]
    public Color _CapitalColour;

    public ContinentComponent _Continent;

    public CountryComponent[] _NeighbouringCountries;
    public Transform[] _CameraPositions;

    [SyncVar(hook = nameof(OnCityAdded))]
    public bool _HasCity = false;
    [SyncVar(hook = nameof(OnGivenCapital))]
    public bool _IsCapital = false;
    public TextMeshProUGUI _CityDisplay;
    public TextMeshProUGUI _CaptialDisplay;

    public bool _ConnectedToProvider = false;
    public bool _ConnectedToReciever = false;
    public bool _BeenAdded = false;
    public bool _BeenChecked = false;

    public TextMeshProUGUI _AirfieldDisplay;
    [SyncVar]
    public bool _HasAirfield = false;
    [SyncVar]
    public bool _HasAirfieldEffect;

    public bool _HasProxy = false;
    public ProxyCountryComponent _Proxy;

    [SyncVar]
    public bool _QuickAttacking = false;

    public bool _IsBorderCountry = false;
    private void Awake()
    {
        //UpdateTroops();
    }

    void OnCityAdded(bool old, bool _new)
    {
        if (_HasCity)
        {
            _CityDisplay.gameObject.SetActive(true);
        }
    }
    void OnColourChanged(Color old, Color _new)
    {
        for (int j = 0; j < _CountryColour.Length; j++)
        {
            _CountryColour[j].color = _CurColour;
        }

        _TroopDisplay.color = _OccupyingArmy._TextColour;
        _CityDisplay.color = _OccupyingArmy._TextColour;
        _CaptialDisplay.color = _OccupyingArmy._TextColour;

        if (_HasProxy)
            _Proxy._TroopDisplay.color = _OccupyingArmy._TextColour;

        if (_HasProxy)
            _Proxy.UpdateDetails();
    }

    void OnTroopsChanged(int old, int _old)
    {
        UpdateTroops();

        if (_HasProxy)
            _Proxy.UpdateDetails();
    }

    [Command(requiresAuthority = false)]
    public void CmdSetCapital(Color c)
    {
        _IsCapital = true;
        _CapitalColour = c;
        //_CaptialDisplay.gameObject.SetActive(true);
    }

    void OnGivenCapital(bool old, bool _new)
    {
        _CaptialDisplay.gameObject.SetActive(true);
    }

    public void UpdateTroops()
    {
        _TroopDisplay.text = _TroopsCount.ToString();

        if (_HasProxy)
            _Proxy.UpdateDetails();
    }

    public void MouseEnter()
    {
        _HoverObject.SetActive(true);

        if (_Selected)
            _MouseHoverTracker = true;

        MainCameraComponent._MainCameraInstance._HoveredCountry = this;
    }

    public void MouseExit()
    {
        if(!_Selected)
            _HoverObject.SetActive(false);
        else
            _MouseHoverTracker = false;

        if (MainCameraComponent._MainCameraInstance._HoveredCountry == this)
            MainCameraComponent._MainCameraInstance._HoveredCountry = null;
    }

    public void MouseClick()
    {
        if (GameCanvasComponent._GameInstance._LocalPlayer._IsTurn)
        {
            if (GameCanvasComponent._GameInstance._CurrentState == TurnStates.PlaceTroops)
            {
                if (!MainCameraComponent._MainCameraInstance._Tweening && MainCameraComponent._MainCameraInstance._AttackingCountry == null)
                {
                    if (this._CurColour == BoardComponent._BoardInstance._GameCanvas._CurrentArmyBanner.color)
                    {
                        MainCameraComponent._MainCameraInstance._Tweening = true;
                        MainCameraComponent._MainCameraInstance._AttackingCountry = this;
                        _MouseHoverTracker = true;
                        _Selected = true;
                        _HoverObject.SetActive(true);
                        MainCameraComponent._MainCameraInstance.transform.DOMove(new Vector3(this.transform.position.x, this.transform.position.y, -350), 1).OnComplete(() => SetUpNewTroopsButtons());
                    }
                    //return;
                }
            }
            else if (GameCanvasComponent._GameInstance._CurrentState == TurnStates.Battle)
            {
                if (!MainCameraComponent._MainCameraInstance._Tweening && MainCameraComponent._MainCameraInstance._AttackingCountry == null)
                {
                    if (this._CurColour == BoardComponent._BoardInstance._GameCanvas._CurrentArmyBanner.color && !_QuickAttacking)
                    {
                        MainCameraComponent._MainCameraInstance._Tweening = true;
                        MainCameraComponent._MainCameraInstance._AttackingCountry = this;
                        _MouseHoverTracker = true;
                        _Selected = true;
                        _HoverObject.SetActive(true);
                        MainCameraComponent._MainCameraInstance.transform.DOMove(new Vector3(this.transform.position.x, this.transform.position.y, -350), 1).OnComplete(() => MainCameraComponent._MainCameraInstance._Tweening = false);
                    }
                    return;
                }



                if (!MainCameraComponent._MainCameraInstance._Tweening && MainCameraComponent._MainCameraInstance._AttackingCountry != null && MainCameraComponent._MainCameraInstance._DefendingCountry == null && MainCameraComponent._MainCameraInstance._AttackingCountry._CurColour != this._CurColour && MainCameraComponent._MainCameraInstance._AttackingCountry._TroopsCount > 1)
                {
                    bool neighouringCountry = false;
                    int index = -1;
                    for (int i = 0; i < MainCameraComponent._MainCameraInstance._AttackingCountry._NeighbouringCountries.Length; i++)
                    {
                        if (this == MainCameraComponent._MainCameraInstance._AttackingCountry._NeighbouringCountries[i])
                        {
                            neighouringCountry = true;
                            index = i;
                        }
                    }

                    if (neighouringCountry)
                    {
                        MainCameraComponent._MainCameraInstance._Tweening = true;
                        MainCameraComponent._MainCameraInstance._DefendingCountry = this;
                        _MouseHoverTracker = true;
                        _Selected = true;
                        _HoverObject.SetActive(true);

                        if (index >= MainCameraComponent._MainCameraInstance._AttackingCountry._CameraPositions.Length)
                        {
                            float pox = MainCameraComponent._MainCameraInstance._AttackingCountry.transform.position.x + ((this.transform.position.x - MainCameraComponent._MainCameraInstance._AttackingCountry.transform.position.x) / 2);
                            float poy = MainCameraComponent._MainCameraInstance._AttackingCountry.transform.position.y + ((this.transform.position.y - MainCameraComponent._MainCameraInstance._AttackingCountry.transform.position.y) / 2);

                            //MainCameraComponent._MainCameraInstance.transform.rotation = new Quaternion(MainCameraComponent._MainCameraInstance.transform.rotation.x, MainCameraComponent._MainCameraInstance.transform.rotation.y, Vector3.Angle(MainCameraComponent._MainCameraInstance._AttackingCountry.transform.position, MainCameraComponent._MainCameraInstance._DefendingCountry.transform.position), MainCameraComponent._MainCameraInstance.transform.rotation.w);

                            Vector3 targetDir = MainCameraComponent._MainCameraInstance._DefendingCountry.transform.position - MainCameraComponent._MainCameraInstance._AttackingCountry.transform.position;

                            //Debug.Log(Vector3.Angle(targetDir, MainCameraComponent._MainCameraInstance._AttackingCountry.transform.right));

                            Vector3 newRot;

                            if (MainCameraComponent._MainCameraInstance._DefendingCountry.transform.position.y > +MainCameraComponent._MainCameraInstance._AttackingCountry.transform.position.y)
                                newRot = new Vector3(MainCameraComponent._MainCameraInstance.transform.eulerAngles.x, MainCameraComponent._MainCameraInstance.transform.eulerAngles.y, Vector3.Angle(targetDir, MainCameraComponent._MainCameraInstance._AttackingCountry.transform.right));
                            else
                                newRot = new Vector3(MainCameraComponent._MainCameraInstance.transform.eulerAngles.x, MainCameraComponent._MainCameraInstance.transform.eulerAngles.y, -Vector3.Angle(targetDir, MainCameraComponent._MainCameraInstance._AttackingCountry.transform.right));



                            MainCameraComponent._MainCameraInstance.transform.DOMove(new Vector3(pox, poy, -350), 1.5f).OnComplete(() => MainCameraComponent._MainCameraInstance.ActivateBattleSystem());
                            MainCameraComponent._MainCameraInstance.transform.DORotate(newRot, 1.5f);
                        }
                        else
                        {
                            MainCameraComponent._MainCameraInstance.transform.DOMove(MainCameraComponent._MainCameraInstance._AttackingCountry._CameraPositions[index].position, 1.5f).OnComplete(() => MainCameraComponent._MainCameraInstance.ActivateBattleSystem());
                            MainCameraComponent._MainCameraInstance.transform.DORotate(MainCameraComponent._MainCameraInstance._AttackingCountry._CameraPositions[index].eulerAngles, 1.5f);
                        }
                    }
                }
                else if (!MainCameraComponent._MainCameraInstance._Tweening && MainCameraComponent._MainCameraInstance._DefendingCountry == null && MainCameraComponent._MainCameraInstance._AttackingCountry != this && MainCameraComponent._MainCameraInstance._AttackingCountry._CurColour == this._CurColour)
                {
                    MainCameraComponent._MainCameraInstance._AttackingCountry._Selected = false;
                    MainCameraComponent._MainCameraInstance._AttackingCountry._HoverObject.SetActive(MainCameraComponent._MainCameraInstance._AttackingCountry._MouseHoverTracker);
                    MainCameraComponent._MainCameraInstance._AttackingCountry._MouseHoverTracker = false;

                    MainCameraComponent._MainCameraInstance._Tweening = true;
                    MainCameraComponent._MainCameraInstance._AttackingCountry = this;
                    _MouseHoverTracker = true;
                    _Selected = true;
                    _HoverObject.SetActive(true);
                    MainCameraComponent._MainCameraInstance.transform.DOMove(new Vector3(this.transform.position.x, this.transform.position.y, -350), 1).OnComplete(() => MainCameraComponent._MainCameraInstance._Tweening = false);

                    //return;
                }
            }
            else if (GameCanvasComponent._GameInstance._CurrentState == TurnStates.Move || GameCanvasComponent._GameInstance._CurrentState == TurnStates.AdditionalMove || GameCanvasComponent._GameInstance._CurrentState == TurnStates.EarlyMove)
            {
                if (!MainCameraComponent._MainCameraInstance._Tweening && MainCameraComponent._MainCameraInstance._AttackingCountry == null)
                {
                    if (this._CurColour == BoardComponent._BoardInstance._GameCanvas._CurrentArmyBanner.color)
                    {

                        MainCameraComponent._MainCameraInstance._AttackingCountry = this;
                        _MouseHoverTracker = true;
                        _Selected = true;
                        _HoverObject.SetActive(true);

                    }
                    return;
                }

                if (!MainCameraComponent._MainCameraInstance._Tweening && MainCameraComponent._MainCameraInstance._AttackingCountry != null && MainCameraComponent._MainCameraInstance._AttackingCountry != this && MainCameraComponent._MainCameraInstance._DefendingCountry == null && MainCameraComponent._MainCameraInstance._AttackingCountry._CurColour == this._CurColour && MainCameraComponent._MainCameraInstance._AttackingCountry._TroopsCount > 1)
                {
                    if (BoardComponent._BoardInstance.CheckCountriesConnection(MainCameraComponent._MainCameraInstance._AttackingCountry, this))
                    {
                        MainCameraComponent._MainCameraInstance._DefendingCountry = this;
                        _MouseHoverTracker = true;
                        _Selected = true;
                        _HoverObject.SetActive(true);

                        SetUpMoveTroopsButtons(MainCameraComponent._MainCameraInstance._AttackingCountry);

                    }
                }
            }
            else if (GameCanvasComponent._GameInstance._CurrentState == TurnStates.PlaceCapital)
            {
                if (!MainCameraComponent._MainCameraInstance._Tweening && MainCameraComponent._MainCameraInstance._AttackingCountry == null)
                {
                    if (this._CurColour == BoardComponent._BoardInstance._GameCanvas._CurrentArmyBanner.color)
                    {
                        /*MainCameraComponent._MainCameraInstance._Tweening = true;
                        MainCameraComponent._MainCameraInstance._AttackingCountry = this;
                        _MouseHoverTracker = true;
                        _Selected = true;
                        _HoverObject.SetActive(true);
                        MainCameraComponent._MainCameraInstance.transform.DOMove(new Vector3(this.transform.position.x, this.transform.position.y, -350), 1).OnComplete(() => SetUpCapital());*/
                        ServerCameraInstuctor("Capital", connectionToClient);
                    }
                    //return;
                }
            }
            else if (GameCanvasComponent._GameInstance._CurrentState == TurnStates.PlaceAirfield)
            {
                if (!MainCameraComponent._MainCameraInstance._Tweening && MainCameraComponent._MainCameraInstance._AttackingCountry == null)
                {
                    if (this._CurColour == BoardComponent._BoardInstance._GameCanvas._CurrentArmyBanner.color)
                    {
                        /*MainCameraComponent._MainCameraInstance._Tweening = true;
                        MainCameraComponent._MainCameraInstance._AttackingCountry = this;
                        _MouseHoverTracker = true;
                        _Selected = true;
                        _HoverObject.SetActive(true);
                        MainCameraComponent._MainCameraInstance.transform.DOMove(new Vector3(this.transform.position.x, this.transform.position.y, -350), 1).OnComplete(() => SetUpCapital());*/
                        ServerCameraInstuctor("Capital", connectionToClient);
                    }
                    //return;
                }
            }
        }
    }

    [Command(requiresAuthority = false)]
    void ServerCameraInstuctor(string instruction, NetworkConnectionToClient client)
    {
        //Debug.Log(client.identity.name);

        switch (instruction)
        {
            case "Capital":
                CapitalPlacementCamera(client.identity.name);
                break;
        }
        
    }

    [ClientRpc]
    void CapitalPlacementCamera(string client)
    {
        MainCameraComponent._MainCameraInstance._Tweening = true;
        MainCameraComponent._MainCameraInstance._AttackingCountry = this;
        _MouseHoverTracker = true;
        _Selected = true;
        _HoverObject.SetActive(true);
        MainCameraComponent._MainCameraInstance.transform.DOMove(new Vector3(this.transform.position.x, this.transform.position.y, -350), 1).OnComplete(() => SetUpCapital(client));
    }
    void SetUpCapital(string client)
    {
        MainCameraComponent._MainCameraInstance._Tweening = false;

        if (NetworkClient.connection.identity.name == client)
        {
            BoardComponent._BoardInstance._IncreaseButton[0].transform.position = new Vector3(this.transform.position.x + 75, this.transform.position.y, BoardComponent._BoardInstance._IncreaseButton[0].transform.position.z);
            BoardComponent._BoardInstance._DecreaseButton[0].transform.position = new Vector3(this.transform.position.x - 75, this.transform.position.y, BoardComponent._BoardInstance._DecreaseButton[0].transform.position.z);

            BoardComponent._BoardInstance._IncreaseButtonValue[0].text = "Yes";
            BoardComponent._BoardInstance._DecreaseButtonValue[0].text = "No";

            foreach (TextMeshProUGUI tmp in BoardComponent._BoardInstance._IncreaseButtonValue)
                tmp.color = MainCameraComponent._MainCameraInstance._AttackingCountry._OccupyingArmy._TextColour;
            foreach (TextMeshProUGUI tmp in BoardComponent._BoardInstance._DecreaseButtonValue)
                tmp.color = MainCameraComponent._MainCameraInstance._AttackingCountry._OccupyingArmy._TextColour;

            //BoardComponent._BoardInstance._IncreaseButton.color = this._CurColour;
            foreach (Image i in BoardComponent._BoardInstance._IncreaseButton)
            {
                i.color = this._CurColour;
            }

            //BoardComponent._BoardInstance._DecreaseButton.color = this._CurColour;
            foreach (Image i in BoardComponent._BoardInstance._DecreaseButton)
            {
                i.color = this._CurColour;
            }

            BoardComponent._BoardInstance._IncreaseButton[0].gameObject.SetActive(true);
            BoardComponent._BoardInstance._DecreaseButton[0].gameObject.SetActive(true);
        }
    }

    void SetUpNewTroopsButtons()
    {
        MainCameraComponent._MainCameraInstance._Tweening = false;

        BoardComponent._BoardInstance._IncreaseButton[0].transform.position = new Vector3(this.transform.position.x + 75, this.transform.position.y, BoardComponent._BoardInstance._IncreaseButton[0].transform.position.z);
        BoardComponent._BoardInstance._DecreaseButton[0].transform.position = new Vector3(this.transform.position.x - 75, this.transform.position.y, BoardComponent._BoardInstance._DecreaseButton[0].transform.position.z);

        foreach (TextMeshProUGUI tmp in BoardComponent._BoardInstance._IncreaseButtonValue)
            tmp.color = MainCameraComponent._MainCameraInstance._AttackingCountry._OccupyingArmy._TextColour;
        foreach (TextMeshProUGUI tmp in BoardComponent._BoardInstance._DecreaseButtonValue)
            tmp.color = MainCameraComponent._MainCameraInstance._AttackingCountry._OccupyingArmy._TextColour;


        if (BoardComponent._BoardInstance._NewTroops > 0)
        {
            int temp = _TroopsCount + 1;
            BoardComponent._BoardInstance._IncreaseButtonValue[0].text = temp.ToString(); //MainCameraComponent._MainCameraInstance._AttackingCountry._OccupyingArmy._TextColour
        }
        if (_AddedTroops > 0)
        {
            int temp = _TroopsCount - 1;
            BoardComponent._BoardInstance._DecreaseButtonValue[0].text = temp.ToString();
        }

        //BoardComponent._BoardInstance._IncreaseButton.color = this._CurColour;
        foreach (Image i in BoardComponent._BoardInstance._IncreaseButton)
        {
            i.color = this._CurColour;
        }

        //BoardComponent._BoardInstance._DecreaseButton.color = this._CurColour;
        foreach (Image i in BoardComponent._BoardInstance._DecreaseButton)
        {
            i.color = this._CurColour;
        }


        BoardComponent._BoardInstance._IncreaseButton[0].gameObject.SetActive(true);
        BoardComponent._BoardInstance._DecreaseButton[0].gameObject.SetActive(true);
    }

    public void SetUpMoveTroopsButtons(CountryComponent givingCountry)
    {
        if (!_HasProxy)
        {
            BoardComponent._BoardInstance._IncreaseButton[0].transform.position = new Vector3(this.transform.position.x + 75, this.transform.position.y, BoardComponent._BoardInstance._IncreaseButton[0].transform.position.z);
            BoardComponent._BoardInstance._DecreaseButton[0].transform.position = new Vector3(this.transform.position.x - 75, this.transform.position.y, BoardComponent._BoardInstance._DecreaseButton[0].transform.position.z);
        }
        else
        {
            if (GameCanvasComponent._GameInstance._CurrentState == TurnStates.BattleMove && MainCameraComponent._MainCameraInstance._AttackingCountry != null && MainCameraComponent._MainCameraInstance._AttackingCountry == _Proxy._AttackerCountry)
            {
                BoardComponent._BoardInstance._IncreaseButton[0].transform.position = new Vector3(_Proxy.transform.position.x + 75, _Proxy.transform.position.y, BoardComponent._BoardInstance._IncreaseButton[0].transform.position.z);
                BoardComponent._BoardInstance._DecreaseButton[0].transform.position = new Vector3(_Proxy.transform.position.x - 75, _Proxy.transform.position.y, BoardComponent._BoardInstance._DecreaseButton[0].transform.position.z);
            }
            else
            {
                BoardComponent._BoardInstance._IncreaseButton[0].transform.position = new Vector3(this.transform.position.x + 75, this.transform.position.y, BoardComponent._BoardInstance._IncreaseButton[0].transform.position.z);
                BoardComponent._BoardInstance._DecreaseButton[0].transform.position = new Vector3(this.transform.position.x - 75, this.transform.position.y, BoardComponent._BoardInstance._DecreaseButton[0].transform.position.z);
            }
        }

        foreach (TextMeshProUGUI tmp in BoardComponent._BoardInstance._IncreaseButtonValue)
            tmp.color = MainCameraComponent._MainCameraInstance._AttackingCountry._OccupyingArmy._TextColour;
        foreach (TextMeshProUGUI tmp in BoardComponent._BoardInstance._DecreaseButtonValue)
            tmp.color = MainCameraComponent._MainCameraInstance._AttackingCountry._OccupyingArmy._TextColour;

        if (givingCountry._TroopsCount > 1)
        {
            int temp = _TroopsCount + 1;
            BoardComponent._BoardInstance._IncreaseButtonValue[0].text = temp.ToString();
        }
        if (_AddedTroops > 0)
        {
            int temp = _TroopsCount - 1;
            BoardComponent._BoardInstance._DecreaseButtonValue[0].text = temp.ToString();
        }

        foreach (Image i in BoardComponent._BoardInstance._IncreaseButton)
        {
            i.color = this._CurColour;
        }
        //BoardComponent._BoardInstance._IncreaseButton.color = this._CurColour;

        foreach (Image i in BoardComponent._BoardInstance._DecreaseButton)
        {
            i.color = this._CurColour;
        }

        //BoardComponent._BoardInstance._DecreaseButton.color = this._CurColour;

        BoardComponent._BoardInstance._IncreaseButton[0].gameObject.SetActive(true);
        BoardComponent._BoardInstance._DecreaseButton[0].gameObject.SetActive(true);
    }
}
