using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
public class ArmyColourButton : NetworkBehaviour
{
    public Button _Button;
    public GameObject _Cross;
    public ArmySelection _Army;
    public Color _Colour;
    public int _ColourIndex;

    public RiskArmyColourSelector _Selector;

    // Starts the process of setting the new Name, Colour and Army when clicking the Button
    public void OnClick()
    {
        ArmySelection temp = new ArmySelection();
        temp._Army = _Selector._SceneScript._ArmyChoices[_Selector._Requester._ColourIndex]._Army;
        temp._Chosen = false;
        _Selector._SceneScript._ArmyChoices.OnSet(_Selector._Requester._ColourIndex, temp);

        if (_Selector._Requester._PlayerName == temp._Army._ArmyName)
            _Selector._Requester.CmdChangePlayerName(_Army._Army._ArmyName);
        _Selector._Requester.CmdChangePlayerColour(_Colour);
        _Selector._Requester._ColourIndex = _ColourIndex;
        _Selector._Requester.CmdChangePlayerArmy(_Army._Army);

        ArmySelection temp2 = new ArmySelection();
        temp2._Army = _Army._Army;
        temp2._Chosen = true;
        _Selector._SceneScript._ArmyChoices.OnSet(_ColourIndex, temp2);

        _Cross.gameObject.SetActive(true);
        _Button.interactable = false;

        _Selector.gameObject.SetActive(false);
    }
}
