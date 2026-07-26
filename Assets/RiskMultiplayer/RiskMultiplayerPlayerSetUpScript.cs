using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class RiskMultiplayerPlayerSetUpScript : NetworkRoomPlayer
{
    [SyncVar(hook = nameof(OnNameChanged))]
    public string _PlayerName;

    [SyncVar(hook = nameof(OnColourChanged))]
    public Color _PlayerColour;

    [SyncVar(hook = nameof(OnArmyChanged))]
    public ArmyScriptableObject _SetArmy;

    public RiskMultiplayerSetUpSceneScript _SceneScript;

    public TMP_InputField _NameInputField;
    public Button _ColourButton;

    public Transform _ColourSelectorPos;
    public RiskArmyColourSelector _ColourSelector;

    [SyncVar]
    public int _ColourIndex;

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

    public void ValueChangedCheck()
    {
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
        if (_PlayerColour != Color.clear) return; //Stops this set up from running if it has already been run

        _Owner._Setup = this;

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

        ArmySelection temp = new ArmySelection();
        temp._Army = _SceneScript._ArmyChoices[ind]._Army;
        temp._Chosen = true;
        _SceneScript._ArmyChoices.OnSet(ind, temp);

        _SceneScript._Players.Add(this);
        _SceneScript._PlayerCount++;
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
    }

    [Command(requiresAuthority = false)]
    public void CmdChangePlayerColour(Color colour)
    {
        _PlayerColour = colour;
    }

    [Command(requiresAuthority = false)]
    public void CmdChangePlayerArmy(ArmyScriptableObject army)
    {
        _SetArmy = army;
    }

    // Don't use this anymore but leaving here just in case
    public void SetOtherTextInputs(RiskMultiplayerSetUpSceneScript sceneScript)
    {
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

    // Update is called once per frame
    void Update()
    {
       
    }    
}
