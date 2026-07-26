using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class StartGameButton : NetworkBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!isServer)
        {
            this.gameObject.SetActive(false);
        }
    }
}
