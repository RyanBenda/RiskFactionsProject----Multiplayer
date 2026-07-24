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
    public void OnClick()
    {
        //_Selector._Buttons[_Selector._Requester._ColourIndex]._Cross.gameObject.SetActive(false);
        //_Selector._Buttons[_Selector._Requester._ColourIndex]._Button.interactable = true;

        ArmySelection temp = new ArmySelection();
        temp._Army = _Selector._SceneScript._ArmyChoices[_Selector._Requester._ColourIndex]._Army;
        temp._Chosen = false;
        _Selector._SceneScript._ArmyChoices.OnSet(_Selector._Requester._ColourIndex, temp);
        //_Selector._SceneScript._ArmyChoices.RemoveAt(_Selector._Requester._ColourIndex);
        //_Selector._SceneScript._ArmyChoices.Insert(_Selector._Requester._ColourIndex, temp);

        //_Selector._Buttons[_Selector._Requester._ColourIndex]._Army._Chosen = false;
        _Selector._Requester.CmdChangePlayerColour(_Colour);
        _Selector._Requester._ColourIndex = _ColourIndex;

        ArmySelection temp2 = new ArmySelection();
        temp._Army = _Army._Army;
        temp._Chosen = true;
        _Selector._SceneScript._ArmyChoices.OnSet(_ColourIndex, temp);
        //_Selector._SceneScript._ArmyChoices.RemoveAt(_ColourIndex);
        //_Selector._SceneScript._ArmyChoices.Insert(_ColourIndex, temp2);

        _Cross.gameObject.SetActive(true);
        _Button.interactable = false;

        _Selector.gameObject.SetActive(false);
    }
}
