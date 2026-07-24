using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class PlayerScript : NetworkBehaviour
{
    public TextMeshPro _PlayerNameText;
    public GameObject _FloatingInfo;

    private Material _PlayerMaterialClone;

    [SyncVar(hook = nameof(OnNameChanged))]
    public string _PlayerName;

    [SyncVar(hook = nameof(OnColourChanged))]
    public Color _PlayerColour = Color.white;

    private SceneScript _SceneScript;

    void Awake()
    {
        _SceneScript = FindObjectOfType<SceneScript>();
    }

    void OnNameChanged(string old, string _new)
    {
        _PlayerNameText.text = _PlayerName;
        this.name = _PlayerName;
    }

    void OnColourChanged(Color old, Color _new)
    {
        _PlayerNameText.color = _new;
        _PlayerMaterialClone = new Material(GetComponent<Renderer>().material);
        _PlayerMaterialClone.color = _new;
        GetComponent<Renderer>().material = _PlayerMaterialClone;
    }
    public override void OnStartLocalPlayer()
    {

        _SceneScript._PlayerScript = this;

        Camera.main.transform.SetParent(transform);
        Camera.main.transform.localPosition = Vector3.zero;

        _FloatingInfo.transform.localPosition = new Vector3(0, -3.3f, 0.6f);
        _FloatingInfo.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

        string name = "Player" + Random.Range(100, 999);
        Color colour = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        CmdSetupPlayer(name, colour);
    }

    [Command]
    public void CmdSetupPlayer(string name, Color col)
    {
        // player info sent to server, then server updates sync vars which handles it on all clients
        _PlayerName = name;
        _PlayerColour = col;
    }

    void Update()
    {
        if (!isLocalPlayer) 
        {
            _FloatingInfo.transform.LookAt(Camera.main.transform);
            return; 
        }

        float moveX = Input.GetAxis("Horizontal") * Time.deltaTime * 110.0f;
        float moveZ = Input.GetAxis("Vertical") * Time.deltaTime * 4f;

        transform.Rotate(0, moveX, 0);
        transform.Translate(0, 0, moveZ);
    }
}
