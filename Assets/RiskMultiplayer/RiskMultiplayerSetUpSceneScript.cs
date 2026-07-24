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

    public Transform _ScrollViewContent;

    public ArmyScriptableObject[] _Armies;
    public readonly SyncList<ArmySelection> _ArmyChoices = new SyncList<ArmySelection>();

    private void Awake()
    {
        foreach (ArmyScriptableObject a in _Armies)
        {
            ArmySelection sel = new ArmySelection();
            sel._Army = a;
            sel._Chosen = false;
            _ArmyChoices.Add(sel);
        }
    }

    public override void OnStartClient()
    {
        _Players.OnAdd += OnItemAdded;
    }

    void OnPlayersChanged(int old, int _new)
    {

        Debug.Log("Made it here");
        foreach (RiskMultiplayerPlayerSetUpScript p in _Players)
        {
            p.transform.parent = _ScrollViewContent;
            p.transform.localScale = Vector3.one;
        }
    }

    void OnItemAdded(int index)
    {
        Debug.Log($"Element added at index {index} {_Players[index]}");
        // _Players2.Add(_Players[index]);

        int diff = _Players.Count - _PlayersLocal.Count;
        for (int i = _PlayersLocal.Count; i < _Players.Count; i++)
        {
            _PlayersLocal.Add(_Players[i]);
        }
        if (diff > 1)
        {
            _PlayersLocal.Remove(_Players[index]);
            _PlayersLocal.Insert(0, _Players[index]);

            _ScrollViewContent.DetachChildren();
            foreach (RiskMultiplayerPlayerSetUpScript p in _PlayersLocal)
            {
                p.transform.parent = _ScrollViewContent;
                p.transform.localScale = Vector3.one;
            }
        }

        foreach (RiskMultiplayerPlayerSetUpScript p in _PlayersLocal)
        {
            p.SetOtherTextInputs();
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
