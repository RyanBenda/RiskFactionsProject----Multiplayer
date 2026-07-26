using UnityEngine;
using System.Collections.Generic;
using Mirror;

public class RiskFactionGameSceneScript : NetworkBehaviour
{
    public readonly SyncList<RiskFactionsPlayerScript> _Players = new SyncList<RiskFactionsPlayerScript>();

    [SyncVar(hook = nameof(OnPlayersChanged))]
    public int _PlayerCount = 0;

    //public readonly SyncList<RiskMultiplayerPlayerSetUpScript> _ToBeDeleted = new SyncList<RiskMultiplayerPlayerSetUpScript>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
   

    public override void OnStartClient()
    {
        
    }

    void OnPlayersChanged(int old, int _new)
    {

       

        
    }

    void OnItemAdded(int index)
    {
        /*if (_Players[index].connectionToClient.authenticationData is CustomConnectionData clientData)
        {
            _Players[index]._PlayerName = clientData.name;
            _Players[index]._PlayerColour = clientData.colour;
        }*/
    }

    [ClientRpc]
    public void Test()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isServer)
        {
            foreach (KeyValuePair<int, NetworkConnectionToClient> p in NetworkServer.connections)
            {
                //Debug.Log(p.Value.identity.gameObject.name);
            }
        }
    }
}
