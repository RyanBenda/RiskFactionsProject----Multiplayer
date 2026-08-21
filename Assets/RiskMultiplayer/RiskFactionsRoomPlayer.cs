using UnityEngine;
using Mirror;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class RiskFactionsRoomPlayer : NetworkRoomPlayer
{

    public GameObject _RiskFactionsRoomSetupPrefab;
    [SyncVar]
    public RiskMultiplayerPlayerSetUpScript _Setup;

    [SyncVar]
    public string _ArmyName;

    [SyncVar]
    public Color _ArmyColor;

    [SyncVar]
    public ArmyScriptableObject _Army;

    public override void OnStartClient()
    {
        //Debug.Log($"OnStartClient {gameObject}");
    }

    public override void OnClientEnterRoom()
    {
        //Debug.Log($"OnClientEnterRoom {SceneManager.GetActiveScene().path}");    
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        if (SceneManager.GetActiveScene().name == "MultiplayerRiskSetup")
        {
            CreateSetupObject(this);
            CmdSetStartButton();
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdSetStartButton() //CMD for getting the first player in the server and giving them and only them the start button to start the game
    {
        RiskMultiplayerSetUpSceneScript _SceneScript = FindAnyObjectByType<RiskMultiplayerSetUpSceneScript>();

        int pCount = _SceneScript._PlayerCount;

        TRPCSetStartButton(connectionToClient, pCount);
    }

    [TargetRpc]
    private void TRPCSetStartButton(NetworkConnectionToClient target, int value) //Target RPC for getting the first player in the server and giving them and only them the start button to start the game
    {
        if (value == 0)
        {
            FindAnyObjectByType<StartGameButton>(FindObjectsInactive.Include).gameObject.SetActive(true);
        }
    }

    [Command] //Creates the set up object in the Room scene so player can choose the Colour and Name they want
    void CreateSetupObject(RiskFactionsRoomPlayer p)
    {
        RiskMultiplayerSetUpSceneScript sceneScript = FindFirstObjectByType<RiskMultiplayerSetUpSceneScript>();
        GameObject g = Instantiate(_RiskFactionsRoomSetupPrefab);  
        RiskMultiplayerPlayerSetUpScript r = g.GetComponent<RiskMultiplayerPlayerSetUpScript>();
        r._Owner = p;
        NetworkServer.Spawn(g);

        RpcParentUpdate(g.transform, sceneScript);
    }


    [ClientRpc] //Updates the parent of the set up object and calls its localPlayerStart Function as that doesn't run cause it isn't owned by player
    void RpcParentUpdate(Transform obj, RiskMultiplayerSetUpSceneScript sceneScript)
    {
        obj.parent = sceneScript._ScrollViewContent.transform;
        //RiskMultiplayerPlayerSetUpScript p = obj.GetComponent<RiskMultiplayerPlayerSetUpScript>();
        //p.OnStartLocalPlayer();
    }

    public override void OnClientExitRoom()
    {
        //Debug.Log($"OnClientExitRoom {SceneManager.GetActiveScene().path}");
    }

    public override void IndexChanged(int oldIndex, int newIndex)
    {
        //Debug.Log($"IndexChanged {newIndex}");
    }

    public override void ReadyStateChanged(bool oldReadyState, bool newReadyState)
    {
        //Debug.Log($"ReadyStateChanged {newReadyState}");
    }

    public override void OnGUI()
    {
        base.OnGUI();
    }
}
