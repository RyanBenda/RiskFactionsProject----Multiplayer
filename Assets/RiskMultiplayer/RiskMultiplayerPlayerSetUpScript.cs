using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

public class RiskMultiplayerPlayerSetUpScript : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnNameChanged))]
    public string _PlayerName;

    [SyncVar(hook = nameof(OnColourChanged))]
    public Color _PlayerColour;

    [SyncVar(hook = nameof(OnParentChanged))]
    public Transform _Parent;

    //[ShowInInspector]
    public RiskMultiplayerSetUpSceneScript _SceneScript;

    //[SyncVar(hook = nameof(OnInputFieldChanged))]
    public TMP_InputField _NameInputField;
    public Button _ColourButton;

    public Transform _ColourSelectorPos;
    public RiskArmyColourSelector _ColourSelector;
    public int _ColourIndex;

    public bool temp = false;

    void Awake()
    {
        _SceneScript = FindAnyObjectByType<RiskMultiplayerSetUpSceneScript>();
        _NameInputField.onEndEdit.AddListener(delegate { ValueChangedCheck(); });
        _ColourSelector = FindAnyObjectByType<RiskArmyColourSelector>(FindObjectsInactive.Include);
        _ColourButton.onClick.AddListener(delegate { SetColourSelectorActive(); });
    }
    void OnNameChanged(string old, string _new)
    {
        this.name = _PlayerName;
        _NameInputField.text = _PlayerName;
    }

    void OnColourChanged(Color old, Color _new)
    {
        _ColourButton.image.color = _PlayerColour;
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

        //_SceneScript._PlayerScript = this;

        CmdSetupPlayer();
    }

    [Command]
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

        int ind = 0;
        while (_SceneScript._ArmyChoices[ind]._Chosen) // would cause issues if more people joined then possible armies but there are 42 territories and 42 army options so player 43 couldn't even play anyway
        {
            ind++;

            if (ind == _SceneScript._ArmyChoices.Count)
                ind = 0;
        }

        _PlayerName = _SceneScript._ArmyChoices[ind]._Army._ArmyName;
        _PlayerColour = _SceneScript._ArmyChoices[ind]._Army._ArmyColour;
        _ColourIndex = ind;
        //_ColourSelector._Buttons[ind]._Army._Chosen = true;


        ArmySelection temp = new ArmySelection();
        temp._Army = _SceneScript._ArmyChoices[ind]._Army;
        temp._Chosen = true;
        _SceneScript._ArmyChoices.OnSet(ind, temp);

        _SceneScript._Players.Add(this);
        _SceneScript._PlayerCount++;
    }

    [Command]
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

    [Command]
    public void CmdChangePlayerColour(Color colour)
    {
        _PlayerColour = colour;
    }

    public void SetOtherTextInputs()
    {
        if (isLocalPlayer)
        {
            for (int i = 1; i < _SceneScript._PlayersLocal.Count; i++)
            {
                _SceneScript._PlayersLocal[i]._NameInputField.interactable = false;
                _SceneScript._PlayersLocal[i]._ColourButton.interactable = false;
            }
        }
    }

    public void SetOtherColourButtons()
    {
        _ColourSelector.OnEnable();

        if (isLocalPlayer)
        {
            

            //Debug.Log(_PlayerName);
        }
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
        if (temp)
        {
            SetOtherTextInputs();
            //Debug.Log(_NameInputField.text);
        }
    }
}
