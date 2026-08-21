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

    public NetworkManagerHUD _NMHUD;

    public override void Awake()
    {
        base.Awake();
        _NMHUD = GetComponent<NetworkManagerHUD>();
    }

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

        _RFSceneScript.HideNetworkHud();

        if (_RFSceneScript._PlayerCount == NetworkServer.connections.Count)
        {
            ArmyScriptableObject[] armies = new ArmyScriptableObject[_RFSceneScript._PlayerCount];
            for (int i = 0; i < _RFSceneScript._PlayerCount; i++)
                armies[i] = _RFSceneScript._Players[i]._Army;

            _RFSceneScript._ObjectiveManager.SetUpObjectives();
            _RFSceneScript._Board.BeginMatch(armies);
        }

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
