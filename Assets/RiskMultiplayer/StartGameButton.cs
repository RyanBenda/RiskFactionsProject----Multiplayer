using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using System.Linq;

public class StartGameButton : NetworkBehaviour
{
    public StartGameNetworkManager _NM;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        this.gameObject.SetActive(false);
    }

    [Command(requiresAuthority = false)]
    public void StartGame()
    {
        _NM.StartGame();
    }
}
