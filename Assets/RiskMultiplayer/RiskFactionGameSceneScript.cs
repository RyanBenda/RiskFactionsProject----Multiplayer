using UnityEngine;
using System.Collections.Generic;
using Mirror;

public class RiskFactionGameSceneScript : NetworkBehaviour
{
    public readonly SyncList<RiskFactionsPlayerScript> _Players = new SyncList<RiskFactionsPlayerScript>();

    [SyncVar(hook = nameof(OnPlayersChanged))]
    public int _PlayerCount = 0;

    public override void OnStartClient()
    {
        
    }

    void OnPlayersChanged(int old, int _new)
    {

    }

    void OnItemAdded(int index)
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
