using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;




public class StartGameButton : NetworkBehaviour
{
    public StartGameNetworkManager _NetworkManager;
    public RiskMultiplayerSetUpSceneScript _SceneScript;
    public GameObject _RiskFactionsPlayerPrefab;

    bool test = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!isServer)
        {
            this.gameObject.SetActive(false);
        }
    }

    //[Command(requiresAuthority = false)]
    //[ClientRpc]
    /*public void LoadScene()
    {
        if (!test)
        {
            test = true;

            foreach (RiskMultiplayerPlayerSetUpScript p in _SceneScript._Players)
            {
                

                //p.RpcCreateReplacement(_RiskFactionsPlayerPrefab);
                //p.transform.parent = null;
                //DontDestroyOnLoad(p.gameObject);
                /*for (int i = 0; i < p.transform.childCount; i++)
                {
                    Destroy(p.transform.GetChild(i).gameObject);
                }
                p._RiskFactionsPlayerScript.enabled = true;
                p.enabled = false;
            }

            _NetworkManager.ServerChangeScene("SampleScene");
        }
        else
        {
            foreach (RiskMultiplayerPlayerSetUpScript p in _SceneScript._Players)
            {
                Debug.Log(p.connectionToClient.identity.gameObject.name);

            }
        }
    }*/

    
}
