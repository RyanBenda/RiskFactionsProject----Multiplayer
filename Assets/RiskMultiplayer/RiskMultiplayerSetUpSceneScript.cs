using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mirror;

[System.Serializable]
public struct ArmySelection
{
    public ArmyScriptableObject _Army;
    public bool _Chosen;
}

public class RiskMultiplayerSetUpSceneScript : NetworkBehaviour
{
    
    public List<RiskMultiplayerPlayerSetUpScript> _PlayersLocal = new List<RiskMultiplayerPlayerSetUpScript>();
    public readonly SyncList<RiskMultiplayerPlayerSetUpScript> _Players = new SyncList<RiskMultiplayerPlayerSetUpScript>();

    [SyncVar(hook = nameof(OnPlayersChanged))]
    public int _PlayerCount = 0;

    public NetworkIdentity _ScrollViewContent;

    public ArmyScriptableObject[] _Armies;
    public readonly SyncList<ArmySelection> _ArmyChoices = new SyncList<ArmySelection>();

    public RiskArmyColourSelector _ArmyColourSelector;

    private void Start()
    {
        if (isServer)
        {
            foreach (ArmyScriptableObject a in _Armies)
            {
                ArmySelection sel = new ArmySelection();
                sel._Army = a;
                sel._Chosen = false;
                _ArmyChoices.Add(sel);
            }
        }
    }

    public override void OnStartClient()
    {
        _Players.OnAdd += OnItemAdded;
        _ArmyChoices.OnSet += OnArmyChoice;
    }

    void OnPlayersChanged(int old, int _new)
    {
        foreach (RiskMultiplayerPlayerSetUpScript p in _Players)
        {
            p.transform.parent = _ScrollViewContent.transform;
            p.transform.localScale = Vector3.one;
        }
    }

    void OnItemAdded(int index)
    {
        int diff = _Players.Count - _PlayersLocal.Count;
        for (int i = _PlayersLocal.Count; i < _Players.Count; i++)
        {
            _PlayersLocal.Add(_Players[i]);
        }
        if (diff > 1)
        {
            _PlayersLocal.Remove(_Players[index]);
            _PlayersLocal.Insert(0, _Players[index]);

            _ScrollViewContent.transform.DetachChildren();
            foreach (RiskMultiplayerPlayerSetUpScript p in _PlayersLocal)
            {
                p.transform.parent = _ScrollViewContent.transform;
                p.transform.localScale = Vector3.one;
            }
        }

        RiskFactionsRoomPlayer setupOwner =  NetworkClient.connection.identity.gameObject.GetComponent<RiskFactionsRoomPlayer>();

        for (int i = 1; i < _PlayersLocal.Count; i++)
        {
            if (_PlayersLocal[i]._Owner != setupOwner)
            {
                _PlayersLocal[i]._NameInputField.interactable = false;
                _PlayersLocal[i]._ColourButton.interactable = false;
            }
        }
    }
    [Command(requiresAuthority = false)]
    void OnArmyChoice(int test, ArmySelection temp2)
    {
        _ArmyChoices.RemoveAt(test);
        _ArmyChoices.Insert(test, temp2);

        if (_ArmyColourSelector != null)
            _ArmyColourSelector.OnEnable(); // Updates the colour buttons to be correct for host client
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
