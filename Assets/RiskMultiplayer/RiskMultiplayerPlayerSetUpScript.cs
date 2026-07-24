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

    public bool temp = false;

    void Awake()
    {
        _SceneScript = FindObjectOfType<RiskMultiplayerSetUpSceneScript>();
        _NameInputField.onEndEdit.AddListener(delegate { ValueChangedCheck(); });
    }
    void OnNameChanged(string old, string _new)
    {
        this.name = _PlayerName;
        _NameInputField.text = _PlayerName;
    }

    void OnColourChanged(Color old, Color _new)
    {
        
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
        string name = _PlayerName = "Player " + _SceneScript._Players.Count;
        int ind = _SceneScript._Players.Count;
        for (int i = 0; i < _SceneScript._Players.Count; i++)
        {
            if (name == _SceneScript._Players[i]._PlayerName)
            {
                name = "Player " + ind;
                ind++;
                i = 0;
            }
        }

        _PlayerName = name;
        _SceneScript._Players.Add(this);
        _SceneScript._PlayerCount++;
    }

    [Command]
    public void CmdChangePlayerName(string text)
    {
        for (int i = 0; i < _SceneScript._Players.Count; i++)
        {
            if (text == _SceneScript._Players[i]._PlayerName)
            {
                text = text + " Copy";
                i = 0;
            }
        }

        _PlayerName = text;
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
