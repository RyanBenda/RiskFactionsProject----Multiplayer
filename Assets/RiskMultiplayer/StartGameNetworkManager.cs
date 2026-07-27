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
        RiskFactionsPlayerScript riskPlayer = gamePlayer.GetComponent<RiskFactionsPlayerScript>();
        RiskFactionsRoomPlayer rp = roomPlayer.GetComponent<RiskFactionsRoomPlayer>();
        riskPlayer.index = rp.index;
        riskPlayer._PlayerName = rp._ArmyName;
        riskPlayer._PlayerColour = rp._ArmyColor;
        riskPlayer._Army = rp._Army;
        _RFSceneScript = FindFirstObjectByType<RiskFactionGameSceneScript>();
        riskPlayer._SceneScript = _RFSceneScript;
        NetworkServer.ReplacePlayerForConnection(conn, gamePlayer, ReplacePlayerOptions.KeepAuthority);
        _RFSceneScript._Players.Add(riskPlayer);
        _RFSceneScript._PlayerCount++;
        //_RFSceneScript._Board._GameCanvas._LocalPlayer = riskPlayer;

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
