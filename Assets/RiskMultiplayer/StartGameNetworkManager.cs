using UnityEngine;
using Mirror;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class StartGameNetworkManager : NetworkRoomManager
{
    public static new StartGameNetworkManager singleton => NetworkManager.singleton as StartGameNetworkManager;


    public GameObject _RiskFactionsPlayerPrefab;
    public int val = 0;
    //public RiskMultiplayerSetUpSceneScript _SceneScript;

    public RiskFactionGameSceneScript _RFSceneScript;

    //public List<RiskFactionsPlayerScript> _Replacements = new List<RiskFactionsPlayerScript>();

    

    /*public override void OnServerReady(NetworkConnectionToClient conn)
    {
        base.OnServerReady(conn);
        
        if (SceneManager.GetActiveScene().name == "SampleScene")
        {
            //NetworkConnectionToClient c = conn;

            //GameObject setup = c.identity.gameObject;

            GameObject player = Instantiate(spawnPrefabs[0]);

            //NetworkServer.ReplacePlayerForConnection(conn, player, ReplacePlayerOptions.KeepAuthority);




            foreach (KeyValuePair<int, NetworkConnectionToClient> p in NetworkServer.connections)
            {

                Debug.Log(p.Value.identity.gameObject.name);
                //Destroy(setup, 0.1f);
            }
        }

    }*/

    /*public override void OnServerSceneChanged(string sceneName)
    {
        
        base.OnServerSceneChanged(sceneName);

        if (sceneName == "SampleScene")
        {
            _RFSceneScript = FindObjectOfType<RiskFactionGameSceneScript>();
            _RFSceneScript.Test();
            
        }
    }*/

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
        foreach (KeyValuePair<int, NetworkConnectionToClient> p in NetworkServer.connections)
        {
            p.Value.identity.gameObject.transform.parent = null;
            DontDestroyOnLoad(p.Value.identity.gameObject);
        }


        ServerChangeScene(GameplayScene);
    }


    public override void Update()
    {
        base.Update();

        foreach (KeyValuePair<int, NetworkConnectionToClient> p in NetworkServer.connections)
        {
            //Debug.Log(p.Value.identity.gameObject.name);
        }
    }
}
