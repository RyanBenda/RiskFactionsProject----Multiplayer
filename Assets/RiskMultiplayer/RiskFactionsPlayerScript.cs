using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class RiskFactionsPlayerScript : NetworkBehaviour
{
    [SyncVar]
    public int index = -1;

    public RiskFactionGameSceneScript _SceneScript;

    [SyncVar(hook = nameof(OnNameChanged))]
    public string _PlayerName;

    [SyncVar]
    public Color _PlayerColour;

    [SyncVar]
    public ArmyScriptableObject _Army;

    void OnNameChanged(string old, string _new)
    {
        this.name = _PlayerName;
    }

    [Command]
    public void CmdChangePlayerName(string text)
    {
        _PlayerName = text;
    }

    [Command]
    public void CmdChangePlayerColour(Color colour)
    {
        _PlayerColour = colour;
    }
}
