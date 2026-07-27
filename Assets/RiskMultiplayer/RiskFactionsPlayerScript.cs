using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class RiskFactionsPlayerScript : NetworkBehaviour
{
    [SyncVar]
    public int index = -1;

    public RiskFactionGameSceneScript _SceneScript;
    GameCanvasComponent _GameCanvas;

    [SyncVar(hook = nameof(OnNameChanged))]
    public string _PlayerName;

    [SyncVar]
    public Color _PlayerColour;

    [SyncVar]
    public ArmyScriptableObject _Army;

    [SyncVar]
    public bool _IsTurn;

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

    [Command]
    public void CmdSetTurn(bool isTurn)
    {
        _IsTurn = isTurn;
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        _GameCanvas = FindFirstObjectByType<GameCanvasComponent>();

        //GameCanvasComponent._GameInstance._LocalPlayer = this;

        _GameCanvas._LocalPlayer = this;
    }
}
