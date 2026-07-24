using UnityEngine;
using Mirror;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneScript : NetworkBehaviour
{
    public PlayerScript _PlayerScript;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ButtonChangeScene()
    {
        if (isServer)
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.name == "MirrorTest")
                NetworkManager.singleton.ServerChangeScene("MirrorTest Other");
            else
                NetworkManager.singleton.ServerChangeScene("MirrorTest");
        }
        else
            Debug.Log("You are not Host");
    }
}
