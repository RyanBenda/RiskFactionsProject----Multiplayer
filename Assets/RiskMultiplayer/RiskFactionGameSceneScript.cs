using UnityEngine;
using System.Collections.Generic;
using Mirror;

public class RiskFactionGameSceneScript : NetworkBehaviour
{
    public readonly SyncList<RiskFactionsPlayerScript> _Players = new SyncList<RiskFactionsPlayerScript>();

    [SyncVar(hook = nameof(OnPlayersChanged))]
    public int _PlayerCount = 0;

    public BoardComponent _Board;

    public override void OnStartClient()
    {
        
    }

    void OnPlayersChanged(int old, int _new)
    {
        if (_PlayerCount == NetworkServer.connections.Count)
        {
            Debug.Log(_PlayerCount);
            Debug.Log("All Ready");

            if (isServer)
            {
                ArmyScriptableObject[] armies = new ArmyScriptableObject[_PlayerCount];
                for (int i = 0; i < _PlayerCount; i++)
                    armies[i] = _Players[i]._Army;

                _Board.BeginMatch(armies);
            }

            //_Board.SetUpCountries();
        }    
    }

    void OnItemAdded(int index)
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
