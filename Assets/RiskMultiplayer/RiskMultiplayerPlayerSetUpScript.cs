using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class CustomConnectionData
{
    public string name;
    public Color colour;
    //public int selectedCharacterId;
}

public class RiskMultiplayerPlayerSetUpScript : NetworkRoomPlayer
{
    [SyncVar(hook = nameof(OnNameChanged))]
    public string _PlayerName;

    [SyncVar(hook = nameof(OnColourChanged))]
    public Color _PlayerColour;

    [SyncVar(hook = nameof(OnParentChanged))]
    public Transform _Parent;

    [SyncVar(hook = nameof(OnArmyChanged))]
    public ArmyScriptableObject _SetArmy;

    //[ShowInInspector]
    public RiskMultiplayerSetUpSceneScript _SceneScript;

    //[SyncVar(hook = nameof(OnInputFieldChanged))]
    public TMP_InputField _NameInputField;
    public Button _ColourButton;

    public Transform _ColourSelectorPos;
    public RiskArmyColourSelector _ColourSelector;

    [SyncVar]
    public int _ColourIndex;

    public bool temp = false;

    //public GameObject _RiskFactionsPlayerPrefab;

    public RiskFactionsPlayerScript _RiskFactionsPlayerScript;

    public SetUpDataRecorder _NM;
    int _RecorderIndex = -1;

    [SyncVar]
    public RiskFactionsRoomPlayer _Owner;

    void Awake()
    {
        if (SceneManager.GetActiveScene().name != "SampleScene")
        {
            _SceneScript = FindAnyObjectByType<RiskMultiplayerSetUpSceneScript>();
            _NameInputField.onEndEdit.AddListener(delegate { ValueChangedCheck(); });
            _ColourSelector = FindAnyObjectByType<RiskArmyColourSelector>(FindObjectsInactive.Include);
            _ColourButton.onClick.AddListener(delegate { SetColourSelectorActive(); });

            if (_RiskFactionsPlayerScript == null)
                _RiskFactionsPlayerScript = GetComponent<RiskFactionsPlayerScript>();

            if (_NM == null)
                _NM = FindFirstObjectByType<SetUpDataRecorder>();
        }
    }
    void OnNameChanged(string old, string _new)
    {
        this.name = _PlayerName;
        _NameInputField.text = _PlayerName;

        if (_Owner != null)
            _Owner._ArmyName = _PlayerName;
    }

    void OnColourChanged(Color old, Color _new)
    {
        _ColourButton.image.color = _PlayerColour;
        SetOtherColourButtons(); // Updates the colour buttons to be correct for non host clients

        if (_Owner != null)
            _Owner._ArmyColor = _PlayerColour;
    }

    void OnArmyChanged(ArmyScriptableObject old, ArmyScriptableObject _new)
    {
       
        if (_Owner != null)
            _Owner._Army = _SetArmy;
    }

    void OnParentChanged(Transform old, Transform _new)
    {
        this.transform.parent = _Parent;
    }

    public void ValueChangedCheck()
    {
        Debug.Log(_NameInputField.text);

        CmdChangePlayerName(_NameInputField.text);
    }

    public override void OnStartLocalPlayer()
    {
        CmdSetupPlayer();
        SetOtherColourButtons(); // Updates the colour buttons to be correct when starting
    }

    [Command(requiresAuthority = false)]
    public void CmdSetupPlayer()
    {



        // player info sent to server, then server updates sync vars which handles it on all clients
        /*string name = _PlayerName = "Player " + _SceneScript._Players.Count;
        int ind = _SceneScript._Players.Count;
        for (int i = 0; i < _SceneScript._Players.Count; i++)
        {
            if (name == _SceneScript._Players[i]._PlayerName)
            {
                name = "Player " + ind;
                ind++;
                i = 0;
            }
        }*/


        if (_PlayerColour != Color.clear) return;

        _Owner._Setup = this;
        RiskFactionsRoomPlayer conn = NetworkClient.connection.identity.gameObject.GetComponent<RiskFactionsRoomPlayer>();

       
        
        int ind = 0;
        while (_SceneScript._ArmyChoices[ind]._Chosen) // would cause issues if more people joined then possible armies but there are 42 territories and 42 army options so player 43 couldn't even play anyway
        {
            ind++;

            if (ind == _SceneScript._ArmyChoices.Count)
                ind = 0;
        }

        _PlayerName = _SceneScript._ArmyChoices[ind]._Army._ArmyName;
        _PlayerColour = _SceneScript._ArmyChoices[ind]._Army._ArmyColour;
        _SetArmy = _SceneScript._ArmyChoices[ind]._Army;
        _ColourIndex = ind;
        //_ColourSelector._Buttons[ind]._Army._Chosen = true;

        //Debug.Log(ind);

        ArmySelection temp = new ArmySelection();
        temp._Army = _SceneScript._ArmyChoices[ind]._Army;
        temp._Chosen = true;
        _SceneScript._ArmyChoices.OnSet(ind, temp);

        _SceneScript._Players.Add(this);
        _SceneScript._PlayerCount++;


        ArmySetUpDetails a = new ArmySetUpDetails();
        a._SetName = _PlayerName;
        a._SetColour = _PlayerColour;
        a._SetArmy = _SetArmy;

        _NM._ArmyInfo.Add(a);

        for (int i = 0; i < _NM._ArmyInfo.Count; i++)
        {
            if (_NM._ArmyInfo[i]._SetArmy == a._SetArmy)
            {
                _RecorderIndex = i;
                break;
            }
        }
        
    }

    [Command(requiresAuthority = false)]
    public void CmdChangePlayerName(string text)
    {
        for (int i = 0; i < _SceneScript._Players.Count; i++)
        {
            if (_SceneScript._Players[i] != this && text == _SceneScript._Players[i]._PlayerName)
            {
                text = text + " Copy";
                i = 0;
            }
        }

        _PlayerName = text;
        UpdateClientData();
    }

    [Command(requiresAuthority = false)]
    public void CmdChangePlayerColour(Color colour)
    {
        _PlayerColour = colour;
        UpdateClientData();


    }

    [Command(requiresAuthority = false)]
    public void CmdChangePlayerArmy(ArmyScriptableObject army)
    {
        _SetArmy = army;
        UpdateClientData();
    }

    

    public void SetOtherTextInputs(RiskMultiplayerSetUpSceneScript sceneScript)
    {
        //if (sceneScript == null) return;

        for (int i = 1; i < _SceneScript._PlayersLocal.Count; i++)
        {
            sceneScript._PlayersLocal[i]._NameInputField.interactable = false;
            sceneScript._PlayersLocal[i]._ColourButton.interactable = false;
        }
    }

    public void SetOtherColourButtons()
    {
        if(_ColourSelector != null)
            _ColourSelector.OnEnable();
    }
    public void SetColourSelectorActive()
    {
        if (!_ColourSelector.gameObject.activeSelf)
        {
            _ColourSelector._Requester = this;
            _ColourSelector.transform.position = _ColourSelectorPos.position;
            _ColourSelector.gameObject.SetActive(true);
        }
        else
            _ColourSelector.gameObject.SetActive(false);
    }

    [Command(requiresAuthority = false)]
    public void UpdateClientData()
    {
        ArmySetUpDetails a = new ArmySetUpDetails();
        a._SetName = _PlayerName;
        a._SetColour = _PlayerColour;
        a._SetArmy = _SetArmy;

        _NM._ArmyInfo.RemoveAt(_RecorderIndex);
        _NM._ArmyInfo.Insert(_RecorderIndex, a);
    }

    // Update is called once per frame
    void Update()
    {
        /*if (isLocalPlayer)
        {
            //SetOtherTextInputs();
            //Debug.Log(connectionToClient.identity.gameObject.name);
        }*/

        /*if (isServer && SceneManager.GetActiveScene().name == "SampleScene")
        {
            
            foreach (KeyValuePair<int, NetworkConnectionToClient> p in NetworkServer.connections)
            {
                //Debug.Log(p.Value.identity);
                if (p.Value.identity == null)
                    return;

                Debug.Log("All Connected");
                RiskFactionGameSceneScript sceneScript = FindFirstObjectByType<RiskFactionGameSceneScript>();

                if (p.Value.authenticationData is CustomConnectionData clientData && sceneScript._Players.Count == 0)
                {
                    StartGameNetworkManager NM = FindAnyObjectByType<StartGameNetworkManager>();

                    GameObject newPlayerObject = Instantiate(NM.spawnPrefabs[0]);

                    RiskFactionsPlayerScript gameScript = newPlayerObject.GetComponent<RiskFactionsPlayerScript>();
                    gameScript._PlayerName = clientData.name;
                    gameScript._PlayerColour = clientData.colour;
                    gameScript._SceneScript = sceneScript;

                    NetworkServer.ReplacePlayerForConnection(p.Value, newPlayerObject, ReplacePlayerOptions.KeepAuthority);

                    gameScript._SceneScript._Players.Add(gameScript);
                    
                    Destroy(this.gameObject);
                }
                else
                    Destroy(this.gameObject);
            }
        }*/
    }

    //[ClientRpc]
    public void RpcCreateReplacement(GameObject prefab)
    {
        /*if (isServer)
        {
            SaveClientData(connectionToClient, _PlayerName, _PlayerColour);

            GameObject temp = Instantiate(prefab);
            temp.name = _PlayerName;

            StartGameNetworkManager NM = FindFirstObjectByType<StartGameNetworkManager>();

            NM._Replacements.Add(temp.GetComponent<RiskFactionsPlayerScript>());
            NetworkServer.Spawn(temp);
            DontDestroyOnLoad(temp);

            //NetworkServer.ReplacePlayerForConnection(connectionToClient, temp, ReplacePlayerOptions.KeepAuthority);
        }*/
    }

    /*public override void OnStartAuthority()
    {
        base.OnStartAuthority();

        /*if (isLocalPlayer)
        {
            RiskFactionGameSceneScript sceneScript = FindFirstObjectByType<RiskFactionGameSceneScript>();
            sceneScript._ToBeDeleted.Add(this);
        }
    }*/

    
}
