using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class RiskMultiplayerAISetupScript : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnNameChanged))]
    public string _AIName;

    [SyncVar(hook = nameof(OnColourChanged))]
    public Color _AIColour;

    //[SyncVar(hook = nameof(OnArmyChanged))]
    public ArmyScriptableObject _SetArmy;

    public RiskMultiplayerSetUpSceneScript _SceneScript;

    public TMP_InputField _NameInputField;
    public Button _ColourButton;

    public Transform _ColourSelectorPos;
    public RiskArmyColourSelector _ColourSelector;

    [SyncVar]
    public int _ColourIndex;

    void OnNameChanged(string old, string _new)
    {
        this.name = _AIName;
        _NameInputField.text = _AIName;
    }

    void OnColourChanged(Color old, Color _new)
    {
        _ColourButton.image.color = _AIColour;
        SetOtherColourButtons(); // Updates the colour buttons to be correct for non host clients
    }

    public void SetOtherColourButtons()
    {
        if (_ColourSelector != null)
            _ColourSelector.OnEnable();
    }

    public void SetColourSelectorActive()
    {
        if (!_ColourSelector.gameObject.activeSelf)
        {
            //_ColourSelector._Requester = this;
            _ColourSelector.transform.position = _ColourSelectorPos.position;
            _ColourSelector.gameObject.SetActive(true);
        }
        else
            _ColourSelector.gameObject.SetActive(false);
    }
}
