using UnityEngine;
using Mirror;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class StartGameNetworkManager : NetworkRoomManager
{
    public static new StartGameNetworkManager singleton => NetworkManager.singleton as StartGameNetworkManager;

    public GameObject _RiskFactionsPlayerPrefab;
    public int val = 0;

    public RiskFactionGameSceneScript _RFSceneScript;

    // Sets up the Actual Player Objects when loading to the actual game scene from the room
    public override bool OnRoomServerSceneLoadedForPlayer(NetworkConnectionToClient conn, GameObject roomPlayer, GameObject gamePlayer)
    {
        RiskFactionsPlayerScript playerScore = gamePlayer.GetComponent<RiskFactionsPlayerScript>();
        RiskFactionsRoomPlayer rp = roomPlayer.GetComponent<RiskFactionsRoomPlayer>();
        playerScore.index = rp.index;
        playerScore._PlayerName = rp._ArmyName;
        playerScore._PlayerColour = rp._ArmyColor;
        playerScore._Army = rp._Army;
        return true;
    }

    public override void OnRoomStopClient()
    {
        base.OnRoomStopClient();
    }

    public override void OnRoomStopServer()
    {
        base.OnRoomStopServer();
    }

    public void StartGame()
    {
        ServerChangeScene(GameplayScene);
    }
}
