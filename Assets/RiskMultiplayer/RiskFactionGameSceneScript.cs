using UnityEngine;
using System.Collections.Generic;
using Mirror;

public class RiskFactionGameSceneScript : NetworkBehaviour
{
    public readonly SyncList<RiskFactionsPlayerScript> _Players = new SyncList<RiskFactionsPlayerScript>();

    [SyncVar(hook = nameof(OnPlayersChanged))]
    public int _PlayerCount = 0;

    public BoardComponent _Board;
    public BattleSystem _BattleSystem;
    public ObjectiveManager _ObjectiveManager;

    public override void OnStartClient()
    {
        
    }

    //Starts the game when all the players have loaded in and added themselves to the Players SyncList
    void OnPlayersChanged(int old, int _new)
    {
        if (_PlayerCount == NetworkServer.connections.Count)
        {
            if (isServer)
            {
                /*ArmyScriptableObject[] armies = new ArmyScriptableObject[_PlayerCount];
                for (int i = 0; i < _PlayerCount; i++)
                    armies[i] = _Players[i]._Army;

                _ObjectiveManager.SetUpObjectives();
                _Board.BeginMatch(armies);*/
            }
        }    
    }

    [ClientRpc]
    public void HideNetworkHud()
    {
        StartGameNetworkManager.singleton._NMHUD.enabled = false;
    }
}
